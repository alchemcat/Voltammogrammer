using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Collections;
using System.Xml;
using System.Runtime.Serialization;
using System.IO;
using System.Xml.Serialization;

namespace Voltammogrammer
{
    public partial class Configure_Potentiostat : Form
    {
        Potentiostat _ps;
        DataTable _tableRegister;

        public Configure_Potentiostat(Potentiostat ps)
        {
            InitializeComponent();

            _ps = ps;
            //double p = Properties.Settings.Default.offset_potential;
            //double c = Properties.Settings.Default.offset_current;
            //this.textBoxPotential.Text = p.ToString();
            //this.textBoxCurrent.Text = c.ToString();

            //_ps.SetCalibrationData(p, c);

            string r1 = Properties.Settings.Default.configure_register_values;
            _tableRegister = new DataTable("register_table1");
            if (r1 == "")
            {
                //_tableRegister.Columns.Add("Register");
                //_tableRegister.Columns.Add("Value [ohm]", typeof(int));
                //_tableRegister.Columns.Add("Caption (Current)", typeof(string));
                //_tableRegister.Columns.Add("Caption (EIS)", typeof(string));
                //_tableRegister.Rows.Add("R1", 10, "+- 200 mA",    "10 Ohm");
                //_tableRegister.Rows.Add("R2", 100, "+- 20 mA",    "100 Ohm");
                //_tableRegister.Rows.Add("R3", 1000, "+- 2 mA",    "1 kOhm");
                //_tableRegister.Rows.Add("R4", 10000, "+- 200 uA", "10 kOhm");
                //_tableRegister.Rows.Add("R5", 100000, "+- 20 uA", "100 kOhm");
                //_tableRegister.Rows.Add("R6", 1000000, "+- 2 uA", "1 MOhm");
            }
            else
            {
                XmlSerializer serializer = new XmlSerializer(typeof(DataTable)); // _tableRegister.GetType()
                StringReader sr = new StringReader(r1);

                XmlReader xr = XmlReader.Create(sr);
                _tableRegister = (DataTable)serializer.Deserialize(xr);
                xr.Close();

            }
            dataGridView1.DataSource = _tableRegister;

            int r2 = Properties.Settings.Default.configure_filtering_method;
            comboBoxFilteringMethod.SelectedIndex = (r2);
        }

        private void Configure_Potentiostat_FormClosing(object sender, FormClosingEventArgs e)
        {
            ((CancelEventArgs)e).Cancel = true;
            this.Hide();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            //double p, c;

            //if (Double.TryParse(this.textBoxPotential.Text, out p) && Double.TryParse(this.textBoxCurrent.Text, out c))
            //{
            //    Properties.Settings.Default.offset_potential = p;
            //    Properties.Settings.Default.offset_current = c;
            //    Properties.Settings.Default.Save();

            //    _ps.SetCalibrationData(p, c);
            //}

            dataGridView1[1, 6].Value = dataGridView1[1, 3].Value;
            dataGridView1[2, 6].Value = dataGridView1[2, 3].Value;
            dataGridView1[3, 6].Value = dataGridView1[3, 3].Value;

            System.Xml.Serialization.XmlSerializer serializer2 = new System.Xml.Serialization.XmlSerializer(typeof(DataTable));
            StringBuilder sb2 = new StringBuilder();
            XmlWriter xw2 = XmlWriter.Create(sb2);
            serializer2.Serialize(xw2, _tableRegister);
            //ファイルを閉じる
            xw2.Close();


            Properties.Settings.Default.configure_register_values = sb2.ToString();
            Properties.Settings.Default.configure_filtering_method = comboBoxFilteringMethod.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.configure_register_values = "";
            Properties.Settings.Default.configure_filtering_method = 0;
            Properties.Settings.Default.Save();
        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dataGridView1.Columns[0].ReadOnly = true;
            dataGridView1.Columns[0].DefaultCellStyle.BackColor = Color.LightGray;

            dataGridView1.Rows[6].DefaultCellStyle.BackColor = Color.LightGray;
            dataGridView1.Rows[6].ReadOnly = true;
        }
    }
}
