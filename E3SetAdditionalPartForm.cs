using e3;
using E3WGM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace E3SetAdditionalPart
{
    public partial class E3SetAdditionalPartForm : Form
    {
        private List<AdditionalPart> additionalParts = new List<AdditionalPart>();
        private List<AdditionalPart> replacementParts = new List<AdditionalPart>(); //TODO переименовать класс AdditionalPart - он общий
        private List<string> errorMessages = new List<string>();
        public static WindchillHTTPClient wchHTTPClient;

        private e3Application app = null;
        private e3Job job = null;
        private e3Tree tree = null;
        private e3Device dev = null;
        private int shtampDevId = 0; // запоминаем, чтобы при повторном подключении к Е3 восстановить устройство для которого у нас накоплены данные
        private e3Attribute attribute;


        // Start Делем недоступным закрытие формы по "крестику"
        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CP_NOCLOSE_BUTTON;
                return cp;
            }
        }
        // End Делем недоступным закрытие формы по "крестику"


        public E3SetAdditionalPartForm()
        {
            try
            {
                InitialiseE3Objects();

                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string configFile = Path.Combine(appDir, "windchillserver.json");

                Dictionary<string, string> allWCHServer = new Dictionary<string, string>();
                if (File.Exists( configFile))
                {
                    String jsonWCHServer = "";
                    using (StreamReader streamReader = new StreamReader( configFile))
                    {
                        jsonWCHServer = streamReader.ReadToEnd();
                    }

                    using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonWCHServer)))
                    {
                        DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings();
                        settings.UseSimpleDictionaryFormat = true;
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Dictionary<string, string>), settings);
                        allWCHServer = ser.ReadObject(stream) as Dictionary<string, string>;
                    }
                }

                string serverProtocol = "";
                string serverName = "";
                string ignoreSSLPolicyErrors = "";
                allWCHServer.TryGetValue("serverProtocol", out serverProtocol);
                allWCHServer.TryGetValue("serverName", out serverName);
                allWCHServer.TryGetValue("ignoreSSLPolicyErrors", out ignoreSSLPolicyErrors);
                wchHTTPClient = new WindchillHTTPClient(serverProtocol, serverName, Boolean.Parse(ignoreSSLPolicyErrors));

                WindchillLoginForm wchLogin = new WindchillLoginForm(wchHTTPClient);
                wchLogin.ShowDialog();
                if (wchLogin.DialogResult.Equals(DialogResult.Cancel))
                {
                    DisconnectFromE3Series();

                    MessageBox.Show("Не выполнен вход в Windchill" +
                           "\n\nВыход из программы");
                    System.Environment.Exit(0);
                }


                InitializeComponent();

                tbNameDevice.Text = dev.GetName();
                shtampDevId = dev.GetId();

                // Скрываем колонку "Кол-во" для таблицы замен
                replacementPartsUserControl.ShowAmountColumn = false;

                // Скрываем колонку "Примечание" для таблицы доп. частей
                additionalPartsUserControl.ShowPrimColumn = false;


                //TODO ?  Общий для вкладок PartsUserControl помечаем своим типом для различия в кодах обработки данных
                //additionalPartsUserControl.ControlType = PartControlType.Additional;
                //replacementPartsUserControl.ControlType = PartControlType.Replacement;

                // Подписываемся на события кнопок в additionalPartsUserControl и replacementPartsUserControl (это 2 объекта одного класса PartsUserControl)
                additionalPartsUserControl.SearchButtonClicked += AdditionalPartsUserControl_SearchButtonClicked;
                additionalPartsUserControl.DeleteButtonClicked += AdditionalPartsUserControl_DeleteButtonClicked;

                replacementPartsUserControl.SearchButtonClicked += ReplacementPartsUserControl_SearchButtonClicked;
                replacementPartsUserControl.DeleteButtonClicked += ReplacementPartsUserControl_DeleteButtonClicked;



                // Загружаем из многозначных атрибутов изделия Е3 его Дополнительные части и Замены и синхронизируем каждую из них с Windchill
                readAdditionalParts(); // наполнили список additionalParts
                readReplacementParts(); // наполнили список replacementParts

                // отображаем данные в этой утилите
                additionalPartsUserControl.AddPartsToGrid( additionalParts);
                replacementPartsUserControl.AddPartsToGrid( replacementParts);

                // Показываем все ошибки одним MessageBox
                if (errorMessages.Count > 0)
                {
                    string allErrors = string.Join("\n\n", errorMessages);
                    MessageBox.Show(allErrors, "Ошибки при чтении атрибутов и синхронизации с Windchill",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                DisconnectFromE3Series(); // имеем сохраненный devId, чтобы при повторном подкл. к Е3 работать с тем же Изделием !
                
            }
            catch (Exception ex)
            {
                DisconnectFromE3Series();

                MessageBox.Show(ex.Message +
                       "\n\nВыход из программы");
                System.Environment.Exit(0);
            }
        }

        private void readAdditionalParts()
        {
            String numberAdditionalPart;
            double amountAddPart = 1.0;

            dynamic attributeIds = null;
            int nAllValues = dev.GetAttributeIds(ref attributeIds, "AdditionalPart");
            for (int i = 1; i <= nAllValues; i++) // считываем по очереди все значения многозначного атрибута "AdditionalPart"
            {
                attribute.SetId(attributeIds[i]);
                String valueAttr = attribute.GetValue(); // обозначение Доп.СЧ или обозначение Доп.СЧ;количество 

                if (string.IsNullOrEmpty(valueAttr))
                    continue;

                try
                {
                    ParseValueAttr(valueAttr, out numberAdditionalPart, out amountAddPart);
                }
                catch (Exception ex)
                {
                    if (!errorMessages.Contains(dev.GetName() + " дополнительная часть " + ex.Message))
                        errorMessages.Add(dev.GetName() + " дополнительная часть " + ex.Message);
                    continue;
                }


                AdditionalPart tempPart = new AdditionalPart();
                tempPart.number = numberAdditionalPart;

                try
                {
                    AdditionalPart additional_Part = syncDataWithWindchill(tempPart);
                    additional_Part.amount = amountAddPart;
                    //additionalPart.merge(additional_Part);
                    //additionalParts.Add( tempPart);
                    additionalParts.Add( additional_Part);
                }
                catch (Exception e)
                {
                    if (!errorMessages.Contains($"Дополнительная часть {numberAdditionalPart} не найдена в Windchill"))
                    {
                        errorMessages.Add($"Дополнительная часть {numberAdditionalPart} не найдена в Windchill");
                    }
                }
            }
        }

        private void readReplacementParts()
        {
            String numberReplacementPart;
            dynamic attributeIds = null;
            int nAllValues = dev.GetAttributeIds(ref attributeIds, "AdditionalReplacement");
            for (int i = 1; i <= nAllValues; i++) // считываем по очереди все значения многозначного атрибута "ReplacementPart"
            {
                attribute.SetId(attributeIds[i]);
                numberReplacementPart = attribute.GetValue();

                if (String.IsNullOrEmpty(numberReplacementPart))
                    continue;

                AdditionalPart tempPart = new AdditionalPart(); // класс AdditionalPart - он общий
                tempPart.number = numberReplacementPart;

                try { 
                    AdditionalPart replacment_Part = syncDataWithWindchill( tempPart);
                    replacementParts.Add( replacment_Part);
                }
                catch (Exception e)
                {
                    if (!errorMessages.Contains($"Подстановка {numberReplacementPart} не найдена в Windchill"))
                    {
                        errorMessages.Add($"Подстановка {numberReplacementPart} не найдена в Windchill");
                    }
                }

            }
        }


        private void AdditionalPartsUserControl_SearchButtonClicked(object sender, EventArgs e)
        {
            SearchForm searchForm = new SearchForm();

            if (searchForm.ShowDialog() == DialogResult.OK)
            {
                foreach (AdditionalPart part in searchForm.resultSelectedData)
                {
                    if (!additionalParts.Exists(x => x.oidMaster == part.oidMaster))
                    {
                        part.amount = 1.0; // Устанавливаем количество по умолчанию
                        additionalParts.Add(part);
                    }
                }

                // Обновляем данные в additionalPartsUserControl
                additionalPartsUserControl.AddPartsToGrid(additionalParts);
            }
        }

        private void AdditionalPartsUserControl_DeleteButtonClicked(object sender, EventArgs e)
        {
            var partsUserControl = sender as PartsUserControl;
            if (partsUserControl == null) return;

            var selectedParts = partsUserControl.GetSelectedParts();
            if (selectedParts.Count > 0)
            {
                foreach (var part in selectedParts)
                {
                    additionalParts.Remove(part);
                }

                // Обновляем данные в additionalPartsUserControl
                partsUserControl.AddPartsToGrid(additionalParts);
            }
            else
            {
                MessageBox.Show("Выделите строку.");
            }
        }


        private void ReplacementPartsUserControl_SearchButtonClicked(object sender, EventArgs e)
        {
            SearchForm searchForm = new SearchForm();

            if (searchForm.ShowDialog() == DialogResult.OK)
            {
                foreach (AdditionalPart part in searchForm.resultSelectedData)
                {
                    if (!replacementParts.Exists(x => x.oidMaster == part.oidMaster))
                    {
                        part.amount = 1.0; // для единообразия
                        replacementParts.Add(part);
                    }
                }

                // Обновляем данные в replacementPartsUserControl
                replacementPartsUserControl.AddPartsToGrid( replacementParts);
            }
        }

        private void ReplacementPartsUserControl_DeleteButtonClicked(object sender, EventArgs e)
        {
            var partsUserControl = sender as PartsUserControl;
            if (partsUserControl == null) return;

            var selectedParts = partsUserControl.GetSelectedParts();
            if (selectedParts.Count > 0)
            {
                foreach (var part in selectedParts)
                {
                    replacementParts.Remove(part);
                }

                // Обновляем данные в replacementPartsUserControl
                partsUserControl.AddPartsToGrid(replacementParts);
            }
            else
            {
                MessageBox.Show("Выделите строку.");
            }
        }


        /// <summary>
        /// Сохраняем изменения и выходим из утилиты
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOk_Click(object sender, EventArgs e)
        {
            InitialiseE3Objects();
            dev.SetId( shtampDevId); // восстановили изделие. За время работы в утилите могли выбрать другое !

            saveData(dev);

            DisconnectFromE3Series();
            System.Environment.Exit(0);
        }


        private void buttonSynchr_Click(object sender, EventArgs e)
        {
            InitialiseE3Objects(); // dev - тут уже получили новое выделенное устройство
            int newDevId = dev.GetId();


            // сначала нужно сохранить возможные внесенные изменения в устройство которое редактировалось до нажатия кнопки Синхронизации
            dev.SetId( shtampDevId); // восстановили устройство
            saveData( dev);


            // переключаемся на новое устройство для работы дальше в утилите уже с ним
            dev.SetId(newDevId);
            tbNameDevice.Text = dev.GetName();
            shtampDevId = dev.GetId();

            // Загружаем из многозначных атрибутов изделия Е3 его Дополнительные части и Замены и синхронизируем каждую из них с Windchill
            additionalParts = new List<AdditionalPart>();
            replacementParts = new List<AdditionalPart>();

            readAdditionalParts(); // наполнили список additionalParts
            readReplacementParts(); // наполнили список replacementParts

            // отображаем данные в этой утилите
            additionalPartsUserControl.AddPartsToGrid(additionalParts);
            replacementPartsUserControl.AddPartsToGrid(replacementParts);


            // Показываем все ошибки одним MessageBox
            if (errorMessages.Count > 0)
            {
                string allErrors = string.Join("\n\n", errorMessages);
                MessageBox.Show(allErrors, "Ошибки при чтении атрибутов и синхронизации с Windchill",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            DisconnectFromE3Series(); // имеем сохраненный devId, чтобы при повторном подкл. к Е3 работать с тем же Изделием !

        }

        private void saveData(e3Device dev)
        {
            // Синхронизируем количество из грида дополнительных частей
            //additionalPartsUserControl.SyncAmountFromGrid();

            // Проверяем корректность количества в дополнительных частях
            if (!additionalPartsUserControl.TrySyncAmountFromGrid(out List<string> errors))
            {
                string message = "Обнаружены ошибки в значениях количества:\n\n" + string.Join("\n", errors);
                MessageBox.Show(message, "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Оставляем форму открытой для исправления
            }


            dev.DeleteAttribute("AdditionalPart");
            updateAttributeData(additionalPartsUserControl.Parts, "AdditionalPart");

            dev.DeleteAttribute("AdditionalReplacement");
            updateAttributeData(replacementPartsUserControl.Parts, "AdditionalReplacement");
        }


        /* private void updateAttributeData(List<AdditionalPart> parts, string attributeName)
         {
             foreach (AdditionalPart part in parts)
             {
                 if( dev.HasAttribute(attributeName) == 0) // у dev еще нет атрибута 
                 {
                     dev.SetAttributeValue(attributeName, part.number);
                 }
                 else
                 {
                     dev.AddAttibuteValue(attributeName, part.number);
                 }
             }
         } */

        private void updateAttributeData(List<AdditionalPart> parts, string attributeName)
        {
            foreach (AdditionalPart part in parts)
            {
                string valueToWrite;

                if (attributeName == "AdditionalPart")
                {
                    // Для дополнительных частей учитываем количество
                    if (part.amount == 1.0)
                        valueToWrite = part.number;
                    else
                        valueToWrite = part.number + ";" + part.amount.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
                else // AdditionalReplacement
                {
                    // Для замен количество не используется
                    valueToWrite = part.number;
                }



                if (dev.HasAttribute(attributeName) == 0)
                {
                    dev.SetAttributeValue(attributeName, valueToWrite);
                }
                else
                {
                    dev.AddAttibuteValue(attributeName, valueToWrite);
                }
            }
        }


        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DisconnectFromE3Series();
            System.Environment.Exit(0);
        }



        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        private AdditionalPart syncDataWithWindchill(AdditionalPart additionalPart)
        {
            AdditionalPart tempPart;

            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings();
            settings.UseSimpleDictionaryFormat = true;
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(AdditionalPart), settings);
            ser.WriteObject(stream, additionalPart);
            stream.Position = 0;
            StreamReader sr = new StreamReader(stream);

            string data = sr.ReadToEnd();
            data = data.Replace("{", "");
            //data = data.Replace("}", "");
            string jsonAdditionalPart = "{\"__type\":\"AdditionalPart:#E3SetAdditionalPart\"," + data;
            string received = wchHTTPClient.getJSON(jsonAdditionalPart, "netmarkets/jsp/by/iba/e3/http/syncAdditionalPart.jsp");

            MemoryStream stream2 = new MemoryStream(Encoding.UTF8.GetBytes(received));
            DataContractJsonSerializer ser2 = new DataContractJsonSerializer(typeof(AdditionalPart), settings);
            tempPart = (AdditionalPart)ser2.ReadObject(stream2);

            if (String.IsNullOrEmpty(tempPart.oidMaster))
            {
                throw new Exception();
            }

            return tempPart;
        }


        /// <summary>
        /// <para>В атрибуте "Дополнительная честь" может содержаться значение по шаблону "number;количество"</para>
        /// Возвращает: Number доп.части, количество этой доп.части на единицу изделия где задана эта доп.часть.
        /// </summary>
        /// <param name="valueAttr"></param>
        /// <param name="numberAddPart"></param>
        /// <param name="amount"></param>
        /// <exception cref="FormatException"></exception>
        private void ParseValueAttr(string valueAttr, out string numberAddPart, out double amount)
        {
            // Проверка количества разделителей
            int separatorIndex = valueAttr.IndexOf(';');
            int lastSeparatorIndex = valueAttr.LastIndexOf(';');

            if (separatorIndex != lastSeparatorIndex)
                throw new FormatException("Допускается только один разделитель ';'");

            // Нет разделителя
            if (separatorIndex == -1)
            {
                numberAddPart = valueAttr;
                amount = 1.0;
                return;
            }

            // Проверка текста перед разделителем
            string text = valueAttr.Substring(0, separatorIndex);
            if (string.IsNullOrWhiteSpace(text))
                throw new FormatException("Текст перед ';' обязателен");

            // Нет числа после разделителя
            if (separatorIndex == valueAttr.Length - 1)
            {
                numberAddPart = text;
                amount = 1.0;
                return;
            }

            string numberStr = valueAttr.Substring(separatorIndex + 1);
            if (string.IsNullOrWhiteSpace(numberStr))
            {
                numberAddPart = text;
                amount = 1.0;
                return;
            }

            // Парсинг числа
            if (!double.TryParse(numberStr.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double number))
            {
                throw new FormatException($"Неверный числовой формат: '{numberStr}'");
            }

            numberAddPart = text;
            amount = number;
            return;
        }


        private void InitialiseE3Objects()
        {
            app = ConnectToE3();
            job = CreateJobObject();
            dev = CheckAndGetSelectedDevice();
            attribute = job.CreateAttributeObject();
        }


        private e3Application ConnectToE3()
        {
            // получаем количество процессов E3.series
            int processCount = System.Diagnostics.Process.GetProcessesByName("E3.series").Length;

            switch (processCount)
            {
                case 0:
                    MessageBox.Show("Нет запущенных окон E3.Series" + "\n\nВыход из программы");
                    System.Environment.Exit(0);
                    return null; // Не выполнится, но нужен для компиляции
                case 1:
                    try
                    {
                        return (e3Application)System.Activator.CreateInstance
                            (System.Type.GetTypeFromProgID("CT.Application"));
                    }
                    catch (System.Exception)
                    {
                        MessageBox.Show("Не установлен E3.Series" +
                            "\n\nВыход из программы");
                        System.Environment.Exit(0);
                        return null; // Не выполнится, но нужен для компиляции
                    }
                default:  // Несколько процессов E3
                    try
                    {
                        dynamic dispatcher = Activator.CreateInstance(
                            Type.GetTypeFromProgID("CT.DispatcherViewer"));

                        dynamic selectedApp = null;
                        dispatcher.ShowViewer(ref selectedApp);

                        if (selectedApp == null)
                        {
                            MessageBox.Show("Не выбрано окно E3.Series\n\nВыход из программы");
                            Environment.Exit(0);
                            return null;
                        }

                        return (e3Application)selectedApp;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(
                            "Открыто несколько окон E3.Series\n" +
                            "Закройте 'лишние' окна или установите E3.Dispatcher\n\n" +
                            "Выход из программы");
                        Environment.Exit(0);
                        return null;
                    }
            }

            return null;
        }

        private e3Job CreateJobObject()
        {
            // возвращаемое функцией значение
            e3Job ret = null;

            try
            {
                ret = (e3Job)app.CreateJobObject();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Невозможно создать объект Job в E3.Series" +
                    "\nСообщение: " + ex.Message +
                    "\n\nВыход из программы");
                System.Environment.Exit(0);
            }

            // нет открытых проектов - выход
            if (ret.GetId() == 0)
            {
                MessageBox.Show("Нет открытых проектов" +
                    "\n\nВыход из программы");
                System.Environment.Exit(0);
            }

            return ret;
        }

        private e3Device CheckAndGetSelectedDevice()
        {
            tree = job.CreateTreeObject();
            tree.SetId(job.GetActiveTreeID());
            dev = job.CreateDeviceObject();

            dynamic sSelDevIds = null;
            int nSelDev = tree.GetSelectedAllDeviceIds(ref sSelDevIds);

            if (nSelDev == 0)
            {
                MessageBox.Show("Нет выбранных объектов." +
                            "\n\nВыход из программы");

                DisconnectFromE3Series();
                System.Environment.Exit(0);
            }

            if (nSelDev > 1)
            {
                MessageBox.Show("Выбрано более одного объекта." +
                            "\n\nВыход из программы");

                DisconnectFromE3Series();
                System.Environment.Exit(0);
            }

            int deviceId = dev.SetId(sSelDevIds[1]);
            // если выделили не изделие, а его изображение
            int originalId = dev.GetOriginalId();
            if (originalId > 0)
            {
                // Перестановка c изображения изделия на оригинал
                deviceId = dev.SetId(originalId);
            }

            return dev;
        }

        // Отключение от E3.Series и освобождение всех полей, связанных
        // с COM-объектами.
        //
        public void DisconnectFromE3Series()
        {
            if ( app == null)
                return;
            
            // разрываем соединение с E3.Series.
            Marshal.ReleaseComObject( app);
            Marshal.ReleaseComObject( job);

            // Обнуляем поля, ссылавшиеся на COM-объекты
            app = null;
            job = null;
            tree = null;
            dev = null;
            attribute = null;

            // принудительно удаляем пустые (null) ссылки
            GC.Collect();
        }

    }
}
