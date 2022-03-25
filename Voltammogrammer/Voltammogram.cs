
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
using System.IO;
//using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Xml;
using System.Xml.Serialization;

using Microsoft.WindowsAPICodePack;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs.Controls;


namespace Voltammogrammer
{
    public partial class formVoltammogram : Form
    {
        int _currentIndex = -1;
        int _indexRecording = -1;
        Calc_halfwavepotential _calc_halfwavepotential = new Calc_halfwavepotential();
        Show_information _show_information = new Show_information();
        Set_xaxisrange _set_xaxisrange;
        Set_Font _set_font;
        //Export_DataAsCSV _export_data;
        //bool _is_calc_halfwavepotential = false;

        double _currentMinX = double.NaN;
        double _currentMaxX = double.NaN;
        double _currentMinY = double.NaN;
        double _currentMaxY = double.NaN;

        List<string> _series = new List<string>();
        bool _root = false;
        bool _standalone = false;

        bool _mode_coulomb_counting = false;
        double _coulomb_counting_time, _coulomb_counting_current;
        static formVoltammogram _chronoamperogram;// = new formVoltammogram(true, true);

        string _filename_saved = "(not saved yet)";

        // 
        // AddDataToCurrentSeriesにおいてAddXY時のパラメータを増やす時には、YValuesPerPointも増やす必要がある
        //
        public enum typeAxisY : int
        {
            Current_in_uA = 0,
            Current_in_mA = 1,
            Coulomb_in_mC = 2,
            Coulomb_in_C = 3,
            Potential_in_mV = 4,
            Current_in_nA = 5,
            ImZ_in_ohm = 6,
            C2_in_MS_plot = 7,
            Z_in_ohm = 8,
        }
        typeAxisY _selectedAxisY = typeAxisY.Current_in_uA;
        double[] _scaleAxisY = { 1000, 1, 1, 0.001, 1, 1000000, 1 };

        public enum typeAxisX : int
        {
            Potential_in_mV = 0,
            Potential_in_V = 1,
            Time_in_sec = 2,
            Time_in_hour = 3,
            ReZ_in_ohm = 4,
            Freq = 5,
            NEED_TO_RENEW = 6,
        }
        typeAxisX _selectedAxisX = typeAxisX.Potential_in_mV;
        double[] _scaleAxisX = { 1000, 1, 1, 0.0002777778, 1 };

        const int POTENTIAL = 1, CURRENT = 2, TIME = 3, COULOMB = 4, RE_Z = 5, IM_Z = 6, FREQ = 7; // POTENTIAL [mV], CURRENT [uA], TIME [s], COULOMB [mC], RE_Z [ohm], IM_Z [ohm], FREQ [Hz]

        public formVoltammogram()
        {
            //_root = false;
            InitializeComponent();
            _initialize();
        }

        public formVoltammogram(bool root, bool mode_coulomb_counting = false)
        {
            _root = root; _mode_coulomb_counting = mode_coulomb_counting;
            InitializeComponent();
            _initialize();

            //if (_root && !_mode_coulomb_counting)
            //{
            //    _chronoamperogram = new formVoltammogram(true, true);
            //}

            //if (mode_coulomb_counting)
            //{
            //    this.Text = "Actual Chronocoulogram";
            //}
        }

        public formVoltammogram(string[] files)
        {
            //_root = false;
            _standalone = true;
            InitializeComponent();
            _initialize();

            for (int i = 0; i < files.Count(); i++)
            {
                if (i == 0)
                {
                    LoadFile(files[i], false);
                }
                else
                {
                    LoadFile(files[i], true);
                }
            }
            //LoadFile(files[0], false);
        }

        private void _initialize()
        {
            //chartVoltammogram.Palette.
            //toolStripComboBoxSeriesColor.Items.AddRange
            //System.Windows.Forms.DataVisualization.Charting.ChartColorPalette ccp = chartVoltammogram.Palette;
            //TypeCode t = ccp.GetTypeCode();

            //foreach(TypeCode i in ccp)
            //{

            //}

            //toolStripComboBoxSeriesColor.Items.AddRange(ccp);

            toolStripComboBoxSeriesColor.ComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            toolStripComboBoxSeriesColor.ComboBox.DrawItem += new DrawItemEventHandler(this.cmbColor_DrawItem);

            //toolStripComboBoxSeriesColor.Items.AddRange(Enum.GetNames(typeof(KnownColor)));
            toolStripComboBoxSeriesColor.Items.AddRange(ChartColorPallets.Custom.ToArray());

            //toolStripComboBoxSeriesLineStyle.Items.AddRange(System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.);


            //toolStripComboBoxSeriesLineStyle.Items.AddRange((object[])Enum.GetValues(typeof(System.Windows.Forms.DataVisualization.Charting.ChartDashStyle)));

            object[] test1 = Enum.GetValues(typeof(System.Windows.Forms.DataVisualization.Charting.ChartDashStyle)).Cast<object>().ToArray();
            toolStripComboBoxSeriesLineStyle.Items.AddRange(test1);

            int[] test2 = new int[] { 1, 2, 3, 4, 5 };
            toolStripComboBoxSeriesLineWidth.Items.AddRange(test2.Cast<object>().ToArray());

            int[] test3 = new int[] { };
            toolStripComboBoxSeriesRefPotential.Items.AddRange(test3.Cast<object>().ToArray());

            chartVoltammogram.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.None;
            chartVoltammogram.PaletteCustomColors = (Color[])ChartColorPallets.Custom.ToArray(typeof(Color));

            chartVoltammogram.ChartAreas[0].Tag = _series;

            toolStripComboBoxAxisY.SelectedIndex = 0;// (int)typeAxisY.Current_in_uA;
            toolStripComboBoxAxisX.SelectedIndex = 0;// (int)typeAxisX.Potential_in_mV;

            if (_mode_coulomb_counting)
            {
                toolStrip3.Enabled = false;
                toolStripComboBoxSeriesRefPotential.Enabled = false;
                toolStripComboSeriesShiftY.Enabled = false;
                toolStripDropDownButton1.Enabled = false;
                toolStripButtonSave.Enabled = false;
                toolStripButtonNewWindow.Enabled = false;
                toolStripMenuItemLoadAsVoltammogram.Enabled = false;

                chartVoltammogram.ChartAreas[0].AxisX.Title = "Time / s";
                chartVoltammogram.ChartAreas[0].AxisY.Title = "Charge / mC";
                //chartVoltammogram.ChartAreas[0].AxisX.Maximum = 100;// Double.NaN;
                //chartVoltammogram.ChartAreas[0].AxisX.Minimum = 0;// Double.NaN;
            }

            _set_xaxisrange = new Set_xaxisrange(this);
            _set_font = new Set_Font(this);
            //_export_data = new Export_DataAsCSV(this);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern int GetMenuItemCount(IntPtr hMenu);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool DrawMenuBar(IntPtr hWnd);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_root)
            {
                const Int32 MF_BYPOSITION = 0x400;
                const Int32 MF_REMOVE = 0x1000;

                IntPtr menu = GetSystemMenu(this.Handle, false);
                int menuCount = GetMenuItemCount(menu);
                if (menuCount > 1)
                {
                    //メニューの「閉じる」とセパレータを削除
                    RemoveMenu(menu, (uint)(menuCount - 1), MF_BYPOSITION | MF_REMOVE);
                    RemoveMenu(menu, (uint)(menuCount - 2), MF_BYPOSITION | MF_REMOVE);
                    DrawMenuBar(this.Handle);
                }
            }
        }

        private void cmbColor_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            if (e.Index == -1)
            {
                return;
            }

            ComboBox cmbColor = (ComboBox)sender;

            int itemString = ((Color)cmbColor.Items[e.Index]).ToArgb();
            e.DrawBackground();

            //e.Graphics.DrawString("■", e.Font, new SolidBrush(Color.FromArgb(itemString)),
            //     new RectangleF(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height));
            //e.Graphics.DrawString("    " + itemString, e.Font, new SolidBrush(e.ForeColor),
            //     new RectangleF(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height));
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(itemString)),
                 new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 1, e.Bounds.Width - 4, e.Bounds.Height - 2));
            e.DrawFocusRectangle();
        }

        //
        // Event Handler for Form
        //

        private void formVoltammogram_Load(object sender, EventArgs e)
        {
            //chartVoltammogram.Series[0].Points.AddXY(0, 0);
            //chartVoltammogram.Update();

            //toolStripContainer1.SuspendLayout();
            //toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            //toolStrip1.SuspendLayout();

            toolStripContainer1.TopToolStripPanel.Join(toolStrip2, 0, 0);
            toolStripContainer1.TopToolStripPanel.Join(toolStrip3, toolStrip2.Width+1, 0);
            toolStripContainer1.BottomToolStripPanel.Join(statusStrip1, toolStripContainer1.BottomToolStripPanel.Controls.Count);

            //toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            //toolStripContainer1.BottomToolStripPanel.PerformLayout();
            //toolStripContainer1.ResumeLayout(false);
            //toolStripContainer1.PerformLayout();
            //toolStrip1.ResumeLayout(false);
            //toolStrip1.PerformLayout();
        }

        private void formVoltammogram_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case 'c': //Console.WriteLine("c pressed...");

                    if (chartVoltammogram.ChartAreas[0].CursorX.LineDashStyle == System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.NotSet)
                    {
                        chartVoltammogram.ChartAreas[0].CursorX.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
                        chartVoltammogram.ChartAreas[0].CursorY.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
                    }
                    else
                    {
                        chartVoltammogram.ChartAreas[0].CursorX.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.NotSet;
                        chartVoltammogram.ChartAreas[0].CursorY.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.NotSet;
                    }
                    break;

                default:
                    break;
            }
        }

        private void formVoltammogram_FormClosing(object sender, FormClosingEventArgs e)
        {
            //e.Cancel = true;
            //this.Hide();

            if (_standalone)
            {
                DialogResult dr = MessageBox.Show(this, "Are you OK to close all windows showing voltammograms?", "Voltammogrammer 2", MessageBoxButtons.OKCancel);

                if (dr == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void formVoltammogram_FormClosed(object sender, FormClosedEventArgs e)
        {
            openFileDialog1.Dispose();
            //_set_font.Close();
        }

        //
        // Methods
        //

        public void LoadFile(string file_path, bool doLoadNewWindow = true)
        {
            //string file_path = openFileDialog1.FileNames[i];
            string dir = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(file_path));
            string item = dir + "/" + System.IO.Path.GetFileName(file_path);
            string ext = System.IO.Path.GetExtension(file_path);

            if (ext == ".mpt")
            {
                LoadSingleVoltammogram(item, file_path);
            }
            else if (ext == ".volta" || ext == ".voltax")
            {
                bool compressed = (ext == ".voltax") ? true : false;

                if (chartVoltammogram.Series.Count != 0 && doLoadNewWindow)
                {
                    formVoltammogram v = new formVoltammogram();
                    v.Show();
                    v.LoadSetOfSeries(file_path, compressed);
                }
                else
                {
                    LoadSetOfSeries(file_path, compressed);
                }
            }
            else if (ext == ".txt")
            {
                LoadSingleVoltammogram_Hokuto(item, file_path);
            }
            else
            {

            }
        }

        public void LoadSingleVoltammogram(string name, string file)
        {
            //System.Text.RegularExpressions.Regex r =
            //    new System.Text.RegularExpressions.Regex
            //    (
            //        @"^\s*([+-]?(?:\d+\.?\d*|\.\d+)(?:[eE][+-]?\d+)?)\s+([+-]?(?:\d+\.?\d*|\.\d+)(?:[eE][+-]?\d+)?)\s+([+-]?(?:\d+\.?\d*|\.\d+)(?:[eE][+-]?\d+)?)\s*$",
            //        System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline
            //    );
            System.Text.RegularExpressions.Regex r =
                new System.Text.RegularExpressions.Regex
                (
                    @"^\s*(?:([+-]?(?:\d+\.?\d*|\.\d+)(?:[eE][+-]?\d+)?)\s*)+$",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline
                );
            System.Text.RegularExpressions.Regex r2 =
                new System.Text.RegularExpressions.Regex
                (
                    @"^\s*Nb header lines : (\d+)\s*$",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline
                );
            //System.Text.RegularExpressions.Regex r0 =
            //    new System.Text.RegularExpressions.Regex
            //    (
            //        @"^\s*(?:(?:(Ewe/V)|(I/mA)|(time/s))\s*)*$",
            //        System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline
            //    );
            System.Text.RegularExpressions.Regex r0 =
                new System.Text.RegularExpressions.Regex
                (
                    @"^\s*(?:(Ewe/V|I/mA|[^\t]+)\t*)+$",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline
                );

            System.Windows.Forms.DataVisualization.Charting.Series series = AddNewSeries(!_mode_coulomb_counting, name, name, false);

            System.IO.FileStream streamFile = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read, FileShare.ReadWrite);
            TextReader reader = new StreamReader(streamFile);

            string line; int line_number = 1; int idxPotential = -1, idxCurrent = -1, idxTime = -1, idxReZ = -1, idxImZ = -1, idxFreq = -1;
            int line_header = 73;
            double potential, current, time, ReZ, ImZ, freq, p = Double.NaN, t = Double.NaN, c = Double.NaN, q = Double.NaN;
            while ((line = reader.ReadLine()) != null)
            {
                switch (line_number)
                {
                    case 1: // EC-Lab ASCII FILE
                        break;

                    case 2: // Nb header lines : ?
                        System.Text.RegularExpressions.MatchCollection mcs0 = r2.Matches(line);
                        if (mcs0.Count == 1)
                        {
                            if (mcs0[0].Groups.Count == 2)
                            {
                                line_header = int.Parse(mcs0[0].Groups[1].Value);
                            }
                        }
                        break;

                    default:
                        if (line_number == line_header)
                        {
                            System.Text.RegularExpressions.MatchCollection mcs2 = r0.Matches(line);
                            if (mcs2.Count == 1)
                            {
                                //System.Text.RegularExpressions.CaptureCollection cc0 = mcs2[0].Groups[0].Captures;
                                //System.Text.RegularExpressions.CaptureCollection cc1 = mcs2[0].Groups[1].Captures;
                                //for (int i = 1; i < mcs2[0].Groups.Count; i++)
                                //{
                                //    if(mcs2[0].Groups[i].Value == "Ewe/V")
                                //    {
                                //        idxPotential = i;
                                //    }
                                //    else if (mcs2[0].Groups[i].Value == "I/mA")
                                //    {
                                //        idxCurrent = i;
                                //    }
                                //    else if (mcs2[0].Groups[i].Value == "<I>/mA")
                                //    {
                                //        idxCurrent = i;
                                //    }
                                //    else if (mcs2[0].Groups[i].Value == "time/s")
                                //    {
                                //        idxTime = i;
                                //    }
                                //}
                                System.Text.RegularExpressions.CaptureCollection cc1 = mcs2[0].Groups[1].Captures;
                                for (int i = 0; i < cc1.Count; i++)
                                {
                                    if (cc1[i].Value == "Ewe/V")
                                    {
                                        idxPotential = i;
                                    }
                                    else if (cc1[i].Value == "I/mA")
                                    {
                                        idxCurrent = i;
                                    }
                                    else if (cc1[i].Value == "<I>/mA")
                                    {
                                        idxCurrent = i;
                                    }
                                    else if (cc1[i].Value == "time/s")
                                    {
                                        idxTime = i;
                                    }
                                    else if (cc1[i].Value == "-Im(Z)/Ohm")
                                    {
                                        idxImZ = i;
                                    }
                                    else if (cc1[i].Value == "Re(Z)/Ohm")
                                    {
                                        idxReZ = i;
                                    }
                                    else if (cc1[i].Value == "freq/Hz")
                                    {
                                        idxFreq = i;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (line_number > line_header)
                            {
                                System.Text.RegularExpressions.MatchCollection mcs = r.Matches(line);
                                if (mcs.Count == 1)
                                {
                                    //if (mcs[0].Groups.Count == 4)
                                    //{
                                    //    if (Double.TryParse(mcs[0].Groups[1].Value, out potential) && Double.TryParse(mcs[0].Groups[2].Value, out current))
                                    //    {
                                    //        if (Double.IsNaN(p) || Math.Abs(potential - p) > 1)
                                    //        {
                                    //            if (true)
                                    //            {
                                    //                AddDataToCurrentSeries(potential, current);
                                    //                p = potential;
                                    //            }
                                    //        }
                                    //        else
                                    //        {
                                    //        }
                                    //    }
                                    //    else
                                    //    {
                                    //        continue;
                                    //    }
                                    //}

                                    System.Text.RegularExpressions.CaptureCollection cc = mcs[0].Groups[1].Captures;
                                    //if (mcs[0].Groups.Count == 4)
                                    //{
                                    if (true || !_mode_coulomb_counting)
                                    {
                                        if(idxPotential != -1 && idxCurrent != -1 && idxTime != -1)
                                        {
                                            if (Double.TryParse(cc[idxTime].Value, out time) && Double.TryParse(cc[idxPotential].Value, out potential) && Double.TryParse(cc[idxCurrent].Value, out current))
                                            {
                                                potential *= 1000.0; current *= 1000.0;

                                                if (true || Double.IsNaN(p) || Math.Abs(potential - p) >= 0.1)
                                                {
                                                    if (true)
                                                    {
                                                        AddDataToCurrentSeries(series, !_mode_coulomb_counting, potential, typeAxisX.Potential_in_mV, current, typeAxisY.Current_in_uA, time);
                                                        p = potential;
                                                    }
                                                }
                                                else
                                                {
                                                }
                                            }
                                        }
                                        else if(idxReZ != -1 && idxImZ != -1)
                                        {
                                            if (Double.TryParse(cc[idxReZ].Value, out ReZ) && Double.TryParse(cc[idxImZ].Value, out ImZ) && Double.TryParse(cc[idxFreq].Value, out freq) && Double.TryParse(cc[idxPotential].Value, out potential))
                                            {
                                                AddDataToCurrentSeries(series, !_mode_coulomb_counting, potential, typeAxisX.Potential_in_mV, 0, typeAxisY.Current_in_uA, 0, ReZ, ImZ, freq);
                                            }
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        if (Double.TryParse(cc[idxTime].Value, out time) && Double.TryParse(cc[idxPotential].Value, out potential) && Double.TryParse(cc[idxCurrent].Value, out current))
                                        {
                                            potential *= 1000.0; current *= 1000.0;
                                            //Console.WriteLine("{0},{1}", time, current);
                                            //if (!Double.IsNaN(t))
                                            //{
                                            //    q += (c)*(time-t)/1000;
                                            //    AddDataToCurrentSeries(_mode_coulomb_counting, time, typeAxisX.Potential_in_mV, q, typeAxisY.Current_in_uA);
                                            //}
                                            //else
                                            //{
                                            //    q = 0.0;
                                            //}
                                            //t = time; c = current;
                                            AddDataToCurrentSeries(series, !_mode_coulomb_counting, time, typeAxisX.Potential_in_mV, current, typeAxisY.Current_in_uA, time);
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                    //}
                                }
                            }
                            else
                            {
                                if(line_number == 3)
                                {
                                    XMLDataHolder dh = new XMLDataHolder();
                                    dh.LoadDataFromString(line);
                                    AddExpConditions(series, dh);
                                }
                            }
                        }
                        break;
                }

                line_number++;
            }

            reader.Close();
            streamFile.Close();
        }

        public void LoadSingleVoltammogram_Hokuto(string name, string file)
        {
            ////System.Text.RegularExpressions.Regex r =
            ////    new System.Text.RegularExpressions.Regex
            ////    (
            ////        @"^\s*([+-]?(?:\d+\.?\d*|\.\d+)(?:[eE][+-]?\d+)?)\s+([+-]?(?:\d+\.?\d*|\.\d+)(?:[eE][+-]?\d+)?)\s+([+-]?(?:\d+\.?\d*|\.\d+)(?:[eE][+-]?\d+)?)\s*$",
            ////        System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline
            ////    );
            //System.Text.RegularExpressions.Regex r =
            //    new System.Text.RegularExpressions.Regex
            //    (
            //        @"^\s*(?:([+-]?(?:\d+\.?\d*|\.\d+)(?:[eE][+-]?\d+)?)\s*)+$",
            //        System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline
            //    );
            //System.Text.RegularExpressions.Regex r2 =
            //    new System.Text.RegularExpressions.Regex
            //    (
            //        @"^\s*Nb header lines : (\d+)\s*$",
            //        System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline
            //    );
            ////System.Text.RegularExpressions.Regex r0 =
            ////    new System.Text.RegularExpressions.Regex
            ////    (
            ////        @"^\s*(?:(?:(Ewe/V)|(I/mA)|(time/s))\s*)*$",
            ////        System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline
            ////    );
            //System.Text.RegularExpressions.Regex r0 =
            //    new System.Text.RegularExpressions.Regex
            //    (
            //        @"^\s*(?:(Ewe/V|I/mA|[^\t]+)\t*)+$",
            //        System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline
            //    );






            System.Text.RegularExpressions.Regex r1 =
                new System.Text.RegularExpressions.Regex
                (
                    @"^\[PHASE\], 3",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline
                );
            System.Text.RegularExpressions.Regex r2 =
                new System.Text.RegularExpressions.Regex
                (
                    @"^\s*,\s*([+-]?(?:\d+\.?\d*|\.\d+)(?:[eE][+-]?\d+)?),\s*([+-]?(?:\d+\.?\d*|\.\d+)(?:[eE][+-]?\d+)?),\s*([+-]?(?:\d+\.?\d*|\.\d+)(?:[eE][+-]?\d+)?),",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline
                );



            System.Windows.Forms.DataVisualization.Charting.Series series = AddNewSeries(!_mode_coulomb_counting, name, name, false);

            System.IO.FileStream streamFile = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read, FileShare.ReadWrite);
            TextReader reader = new StreamReader(streamFile);

            string line; int line_number = 1; int idxPotential = 0, idxCurrent = 0, idxTime = 0; int line_header = 2000;
            double potential, current, time, p = Double.NaN, t = Double.NaN, c = Double.NaN, q = Double.NaN;






            while ((line = reader.ReadLine()) != null)
            {
                if (line_number > (line_header+1))
                {
                    System.Text.RegularExpressions.MatchCollection mcs2 = r2.Matches(line);
                    if (mcs2.Count == 1)
                    {
                        System.Text.RegularExpressions.CaptureCollection cc2a = mcs2[0].Groups[1].Captures;
                        System.Text.RegularExpressions.CaptureCollection cc2b = mcs2[0].Groups[2].Captures;
                        System.Text.RegularExpressions.CaptureCollection cc2c = mcs2[0].Groups[3].Captures;

                        if (Double.TryParse(cc2a[0].Value, out time) && Double.TryParse(cc2b[0].Value, out potential) && Double.TryParse(cc2c[0].Value, out current))
                        {
                            potential *= 1000.0; current *= 1000.0;

                            if (true || Double.IsNaN(p) || Math.Abs(potential - p) >= 0.1)
                            {
                                if (true)
                                {
                                    AddDataToCurrentSeries(series, !_mode_coulomb_counting, potential, typeAxisX.Potential_in_mV, current, typeAxisY.Current_in_uA, time);
                                    p = potential;
                                }
                            }
                            else
                            {
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    System.Text.RegularExpressions.MatchCollection mcs1 = r1.Matches(line);
                    if (mcs1.Count == 1)
                    {
                        line_header = line_number;
                    }
                }

                line_number++;
            }

            reader.Close();
            streamFile.Close();
        }

        public void LoadSetOfSeries(string filename, bool compressed)
        {
            //FileStream streamFile = new FileStream(filename, FileMode.Open);
            System.IO.FileStream streamFile = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read, FileShare.ReadWrite);

            System.IO.Stream gzipStrm;
            if (compressed)
            {
                //圧縮解除モードのGZipStreamを作成する
                //System.IO.Compression.GZipStream gzipStrm = new System.IO.Compression.GZipStream(streamFile, System.IO.Compression.CompressionMode.Decompress);
                gzipStrm = new System.IO.Compression.GZipStream(streamFile, System.IO.Compression.CompressionMode.Decompress);
            }
            else
            {
                //System.IO.FileStream gzipStrm = streamFile;
                gzipStrm = streamFile;
            }


            bool use_serializer = false;
            if(use_serializer)
            {
                System.Windows.Forms.DataVisualization.Charting.ChartSerializer cs = chartVoltammogram.Serializer;
                cs.IsUnknownAttributeIgnored = true;
                cs.Load(streamFile);
            }
            else
            {
                //System.Text.RegularExpressions.Regex r =
                //    new System.Text.RegularExpressions.Regex
                //    (
                //        @"^\s*(?:([+-]?(?:\d+\.?\d*|\.\d+)(?:[eE][+-]?\d+)?),?\s*)+$",
                //        System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline
                //    );

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreWhitespace = true;
                settings.IgnoreProcessingInstructions = true;
                settings.IgnoreComments = true;

                System.Xml.XmlReader reader = System.Xml.XmlReader.Create(gzipStrm, settings);

                string lines;
                string line; string[] values;
                double x, y, y1, y2, y3, y4, y5, y6, y7;
                string name, original_name;
                string color, width, dash;

                System.Windows.Forms.DataVisualization.Charting.ChartColorPalette ttt;

                //StringBuilder sb; sb.
                //for (; ; ) // reader.Read()
                //{
                    //reader.ReadStartElement("Chart"); //Console.WriteLine(streamFile.Position);
                    //reader.ReadStartElement("Series"); //Console.WriteLine(streamFile.Position);
                    //reader.Read
                    while (reader.ReadToFollowing("Data"))
                    {
                        name = reader.GetAttribute("name");
                        original_name = reader.GetAttribute("base-name"); if (original_name == null) original_name = name;
                        System.Windows.Forms.DataVisualization.Charting.Series series = AddNewSeries(true, name, original_name, false);


                        color = reader.GetAttribute("color"); if (color == null) color = "Black";
                        width = reader.GetAttribute("width"); if (width == null) width = "1";
                        dash = reader.GetAttribute("dash");   if (dash == null) dash = "5";

                        series.Color = System.Drawing.Color.FromName(color);
                        series.BorderWidth = Int32.Parse(width);
                        series.BorderDashStyle = (System.Windows.Forms.DataVisualization.Charting.ChartDashStyle)Int32.Parse(dash);





                        lines = reader.ReadString();

                        Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(lines));
                        StreamReader readerStream = new StreamReader(stream);

                        while ((line = readerStream.ReadLine()) != null)
                        {
                            values = line.Split(new char[] { ',' });

                            if (
                                values.Length == 6
                                && Double.TryParse(values[0], out x)
                                && Double.TryParse(values[1], out y)
                                && Double.TryParse(values[2], out y1)
                                && Double.TryParse(values[3], out y2)
                                && Double.TryParse(values[4], out y3)
                                && Double.TryParse(values[5], out y4)
                            )
                            {
                                series.Points.AddXY(
                                    x,
                                    y,
                                    y1, //potential,
                                    y2, //current,
                                    y3, //time,
                                    y4  //coulomb
                                );
                            }
                            else if (
                                values.Length == 9
                                && Double.TryParse(values[0], out x)
                                && Double.TryParse(values[1], out y)
                                && Double.TryParse(values[2], out y1)
                                && Double.TryParse(values[3], out y2)
                                && Double.TryParse(values[4], out y3)
                                && Double.TryParse(values[5], out y4)
                                && Double.TryParse(values[6], out y5)
                                && Double.TryParse(values[7], out y6)
                                && Double.TryParse(values[8], out y7)
                            )
                            {
                                series.Points.AddXY(
                                    x,
                                    y,
                                    y1, //potential,
                                    y2, //current,
                                    y3, //time,
                                    y4, //coulomb
                                    y5, y6, y7 // ReZ, ImZ, Freq
                                );

                            }
                            else
                            {
                                continue;
                            }


                            //System.Text.RegularExpressions.MatchCollection mcs = r.Matches(line);
                            //if (mcs.Count == 1)
                            //{
                            //    System.Text.RegularExpressions.CaptureCollection cc = mcs[0].Groups[1].Captures;
                            //    if (mcs[0].Groups.Count == 6)
                            //    {
                            //    }

                            //    if (
                            //           Double.TryParse(cc[0].Value, out x)
                            //        && Double.TryParse(cc[1].Value, out y)
                            //        && Double.TryParse(cc[2].Value, out y1)
                            //        && Double.TryParse(cc[3].Value, out y2)
                            //        && Double.TryParse(cc[4].Value, out y3)
                            //        && Double.TryParse(cc[5].Value, out y4)
                            //    )
                            //    {
                            //    }
                            //    else
                            //    {
                            //        continue;
                            //    }
                            //}
                        }

                        readerStream.Close();
                        stream.Close();
                    }

                    //string name = "rrr";
                    //name = reader.GetAttribute("name");
                    //reader.ReadStartElement("Data");// Console.WriteLine(streamFile.Position);
                    //reader.ReadToFollowing("Data");

                    //lines = reader.ReadString();
                    //    reader.

                    //if (reader.ReadToFollowing("Data"))
                    //{
                    //    name = reader.GetAttribute("name");
                    //    lines = reader.ReadString();
                    //}

                    //reader.ReadToNextSibling()

                    //reader.ReadEndElement();
                    //reader.ReadEndElement();

                    reader.Close();

                    //reader.ReadEndElement();
                    //
                    // この時点でカーソルはnameを指している.
                    //
                    //Console.WriteLine(reader.GetAttribute("nickname"));
                    //reader.ReadStartElement("name");
                    //Console.WriteLine(reader.ReadString());
                    //reader.ReadEndElement();

                //    break;
                //}
            }

            //gzipStrm.Position = 0;
            streamFile.Position = 0;

            ////System.IO.Compression.GZipStream gzipStrm2 = new System.IO.Compression.GZipStream(streamFile, System.IO.Compression.CompressionMode.Decompress);
            //System.IO.FileStream gzipStrm2 = streamFile;

            System.IO.Stream gzipStrm2;
            if (compressed)
            {
                //圧縮解除モードのGZipStreamを作成する
                //System.IO.Compression.GZipStream gzipStrm = new System.IO.Compression.GZipStream(streamFile, System.IO.Compression.CompressionMode.Decompress);
                gzipStrm2 = new System.IO.Compression.GZipStream(streamFile, System.IO.Compression.CompressionMode.Decompress);
            }
            else
            {
                //System.IO.FileStream gzipStrm = streamFile;
                gzipStrm2 = streamFile;
            }


            XmlDocument doc = new XmlDocument();
            doc.Load(gzipStrm2);

            toolStripComboBoxAxisX.SelectedIndex = (dynamic)DeserializeParameter(doc, toolStripComboBoxAxisX.SelectedIndex.GetType(), "toolStripComboBoxAxisX.SelectedIndex");
            toolStripComboBoxRef.SelectedIndex = (dynamic)DeserializeParameter(doc, toolStripComboBoxRef.SelectedIndex.GetType(), "toolStripComboBoxRef.SelectedIndex");
            toolStripComboBoxAxisY.SelectedIndex = (dynamic)DeserializeParameter(doc, toolStripComboBoxAxisY.SelectedIndex.GetType(), "toolStripComboBoxAxisY.SelectedIndex");

            //_selectedAxisX = (dynamic)DeserializeParameter(doc, _selectedAxisX.GetType(), nameof(_selectedAxisX));
            //switch(_selectedAxisX)
            //{
            //    case typeAxisX.Potential_in_mV: toolStripComboBoxAxisX.SelectedIndex = 0; break;
            //    case typeAxisX.Potential_in_V: toolStripComboBoxAxisX.SelectedIndex = 1; break;
            //    default: break;
            //}
            //_selectedAxisY = (dynamic)DeserializeParameter(doc, _selectedAxisY.GetType(), nameof(_selectedAxisY));
            //switch(_selectedAxisY)
            //{
            //    case typeAxisY.Current_in_uA: toolStripComboBoxAxisY.SelectedIndex = 0; break;
            //    case typeAxisY.Current_in_mA: toolStripComboBoxAxisY.SelectedIndex = 1; break;
            //    default: break;
            //}

            XmlNodeList propertyVoltammograms = doc.SelectNodes("//XMLDataHolder");

            // _seriesでデータ系列の名前を読み出していたが、既定の[Scan]ではなく、常に[Scan 1]を使用するようにしたので、
            // _seriesをDeserializeParameterするのではなく、データ本体の読み出し時に、_seriesを構築するようにする。
            // と思ったら、既にAddNewSeriesで構築していた...。以下2行をコメントアウト(2021/1/2)。
            //_series = (dynamic)DeserializeParameter(doc, _series.GetType(), nameof(_series));
            //_currentIndex = _series.Count() - 1;

            //List<int> shifts = new List<int>();
            //shifts = (dynamic)DeserializeParameter(doc, shifts.GetType(), nameof(shifts));

            System.Windows.Forms.DataVisualization.Charting.SeriesCollection sc = chartVoltammogram.Series;
            ToolStripItemCollection tsic = toolStripDropDownButton2.DropDownItems;
            for (int i = 0; i < sc.Count(); i++)
            {
                //chartVoltammogram.Series[i].Tag = shifts[i];
                chartVoltammogram.Series[i].Tag = new XMLDataHolder(propertyVoltammograms[i]);

                //ToolStripItem tsi = tsic.Add(_series[i]);
                //tsi.Click += new System.EventHandler(this.toolStripMenuItem_Series_Click);

                //toolStripComboBoxSeries.Items.Add(_series[i]);
            }

            //toolStripComboBoxSeries.SelectedIndex = _currentIndex;
            updateSelectionOfSeries();

            gzipStrm.Close();
            gzipStrm2.Close();

            string dir = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(filename));
            string item = dir + "/" + System.IO.Path.GetFileName(filename);
            this.Text = item;

            ChangeAxisX(true);
            ChangeAxisY(true); //呼ばないほうがいい？
        }

        public void EndRecording()
        {
            _indexRecording = -1;
        }

        public System.Windows.Forms.DataVisualization.Charting.Series AddNewSeries(bool mode_voltammmetry, string name, string base_name, bool mode_recording)
        {
            //chronoamperogramモードなのに、現インスタンスが_mode_coulomb_countingでない場合には、_chronoamperogramのそれを呼び出し
            if (false && !mode_voltammmetry && !_mode_coulomb_counting)
            {
                return _chronoamperogram.AddNewSeries(false, name, base_name, mode_recording);
            }
            if (_mode_coulomb_counting) this.Show();

            string nameActual = name;

            if(name == base_name)
            {
                if (name == "Scan") { nameActual = name + " " + 1.ToString(); }

                //try
                //{
                int i = 1;
                while (_series.IndexOf(nameActual) != -1)
                {
                    nameActual = name + " " + (_currentIndex + (++i)).ToString();
                }
                //int i = 1;
                //while (chartVoltammogram.Series.IndexOf(nameActual) != -1)
                //{
                //    nameActual = name + " " + (_currentIndex + (++i)).ToString();
                //}
                _series.Add(nameActual);
            }
            else
            {
                _series.Add(base_name);
                nameActual = name;
            }

            _currentIndex = _series.Count() - 1;
            if (mode_recording) _indexRecording = _currentIndex;
            //}
            //catch (ArgumentException)
            //{
            //    chartVoltammogram.Series.Add(name + " " + (_currentIndex+1).ToString());
            //}

            ToolStripItemCollection tsic = toolStripDropDownButton2.DropDownItems;
            ToolStripItem tsi = tsic.Add(nameActual);
            tsi.Click += new System.EventHandler(this.toolStripMenuItem_Series_Click);

            System.Windows.Forms.DataVisualization.Charting.Series series = chartVoltammogram.Series.Add(nameActual);
            chartVoltammogram.Series[_currentIndex].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chartVoltammogram.Series[_currentIndex].Color = chartVoltammogram.PaletteCustomColors[(_currentIndex % 10)];
            chartVoltammogram.Series[_currentIndex].BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            chartVoltammogram.Series[_currentIndex].BorderWidth = 1;
            chartVoltammogram.Series[_currentIndex].YValuesPerPoint = 5+3;
            chartVoltammogram.Series[_currentIndex].XAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Primary;
            chartVoltammogram.Series[_currentIndex].YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Primary;
            XMLDataHolder dh = new XMLDataHolder();
            dh.SetDatum("reference", "0");
            dh.SetDatumAttribute("reference", "unit", "mV");
            dh.SetDatum("shiftY", "0");
            dh.SetDatumAttribute("shiftY", "unit", "uA");
            dh.SetDatum("scaleY", "1.0");
            dh.SetDatumAttribute("scaleY", "unit", "none");
            chartVoltammogram.Series[_currentIndex].Tag = dh;

            chartVoltammogram.Series[_currentIndex].MarkerStep = 10;
            chartVoltammogram.Series[_currentIndex].MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.None;
            chartVoltammogram.Series[_currentIndex].MarkerSize = 2;

            int idx = toolStripComboBoxSeries.Items.Add(nameActual);
            toolStripComboBoxSeries.SelectedIndex = idx;

            this.Text = _filename_saved + "*" + " - Actual Voltammogram";

            return series;
        }

        public void AddExpConditions(System.Windows.Forms.DataVisualization.Charting.Series series, XMLDataHolder exp_conditions)
        {
            XMLDataHolder prop = (XMLDataHolder)series.Tag;

            XmlNode ec = exp_conditions.GetData();

           // XmlNode root = prop.GetData().ImportNode(node, true);

            prop.AddData(ec);

            int idx = toolStripComboBoxSeries.SelectedIndex;
            _show_information.SetInformation(((XMLDataHolder)chartVoltammogram.Series[idx].Tag).GetData());
        }

        public void AddDataToCurrentSeries(System.Windows.Forms.DataVisualization.Charting.Series series, bool mode_voltammmetry, double x_value, typeAxisX scaleX, double y_value, typeAxisY scaleY, double time, double ReZ = 0, double ImZ = 0, double freq = 0)
        {
            // chronoamperogramモードなのに、現インスタンスが_mode_coulomb_countingでない場合には、_chronoamperogramのそれを呼び出し
            if (false && !mode_voltammmetry && !_mode_coulomb_counting)
            {
                //_chronoamperogram.AddDataToCurrentSeries(false, x_value, scaleX, y_value, scaleY, time);
                return;
            }

            if (_currentIndex >= 0)
            {
                double x = Double.NaN, y = Double.NaN, coulomb = Double.NaN, current = Double.NaN, potential = Double.NaN;

                //double x = x_value / _scaleAxisX[(int)scaleX] * _scaleAxisX[(int)_selectedAxisX];
                //double y = y_value / _scaleAxisY[(int)scaleY] * _scaleAxisY[(int)_selectedAxisY];

                if (true || !_mode_coulomb_counting)
                {
                    potential = x_value / _scaleAxisX[(int)scaleX] * _scaleAxisX[(int)typeAxisX.Potential_in_mV];
                    current = y_value / _scaleAxisY[(int)scaleY] * _scaleAxisY[(int)typeAxisY.Current_in_uA];
                    //System.Windows.Forms.DataVisualization.Charting.DataPoint p = chartVoltammogram.Series[_currentIndex].Points.LastOrDefault();

                    Invoke((Action)delegate ()
                    {
                        System.Windows.Forms.DataVisualization.Charting.DataPoint p = series.Points.LastOrDefault();

                        if (p != null)
                        {
                            coulomb = p.YValues[COULOMB] + (y_value / _scaleAxisY[(int)scaleY] * _scaleAxisY[(int)typeAxisY.Current_in_mA]) * (time - p.YValues[TIME]);
                        }
                        else
                        {
                            coulomb = 0 + (y_value / _scaleAxisY[(int)scaleY] * _scaleAxisY[(int)typeAxisY.Current_in_mA]) * (time - 0);
                        }

                        switch (_selectedAxisX)
                        {
                            case typeAxisX.Potential_in_mV:
                                x = potential;
                                break;

                            case typeAxisX.Potential_in_V:
                                x = x_value / _scaleAxisX[(int)scaleX] * _scaleAxisX[(int)typeAxisX.Potential_in_V];
                                break;

                            case typeAxisX.Time_in_sec:
                                x = time;
                                break;

                            case typeAxisX.Time_in_hour:
                                x = time * _scaleAxisX[(int)typeAxisX.Time_in_hour];
                                break;

                            case typeAxisX.ReZ_in_ohm:
                                x = ReZ;
                                break;

                            case typeAxisX.Freq:
                                x = freq;
                                break;

                                /*
                                 * 軸パラメータの追加の仕方 (HOWTO-01)
                                 * 
                                 * 
                            case typeAxisX.Freq:
                                x = freq;
                                break;
                                 * 
                                 * 
                                 * 
                                 * 
                                 */

                            default:
                                break;
                        }

                        switch(_selectedAxisY)
                        {
                            case typeAxisY.Current_in_uA:
                                y = current;
                                break;

                            case typeAxisY.Current_in_mA:
                                y = y_value / _scaleAxisY[(int)scaleY] * _scaleAxisY[(int)typeAxisY.Current_in_mA];
                                break;

                            case typeAxisY.Current_in_nA:
                                y = y_value / _scaleAxisY[(int)scaleY] * _scaleAxisY[(int)typeAxisY.Current_in_nA];
                                break;

                            case typeAxisY.Coulomb_in_mC:
                                y = coulomb;
                                break;

                            case typeAxisY.Coulomb_in_C:
                                y = coulomb / _scaleAxisY[(int)typeAxisY.Coulomb_in_mC] * _scaleAxisY[(int)typeAxisY.Coulomb_in_C];
                                break;

                            case typeAxisY.Potential_in_mV:
                                y = potential;
                                break;

                            case typeAxisY.ImZ_in_ohm:
                                y = ImZ;
                                break;

                            case typeAxisY.C2_in_MS_plot:
                                y = Math.Pow(ImZ * freq * 2 * Math.PI, 2) * 1e-6;
                                break;

                            case typeAxisY.Z_in_ohm:
                                y = 0.5 * Math.Log10(Math.Pow(ReZ, 2) + Math.Pow(ImZ, 2));
                                break;

                            default:
                                break;
                        }
                        //Console.WriteLine($"|Z| = {y}, {Math.Sqrt(Math.Pow(ReZ, 2) + Math.Pow(ImZ, 2))}");

                        //chartVoltammogram.Series[_currentIndex].Points.AddXY(
                        series.Points.AddXY(
                            x,
                            y,
                            potential,
                            current,
                            time,
                            coulomb,
                            ReZ, ImZ, freq
                        );
                    });
                }
                else
                {
                    if (chartVoltammogram.Series[_currentIndex].Points.Count == 0)
                    {
                        double q = 0.0;
                        chartVoltammogram.Series[_currentIndex].Points.AddXY(x, q);

                        _coulomb_counting_time = x;
                        _coulomb_counting_current = y;

                        return;
                    }
                    else
                    {
                        double q = chartVoltammogram.Series[_currentIndex].Points.Last().YValues[0] + ((y) * (x - _coulomb_counting_time) / 1000);
                        chartVoltammogram.Series[_currentIndex].Points.AddXY(x, q);

                        _coulomb_counting_time = x;
                        _coulomb_counting_current = y;

                        y = q;
                    }
                }

                //
                // 軸の調整
                //   1. 最小値と最大値の更新 (ここでは値のリセットはしない)
                //   2. 最小値と最大値に基づく目盛り間隔の更新
                //

                //
                // X軸の調整
                //

                bool renew_min = false, renew_max = false;

                if (true ||  !_mode_coulomb_counting)
                {
                    // ボルタモグラムを記録する場合

                    if ((x < _currentMinX) || (double.IsNaN(_currentMinX)))
                    {
                        _currentMinX = x; renew_min = true;
                    }
                }
                else
                {
                    chartVoltammogram.ChartAreas[0].AxisX.Minimum = 0;
                    chartVoltammogram.ChartAreas[0].AxisX2.Minimum = 0;
                }
                if ((x > _currentMaxX) || (double.IsNaN(_currentMaxX)))
                {
                    _currentMaxX = x; renew_max = true;
                }

                if (renew_min || renew_max)
                {
                    //   1. 最小値と最大値の更新 (ここでは値のリセットはしない)
                    double length = Math.Abs(_currentMaxX - _currentMinX);
                    double ext = Math.Max(length, Math.Abs(_currentMaxX)) * 0.10;
                    if (renew_min) { _currentMinX -= ext; }
                    if (renew_max) { _currentMaxX += ext; }

                    //double order = Math.Truncate(Math.Log10(Math.Abs(length)));
                    //double interval = Math.Pow(10, order - 1);
                    //double interval_f = Math.Log10(length) % 1; //if (interval_f < 0) interval_f = 1 - interval_f;
                    //double m = (interval_f < 0.1) ? 2.5 : ((interval_f < 0.4) ? 5 : ((interval_f < 0.7) ? 10 : 20));
                    ////double length_normalized = Math.Ceiling(length / (interval * m)) * (interval * m);

                    //if (renew_min) { _currentMinX = Math.Sign(_currentMinX) * Math.Ceiling(Math.Abs(_currentMinX) / (interval * m)) * (interval * m); }
                    //if (renew_max) { _currentMaxX = Math.Sign(_currentMaxX) * Math.Ceiling(Math.Abs(_currentMaxX) / (interval * m)) * (interval * m); }

                    //   2. 最小値と最大値に基づく目盛り間隔の更新
                    length = Math.Abs(_currentMaxX - _currentMinX);
                    RescaleTicksX(length, _currentMinX);
                }

                //
                // Y軸の調整
                //

                renew_min = false; renew_max = false;

                if ((y < _currentMinY) || (double.IsNaN(_currentMinY)))
                {
                    _currentMinY = y; renew_min = true;
                }
                if ((y > _currentMaxY) || (double.IsNaN(_currentMaxY)))
                {
                    _currentMaxY = y; renew_max = true;
                }

                if ( renew_min || renew_max )
                {
                    //   1. 最小値と最大値の更新 (ここでは値のリセットはしない)
                    double length = Math.Abs(_currentMaxY - _currentMinY);
                    double ext = Math.Max(length, Math.Abs(_currentMaxY)) * 0.10;

                    if (renew_min) { _currentMinY -= ext; /*Console.WriteLine("renew_min, {0}", _currentMinY);*/  }
                    if (renew_max) { _currentMaxY += ext; /*Console.WriteLine("renew_max, {0}", _currentMaxY);*/  }

                    //   2. 最小値と最大値に基づく目盛り間隔の更新
                    length = Math.Abs(_currentMaxY - _currentMinY);
                    RescaleTicksY(length, _currentMinY);
                }
            }
            else
            {
                Console.WriteLine("_currentIndex is -1.");
            }

            return;
        }

        public int RemoveSeries(int index)
        {
            if (index <= -1)
            {
                if(_indexRecording == -1)
                {
                    _series.Clear();
                    chartVoltammogram.Series.Clear();
                    toolStripComboBoxSeries.Items.Clear();

                    ToolStripItemCollection tsic = toolStripDropDownButton2.DropDownItems;
                    for (int i = tsic.Count; i >= 3; i--)
                    {
                        tsic.RemoveAt(i - 1);
                    }

                    _currentIndex = -1;
                }
                else
                {
                    for(int i = (_indexRecording+1); i < _series.Count(); /* i++ */) // カウンタの上限値の方が--されていくのでこの場合のi++は不要
                    {
                        RemoveSeries(i);
                    }

                    for(int i = 0; i < _indexRecording; /* i++ */) // カウンタの上限値の方が--されていくのでこの場合のi++は不要
                    {
                        RemoveSeries(i);
                    }
                }
            }
            else
            {
                _series.RemoveAt(index);
                chartVoltammogram.Series.RemoveAt(index);
                toolStripComboBoxSeries.Items.RemoveAt(index);

                ToolStripItemCollection tsic = toolStripDropDownButton2.DropDownItems;
                //int idx2 = tsic.IndexOf(((ToolStripMenuItem)sender));
                //tsic.Remove((ToolStripMenuItem)sender);
                tsic.RemoveAt(index + 2);

                if(index == _indexRecording)
                {
                    _indexRecording = -1;
                } else if( index < _indexRecording)
                {
                    _indexRecording--;
                }
                _currentIndex = _series.Count() - 1;
            }

            return _currentIndex;
        }

        private void SerializeParameter(XmlDocument doc, object obj, string name)
        {
            Type T = obj.GetType();
            XmlSerializer serializer = new XmlSerializer(T);//typeof(List<string>)

            StringWriter twriter = new StringWriter();
            XmlTextWriter xmltwriter = new XmlTextWriter(twriter);
            serializer.Serialize(xmltwriter, obj);

            XmlDocumentFragment docflag = doc.CreateDocumentFragment();
            docflag.InnerXml = twriter.ToString();
            XmlAttribute xmlattr = doc.CreateAttribute("id");
            xmlattr.Value = name;
            docflag.LastChild.Attributes.Append(xmlattr);

            XmlElement targetRoot = doc["Chart"];
            XmlNode node = doc.ImportNode(docflag, true);
            //XmlNode node2 = node.LastChild.CloneNode(true);
            //targetRoot.AppendChild(node2);
            targetRoot.AppendChild(node.LastChild);
        }

        private object DeserializeParameter(XmlDocument doc, Type T, string name)
        {
            XmlDocumentFragment docflag2 = doc.CreateDocumentFragment();
            XmlNode xmldecl = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            XmlNodeList xmllist = doc.SelectNodes("//*[@id='" + name + "']"); // nameof(_series) // ArrayOfString
            docflag2.AppendChild(xmldecl);
            docflag2.AppendChild(xmllist.Item(0));
            //string t = nameof(_series);
            //Type T = obj.GetType();
            XmlSerializer serializer = new XmlSerializer(T);
            StringReader treader = new StringReader(docflag2.InnerXml);
            //Type T = _series.GetType();
            //_series = (dynamic)Convert.ChangeType(serializer.Deserialize(treader), T);
            //obj = serializer.Deserialize(treader);
            return serializer.Deserialize(treader);
        }

        public void SaveData(string path, int reduced, bool compressed)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            //settings.IndentChars = "\t";

            Stream stream = new MemoryStream();

            System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(stream, settings);
            bool use_serializer = false;
            if (use_serializer)
            {
                System.Windows.Forms.DataVisualization.Charting.ChartSerializer cs = chartVoltammogram.Serializer;
                //cs.SerializableContent = "*.*";
                //cs.Content = System.Windows.Forms.DataVisualization.Charting.SerializationContents.All;
                cs.Save(writer); // データ点数が多いとかなり時間がかかる！
            }
            else
            {
                writer.WriteStartElement("Chart");
                writer.WriteStartElement("Series");

                //int step = (reduced > 1) ? reduced : 1;
                for (int i = 0; i < chartVoltammogram.Series.Count(); i++)
                {
                    writer.WriteStartElement("Data");
                    writer.WriteAttributeString("name", chartVoltammogram.Series[i].Name);
                    writer.WriteAttributeString("base-name", _series[i]);
                    writer.WriteAttributeString("color", chartVoltammogram.Series[i].Color.Name);
                    writer.WriteAttributeString("width", chartVoltammogram.Series[i].BorderWidth.ToString());
                    writer.WriteAttributeString("dash", ((int)chartVoltammogram.Series[i].BorderDashStyle).ToString());

                    writer.WriteString("\n");

                    int step = (chartVoltammogram.Series[i].Points.Count() < 200) ? 1 : reduced;

                    for (int j = 0, k = 0; j < chartVoltammogram.Series[i].Points.Count(); /* j += step */ )
                    {
                        //serializedSeries += (
                        //      "\t\t"
                        //    + chartVoltammogram.Series[i].Points[j].XValue.ToString()
                        //    + ", "
                        //    //String.Join(",", chartVoltammogram.Series[i].Points[j].YValues.Select(o => o.ToString()).ToArray()) +
                        //    + chartVoltammogram.Series[i].Points[j].YValues[0].ToString()
                        //    + "\n"
                        //);

                        //serializedSeries += chartVoltammogram.Series[i].Points[j].XValue.ToString() + ","+ chartVoltammogram.Series[i].Points[j].YValues[0].ToString();

                        //writer.WriteString(chartVoltammogram.Series[i].Points[j].XValue.ToString() + "," + chartVoltammogram.Series[i].Points[j].YValues[0].ToString()+ "\n");

                        if (true)
                        {
                            // stepずつのpointのみを残す。それ以外のpointはchartVoltammogram.Series[i].Pointsから削除する。
                            if ((k % step) == 0)
                            {
                                writer.WriteString(
                                      "      "
                                    + chartVoltammogram.Series[i].Points[j].XValue.ToString()
                                    + ", "
                                    + String.Join(",", chartVoltammogram.Series[i].Points[j].YValues.Select(o => o.ToString()).ToArray())
                                    + "\n"
                                );
                                j++;
                            }
                            else
                            {
                                chartVoltammogram.Series[i].Points.RemoveAt(j);
                                 // if (k == step) k = 0;
                            }
                            k++;
                        }
                        else
                        {
                            writer.WriteString(
                                  "      "
                                + chartVoltammogram.Series[i].Points[j].XValue.ToString()
                                + ", "
                                + String.Join(",", chartVoltammogram.Series[i].Points[j].YValues.Select(o => o.ToString()).ToArray())
                                + "\n"
                            );
                        }
                    }
                    writer.WriteString(
                            "    "
                    );
                    //writer.WriteElementString("Series", serializedSeries);

                    writer.WriteEndElement();
                }

                //writer.WriteStartElement("child");
                //writer.WriteStartElement("child-child");
                //writer.WriteStartAttribute("attr");
                //writer.WriteString("attr-value");
                //writer.WriteEndAttribute();
                //writer.WriteString("へへ");
                //writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                //writer.WriteEndDocument();

                writer.Flush();
            }
            writer.Close(); // ストリームに一旦書き込み

            stream.Position = 0;

            XmlDocument doc = new XmlDocument();
            doc.Load(stream);

            // 2021/1/29: LoadSetOfSeriesの方でも使わなくなったので、comment out
            //SerializeParameter(doc, _series, nameof(_series));

            //List<int> shifts = new List<int>();
            //System.Windows.Forms.DataVisualization.Charting.SeriesCollection sc = chartVoltammogram.Series;
            //for (int i = 0; i < sc.Count(); i++)
            //{
            //    shifts.Add((int)chartVoltammogram.Series[i].Tag);
            //}
            //SerializeParameter(doc, shifts, nameof(shifts));
            System.Windows.Forms.DataVisualization.Charting.SeriesCollection sc = chartVoltammogram.Series;
            for (int i = 0; i < sc.Count(); i++)
            {
                XmlNode p = doc.ImportNode(((XMLDataHolder)chartVoltammogram.Series[i].Tag).GetData(), true);
                doc["Chart"].AppendChild(p);
            }

            SerializeParameter(doc, toolStripComboBoxAxisX.SelectedIndex, "toolStripComboBoxAxisX.SelectedIndex");
            SerializeParameter(doc, toolStripComboBoxRef.SelectedIndex, "toolStripComboBoxRef.SelectedIndex");
            SerializeParameter(doc, toolStripComboBoxAxisY.SelectedIndex, "toolStripComboBoxAxisY.SelectedIndex");

            System.IO.FileStream streamFile = new System.IO.FileStream(path, System.IO.FileMode.Create);
            System.IO.Stream gzipStrm;
            if (compressed)
            {
                //圧縮モードのGZipStreamを作成する
                //System.IO.Compression.GZipStream gzipStrm = new System.IO.Compression.GZipStream(streamFile, System.IO.Compression.CompressionMode.Compress);
                gzipStrm = new System.IO.Compression.GZipStream(streamFile, System.IO.Compression.CompressionMode.Compress);
            }
            else
            {
                //System.IO.FileStream gzipStrm = streamFile;
                gzipStrm = streamFile;
            }
            doc.Save(gzipStrm);
            //doc.Save(file_path);
            gzipStrm.Close();
        }

        public void ExportDataAsCSV(string path, bool reduced, bool for_excel)
        {
            //saveFileDialog1.Filter = "CSV file (*.csv)|*.csv|All types(*.*)|*.*";
            //saveFileDialog1.FilterIndex = 1;
            //saveFileDialog1.Title = "Export in the CSV format";
            //saveFileDialog1.FileName = "";
            //saveFileDialog1.ShowHelp = true;

            //if (saveFileDialog1.InitialDirectory == null)
            //{
            //    saveFileDialog1.InitialDirectory = @"c:\";
            //}

            //if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            //{
            //    string file_path = saveFileDialog1.FileName;

            try
            {
                TextWriter writer = new StreamWriter(path, false);

                if (for_excel)
                {
                    string potential = "";
                    switch (_selectedAxisX)
                    {
                        case typeAxisX.Potential_in_mV: potential = "mV"; break;
                        case typeAxisX.Potential_in_V:  potential = "V"; break;
                        case typeAxisX.Time_in_sec:     potential = "s"; break;
                        case typeAxisX.ReZ_in_ohm:      potential = "ohm(ReZ)"; break;
                        default: break;
                    }
                    writer.Write(", {0}", potential);
                    for (int i = 0; i < _series.Count(); i++)
                    {
                        string current = "";

                        switch (_selectedAxisY)
                        {
                            case typeAxisY.Current_in_nA: current = "nA"; break;
                            case typeAxisY.Current_in_uA: current = "uA"; break;
                            case typeAxisY.Current_in_mA: current = "mA"; break;
                            case typeAxisY.Coulomb_in_mC: current = "mC"; break;
                            case typeAxisY.Coulomb_in_C:  current = "C"; break;
                            case typeAxisY.ImZ_in_ohm:    current = "ohm(ImZ)"; break;
                            default: break;
                        }

                        writer.Write(", {0}", current);
                    }
                    writer.WriteLine();

                    writer.Write(",");
                    for (int i = 0; i < _series.Count(); i++)
                    {
                        writer.Write(", {0} ({1})", _series[i], ((XMLDataHolder)chartVoltammogram.Series[i].Tag).GetDatum("reference", "0"));
                    }
                    writer.WriteLine();

                    string shift_series = ""; int step = (reduced) ? 5 : 1;// とりあえず5点ごと採取でデータを減少させる。
                    for (int i = 0; i < _series.Count(); i++)
                    {
                        for (int j = 0; j < chartVoltammogram.Series[i].Points.Count(); j += step)
                        {
                            writer.WriteLine(", " + chartVoltammogram.Series[i].Points[j].XValue + shift_series + ", " + chartVoltammogram.Series[i].Points[j].YValues[0]);
                        }

                        shift_series += ",";
                    }
                }
                else
                {
                    for (int i = 0; i < _series.Count(); i++)
                    {
                        string potential = "", current = "";

                        switch (_selectedAxisX)
                        {
                            case typeAxisX.Potential_in_mV: potential = "mV"; break;
                            case typeAxisX.Potential_in_V:  potential = "V"; break;
                            case typeAxisX.Time_in_sec:     potential = "s"; break;
                            case typeAxisX.ReZ_in_ohm:      potential = "ohm(ReZ)"; break;
                            default: break;
                        }
                        switch (_selectedAxisY)
                        {
                            case typeAxisY.Current_in_nA: current = "nA"; break;
                            case typeAxisY.Current_in_uA: current = "uA"; break;
                            case typeAxisY.Current_in_mA: current = "mA"; break;
                            case typeAxisY.Coulomb_in_mC: current = "mC"; break;
                            case typeAxisY.Coulomb_in_C:  current = "C"; break;
                            case typeAxisY.ImZ_in_ohm:    current = "ohm(ImZ)"; break;
                            default: break;
                        }

                        writer.Write(", {0}, {1}", potential, current);
                    }
                    writer.WriteLine();

                    for (int i = 0; i < _series.Count(); i++)
                    {
                        writer.Write(", , {0} ({1})", _series[i], ((XMLDataHolder)chartVoltammogram.Series[i].Tag).GetDatum("reference", "0"));
                    }
                    writer.WriteLine();

                    int j = 0; bool is_output = false; int step = (reduced) ? 5 : 1;// とりあえず5点ごと採取でデータを減少させる。
                    do
                    {
                        is_output = false;
                        for (int i = 0; i < _series.Count(); i++)
                        {
                            if (j < chartVoltammogram.Series[i].Points.Count)
                            {
                                writer.Write(", " + chartVoltammogram.Series[i].Points[j].XValue + ", " + chartVoltammogram.Series[i].Points[j].YValues[0]);
                                is_output |= true;
                            }
                            else
                            {
                                writer.Write(",,");
                                is_output |= false;
                            }
                        }
                        writer.WriteLine();

                        //j++;
                        j += step;
                    }
                    while (is_output);
                }

                writer.Close();
            }
            catch (System.IO.IOException)
            {
                MessageBox.Show(this, "Cannot open file.", "Error Opening File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //}
        }

        public void ExportDataAsMetafile(int decimation_ratio)
        {
            try
            {
                System.IO.MemoryStream memStream = new System.IO.MemoryStream();

                if (true)
                {
                    bool scrollbar_x = false, scrollbar_y = false;
                    if (chartVoltammogram.ChartAreas[0].AxisX.ScrollBar.IsVisible)
                    {
                        scrollbar_x = true;
                        chartVoltammogram.ChartAreas[0].AxisX.ScrollBar.Enabled = false;
                    }
                    if (chartVoltammogram.ChartAreas[0].AxisY.ScrollBar.IsVisible)
                    {
                        scrollbar_y = true;
                        chartVoltammogram.ChartAreas[0].AxisY.ScrollBar.Enabled = false;
                    }
                    chartVoltammogram.ChartAreas[0].CursorX.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.NotSet;
                    chartVoltammogram.ChartAreas[0].CursorY.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.NotSet;

                    //chartVoltammogram.ChartAreas[0].CursorX.


                    //var chart = chartVoltammogram.DeepClone();

                    //System.Windows.Forms.DataVisualization.Charting.Chart chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
                    //Enter your chart building code here
                    //System.IO.MemoryStream myStream = new System.IO.MemoryStream();
                    //System.Windows.Forms.DataVisualization.Charting.Chart chart2 = new System.Windows.Forms.DataVisualization.Charting.Chart();
                    //chartVoltammogram.Serializer.Save(myStream);
                    //chart2.Serializer.Load(myStream);



                    //
                    // 一旦、データを退避させて、データ点数を減らしたものをメタファイル化するようにする
                    //

                    DataSet ds = chartVoltammogram.DataManipulator.ExportSeriesValues();

                    for (int i = 0; i < ds.Tables.Count; i++)
                    {
                        System.Windows.Forms.DataVisualization.Charting.Series series = chartVoltammogram.Series[i];
                        series.Points.Clear();

                        for (int j = 0; j < ds.Tables[i].Rows.Count; j++)
                        {
                            if ((j % decimation_ratio) == 0)
                            {
                                DataRow row = ds.Tables[i].Rows[j];
                                series.Points.AddXY(row.Field<double>(0), row.ItemArray.Skip(1).ToArray());
                            }
                            j++;
                        }
                    }


                    //System.Windows.Forms.DataVisualization.Charting.Series series = chartVoltammogram.Series[0];
                    //series.Points.Clear();

                    //int i = 0;
                    //foreach (DataRow row in ds.Tables[0].Rows)
                    //{
                    //    if((i % 100) == 0)
                    //    {
                    //        series.Points.AddXY(row.Field<double>(0), row.ItemArray.Skip(1).ToArray());                   
                    //    }
                    //    i++;
                    //}






                    chartVoltammogram.SaveImage(memStream, System.Drawing.Imaging.ImageFormat.Emf);
                    memStream.Seek(0, SeekOrigin.Begin);
                    System.Drawing.Imaging.Metafile mf1 = new System.Drawing.Imaging.Metafile(memStream);
                    //Metafile
                    //chartVoltammogram.SaveImage(@"c:\temp\test.wmf", System.Drawing.Imaging.ImageFormat.Wmf);
                    //System.Drawing.Imaging.Metafile mf2 = new System.Drawing.Imaging.Metafile(@"c:\temp\test.wmf");
                    //mf2.Dispose();

                    //Image img = Image.FromFile(@"c:\temp\test4.wmf");
                    //System.Windows.Forms.Clipboard.SetDataObject(img);
                    //img.Dispose();


                    ClipboardMetafileHelper.PutEnhMetafileOnClipboard(this.Handle, mf1);

                    //Clipboard.SetDataObject(memStream);

                    mf1.Dispose();
                    //mf2.Dispose();

                    //if (scrollbar_x)
                    //{
                    //    chartVoltammogram.ChartAreas[0].AxisX.ScrollBar.Enabled = true;
                    //}
                    //if (scrollbar_y)
                    //{
                    //    chartVoltammogram.ChartAreas[0].AxisY.ScrollBar.Enabled = true;
                    //}




                    //series.Points.Clear();

                    //foreach (DataRow row in ds.Tables[0].Rows)
                    //{
                    //    series.Points.AddXY(row.Field<double>(0), row.ItemArray.Skip(1).ToArray());
                    //}


                    for (int i = 0; i < ds.Tables.Count; i++)
                    {
                        System.Windows.Forms.DataVisualization.Charting.Series series = chartVoltammogram.Series[i];
                        series.Points.Clear();

                        for (int j = 0; j < ds.Tables[i].Rows.Count; j++)
                        {
                            DataRow row = ds.Tables[i].Rows[j];
                            series.Points.AddXY(row.Field<double>(0), row.ItemArray.Skip(1).ToArray());
                        }
                    }









                    chartVoltammogram.ChartAreas[0].CursorX.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
                    chartVoltammogram.ChartAreas[0].CursorY.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
                }
                else
                {
                    chartVoltammogram.SaveImage(memStream, System.Drawing.Imaging.ImageFormat.Emf);
                    memStream.Seek(0, SeekOrigin.Begin);

                    Graphics offScreenBufferGraphics = Graphics.FromHwndInternal(IntPtr.Zero);
                    IntPtr deviceContextHandle = offScreenBufferGraphics.GetHdc();

                    //System.Drawing.Imaging.Metafile bmp = new System.Drawing.Imaging.Metafile(memStream, deviceContextHandle, System.Drawing.Imaging.EmfType.EmfPlusOnly);
                    System.Drawing.Imaging.Metafile bmp = new System.Drawing.Imaging.Metafile(memStream);

                    //Image img = Image.FromStream(memStream);
                    System.Windows.Forms.Clipboard.SetDataObject(bmp);


                    //Clipboard.SetDataObject(memStream);

                }
            }
            catch (Exception exc)
            {

            }
        }

        public void SaveDataOfCurrentSeries(string filename, XMLDataHolder exp_conditions)
        {
            TextWriter writer = new StreamWriter(filename, false);

            writer.WriteLine("EC-Lab ASCII FILE");
            writer.WriteLine("Nb header lines : 4");
            writer.WriteLine(exp_conditions.GetData().OuterXml);
            //XmlNodeList xnl = ((XMLDataHolder)chartVoltammogram.Series[_currentIndex].Tag).GetData("conditions");
            //if(xnl.Count ==1)
            //{
            //    writer.WriteLine("Nb header lines : 4");
            //    writer.WriteLine(xnl[0].OuterXml);
            //}
            //else
            //{
            //    writer.WriteLine("Nb header lines : 3");
            //}
            writer.WriteLine("Ewe/V	I/mA    time/s");

            for (int i = 1; i < chartVoltammogram.Series[_currentIndex].Points.Count; i++)
            {
                writer.Write(
                    "{0}\t{1}\t{2}",
                    chartVoltammogram.Series[_currentIndex].Points[i].YValues[POTENTIAL] / _scaleAxisX[(int)typeAxisX.Potential_in_mV] * _scaleAxisX[(int)typeAxisX.Potential_in_V],
                    chartVoltammogram.Series[_currentIndex].Points[i].YValues[CURRENT] / _scaleAxisY[(int)typeAxisY.Current_in_uA] * _scaleAxisY[(int)typeAxisY.Current_in_mA],
                    chartVoltammogram.Series[_currentIndex].Points[i].YValues[TIME]
                );
                writer.WriteLine();
            }

            writer.Close();
        }

        private double RescaleTicksX(double length, double p, bool is_zooming = false)
        {
            if (length == 0) return Double.NaN;

            switch (_selectedAxisX)
            {
                case typeAxisX.Potential_in_mV:
                case typeAxisX.Potential_in_V:
                case typeAxisX.Time_in_sec:
                case typeAxisX.Time_in_hour:
                case typeAxisX.ReZ_in_ohm:
                    chartVoltammogram.ChartAreas[0].AxisX.IsLogarithmic = false;
                    chartVoltammogram.ChartAreas[0].AxisX2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
                    break;

                case typeAxisX.Freq:
                    chartVoltammogram.ChartAreas[0].AxisX.IsLogarithmic = true;
                    chartVoltammogram.ChartAreas[0].AxisX2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.False;
                    break;

                default:
                    break;
            }

            if (_selectedAxisX != typeAxisX.Freq)
            {
                double order = Math.Truncate(Math.Log10(Math.Abs(length)));
                double interval = Math.Pow(10, order - 1);
                double interval_f = Math.Log10(length) % 1; //if (interval_f < 0) interval_f = 1 - interval_f;
                double m = (interval_f < 0.1) ? 2.5 : ((interval_f < 0.4) ? 5 : ((interval_f < 0.7) ? 10 : 20));
                //Console.WriteLine($"plot X interval (interval_f), fine-interval: {interval} ({interval_f}), {m}");

                chartVoltammogram.ChartAreas[0].AxisX.Interval = 0;// interval * m;
                chartVoltammogram.ChartAreas[0].AxisX2.Interval = 0;// interval * m;
                chartVoltammogram.ChartAreas[0].AxisX.MajorTickMark.Interval = interval * m;
                chartVoltammogram.ChartAreas[0].AxisX2.MajorTickMark.Interval = interval * m;
                chartVoltammogram.ChartAreas[0].AxisX.MinorTickMark.Interval = interval * m / 5;
                chartVoltammogram.ChartAreas[0].AxisX2.MinorTickMark.Interval = interval * m / 5;
                chartVoltammogram.ChartAreas[0].AxisX.LabelStyle.Interval = interval * m;
                chartVoltammogram.ChartAreas[0].AxisX2.LabelStyle.Interval = interval * m;
                chartVoltammogram.ChartAreas[0].CursorX.Interval = interval / 1000;

                chartVoltammogram.ChartAreas[0].AxisX.LabelStyle.Format = "0" + ((order < 1)? ("." + new String('0',  (int)(Math.Abs(order-2)))) : "");

                chartVoltammogram.ChartAreas[0].AxisX.Minimum = _currentMinX;
                chartVoltammogram.ChartAreas[0].AxisX2.Minimum = _currentMinX;
                chartVoltammogram.ChartAreas[0].AxisX.Maximum = _currentMaxX;
                chartVoltammogram.ChartAreas[0].AxisX2.Maximum = _currentMaxX;

                if (!is_zooming)
                {
                    if (p < 0)
                    {
                        chartVoltammogram.ChartAreas[0].AxisX.LabelStyle.IntervalOffset = Math.Abs((p % (interval * m)));
                        chartVoltammogram.ChartAreas[0].AxisX.MajorTickMark.IntervalOffset = Math.Abs(p % (interval * m));
                        chartVoltammogram.ChartAreas[0].AxisX.MinorTickMark.IntervalOffset = Math.Abs(p % (interval * m / 5));
                        chartVoltammogram.ChartAreas[0].AxisX2.MajorTickMark.IntervalOffset = Math.Abs(p % (interval * m));
                        chartVoltammogram.ChartAreas[0].AxisX2.MinorTickMark.IntervalOffset = Math.Abs(p % (interval * m / 5));
                    }
                    else
                    {
                        chartVoltammogram.ChartAreas[0].AxisX.LabelStyle.IntervalOffset = (interval * m) - Math.Abs((p % (interval * m)));
                        chartVoltammogram.ChartAreas[0].AxisX.MajorTickMark.IntervalOffset = (interval * m) - Math.Abs((p % (interval * m)));
                        chartVoltammogram.ChartAreas[0].AxisX.MinorTickMark.IntervalOffset = (interval * m / 5) - Math.Abs((p % (interval * m / 5)));
                        chartVoltammogram.ChartAreas[0].AxisX2.MajorTickMark.IntervalOffset = (interval * m) - Math.Abs((p % (interval * m)));
                        chartVoltammogram.ChartAreas[0].AxisX2.MinorTickMark.IntervalOffset = (interval * m / 5) - Math.Abs((p % (interval * m / 5)));
                    }
                }

                return (interval * m);
            }
            else
            {
                double order2 = Math.Floor(Math.Log10(_currentMinX));
                double min2 = Math.Pow(10, order2);

                if (Double.IsNaN(chartVoltammogram.ChartAreas[0].AxisX.Minimum))
                {
                    chartVoltammogram.ChartAreas[0].AxisX.Minimum = min2;
                }
                else
                {
                    if (chartVoltammogram.ChartAreas[0].AxisX.Minimum > (min2)) chartVoltammogram.ChartAreas[0].AxisX.Minimum = min2;
                }

                double order = Math.Ceiling(Math.Log10(_currentMaxX));
                double max = Math.Pow(10, order);

                if (Double.IsNaN(chartVoltammogram.ChartAreas[0].AxisX.Maximum))
                {
                    chartVoltammogram.ChartAreas[0].AxisX.Maximum = max;
                }
                else
                {
                    if (chartVoltammogram.ChartAreas[0].AxisX.Maximum < (max)) chartVoltammogram.ChartAreas[0].AxisX.Maximum = max;
                }

                chartVoltammogram.ChartAreas[0].AxisX.MajorTickMark.Interval = 1;
                chartVoltammogram.ChartAreas[0].AxisX.MajorTickMark.IntervalOffset = 0;
                chartVoltammogram.ChartAreas[0].AxisX.MinorTickMark.Interval = 1;
                chartVoltammogram.ChartAreas[0].AxisX.MinorTickMark.IntervalOffset = 0;
                chartVoltammogram.ChartAreas[0].AxisX.LabelStyle.Interval = 1;
                chartVoltammogram.ChartAreas[0].AxisX.LabelStyle.IntervalOffset = 0;

                //chartVoltammogram.ChartAreas[0].AxisX.Minimum = Double.NaN;

                return 1;
            }

        }

        private double RescaleTicksY(double length, double p, bool is_zooming = false)
        {
            if (length == 0) return Double.NaN;

            //switch (_selectedAxisY)
            //{
            //    case typeAxisY.Current_in_nA:
            //    case typeAxisY.Current_in_uA:
            //    case typeAxisY.Current_in_mA:
            //    case typeAxisY.Coulomb_in_mC:
            //    case typeAxisY.Coulomb_in_C:
            //    case typeAxisY.Potential_in_mV:
            //    case typeAxisY.ImZ_in_ohm:
            //    case typeAxisY.C2_in_MS_plot:
            //        chartVoltammogram.ChartAreas[0].AxisY.IsLogarithmic = false;
            //        chartVoltammogram.ChartAreas[0].AxisY2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            //        break;

            //    case typeAxisY.Z_in_ohm:
            //        chartVoltammogram.ChartAreas[0].AxisY.IsLogarithmic = true; // Y軸データが複数だと、対数軸が機能しない様子。
            //        chartVoltammogram.ChartAreas[0].AxisY2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.False;
            //        break;

            //    default:
            //        break;
            //}

            if (_selectedAxisY != typeAxisY.Z_in_ohm)
            {
                double order = Math.Truncate(Math.Log10(Math.Abs(length)));
                double interval = Math.Pow(10, order - 1);
                double interval_f = Math.Log10(length) % 1;
                double m = (interval_f < 0.1) ? 2 : ((interval_f < 0.4) ? 5 : ((interval_f < 0.7) ? 10 : 20));
                //Console.WriteLine($"plot Y interval, fine-interval: {interval}, {m}");

                chartVoltammogram.ChartAreas[0].AxisY.Interval = 0;// interval * m;
                chartVoltammogram.ChartAreas[0].AxisY2.Interval = 0;// interval * m;
                chartVoltammogram.ChartAreas[0].AxisY.MajorTickMark.Interval = interval * m;
                chartVoltammogram.ChartAreas[0].AxisY2.MajorTickMark.Interval = interval * m;
                chartVoltammogram.ChartAreas[0].AxisY.MinorTickMark.Interval = interval * m / 2;
                chartVoltammogram.ChartAreas[0].AxisY2.MinorTickMark.Interval = interval * m / 2;
                chartVoltammogram.ChartAreas[0].AxisY.LabelStyle.Interval = interval * m;
                chartVoltammogram.ChartAreas[0].AxisY2.LabelStyle.Interval = interval * m;
                chartVoltammogram.ChartAreas[0].CursorY.Interval = interval / 1000;

                chartVoltammogram.ChartAreas[0].AxisY.LabelStyle.Format = "0" + ((order < 1) ? ("." + new String('0', (int)(Math.Abs(order - 2)))) : "");

                chartVoltammogram.ChartAreas[0].AxisY.Minimum = _currentMinY;
                chartVoltammogram.ChartAreas[0].AxisY2.Minimum = _currentMinY;
                chartVoltammogram.ChartAreas[0].AxisY.Maximum = _currentMaxY;
                chartVoltammogram.ChartAreas[0].AxisY2.Maximum = _currentMaxY;

                if(!is_zooming)
                {
                    if (p < 0)
                    {
                        chartVoltammogram.ChartAreas[0].AxisY.LabelStyle.IntervalOffset = Math.Abs((p % (interval * m)));
                        chartVoltammogram.ChartAreas[0].AxisY.MajorTickMark.IntervalOffset = Math.Abs(p % (interval * m));
                        chartVoltammogram.ChartAreas[0].AxisY.MinorTickMark.IntervalOffset = Math.Abs(p % (interval * m / 2));
                        chartVoltammogram.ChartAreas[0].AxisY2.MajorTickMark.IntervalOffset = Math.Abs(p % (interval * m));
                        chartVoltammogram.ChartAreas[0].AxisY2.MinorTickMark.IntervalOffset = Math.Abs(p % (interval * m / 2));
                    }
                    else
                    {
                        chartVoltammogram.ChartAreas[0].AxisY.LabelStyle.IntervalOffset = (interval * m) - Math.Abs((p % (interval * m)));
                        chartVoltammogram.ChartAreas[0].AxisY.MajorTickMark.IntervalOffset = (interval * m) - Math.Abs((p % (interval * m)));
                        chartVoltammogram.ChartAreas[0].AxisY.MinorTickMark.IntervalOffset = (interval * m / 2) - Math.Abs((p % (interval * m / 2)));
                        chartVoltammogram.ChartAreas[0].AxisY2.MajorTickMark.IntervalOffset = (interval * m) - Math.Abs((p % (interval * m)));
                        chartVoltammogram.ChartAreas[0].AxisY2.MinorTickMark.IntervalOffset = (interval * m / 2) - Math.Abs((p % (interval * m / 2)));
                    }
                }

                return (interval * m);
            }
            else
            {
                double min = Math.Floor(_currentMinY); 
                chartVoltammogram.ChartAreas[0].AxisY.Minimum = min;
                double max = Math.Ceiling(_currentMaxY);
                chartVoltammogram.ChartAreas[0].AxisY.Maximum = max;

                chartVoltammogram.ChartAreas[0].AxisY.MajorTickMark.Interval = 1;
                chartVoltammogram.ChartAreas[0].AxisY.MajorTickMark.IntervalOffset = 0;
                //chartVoltammogram.ChartAreas[0].AxisY.MajorTickMark.Size = 3F;
                chartVoltammogram.ChartAreas[0].AxisY.MinorTickMark.Interval = 0.1;
                chartVoltammogram.ChartAreas[0].AxisY.MinorTickMark.IntervalOffset = 0;
                chartVoltammogram.ChartAreas[0].AxisY.MinorTickMark.Size = 0.33F;
                chartVoltammogram.ChartAreas[0].AxisY.LabelStyle.Interval = 1;
                chartVoltammogram.ChartAreas[0].AxisY.LabelStyle.IntervalOffset = 0;

                return 1;
            }
        }

        public void SetAxisRangeX(bool auto, double from, double to)
        {
            if (auto)
            {
                ChangeAxisX(force: true);
            }
            else
            {
                double length = Math.Abs(to - from);

                _currentMinX = (from < to) ? from : to;
                _currentMaxX = (from < to) ? to : from;

                //   2. 最小値と最大値に基づく目盛り間隔の更新
                length = Math.Abs(_currentMaxX - _currentMinX);
                RescaleTicksX(length, _currentMinX);
                //RescaleTicksX(Math.Abs(to - from), (from < to)? from : to);
            }
        }

        public void SetAxisRangeY(bool auto, double from, double to)
        {
            if (auto)
            {
                ChangeAxisY(force: true);
            }
            else
            {
                double length = Math.Abs(to - from);

                _currentMinY = (from < to) ? from : to;
                _currentMaxY = (from < to) ? to : from;

                //   2. 最小値と最大値に基づく目盛り間隔の更新
                length = Math.Abs(_currentMaxY - _currentMinY);
                RescaleTicksY(length, _currentMinY);//, _currentMinY);
                //RescaleTicksX(Math.Abs(to - from), (from < to)? from : to);
            }
        }

        private void ChangeAxisX(bool force)
        {
            int idx = toolStripComboBoxRef.SelectedIndex;
            string reference_electrode = ((idx != -1) ? (string)toolStripComboBoxRef.SelectedItem : "");
            //reference_electrode = ((idx != -1) ? (string)toolStripComboBoxRef.SelectedItem : "");

            typeAxisX prev = _selectedAxisX;
            switch (toolStripComboBoxAxisX.SelectedIndex)
            {
                case 0:
                    _selectedAxisX = typeAxisX.Potential_in_mV;
                    chartVoltammogram.ChartAreas[0].AxisX.Title = "Potential / mV " + reference_electrode;// vs Fc(+/0)";
                    //chartVoltammogram.ChartAreas[0].AxisX.LabelStyle.Format = "0";
                    break;

                case 1:
                    _selectedAxisX = typeAxisX.Potential_in_V;
                    chartVoltammogram.ChartAreas[0].AxisX.Title = "Potential / V " + reference_electrode;// vs Fc(+/0)";
                    //chartVoltammogram.ChartAreas[0].AxisX.LabelStyle.Format = "0.00";
                    break;

                case 2:
                    _selectedAxisX = typeAxisX.Time_in_sec;
                    chartVoltammogram.ChartAreas[0].AxisX.Title = "Time / s";
                    //chartVoltammogram.ChartAreas[0].AxisX.LabelStyle.Format = "0";
                    break;

                case 3:
                    _selectedAxisX = typeAxisX.Time_in_hour;
                    chartVoltammogram.ChartAreas[0].AxisX.Title = "Time / h";
                    //chartVoltammogram.ChartAreas[0].AxisX.LabelStyle.Format = "0";
                    break;

                case 4:
                    _selectedAxisX = typeAxisX.ReZ_in_ohm;
                    chartVoltammogram.ChartAreas[0].AxisX.Title = "Re[Z] / ohm";
                    //chartVoltammogram.ChartAreas[0].AxisX.LabelStyle.Format = "0";
                    break;

                case 5:
                    _selectedAxisX = typeAxisX.Freq;
                    chartVoltammogram.ChartAreas[0].AxisX.Title = "Frequency / Hz";
                    //chartVoltammogram.ChartAreas[0].AxisX.LabelStyle.Format = "0";
                    break;

                /* 
                 * 軸パラメータの増やし方 (HOWTO-01)
                 * 
                 * 1. toolStripComboBoxAxisX.Itemsに追加する
                 * 
                 * 2. 以下を追加 (本関数内にもう2箇所ある)
                 * 
                 * case X:
                 *     _selectedAxisX = typeAxisX.Freq;
                 *     chartVoltammogram.ChartAreas[0].AxisX.Title = "Frequency * s";
                 *     break;  
                 *     
                 * 3. マウスカーソルによる値の表示機能にも修正を追加する
                 * 
                 * 4. AddDataToCurrentSeriesにも修正を追加 
                 *    (RescaleTicksXとの兼ね合いで、どこでIsLogarithmic = trueするのか、mixmaxをどう決めるのかは要調整 (TODO))
                 * 
                 */

                default:
                    break;
            }

            if (prev == _selectedAxisX && !force) return;
            if (_series.Count() == 0)
            {
                return;
            }

            //
            // X軸が対数軸なら、解除する必要がある
            //

            switch (_selectedAxisX)
            {
                case typeAxisX.Potential_in_mV:
                case typeAxisX.Potential_in_V:
                case typeAxisX.Time_in_sec:
                case typeAxisX.Time_in_hour:
                case typeAxisX.ReZ_in_ohm:
                    chartVoltammogram.ChartAreas[0].AxisX.IsLogarithmic = false;
                    break;

                case typeAxisX.Freq:
                    break;

                /* 
                 * 軸パラメータの増やし方 (HOWTO-01)
                 * 
                 * case typeAxisX.X:
                 *     break;     
                 * 
                 */

                default:
                    break;
            }

            _currentMinX = Double.NaN;
            _currentMaxX = Double.NaN;

            for (int i = 0; i < chartVoltammogram.Series.Count(); i++)
            {
                System.Windows.Forms.DataVisualization.Charting.DataPointCollection dpc = chartVoltammogram.Series[i].Points;
                foreach (System.Windows.Forms.DataVisualization.Charting.DataPoint itr in dpc)
                {
                    //itr.XValue = itr.XValue / _scaleAxisX[(int)prev] * _scaleAxisX[(int)_selectedAxisX];

                    switch (_selectedAxisX)
                    {
                        case typeAxisX.Potential_in_mV:
                        case typeAxisX.Potential_in_V:
                            itr.XValue = itr.YValues[POTENTIAL] / _scaleAxisX[(int)typeAxisX.Potential_in_mV] * _scaleAxisX[(int)_selectedAxisX];
                            break;

                        case typeAxisX.Time_in_sec:
                        case typeAxisX.Time_in_hour:
                            itr.XValue = itr.YValues[TIME] / _scaleAxisX[(int)typeAxisX.Time_in_sec] * _scaleAxisX[(int)_selectedAxisX];
                            break;

                        case typeAxisX.ReZ_in_ohm:
                            itr.XValue = itr.YValues[RE_Z];
                            break;

                        case typeAxisX.Freq:
                            itr.XValue = (itr.YValues[FREQ] > 0)? itr.YValues[FREQ] : 1;
                            break;

                        /* 
                         * 軸パラメータの増やし方 (HOWTO-01)
                         * 
                         * case typeAxisX.X:
                         *     itr.XValue = itr.YValues[FREQ];
                         *     break;     
                         * 
                         */

                        default:
                            break;
                    }

                    //double factor = (double)_selectedAxisX / (double)typeAxisX.Potential_in_mV;

                    if ((itr.XValue < _currentMinX) || (double.IsNaN(_currentMinX)))
                    {
                        _currentMinX = itr.XValue;
                    }
                    if ((itr.XValue > _currentMaxX) || (double.IsNaN(_currentMaxX)))
                    {
                        _currentMaxX = itr.XValue;
                    }
                }
            }

            if (double.IsNaN(_currentMinX) || double.IsNaN(_currentMaxX)) return;

            switch (_selectedAxisX)
            {
                case typeAxisX.Potential_in_mV:
                case typeAxisX.Potential_in_V:
                case typeAxisX.Time_in_sec:
                case typeAxisX.Time_in_hour:
                case typeAxisX.ReZ_in_ohm:
                    //   1. 最小値と最大値の更新 (ここでは値のリセットはしない)
                    double length = Math.Abs(_currentMaxX - _currentMinX);
                    double ext = Math.Max(length, Math.Abs(_currentMaxX)) * 0.10;
                    _currentMinX -= ext;
                    _currentMaxX += ext;

                    //double order = Math.Truncate(Math.Log10(Math.Abs(length)));
                    //double interval = Math.Pow(10, order - 1);
                    //double interval_f = Math.Log10(length) % 1; //if (interval_f < 0) interval_f = 1 - interval_f;
                    //double m = (interval_f < 0.1) ? 2.5 : ((interval_f < 0.4) ? 5 : ((interval_f < 0.7) ? 10 : 20));

                    //_currentMinX = Math.Sign(_currentMinX) * Math.Ceiling(Math.Abs(_currentMinX) / (interval * m)) * (interval * m);
                    //_currentMaxX = Math.Sign(_currentMaxX) * Math.Ceiling(Math.Abs(_currentMaxX) / (interval * m)) * (interval * m);

                    //   2. 最小値と最大値に基づく目盛り間隔の更新
                    length = Math.Abs(_currentMaxX - _currentMinX);
                    RescaleTicksX(length, _currentMinX);
                    break;

                case typeAxisX.Freq:
                    RescaleTicksX(Math.Abs(_currentMaxX - _currentMinX), _currentMinX);
                    break;

                /* 
                 * 軸パラメータの増やし方 (HOWTO-01)
                 * 
                 * case typeAxisX.X:
                 *     break;     
                 * 
                 */

                default:
                    break;
            }
        }

        private void ChangeAxisY(bool force)
        {
            typeAxisY prev = _selectedAxisY;
            switch (toolStripComboBoxAxisY.SelectedIndex)
            {
                case 5:
                    _selectedAxisY = typeAxisY.Current_in_nA;
                    chartVoltammogram.ChartAreas[0].AxisY.Title = "Current / nA";
                    break;
                case 0:
                    _selectedAxisY = typeAxisY.Current_in_uA;
                    chartVoltammogram.ChartAreas[0].AxisY.Title = "Current / μA";
                    break;
                case 1:
                    _selectedAxisY = typeAxisY.Current_in_mA;
                    chartVoltammogram.ChartAreas[0].AxisY.Title = "Current / mA";
                    break;
                case 2:
                    _selectedAxisY = typeAxisY.Coulomb_in_mC;
                    chartVoltammogram.ChartAreas[0].AxisY.Title = "Coulomb / mC";
                    break;
                case 3:
                    _selectedAxisY = typeAxisY.Coulomb_in_C;
                    chartVoltammogram.ChartAreas[0].AxisY.Title = "Coulomb / C";
                    break;
                case 4:
                    _selectedAxisY = typeAxisY.Potential_in_mV;
                    chartVoltammogram.ChartAreas[0].AxisY.Title = "Potential / mV";
                    break;
                case 6:
                    _selectedAxisY = typeAxisY.ImZ_in_ohm;
                    chartVoltammogram.ChartAreas[0].AxisY.Title = "Im[Z] / ohm";
                    break;
                case 7:
                    _selectedAxisY = typeAxisY.C2_in_MS_plot;
                    chartVoltammogram.ChartAreas[0].AxisY.Title = "(Im[Z] * 2πf)^2 / mF^(-2)";
                    break;
                case 8:
                    _selectedAxisY = typeAxisY.Z_in_ohm;
                    chartVoltammogram.ChartAreas[0].AxisY.Title = "log(|Z| / ohm)";
                    break;

                default:
                    break;
            }

            if (prev == _selectedAxisY && !force) return;
            if (_series.Count() == 0) return;

            chartVoltammogram.SuspendLayout();

            _currentMinY = Double.NaN;
            _currentMaxY = Double.NaN;

            for (int i = 0; i < chartVoltammogram.Series.Count(); i++)
            {
                System.Windows.Forms.DataVisualization.Charting.DataPointCollection dpc = chartVoltammogram.Series[i].Points;
                foreach (System.Windows.Forms.DataVisualization.Charting.DataPoint itr in dpc)
                {
                    //itr.YValues[0] = itr.YValues[0] / _scaleAxisY[(int)prev] * _scaleAxisY[(int)_selectedAxisY];

                    switch (_selectedAxisY)
                    {
                        case typeAxisY.Current_in_nA:
                        case typeAxisY.Current_in_uA:
                        case typeAxisY.Current_in_mA:
                            itr.YValues[0] = itr.YValues[CURRENT] / _scaleAxisY[(int)typeAxisY.Current_in_uA] * _scaleAxisY[(int)_selectedAxisY];
                            break;

                        case typeAxisY.Coulomb_in_mC:
                        case typeAxisY.Coulomb_in_C:
                            itr.YValues[0] = itr.YValues[COULOMB] / _scaleAxisY[(int)typeAxisY.Coulomb_in_mC] * _scaleAxisY[(int)_selectedAxisY];
                            break;

                        case typeAxisY.Potential_in_mV:
                            itr.YValues[0] = itr.YValues[POTENTIAL] / _scaleAxisY[(int)typeAxisY.Potential_in_mV] * _scaleAxisY[(int)_selectedAxisY];
                            break;

                        case typeAxisY.ImZ_in_ohm:
                            itr.YValues[0] = itr.YValues[IM_Z];
                            break;

                        case typeAxisY.C2_in_MS_plot:
                            itr.YValues[0] = Math.Pow(itr.YValues[IM_Z] * itr.YValues[FREQ] * 2 * Math.PI, 2) * 1e-6;
                            break;

                        case typeAxisY.Z_in_ohm:
                            itr.YValues[0] = 0.5 * Math.Log10((Math.Pow(itr.YValues[RE_Z],2) + Math.Pow(itr.YValues[IM_Z], 2)));
                            break;

                        default:
                            break;
                    }

                    if ((itr.YValues[0] < _currentMinY) || (double.IsNaN(_currentMinY)))
                    {
                        _currentMinY = itr.YValues[0];
                    }
                    if ((itr.YValues[0] > _currentMaxY) || (double.IsNaN(_currentMaxY)))
                    {
                        _currentMaxY = itr.YValues[0];
                    }
                }
            }

            if (double.IsNaN(_currentMinY) || double.IsNaN(_currentMaxY)) return;

            //switch (_selectedAxisY)
            //{
            //    case typeAxisY.Current_in_nA:
            //    case typeAxisY.Current_in_uA:
            //    case typeAxisY.Current_in_mA:
            //    case typeAxisY.Coulomb_in_mC:
            //    case typeAxisY.Coulomb_in_C:
            //    case typeAxisY.Potential_in_mV:
            //    case typeAxisY.ImZ_in_ohm:
            //    case typeAxisY.C2_in_MS_plot:

                    //   1. 最小値と最大値の更新 (ここでは値のリセットはしない)
                    double length = Math.Abs(_currentMaxY - _currentMinY);
                    double ext = Math.Max(length, Math.Abs(_currentMaxY)) * 0.10;
                    _currentMinY -= ext;
                    _currentMaxY += ext;

                    chartVoltammogram.ChartAreas[0].RecalculateAxesScale();
                    //chartVoltammogram.ResetAutoValues();

                    //   2. 最小値と最大値に基づく目盛り間隔の更新
                    length = Math.Abs(_currentMaxY - _currentMinY);
                    RescaleTicksY(length, _currentMinY);

            //        break;

            //    case typeAxisY.Z_in_ohm:
            //        RescaleTicksY(Math.Abs(_currentMaxY - _currentMinY), _currentMinY);
            //        break;

            //    default:
            //        break;
            //}

            chartVoltammogram.ResumeLayout();
            chartVoltammogram.PerformLayout();
            chartVoltammogram.Update();
        }

        public void SetFont(double title, double label)
        {
            chartVoltammogram.ChartAreas[0].AxisX.TitleFont = new Font(chartVoltammogram.ChartAreas[0].AxisX.TitleFont.OriginalFontName, (float)title, FontStyle.Bold);
            chartVoltammogram.ChartAreas[0].AxisX.LabelStyle.Font = new Font(chartVoltammogram.ChartAreas[0].AxisX.TitleFont.OriginalFontName, (float)label, FontStyle.Regular);
            chartVoltammogram.ChartAreas[0].AxisY.TitleFont = new Font(chartVoltammogram.ChartAreas[0].AxisX.TitleFont.OriginalFontName, (float)title, FontStyle.Bold);
            chartVoltammogram.ChartAreas[0].AxisY.LabelStyle.Font = new Font(chartVoltammogram.ChartAreas[0].AxisX.TitleFont.OriginalFontName, (float)label, FontStyle.Regular);
        }

        public void SetTickDirection(int major, int minor)
        {
            chartVoltammogram.ChartAreas[0].AxisX.MajorTickMark.TickMarkStyle = (System.Windows.Forms.DataVisualization.Charting.TickMarkStyle)major;
            chartVoltammogram.ChartAreas[0].AxisX.MinorTickMark.TickMarkStyle = (System.Windows.Forms.DataVisualization.Charting.TickMarkStyle)minor;
            chartVoltammogram.ChartAreas[0].AxisX2.MajorTickMark.TickMarkStyle = (System.Windows.Forms.DataVisualization.Charting.TickMarkStyle)major;
            chartVoltammogram.ChartAreas[0].AxisX2.MinorTickMark.TickMarkStyle = (System.Windows.Forms.DataVisualization.Charting.TickMarkStyle)minor;
            chartVoltammogram.ChartAreas[0].AxisY.MajorTickMark.TickMarkStyle = (System.Windows.Forms.DataVisualization.Charting.TickMarkStyle)major;
            chartVoltammogram.ChartAreas[0].AxisY.MinorTickMark.TickMarkStyle = (System.Windows.Forms.DataVisualization.Charting.TickMarkStyle)minor;
            chartVoltammogram.ChartAreas[0].AxisY2.MajorTickMark.TickMarkStyle = (System.Windows.Forms.DataVisualization.Charting.TickMarkStyle)major;
            chartVoltammogram.ChartAreas[0].AxisY2.MinorTickMark.TickMarkStyle = (System.Windows.Forms.DataVisualization.Charting.TickMarkStyle)minor;
        }

        public void SetSizeOfPlotArea(bool manual, double width = 0, double height = 0)
        {
            if (manual)
            {
                //chartVoltammogram.ChartAreas[0].Position.Auto = false;
                //chartVoltammogram.ChartAreas[0].InnerPlotPosition. = (float)height;
                //chartVoltammogram.ChartAreas[0].InnerPlotPosition.Width = (float)width;

                chartVoltammogram.Dock = DockStyle.None;

                chartVoltammogram.ChartAreas[0].Position.Auto = false;
                chartVoltammogram.ChartAreas[0].InnerPlotPosition.Auto = false;
                chartVoltammogram.Width = (int)Math.Round(width / ((chartVoltammogram.ChartAreas[0].InnerPlotPosition.Width - chartVoltammogram.Legends[0].Position.Width)*1.02 / 100));
                chartVoltammogram.Height = (int)Math.Round(height / ((chartVoltammogram.ChartAreas[0].InnerPlotPosition.Height) / 100));
                chartVoltammogram.Update();
                chartVoltammogram.UpdateAnnotations();
                chartVoltammogram.PerformLayout();
                //chartVoltammogram.ChartAreas[0].Position.Auto = true;
                //chartVoltammogram.Width = (int)Math.Round(width / (chartVoltammogram.ChartAreas[0].Position.Width / 100));
                //chartVoltammogram.Height = (int)Math.Round(height / (chartVoltammogram.ChartAreas[0].Position.Height / 100));
            }
            else
            {
                chartVoltammogram.Dock = DockStyle.Fill;
                chartVoltammogram.ChartAreas[0].Position.Auto = true;
                chartVoltammogram.ChartAreas[0].InnerPlotPosition.Auto = true;
                chartVoltammogram.Update();
                chartVoltammogram.UpdateAnnotations();
                chartVoltammogram.PerformLayout();
            }
        }

        private void updateSelectionOfSeries()
        {
            int idx = toolStripComboBoxSeries.SelectedIndex;

            int idxColor = toolStripComboBoxSeriesColor.Items.IndexOf(chartVoltammogram.Series[idx].Color);
            toolStripComboBoxSeriesColor.SelectedIndex = idxColor;

            int idxLineStyle = toolStripComboBoxSeriesLineStyle.Items.IndexOf(chartVoltammogram.Series[idx].BorderDashStyle);
            toolStripComboBoxSeriesLineStyle.SelectedIndex = idxLineStyle;

            int idxLineWidth = toolStripComboBoxSeriesLineWidth.Items.IndexOf(chartVoltammogram.Series[idx].BorderWidth);
            toolStripComboBoxSeriesLineWidth.SelectedIndex = idxLineWidth;

            int idxRefP = toolStripComboBoxSeriesRefPotential.Items.IndexOf(((XMLDataHolder)chartVoltammogram.Series[idx].Tag).GetDatum("reference", "0"));
            toolStripComboBoxSeriesRefPotential.SelectedIndex = idxRefP;

            //XmlNodeList xmllist = ((XMLDataHolder)chartVoltammogram.Series[idx].Tag).GetData("conditions");
            //if(xmllist.Count == 1)
            //{
            //    _show_information.SetInformation(xmllist[0]);
            //}
            _show_information.SetInformation(((XMLDataHolder)chartVoltammogram.Series[idx].Tag).GetData());
        }

        private void undoZoom()
        {
            if (_currentIndex == -1) return;

            chartVoltammogram.SuspendLayout();

            chartVoltammogram.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
            chartVoltammogram.ChartAreas[0].AxisX2.ScaleView.ZoomReset(0);
            chartVoltammogram.ChartAreas[0].AxisY.ScaleView.ZoomReset(0);
            chartVoltammogram.ChartAreas[0].AxisY2.ScaleView.ZoomReset(0);

            //   X軸
            double length = chartVoltammogram.ChartAreas[0].AxisX.ScaleView.Size;
            if (Double.IsNaN(length)) length = (_currentMaxX - _currentMinX);
            double p = chartVoltammogram.ChartAreas[0].AxisX.ScaleView.Position;
            if (Double.IsNaN(p)) p = (_currentMinX);
            RescaleTicksX(length, p);

            //   Y軸
            length = chartVoltammogram.ChartAreas[0].AxisY.ScaleView.Size;
            if (Double.IsNaN(length)) length = (_currentMaxY - _currentMinY);
            p = chartVoltammogram.ChartAreas[0].AxisY.ScaleView.Position;
            if (Double.IsNaN(p)) p = (_currentMinY);
            RescaleTicksY(length, p);

            chartVoltammogram.ResumeLayout();
            chartVoltammogram.PerformLayout();
        }

        //
        // Event Handler
        //

        private void toolStripComboBoxSeries_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateSelectionOfSeries();
        }

        private void toolStripComboBoxSeriesColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = toolStripComboBoxSeries.SelectedIndex;
            int idxColor = toolStripComboBoxSeriesColor.SelectedIndex;

            chartVoltammogram.Series[idx].Color = (Color)toolStripComboBoxSeriesColor.Items[idxColor];
        }

        private void toolStripComboBoxSeriesLineStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = toolStripComboBoxSeries.SelectedIndex;
            int idxLs = toolStripComboBoxSeriesLineStyle.SelectedIndex;

            chartVoltammogram.Series[idx].BorderDashStyle = (System.Windows.Forms.DataVisualization.Charting.ChartDashStyle)toolStripComboBoxSeriesLineStyle.Items[idxLs];

            switch (chartVoltammogram.Series[idx].BorderDashStyle)
            {
                case System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash:
                    chartVoltammogram.Series[idx].BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.NotSet;
                    chartVoltammogram.Series[idx].MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
                    chartVoltammogram.Series[idx].MarkerStep = (int)Math.Round(100 / Properties.Settings.Default.line_style_1);
                    chartVoltammogram.Series[idx].MarkerSize = 2;
                    break;

                case System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.DashDot:
                    chartVoltammogram.Series[idx].BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.NotSet;
                    chartVoltammogram.Series[idx].MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
                    chartVoltammogram.Series[idx].MarkerStep = (int)Math.Round(100 / Properties.Settings.Default.line_style_2);
                    chartVoltammogram.Series[idx].MarkerSize = 2;
                    break;

                case System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.DashDotDot:
                    chartVoltammogram.Series[idx].BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.NotSet;
                    chartVoltammogram.Series[idx].MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
                    chartVoltammogram.Series[idx].MarkerStep = (int)Math.Round(100 / Properties.Settings.Default.line_style_3);
                    chartVoltammogram.Series[idx].MarkerSize = 2;
                    break;

                default:
                    chartVoltammogram.Series[idx].MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.None;
                    break;
            }
        }

        private void toolStripComboBoxSeriesLineWidth_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = toolStripComboBoxSeries.SelectedIndex;
            int idxLw = toolStripComboBoxSeriesLineWidth.SelectedIndex;

            chartVoltammogram.Series[idx].BorderWidth = (int)toolStripComboBoxSeriesLineWidth.Items[idxLw];
        }

        private void toolStripComboBoxSeriesRefPotential_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = toolStripComboBoxSeries.SelectedIndex;
            int idxRefP = toolStripComboBoxSeriesRefPotential.SelectedIndex;

            if (idxRefP == -1) return;
            //toolStripComboBoxSeriesRefPotential.Items[idxRefP].GetType;
            int original = int.Parse(((XMLDataHolder)chartVoltammogram.Series[idx].Tag).GetDatum("reference", "0"));
            if (!int.TryParse(toolStripComboBoxSeriesRefPotential.Items[idxRefP].ToString(), out int set)) return;
            if (original == set) return;

            switch (_selectedAxisX)
            {
                case typeAxisX.Potential_in_mV:
                case typeAxisX.Potential_in_V:
                    {
                        if(false)
                        {
                            //_currentMinX = Double.NaN;
                            //_currentMaxX = Double.NaN;

                            //System.Windows.Forms.DataVisualization.Charting.DataPointCollection dpc = chartVoltammogram.Series[idx].Points;
                            //foreach (System.Windows.Forms.DataVisualization.Charting.DataPoint itr in dpc)
                            //{
                            //    //itr.XValue = itr.XValue + (((original) - (set)) / _scaleAxisX[(int)typeAxisX.Potential_in_mV] * _scaleAxisX[(int)_selectedAxisX]);

                            //    itr.YValues[POTENTIAL] += (original - set); // [mV]
                            //    itr.XValue = itr.YValues[POTENTIAL] / _scaleAxisX[(int)typeAxisX.Potential_in_mV] * _scaleAxisX[(int)_selectedAxisX];

                            //    if ((itr.XValue < _currentMinX) || (double.IsNaN(_currentMinX)))
                            //    {
                            //        _currentMinX = itr.XValue;
                            //    }
                            //    if ((itr.XValue > _currentMaxX) || (double.IsNaN(_currentMaxX)))
                            //    {
                            //        _currentMaxX = itr.XValue;
                            //    }
                            //}

                            ////   1. 最小値と最大値の更新 (ここでは値のリセットはしない)
                            //double length = Math.Abs(_currentMaxX - _currentMinX);
                            //double ext = length * 0.10;

                            //_currentMinX -= ext;
                            //_currentMaxX += ext;

                            ////chartVoltammogram.ChartAreas[0].RecalculateAxesScale();

                            ////if (chartVoltammogram.ChartAreas[0].AxisX.Minimum < _currentMinX) _currentMinX = chartVoltammogram.ChartAreas[0].AxisX.Minimum;
                            ////if (chartVoltammogram.ChartAreas[0].AxisX.Maximum > _currentMaxX) _currentMaxX = chartVoltammogram.ChartAreas[0].AxisX.Maximum;

                            ////   2. 最小値と最大値に基づく目盛り間隔の更新
                            //length = Math.Abs(_currentMaxX - _currentMinX);
                            //RescaleTicksX(length, _currentMinX);
                        }
                        else
                        {
                            System.Windows.Forms.DataVisualization.Charting.DataPointCollection dpc = chartVoltammogram.Series[idx].Points;
                            foreach (System.Windows.Forms.DataVisualization.Charting.DataPoint itr in dpc)
                            {
                                itr.YValues[POTENTIAL] += (original - set); // [mV]
                                itr.XValue = itr.YValues[POTENTIAL] / _scaleAxisX[(int)typeAxisX.Potential_in_mV] * _scaleAxisX[(int)_selectedAxisX];
                            }

                            ChangeAxisX(true);
                        }
                    }
                    break;

                case typeAxisX.Time_in_sec:
                    {
                        System.Windows.Forms.DataVisualization.Charting.DataPointCollection dpc = chartVoltammogram.Series[idx].Points;
                        foreach (System.Windows.Forms.DataVisualization.Charting.DataPoint itr in dpc)
                        {
                            itr.YValues[POTENTIAL] += (original - set) / _scaleAxisX[(int)typeAxisX.Potential_in_mV];
                        }
                    }
                    break;

                default:
                    break;
            }

            string str = toolStripComboBoxSeriesRefPotential.Items[idxRefP].ToString();
            ((XMLDataHolder)chartVoltammogram.Series[idx].Tag).SetDatum("reference", str);
            chartVoltammogram.Series[idx].Name// = _series[idx] + " (" + str + ")";
                = _series[idx] + " (" + str + ")" + " x" + ((XMLDataHolder)chartVoltammogram.Series[idx].Tag).GetDatum("scaleY", "1.0") + " +" + ((XMLDataHolder)chartVoltammogram.Series[idx].Tag).GetDatum("shiftY", "0") + "uA";
        }

        private void toolStripComboBoxSeriesRefPotential_TextUpdate(object sender, EventArgs e)
        {
            //Console.WriteLine("Tup: " + toolStripComboBoxSeriesRefPotential.Text);
        }

        private void toolStripComboBoxSeriesRefPotential_TextChanged(object sender, EventArgs e)
        {
            //Console.WriteLine("Tchged: " + toolStripComboBoxSeriesRefPotential.Text);

        }

        private void toolStripComboBoxSeriesRefPotential_Validating(object sender, CancelEventArgs e)
        {
            //System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^[+-]?(?:\d+\.?\d*|\.\d+)$");
            //if (!regex.IsMatch(((ToolStripComboBox)sender).Text))
            //{
            //    //this.errorProvider1.SetError((TextBox)sender, "a-z のみの文字列");
            //    // e.Cancel = true　でCancel を true にすると正しく入力しないと次に行けない。
            //    e.Cancel = true;
            //}
        }

        private void toolStripComboBoxSeriesRefPotential_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter || e.KeyChar == (char)Keys.Escape)
            {
                if (Double.TryParse(((ToolStripComboBox)sender).Text, out double ret))
                {
                    int idx = toolStripComboBoxSeriesRefPotential.Items.Add(Math.Round(ret));
                    toolStripComboBoxSeriesRefPotential.SelectedIndex = idx;
                }
                else
                {
                    ((ToolStripComboBox)sender).Text = "0";
                }

                e.Handled = true;
            }
        }

        private void toolStripComboSeriesShiftY_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = toolStripComboBoxSeries.SelectedIndex;
            int idxRefP = toolStripComboSeriesShiftY.SelectedIndex;

            if (idxRefP == -1) return;

            double original = double.Parse(((XMLDataHolder)chartVoltammogram.Series[idx].Tag).GetDatum("shiftY", "0"));
            double set = (double)toolStripComboSeriesShiftY.Items[idxRefP];
            if (original == set) return;

            switch (_selectedAxisY)
            {
                case typeAxisY.Current_in_mA:
                case typeAxisY.Current_in_uA:
                case typeAxisY.Current_in_nA:
                    {
                        _currentMinY = Double.NaN;
                        _currentMaxY = Double.NaN;

                        System.Windows.Forms.DataVisualization.Charting.DataPointCollection dpc = chartVoltammogram.Series[idx].Points;
                        foreach (System.Windows.Forms.DataVisualization.Charting.DataPoint itr in dpc)
                        {
                            //itr.YValues[0] = itr.YValues[0] + ((-(original) + (set)) / _scaleAxisY[(int)typeAxisY.Current_in_uA]) * _scaleAxisY[(int)_selectedAxisY];

                            itr.YValues[CURRENT] += (-original + set); // [uA]
                            itr.YValues[0] = itr.YValues[CURRENT] / _scaleAxisY[(int)typeAxisY.Current_in_uA] * _scaleAxisY[(int)_selectedAxisY];

                            if ((itr.YValues[0] < _currentMinY) || (double.IsNaN(_currentMinY)))
                            {
                                _currentMinY = itr.YValues[0];
                            }
                            if ((itr.YValues[0] > _currentMaxY) || (double.IsNaN(_currentMaxY)))
                            {
                                _currentMaxY = itr.YValues[0];
                            }
                        }

                        //   1. 最小値と最大値の更新 (ここでは値のリセットはしない)
                        double length = Math.Abs(_currentMaxY - _currentMinY);
                        double ext = Math.Max(length, Math.Abs(_currentMaxY)) * 0.10;

                        _currentMinY -= ext;
                        _currentMaxY += ext;

                        chartVoltammogram.ChartAreas[0].RecalculateAxesScale();

                        //   2. 最小値と最大値に基づく目盛り間隔の更新
                        length = Math.Abs(_currentMaxY - _currentMinY);
                        RescaleTicksY(length, _currentMinY);
                    }
                    break;

                case typeAxisY.Coulomb_in_mC:
                    {
                        System.Windows.Forms.DataVisualization.Charting.DataPointCollection dpc = chartVoltammogram.Series[idx].Points;
                        foreach (System.Windows.Forms.DataVisualization.Charting.DataPoint itr in dpc)
                        {
                            itr.YValues[CURRENT] += (-original + set) / _scaleAxisY[(int)typeAxisY.Current_in_uA];
                        }

                    }
                    break;

                default:
                    break;
            }

            string str = toolStripComboSeriesShiftY.Items[idxRefP].ToString();
            ((XMLDataHolder)chartVoltammogram.Series[idx].Tag).SetDatum("shiftY", str);
            chartVoltammogram.Series[idx].Name
                = _series[idx] + " (" + ((XMLDataHolder)chartVoltammogram.Series[idx].Tag).GetDatum("reference", "0") + ")" + " x" + ((XMLDataHolder)chartVoltammogram.Series[idx].Tag).GetDatum("scaleY", "1.0") + " +" + str + "uA";
        }

        private void toolStripComboSeriesShiftY_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter || e.KeyChar == (char)Keys.Escape)
            {
                if (double.TryParse(((ToolStripComboBox)sender).Text, out double ret))
                {
                    int idx = toolStripComboSeriesShiftY.Items.Add(ret);
                    toolStripComboSeriesShiftY.SelectedIndex = idx;
                }
                else
                {
                    ((ToolStripComboBox)sender).Text = "0";
                }

                e.Handled = true;
            }
        }

        private void toolStripComboBoxAxisY_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChangeAxisY(force: false);
        }

        private void toolStripComboBoxAxisX_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChangeAxisX(force:false);
        }

        private void toolStripComboBoxRef_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = toolStripComboBoxRef.SelectedIndex;
            string reference_electrode = ((idx != -1) ? (string)toolStripComboBoxRef.SelectedItem : "");

            switch (_selectedAxisX)
            {
                case typeAxisX.Potential_in_mV:
                    chartVoltammogram.ChartAreas[0].AxisX.Title = "Potential / mV " + reference_electrode;// vs Fc(+/0)";
                    break;

                case typeAxisX.Potential_in_V:
                    chartVoltammogram.ChartAreas[0].AxisX.Title = "Potential / V " + reference_electrode;// vs Fc(+/0)";
                    break;

                default:
                    break;
            }

            //switch (toolStripComboBoxRef.SelectedIndex)
            //{
            //    case 0:
            //        break;

            //    case 1:
            //        break;

            //    case 2:
            //        break;

            //    default:
            //        break;
            //}
        }

        private void toolStripButtonNewWindow_Click(object sender, EventArgs e)
        {
            formVoltammogram v = new formVoltammogram();
            v.Show();
        }

        private void toolStripMenuItemLoadAsVoltammogram_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "EC-Lab text; Voltammogrammer; Hokuto (*.mpt;*.volta;*.voltax;*.txt)|*.mpt;*.volta;*.voltax;*.txt|EC-Lab text file (*.mpt)|*.mpt|Voltammogrammer file (*.volta)|*.volta|Voltammogrammer file, compressed (*.voltax)|*.voltax|Hokuto text file (*.txt)|*.txt|All types(*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Title = "Open File As Voltammogram";
            openFileDialog1.FileName = "";
            openFileDialog1.ShowHelp = true;
            openFileDialog1.SupportMultiDottedExtensions = true;

            if (openFileDialog1.InitialDirectory == null)
            {
                openFileDialog1.InitialDirectory = @"c:\";
            }

            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                for (int i = 0; i < openFileDialog1.FileNames.Length; i++)
                {
                    string _file_path = openFileDialog1.FileNames[i];
                    LoadFile(_file_path);
                }
            }
        }

        private void toolStripMenuItemAsChronoamperogram_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "EC-Lab Text; (*.mpt)|*.mpt|All types(*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Title = "Open File As Chronoamperogram";
            openFileDialog1.FileName = "";
            openFileDialog1.ShowHelp = true;

            if (openFileDialog1.InitialDirectory == null)
            {
                openFileDialog1.InitialDirectory = @"c:\";
            }

            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                for (int i = 0; i < openFileDialog1.FileNames.Length; i++)
                {
                    string file_path = openFileDialog1.FileNames[i];

                    string dir = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(file_path));
                    string item = dir + "/" + System.IO.Path.GetFileName(file_path);
                    string ext = System.IO.Path.GetExtension(file_path);

                    if (!_mode_coulomb_counting)
                    {
                        //このインスタンスはvoltammogramモードにあるので、新しいwindowで読み込む
                        _chronoamperogram.LoadSingleVoltammogram(item, file_path);
                        _chronoamperogram.Show();
                    }
                    else
                    {
                        LoadSingleVoltammogram(item, file_path);
                    }
                }
            }
        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            //// Windowsフォームのインスタンスを作成
            ////var dialog = new SearchReplaceWindow();
            //// Excelのウィンドウハンドルを取得
            //var handle = Process.GetCurrentProcess().MainWindowHandle;
            ////var owner = Control.FromHandle(handle);
            //// オーナーを指定してダイアログの表示
            ////dialog.Show(owner);

            CommonSaveFileDialog dialog = new CommonSaveFileDialog();
            dialog.Filters.Add(new CommonFileDialogFilter("Voltammogrammer file", "*.volta"));
            dialog.Filters.Add(new CommonFileDialogFilter("Voltammogrammer file, compressed", "*.voltax"));
            dialog.Filters.Add(new CommonFileDialogFilter("All types", "*.*"));
            dialog.Title = "Save As";
            //dialog.SelectedFileTypeIndex
            //dialog.AlwaysAppendDefaultExtension = true;
            //dialog.DefaultExtension = "volta";
            dialog.OverwritePrompt = true;

            //CommonFileDialogComboBox comboBox = new CommonFileDialogComboBox();
            //comboBox.Items.Add(new CommonFileDialogComboBoxItem("ペンギン"));
            //comboBox.Items.Add(new CommonFileDialogComboBoxItem("くじら"));
            //comboBox.Items.Add(new CommonFileDialogComboBoxItem("ハシビロコウ"));
            //comboBox.SelectedIndex = 0;
            //dialog.Controls.Add(comboBox);

            //CommonFileDialogCheckBox chkBoxDataReduction = new CommonFileDialogCheckBox("Data decimation (1/5)");
            CommonFileDialogComboBox chkBoxDataReductionCombo = new CommonFileDialogComboBox();
            chkBoxDataReductionCombo.Items.Add(new CommonFileDialogComboBoxItem("No data decimation"));
            chkBoxDataReductionCombo.Items.Add(new CommonFileDialogComboBoxItem("Data decimation (1/5)"));
            chkBoxDataReductionCombo.Items.Add(new CommonFileDialogComboBoxItem("Data decimation (1/10)"));
            chkBoxDataReductionCombo.Items.Add(new CommonFileDialogComboBoxItem("Data decimation (1/25)"));
            chkBoxDataReductionCombo.Items.Add(new CommonFileDialogComboBoxItem("Data decimation (1/100)"));
            chkBoxDataReductionCombo.SelectedIndex = 0;
            //CommonFileDialogCheckBox chkBoxForExcel = new CommonFileDialogCheckBox("For Excel");
            dialog.Controls.Add(chkBoxDataReductionCombo);
            //dialog.Controls.Add(chkBoxDataReduction2);
            //dialog.Controls.Add(chkBoxForExcel);

            //if (dialog.DefaultDirectory == null)
            //{
            //    dialog.InitialDirectory = @"c:\";
            //}

            if (dialog.ShowDialog(this.Handle) == CommonFileDialogResult.Ok)
            {
                string file_path = dialog.FileName;
                string ext = System.IO.Path.GetExtension(file_path);
                if(ext == "")
                {
                    switch(dialog.SelectedFileTypeIndex)
                    {
                        case 1: file_path += ".volta";  break;
                        case 2: file_path += ".voltax";  break;
                        default: break;
                    }
                }
                bool compressed = (dialog.SelectedFileTypeIndex == 2) ? true : false;

                int reduction_level = 0;
                switch (chkBoxDataReductionCombo.SelectedIndex)
                {
                    case 0: reduction_level = 1; break;
                    case 1: reduction_level = 5; break;
                    case 2: reduction_level = 10; break;
                    case 3: reduction_level = 25; break;
                    case 4: reduction_level = 100; break;
                }

                SaveData(file_path, reduction_level, compressed);

                _filename_saved = System.IO.Path.GetFileName(file_path);
                this.Text = _filename_saved + " - Actual Voltammogram";
            }
        }

        private void toolStripMenuItemExportCSV_Click(object sender, EventArgs e)
        {
            CommonSaveFileDialog dialog = new CommonSaveFileDialog();
            dialog.Filters.Add(new CommonFileDialogFilter("CSV file", "*.csv"));
            dialog.Filters.Add(new CommonFileDialogFilter("All types", "*.*"));
            dialog.Title = "Export in the CSV format";
            dialog.AlwaysAppendDefaultExtension = true;
            dialog.DefaultExtension = "csv";

            //CommonFileDialogComboBox comboBox = new CommonFileDialogComboBox();
            //comboBox.Items.Add(new CommonFileDialogComboBoxItem("ペンギン"));
            //comboBox.Items.Add(new CommonFileDialogComboBoxItem("くじら"));
            //comboBox.Items.Add(new CommonFileDialogComboBoxItem("ハシビロコウ"));
            //comboBox.SelectedIndex = 0;
            //dialog.Controls.Add(comboBox);

            CommonFileDialogCheckBox chkBoxDataReduction = new CommonFileDialogCheckBox("Data Decimation (1/5)");
            CommonFileDialogCheckBox chkBoxForExcel = new CommonFileDialogCheckBox("For Excel");
            dialog.Controls.Add(chkBoxDataReduction);
            dialog.Controls.Add(chkBoxForExcel);

            //if (dialog.InitialDirectory == null)
            //{
            //    dialog.InitialDirectory = @"c:\";
            //}

            if (dialog.ShowDialog(this.Handle) == CommonFileDialogResult.Ok)
            {
                string file_path = dialog.FileName;

                ExportDataAsCSV(file_path, chkBoxDataReduction.IsChecked, chkBoxForExcel.IsChecked);
            }

            //_export_data.Show();
        }

        private void toolStripMenuItemExportSinglePlot_Click(object sender, EventArgs e)
        {
            int idx = toolStripComboBoxSeries.SelectedIndex;
            if (idx == -1) return;

            saveFileDialog1.Filter = "MPT file (*.mpt)|*.mpt|All types(*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.Title = "Export as an EC-Lab Text file";
            saveFileDialog1.FileName = "";
            //saveFileDialog1.ShowHelp = true;
            saveFileDialog1.AddExtension = true;

            if (saveFileDialog1.InitialDirectory == null)
            {
                saveFileDialog1.InitialDirectory = @"c:\";
            }

            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                string file_path = saveFileDialog1.FileName;

                TextWriter writer = new StreamWriter(file_path, false);

                writer.WriteLine("EC-Lab ASCII FILE");
                XmlNodeList xnl = ((XMLDataHolder)chartVoltammogram.Series[idx].Tag).GetData("experimantal-conditions");
                if (xnl.Count == 1)
                {
                    writer.WriteLine("Nb header lines : 4");
                    writer.WriteLine(xnl[0].OuterXml);
                }
                else
                {
                    writer.WriteLine("Nb header lines : 3");
                }
                writer.WriteLine("Ewe/V	I/mA	time/s");

                for (int i = 1; i < chartVoltammogram.Series[idx].Points.Count; i++)
                {
                    writer.Write(
                        "{0}\t{1}\t{2}",
                        chartVoltammogram.Series[idx].Points[i].YValues[POTENTIAL] / _scaleAxisX[(int)typeAxisX.Potential_in_mV] * _scaleAxisX[(int)typeAxisX.Potential_in_V],
                        chartVoltammogram.Series[idx].Points[i].YValues[CURRENT] / _scaleAxisY[(int)typeAxisY.Current_in_uA] * _scaleAxisY[(int)typeAxisY.Current_in_mA],
                        chartVoltammogram.Series[idx].Points[i].YValues[TIME]
                    );
                    writer.WriteLine();
                }

                writer.Close();
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            RemoveSeries(-1);

            SetAxisRangeX(true, 0, 0);
            SetAxisRangeY(true, 0, 0);
        }

        private void toolStripMenuItem_Series_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(((ToolStripMenuItem)sender).Text);

            //string name = ((ToolStripMenuItem)sender).Text;
            //int idx = _series.IndexOf(name)
            ToolStripItemCollection tsic = toolStripDropDownButton2.DropDownItems;
            int idx = tsic.IndexOf((ToolStripMenuItem)sender) - 2;

            RemoveSeries(idx);
        }

        private void toolStripMenuItemShowInformation_Click(object sender, EventArgs e)
        {
            _show_information.Show();
        }

        private void toolStripComboSeriesScaleY_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter || e.KeyChar == (char)Keys.Escape)
            {
                if (Double.TryParse(((ToolStripComboBox)sender).Text, out double ret))
                {
                    int idx = toolStripComboSeriesScaleY.Items.Add(ret);
                    toolStripComboSeriesScaleY.SelectedIndex = idx;
                }
                else
                {
                    //if(((ToolStripComboBox)sender).Text.ToLower() == "ms")
                    //{
                    //    int idx = toolStripComboSeriesScaleY.Items.Add("ms");
                    //    toolStripComboSeriesScaleY.SelectedIndex = idx;
                    //}
                    //else
                    //{ 
                        ((ToolStripComboBox)sender).Text = "1.0";
                    //}
                }

                e.Handled = true;
            }
        }

        private void toolStripComboSeriesScaleY_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = toolStripComboBoxSeries.SelectedIndex;
            int idxRefP = toolStripComboSeriesScaleY.SelectedIndex;

            if (idxRefP == -1) return;

            string original_str = (((XMLDataHolder)chartVoltammogram.Series[idx].Tag).GetDatum("scaleY", "1.0"));
            double originalShift = Double.Parse(((XMLDataHolder)chartVoltammogram.Series[idx].Tag).GetDatum("shiftY", "0"));
            string set_str = (string)toolStripComboSeriesScaleY.Items[idxRefP];
            if (original_str == set_str) return;

            double original = 1.0, set = 1.0;
            switch (_selectedAxisY)
            {
                case typeAxisY.Current_in_mA:
                case typeAxisY.Current_in_uA:
                case typeAxisY.Current_in_nA:
                case typeAxisY.Coulomb_in_mC:
                    {
                        if (!Double.TryParse(original_str, out original))
                        {
                            original = 1.0;
                        }
                        if (!Double.TryParse(set_str, out set))
                        {
                            set = 1.0;
                        }
                    }
                    break;

                default:
                    break;
            }

            switch (_selectedAxisY)
            {
                case typeAxisY.Current_in_mA:
                case typeAxisY.Current_in_uA:
                case typeAxisY.Current_in_nA:
                    {
                        _currentMinY = Double.NaN;
                        _currentMaxY = Double.NaN;

                        System.Windows.Forms.DataVisualization.Charting.DataPointCollection dpc = chartVoltammogram.Series[idx].Points;
                        foreach (System.Windows.Forms.DataVisualization.Charting.DataPoint itr in dpc)
                        {
                            //itr.YValues[0] = itr.YValues[0] + ((-(original) + (set)) / _scaleAxisY[(int)typeAxisY.Current_in_uA]) * _scaleAxisY[(int)_selectedAxisY];

                            //itr.YValues[CURRENT] += (-original + set); // [uA]
                            itr.YValues[CURRENT] = ((itr.YValues[CURRENT] - originalShift) / original * set) + originalShift;
                            itr.YValues[0] = itr.YValues[CURRENT] / _scaleAxisY[(int)typeAxisY.Current_in_uA] * _scaleAxisY[(int)_selectedAxisY];

                            if ((itr.YValues[0] < _currentMinY) || (double.IsNaN(_currentMinY)))
                            {
                                _currentMinY = itr.YValues[0];
                            }
                            if ((itr.YValues[0] > _currentMaxY) || (double.IsNaN(_currentMaxY)))
                            {
                                _currentMaxY = itr.YValues[0];
                            }
                        }

                        //   1. 最小値と最大値の更新 (ここでは値のリセットはしない)
                        double length = Math.Abs(_currentMaxY - _currentMinY);
                        double ext = Math.Max(length, Math.Abs(_currentMaxY)) * 0.10;

                        _currentMinY -= ext;
                        _currentMaxY += ext;

                        chartVoltammogram.ChartAreas[0].RecalculateAxesScale();

                        //   2. 最小値と最大値に基づく目盛り間隔の更新
                        length = Math.Abs(_currentMaxY - _currentMinY);
                        RescaleTicksY(length, _currentMinY);
                    }
                    break;

                case typeAxisY.Coulomb_in_mC:
                    {
                        // TO DO: 乗算対応

                        System.Windows.Forms.DataVisualization.Charting.DataPointCollection dpc = chartVoltammogram.Series[idx].Points;
                        foreach (System.Windows.Forms.DataVisualization.Charting.DataPoint itr in dpc)
                        {
                            itr.YValues[CURRENT] += (-original + set) / _scaleAxisY[(int)typeAxisY.Current_in_uA];
                        }

                    }
                    break;

                //case typeAxisY.ImZ_in_ohm:
                //    {
                //        System.Windows.Forms.DataVisualization.Charting.DataPointCollection dpc = chartVoltammogram.Series[idx].Points;
                //        foreach (System.Windows.Forms.DataVisualization.Charting.DataPoint itr in dpc)
                //        {
                //            itr.YValues[0] = Math.Pow(itr.YValues[IM_Z] * itr.YValues[FREQ], -2);
                //        }

                //    }
                //    break;

                default:
                    break;
            }

            string str = toolStripComboSeriesScaleY.Items[idxRefP].ToString();
            ((XMLDataHolder)chartVoltammogram.Series[idx].Tag).SetDatum("scaleY", str);
            chartVoltammogram.Series[idx].Name
                = _series[idx] + " (" + ((XMLDataHolder)chartVoltammogram.Series[idx].Tag).GetDatum("reference", "0") + ")" + " x" + str + " +" + ((XMLDataHolder)chartVoltammogram.Series[idx].Tag).GetDatum("shiftY", "0") + "uA";
        }

        //private void toolStripDropDownButton2_Click(object sender, EventArgs e)
        //{

        //}

        //private void toolStripComboBoxAxisY_Click(object sender, EventArgs e)
        //{

        //}

        private void toolStripDropDownButtonCopy_ButtonClick(object sender, EventArgs e)
        {
            ExportDataAsMetafile(1);
        }

        private void toolStripDropDownButtonCopy_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            //ToolStripMenuItem item = (ToolStripMenuItem)sender;
            //int idx = item.DropDownItems.IndexOf(e.ClickedItem);

            ExportDataAsMetafile(int.Parse(e.ClickedItem.Tag.ToString()));
        }

        private void toolStripMenuItemSetXRange_Click(object sender, EventArgs e)
        {
            _set_xaxisrange.Show();
        }

        private void toolStripMenuItemFont_Click(object sender, EventArgs e)
        {
            _set_font.Show();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            _calc_halfwavepotential.Show();

            //_is_calc_halfwavepotential = true;
        }

        private void contextMenuItemUndoZoom_Click(object sender, EventArgs e)
        {
            undoZoom();
        }
        
        private void toolStripMenuItemUndoZoom_Click(object sender, EventArgs e)
        {
            undoZoom();
        }

        //
        // Event Handler for Chart
        //
        
        private void chartVoltammogram_SelectionRangeChanged(object sender, System.Windows.Forms.DataVisualization.Charting.CursorEventArgs e)
        {
            //Console.WriteLine("chartVoltammogram_SelectionRangeChanged: " + e.NewPosition);
            //Console.WriteLine("    {0}", e.Axis.ToString());
            //Console.WriteLine("    e.NewSelection: start {0}; end {1}", e.NewSelectionStart, e.NewSelectionEnd);
            //Console.WriteLine("    e.NewSelection (%): start {0}; end {1}", e.Axis.ValueToPosition(e.NewSelectionStart), e.Axis.ValueToPosition(e.NewSelectionEnd));
            //Console.WriteLine("    e.Axis.ScaleView.Size: {0}", e.Axis.ScaleView.Size);
            //Console.WriteLine("    e.NewPosition: {0}", e.NewPosition);
            //Console.WriteLine("    e.ChartArea.CursorX.Position: X {0}; Y {1}", e.ChartArea.CursorX.Position, e.ChartArea.CursorY.Position);

            if (chartVoltammogram.Series.Count == 0) return;
            if ((chartVoltammogram.Series[0]).Points.Count == 0) return;

            try
            {
                if(Math.Abs(e.Axis.ValueToPosition(e.NewSelectionStart) - e.Axis.ValueToPosition(e.NewSelectionEnd)) < 1)
                {
                    if (e.Axis.ToString().StartsWith("Axis-X"))
                    {
                        string X = "", Y1 = "", Y2 = "", XY = "";
                        string format_volt = "0", format_current = "0";
                        string unit_x = "", unit_y1 = "", unit_y2 = "";

                        switch (_selectedAxisY)
                        {
                            case typeAxisY.Coulomb_in_mC:
                            case typeAxisY.Coulomb_in_C:
                                Y1 = (e.ChartArea.CursorY.Position / _scaleAxisY[(int)_selectedAxisY]).ToString("0.000");
                                unit_y1 = " mC corresponding to " + (e.ChartArea.CursorY.Position / 96485.3329 * 1000 / _scaleAxisY[(int)_selectedAxisY]).ToString("0.000") + " umol of electron";
                                break;

                            case typeAxisY.Current_in_mA:
                            case typeAxisY.Current_in_nA:
                            case typeAxisY.Current_in_uA:
                            case typeAxisY.Potential_in_mV:
                                switch (_selectedAxisY)
                                {
                                    case typeAxisY.Current_in_nA: unit_y1 = "nA"; format_current = "0.000"; break;
                                    case typeAxisY.Current_in_uA: unit_y1 = "uA"; format_current = "0.000"; break;
                                    case typeAxisY.Current_in_mA: unit_y1 = "mA"; format_current = "0.000"; break;
                                }
                                Y1 = e.ChartArea.CursorY.Position.ToString(format_current);

                                if (_calc_halfwavepotential.Visible)
                                {
                                    _calc_halfwavepotential.SetCursor(e.ChartArea.CursorX.Position / _scaleAxisX[(int)_selectedAxisX] * _scaleAxisX[(int)typeAxisX.Potential_in_mV]);
                                }
                                break;

                            case typeAxisY.ImZ_in_ohm:
                                double x = e.ChartArea.CursorX.Position;
                                double y = e.ChartArea.CursorY.Position;
                                double f1 = 0.0;
                                for (int i = 0; i < chartVoltammogram.Series[_currentIndex].Points.Count; i++)
                                {
                                    if (chartVoltammogram.Series[_currentIndex].Points[i].XValue < x)
                                    {
                                        if (i > 0)
                                        {
                                            // Bode plot: chartVoltammogram.Series[6].Points
                                            // Cole-Cole plot: chartVoltammogram.Series[1].Points

                                            // indexがiとi-1のデータ間にXがある

                                            double f1_1 = chartVoltammogram.Series[_currentIndex].Points[i].YValues[FREQ];
                                            double f1_0 = chartVoltammogram.Series[_currentIndex].Points[i - 1].YValues[FREQ];
                                            double x1_1 = chartVoltammogram.Series[_currentIndex].Points[i].XValue;
                                            double x1_0 = chartVoltammogram.Series[_currentIndex].Points[i - 1].XValue;

                                            f1 = (f1_1 - f1_0) / (x1_1 - x1_0) * (x - x1_0) + f1_0;

                                            break;
                                        }
                                    }
                                }
                                Y1 = y.ToString("0.0"); unit_y1 = "ohm, " + f1.ToString("0.0") + " Hz";
                                break;

                            case typeAxisY.C2_in_MS_plot:
                                Y1 = e.ChartArea.CursorY.Position.ToString("0.00"); unit_y1 = "mF^-2";
                                break;

                            default:
                                break;
                        }

                        switch (_selectedAxisX)
                        {
                            case typeAxisX.Time_in_sec: unit_x = "s"; format_volt = "0"; break;
                            case typeAxisX.Time_in_hour: unit_x = "h"; format_volt = "0.0"; break;
                            case typeAxisX.ReZ_in_ohm: unit_x = "ohm"; format_volt = "0.0"; break;
                            case typeAxisX.Potential_in_mV: unit_x = "mV"; format_volt = "0.0"; break;
                            case typeAxisX.Potential_in_V: unit_x = "V"; format_volt = "0.0000"; break;
                            case typeAxisX.Freq: unit_x = "Hz"; format_volt = "0.0"; break;
                            default: break;
                        }
                        switch (_selectedAxisX)
                        {
                            case typeAxisX.Time_in_sec: 
                            case typeAxisX.Time_in_hour: 
                            case typeAxisX.ReZ_in_ohm:
                            case typeAxisX.Potential_in_mV: 
                            case typeAxisX.Potential_in_V:
                                X = e.ChartArea.CursorX.Position.ToString(format_volt);
                                break;
                            case typeAxisX.Freq:  
                                X = Math.Pow(10, e.ChartArea.CursorX.Position).ToString(format_volt);
                                break;
                            default: 
                                break;
                        }
                        XY = "(" + X + " " + unit_x + ", " + Y1 + " " + unit_y1 + ")";
                        toolStripStatusCursor.Text = XY;
                    }
                }
                else
                {
                    //Console.WriteLine("    Zooming");

                    System.Windows.Forms.DataVisualization.Charting.ChartArea ca = chartVoltammogram.ChartAreas[0];

                    if (e.Axis.ToString().StartsWith("Axis-X"))
                    {
                        double p = Math.Min(e.NewSelectionStart, e.ChartArea.CursorX.Position);
                        double size = Math.Abs(e.NewSelectionStart - e.NewSelectionEnd);
                        //p = 100000;
                        //size = 125000;
                        double order = Math.Truncate(Math.Log10(Math.Abs(size)));
                        double interval = Math.Pow(10, order - 1);
                        //double interval_f = Math.Log10(size) % 1; //if (interval_f < 0) interval_f = 1 - interval_f;
                        //double m = (interval_f < 0.1) ? 2.5 : ((interval_f < 0.4) ? 5 : ((interval_f < 0.7) ? 10 : 20));
                        double interval_m = RescaleTicksX(size, p, is_zooming:true);
                        //double new_size = Math.Ceiling(size / (interval_m)) * interval_m;
                        double new_p = Math.Round(p / (interval)) * (interval);

                        ca.AxisX.ScaleView.Zoom(new_p, size, System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number, false);
                        ca.AxisX2.ScaleView.Zoom(new_p, size, System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number, false);

                        //Console.WriteLine("    After zooming: ViewMinimum {0}; size {1}; offset {2}; new_p {3}",
                        //    ca.AxisX.ScaleView.ViewMinimum,
                        //    ca.AxisX.ScaleView.Size,
                        //    ca.AxisX.LabelStyle.IntervalOffset,
                        //    new_p);

                        // Chartコントロールのバグ？Zoom後に少しずれるのを補正する
                        double delta = ca.AxisX.ScaleView.ViewMinimum - new_p;
                        ca.AxisX.LabelStyle.IntervalOffset = -delta;
                        ca.AxisX.MajorTickMark.IntervalOffset = -delta;
                        ca.AxisX.MinorTickMark.IntervalOffset = -delta;
                        ca.AxisX2.MajorTickMark.IntervalOffset = -delta;
                        ca.AxisX2.MinorTickMark.IntervalOffset = -delta;

                        if(_selectedAxisX != typeAxisX.Freq)
                        {
                            // 軸ラベル位置をキリのよいところ＝主目盛り間隔で始まるようにする
                            double delta2 = (AbsCeiling(new_p / (interval_m)) * (interval_m)) - new_p;
                            ca.AxisX.LabelStyle.IntervalOffset += delta2;
                            ca.AxisX.MajorTickMark.IntervalOffset += delta2;
                            ca.AxisX2.MajorTickMark.IntervalOffset += delta2;

                            // 副目盛りは、主目盛りよりもoffsetとしては小さくなるので (現状、副目盛り間隔は、主目盛りよりも5倍狭い) 
                            // Offsetは、グラフの左下が基準
                            double delta3 = (AbsCeiling(new_p / (interval_m / 5)) * (interval_m / 5)) - new_p;
                            ca.AxisX.MinorTickMark.IntervalOffset += delta3;
                            ca.AxisX2.MinorTickMark.IntervalOffset += delta3;
                        }
                        else
                        {
                            double delta2 = Math.Ceiling(new_p) - new_p;
                            ca.AxisX.LabelStyle.IntervalOffset += delta2;
                            ca.AxisX.MajorTickMark.IntervalOffset += delta2;
                            ca.AxisX.MinorTickMark.IntervalOffset += delta2;
                            //double delta3_x2 = (Math.Ceiling(new_p_x2 / (1.0/10)) * (1.0/10)) - new_p_x2;
                            ca.AxisX.MinorTickMark.IntervalOffset += -1;
                        }

                        // selectioの解除
                        double new_x = (e.NewSelectionStart + e.NewSelectionEnd) / 2;
                        e.ChartArea.CursorX.Position = new_x; 
                        e.ChartArea.CursorX.SetSelectionPosition(new_x, new_x);

                        // selectionの範囲が狭い時には、zoomしないので、ここでselectionを解除しておく
                        if(Math.Abs(
                              ca.AxisY.ValueToPosition(e.ChartArea.CursorY.SelectionEnd)
                            - ca.AxisY.ValueToPosition(e.ChartArea.CursorY.SelectionStart)
                                    ) < 1)
                        {
                            double new_y = (e.ChartArea.CursorY.SelectionEnd + e.ChartArea.CursorY.SelectionStart) / 2;
                            e.ChartArea.CursorY.SetSelectionPosition(new_y, new_y);
                        }
                    }
                    else
                    {
                        double p = Math.Min(e.NewSelectionStart, e.ChartArea.CursorY.Position);
                        double size = Math.Abs(e.NewSelectionStart - e.NewSelectionEnd);
                        //p = -100;
                        //size = 100;
                        //   2. 最小値と最大値に基づく目盛り間隔の更新
                        double order = Math.Truncate(Math.Log10(Math.Abs(size)));
                        double interval = Math.Pow(10, order - 1);
                        double interval_m = RescaleTicksY(size, p, is_zooming:true);
                        //double new_size = Math.Ceiling(size / (interval_m)) * interval_m;
                        double new_p = Math.Round(p / (interval)) * (interval);

                        ca.AxisY.ScaleView.Zoom(new_p, size, System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number, false);
                        ca.AxisY2.ScaleView.Zoom(new_p, size, System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number, false);

                        //Console.WriteLine("    After zooming: ViewMinimum {0}; (ScaleView)Size {1}; IntervalOffset {2}; new_p {3}; size {4}",
                        //    ca.AxisY.ScaleView.ViewMinimum - ca.AxisY.ScaleView.ViewMaximum,
                        //    ca.AxisY.ScaleView.Size,
                        //    ca.AxisY.LabelStyle.IntervalOffset,
                        //    new_p, size);

                        double delta = (ca.AxisY.ScaleView.ViewMinimum - new_p);
                        ca.AxisY.LabelStyle.IntervalOffset = -delta;
                        ca.AxisY.MajorTickMark.IntervalOffset = -delta;
                        ca.AxisY.MinorTickMark.IntervalOffset = -delta;
                        ca.AxisY2.MajorTickMark.IntervalOffset = -delta;
                        ca.AxisY2.MinorTickMark.IntervalOffset = -delta;

                        // 軸ラベル位置をキリのよいところ＝主目盛り間隔で始まるようにする
                        double delta2 = (AbsCeiling(new_p / (interval_m)) * (interval_m)) - new_p;
                        ca.AxisY.LabelStyle.IntervalOffset += delta2;
                        ca.AxisY.MajorTickMark.IntervalOffset += delta2;
                        ca.AxisY2.MajorTickMark.IntervalOffset += delta2;

                        // 副目盛りは、主目盛りよりもoffsetとしては小さくなるので (現状、副目盛り間隔は、主目盛りよりも2倍狭い) 
                        // Offsetは、グラフの左下が基準
                        double delta3 = (AbsCeiling(new_p / (interval_m / 2)) * (interval_m / 2)) - new_p;
                        ca.AxisY.MinorTickMark.IntervalOffset += delta3;
                        ca.AxisY2.MinorTickMark.IntervalOffset += delta3;

                        double new_y = (e.NewSelectionStart + e.NewSelectionEnd) / 2;
                        e.ChartArea.CursorY.Position = new_y;
                        e.ChartArea.CursorY.SetSelectionPosition(new_y, new_y);

                        if (Math.Abs(
                              ca.AxisX.ValueToPosition(e.ChartArea.CursorX.SelectionEnd)
                            - ca.AxisX.ValueToPosition(e.ChartArea.CursorX.SelectionStart)
                                    ) < 1)
                        {
                            double new_x = (e.ChartArea.CursorX.SelectionEnd + e.ChartArea.CursorX.SelectionStart) / 2;
                            e.ChartArea.CursorX.SetSelectionPosition(new_x, new_x);
                        }
                    }

                    double AbsCeiling(double v)
                    {
                        return (v > 0) ? Math.Ceiling(v) : Math.Floor(v);
                    }
                }
            }
            catch (System.Exception error)
            {
                Console.WriteLine("Exception: {0}", error.Message); //Console.WriteLine("    {0}", error.ToString());
                                                                    //MessageBox.Show("exception");
            }
        }

        private void chartVoltammogram_SelectionRangeChanging(object sender, System.Windows.Forms.DataVisualization.Charting.CursorEventArgs e)
        {
            //Console.WriteLine("chartVoltammogram_SelectionRangeChanging: " + e.NewPosition);
            //Console.WriteLine("    {0}", e.Axis.ToString());
            //Console.WriteLine("    e.NewSelection: start {0}; end {1}", e.NewSelectionStart, e.NewSelectionEnd);
            //Console.WriteLine("    e.Axis.ScaleView.Size: {0}", e.Axis.ScaleView.Size);
        }

        private void chartVoltammogram_CursorPositionChanged(object sender, System.Windows.Forms.DataVisualization.Charting.CursorEventArgs e)
        {
            //string X = "", Y1 = "", Y2 = "", XY = "";
            //string format_volt = "0", format_current = "0";
            //string unit_x = "", unit_y1 = "", unit_y2 = "";

            //switch (_selectedAxisY)
            //{
            //    case typeAxisY.Coulomb_in_mC:
            //    case typeAxisY.Coulomb_in_C:
            //        Y1 = (e.ChartArea.CursorY.Position / _scaleAxisY[(int)_selectedAxisY]).ToString("0.000"); 
            //        unit_y1 = " mC corresponding to " + (e.ChartArea.CursorY.Position / 96485.3329 * 1000 / _scaleAxisY[(int)_selectedAxisY]).ToString("0.000") + " umol of electron";
            //        break;

            //    case typeAxisY.Current_in_mA:
            //    case typeAxisY.Current_in_nA:
            //    case typeAxisY.Current_in_uA:
            //    case typeAxisY.Potential_in_mV:
            //        switch (_selectedAxisY)
            //        {
            //            case typeAxisY.Current_in_nA: unit_y1 = "nA"; format_current = "0.000"; break;
            //            case typeAxisY.Current_in_uA: unit_y1 = "uA"; format_current = "0.000"; break;
            //            case typeAxisY.Current_in_mA: unit_y1 = "mA"; format_current = "0.000"; break;
            //        }
            //        Y1 = e.ChartArea.CursorY.Position.ToString(format_current);

            //        if (_calc_halfwavepotential.Visible)
            //        {
            //            _calc_halfwavepotential.SetCursor(e.ChartArea.CursorX.Position / _scaleAxisX[(int)_selectedAxisX] * _scaleAxisX[(int)typeAxisX.Potential_in_mV]);
            //        }
            //        break;

            //    case typeAxisY.ImZ_in_ohm:
            //        double x = e.ChartArea.CursorX.Position;
            //        double y = e.ChartArea.CursorY.Position;
            //        double f1 = 0.0;
            //        for (int i = 0; i < chartVoltammogram.Series[_currentIndex].Points.Count; i++)
            //        {
            //            if (chartVoltammogram.Series[_currentIndex].Points[i].XValue < x)
            //            {
            //                if (i > 0)
            //                {
            //                    // Bode plot: chartVoltammogram.Series[6].Points
            //                    // Cole-Cole plot: chartVoltammogram.Series[1].Points

            //                    // indexがiとi-1のデータ間にXがある

            //                    double f1_1 = chartVoltammogram.Series[_currentIndex].Points[i].YValues[FREQ];
            //                    double f1_0 = chartVoltammogram.Series[_currentIndex].Points[i - 1].YValues[FREQ];
            //                    double x1_1 = chartVoltammogram.Series[_currentIndex].Points[i].XValue;
            //                    double x1_0 = chartVoltammogram.Series[_currentIndex].Points[i - 1].XValue;

            //                    f1 = (f1_1 - f1_0) / (x1_1 - x1_0) * (x - x1_0) + f1_0;

            //                    break;
            //                }
            //            }
            //        }
            //        Y1 = y.ToString("0.0"); unit_y1 = "ohm, " + f1.ToString("0.0") + " Hz";
            //        break;

            //    case typeAxisY.C2_in_MS_plot:
            //        Y1 = e.ChartArea.CursorY.Position.ToString("0.00"); unit_y1 = "mF^-2";
            //        break;

            //    default:
            //        break;
            //}

            //switch (_selectedAxisX)
            //{
            //    case typeAxisX.Time_in_sec: unit_x = "s"; format_volt = "0"; break;
            //    case typeAxisX.Time_in_hour: unit_x = "h"; format_volt = "0.0"; break;
            //    case typeAxisX.ReZ_in_ohm: unit_x = "ohm"; format_volt = "0.0"; break;
            //    case typeAxisX.Potential_in_mV: unit_x = "mV"; format_volt = "0.0"; break;
            //    case typeAxisX.Potential_in_V: unit_x = "V"; format_volt = "0.0000"; break;
            //    default: break;
            //}
            //X = e.ChartArea.CursorX.Position.ToString(format_volt);

            //XY = "(" + X + " " + unit_x + ", " + Y1 + " " + unit_y1 + ")";
            //toolStripStatusCursor.Text = XY;
        }

        private void chartVoltammogram_AxisViewChanging(object sender, System.Windows.Forms.DataVisualization.Charting.ViewEventArgs e)
        {
            //Console.WriteLine("chartVoltammogram_AxisViewChanging: " + e.NewPosition);
            //Console.WriteLine("    {0}", e.Axis.ToString());
            //Console.WriteLine("    e.NewSize: {0}", e.NewSize);
            //Console.WriteLine("    e.Axis.ScaleView.Size: {0}", e.Axis.ScaleView.Size);

            //if (e.NewSize > 300)
            //{
            //}
            //else
            //{
            //    Console.WriteLine("    Zoom was cancelled");
            //    e.Axis.ScaleView.Zoomable = false;
            //}            
        }

        private void chartVoltammogram_AxisViewChanged(object sender, System.Windows.Forms.DataVisualization.Charting.ViewEventArgs e)
        {
            Console.WriteLine("chartVoltammogram_AxisViewChanged: " + e.NewPosition);
            Console.WriteLine("    {0}", e.Axis.ToString());
            Console.WriteLine("    e.NewSize: {0}", e.NewSize);
            Console.WriteLine("    e.Axis.ScaleView.Size: {0}", e.Axis.ScaleView.Size);

            //double p = e.NewPosition; double size;

            //if (true)// || p != new_p)
            //{
            //    if (e.Axis.ToString().StartsWith("Axis-X"))
            //    {
            //        Console.WriteLine("    {0}", e.Axis.ToString());
            //        Console.WriteLine("    e.NewSize: {0}", e.NewSize);
            //        Console.WriteLine("    e.Axis.ScaleView.Size: {0}", e.Axis.ScaleView.Size);

            //        if(e.NewSize > 1.0)
            //        {
            //            double interval_m = RescaleTicksX(e.Axis.ScaleView.Size, Double.NaN);
            //            Console.WriteLine("    AxisX.MajorTickMark.IntervalOffset: {0}", chartVoltammogram.ChartAreas[0].AxisX.MajorTickMark.IntervalOffset);

            //            double new_p = Math.Round(p / (interval_m)) * interval_m;
            //            e.NewPosition = new_p;
            //            e.Axis.ScaleView.Position = new_p;
            //            //chartVoltammogram.ChartAreas[0].AxisX2.ScaleView.
            //            chartVoltammogram.ChartAreas[0].AxisX2.ScaleView.Zoom(new_p, e.Axis.ScaleView.Size, System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number, true);
            //        }
            //        else
            //        {
            //            Console.WriteLine("    Zoom was cancelled");
            //        }
            //    }
            //    else
            //    {
            //        Console.WriteLine("    {0}", e.Axis.ToString());
            //        Console.WriteLine("    e.NewSize: {0}", e.NewSize);
            //        Console.WriteLine("    e.Axis.ScaleView.Size: {0}", e.Axis.ScaleView.Size);

            //        //   2. 最小値と最大値に基づく目盛り間隔の更新
            //        double interval_m = RescaleTicksY(e.Axis.ScaleView.Size, Double.NaN);

            //        double new_p = Math.Round(p / interval_m) * interval_m;
            //        e.NewPosition = new_p;
            //        e.Axis.ScaleView.Position = new_p;

            //        chartVoltammogram.ChartAreas[0].AxisY2.ScaleView.Zoom(new_p, e.Axis.ScaleView.Size, System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number, true);
            //    }
            //}
        }

        private void chartVoltammogram_DoubleClick(object sender, EventArgs e)
        {
            //chartVoltammogram.SuspendLayout();

            //chartVoltammogram.ChartAreas[0].AxisX.ScaleView.ZoomReset();
            //chartVoltammogram.ChartAreas[0].AxisX2.ScaleView.ZoomReset();
            //chartVoltammogram.ChartAreas[0].AxisY.ScaleView.ZoomReset();
            //chartVoltammogram.ChartAreas[0].AxisY2.ScaleView.ZoomReset();

            ////   X軸
            //double length = chartVoltammogram.ChartAreas[0].AxisX.ScaleView.Size;
            //if (Double.IsNaN(length)) length = (_currentMaxX - _currentMinX);
            //double p = chartVoltammogram.ChartAreas[0].AxisX.ScaleView.Position;
            //if (Double.IsNaN(p)) p = (_currentMinX);
            //RescaleTicksX(length, p);

            ////   Y軸
            //length = chartVoltammogram.ChartAreas[0].AxisY.ScaleView.Size;
            //if (Double.IsNaN(length)) length = (_currentMaxY - _currentMinY);
            //p = chartVoltammogram.ChartAreas[0].AxisY.ScaleView.Position;
            //if (Double.IsNaN(p)) p = (_currentMinY);
            //RescaleTicksY(length,p);

            //chartVoltammogram.ResumeLayout();
            //chartVoltammogram.PerformLayout();
        }

        private void chartVoltammogram_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // ドラッグ中のファイルやディレクトリの取得
                string[] drags = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string d in drags)
                {
                    if (!System.IO.File.Exists(d))
                    {
                        // ファイル以外であればイベント・ハンドラを抜ける
                        return;
                    }
                }
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void chartVoltammogram_DragDrop(object sender, DragEventArgs e)
        {
            // ドラッグ＆ドロップされたファイル
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            //listBox1.Items.AddRange(files); // リストボックスに表示

            for (int i = 0; i < files.Count(); i++)
            {
                LoadFile(files[i]);
            }
        }
    }

    public static class ObjectExtension
    {
        // ディープコピーの複製を作る拡張メソッド
        public static T DeepClone<T>(this T src)
        {
            using (var memoryStream = new System.IO.MemoryStream())
            {
                var binaryFormatter
                  = new System.Runtime.Serialization
                        .Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, src); // シリアライズ
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                return (T)binaryFormatter.Deserialize(memoryStream); // デシリアライズ
            }
        }
    }

    public class ClipboardMetafileHelper
    {
        [DllImport("user32.dll")]
        static extern bool OpenClipboard(IntPtr hWndNewOwner);
        [DllImport("user32.dll")]
        static extern bool EmptyClipboard();
        [DllImport("user32.dll")]
        static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);
        [DllImport("user32.dll")]
        static extern bool CloseClipboard();
        [DllImport("gdi32.dll")]
        static extern IntPtr CopyEnhMetaFile(IntPtr hemfSrc, IntPtr hNULL);
        [DllImport("gdi32.dll")]
        static extern bool DeleteEnhMetaFile(IntPtr hemf);

        // Metafile mf is set to an invalid state inside this function
        static public bool PutEnhMetafileOnClipboard(IntPtr hWnd, Metafile mf)
        {
            bool bResult = false;
            IntPtr hEMF, hEMF2;
            hEMF = mf.GetHenhmetafile(); // invalidates mf
            if (!hEMF.Equals(IntPtr.Zero))
            {
                hEMF2 = CopyEnhMetaFile(hEMF, new IntPtr(0));
                if (!hEMF2.Equals(IntPtr.Zero))
                {
                    if (OpenClipboard(hWnd))
                    {
                        if (EmptyClipboard())
                        {
                            IntPtr hRes = SetClipboardData(14 /*CF_ENHMETAFILE*/, hEMF2);
                            bResult = hRes.Equals(hEMF2);
                            CloseClipboard();
                        }
                    }
                }
                DeleteEnhMetaFile(hEMF);
            }
            return bResult;
        }
    }
}
