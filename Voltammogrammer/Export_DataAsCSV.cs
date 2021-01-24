
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
    public partial class Export_DataAsCSV : Form
    {
        formVoltammogram _vg;

        public Export_DataAsCSV(formVoltammogram vg)
        {
            InitializeComponent();

            _vg = vg;
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            //_vg.ExportData(checkBoxDataReduction.Checked, checkBoxForExcel.Checked);
        }

        private void Export_DataAsCSV_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
