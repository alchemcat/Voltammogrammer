using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using System.Xml.Serialization;
using System.Xml;


namespace Voltammogrammer
{
    public partial class Calibrate_Potentiostat : Form
    {
        Potentiostat _ps;
        Configure_Potentiostat _configure_ps;
        calibration_data _data;

        //public double ohmInternalResistance = 0.0;
        public double ohmInternalResistance
        {
            set
            {
                _data.res = value;
                //ohmInternalResistance = _data.res;
            }
            get
            {
                return _data.res;
            }
        }

        //public double r_ref, p_awg, p_osc1, p_osc2, c, c_slope, p_slope_osc1, p_slope_osc2, p_slope_awg;
        [System.Xml.Serialization.XmlRoot(Namespace = "https://doi.org/10.1021/acs.jchemed.1c00228")]
        public class calibration_data
        {
            public string id;
            public double res;
            public double r_ref, p_awg, p_osc1, p_osc2, c, c_slope, p_slope_osc1, p_slope_osc2, p_slope_awg;

            //[XmlIgnore]
            public string configure_resistor_values;

            public calibration_data()
            {

            }
            //public calibration_data(string id, double res, double r_ref, double p_awg, double p_osc1, double p_osc2, double c, double c_slope, double p_slope_osc1, double p_slope_osc2, double p_slope_awg, string configure_resistor_values)
            //{
            //    this.id           = id;

            //    this.res          = res;
            //    this.r_ref        = r_ref;

            //    this.p_awg        = p_awg;
            //    this.p_osc1       = p_osc1;
            //    this.p_osc2       = p_osc2;
            //    this.c            = c;

            //    this.p_slope_awg  = p_slope_awg;
            //    this.p_slope_osc1 = p_slope_osc1;
            //    this.p_slope_osc2 = p_slope_osc2;
            //    this.c_slope      = c_slope;

            //    this.configure_resistor_values = configure_resistor_values;
            //}

            //public void getData(out string id, out double res, out double r_ref, out double p_awg, out double p_osc1, out double p_osc2, out double c, out double c_slope, out double p_slope_osc1, out double p_slope_osc2, out double p_slope_awg, out string configure_resistor_values)
            //{
            //    id = this.id;

            //    res = this.res;
            //    r_ref = this.r_ref;

            //    p_awg = this.p_awg;
            //    p_osc1 = this.p_osc1;
            //    p_osc2 = this.p_osc2;
            //    c = this.c;

            //    p_slope_awg = this.p_slope_awg;
            //    p_slope_osc1 = this.p_slope_osc1;
            //    p_slope_osc2 = this.p_slope_osc2;
            //    c_slope = this.c_slope;

            //    configure_resistor_values = this.configure_resistor_values;
            //}

            //public double r_ref { set; get; } 
            //public double p_awg { set; get; } 
            //public double p_osc1 { set; get; }
            //public double p_osc2 { set; get; }
            //public double c { set; get; }
            //public double c_slope { set; get; }
            //public double p_slope_osc1 { set; get; }
            //public double p_slope_osc2 { set; get; }
            //public double p_slope_awg { set; get; }
        }

        public Calibrate_Potentiostat(Potentiostat ps, Configure_Potentiostat configure_ps)
        {
            InitializeComponent();

            _ps = ps;
            _configure_ps = configure_ps;

            // 初期値をProperties.Settingsにベタ打ちするのではなく、calibration_dataクラスをシリアライズする方法により(_tableResistorの様に)初期値を判断する。
            // そうすれば、ビルド構成におけるコンパイル定数によって初期値を替えれるようになる（R2/R1が変わったとき様）。
            string r1 = Properties.Settings.Default.calibration_data;
            if (r1 == "")
            {
                Reset();
            }
            else
            {
                XmlSerializer serializer = new XmlSerializer(typeof(calibration_data));
                StringReader sr = new StringReader(r1);
                XmlReader xr = XmlReader.Create(sr);
                _data = (calibration_data)serializer.Deserialize(xr);
                xr.Close();
            }

            UpdateUI();

            _ps.SetCalibrationData(
                _data.p_awg,
                (_data.p_osc2 - _data.p_osc1),
                (_data.c - _data.p_osc1 * 1000 / _data.r_ref) * ((1 / (double)Potentiostat.rangeCurrent.Range20mA) + _data.res / 1000000), // (c [uA] - (p_osc1 * 1000) [uV] / r_ref [ohm]) / 10000
                _data.p_slope_awg / 1000,
                _data.p_slope_osc2 / _data.p_slope_osc1,
                (_data.c_slope / (_data.p_slope_osc1 * 1000.0 / _data.r_ref))//     / (1 + 0.5 / (1000000 / (double)Potentiostat.rangeCurrent.Range20mA))// * (r_ref / 1000.0)),
                );
        }

        private void Reset()
        {
            _data = new calibration_data();

            _data.res = 0.5;  // internal_resistance
            _data.r_ref = 1000; // offset_resistor_ref

            _data.p_awg = 0; // offset_potential_awg
            _data.p_osc1 = 0; // offset_potential_osc1
            _data.p_osc2 = 0; // offset_potential_osc2
            _data.c = 0; // offset_current (Assuming calibration was done using +-20mA range) 

            _data.p_slope_awg = 1000; // slope_potential_awg
            _data.p_slope_osc1 = 1000; // slope_potential_osc1
            _data.p_slope_osc2 = 1000; // slope_potential_osc2
            _data.c_slope = 1000; // slope_current
        }

        private void UpdateUI()
        {
            this.toolStripTextBoxInternalResistance.Text = _data.res.ToString();
            this.textBoxResistor.Text = _data.r_ref.ToString();

            this.textBoxPotential1.Text = _data.p_awg.ToString();
            this.textBoxPotentialOsc1.Text = _data.p_osc1.ToString();
            this.textBoxPotentialOsc2.Text = _data.p_osc2.ToString();
            this.textBoxCurrent.Text = _data.c.ToString();

            this.textBoxPotential2.Text = _data.p_slope_awg.ToString();
            this.textBoxPotentialSlopeOsc1.Text = _data.p_slope_osc1.ToString();
            this.textBoxPotentialSlopeOsc2.Text = _data.p_slope_osc2.ToString();
            this.textBoxCurrentSlope.Text = _data.c_slope.ToString();
        }

        private void Calibrate_Potentiostat_Load(object sender, EventArgs e)
        {

        }

        private void Calibrate_Potentiostat_FormClosing(object sender, FormClosingEventArgs e)
        {
            ((CancelEventArgs)e).Cancel = true;
            this.Hide();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            if(
                   Double.TryParse(this.toolStripTextBoxInternalResistance.Text, out _data.res)
                && Double.TryParse(this.textBoxPotential1.Text, out _data.p_awg)
                && Double.TryParse(this.textBoxResistor.Text, out _data.r_ref)
                && Double.TryParse(this.textBoxPotentialOsc1.Text, out _data.p_osc1)
                && Double.TryParse(this.textBoxPotentialOsc2.Text, out _data.p_osc2)
                && Double.TryParse(this.textBoxCurrent.Text, out _data.c)
                && Double.TryParse(this.textBoxPotential2.Text, out _data.p_slope_awg)
                && Double.TryParse(this.textBoxPotentialSlopeOsc1.Text, out _data.p_slope_osc1)
                && Double.TryParse(this.textBoxPotentialSlopeOsc2.Text, out _data.p_slope_osc2)
                && Double.TryParse(this.textBoxCurrentSlope.Text, out _data.c_slope)
            )
            {
                _ps.SetCalibrationData(
                    _data.p_awg,
                    (_data.p_osc2 - _data.p_osc1),
                    (_data.c - _data.p_osc1 * 1000 / _data.r_ref) * ((1 / (double)Potentiostat.rangeCurrent.Range20mA) + _data.res / 1000000),
                    _data.p_slope_awg / 1000.0,
                    _data.p_slope_osc2 / _data.p_slope_osc1,
                    (1.0)// / (1 + 0.5 / (1000000 / (double)Potentiostat.rangeCurrent.Range20mA))) // * (r_ref / 1000.0)),
                    );
            }
            else
            {
                MessageBox.Show(this, "Input number(s)!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void setID(string id)
        {
            _data.id = id;
            toolStripTextBoxID.Text = id;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if(
                   Double.TryParse(this.toolStripTextBoxInternalResistance.Text, out _data.res)
                && Double.TryParse(this.textBoxPotential1.Text, out _data.p_awg)
                && Double.TryParse(this.textBoxResistor.Text, out _data.r_ref)
                && Double.TryParse(this.textBoxPotentialOsc1.Text, out _data.p_osc1)
                && Double.TryParse(this.textBoxPotentialOsc2.Text, out _data.p_osc2)
                && Double.TryParse(this.textBoxCurrent.Text, out _data.c)
                && Double.TryParse(this.textBoxPotential2.Text, out _data.p_slope_awg)
                && Double.TryParse(this.textBoxPotentialSlopeOsc1.Text, out _data.p_slope_osc1)
                && Double.TryParse(this.textBoxPotentialSlopeOsc2.Text, out _data.p_slope_osc2)
                && Double.TryParse(this.textBoxCurrentSlope.Text, out _data.c_slope)
            )
            {
                System.Xml.Serialization.XmlSerializer serializer2 = new System.Xml.Serialization.XmlSerializer(typeof(calibration_data));
                StringBuilder sb2 = new StringBuilder();
                XmlWriter xw2 = XmlWriter.Create(sb2);
                serializer2.Serialize(xw2, _data);
                //ファイルを閉じる
                xw2.Close();

                Properties.Settings.Default.calibration_data = sb2.ToString();
                Properties.Settings.Default.Save();

                _ps.SetCalibrationData(
                    _data.p_awg,
                    (_data.p_osc2 - _data.p_osc1),
                    (_data.c - _data.p_osc1 * 1000 / _data.r_ref) * ((1/(double)Potentiostat.rangeCurrent.Range20mA) + _data.res / 1000000),
                    _data.p_slope_awg / 1000.0,
                    _data.p_slope_osc2 / _data.p_slope_osc1,
                    (_data.c_slope / (_data.p_slope_osc1 * 1000.0/ _data.r_ref)) //      / (1 + 0.5 / (1000000 / (double)Potentiostat.rangeCurrent.Range20mA)) // * (r_ref / 1000.0)),
                    );

                this.Hide();
            }
            else
            {
                MessageBox.Show(this, "Input number(s)!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            DialogResult r = MessageBox.Show
            (
                "Are you sure to reset the calibration data?",
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

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {

            saveFileDialog1.Filter = "Calibration data file (*.xml)|*.xml|All types(*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.Title = "Save As";
            saveFileDialog1.FileName = "calibration_data_" + toolStripTextBoxID.Text + ".xml"; 
            saveFileDialog1.ShowHelp = true;

            if (saveFileDialog1.InitialDirectory == null)
            {
                saveFileDialog1.InitialDirectory = @"c:\";
            }

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string file_path = saveFileDialog1.FileName;
                //if (System.IO.Path.GetExtension(_file_path) != ".mpt") _file_path += ".mpt";

                //XMLDataHolder calibration_data = new XMLDataHolder("calibration-data");
                //calibration_data.SetDatum("version", this.Text);
                //TextWriter _writer = new StreamWriter(file_path, false);
                //_writer.WriteLine(calibration_data.GetData().OuterXml);
                //_writer.Close();

                //calibration_data c_data = new calibration_data(
                //    toolStripTextBoxID.Text, 
                //    ohmInternalResistance, r_ref, p_awg, p_osc1, p_osc2, c, c_slope, p_slope_osc1, p_slope_osc2, p_slope_awg,
                //    Properties.Settings.Default.configure_resistor_values
                //);

                _data.configure_resistor_values = _configure_ps.SerializedValues; // Properties.Settings.Default.configure_resistor_values;

                //XmlSerializerオブジェクトを作成
                //オブジェクトの型を指定する
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(calibration_data));
                //書き込むファイルを開く（UTF-8 BOM無し）
                System.IO.StreamWriter sw = new System.IO.StreamWriter(file_path, false, new System.Text.UTF8Encoding(false));
                //シリアル化し、XMLファイルに保存する
                serializer.Serialize(sw, _data);
                //ファイルを閉じる
                sw.Close();
            }
        }

        private void toolStripButtonLoad_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Calibration data file (*.xml)|*.xml|All types(*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Title = "Open File As Voltammogram";
            openFileDialog1.FileName = "";
            openFileDialog1.ShowHelp = true;
            openFileDialog1.SupportMultiDottedExtensions = true;
            openFileDialog1.Multiselect = false;

            if (openFileDialog1.InitialDirectory == null)
            {
                openFileDialog1.InitialDirectory = @"c:\";
            }

            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                for (int i = 0; i < openFileDialog1.FileNames.Length; i++)
                {
                    string file_path = openFileDialog1.FileNames[i];

                    //XmlSerializerオブジェクトを作成
                    System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(calibration_data));
                    //読み込むファイルを開く
                    System.IO.StreamReader sr = new System.IO.StreamReader(file_path, new System.Text.UTF8Encoding(false));
                    //XMLファイルから読み込み、逆シリアル化する
                    calibration_data _temp_data = (calibration_data)serializer.Deserialize(sr);
                    //ファイルを閉じる
                    sr.Close();

                    if(_temp_data.id != _data.id)
                    {
                        DialogResult r = MessageBox.Show
                        (
                            "Are you sure to override the current calibration data?",
                            "Warning",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning
                        );
                        if (r == DialogResult.No)
                        {
                            return;
                        }
                    }

                    //c_data.getData(
                    //    out string id, 
                    //    out ohmInternalResistance, out r_ref, out p_awg, out p_osc1, out p_osc2, out c, out c_slope, out p_slope_osc1, out p_slope_osc2, out p_slope_awg, 
                    //    out string configure_resistor_values
                    //);            

                    _data = _temp_data;

                    UpdateUI();

                    ////this.toolStripTextBoxID.Text = id;

                    //this.toolStripTextBoxInternalResistance.Text = ohmInternalResistance.ToString();
                    //this.textBoxResistor.Text = r_ref.ToString();

                    //this.textBoxPotential1.Text = p_awg.ToString();
                    //this.textBoxPotentialOsc1.Text = p_osc1.ToString();
                    //this.textBoxPotentialOsc2.Text = p_osc2.ToString();
                    //this.textBoxCurrent.Text = c.ToString();

                    //this.textBoxPotential2.Text = p_slope_awg.ToString();
                    //this.textBoxPotentialSlopeOsc1.Text = p_slope_osc1.ToString();
                    //this.textBoxPotentialSlopeOsc2.Text = p_slope_osc2.ToString();
                    //this.textBoxCurrentSlope.Text = c_slope.ToString();

                    //Properties.Settings.Default.configure_resistor_values = _data.configure_resistor_values;
                    //Properties.Settings.Default.Save();
                    //_configure_ps.InitializeResistors();

                    //// Potentiostat本体の値を更新
                    //_ps.UpdateResistors();

                    _configure_ps.SerializedValues = _data.configure_resistor_values;
                }
            }
        }

        private void buttonConfigureResistors_Click(object sender, EventArgs e)
        {
            _configure_ps.Show();
        }
    }
}
