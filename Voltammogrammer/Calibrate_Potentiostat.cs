using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Voltammogrammer
{
    public partial class Calibrate_Potentiostat : Form
    {
        Potentiostat _ps;

        public Calibrate_Potentiostat(Potentiostat ps)
        {
            InitializeComponent();

            _ps = ps;
            double r_ref = Properties.Settings.Default.offset_resistor_ref;
            double p_awg = Properties.Settings.Default.offset_potential_awg;
            double p_osc1 = Properties.Settings.Default.offset_potential_osc1;
            double p_osc2 = Properties.Settings.Default.offset_potential_osc2;
            double c = Properties.Settings.Default.offset_current * 10000; // Assuming calibration was done using +-20mA range
            double p_slope = Properties.Settings.Default.slope_potential;
            double c_slope = Properties.Settings.Default.slope_current;

            this.textBoxResistor.Text = r_ref.ToString();
            this.textBoxPotential1.Text = p_awg.ToString();
            this.textBoxPotentialOsc1.Text = p_osc1.ToString();
            this.textBoxPotentialOsc2.Text = p_osc2.ToString();
            this.textBoxCurrent.Text = c.ToString();
            this.textBoxPotentialSlope.Text = p_slope.ToString();
            this.textBoxCurrentSlope.Text = c_slope.ToString();

            _ps.SetCalibrationData(p_awg, (p_osc2 - p_osc1), (c - p_osc1*1000/r_ref)/10000, p_slope/1000, c_slope/1000);
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
            double r_ref, p_awg, p_osc1, p_osc2, c, c_slope, p_slope;

            if(Double.TryParse(this.textBoxPotential1.Text, out p_awg)
                && Double.TryParse(this.textBoxResistor.Text, out r_ref)
                && Double.TryParse(this.textBoxPotentialOsc1.Text, out p_osc1)
                && Double.TryParse(this.textBoxPotentialOsc2.Text, out p_osc2)
                && Double.TryParse(this.textBoxCurrent.Text, out c)
                && Double.TryParse(this.textBoxPotentialSlope.Text, out p_slope)
                && Double.TryParse(this.textBoxCurrentSlope.Text, out c_slope)
            )
            {
                Properties.Settings.Default.offset_resistor_ref = r_ref;
                Properties.Settings.Default.offset_potential_awg = p_awg;
                Properties.Settings.Default.offset_potential_osc1 = p_osc1;
                Properties.Settings.Default.offset_potential_osc2 = p_osc2;
                Properties.Settings.Default.offset_current = c / 10000; // Assuming calibration was done using +-20mA range
                Properties.Settings.Default.slope_potential = p_slope;
                Properties.Settings.Default.slope_current = c_slope;
                Properties.Settings.Default.Save();

                _ps.SetCalibrationData(p_awg, (p_osc2 - p_osc1), (c - p_osc1 * 1000 / r_ref) / 10000, p_slope/1000, c_slope/1000);
            }
        }

        public void setID(string id)
        {
            toolStripTextBoxID.Text = id;
        }
    }
}
