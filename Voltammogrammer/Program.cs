using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Voltammogrammer
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            string[] Commands = System.Environment.GetCommandLineArgs();

            if(Commands.Count() == 1)
            {
                Application.Run(new Potentiostat());
            }
            else
            {
                Application.Run(new formVoltammogram(Commands.Skip(1).ToArray()));
            }
        }
    }
}
