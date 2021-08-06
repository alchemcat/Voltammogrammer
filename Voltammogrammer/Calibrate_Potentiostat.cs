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
            double p_awg = Properties.Settings.Default.offset_potential_awg;
            double p_osc = Properties.Settings.Default.offset_potential_osc;
            double c = Properties.Settings.Default.offset_current * 10000; // Assuming calibration was done using +-20mA range
            this.textBoxPotential.Text = p_awg.ToString();
            this.textBoxPotentialOsc.Text = p_osc.ToString();
            this.textBoxCurrent.Text = c.ToString();

            _ps.SetCalibrationData(p_awg, p_osc, c/10000);
        }

        private void Calibrate_Potentiostat_FormClosing(object sender, FormClosingEventArgs e)
        {
            ((CancelEventArgs)e).Cancel = true;
            this.Hide();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            double p_awg, p_osc, c;

            if(Double.TryParse(this.textBoxPotential.Text, out p_awg)
                && Double.TryParse(this.textBoxPotentialOsc.Text, out p_osc)
                && Double.TryParse(this.textBoxCurrent.Text, out c))
            {
                Properties.Settings.Default.offset_potential_awg = p_awg;
                Properties.Settings.Default.offset_potential_osc = p_osc;
                Properties.Settings.Default.offset_current = c / 10000; // Assuming calibration was done using +-20mA range
                Properties.Settings.Default.Save();

                _ps.SetCalibrationData(p_awg, p_osc, c/10000);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
