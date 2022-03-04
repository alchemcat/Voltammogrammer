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
        public DataTable _tableResistor;
        public string SerializedValues
        {
            set 
            {
                Properties.Settings.Default.configure_resistor_values = value;
                Properties.Settings.Default.Save();

                InitializeResistors();                
            }
            get 
            { 
                return Properties.Settings.Default.configure_resistor_values;
            }
        }

        public Configure_Potentiostat(Potentiostat ps)
        {
            InitializeComponent();

            _ps = ps;

            InitializeResistors();

            int r2 = Properties.Settings.Default.configure_filtering_method;
            comboBoxFilteringMethod.SelectedIndex = (r2);
        }

        public void InitializeResistors()
        {
            string r1 = Properties.Settings.Default.configure_resistor_values;
            if (r1 == "")
            {
                Reset();
            }
            else
            {
                XmlSerializer serializer = new XmlSerializer(typeof(DataTable)); // _tableResistor.GetType()
                StringReader sr = new StringReader(r1);

                XmlReader xr = XmlReader.Create(sr);
                _tableResistor = (DataTable)serializer.Deserialize(xr);
                xr.Close();
            }

            UpdateUI();

            // Potentiostat本体の値を更新
            _ps.UpdateResistors(_tableResistor);
        }

        private void Reset()
        {
            _tableResistor = new DataTable("resistor_table1");

            _tableResistor.Columns.Add("Resistor");
            _tableResistor.Columns.Add("Value [ohm]", typeof(double));
            _tableResistor.Columns.Add("Caption (Current)", typeof(string));
            _tableResistor.Columns.Add("Caption (EIS)", typeof(string));
            _tableResistor.Rows.Add("R1", 10, "+- 200 mA", "10 Ohm");
            _tableResistor.Rows.Add("R2", 100, "+- 20 mA", "100 Ohm");
            _tableResistor.Rows.Add("R3", 1000, "+- 2 mA", "1 kOhm");
            _tableResistor.Rows.Add("R4", 10000, "+- 200 uA", "10 kOhm");
            _tableResistor.Rows.Add("R5", 100000, "+- 20 uA", "100 kOhm");
            _tableResistor.Rows.Add("R6", 1000000, "+- 2 uA", "1 MOhm");

            _tableResistor.Rows.Add("R7 (=R4, raw)", 10000, "+- 200 uA (raw)", "10 kOhm"); // For RAW mode
        }

        private void UpdateUI()
        {
            dataGridView1.DataSource = _tableResistor;
        }

        private void Configure_Potentiostat_FormClosing(object sender, FormClosingEventArgs e)
        {
            ((CancelEventArgs)e).Cancel = true;
            this.Hide();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            dataGridView1[1, 6].Value = dataGridView1[1, 3].Value;
            dataGridView1[2, 6].Value = dataGridView1[2, 3].Value;
            dataGridView1[3, 6].Value = dataGridView1[3, 3].Value;

            System.Xml.Serialization.XmlSerializer serializer2 = new System.Xml.Serialization.XmlSerializer(typeof(DataTable));
            StringBuilder sb2 = new StringBuilder();
            XmlWriter xw2 = XmlWriter.Create(sb2);
            serializer2.Serialize(xw2, _tableResistor);
            //ファイルを閉じる
            xw2.Close();

            Properties.Settings.Default.configure_resistor_values = sb2.ToString();
            Properties.Settings.Default.configure_filtering_method = comboBoxFilteringMethod.SelectedIndex;
            Properties.Settings.Default.Save();

            // Potentiostat本体の値を更新
            _ps.UpdateResistors(_tableResistor);
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            DialogResult r = MessageBox.Show
            (
                "Are you sure to reset the configuration data?",
                "Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );
            if (r == DialogResult.No)
            {
                return;
            }

            Reset();
            UpdateUI();
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
