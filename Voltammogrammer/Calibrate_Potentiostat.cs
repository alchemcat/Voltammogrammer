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

            double p_slope_awg = Properties.Settings.Default.slope_potential_awg;
            double p_slope_osc1 = Properties.Settings.Default.slope_potential_osc1;
            double p_slope_osc2 = Properties.Settings.Default.slope_potential_osc2;
            double c_slope = Properties.Settings.Default.slope_current;

            this.textBoxResistor.Text = r_ref.ToString();

            this.textBoxPotential1.Text = p_awg.ToString();
            this.textBoxPotentialOsc1.Text = p_osc1.ToString();
            this.textBoxPotentialOsc2.Text = p_osc2.ToString();
            this.textBoxCurrent.Text = c.ToString();

            this.textBoxPotential2.Text = p_slope_awg.ToString();
            this.textBoxPotentialSlopeOsc1.Text = p_slope_osc1.ToString();
            this.textBoxPotentialSlopeOsc2.Text = p_slope_osc2.ToString();
            this.textBoxCurrentSlope.Text = c_slope.ToString();

            _ps.SetCalibrationData(
                p_awg,
                (p_osc2 - p_osc1),
                (c - p_osc1 * 1000 / r_ref) / 10000,
                p_slope_awg / 1000,
                p_slope_osc2 / p_slope_osc1,
                (c_slope / (p_slope_osc1 * 1000.0 / r_ref)) // * (r_ref / 1000.0)),
                );
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
            double r_ref, p_awg, p_osc1, p_osc2, c, c_slope, p_slope_osc1, p_slope_osc2, p_slope_awg;

            if(
                   Double.TryParse(this.textBoxPotential1.Text, out p_awg)
                && Double.TryParse(this.textBoxResistor.Text, out r_ref)
                && Double.TryParse(this.textBoxPotentialOsc1.Text, out p_osc1)
                && Double.TryParse(this.textBoxPotentialOsc2.Text, out p_osc2)
                && Double.TryParse(this.textBoxCurrent.Text, out c)
                && Double.TryParse(this.textBoxPotential2.Text, out p_slope_awg)
                && Double.TryParse(this.textBoxPotentialSlopeOsc1.Text, out p_slope_osc1)
                && Double.TryParse(this.textBoxPotentialSlopeOsc2.Text, out p_slope_osc2)
                && Double.TryParse(this.textBoxCurrentSlope.Text, out c_slope)
            )
            {
                _ps.SetCalibrationData(
                    p_awg,
                    (p_osc2 - p_osc1),
                    (c - p_osc1 * 1000 / r_ref) / 10000,
                    p_slope_awg / 1000.0,
                    p_slope_osc2 / p_slope_osc1,
                    1.0 // * (r_ref / 1000.0)),
                    );

                //Properties.Settings.Default.offset_resistor_ref = r_ref;

                //Properties.Settings.Default.offset_potential_awg = p_awg;
                //Properties.Settings.Default.offset_potential_osc1 = p_osc1;
                //Properties.Settings.Default.offset_potential_osc2 = p_osc2;
                //Properties.Settings.Default.offset_current = c / 10000; // Assuming calibration was done using +-20mA range

                //Properties.Settings.Default.slope_potential_awg = p_slope_awg;
                //Properties.Settings.Default.slope_potential_osc1 = p_slope_osc1;
                //Properties.Settings.Default.slope_potential_osc2 = p_slope_osc2;
                //Properties.Settings.Default.slope_current = c_slope;

                //Properties.Settings.Default.Save();
            }
            else
            {
                MessageBox.Show(this, "Input number(s)!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void setID(string id)
        {
            toolStripTextBoxID.Text = id;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            double r_ref, p_awg, p_osc1, p_osc2, c, c_slope, p_slope_osc1, p_slope_osc2, p_slope_awg;

            if(
                   Double.TryParse(this.textBoxPotential1.Text, out p_awg)
                && Double.TryParse(this.textBoxResistor.Text, out r_ref)
                && Double.TryParse(this.textBoxPotentialOsc1.Text, out p_osc1)
                && Double.TryParse(this.textBoxPotentialOsc2.Text, out p_osc2)
                && Double.TryParse(this.textBoxCurrent.Text, out c)
                && Double.TryParse(this.textBoxPotential2.Text, out p_slope_awg)
                && Double.TryParse(this.textBoxPotentialSlopeOsc1.Text, out p_slope_osc1)
                && Double.TryParse(this.textBoxPotentialSlopeOsc2.Text, out p_slope_osc2)
                && Double.TryParse(this.textBoxCurrentSlope.Text, out c_slope)
            )
            {
                _ps.SetCalibrationData(
                    p_awg,
                    (p_osc2 - p_osc1),
                    (c - p_osc1 * 1000 / r_ref) / 10000,
                    p_slope_awg / 1000.0,
                    p_slope_osc2 / p_slope_osc1,
                    (c_slope / (p_slope_osc1 * 1000.0/ r_ref)) // * (r_ref / 1000.0)),
                    );

                Properties.Settings.Default.offset_resistor_ref = r_ref;

                Properties.Settings.Default.offset_potential_awg = p_awg;
                Properties.Settings.Default.offset_potential_osc1 = p_osc1;
                Properties.Settings.Default.offset_potential_osc2 = p_osc2;
                Properties.Settings.Default.offset_current = c / 10000; // Assuming calibration was done using +-20mA range

                Properties.Settings.Default.slope_potential_awg = p_slope_awg;
                Properties.Settings.Default.slope_potential_osc1 = p_slope_osc1;
                Properties.Settings.Default.slope_potential_osc2 = p_slope_osc2;
                Properties.Settings.Default.slope_current = c_slope;

                Properties.Settings.Default.Save();

                this.Hide();
            }
            else
            {
                MessageBox.Show(this, "Input number(s)!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
