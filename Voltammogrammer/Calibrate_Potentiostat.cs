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
            double p = Properties.Settings.Default.offset_potential;
            double c = Properties.Settings.Default.offset_current;
            this.textBoxPotential.Text = p.ToString();
            this.textBoxCurrent.Text = c.ToString();

            _ps.SetCalibrationData(p, c);
        }

        private void Calibrate_Potentiostat_FormClosing(object sender, FormClosingEventArgs e)
        {
            ((CancelEventArgs)e).Cancel = true;
            this.Hide();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            double p, c;

            if(Double.TryParse(this.textBoxPotential.Text, out p) && Double.TryParse(this.textBoxCurrent.Text, out c))
            {
                Properties.Settings.Default.offset_potential = p;
                Properties.Settings.Default.offset_current = c;
                Properties.Settings.Default.Save();

                _ps.SetCalibrationData(p, c);
            }
        }
    }
}
