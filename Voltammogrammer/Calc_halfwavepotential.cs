﻿
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

namespace Voltammogrammer
{
    public partial class Calc_halfwavepotential : Form
    {
        TextBox _currentSelection;

        public Calc_halfwavepotential()
        {
            InitializeComponent();

            _currentSelection = textBox1;
        }

        public void SetCursor(double potential)
        {
            _currentSelection.Text = potential.ToString("0.00");

            try
            {
                double p1 = Convert.ToDouble(textBox1.Text);
                double p2 = Convert.ToDouble(textBox2.Text);

                label2.Text = ((p1 + p2) / 2).ToString("0.0") + " mV";
                label4.Text = (Math.Abs((p1 - p2))).ToString("0.0") + " mV";
            }
            catch (FormatException)
            {
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            _currentSelection = textBox1;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            _currentSelection = textBox2;
        }

        private void Calc_halfwavepotential_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            radioButton1.Checked = true;
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            radioButton2.Checked = true;
        }
    }
}
