
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
    public partial class Set_Font : Form//, IDisposable
    {
        formVoltammogram _vg;

        public Set_Font(formVoltammogram vg)
        {
            InitializeComponent();

            this.Disposed += (sender, args) =>
            {
                //
                // ここに後処理を記述
                //
                Console.WriteLine("Set_Font.Disposed called");
                //MessageBox.Show("test");
                Properties.Settings.Default.Save();
                //if (Double.TryParse(this.numericUpDownTitleSize.Text, out double t) && Double.TryParse(this.numericUpDownLabelSize.Text, out double l))
                //{
                //    Properties.Settings.Default.font_title = t;
                //    Properties.Settings.Default.font_label = l;
                //    Properties.Settings.Default.tick_dir_major = comboBoxTickDirMajor.SelectedIndex;
                //    Properties.Settings.Default.tick_dir_minor = comboBoxTickDirMinor.SelectedIndex;
                //    Properties.Settings.Default.size_weight= int.Parse(textBoxWidth.Text);
                //    Properties.Settings.Default.size_height = int.Parse(textBoxHeight.Text);
                //    Properties.Settings.Default.Save();
                //}
            };

            _vg = vg;

            double t = Properties.Settings.Default.font_title;
            double l = Properties.Settings.Default.font_label;
            this.numericUpDownTitleSize.Value = Convert.ToDecimal(t);
            this.numericUpDownLabelSize.Value = Convert.ToDecimal(l);
            _vg.SetFont(t,l);

            int major = Properties.Settings.Default.tick_dir_major;
            int minor = Properties.Settings.Default.tick_dir_minor;
            this.comboBoxTickDirMajor.SelectedIndex = major;
            this.comboBoxTickDirMinor.SelectedIndex = minor;

            int w = Properties.Settings.Default.size_weight;
            int h = Properties.Settings.Default.size_height;
            this.textBoxWidth.Text = w.ToString();
            this.textBoxHeight.Text = h.ToString();

            numericUpDownLineStyle1.Value = Convert.ToDecimal(Properties.Settings.Default.line_style_1);
            numericUpDownLineStyle2.Value = Convert.ToDecimal(Properties.Settings.Default.line_style_2);
            numericUpDownLineStyle3.Value = Convert.ToDecimal(Properties.Settings.Default.line_style_3);
        }

        //void IDisposable.Dispose()
        //{
        //   Console.WriteLine("void IDisposable.Dispose() called");
        //}

        private void Set_Font_FormClosing(object sender, FormClosingEventArgs e)
        {
            ((CancelEventArgs)e).Cancel = true;
            this.Hide();
        }

        private void Set_Font_FormClosed(object sender, FormClosedEventArgs e)
        {
            Console.WriteLine("Set_Font_FormClosed called");
        }

        private void numericUpDownTitleSize_ValueChanged(object sender, EventArgs e)
        {
            UpdateFontSize();
        }

        private void numericUpDownLabelSize_ValueChanged(object sender, EventArgs e)
        {
            UpdateFontSize();
        }

        private void UpdateFontSize()
        {
            //if (Double.TryParse(this.numericUpDownTitleSize.Text, out double t) && Double.TryParse(this.numericUpDownLabelSize.Text, out double l))
            //{
            //    _vg.SetFont(t, l);
            //}
            _vg.SetFont(Convert.ToDouble(numericUpDownTitleSize.Value), Convert.ToDouble(numericUpDownLabelSize.Value));
        }

        private void comboBoxTickDirMajor_SelectedIndexChanged(object sender, EventArgs e)
        {
            _vg.SetTickDirection(comboBoxTickDirMajor.SelectedIndex, comboBoxTickDirMinor.SelectedIndex);
        }

        private void comboBoxTickDirMinor_SelectedIndexChanged(object sender, EventArgs e)
        {
            _vg.SetTickDirection(comboBoxTickDirMajor.SelectedIndex, comboBoxTickDirMinor.SelectedIndex);
        }

        private void radioButtonPlotAreaAuto_CheckedChanged(object sender, EventArgs e)
        {
            textBoxWidth.Enabled = false;
            textBoxHeight.Enabled = false;

            _vg.SetSizeOfPlotArea(false);
        }

        private void radioButtonPlotAreaManual_CheckedChanged(object sender, EventArgs e)
        {
            textBoxWidth.Enabled = true;
            textBoxHeight.Enabled = true;

            if (Double.TryParse(this.textBoxWidth.Text, out double w) && Double.TryParse(this.textBoxHeight.Text, out double h))
            {
                _vg.SetSizeOfPlotArea(true, w, h);
            }
        }

        private void textBoxWidth_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter || e.KeyChar == (char)Keys.Escape)
            {
                if (Double.TryParse(((TextBox)sender).Text, out double ret) && Double.TryParse(this.textBoxHeight.Text, out double h))
                {
                    //int idx = toolStripComboBoxSeriesRefPotential.Items.Add(Math.Round(ret));
                    //toolStripComboBoxSeriesRefPotential.SelectedIndex = idx;

                    _vg.SetSizeOfPlotArea(false);
                    _vg.SetSizeOfPlotArea(true, ret, h);
                }

                e.Handled = true;
            }
        }

        private void textBoxHeight_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter || e.KeyChar == (char)Keys.Escape)
            {
                if (Double.TryParse(((TextBox)sender).Text, out double ret) && Double.TryParse(this.textBoxWidth.Text, out double w))
                {
                    //int idx = toolStripComboBoxSeriesRefPotential.Items.Add(Math.Round(ret));
                    //toolStripComboBoxSeriesRefPotential.SelectedIndex = idx;

                    _vg.SetSizeOfPlotArea(false);
                    _vg.SetSizeOfPlotArea(true, w, ret);
                }

                e.Handled = true;
            }
        }

        private void numericUpDownLineStyle1_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.line_style_1 = Convert.ToDouble(numericUpDownLineStyle1.Value);

            //if (Double.TryParse(numericUpDownLineStyle1.Text, out double ret))
            //{
            //    Properties.Settings.Default.line_style_1 = ret;
            //}
        }

        private void numericUpDownLineStyle2_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.line_style_2 = Convert.ToDouble(numericUpDownLineStyle2.Value);
            //if (Double.TryParse(numericUpDownLineStyle2.Text, out double ret))
            //{
            //    Properties.Settings.Default.line_style_2 = ret;
            //}
        }

        private void numericUpDownLineStyle3_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.line_style_3 = Convert.ToDouble(numericUpDownLineStyle3.Value);
            //if (Double.TryParse(numericUpDownLineStyle3.Text, out double ret))
            //{
            //    Properties.Settings.Default.line_style_3 = ret;
            //}
        }
    }
}
