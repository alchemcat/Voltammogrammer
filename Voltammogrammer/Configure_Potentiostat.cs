
/*
    PocketPotentiostat

    Copyright (C) 2019-2022 Yasuo Matsubara

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA
*/

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

        configration_data _data;

        [System.Xml.Serialization.XmlRoot(Namespace = "https://doi.org/10.1021/acs.jchemed.1c00228")]
        public class configration_data
        {
            public DataTable tableResistors;
            public int filtering_method = 0;
            public int submodule = 0;

            public configration_data()
            {
                tableResistors = new DataTable("resistor_table1");

                tableResistors.Columns.Add("Resistor");
                tableResistors.Columns.Add("Value [ohm]", typeof(double));
                tableResistors.Columns.Add("Caption (Current)", typeof(string));
                tableResistors.Columns.Add("Caption (EIS)", typeof(string));
                tableResistors.Rows.Add("R1", 10, "+- 200 mA", "10 Ohm");
                tableResistors.Rows.Add("R2", 100, "+- 20 mA", "100 Ohm");
                tableResistors.Rows.Add("R3", 1000, "+- 2 mA", "1 kOhm");
                tableResistors.Rows.Add("R4", 10000, "+- 200 uA", "10 kOhm");
                tableResistors.Rows.Add("R5", 100000, "+- 20 uA", "100 kOhm");
                tableResistors.Rows.Add("R6", 1000000, "+- 2 uA", "1 MOhm");

                tableResistors.Rows.Add("R7 (=R4, raw)", 10000, "+- 200 uA (raw)", "10 kOhm"); // For RAW mode
            }
        }

        public string SerializedValues
        {
            set 
            {
                if (value == "")
                {
                    _data = Reset();
                }
                else
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(configration_data)); // _tableResistor.GetType()
                    StringReader sr = new StringReader(value);

                    XmlReader xr = XmlReader.Create(sr);
                    _data = (configration_data)serializer.Deserialize(xr);
                    xr.Close();
                }

                UpdateUI();

                // Potentiostat本体の値を更新
                _ps.UpdateResistors(_data.tableResistors);
            }
            get 
            {
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(configration_data));
                StringBuilder sb = new StringBuilder();
                XmlWriter xw = XmlWriter.Create(sb);
                serializer.Serialize(xw, _data);
                //ファイルを閉じる
                xw.Close();

                return sb.ToString();
            }
        }

        public DataTable tableResistors
        {
            get 
            {
                return _data.tableResistors;
            }
        }

        public int filtering_method
        {
            get
            {
                return _data.filtering_method;
            }
        }

        public int submodule
        {
            get
            {
                return _data.submodule;
            }
        }

        public Configure_Potentiostat(Potentiostat ps)
        {
            InitializeComponent();

            _ps = ps;

            _data = new configration_data();
        }

        //public void InitializeResistors()
        //{
        //    string r1 = Properties.Settings.Default.configure_resistor_values;
        //    if (r1 == "")
        //    {
        //        Reset();
        //    }
        //    else
        //    {
        //        XmlSerializer serializer = new XmlSerializer(typeof(DataTable)); // _tableResistor.GetType()
        //        StringReader sr = new StringReader(r1);

        //        XmlReader xr = XmlReader.Create(sr);
        //        _tableResistor = (DataTable)serializer.Deserialize(xr);
        //        xr.Close();
        //    }

        //    UpdateUI();

        //    // Potentiostat本体の値を更新
        //    _ps.UpdateResistors(_tableResistor);
        //}

        private configration_data Reset()
        {
            return new configration_data();
        }

        private void UpdateUI()
        {
            dataGridView1.DataSource = _data.tableResistors.Copy();
            comboBoxFilteringMethod.SelectedIndex = _data.filtering_method;
            comboBoxSubmodule.SelectedIndex = _data.submodule;
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

            _data.tableResistors = ((DataTable)dataGridView1.DataSource).Copy();
            _data.filtering_method = comboBoxFilteringMethod.SelectedIndex;
            _data.submodule = comboBoxSubmodule.SelectedIndex;

            //System.Xml.Serialization.XmlSerializer serializer2 = new System.Xml.Serialization.XmlSerializer(typeof(DataTable));
            //StringBuilder sb2 = new StringBuilder();
            //XmlWriter xw2 = XmlWriter.Create(sb2);
            //serializer2.Serialize(xw2, _tableResistor);
            ////ファイルを閉じる
            //xw2.Close();

            //Properties.Settings.Default.configure_resistor_values = sb2.ToString();
            //Properties.Settings.Default.configure_filtering_method = comboBoxFilteringMethod.SelectedIndex;
            //Properties.Settings.Default.configure_submodule_pluggedin = comboBoxSubmodule.SelectedIndex;
            //Properties.Settings.Default.Save();

            // Potentiostat本体の値を更新
            _ps.UpdateResistors(_data.tableResistors);
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

            _data = Reset();
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
