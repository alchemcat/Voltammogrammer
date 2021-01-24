
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
    public partial class Select_RotationSpeed : Form
    {
        CheckBox[] _checkboxes;
        ComboBox[] _comboboxes;
        Potentiostat _ps;

        public Select_RotationSpeed(Potentiostat ps)
        {
            InitializeComponent();

            _ps = ps;

            _checkboxes = new CheckBox[] { checkBox1, checkBox2, checkBox3, checkBox4, checkBox5, checkBox6 };
            _comboboxes = new ComboBox[] { comboBox1, comboBox2, comboBox3, comboBox4, comboBox5, comboBox6 };

            for(int i = 0; i < _comboboxes.Length; i++)
            {
                _comboboxes[i].SelectedIndex = i; // comboboxes[i].
            }

            SetSequenceOfRDE();
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            string senderName = ((CheckBox)(sender)).Name;

            //ボタンのベース名　長さの取得に使用
            string strBut = "checkBox";

            //Buttonxxのxxを取得して数字に直している
            int index = int.Parse(senderName.Substring(strBut.Length, senderName.Length - strBut.Length));

            if (((CheckBox)sender).Checked)
            {
                for (int i = 1; i < index; i++)
                {
                    _checkboxes[i].Checked = true;
                }
            }
            else
            {
               for (int i = index; i < _checkboxes.Length; i++)
                {
                    _checkboxes[i].Checked = false;
                }
            }
        }

        private void Select_RotationSpeed_FormClosing(object sender, FormClosingEventArgs e)
        {
            ((CancelEventArgs)e).Cancel = true;
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SetSequenceOfRDE();
        }

        private void SetSequenceOfRDE()
        {
            int[] speeds = { };
            int result;

            for(int i = 0; i < _comboboxes.Length; i++)
            {
                if(_checkboxes[i].Checked)
                {
                    //speeds[i] = int.Parse(_checkboxes[i].Text);

                    if (int.TryParse(_comboboxes[i].Text, out result) && (result <= 6400) && (result >= 100) )
                    {
                        Array.Resize(ref speeds, speeds.Length + 1);
                        speeds[i] = result;
                    }
                    else { MessageBox.Show(this, "One of values for the rotation speed is invalid."); return; }
                }
                else
                {
                    break;
                }
            }

            _ps.SetSequenceOfRDE(speeds);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SetSequenceOfRDE();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
