
/*
    PocketPotentiostat

    Copyright (C) 2019 Yasuo Matsubara

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

namespace Voltammogrammer
{
    public partial class Set_xaxisrange : Form
    {
        formVoltammogram _vg;

        public Set_xaxisrange(formVoltammogram vg)
        {
            InitializeComponent();

            _vg = vg;
        }

        private void Set_xaxisrange_FormClosing(object sender, FormClosingEventArgs e)
        {
            ((CancelEventArgs)e).Cancel = true;
            this.Hide();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            double from, to;

            if(radioButton1.Checked)
            {
                _vg.SetAxisRangeX(true, 0, 0);
            }
            else
            {
                if(Double.TryParse(textBox1.Text, out from) && Double.TryParse(textBox2.Text, out to))
                {
                    _vg.SetAxisRangeX(false, from, to);
                }
            }

            if (radioButtonAutoY.Checked)
            {
                _vg.SetAxisRangeY(true, 0, 0);
            }
            else
            {
                if (Double.TryParse(textBox3.Text, out from) && Double.TryParse(textBox4.Text, out to))
                {
                    _vg.SetAxisRangeY(false, from, to);
                }
            }
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            radioButton2.Checked = true;
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            radioButton2.Checked = true;
        }
    }
}
