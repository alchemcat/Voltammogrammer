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
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Voltammogrammer
{
    public partial class WaitDialog : System.Windows.Forms.Form
    {
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        System.Threading.Timer timer;

        public WaitDialog()
        {
            InitializeComponent();
        }

        public Task<bool> ShowAsync(int timeout)
        {
            var tcs = new TaskCompletionSource<bool>();

            Show();

            btnCancel.Click += new System.EventHandler((object sender, System.EventArgs e) => { timer.Dispose(); tcs.SetResult(true); Close(); });
            buttonStart.Click += new System.EventHandler((object sender, System.EventArgs e) => { timer.Dispose(); tcs.SetResult(false); Close(); });
            this.Closing += new System.ComponentModel.CancelEventHandler((object sender, System.ComponentModel.CancelEventArgs e) => { timer.Dispose(); tcs.TrySetResult(true); });

            int c = (timeout > 10) ? timeout: 10; // in [s]
            timeout = c;
            progBarMeter.Value = 100;
            progBarMeter.Minimum = 0;
            progBarMeter.Maximum = 100;
            progBarMeter.Refresh();

            timer = new System.Threading.Timer(delegate
            {
                if(c-- == 0)
                {
                    timer.Dispose();
                    tcs.TrySetResult(false);

                    Invoke((Action)delegate ()
                    {
                        Close();
                    });
                }
                else
                {
                    Invoke((Action)delegate ()
                    {
                        labelProgress.Text = c.ToString() + " s left...";
                        progBarMeter.Value = (int)Math.Round(((double)c / timeout) * 100.0); //Console.WriteLine($"{progBarMeter.Value}");
                        progBarMeter.Refresh();
                    });
                }
            });

            // int waitMilliseconds = 10 * 1000; // (int)(time - DateTime.Now).TotalMilliseconds;
            // timer.Change(waitMilliseconds, Timeout.Infinite); // called only once after waitMilliseconds
            timer.Change(0, 1000); // called only once after waitMilliseconds

            return tcs.Task;
        }
    }
}
