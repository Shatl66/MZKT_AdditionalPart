using E3WGM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace E3SetAdditionalPart
{
    public partial class SearchForm : Form
    {
        List<AdditionalPart> resultSearchData;
        public List<AdditionalPart> resultSelectedData;

        public SearchForm()
        {
            InitializeComponent();

            this.AcceptButton = buttonSearch; // поиск начнется по клавише "Ввод"
        }



        private void buttonSearch_Click(object sender, EventArgs e)
        {
            if (!E3SetAdditionalPartForm.wchHTTPClient.isAuthorization())
            {
                WindchillLoginForm wchLogin = new WindchillLoginForm(E3SetAdditionalPartForm.wchHTTPClient);
                wchLogin.ShowDialog();
                if (wchLogin.DialogResult.Equals(DialogResult.Cancel))
                {
                    return;
                }
            }

            Dictionary<String, String> searchData = new Dictionary<String, String>();

            if ((textBoxNumber.Text+textBoxName.Text) != "")
            {
                if (textBoxNumber.Text.Trim().Length>3||textBoxName.Text.Trim().Length>3)
                {
                    searchData.Add("number", textBoxNumber.Text.Trim());
                    searchData.Add("name", textBoxName.Text.Trim());
                } else
                {
                    MessageBox.Show("Критерии поиска должны содержать более 3 символов.");
                    return;
                }
                
            } else
            {
                MessageBox.Show("Критерии поиска не заполнены.");
                return;
            }

            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings();
            settings.UseSimpleDictionaryFormat = true;
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Dictionary<String, String>), settings);
            ser.WriteObject(stream, searchData);
            stream.Position = 0;
            StreamReader sr = new StreamReader(stream);
            string jsonObjArray = sr.ReadToEnd();

            string jsonListE3Project = E3SetAdditionalPartForm.wchHTTPClient.getJSON(jsonObjArray, "netmarkets/jsp/by/iba/e3/http/findPart.jsp");

            MemoryStream stream2 = new MemoryStream(Encoding.UTF8.GetBytes(jsonListE3Project));
            DataContractJsonSerializer ser2 = new DataContractJsonSerializer(typeof(List<AdditionalPart>), settings);
            resultSearchData = (List<AdditionalPart>)ser2.ReadObject(stream2);

            dataGridView1.Rows.Clear();
            foreach (AdditionalPart part in resultSearchData)
            {
                int row = dataGridView1.Rows.Add(part.getDataForRow());
                dataGridView1.Rows[row].ReadOnly = true;
            }
       }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            resultSelectedData = new List<AdditionalPart>();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            resultSelectedData = new List<AdditionalPart>();
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                resultSelectedData.Add(resultSearchData.Find(x => x.oidMaster == (string)row.Cells["oidMaster"].Value));
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                buttonOk.Enabled = false;
            }
            else
            {
                buttonOk.Enabled = true;
            }
        }
    }
}
