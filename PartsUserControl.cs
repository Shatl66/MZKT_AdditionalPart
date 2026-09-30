using E3WGM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace E3SetAdditionalPart
{
    //public enum PartControlType { Additional, Replacement }

    public partial class PartsUserControl : UserControl
    {
        //public PartControlType ControlType { get; set; } // свойство для различения типа вкладки


        // Свойство для доступа к DataGridView        
        /* Может пригодится когда !
        public DataGridView PartsDataGridView => dataGridViewAdditionalParts;

        // Где-то в коде можно получить доступ к DataGridView
        DataGridView grid = partsUserControl.PartsDataGridView;

        // Можно работать с полученным объектом
        int rowCount = grid.Rows.Count;
        grid.ClearSelection();
        */

        // ------------ Свойство для хранения списка частей ------------
        //public List<AdditionalPart> Parts { get; set; } = new List<AdditionalPart>();
        //Без всей каши с _parts нельзя ! Ломается что то в VisualStudio
        //Пишет: Ошибка при генерации кода для свойства "Parts". Произошла следующая ошибка: 'Тип E3SetAdditionalPart.AdditionalPart' в сборке "E3SetAdditionalPart, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" не помечен как сериализуемый.
        private List<AdditionalPart> _parts = new List<AdditionalPart>();
        internal List<AdditionalPart> Parts
        {
            get { return _parts; }
            set { _parts = value ?? new List<AdditionalPart>(); }
        }
        // ---------------------------------------------------------------

        private bool _showAmountColumn = true;
        public bool ShowAmountColumn
        {
            get { return _showAmountColumn; }
            set
            {
                if (_showAmountColumn != value)
                {
                    _showAmountColumn = value;
                    UpdateAmountColumnVisibility();
                }
            }
        }

        private bool _showPrimColumn = true;
        public bool ShowPrimColumn
        {
            get { return _showPrimColumn; }
            set
            {
                if (_showPrimColumn != value)
                {
                    _showPrimColumn = value;
                    UpdatePrimColumnVisibility();
                }
            }
        }


        public event EventHandler SearchButtonClicked;
        public event EventHandler DeleteButtonClicked;



        public PartsUserControl()
        {
            InitializeComponent();

            // Задаём имена колонкам для последующего доступа по имени
            if (dataGridViewAdditionalParts.Columns.Count >= 5)
            {
                dataGridViewAdditionalParts.Columns[0].Name = "oidMaster";
                dataGridViewAdditionalParts.Columns[1].Name = "number";
                dataGridViewAdditionalParts.Columns[2].Name = "name";
                dataGridViewAdditionalParts.Columns[3].Name = "ATR_BOM_RS";
                dataGridViewAdditionalParts.Columns[4].Name = "amount";
            }

            // Подписываемся на события кнопок
            btnSearch.Click += (s, e) => SearchButtonClicked?.Invoke(this, e);
            btnDelete.Click += (s, e) => DeleteButtonClicked?.Invoke(this, e);
        }


        private void UpdateAmountColumnVisibility()
        {
            if (dataGridViewAdditionalParts.Columns.Contains("amount"))
            {
                dataGridViewAdditionalParts.Columns["amount"].Visible = _showAmountColumn;
            }
        }

        private void UpdatePrimColumnVisibility()
        {
            if (dataGridViewAdditionalParts.Columns.Contains("prim"))
            {
                dataGridViewAdditionalParts.Columns["prim"].Visible = _showPrimColumn;
            }
        }


        // Метод для добавления данных в DataGridView
        public void AddPartsToGrid(List<AdditionalPart> parts)
        {
            this.Parts = parts;
            dataGridViewAdditionalParts.Rows.Clear();

            foreach (Part part in parts)
            {
                int row = dataGridViewAdditionalParts.Rows.Add(part.getDataForRow());
            }
        }

        // Метод для получения выбранных частей
        public List<AdditionalPart> GetSelectedParts()
        {
            var selectedParts = new List<AdditionalPart>();
            foreach (DataGridViewRow row in dataGridViewAdditionalParts.SelectedRows)
            {
                string oidMaster = row.Cells["oidMaster"].Value?.ToString();
                if (!string.IsNullOrEmpty(oidMaster))
                {
                    var part = Parts.FirstOrDefault(p => p.oidMaster == oidMaster);
                    if (part != null)
                    {
                        selectedParts.Add(part);
                    }
                }
            }
            return selectedParts;
        }

        // Метод для обновления данных в DataGridView
        public void RefreshGrid()
        {
            dataGridViewAdditionalParts.Rows.Clear();
            foreach (Part part in Parts)
            {
                int row = dataGridViewAdditionalParts.Rows.Add(part.getDataForRow());
            }
        }


        public bool TrySyncAmountFromGrid(out List<string> errors)
        {
            errors = new List<string>();

            if (!dataGridViewAdditionalParts.Columns.Contains("oidMaster") ||
                !dataGridViewAdditionalParts.Columns.Contains("number") ||
                !dataGridViewAdditionalParts.Columns.Contains("amount"))
            {
                errors.Add("Ошибка: не найдены необходимые колонки в таблице.");
                return false;
            }

            int oidMasterColIndex = dataGridViewAdditionalParts.Columns["oidMaster"].Index;
            int numberColIndex = dataGridViewAdditionalParts.Columns["number"].Index;
            int amountColIndex = dataGridViewAdditionalParts.Columns["amount"].Index;

            var tempUpdates = new Dictionary<string, double>();

            foreach (DataGridViewRow row in dataGridViewAdditionalParts.Rows)
            {
                if (row.IsNewRow) continue;

                var oidCell = row.Cells[oidMasterColIndex];
                if (oidCell.Value == null) continue;
                string oidMaster = oidCell.Value.ToString();
                if (string.IsNullOrEmpty(oidMaster)) continue;

                var numberCell = row.Cells[numberColIndex];
                string partNumber = numberCell.Value?.ToString() ?? "?";

                var amountCell = row.Cells[amountColIndex];
                string amountText = amountCell.Value?.ToString().Trim();

                if (string.IsNullOrEmpty(amountText))
                {
                    tempUpdates[oidMaster] = 1.0;
                    continue;
                }

                if (!double.TryParse(amountText.Replace(',', '.'),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out double amount))
                {
                    errors.Add($"Деталь {partNumber}: значение '{amountText}' не является числом");
                    continue;
                }

                if (amount < 0)
                {
                    errors.Add($"Деталь {partNumber}: количество должно быть положительным числом");
                    continue;
                }

                tempUpdates[oidMaster] = amount;
            }

            if (errors.Count > 0)
                return false;

            foreach (var kvp in tempUpdates)
            {
                var part = Parts.FirstOrDefault(p => p.oidMaster == kvp.Key);
                if (part != null)
                {
                    part.amount = kvp.Value;
                }
            }

            return true;
        }

    }
}
