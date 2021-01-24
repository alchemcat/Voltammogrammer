using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;
using System.Threading;

namespace Voltammogrammer
{
    class Arduino
    {
        SerialPort _port = null;
        bool _ready = false;

        public Arduino()
        {
            _port = new SerialPort();
        }

        ~Arduino()
        {
            if (_port.IsOpen)
            {
                _port.Close();
            }
        }

        public int setPortNameToToolStripComboBox(System.Windows.Forms.ToolStripComboBox cb)
        {
            foreach (var portName in SerialPort.GetPortNames())
            {
                cb.Items.Add(portName);
            }
            if (cb.Items.Count > 0)
            {
                cb.SelectedIndex = 0;
            }

            return cb.Items.Count;
        }

        public uint Open(string portname)
        {
            _port.BaudRate = 9600;// int.Parse(bauRateComboBox1.Text);//9600;
            _port.PortName = portname;
            _port.DtrEnable = false;// useDTRcheckBox1.Enabled;
            _port.ReadTimeout = 2000;
            _port.NewLine = Environment.NewLine;// "\r\n";
            _port.Open();

            Thread.Sleep(2000);

            _port.WriteLine("i");
            try
            {
                string r = _port.ReadLine();
                if (r == "OK")
                {
                    //logTextBox1.AppendText("Connected.\n");
                    //_port.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(_DataReceived);
                    Console.WriteLine("Connected to Arduino.");

                    return 0;
                }
            }
            catch (TimeoutException e)
            {
                //Console.WriteLine(e);
                System.Windows.Forms.MessageBox.Show("Cannot open connection to Arduino.");
            }

            return 1;
        }

        private void _DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // 受信したデータ
            string data = _port.ReadLine();
            _port.DiscardOutBuffer();
            _port.DiscardInBuffer();

            Console.WriteLine(data);

            // 異なるスレッドのテキストボックスに書き込む
            //Invoke(new Delegate_write(write), new Object[] { data });
            _ready = true;
        }

        public void Close()
        {
            if (_port.IsOpen)
            {
                //_ready = false;
                openCircuit();
                //while (!_ready)
                //{
                //    Thread.Sleep(1);
                //}

                //_ready = false;
                setRotation(0);
                //while (!_ready)
                //{
                //    Thread.Sleep(1);
                //}

                //_ready = false;
                startPurging();
                //while (!_ready)
                //{
                //    Thread.Sleep(1);
                //}
                _deactivateAllRelay();

                //_port.DiscardInBuffer();
                //_port.DataReceived -= new System.IO.Ports.SerialDataReceivedEventHandler(_DataReceived);
                _port.DiscardOutBuffer();
                _port.DiscardInBuffer();
                _port.WriteLine("i");
                try
                {
                    string r = _port.ReadLine();
                    if (r == "OK")
                    {
                        //return 0;
                        Console.WriteLine("Disconnected from Arduino.");
                        _port.Close();
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Cannot readline @ i.");
                    }
                }
                catch (TimeoutException e)
                {
                    //Console.WriteLine(e);
                    System.Windows.Forms.MessageBox.Show("Cannot close connection to Arduino.");
                }
            }
        }

        //
        // Arduinoへのコマンド発行関数
        //

        public void setRotation(uint rpm)
        {
            if (!_port.IsOpen) return;

            _port.DiscardOutBuffer();
            _port.DiscardInBuffer();

            _port.WriteLine("r" + rpm.ToString());
            try
            {
                string r = _port.ReadLine();
                Console.WriteLine(r);
                //if (r == "OK")
                //{
                //    return 0;
                //}
            }
            catch (TimeoutException e)
            {
                Console.WriteLine("Cannot readline @ r");
            }
        }

        private void _activateRelay(uint relay)
        {
            if (!_port.IsOpen) return;

            _port.DiscardOutBuffer();
            _port.DiscardInBuffer();

            _port.WriteLine("s" + relay.ToString() + "a");
            try
            {
                string r = _port.ReadLine();
                Console.WriteLine(r);
                //if (r == "OK")
                //{
                //    return 0;
                //}
            }
            catch (TimeoutException e)
            {
                Console.WriteLine("Cannot readline @ s a");
            }
        }

        private void _deactivateRelay(uint relay)
        {
            if (!_port.IsOpen) return;

            _port.DiscardOutBuffer();
            _port.DiscardInBuffer();

            _port.WriteLine("s" + relay.ToString() + "d");
            try
            {
                string r = _port.ReadLine();
                Console.WriteLine(r);
                //if (r == "OK")
                //{
                //    return 0;
                //}
            }
            catch (TimeoutException e)
            {
                Console.WriteLine("Cannot readline @ s d");
            }
        }

        private void _deactivateAllRelay()
        {
            if (!_port.IsOpen) return;

            _port.DiscardOutBuffer();
            _port.DiscardInBuffer();

            _port.WriteLine("sxd");
            try
            {
                string r = _port.ReadLine();
                Console.WriteLine(r);
                //if (r == "OK")
                //{
                //    return 0;
                //}
            }
            catch (TimeoutException e)
            {
                Console.WriteLine("Cannot readline @ s d");
            }
        }


        public void startPurging()
        {
            if (!_port.IsOpen) return;

            _port.DiscardOutBuffer();
            _port.DiscardInBuffer();

            _port.WriteLine("pa");
            try
            {
                string r = _port.ReadLine();
                Console.WriteLine(r);
                //if (r == "OK")
                //{
                //    return 0;
                //}
            }
            catch (TimeoutException e)
            {
                Console.WriteLine("Cannot readline @ p a");
            }
        }

        public void stopPurging()
        {
            if (!_port.IsOpen) return;

            _port.DiscardOutBuffer();
            _port.DiscardInBuffer();

            _port.WriteLine("pd");
            try
            {
                string r = _port.ReadLine();
                Console.WriteLine(r);
                //if (r == "OK")
                //{
                //    return 0;
                //}
            }
            catch (TimeoutException e)
            {
                Console.WriteLine("Cannot readline @ p d");
            }
        }

        public void openCircuit()
        {
            _deactivateRelay(2);
            _deactivateRelay(3);
        }

        public void closeCircuit()
        {
            _activateRelay(2);
            _activateRelay(3);
        }

        public void selectCurrentRange(Potentiostat.rangeCurrent range)
        {
            switch(range)
            {
                case Potentiostat.rangeCurrent.Range20mA:
                    _activateRelay(1);
                    break;

                case Potentiostat.rangeCurrent.Range2mA:
                    _activateRelay(1);
                    break;

                case Potentiostat.rangeCurrent.Range200uA:
                    _deactivateRelay(1);
                    break;

                default:
                    _deactivateRelay(1);
                    break;
            }
        }
    }
}
