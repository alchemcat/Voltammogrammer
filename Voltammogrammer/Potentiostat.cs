
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

//#define VERSION_0_9e

using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices; // DllImport
using System.Data;
using System.Diagnostics;

using System.Xml.Serialization;
using System.Xml;
using System.Runtime.Serialization;
using System.IO;
using System.Text;


using static AnalogDiscovery2.dwf;


namespace Voltammogrammer
{
    public partial class Potentiostat : Form
    {
        int _handle = 0;
        int status;

        const bool DIGITALIO_FROM_ARDUINO = false;

#if VERSION_0_9e
        const string VERSION_string = "0.9e";
        const bool VERSION_0_5 = false;
        const bool VERSION_0_9c = false;
        const bool VERSION_0_9d = false;
        const bool VERSION_0_9e = true;
        const bool VERSION_0_9k = false;
        const int CHANNEL_POTENTIAL = 1; const int CHANNEL_CURRENT = 2;
        const double RATIO_POTENTIAL = 1.0; // R2/R1 (Figure 2 in JCE2012) which determines an amplification factor of potential
#elif VERSION_0_9k
        const string VERSION_string = "0.9k (the simplified version)";
        const bool VERSION_0_5 = false;
        const bool VERSION_0_9c = false;
        const bool VERSION_0_9d = false;
        const bool VERSION_0_9e = false;
        const bool VERSION_0_9k = true;
        const int CHANNEL_POTENTIAL = 1; const int CHANNEL_CURRENT = 2;
#elif VERSION_0_9j
        const string VERSION_string = "0.9j";
        const bool VERSION_0_5  = false;
        const bool VERSION_0_9c = false;
        const bool VERSION_0_9d = false;
        const bool VERSION_0_9e = true;
        const bool VERSION_0_9k = false;
        const int CHANNEL_POTENTIAL = 2; const int CHANNEL_CURRENT = 1;
#elif VERSION_0_9c
        const string VERSION_string = "0.9c";
        const bool VERSION_0_5  = false;
        const bool VERSION_0_9c = true;
        const bool VERSION_0_9d = false;
        const bool VERSION_0_9e = false;
        const bool VERSION_0_9k = false;
        const int CHANNEL_POTENTIAL = 2; const int CHANNEL_CURRENT = 1;
#elif VERSION_0_9d
        const string VERSION_string = "0.9d";
        const bool VERSION_0_5  = false;
        const bool VERSION_0_9c = false;
        const bool VERSION_0_9d = true;
        const bool VERSION_0_9e = false;
        const bool VERSION_0_9k = false;
        const int CHANNEL_POTENTIAL = 2; const int CHANNEL_CURRENT = 1;
#endif

        const int CHANNEL_VIRTUAL_REAL_Z = 3;
        const int CHANNEL_VIRTUAL_IM_Z = 4;
        const int CHANNEL_VIRTUAL_FREQ = 5;
        const int CHANNEL_VIRTUAL_ATTN = 6;
        const int CHANNEL_VIRTUAL_OPEN_RS = 7;
        const int CHANNEL_VIRTUAL_OPEN_XS = 8;
        const int CHANNEL_VIRTUAL_SHORT_RS = 9;
        const int CHANNEL_VIRTUAL_SHORT_XS = 10;
        const int CHANNEL_VIRTUAL_PHASE = 11;
        //const int CHANNEL_VIRTUAL_POTENTIAL = 12;

        bool DEBUG_VOLTAMMOGRAM = false;
        const int POTENTIAL_SCALE = 1000;
        const int DELAY_TIME_SWITCHING_RELAY = 200; // 200だとPP09cでは200uA - 2mAの切り替えが起こらない様子 -> と思ったら半田不良でした

        double POTENTIAL_OFFSET_AWG = 0.0;
        double POTENTIAL_OFFSET_OSC = 0.0;
        double POTENTIAL_SLOPE_AWG = 1.0;
        double POTENTIAL_SLOPE_OSC = 1.0;
        double CURRENT_OFFSET = 0.0;
        double CURRENT_SLOPE = 1.0;

        formVoltammogram _voltammogram = new formVoltammogram(true);
        System.Windows.Forms.DataVisualization.Charting.Series _series;
        XMLDataHolder _exp_conditions;
        Select_RotationSpeed _select_rotation_speeds;
        Calibrate_Potentiostat _calibrate_potentiostat;
        Configure_Potentiostat _configure_potentiostat;
        NonlinearRegression _nlr = new NonlinearRegression();

        public const int BUFFER_SIZE = 4096 * 16 * 8; // Number of samples per waveform capture for block captures
        //public const int AWG_SIZE = 4096; // Number of samples per waveform capture for block captures
        //public const int RAPID_BLOCK_BUFFER_SIZE = 100; // Number of samples per waveform capture for rapid block captures
        public const int MAX_CHANNELS = 2;
        //public const int QUAD_SCOPE = 4;
        //public const int DUAL_SCOPE = 2;

        double[][] _readingBuffers;
        double[][] _recordingSeries;
        System.Diagnostics.Stopwatch _clockingStopwatch = new System.Diagnostics.Stopwatch();

        double _millivoltInitial, _millivoltInitial_raw;
        double _millivoltVertex, _millivoltVertex_raw;
        double _millivoltLimit, _millivoltLimit_raw;
        double _countRepeat;
        double _millivoltScanrate, _millivoltScanrate_raw;
        double _millivoltAmplitude, _millivoltAmplitude_raw;
        double _millivoltStep, _millivoltStep_raw;
        double _countStep;
        double _hertzInitial;
        double _hertzFinal;
        double _secDuration;
        double _secPreelectrolysis;
        double _secInterval;
        uint _rpmRDE = 0;
        double _voltAnalogInChannelRange_Current = 0.0;
        bool _is_warned_about_potential_limit = false;
        double _coulombPassingThroughCell;

        bool _is_using_potential_switcher = false; 
        double _millivoltSwitchingPotential;
        double _secPotentialSwitchingInterval;

        string _file_path = null;
        TextWriter _writer;

        int[] _rotation_speeds = { 3600 };

        bool _is_switched_on = false;
        bool _is_using_automatical_switcher = false; 
        double _secSwitchingInterval = 1.0;
        bool _is_applying_switching_on_finally = false;
        
        ToolStripMenuItem[] _toolstripmenuitemsFrequencyOfAcquisition;
        ToolStripMenuItem[] _toolstripmenuitemsRange;
        // ToolStripMenuItem[] _toolstripmenuitemsFilteringMethod;
        ToolStripMenuItem[] _toolstripmenuitemsMode;

        DataTable[] _tablesMethods = { new DataTable("method_table1"), new DataTable("method_table2"), new DataTable("method_table3") };
        DataTable[] _tablesRanges = { new DataTable("range_table1"), new DataTable("range_table2"), new DataTable("range_table3") };
        DataTable _tableDevice = new DataTable("device_table1");
        //DataTable _tableMethod_galvano = new DataTable("method_table1");
        //DataTable _tableMethod_eis = new DataTable("method_table1");
        //DataTable _tableResistor;


        enum modeMeasurement : int
        {
            none = -1,
            voltammetry = 0,
            galvanometry,
            eis,
        }
        modeMeasurement _selectedMode = modeMeasurement.none;
        modeMeasurement _selectedMode_previous = modeMeasurement.none;

        public enum methodMeasurement : int
        {
            none = -1,
            Cyclicvoltammetry = 0,
            CyclicvoltammetryQuick,
            BulkElectrolysis,
            LSV,
            Series_of_RDE_CV,
            Series_of_RDE_LSV,
            DPSCA,
            IRC,
            OSWV,
            OCP,
            Oscilloscope,

            Cyclicgalvanometry,
            ConstantCurrent,

            EIS_Open_Circuit,
            EIS_Short_Circuit,
            EIS,
            EIS_MottSchottkey,
        }
        methodMeasurement _selectedMethod = methodMeasurement.none;
        methodMeasurement _selectedMethod_previous = methodMeasurement.none;

        public enum typeSubModule : int
        {
            RDE = 0,
            Synchronous_switching,
            Bipotentiostat
        }
        typeSubModule _selectedSubModule = typeSubModule.RDE;

        /*
         * [測定方法の追加方法]
         * 
         * 定数追加 
         *     methodMeasurement
         *     [Initialize ComboBox]
         * 
         * backgroundWorkerCV_DoWork
         *     _selectedMethod用のif文を追加。methodMeasurement.CyclicvoltammetryQuick用のif文が最も単純
         *     必要に応じてDoVoltammetryQuickのようなメソッドを追加してもよいが、とりあえずinlineでもよい
         * 
         * backgroundWorkerCV_ProgressChanged
         *     case (int) statusMeasurement.MeasuremetWasDoneとdefaultに対応分を追加
         * 
         * StartAcquisition
         *     caseを追加。一番近いものをコピーして始めるとよい。取り込み周波数の設定、取り込み電圧オフセットも必要に応じて設定する
         * 
         * updateComboBoxMethod
         *     UI変更分を追加
         * 
         */

        enum statusMeasurement : int
        {
            Initialized = -1,
            CAGotStarted = -2,
            MeasurementGotStarted = -3,
            MeasuremetWasDone = -4,
            MeasuremetWasCancelled = -5,
            BackgroundProcessWasDone = -6,
        }

        public enum rangeCurrent : long
        {
            RangeNone = -1,
            Range200mA = 100000, // 10ohm
            Range20mA = 10000, // 100ohm
            Range2mA = 1000, // 1kohm
            Range200uA = 100, // 10kohm
            Range20uA = 10, // 100kohm
            Range2uA = 1, // 1Mohm
            RangeRAW = 999
        }
        rangeCurrent _selectedCurrentRange = rangeCurrent.RangeNone;
        double _selectedCurrentFactor = Double.NaN;

        public enum rangePotential : long
        {
            RangeNone = -1,
            Range50V = 50000,
            Range5V = 5000,
            Range500mV = 500,
        }
        rangePotential _selectedPotentialRange = rangePotential.Range5V;

        double _hertzAcquisition = 400.0;
        //double _herzAcquisition_prev;
        int _itrRecording = -1;
        bool _is_using_digitalfilter = true;
        double _hertzTargetFilteringFrequency = 60.0;
        DigitalFilter_BiQuad _digital_filter_notch = new DigitalFilter_BiQuad();
        DigitalFilter_BiQuad _digital_filter_lowpass = new DigitalFilter_BiQuad();

        Arduino _arduino = new Arduino();

        [DllImport("USER32.DLL")] private static extern IntPtr GetSystemMenu(IntPtr hWnd, UInt32 bRevert);
        [DllImport("USER32.DLL")] private static extern UInt32 RemoveMenu(IntPtr hMenu, UInt32 nPosition, UInt32 wFlags);
        private const UInt32 SC_CLOSE = 0x0000F060;
        private const UInt32 MF_BYCOMMAND = 0x00000000;
        //[DllImport("shlwapi.dll", CallingConvention = CallingConvention.StdCall, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        //extern static bool PathCompactPathExW([Out] System.Text.StringBuilder ResultPath, String SourcePath, int HowManyLetters, int Delimiter);

        //[System.Runtime.InteropServices.DllImport("kernel32.dll")] // この行を追加
        //private static extern bool AllocConsole();
        [DllImport("kernel32.dll", ExactSpelling = true)] public static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")] public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
        public const int SWP_NOSIZE = 0x0001;
        [DllImport("user32.dll", CharSet = CharSet.Auto)] static extern int ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_SHOWMINNOACTIVE = 7;

        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)] public static extern int AllocConsole();
        [DllImport("Kernel32.dll")] public static extern bool AttachConsole(int processId);
        [DllImport("kernel32.dll", SetLastError = true)] public static extern bool SetStdHandle(int nStdHandle, IntPtr hHandle);
        public const int STD_OUTPUT_HANDLE = -11;
        public const int STD_INPUT_HANDLE = -10;
        public const int STD_ERROR_HANDLE = -12;
        [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)] public static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile([MarshalAs(UnmanagedType.LPTStr)] string filename,
                                               [MarshalAs(UnmanagedType.U4)] uint access,
                                               [MarshalAs(UnmanagedType.U4)] FileShare share,
                                                                                 IntPtr securityAttributes,
                                               [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
                                               [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
                                                                                 IntPtr templateFile);
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint GENERIC_READ = 0x80000000;

        private static void OverrideRedirection()
        {
            //var hOut = GetStdHandle(STD_OUTPUT_HANDLE);
            var hRealOut = CreateFile("CONOUT$", GENERIC_READ | GENERIC_WRITE, FileShare.Write, IntPtr.Zero, FileMode.OpenOrCreate, 0, IntPtr.Zero);
            //if (hRealOut != hOut)
            //{
                SetStdHandle(STD_OUTPUT_HANDLE, hRealOut);
                //Console.SetOut(new StreamWriter(Console.OpenStandardOutput(), Console.OutputEncoding) { AutoFlush = true });
            //}
        }

        public Potentiostat()
        {
#if DEBUG
            AllocConsole();
            OverrideRedirection();
#else
            //
            // Minimize console window
            //
            //IntPtr MyConsole = GetConsoleWindow();
            //ShowWindow(MyConsole, SW_SHOWMINNOACTIVE);
#endif

            InitializeComponent();

            //string target = "";
            //if (VERSION_0_5) { target = "0.5"; }
            //else if (VERSION_0_9c) { target = "0.9c"; }
            //else if (VERSION_0_9d) { target = "0.9d"; }
            //else if (VERSION_0_9e) { target = "0.9e"; }
            //else { }
            this.Text = "Voltammogrammer " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " for PocketPotentiostat " + VERSION_string;

            // 
            // Upgrade App.config
            //
            if (Properties.Settings.Default.IsUpgraded == false)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.IsUpgraded = true;
                Properties.Settings.Default.Save();
            }

            //
            // Initialize ComboBox
            //
            _tablesMethods[(int)modeMeasurement.voltammetry].Columns.Add("name");
            _tablesMethods[(int)modeMeasurement.voltammetry].Columns.Add("index", typeof(methodMeasurement));
            _tablesMethods[(int)modeMeasurement.voltammetry].Rows.Add("Cyclic Voltammetry (CV)", methodMeasurement.Cyclicvoltammetry);
            _tablesMethods[(int)modeMeasurement.voltammetry].Rows.Add("Cyclic Voltammetry (CV) in fast scanning mode", methodMeasurement.CyclicvoltammetryQuick);
            _tablesMethods[(int)modeMeasurement.voltammetry].Rows.Add("Bulk Electrolysis (CPE)", methodMeasurement.BulkElectrolysis);
            _tablesMethods[(int)modeMeasurement.voltammetry].Rows.Add("Linear Scan Voltammetry (LSV)", methodMeasurement.LSV);
            _tablesMethods[(int)modeMeasurement.voltammetry].Rows.Add("Series of RDE (CV mode)", methodMeasurement.Series_of_RDE_CV);
            _tablesMethods[(int)modeMeasurement.voltammetry].Rows.Add("Series of RDE (LSV mode)", methodMeasurement.Series_of_RDE_LSV);
            _tablesMethods[(int)modeMeasurement.voltammetry].Rows.Add("Double Potential Step ChronoAmperometry ", methodMeasurement.DPSCA);
            _tablesMethods[(int)modeMeasurement.voltammetry].Rows.Add("IR Compensation", methodMeasurement.IRC);
            _tablesMethods[(int)modeMeasurement.voltammetry].Rows.Add("Osteryoung Square Wave Voltammetry (OSWV)", methodMeasurement.OSWV);
            _tablesMethods[(int)modeMeasurement.voltammetry].Rows.Add("Open Circut Potential", methodMeasurement.OCP);
            _tablesMethods[(int)modeMeasurement.voltammetry].Rows.Add("Oscilloscope", methodMeasurement.Oscilloscope);

            _tablesRanges[(int)modeMeasurement.voltammetry].Columns.Add("name");
            _tablesRanges[(int)modeMeasurement.voltammetry].Columns.Add("index", typeof(rangeCurrent));
            _tablesRanges[(int)modeMeasurement.voltammetry].Rows.Add("+- 200 mA", rangeCurrent.Range200mA);
            _tablesRanges[(int)modeMeasurement.voltammetry].Rows.Add("+- 20 mA", rangeCurrent.Range20mA);
            _tablesRanges[(int)modeMeasurement.voltammetry].Rows.Add("+- 2 mA", rangeCurrent.Range2mA);
            _tablesRanges[(int)modeMeasurement.voltammetry].Rows.Add("+- 200 uA", rangeCurrent.Range200uA);
            _tablesRanges[(int)modeMeasurement.voltammetry].Rows.Add("+- 20 uA", rangeCurrent.Range20uA);
            _tablesRanges[(int)modeMeasurement.voltammetry].Rows.Add("+- 2 uA", rangeCurrent.Range2uA);
            //for (int i = 0; i < 6; i++)
            //{
            //    // Override with a custom value
            //    _tablesRanges[(int)modeMeasurement.voltammetry].Rows[i][0] = _tableResistor.Rows[i][2];
            //}
            _tablesRanges[(int)modeMeasurement.voltammetry].Rows.Add("+- 200 uA (raw output)", rangeCurrent.RangeRAW);

            _tablesMethods[(int)modeMeasurement.galvanometry].Columns.Add("name");
            _tablesMethods[(int)modeMeasurement.galvanometry].Columns.Add("index", typeof(methodMeasurement));
            _tablesMethods[(int)modeMeasurement.galvanometry].Rows.Add("Cyclic Galvanometry (CG)", methodMeasurement.Cyclicgalvanometry);
            _tablesMethods[(int)modeMeasurement.galvanometry].Rows.Add("Constant Current", methodMeasurement.ConstantCurrent);
            //_tablesMethods[(int)modeMeasurement.galvanometry].Rows.Add("Linear Scan Galvanometry (LSG)");
            //_tablesMethods[(int)modeMeasurement.galvanometry].Rows.Add("Series of RDE (CG mode)");
            //_tablesMethods[(int)modeMeasurement.galvanometry].Rows.Add("Series of RDE (LSG mode)");
            //_tablesMethods[(int)modeMeasurement.galvanometry].Rows.Add("Double Potential Step Chronopotentiometry ");
            //_tablesMethods[(int)modeMeasurement.galvanometry].Rows.Add("IR Compensation");
            //_tablesMethods[(int)modeMeasurement.galvanometry].Rows.Add("Osteryoung Square Wave Galvanometry (OSWG)");
            //_tablesMethods[(int)modeMeasurement.galvanometry].Rows.Add("Open Circut Potential");
            _tablesRanges[(int)modeMeasurement.galvanometry] = _tablesRanges[(int)modeMeasurement.voltammetry];

            _tablesMethods[(int)modeMeasurement.eis].Columns.Add("name");
            _tablesMethods[(int)modeMeasurement.eis].Columns.Add("index", typeof(methodMeasurement));
            _tablesMethods[(int)modeMeasurement.eis].Rows.Add("Compensation: Open", methodMeasurement.EIS_Open_Circuit);
            _tablesMethods[(int)modeMeasurement.eis].Rows.Add("Compensation: Short", methodMeasurement.EIS_Short_Circuit);
            _tablesMethods[(int)modeMeasurement.eis].Rows.Add("Measurement (Cole-Cole plot)", methodMeasurement.EIS);
            _tablesMethods[(int)modeMeasurement.eis].Rows.Add("Measurement (Mott-Schottkey plot)", methodMeasurement.EIS_MottSchottkey);

            _tablesRanges[(int)modeMeasurement.eis].Columns.Add("name");
            _tablesRanges[(int)modeMeasurement.eis].Columns.Add("index", typeof(rangeCurrent));
            _tablesRanges[(int)modeMeasurement.eis].Rows.Add("10 Ohm", rangeCurrent.Range200mA);
            _tablesRanges[(int)modeMeasurement.eis].Rows.Add("100 Ohm", rangeCurrent.Range20mA);
            _tablesRanges[(int)modeMeasurement.eis].Rows.Add("1 kOhm", rangeCurrent.Range2mA);
            _tablesRanges[(int)modeMeasurement.eis].Rows.Add("10 kOhm", rangeCurrent.Range200uA);
            _tablesRanges[(int)modeMeasurement.eis].Rows.Add("100 kOhm", rangeCurrent.Range20uA);
            _tablesRanges[(int)modeMeasurement.eis].Rows.Add("1 MOhm", rangeCurrent.Range2uA);
            //for (int i = 0; i < 6; i++)
            //{
            //    // Override with a custom value
            //    _tablesRanges[(int)modeMeasurement.eis].Rows[i][0] = _tableResistor.Rows[i][3];
            //}

            //
            // Initialize Custom Resistor Values, and Override Resistors With CustomValues
            //
            _configure_potentiostat = new Configure_Potentiostat(this);
            //UpdateResistors();

            //
            // Initialize the filtering method
            //
            //_toolstripmenuitemsFilteringMethod = new ToolStripMenuItem[] { toolStripMenuItemFiltering60Hz, toolStripMenuItemFiltering50Hz };
            //SetFilteringMethod((Properties.Settings.Default.configure_filtering_method));

            toolStripComboBoxMethod.ComboBox.DisplayMember = "name";
            toolStripComboBoxMethod.ComboBox.ValueMember = "index";

            toolStripComboBoxRange.ComboBox.DisplayMember = "name";
            toolStripComboBoxRange.ComboBox.ValueMember = "index";


            //
            // Initialize the reference type for the initial potential
            //
            toolStripComboBoxReferenceForInitialPotential.SelectedIndex = (Properties.Settings.Default.configure_referencing_for_initial_potential);

            //
            // Initialize Others
            //
            _select_rotation_speeds = new Select_RotationSpeed(this);
            _calibrate_potentiostat = new Calibrate_Potentiostat(this, _configure_potentiostat);
            _toolstripmenuitemsFrequencyOfAcquisition = new ToolStripMenuItem[] { hzToolStripMenuItem1, hzToolStripMenuItem2, hzToolStripMenuItem3, hzToolStripMenuItem4, hzToolStripMenuItem5, hzToolStripMenuItem6, hzToolStripMenuItem7, hzToolStripMenuItem8 };
            _toolstripmenuitemsRange = new ToolStripMenuItem[] { toolStripMenuItemRange1, toolStripMenuItemRange2, toolStripMenuItemRange3 };
            _toolstripmenuitemsMode = new ToolStripMenuItem[] { toolStripMenuPotentioStat, toolStripMenuGalvanoStat, toolStripMenuEIS };

            //
            // Disable the [x] button in the Console window
            // (if you handle Ctrl+C, use SetConsoleCtrlHandler Win32-API instead)
            //
            IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hWnd != IntPtr.Zero)
            {
                IntPtr hMenu = GetSystemMenu(hWnd, 0);
                RemoveMenu(hMenu, SC_CLOSE, MF_BYCOMMAND);
            }
        }

        // Functions for analog I/O pins from AnalogDiscovery 2
        //
        //

        private void SetDCVoltageCH1(double microvoltOffset)
        {
            FDwfAnalogOutNodeFunctionSet(_handle, 0, AnalogOutNodeCarrier, funcDC);
            FDwfAnalogOutNodeOffsetSet(_handle, 0, AnalogOutNodeCarrier, microvoltOffset / 1000000.0);
            FDwfAnalogOutConfigure(_handle, 0, Convert.ToInt32(true));

            return;
        }

        private void SetDCVoltageCH2(double microvoltOffset)
        {
            FDwfAnalogOutNodeFunctionSet(_handle, 1, AnalogOutNodeCarrier, funcDC);
            FDwfAnalogOutNodeOffsetSet(_handle, 1, AnalogOutNodeCarrier, microvoltOffset / 1000000.0);
            FDwfAnalogOutConfigure(_handle, 1, Convert.ToInt32(true));

            return;
        }

        private void SampleSingleValue(int idxChannel, out double value)
        {
            value = 0.0;

            byte sts = 0;
            if (FDwfAnalogInStatus(_handle, Convert.ToInt32(true), out sts) == 0)
            {
                Console.WriteLine("Error: STS = " + sts.ToString());

                //timerCurrentEandI.Enabled = false;
                //toolStripButtonRecord.Enabled = false;
                //toolStripButtonScan.Enabled = false;
                //toolStripButtonConnect.Enabled = true;
                //MessageBox.Show("Failed to power PocketPotentiostat on. \nPlease check the auxiliary power supply.", "Error Opening Device", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //Console.WriteLine("Failed to power PocketPotentiostat on");
                //FDwfDeviceClose(_handle);

                return;
            }

            //double value = 0;

            FDwfAnalogInStatusSample(_handle, idxChannel, out value);
            //Console.WriteLine("Single sample (raw value): " + (value).ToString());

            return;
        }

        private void DoVoltammetry_virtual()
        {
            if (_file_path == null) return;

            int cSamples = 0;

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
            System.Text.RegularExpressions.Regex r0 =
                new System.Text.RegularExpressions.Regex
                (
                    @"^\s*(?:(Ewe/V|I/mA|[^\t]+)\t*)+$",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline
                );

            TextReader reader = new StreamReader(_file_path);

            string line; int line_number = 1; int idxPotential = 0, idxCurrent = 0, idxTime = 0; int line_header = 73;
            double potential, current, time, p = Double.NaN, t = Double.NaN, c = Double.NaN, q = Double.NaN;
            while ((line = reader.ReadLine()) != null)
            {
                switch (line_number)
                {
                    case 1: // EC-Lab ASCII FILE
                        break;

                    case 2: // Nb header lines : 2
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
                                System.Text.RegularExpressions.CaptureCollection cc1 = mcs2[0].Groups[1].Captures;
                                for (int i = 1; i < cc1.Count; i++)
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
                                    System.Text.RegularExpressions.CaptureCollection cc = mcs[0].Groups[1].Captures;
                                    if (Double.TryParse(cc[idxPotential].Value, out potential) && Double.TryParse(cc[idxCurrent].Value, out current) && Double.TryParse(cc[idxTime].Value, out time))
                                    {
                                        //potential *= 1000.0; current *= 1000.0;

                                        _recordingSeries[0][cSamples] = time*1000;
                                        _recordingSeries[CHANNEL_POTENTIAL][cSamples] = potential;
                                        _recordingSeries[CHANNEL_CURRENT][cSamples] = current * 1000 / ((double)_selectedCurrentFactor);

                                        backgroundWorkerCV.ReportProgress(cSamples++);
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                            }
                        }
                        break;
                }

                line_number++;
                //Thread.Sleep(1);
                Thread.Yield();
            }

            reader.Close();
        }

        private void DoVoltammetryQuick(ref double[] rgdSamples, double secRecording, double millivoltHeight, ref DoWorkEventArgs e, bool fCA = true)
        {

            byte sts, sts2;

            //FDwfAnalogInReset(_handle);
            //SelectPotentialRange(_selectedPotentialRange);

            FDwfAnalogInAcquisitionModeSet(_handle, acqmodeSingle1);
            //FDwfAnalogInAcquisitionModeSet(_handle, acqmodeRecord);
            //FDwfAnalogInFrequencySet(_handle, (1.0 / secRecording) * 5000); // 取り込み周波数を設定
            //Console.WriteLine($"FDwfAnalogInFrequencySet: {(1.0 / secRecording) * 5000}");
            FDwfAnalogInFrequencySet(_handle, _hertzAcquisition);
            Console.WriteLine($"FDwfAnalogInFrequencySet: {_hertzAcquisition}");
            FDwfAnalogInChannelFilterSet(_handle, -1, filterAverage);
            FDwfAnalogInTriggerAutoTimeoutSet(_handle, 0);
            FDwfAnalogInTriggerSourceSet(_handle, trigsrcPC);
            FDwfAnalogInTriggerPositionSet(_handle, (secRecording)*0.5);

            //Thread.Sleep(500); Thread.Yield();
            FDwfAnalogInConfigure(_handle, Convert.ToInt32(false), Convert.ToInt32(true));


            while (true)
            {
                Thread.Sleep(500); Thread.Yield();
                FDwfAnalogInStatus(_handle, Convert.ToInt32(false), out sts);
                Console.WriteLine($"Wait for stsArm, (In): {sts}");
                if ((sts == stsArm)) break;
            }
            //Thread.Sleep(3000); Thread.Yield();




            backgroundWorkerCV.ReportProgress((int)statusMeasurement.CAGotStarted);
            // まずは3秒間のCAでWEの充電電流を落ち着かせる
            double millivoltCurrent = _millivoltInitial;
            SetDCVoltageCH1(millivoltCurrent * 1000);
            SetCircuit(open: false);


            _clockingStopwatch.Restart();


            if(fCA)
            {
                long t1, t2;
                t1 = _clockingStopwatch.ElapsedMilliseconds;
                do
                {
                    Thread.Sleep(1);

                    t2 = _clockingStopwatch.ElapsedMilliseconds;
                }
                while (t2 < (3 * 1000));
                Console.WriteLine("CA for 3 seconds end.");
                //System.Threading.Thread.Sleep(3 * 1000);

            }





            FDwfAnalogOutNodeEnableSet(_handle, 0, AnalogOutNodeCarrier, Convert.ToInt32(true));
            FDwfAnalogOutNodeFunctionSet(_handle, 0, AnalogOutNodeCarrier, funcCustom);
            FDwfAnalogOutNodeDataSet(_handle, 0, AnalogOutNodeCarrier, rgdSamples, 4096);
            FDwfAnalogOutNodeFrequencySet(_handle, 0, AnalogOutNodeCarrier, (1.0 / secRecording) * 1); // 出力波形の周波数を設定
            FDwfAnalogOutNodeAmplitudeSet(_handle, 0, AnalogOutNodeCarrier, (millivoltHeight/ 1000)); // 波形の出力係数。1.0だとそのままの電圧で出力される。
            //FDwfAnalogOutNodeOffsetSet(_handle, 0, AnalogOutNodeCarrier, (_millivoltInitial / 1000)); // 出力波形のオフセット。但し、ここではOffsetを変更しない(変更すると電圧スパイクが出てしまう)。
            //FDwfAnalogOutNodePhaseSet(_handle, 0, AnalogOutNodeCarrier, 0.0);
            FDwfAnalogOutTriggerSourceSet(_handle, 0, trigsrcPC);
            FDwfAnalogOutRepeatSet(_handle, 0, 1);
            FDwfAnalogOutRunSet(_handle, 0, secRecording);

            //Thread.Sleep(500); Thread.Yield();
            FDwfAnalogOutConfigure(_handle, 0, Convert.ToInt32(true));




            //while(true)
            //{
            //    Thread.Sleep(100); Thread.Yield();
            //    FDwfAnalogInStatus(_handle, Convert.ToInt32(false), out sts);
            //    FDwfAnalogOutStatus(_handle, Convert.ToInt32(false), out sts2);
            //    Console.WriteLine($"Wait for stsArm, (In, Out): {sts}, {sts2}");
            //    if ((sts == stsArm) && (sts2 == stsArm)) break;
            //}
            while (true)
            {
                Thread.Sleep(100); Thread.Yield();
                FDwfAnalogOutStatus(_handle, Convert.ToInt32(false), out sts2);
                Console.WriteLine($"Wait for stsArm, (Out): {sts2}");
                if ((sts2 == stsArm)) break;
            }
            Thread.Sleep(2000); Thread.Yield();



            backgroundWorkerCV.ReportProgress((int)statusMeasurement.MeasurementGotStarted);

            Thread.Sleep(100);
            FDwfDeviceTriggerPC(_handle);

            //byte sts = 0;
            int cSamples = 0;
            int cAvailable = 0, cLost = 0, cCorrupted = 0;

            while (true)
            {
                if (backgroundWorkerCV.CancellationPending)
                {
                    e.Cancel = true;

                    break;
                }

                Thread.Sleep(2000);

                if (FDwfAnalogInStatus(_handle, Convert.ToInt32(true), out sts) == 0)
                {
                    Console.WriteLine("error");
                    return;
                }
                if ((sts != stsDone))
                {
                    // Acquisition not yet started.
                    continue;
                }
                if ((sts == stsCfg || sts == stsPrefill || sts == stsArm))
                {
                    // Acquisition not yet started.
                    continue;
                }

                FDwfAnalogInStatusRecord(_handle, out cAvailable, out cLost, out cCorrupted);

                //cSamples += cLost;
                //cSamples += cCorrupted;

                if (cCorrupted > 0)
                {
                    Console.WriteLine("Data corrupted!");
                }

                if (cLost > 0)
                {
                    Console.WriteLine("Data lost!");
                }

                if (cAvailable > 0)
                {
                    for (int i = 0; i < cAvailable; i++)
                    {
                        //_recordingSeries[0][cSamples + i] = cSamples + i;
                        //_recordingSeries[0][cSamples + i] = (double)(i) / ((1.0 / secRecording)*5000) * 1000;
                        _recordingSeries[0][cSamples + i] = (double)(i) / (_hertzAcquisition) * 1000;

                    }

                    FDwfAnalogInStatusData(_handle, (CHANNEL_POTENTIAL - 1), _readingBuffers[(CHANNEL_POTENTIAL - 1)], cAvailable);
                    Array.Copy(_readingBuffers[(CHANNEL_POTENTIAL - 1)], 0, _recordingSeries[CHANNEL_POTENTIAL], cSamples, cAvailable);
                    FDwfAnalogInStatusData(_handle, (CHANNEL_CURRENT - 1), _readingBuffers[(CHANNEL_CURRENT - 1)], cAvailable);
                    Array.Copy(_readingBuffers[(CHANNEL_CURRENT - 1)], 0, _recordingSeries[CHANNEL_CURRENT], cSamples, cAvailable);

                    cSamples += cAvailable;
                    if (cSamples > 8192) cSamples = 8192;

                    backgroundWorkerCV.ReportProgress(cSamples);

                    Console.WriteLine($"cSamples: {cSamples}");
                    //if(cSamples > 8000) break;
                    break;

                }
            }

            //FDwfAnalogInStatusRecord(_handle, out cAvailable, out cLost, out cCorrupted);

            FDwfAnalogOutConfigure(_handle, 0, Convert.ToInt32(false));
            FDwfAnalogOutReset(_handle, 0);
            //FDwfAnalogInReset(_handle);

            //FDwfDeviceAutoConfigureSet(_handle, Convert.ToInt32(false));

            FDwfAnalogInConfigure(_handle, Convert.ToInt32(false), Convert.ToInt32(false));

            FDwfAnalogInReset(_handle);
            SelectPotentialRange(_selectedPotentialRange);


            return;
        }

        private void DoVoltammetry(ref double[] rgdSamples, double secRecording, double millivoltHeight, ref DoWorkEventArgs e, bool fCA = true, bool fFinal = true)
        {
            //double herzAcquisition = 40.0;
            double secCAing = (fCA) ? _secPreelectrolysis : 0.0;
            //double secDelta = 0.03;
            double secDelta = 1 / _hertzTargetFilteringFrequency * 2; // digital filterによる遅延時間。この分だけ、余分にデータを収集する必要がある。

            //if (!fCA)
            //{
            //    secCAing = 0.0;
            //}

            // オシロ側を設定する
            // recording rate for more samples than the device buffer is limited by device communication
            FDwfAnalogInAcquisitionModeSet(_handle, acqmodeRecord); 
            FDwfAnalogInFrequencySet(_handle, ((double)_hertzAcquisition)); // 取り込み周波数を設定
            Console.WriteLine($"FDwfAnalogInFrequencySet: {_hertzAcquisition}");
            FDwfAnalogInChannelFilterSet(_handle, -1, filterAverage);
            FDwfAnalogInRecordLengthSet(_handle, (secCAing + secRecording + secDelta)); // 取り込みする時間[s]の設定。サンプル数[] / 周波数[s-1]


            _clockingStopwatch.Restart();

            switch (_selectedSubModule)
            {
                case typeSubModule.RDE:
                    break;

                case typeSubModule.Synchronous_switching:
                    //
                    // TODO: Initially On?
                    // 

                    break;

                default:
                    break;
            }

            // AWGの準備
            // enable first channel
            FDwfAnalogOutNodeEnableSet(_handle, 0, AnalogOutNodeCarrier, Convert.ToInt32(true));
            //FDwfDeviceAutoConfigureSet(_handle, Convert.ToInt32(true));
            double millivoltCurrent = _millivoltInitial;
            SetDCVoltageCH1(millivoltCurrent * 1000.0);


            SetCircuit(open: false);

            if(fCA)
            {
                // まずは3秒間のCAでWEの充電電流を落ち着かせる
                backgroundWorkerCV.ReportProgress((int)statusMeasurement.CAGotStarted);

                long t1, t2;
                t1 = _clockingStopwatch.ElapsedMilliseconds;
                do
                {
                    Thread.Sleep(1);

                    t2 = _clockingStopwatch.ElapsedMilliseconds;
                }
                while (t2 < (3 * 1000));
                Console.WriteLine($"CA for 3 seconds end.");
                //System.Threading.Thread.Sleep(3 * 1000);
            }

            byte sts = 0;
            int cSamples = 0;
            int cAvailable = 0, cLost = 0, cCorrupted = 0;
            bool fCVing = false;
            long t_previous_acquisition = 0, t_current_acquisition = 0;
            long t_current = 0, t_next = -1, t_cnt = 0; bool print_once = true;
            int cSwitching = 1;
            bool was_output = false;

            backgroundWorkerCV.ReportProgress((int)statusMeasurement.MeasurementGotStarted);

            // start
            // 開始
            FDwfAnalogInConfigure(_handle, Convert.ToInt32(false), Convert.ToInt32(true));
            //FDwfAnalogOutConfigure(_handle, 0, Convert.ToInt32(true));

            _clockingStopwatch.Restart();

            //while (cSamples <= ((secCAing + secRecording + secDelta) * _herzAcquisition))
            while (cSamples <= (int)Math.Floor((secCAing + secRecording + 0) * _hertzAcquisition) - 1)
            {
                if ((cSamples >= (secCAing * _hertzAcquisition)) && (fCVing == false))
                {
                    Console.WriteLine(_clockingStopwatch.ElapsedMilliseconds);

                    // いよいよCV測定。AWGに波形等を設定する
                    //FDwfAnalogOutConfigure(_handle, 0, Convert.ToInt32(false)); // 一旦止めなくてもよい？？
                    // set custom function
                    FDwfAnalogOutNodeFunctionSet(_handle, 0, AnalogOutNodeCarrier, funcCustom);
                    // set custom waveform samples
                    // normalized to ｱ1 values
                    FDwfAnalogOutNodeDataSet(_handle, 0, AnalogOutNodeCarrier, rgdSamples, 4096);
                    // 10kHz waveform frequency
                    FDwfAnalogOutNodeFrequencySet(_handle, 0, AnalogOutNodeCarrier, (1 / secRecording)); // 出力波形の周波数を設定
                                                                                                         // 2V amplitude, 4V pk2pk, for sample value -1 will output -2V, for 1 +2V
                    FDwfAnalogOutNodeAmplitudeSet(_handle, 0, AnalogOutNodeCarrier, millivoltHeight / 1000.0); // 波形の出力係数。1.0だとそのままの電圧で出力される。
                                                                                                               // by default the offset is 0V
                                                                                                               //FDwfAnalogOutNodeOffsetSet(hdwf, 0, AnalogOutNodeCarrier, 0.0); // 出力波形のオフセット。但し、ここではOffsetを変更しない(変更すると電圧スパイクが出てしまう)。
                    FDwfAnalogOutRunSet(_handle, 0, secRecording);
                    //FDwfAnalogOutRepeatSet(_handle, 0, 1);

                    FDwfAnalogOutConfigure(_handle, 0, Convert.ToInt32(true));

                    Console.WriteLine(_clockingStopwatch.ElapsedMilliseconds);
                    fCVing = true;
                }

                t_current = _clockingStopwatch.ElapsedMilliseconds;
                if (t_current > t_next)
                {
                    if (backgroundWorkerCV.CancellationPending)
                    {
                        e.Cancel = true;

                        break;
                    }

                    if (FDwfAnalogInStatus(_handle, Convert.ToInt32(true), out sts) == 0)
                    {
                        Console.WriteLine("error");
                        return;
                    }
                    if (cSamples == 0 && (sts == stsCfg || sts == stsPrefill || sts == stsArm))
                    {
                        // Acquisition not yet started.
                        continue;
                    }
                    if(cSamples == 0)
                    {
                        Console.WriteLine($"Initial time after stsTrig: {_clockingStopwatch.ElapsedMilliseconds} ms, (sts code: {sts})");
                    }

                    FDwfAnalogInStatusRecord(_handle, out cAvailable, out cLost, out cCorrupted);

                    cSamples += cLost;
                    cSamples += cCorrupted;

                    if (cCorrupted > 0)
                    {
                        Console.WriteLine("Data corrupted!");
                    }

                    if (cLost > 0)
                    {
                        Console.WriteLine("Data lost!");
                    }

                    if (cAvailable > 0)
                    {
                        t_current = _clockingStopwatch.ElapsedMilliseconds;
                        t_current_acquisition = t_current;// _clockingStopwatch.ElapsedMilliseconds;
                        //_recordingSeries[0][cSamples] = (t_current_acquisition);

                        // AWGに波形設定をすると幾分かの遅れが発生するので、その間は記録しない。
                        // 高速掃引時には、このdeltaが問題となるが、現状では未対処(as of 11/11/2018)。
                        const double delta = 0.20;
                        if( true || (cSamples < (((secCAing - 0.10) * _hertzAcquisition)) || cSamples > ((secCAing + delta) * _hertzAcquisition)) )
                        {
                            for (int i = 0; i < cAvailable; i++)
                            {
                                _recordingSeries[0][cSamples + i] = t_current_acquisition + i * (1.0 / _hertzAcquisition) * 1000;
                            }

                            //if (print_once) { Console.WriteLine($"FDwfAnalogInStatusData: {_clockingStopwatch.ElapsedMilliseconds}"); }
                            FDwfAnalogInStatusData(_handle, (CHANNEL_POTENTIAL-1), _readingBuffers[(CHANNEL_POTENTIAL - 1)], cAvailable);
                            //_recordingSeries[1][cSamples] = (_readingBuffers[0][0] * -1000.0);// * -1)*1000.0;// とりあえず最初の値のみ記録。平均しない。
                            Array.Copy(_readingBuffers[(CHANNEL_POTENTIAL - 1)], 0, _recordingSeries[CHANNEL_POTENTIAL], cSamples, cAvailable);
                            FDwfAnalogInStatusData(_handle, (CHANNEL_CURRENT - 1), _readingBuffers[(CHANNEL_CURRENT - 1)], cAvailable);
                            //_recordingSeries[2][cSamples] = (_readingBuffers[1][0]) * 1000.0;// とりあえず最初の値のみ記録。平均しない。
                            Array.Copy(_readingBuffers[(CHANNEL_CURRENT - 1)], 0, _recordingSeries[CHANNEL_CURRENT], cSamples, cAvailable);
                            //if (print_once) { Console.WriteLine($"FDwfAnalogInStatusData: {_clockingStopwatch.ElapsedMilliseconds}"); print_once = false; }

                            //if((cSamples % 10)==0)
                            //{
                            if(++t_cnt == 100)
                            {
                                // この時点でwhileの条件を満たさなくなっている可能性があるので、cSamples += cAvailableする前にReportProgressしておく。
                                backgroundWorkerCV.ReportProgress(cSamples); t_cnt = 0;
                            }
                            //}
                            cSamples += cAvailable;
                            //Console.WriteLine("cAvailable: " + cAvailable.ToString());
                        }
                        else
                        {
                            //Console.WriteLine("checking timeline...");
                            int c = 0;
                            while(_recordingSeries[0][cSamples - 1 - c] > t_current_acquisition)
                            {
                                c++;
                            }
                            cSamples = cSamples - c;
                            //if(c>0)
                            //{
                            //    Console.WriteLine("c>0: {0}", c);
                            //}

                            for (int i = 0; i < cAvailable; i++)
                            {
                                _recordingSeries[0][cSamples + i] = t_current_acquisition + i * (1.0 / _hertzAcquisition) * 1000;
                                //Console.WriteLine(_recordingSeries[0][cSamples + i]);
                            }

                            FDwfAnalogInStatusData(_handle, (CHANNEL_POTENTIAL-1), _readingBuffers[(CHANNEL_POTENTIAL - 1)], cAvailable);
                            Array.Copy(_readingBuffers[(CHANNEL_POTENTIAL - 1)], 0, _recordingSeries[CHANNEL_POTENTIAL], cSamples, cAvailable);
                            FDwfAnalogInStatusData(_handle, (CHANNEL_CURRENT - 1), _readingBuffers[(CHANNEL_CURRENT - 1)], cAvailable);
                            Array.Copy(_readingBuffers[(CHANNEL_CURRENT - 1)], 0, _recordingSeries[CHANNEL_CURRENT], cSamples, cAvailable);

                            cSamples += cAvailable;

                            if(cSamples > ((secCAing + delta) * _hertzAcquisition))
                            {
                                backgroundWorkerCV.ReportProgress(cSamples);
                            }
                        }

                        t_previous_acquisition = t_current_acquisition;
                    }
                    else
                    {
                        if ((cSamples % 10) == 0)
                        {
                            if(was_output == false)
                            {
                                Console.WriteLine($"cAvailable: {cAvailable}, cSamples: {cSamples} (maxSamples: {Math.Floor((secCAing + secRecording) * _hertzAcquisition)}), cLost: {cLost}, sts code: {sts}");
                                was_output = true;
                            }
                        }
                        else { was_output = false; }

                        if(cSamples > 0 && (sts == stsDone))
                        {
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    //if (backgroundWorkerCV.CancellationPending)
                    //{
                    //    e.Cancel = true;

                    //    break;
                    //}

                    // とりあえず50msにしてあるだけで、高速掃引時には未対処 as of 11/11/2018。
                    // 200msの場合、0->1V, 10V/sの条件で、1取り込み以内に測定が終わることになる
                    t_next = t_current + 10; // ReportProgressは20回に1回だけ実行するようにして、処理速度を稼ぐ
                }

                switch (_selectedSubModule)
                {
                    case typeSubModule.RDE:
                        break;

                    case typeSubModule.Synchronous_switching:                            
                        if(_is_using_automatical_switcher && t_current > (secCAing + _secSwitchingInterval * cSwitching) * 1000)
                        {
                            SetSwitchOnAsync(!_is_switched_on);
                            cSwitching++;
                        }
                        break;

                    default:
                        break;
                }

                //Thread.Sleep(1);
                Thread.Yield();
            }

            Console.WriteLine($"final sts code: {sts}, cSamples: {cSamples} (maxSamples: {Math.Floor((secCAing + secRecording) * _hertzAcquisition)})");

            if (backgroundWorkerCV.CancellationPending)
            {
                backgroundWorkerCV.ReportProgress(cSamples);
            }
            else
            {
                // secDeltaの分だけ最後に余分のデータがあるので、幾分か削る必要がある。「-5」はその場しのぎの値。
                //int cEnd = (int)Math.Floor((secCAing + secRecording + secDelta) * _herzAcquisition) - 5;
                int cEnd = (int)Math.Floor((secCAing + secRecording + 0) * _hertzAcquisition) - 1;
                backgroundWorkerCV.ReportProgress(cEnd);
            }

            if(fFinal)
            {
                Console.WriteLine("Finalizing CV...");

                FDwfAnalogOutConfigure(_handle, 0, Convert.ToInt32(false));
                FDwfAnalogOutReset(_handle, 0);
                //FDwfDeviceAutoConfigureSet(_handle, Convert.ToInt32(false));

                FDwfAnalogInConfigure(_handle, Convert.ToInt32(false), Convert.ToInt32(false));
            }

            switch (_selectedSubModule)
            {
                case typeSubModule.RDE:
                    break;

                case typeSubModule.Synchronous_switching:
                    //
                    // TODO: Finally On?
                    // 
                    if(_is_using_automatical_switcher) SetSwitchOnAsync(_is_applying_switching_on_finally);
                    break;

                default:
                    break;
            }

            return;
        }

        //
        //
        // Event Handlers
        //
        //

        #region Event Handlers for Form
        //
        // Event Handlers for Form
        //

        private void Potentiostat_Load(object sender, EventArgs e)
        {
            chartVoltammogram.Series[0].Points.AddXY(0, 0);
            chartVoltammogram.Update();

            _voltammogram.Show();

            //toolStripComboBoxMethod.SelectedIndex = 0;
            //toolStripComboBoxRange.SelectedIndex = 2;

            if (DIGITALIO_FROM_ARDUINO)
            {
                _arduino.setPortNameToToolStripComboBox(this.toolStripComboBoxSerialPort);
            }


            // enumerate multiple AD2...

            _tableDevice.Columns.Add("SN");
            _tableDevice.Columns.Add("idx", typeof(int));

            FDwfEnum(enumfilterAll, out int c);

            for (int i = 0; i < c; i++)
            {
                FDwfEnumDeviceType(i, out int type, out int rev);
                if (type == devidDiscovery2)
                {
                    FDwfEnumSN(i, out string sn);

                    _tableDevice.Rows.Add(sn, i);
                }
            }

            toolStripComboBoxSerialPort.ComboBox.DisplayMember = "SN";
            toolStripComboBoxSerialPort.ComboBox.ValueMember = "idx";
            toolStripComboBoxSerialPort.ComboBox.BindingContext = this.BindingContext; // toolstripComboBoxにはこれが必要
            toolStripComboBoxSerialPort.ComboBox.DataSource = _tableDevice;
            Console.WriteLine($"Default device: {toolStripComboBoxSerialPort.ComboBox.Text}");

            //toolStripContainer1.SuspendLayout();
            //toolStripContainer1.TopToolStripPanel.SuspendLayout();
            //toolStrip1.SuspendLayout();

            toolStripContainer1.TopToolStripPanel.Join(toolStrip1, 1);
            toolStripContainer1.TopToolStripPanel.Join(toolStrip2, 2);

            //toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            //toolStripContainer1.TopToolStripPanel.PerformLayout();
            //toolStripContainer1.ResumeLayout(false);
            //toolStripContainer1.PerformLayout();
            //toolStrip1.ResumeLayout(false);
            //toolStrip1.PerformLayout();
        }

        private void Potentiostat_KeyPress(object sender, KeyPressEventArgs e)
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
                        //chartVoltammogram.ChartAreas[0].CursorX.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.NotSet;
                        //chartVoltammogram.ChartAreas[0].CursorY.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.NotSet;

                        System.IO.MemoryStream memStream = new System.IO.MemoryStream();

                        //bool scrollbar_x = false, scrollbar_y = false;
                        //if (chartVoltammogram.ChartAreas[0].AxisX.ScrollBar.IsVisible)
                        //{
                        //    scrollbar_x = true;
                        //    chartVoltammogram.ChartAreas[0].AxisX.ScrollBar.Enabled = false;
                        //}
                        //if (chartVoltammogram.ChartAreas[0].AxisY.ScrollBar.IsVisible)
                        //{
                        //    scrollbar_y = true;
                        //    chartVoltammogram.ChartAreas[0].AxisY.ScrollBar.Enabled = false;
                        //}
                        chartVoltammogram.ChartAreas[0].CursorX.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.NotSet;
                        chartVoltammogram.ChartAreas[0].CursorY.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.NotSet;

                        //chartVoltammogram.ChartAreas[0].CursorX.

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

                    }
                    break;

                default:
                    break;
            }
        }

        private void Potentiostat_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((_handle != 0 && _handle != -1) && backgroundWorkerCV.IsBusy)
            {
                DialogResult r = MessageBox.Show
                (
                    "Do you stop the current measurement?",
                    "Warning",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if(r == DialogResult.Yes)
                {
                    backgroundWorkerCV.CancelAsync();

                    toolStripButtonRecord.Enabled = false;
                    toolStripButtonRecord.Text = "Stopping...";
                }

                e.Cancel = true;
            }
            else
            {
                DialogResult r = MessageBox.Show
                (
                    _configure_potentiostat,
                    "Do you close Voltammogrammer?",
                    "Warning",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if(r == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        private void Potentiostat_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_handle != 0 && _handle != -1)
            {
                TurnPowerSupply(on: false);
                FDwfDeviceClose(_handle);
            }
             FDwfDeviceCloseAll();

            saveFileDialog1.Dispose();
            _arduino.Close();
            //_voltammogram.Close();
        }
        
        #endregion

        //
        // Event Handlers for backgroundWorker
        //

        private void backgroundWorkerCV_DoWork(object sender, DoWorkEventArgs e)
        {
            bool is_initial_potential_referring_to_OCP = ((Properties.Settings.Default.configure_referencing_for_initial_potential) == 1);
            double ocp = 0.0, ocp_uncorrected = 0.0;

            switch (_selectedSubModule)
            {
                case typeSubModule.RDE:
                    SetPurging(on: false);
                    break;

                case typeSubModule.Synchronous_switching:
                    break;

                default:
                    break;
            }

            //
            // Initialがvs OCPの場合、OCPを取得して、それをInitialに加算。
            //
            if(is_initial_potential_referring_to_OCP && (_selectedMode != modeMeasurement.galvanometry))
            {
                Console.WriteLine("Starting to measure OCP...");

                FDwfAnalogInBufferSizeGet(_handle, out int maxBuffer);
                FDwfAnalogInBufferSizeSet(_handle, 10);
                FDwfAnalogInAcquisitionModeSet(_handle, acqmodeScanShift);
                FDwfAnalogInFrequencySet(_handle, ((double)100.0));
                FDwfAnalogInChannelFilterSet(_handle, -1, filterAverage);
                FDwfAnalogInConfigure(_handle, Convert.ToInt32(false), Convert.ToInt32(true));

                byte sts = 0;
                int cSamples = 0;
                //int cAvailable = 0, cLost = 0, cCorrupted = 0;

                do
                {
                    Thread.Sleep(100);

                    if (backgroundWorkerCV.CancellationPending)
                    {
                        e.Cancel = true;

                        break;
                    }

                    if (FDwfAnalogInStatus(_handle, Convert.ToInt32(true), out sts) == 0)
                    {
                        Console.WriteLine("error");
                        return;
                    }

                    //Console.WriteLine($"sts : {sts}");
                    if (true && (sts == stsCfg || sts == stsPrefill || sts == stsArm))
                    {
                        // Acquisition not yet started.
                        continue;
                    }

                    //FDwfAnalogInStatusIndexWrite(_handle, out int pWrite);
                    FDwfAnalogInStatusSamplesValid(_handle, out int pWrite);
                    //Console.WriteLine($"pWrite: {pWrite}");
                    // FDwfAnalogInBufferSizeSetで10個の指定だが、ここでは、最初の1個だけデータとして採取。いまのところ、平均していない。
                    FDwfAnalogInStatusData2(_handle, (CHANNEL_POTENTIAL - 1), _readingBuffers[(CHANNEL_POTENTIAL - 1)], pWrite, 1);

                    ocp_uncorrected += _readingBuffers[(CHANNEL_POTENTIAL - 1)][0] * -1;
                    Console.WriteLine("OCP = {0}", _readingBuffers[(CHANNEL_POTENTIAL - 1)][0] * -1);

                    if (++cSamples >= 20)
                    {
                        ocp_uncorrected /= cSamples; // ここで平均を取る。
                        Console.WriteLine("OCP = {0} V (ave.)", _readingBuffers[(CHANNEL_POTENTIAL - 1)][0] * -1);

                        break;
                    }
                }
                while (true);

                FDwfAnalogInConfigure(_handle, Convert.ToInt32(false), Convert.ToInt32(false));
                FDwfAnalogInBufferSizeSet(_handle, maxBuffer);

                // TODO: ここでのOCPはRATIO_POTENTIALの影響を受けていない値。なので、POTENTIAL_SLOPE_OSC/(1/RATIO_POTENTIAL)だけ補正する必要がある。
                //       その後、POTENTIAL_OFFSET_AWGとPOTENTIAL_SLOPE_AWGを補正する必要がある。
                ocp = (((ocp_uncorrected * POTENTIAL_SCALE - POTENTIAL_OFFSET_OSC) / 1.0) - POTENTIAL_OFFSET_AWG) / POTENTIAL_SLOPE_AWG;
                _millivoltInitial += ocp; // [mV]
            }

            //if(_selectedMethod != methodMeasurement.OCP) SetCircuit(open:false);

            if (   _selectedMethod == methodMeasurement.Cyclicvoltammetry
                || _selectedMethod == methodMeasurement.CyclicvoltammetryQuick
                || _selectedMethod == methodMeasurement.LSV
                || _selectedMethod == methodMeasurement.Series_of_RDE_CV
                || _selectedMethod == methodMeasurement.Series_of_RDE_LSV
                || _selectedMethod == methodMeasurement.DPSCA
                || _selectedMethod == methodMeasurement.IRC
                || _selectedMethod == methodMeasurement.OSWV
                || _selectedMethod == methodMeasurement.Cyclicgalvanometry
                )
            {
                // 現在は電位入力が反転増幅回路を通っているので、このdirectonもわざと逆にしておく必要がある。--> というのはウソで、非反転でした。
                int direction = (_millivoltVertex > _millivoltInitial) ? 1 : -1;

                //typeWaveForm waveform = typeWaveForm.none;
                double millivoltHeight = Math.Abs(_millivoltVertex - _millivoltInitial);
                double secRecording = 0.0;

                //// AWGに渡す波形を表す配列の長さの上限を確認 -> AD2.0は4096だった。
                //int min = 0, max = 0;
                //FDwfAnalogOutNodeDataInfo(_handle, 0, AnalogOutNodeCarrier, ref min, ref max);

                //double dmin = 0.0, dmax = 0.0;
                //FDwfAnalogInFrequencyInfo(_handle, ref dmin, ref dmax);
                //Console.WriteLine(dmin.ToString() + ", dmax: " + dmax.ToString());

                // AWG用の波形をその場作成
                double[] rgdSamples = new double[4096];
                //int dir = 0;
                switch (_selectedMethod)
                {
                    case methodMeasurement.Cyclicvoltammetry:
                    case methodMeasurement.CyclicvoltammetryQuick:
                    case methodMeasurement.Series_of_RDE_CV:
                    case methodMeasurement.Cyclicgalvanometry:
                        secRecording = (2 * millivoltHeight) / _millivoltScanrate;
                        for (int i = 0; i < 4096; i++)
                        {
                            if (i <= 2048)
                            {
                                rgdSamples[i] = direction * ((double)i / 2048);// 0 -> +1 -> 0まで変化する三角波。単位は[V]と考えてよい。
                            }
                            else
                            {
                                rgdSamples[i] = direction * (1.0 - ((double)(i - 2048) / 2048));
                            }
                        }
                        break;

                    case methodMeasurement.LSV:
                    case methodMeasurement.Series_of_RDE_LSV:
                        secRecording = (millivoltHeight) / _millivoltScanrate;
                        for (int i = 0; i < 4096; i++)
                        {
                            rgdSamples[i] = direction * (1.0 * i / 4095);// 0 -> +1まで変化する波。単位は[V]と考えてよい。
                        }
                        break;

                    case methodMeasurement.DPSCA:
                        secRecording = _secDuration;
                        rgdSamples[0] = direction * (0.0); // ステップ電位のオーバーシュートを避けるために必要
                        for (int i = 1; i < 4096; i++)
                        {
                            if (i <= 2048)
                            {
                                rgdSamples[i] = direction * (1.0);
                            }
                            else
                            {
                                rgdSamples[i] = direction * (0.0);
                            }
                        }
                        break;

                    case methodMeasurement.IRC:
                        millivoltHeight = Math.Abs(_millivoltVertex);
                        secRecording = _secDuration;
                        int dir = 1;
                        rgdSamples[0] = dir * (0.0); // ステップ電位のオーバーシュートを避けるために必要
                        for (int i = 1; i < 4096; i++)
                        {
                            if ((i % 256) == 0)
                            {
                                dir *= -1;
                            }
                            rgdSamples[i] = dir * (1.0);

                            //if (i <= 2048)
                            //{
                            //    rgdSamples[i] = direction * (1.0);
                            //}
                            //else
                            //{
                            //    rgdSamples[i] = direction * (0.0);
                            //}
                        }
                        rgdSamples[4096 - 1] = dir * (0.0);
                        break;

                    case methodMeasurement.OSWV: //ステップ高さ:10mV、パルス高さ:+-25mV、とする
                        secRecording = (millivoltHeight) / _millivoltScanrate;
                        int dir_step = direction;
                        rgdSamples[0] = dir_step * (0.0); // ステップ電位のオーバーシュートを避けるために必要


                        double step = 10.0 / POTENTIAL_SLOPE_AWG; // 電位の変換(SWV用)
                        double pulse = 25.0 / POTENTIAL_SLOPE_AWG; // TODO: SWVでの固定stepをやめる

                        double widthStep = 4096 / (millivoltHeight / step);
                        double halfwidthStep = widthStep / 2;
                        int dir_pulse = direction;
                        int step_count = 1;
                        int step_max = (int)Math.Ceiling((millivoltHeight - Math.Floor(pulse / step) * step) / step); // パルスの高さの分だけ到達できる最終電位が低くなる

                        for (int i = 1; i < 4096; i++)
                        {
                            rgdSamples[i] = dir_step * step_count * (step / millivoltHeight) + dir_pulse * (pulse / millivoltHeight);

                            if (i > (step_count * widthStep))
                            {
                                step_count++;
                                dir_pulse *= -1;

                                if (step_count == step_max)
                                {
                                    for (++i; i < 4096; i++) rgdSamples[i] = dir_step * (0.0);
                                    break;
                                }
                            }
                            if ((i > ((step_count * widthStep) - halfwidthStep)) && (dir_step == dir_pulse))
                            {
                                dir_pulse *= -1;
                            }
                        }
                        rgdSamples[4096 - 1] = dir_step * (0.0);
                        break;
                }

                if (   _selectedMethod == methodMeasurement.Cyclicvoltammetry
                    || _selectedMethod == methodMeasurement.LSV
                    || _selectedMethod == methodMeasurement.Cyclicgalvanometry
                   )
                {
                    toolStripStatusLabelCycle.Text = "(Cycle 1)";

                    if (!DEBUG_VOLTAMMOGRAM)
                    {
                        if (_countRepeat > 1)
                        {
                            string directory = null, fn = null, ext = null;
                            if (_file_path != null)
                            {
                                directory = System.IO.Path.GetDirectoryName(_file_path);
                                fn = System.IO.Path.GetFileNameWithoutExtension(_file_path);
                                ext = System.IO.Path.GetExtension(_file_path);
                            }

                            for (int i = 1; i <= _countRepeat; i++)
                            {
                                if (backgroundWorkerCV.CancellationPending)
                                {
                                    e.Cancel = true;

                                    break;
                                }

                                if (_file_path != null)
                                {
                                    _file_path = directory + "\\" + fn + " " + ((i).ToString()) + ext;
                                }

                                backgroundWorkerCV.ReportProgress((int)statusMeasurement.Initialized);
                                toolStripStatusLabelCycle.Text = "(Cycle " + (i).ToString() + " out of " + _countRepeat.ToString() + ")";

                                if (i == _countRepeat)
                                {
                                    DoVoltammetry(ref rgdSamples, secRecording, millivoltHeight, ref e, fCA: false, fFinal: true);

                                    backgroundWorkerCV.ReportProgress((int)statusMeasurement.MeasuremetWasDone);
                                }
                                else if (i == 1)
                                {

                                    DoVoltammetry(ref rgdSamples, secRecording, millivoltHeight, ref e, fCA: ((!is_initial_potential_referring_to_OCP) && (_selectedMethod != methodMeasurement.Cyclicgalvanometry)), fFinal: false);

                                    backgroundWorkerCV.ReportProgress((int)statusMeasurement.MeasuremetWasDone);
                                }
                                else
                                {
                                    DoVoltammetry(ref rgdSamples, secRecording, millivoltHeight, ref e, fCA: false, fFinal: false);

                                    backgroundWorkerCV.ReportProgress((int)statusMeasurement.MeasuremetWasDone);
                                }
                            }
                        }
                        else
                        {
                            // In the case of a one shot measurement...
                            backgroundWorkerCV.ReportProgress((int)statusMeasurement.Initialized);

                            DoVoltammetry(ref rgdSamples, secRecording, millivoltHeight, ref e, fCA: ((!is_initial_potential_referring_to_OCP) && (_selectedMethod != methodMeasurement.Cyclicgalvanometry)));

                            backgroundWorkerCV.ReportProgress((int)statusMeasurement.MeasuremetWasDone);
                        }
                    }
                    else
                    {
                        backgroundWorkerCV.ReportProgress((int)statusMeasurement.Initialized);

                        DoVoltammetry_virtual();

                        backgroundWorkerCV.ReportProgress((int)statusMeasurement.MeasuremetWasDone);
                    }
                }
                else if (
                       _selectedMethod == methodMeasurement.CyclicvoltammetryQuick
                   )
                {
                    //_herzAcquisition = (1.0 / secRecording) * 5000;

                    backgroundWorkerCV.ReportProgress((int)statusMeasurement.Initialized);
                    toolStripStatusLabelCycle.Text = "(Cycle 1)";

                    //if (!DEBUG_VOLTAMMOGRAM)
                    //{
                    DoVoltammetryQuick(ref rgdSamples, secRecording, millivoltHeight, ref e);
                    //}
                    //else
                    //{
                    //    DoVoltammetry_virtual();
                    //}

                    backgroundWorkerCV.ReportProgress((int)statusMeasurement.MeasuremetWasDone);
                }
                else if (   _selectedMethod == methodMeasurement.DPSCA
                         || _selectedMethod == methodMeasurement.IRC
                         || _selectedMethod == methodMeasurement.OSWV
                        )
                {
                    double _herzAcquisition_prev = _hertzAcquisition; //_herzAcquisition = 400;

                    backgroundWorkerCV.ReportProgress((int)statusMeasurement.Initialized);
                    toolStripStatusLabelCycle.Text = "(Cycle 1)";

                    if (!DEBUG_VOLTAMMOGRAM)
                    {
                        DoVoltammetry(ref rgdSamples, secRecording, millivoltHeight, ref e, fCA: (!is_initial_potential_referring_to_OCP && _selectedMethod == methodMeasurement.OSWV));
                    }
                    else
                    {
                        DoVoltammetry_virtual();
                    }

                    backgroundWorkerCV.ReportProgress((int)statusMeasurement.MeasuremetWasDone);

                    _hertzAcquisition = _herzAcquisition_prev;
                }
                else if (   _selectedMethod == methodMeasurement.Series_of_RDE_CV
                         || _selectedMethod == methodMeasurement.Series_of_RDE_LSV
                        )
                {
                    string directory = null, fn = null, ext = null;
                    if (_file_path != null)
                    {
                        directory = System.IO.Path.GetDirectoryName(_file_path);
                        fn = System.IO.Path.GetFileNameWithoutExtension(_file_path);
                        ext = System.IO.Path.GetExtension(_file_path);
                    }

                    for (int i = 0; i < _rotation_speeds.Length; i++)
                    {
                        if (_rotation_speeds[i] < 100) break;

                        if (backgroundWorkerCV.CancellationPending)
                        {
                            e.Cancel = true;

                            break;
                        }

                        if (DIGITALIO_FROM_ARDUINO)
                        {
                            _arduino.closeCircuit();
                            _arduino.stopPurging();
                        }
                        else
                        {
                            //SetCircuit(open: false);
                            switch (_selectedSubModule)
                            {
                                case typeSubModule.RDE:
                                    SetRotation(Convert.ToUInt32(_rotation_speeds[i]));
                                    SetPurging(on: false);
                                    break;

                                case typeSubModule.Synchronous_switching:
                                    break;

                                default:
                                    break;
                            }
                        }

                        if (_file_path != null)
                        {
                            _file_path = directory + "\\" + fn + " " + ((i + 1).ToString()) + ext;
                        }

                        backgroundWorkerCV.ReportProgress((int)statusMeasurement.Initialized);
                        toolStripStatusLabelCycle.Text = "(Cycle " + (i + 1).ToString() + " out of " + _rotation_speeds.Length.ToString() + ")";

                        DoVoltammetry(ref rgdSamples, secRecording, millivoltHeight, ref e, fCA: !is_initial_potential_referring_to_OCP);

                        backgroundWorkerCV.ReportProgress((int)statusMeasurement.MeasuremetWasDone);

                        if (DIGITALIO_FROM_ARDUINO)
                        {
                            _arduino.openCircuit();
                            _arduino.startPurging();
                        }
                        else
                        {
                            SetCircuit(open: true);
                            switch (_selectedSubModule)
                            {
                                case typeSubModule.RDE:
                                    SetRotation(1600);
                                    SetPurging(on: true);
                                    break;

                                case typeSubModule.Synchronous_switching:
                                    break;

                                default:
                                    break;
                            }
                        }

                        for (int j = 0; j < (10 * 30); j++)
                        {
                            Thread.Sleep(100);

                            if (backgroundWorkerCV.CancellationPending)
                            {
                                e.Cancel = true;

                                break;
                            }
                        }
                    }
                }
                else
                {
                }

                Console.WriteLine("BlockData end");
            }
            else if (   _selectedMethod == methodMeasurement.BulkElectrolysis
                     || _selectedMethod == methodMeasurement.ConstantCurrent
                     || _selectedMethod == methodMeasurement.OCP
                    )
            {
                long nextMeasurementTiming = 0;

                backgroundWorkerCV.ReportProgress((int)statusMeasurement.Initialized);
                toolStripStatusLabelCycle.Text = "(Cycle 1)";

                // AWGの準備
                // enable first channel
                //FDwfAnalogOutNodeEnableSet(_handle, 0, AnalogOutNodeCarrier, Convert.ToInt32(true));

                _clockingStopwatch.Restart();
                backgroundWorkerCV.ReportProgress((int)statusMeasurement.MeasurementGotStarted);


                if (_selectedMethod != methodMeasurement.OCP)
                {
                    SetDCVoltageCH1(_millivoltInitial * 1000.0);
                    SetCircuit(open: false);
                }


                FDwfAnalogInBufferSizeGet(_handle, out int maxBuffer);
                FDwfAnalogInBufferSizeSet(_handle, 16);
                FDwfAnalogInAcquisitionModeSet(_handle, acqmodeScanShift);
                //FDwfAnalogInAcquisitionModeSet(_handle, acqmodeScanScreen);
                FDwfAnalogInFrequencySet(_handle, ((double)_hertzAcquisition)); // 取り込み周波数を設定
                //FDwfAnalogInChannelFilterSet(_handle, -1, filterDecimate);
                FDwfAnalogInChannelFilterSet(_handle, -1, filterAverage);
                //FDwfAnalogInRecordLengthSet(_handle, (secCAing + secRecording + secDelta)); // 取り込みする時間[s]の設定。サンプル数[] / 周波数[s-1]

                FDwfAnalogInConfigure(_handle, Convert.ToInt32(false), Convert.ToInt32(true));


                byte sts = 0;
                int cSamples = 0;
                int cAvailable = 0, cLost = 0, cCorrupted = 0;
                int cSwitching = 1, cPotentialSwitching = 0;


                long t_current = 0, t_next = -1;

                while ((_clockingStopwatch.ElapsedMilliseconds < (_secDuration * 60 * 1000)) && (cSamples < BUFFER_SIZE))
                {
                    if (backgroundWorkerCV.CancellationPending)
                    {
                        e.Cancel = true;

                        break;
                    }

                    t_current = _clockingStopwatch.ElapsedMilliseconds;
                    if (t_current > t_next)
                    {

                        if (FDwfAnalogInStatus(_handle, Convert.ToInt32(true), out sts) == 0)
                        {
                            Console.WriteLine("error");
                            return;
                        }
                        //Console.WriteLine($"sts : {sts}");
                        if (cSamples == 0 && (sts == stsCfg || sts == stsPrefill || sts == stsArm))
                        {
                            // Acquisition not yet started.
                            continue;
                        }

                        //FDwfAnalogInStatusIndexWrite(_handle, out int pWrite);
                        FDwfAnalogInStatusSamplesValid(_handle, out int pWrite);
                        //Console.WriteLine($"pWrite: {pWrite}");

                        //if (i >= 0)
                        if (true)
                        {
                            // FDwfAnalogInBufferSizeSetで16個の指定だが、ここでは、最初の1個だけデータとして採取。いまのところ、平均していない。
                            FDwfAnalogInStatusData2(_handle, (CHANNEL_POTENTIAL - 1), _readingBuffers[(CHANNEL_POTENTIAL - 1)], pWrite, 1);
                            //_recordingSeries[1][cSamples] = (_readingBuffers[0][0] * -1000.0);// * -1)*1000.0;// とりあえず最初の値のみ記録。平均しない。
                            //Array.Copy(_readingBuffers[(CHANNEL_POTENTIAL - 1)], 0, _recordingSeries[CHANNEL_POTENTIAL], cSamples, cAvailable);

                            if (_selectedMethod == methodMeasurement.OCP)
                            {
                                _recordingSeries[CHANNEL_POTENTIAL][cSamples] = _readingBuffers[(CHANNEL_POTENTIAL - 1)][0] * -1;
                            }
                            else
                            {
                                _recordingSeries[CHANNEL_POTENTIAL][cSamples] = _readingBuffers[(CHANNEL_POTENTIAL - 1)][0];// * -1000.0;
                            }

                            //SampleSingleValue((CHANNEL_CURRENT-1), out double valueCh2);
                            FDwfAnalogInStatusData2(_handle, (CHANNEL_CURRENT - 1), _readingBuffers[(CHANNEL_CURRENT - 1)], pWrite, 1);
                            _recordingSeries[CHANNEL_CURRENT][cSamples] = _readingBuffers[(CHANNEL_CURRENT - 1)][0]; ;

                            _recordingSeries[0][cSamples] = t_current;// nextMeasurementTiming;


                            //SampleSingleValue((CHANNEL_POTENTIAL-1), out double valueCh1);
                            //SampleSingleValue((CHANNEL_CURRENT-1), out double valueCh2);
                            //_recordingSeries[0][i] = t2;// nextMeasurementTiming;
                            ////_recordingSeries[1][i] = _readingBuffers[0][0] * -1;
                            ////_recordingSeries[2][i] = _readingBuffers[1][0];
                            //_recordingSeries[CHANNEL_POTENTIAL][i] = valueCh1;// * -1000.0;
                            //_recordingSeries[CHANNEL_CURRENT][i] = valueCh2;// * 1000.0;

                            backgroundWorkerCV.ReportProgress(cSamples);

                            //
                            // 時々、S電極の電位を確認して、_millivoltLimitを超えていたら、_millivoltInitialを調節する
                            //
                            if (_selectedMethod == methodMeasurement.ConstantCurrent)
                            {
                                if (cSamples > 0 && (cSamples % 10) == 0)
                                {
                                    if(_millivoltLimit > 0 && Math.Abs(_recordingSeries[CHANNEL_POTENTIAL][cSamples] * 1000) > _millivoltLimit)
                                    {
                                        // S電極の電位が_millivoltLimitを超えているので、倍率0.9で絶対値を低減させる
                                        _millivoltInitial *= 0.9;
                                        _millivoltInitial_raw *= 0.9;
                                        SetDCVoltageCH1(_millivoltInitial * 1000.0);
                                        Console.WriteLine($"Current for G/S was attenuated. (now: {_millivoltInitial_raw} uA)");
                                    }
                                }
                            }

                            cSamples++;
                        }

                        t_next = t_current + (long)Math.Round(_secInterval * 1000);
                    }

                    if(_is_using_potential_switcher)
                    {
                        // TODO: 

                        if(t_current > (_secPotentialSwitchingInterval * cPotentialSwitching) * 1000)
                        {
                            if((cPotentialSwitching % 2) == 0)
                            {
                                SetDCVoltageCH1(_millivoltInitial * 1000.0);
                            }
                            else
                            {
                                SetDCVoltageCH1(_millivoltSwitchingPotential * 1000.0);
                            }
                            cPotentialSwitching++;
                        }
                    }

                    switch (_selectedSubModule)
                    {
                        case typeSubModule.RDE:
                            break;

                        case typeSubModule.Synchronous_switching:
                            if (_is_using_automatical_switcher && t_current > (_secSwitchingInterval * cSwitching) * 1000)
                            {
                                SetSwitchOnAsync(!_is_switched_on);
                                cSwitching++;
                            }
                            break;

                        default:
                            break;
                    }

                    Thread.Sleep(10);
                    Thread.Yield();
                }

                //for (int i = -1; (i <= (_secDuration * 60 / _secInterval)) && (i < BUFFER_SIZE); i++)
                //{
                //    long t_current; // long t1, t2; 
                //    //t1 = _clockingStopwatch.ElapsedMilliseconds;
                //    do
                //    {
                //        Thread.Sleep(10);

                //        t_current = _clockingStopwatch.ElapsedMilliseconds;
                //    }
                //    while (t_current < nextMeasurementTiming);
                //    //nextMeasurementTiming = t2;

                //    if (FDwfAnalogInStatus(_handle, Convert.ToInt32(true), out sts) == 0)
                //    {
                //        Console.WriteLine("error");
                //        return;
                //    }
                //    //Console.WriteLine($"sts : {sts}");
                //    if (cSamples == 0 && (sts == stsCfg || sts == stsPrefill || sts == stsArm))
                //    {
                //        // Acquisition not yet started.
                //        continue;
                //    }

                //    //FDwfAnalogInStatusIndexWrite(_handle, out int pWrite);
                //    FDwfAnalogInStatusSamplesValid(_handle, out int pWrite);
                //    //Console.WriteLine($"pWrite: {pWrite}");

                //    if (i >= 0)
                //    {
                //        // FDwfAnalogInBufferSizeSetで16個の指定だが、ここでは、最初の1個だけデータとして採取。いまのところ、平均していない。
                //        FDwfAnalogInStatusData2(_handle, (CHANNEL_POTENTIAL - 1), _readingBuffers[(CHANNEL_POTENTIAL - 1)], pWrite, 1);
                //        //_recordingSeries[1][cSamples] = (_readingBuffers[0][0] * -1000.0);// * -1)*1000.0;// とりあえず最初の値のみ記録。平均しない。
                //        //Array.Copy(_readingBuffers[(CHANNEL_POTENTIAL - 1)], 0, _recordingSeries[CHANNEL_POTENTIAL], cSamples, cAvailable);

                //        if (_selectedMethod == methodMeasurement.OCP)
                //        {
                //            _recordingSeries[CHANNEL_POTENTIAL][i] = _readingBuffers[(CHANNEL_POTENTIAL - 1)][0] * -1;
                //        }
                //        else
                //        {
                //            _recordingSeries[CHANNEL_POTENTIAL][i] = _readingBuffers[(CHANNEL_POTENTIAL - 1)][0];// * -1000.0;
                //        }

                //        //SampleSingleValue((CHANNEL_CURRENT-1), out double valueCh2);
                //        FDwfAnalogInStatusData2(_handle, (CHANNEL_CURRENT - 1), _readingBuffers[(CHANNEL_CURRENT - 1)], pWrite, 1);
                //        _recordingSeries[CHANNEL_CURRENT][i] = _readingBuffers[(CHANNEL_CURRENT - 1)][0]; ;

                //        _recordingSeries[0][i] = t_current;// nextMeasurementTiming;


                //        //SampleSingleValue((CHANNEL_POTENTIAL-1), out double valueCh1);
                //        //SampleSingleValue((CHANNEL_CURRENT-1), out double valueCh2);
                //        //_recordingSeries[0][i] = t2;// nextMeasurementTiming;
                //        ////_recordingSeries[1][i] = _readingBuffers[0][0] * -1;
                //        ////_recordingSeries[2][i] = _readingBuffers[1][0];
                //        //_recordingSeries[CHANNEL_POTENTIAL][i] = valueCh1;// * -1000.0;
                //        //_recordingSeries[CHANNEL_CURRENT][i] = valueCh2;// * 1000.0;

                //        backgroundWorkerCV.ReportProgress(i);

                //        //
                //        // 時々、S電極の電位を確認して、_millivoltLimitを超えていたら、_millivoltInitialを調節する
                //        //
                //        if (_selectedMethod == methodMeasurement.ConstantCurrent)
                //        {
                //            if (i > 0 && (i % 10) == 0)
                //            {
                //                if(_millivoltLimit > 0 && Math.Abs(_recordingSeries[CHANNEL_POTENTIAL][i] * 1000) > _millivoltLimit)
                //                {
                //                    // S電極の電位が_millivoltLimitを超えているので、倍率0.9で絶対値を低減させる
                //                    _millivoltInitial *= 0.9;
                //                    _millivoltInitial_raw *= 0.9;
                //                    SetDCVoltageCH1(_millivoltInitial * 1000.0);
                //                    Console.WriteLine($"Current for G/S was attenuated. (now: {_millivoltInitial_raw} uA)");
                //                }
                //            }
                //        }
                //    }

                //    if (backgroundWorkerCV.CancellationPending)
                //    {
                //        e.Cancel = true;

                //        break;
                //    }

                //    switch (_selectedSubModule)
                //    {
                //        case typeSubModule.RDE:
                //            break;

                //        case typeSubModule.Synchronous_switching:
                //            if (_is_using_automatical_switcher && t_current > (_secSwitchingInterval * cSwitching) * 1000)
                //            {
                //                SetSwitchOnAsync(!_is_switched_on);
                //                cSwitching++;
                //            }
                //            break;

                //        default:
                //            break;
                //    }

                //    nextMeasurementTiming += (long)Math.Round(_secInterval * 1000); // [ms]
                //    //if (nextMeasurementTiming > (_millivoltVertex * 60 * 1000) ) break;
                //    //Console.WriteLine("elapsed time: {0} [s]", nextMeasurementTiming/1000);
                //}

                switch (_selectedSubModule)
                {
                    case typeSubModule.RDE:
                        break;

                    case typeSubModule.Synchronous_switching:
                        //
                        // TODO: Finally On?
                        // 
                        if(_is_using_automatical_switcher) SetSwitchOnAsync(_is_applying_switching_on_finally);
                        break;

                    default:
                        break;
                }

                FDwfAnalogInConfigure(_handle, Convert.ToInt32(false), Convert.ToInt32(false));
                FDwfAnalogInBufferSizeSet(_handle, maxBuffer);

                if (_selectedMethod != methodMeasurement.OCP)
                {
                    FDwfAnalogOutConfigure(_handle, 0, Convert.ToInt32(false));
                    FDwfAnalogOutReset(_handle, 0);
                }

                //FDwfDeviceAutoConfigureSet(_handle, Convert.ToInt32(false));

                backgroundWorkerCV.ReportProgress((int)statusMeasurement.MeasuremetWasDone);
            }
            else if (   _selectedMethod == methodMeasurement.EIS
                     || _selectedMethod == methodMeasurement.EIS_Open_Circuit
                     || _selectedMethod == methodMeasurement.EIS_Short_Circuit
                    )
            {
                backgroundWorkerCV.ReportProgress((int)statusMeasurement.Initialized);
                toolStripStatusLabelCycle.Text = "(Cycle 1)";
                _clockingStopwatch.Restart();
                backgroundWorkerCV.ReportProgress((int)statusMeasurement.MeasurementGotStarted);


                FDwfAnalogImpedanceReset(_handle);
                FDwfAnalogImpedanceModeSet(_handle, 0);
                FDwfAnalogImpedanceReferenceSet(_handle, 1000000.0 / (double)_selectedCurrentFactor);
                //FDwfAnalogImpedanceReferenceSet(_handle, 1000.0);
                //FDwfAnalogImpedanceFrequencySet(_handle, 1.0);
                FDwfAnalogImpedanceAmplitudeSet(_handle, (double)_millivoltAmplitude / 1000); // POTENTIAL_RATIOの影響を受ける
                FDwfAnalogImpedanceOffsetSet(_handle, (double)_millivoltInitial / 1000); // POTENTIAL_RATIOの影響を受ける
                FDwfAnalogImpedanceCompReset(_handle);
                FDwfAnalogImpedancePeriodSet(_handle, (int)_countRepeat); // もともとは１６
                //FDwfAnalogImpedanceConfigure(_handle, 1);

                SetCircuit(open: false);

                int direction = (_hertzFinal > _hertzInitial) ? 1 : -1;

                for (int i = 0; i <= (Math.Floor(direction * Math.Log10(_hertzFinal/_hertzInitial) * _countStep) + 1); i++) // 50 = 100kHzまで, 53 = 200kHzまで
                {
                    double freq = Math.Pow(10, (Math.Log10(_hertzInitial) + direction * (double)i / _countStep));

                    //Console.WriteLine("Freq [Hz]:" + freq);
                    FDwfAnalogImpedanceFrequencySet(_handle, freq);

                    switch (_selectedMethod)
                    {
                        case methodMeasurement.EIS:
                            if (!Double.IsNaN(_recordingSeries[CHANNEL_VIRTUAL_OPEN_RS][0]) && !Double.IsNaN(_recordingSeries[CHANNEL_VIRTUAL_SHORT_RS][0]))
                            {
                                FDwfAnalogImpedanceCompSet(
                                    _handle,
                                    _recordingSeries[CHANNEL_VIRTUAL_OPEN_RS][i],
                                    _recordingSeries[CHANNEL_VIRTUAL_OPEN_XS][i],
                                    _recordingSeries[CHANNEL_VIRTUAL_SHORT_RS][i],
                                    _recordingSeries[CHANNEL_VIRTUAL_SHORT_XS][i]
                                    );
                            }
                            break;
                    }

                    FDwfAnalogImpedanceConfigure(_handle, 1);
                    Thread.Sleep(10);
                    Thread.Yield();
                    byte sts;
                    //FDwfAnalogImpedanceStatus(_handle, out sts);
                    //FDwfAnalogImpedanceStatus(_handle, out sts);

                    while (true)
                    {
                        if (backgroundWorkerCV.CancellationPending)
                        {
                            e.Cancel = true;

                            break;
                        }

                        if (FDwfAnalogImpedanceStatus(_handle, out sts) == 0)
                        {
                            Console.WriteLine("error");
                            return;
                        }
                        //if ((sts == stsCfg || sts == stsPrefill || sts == stsArm))
                        //{
                        //    // Acquisition not yet started.
                        //    continue;
                        //}
                        if (sts == stsDone)
                        {
                            // Acquisition not yet started.
                            break;
                        }
                    }

                    FDwfAnalogImpedanceStatusMeasure(_handle, DwfAnalogImpedanceResistance, out double resi);
                    FDwfAnalogImpedanceStatusMeasure(_handle, DwfAnalogImpedanceReactance, out double reac);
                    FDwfAnalogImpedanceStatusMeasure(_handle, DwfAnalogImpedanceImpedance, out double impe);
                    FDwfAnalogImpedanceStatusMeasure(_handle, DwfAnalogImpedanceImpedancePhase, out double phase);

                    _recordingSeries[CHANNEL_VIRTUAL_REAL_Z][i] = -1 * resi; // due to cirtutry of PP0.9e
                    _recordingSeries[CHANNEL_VIRTUAL_IM_Z][i] = reac;
                    _recordingSeries[CHANNEL_VIRTUAL_PHASE][i] = phase;
                    _recordingSeries[CHANNEL_POTENTIAL][i] = _millivoltInitial; // 戻り値はPOTENTIAL_RATIOの影響を受けている


                    switch (_selectedMethod)
                    {
                        case methodMeasurement.EIS:
                            if (Double.IsNaN(_recordingSeries[CHANNEL_VIRTUAL_SHORT_RS][0]))
                            {
                                _recordingSeries[CHANNEL_VIRTUAL_REAL_Z][i] -= (1000000.0 / (double)_selectedCurrentFactor);
                            }
                            break;

                        case methodMeasurement.EIS_Open_Circuit:
                            _recordingSeries[CHANNEL_VIRTUAL_OPEN_RS][i] = resi;
                            _recordingSeries[CHANNEL_VIRTUAL_OPEN_XS][i] = reac;
                            break;

                        case methodMeasurement.EIS_Short_Circuit:
                            _recordingSeries[CHANNEL_VIRTUAL_SHORT_RS][i] = resi;
                            _recordingSeries[CHANNEL_VIRTUAL_SHORT_XS][i] = reac;
                            break;
                    }

                    FDwfAnalogImpedanceStatusInput(_handle, 0, out double gain0, out double radian0);
                    FDwfAnalogImpedanceStatusInput(_handle, 1, out double gain1, out double radian1);

                    _recordingSeries[CHANNEL_VIRTUAL_FREQ][i] = freq;
                    _recordingSeries[CHANNEL_VIRTUAL_ATTN][i] = gain0;

                    Console.WriteLine("Freq {0}, {1} (after corr.), {2}, {3}", freq, _recordingSeries[CHANNEL_VIRTUAL_REAL_Z][i], reac, impe);

                    backgroundWorkerCV.ReportProgress(i);

                    if (backgroundWorkerCV.CancellationPending)
                    {
                        e.Cancel = true;

                        break;
                    }
                }

                FDwfAnalogImpedanceConfigure(_handle, 0);

                switch (_selectedMethod)
                {
                    case methodMeasurement.EIS:
                        break;

                    case methodMeasurement.EIS_Open_Circuit:
                        if (backgroundWorkerCV.CancellationPending)
                        {
                            _recordingSeries[CHANNEL_VIRTUAL_OPEN_RS][0] = Double.NaN;
                        }
                        else
                        {
                            Invoke((Action)delegate ()
                            {
                                toolStripMenuOpenComp.Checked = true;
                            });
                        }
                        break;

                    case methodMeasurement.EIS_Short_Circuit:
                        if (backgroundWorkerCV.CancellationPending)
                        {
                            _recordingSeries[CHANNEL_VIRTUAL_SHORT_RS][0] = Double.NaN;
                        }
                        else
                        {
                            Invoke((Action)delegate ()
                            {
                                toolStripMenuShortComp.Checked = true;
                            });

                        }
                        break;
                }

                backgroundWorkerCV.ReportProgress((int)statusMeasurement.MeasuremetWasDone);
            }
            else if (  _selectedMethod == methodMeasurement.EIS_MottSchottkey
            )
            {
                backgroundWorkerCV.ReportProgress((int)statusMeasurement.Initialized);
                toolStripStatusLabelCycle.Text = "(Cycle 1)";
                _clockingStopwatch.Restart();
                backgroundWorkerCV.ReportProgress((int)statusMeasurement.MeasurementGotStarted);


                FDwfAnalogImpedanceReset(_handle);
                FDwfAnalogImpedanceModeSet(_handle, 0);
                FDwfAnalogImpedanceReferenceSet(_handle, 1000000.0 / (double)_selectedCurrentFactor);
                //FDwfAnalogImpedanceReferenceSet(_handle, 1000.0);
                //FDwfAnalogImpedanceFrequencySet(_handle, 1.0);
                FDwfAnalogImpedanceAmplitudeSet(_handle, (double)_millivoltAmplitude / 1000); // POTENTIAL_RATIOの影響を受ける
                FDwfAnalogImpedanceOffsetSet(_handle, (double)_millivoltInitial / 1000); // POTENTIAL_RATIOの影響を受ける
                FDwfAnalogImpedanceCompReset(_handle);
                FDwfAnalogImpedancePeriodSet(_handle, (int)_countRepeat); // 1024
                //FDwfAnalogImpedanceConfigure(_handle, 1);

                SetCircuit(open: false);

                //
                // 電位を変化させてインピーダンスを測定する(周波数は固定) 
                //

                int direction = (_millivoltVertex > (_millivoltInitial - ocp)) ? 1 : -1;
                double millivoltHeight = Math.Abs(_millivoltVertex - (_millivoltInitial - ocp));

                for (int i = 0; i <= (Math.Floor(millivoltHeight / _millivoltStep) + 1); i++) 
                {
                    double potential = _millivoltInitial + direction * (_millivoltStep * i);

                    FDwfAnalogImpedanceOffsetSet(_handle, (double)potential / 1000); // POTENTIAL_RATIOの影響を受ける
                    FDwfAnalogImpedanceFrequencySet(_handle, _hertzInitial);// 周波数は固定

                    switch (_selectedMethod)
                    {
                        case methodMeasurement.EIS:
                            break;
                    }

                    FDwfAnalogImpedanceConfigure(_handle, 1);
                    Thread.Sleep(100);
                    Thread.Yield();
                    byte sts;
                    //FDwfAnalogImpedanceStatus(_handle, out sts);
                    //FDwfAnalogImpedanceStatus(_handle, out sts);

                    while (true)
                    {
                        if (backgroundWorkerCV.CancellationPending)
                        {
                            e.Cancel = true;

                            break;
                        }

                        if (FDwfAnalogImpedanceStatus(_handle, out sts) == 0)
                        {
                            Console.WriteLine("error");
                            return;
                        }
                        //if ((sts == stsCfg || sts == stsPrefill || sts == stsArm))
                        //{
                        //    // Acquisition not yet started.
                        //    continue;
                        //}
                        if (sts == stsDone)
                        {
                            // Acquisition not yet started.
                            break;
                        }
                    }

                    FDwfAnalogImpedanceStatusMeasure(_handle, DwfAnalogImpedanceResistance, out double resi);
                    FDwfAnalogImpedanceStatusMeasure(_handle, DwfAnalogImpedanceReactance, out double reac);
                    FDwfAnalogImpedanceStatusMeasure(_handle, DwfAnalogImpedanceImpedance, out double impe);
                    FDwfAnalogImpedanceStatusMeasure(_handle, DwfAnalogImpedanceImpedancePhase, out double phase);

                    _recordingSeries[CHANNEL_VIRTUAL_REAL_Z][i] = -1 * resi; // due to cirtutry of PP0.9e
                    _recordingSeries[CHANNEL_VIRTUAL_IM_Z][i] = reac;
                    _recordingSeries[CHANNEL_VIRTUAL_PHASE][i] = phase;
                    _recordingSeries[CHANNEL_POTENTIAL][i] = potential; // 戻り値はPOTENTIAL_RATIOの影響を受けている

                    switch (_selectedMethod)
                    {
                        case methodMeasurement.EIS_MottSchottkey:
                            if (Double.IsNaN(_recordingSeries[CHANNEL_VIRTUAL_SHORT_RS][0]))
                            {
                                _recordingSeries[CHANNEL_VIRTUAL_REAL_Z][i] -= (1000000.0 / (double)_selectedCurrentFactor);
                            }
                            break;
                    }

                    FDwfAnalogImpedanceStatusInput(_handle, 0, out double gain0, out double radian0);
                    FDwfAnalogImpedanceStatusInput(_handle, 1, out double gain1, out double radian1);

                    _recordingSeries[CHANNEL_VIRTUAL_FREQ][i] = _millivoltScanrate;
                    _recordingSeries[CHANNEL_VIRTUAL_ATTN][i] = gain0;

                    Console.WriteLine("Freq {0}, {1}, {2}, {3}, {4}", _millivoltScanrate, resi, reac, impe, potential);

                    backgroundWorkerCV.ReportProgress(i);

                    if (backgroundWorkerCV.CancellationPending)
                    {
                        e.Cancel = true;

                        break;
                    }
                }

                FDwfAnalogImpedanceConfigure(_handle, 0);


                backgroundWorkerCV.ReportProgress((int)statusMeasurement.MeasuremetWasDone);
            }
            else if (
                    _selectedMethod == methodMeasurement.Oscilloscope
                )
            {
                backgroundWorkerCV.ReportProgress((int)statusMeasurement.Initialized);
                toolStripStatusLabelCycle.Text = "(Cycle 1)";

                //
                // AWGの準備
                //     enable first channel
                //

                FDwfAnalogOutNodeEnableSet(_handle, 0, AnalogOutNodeCarrier, Convert.ToInt32(true));
                // AWGに波形等を設定する
                FDwfAnalogOutNodeFunctionSet(_handle, 0, AnalogOutNodeCarrier, funcSine);
                FDwfAnalogOutNodeFrequencySet(_handle, 0, AnalogOutNodeCarrier, _hertzInitial); // 出力波形の周波数を設定
                FDwfAnalogOutNodeAmplitudeSet(_handle, 0, AnalogOutNodeCarrier, _millivoltAmplitude / 1000.0); // 波形の出力係数。1.0だとそのままの電圧で出力される。
                FDwfAnalogOutNodeOffsetSet(_handle, 0, AnalogOutNodeCarrier, _millivoltInitial / 1000.0); // 出力波形のオフセット。但し、ここではOffsetを変更しない(変更すると電圧スパイクが出てしまう)。
                FDwfAnalogOutRunSet(_handle, 0, 0);
                FDwfAnalogOutRepeatSet(_handle, 0, 0);
                FDwfAnalogOutConfigure(_handle, 0, Convert.ToInt32(true));

                SetCircuit(open: false);

                byte sts;
                while (true)
                {
                    Thread.Sleep(100); Thread.Yield();
                    FDwfAnalogOutStatus(_handle, Convert.ToInt32(false), out sts);
                    Console.WriteLine($"Wait for stsArm, (Out): {sts}");
                    if (sts == stsRdy || sts == stsArm || sts == stsTrig) break;
                }
                Thread.Sleep(1000); Thread.Yield(); // この間に凡その充電電流が取り除かれる

                //
                // オシロ側を設定する
                //

                FDwfAnalogInAcquisitionModeSet(_handle, acqmodeSingle1);
                FDwfAnalogInFrequencySet(_handle, _hertzAcquisition); Console.WriteLine($"FDwfAnalogInFrequencySet: {_hertzAcquisition}");
                FDwfAnalogInChannelFilterSet(_handle, -1, filterAverage);
                FDwfAnalogInTriggerAutoTimeoutSet(_handle, 1.0 / _hertzInitial * 10);
                FDwfAnalogInTriggerSourceSet(_handle, trigsrcDetectorAnalogIn);
                FDwfAnalogInTriggerTypeSet(_handle, trigtypeEdge);
                //// first channel
                FDwfAnalogInTriggerChannelSet(_handle, 0);
                FDwfAnalogInTriggerLevelSet(_handle, (_millivoltInitial + 0 * 0.5) / 1000.0); // (_millivoltInitial + _millivoltAmplitude * 0.5) / 1000.0
                FDwfAnalogInTriggerConditionSet(_handle, trigcondRisingPositive);
                FDwfAnalogInTriggerFilterSet(_handle, filterAverage);//filterDecimate

                FDwfAnalogInConfigure(_handle, Convert.ToInt32(false), Convert.ToInt32(true));

                //byte sts;
                //while (true)
                //{
                //    Thread.Sleep(500); Thread.Yield();
                //    FDwfAnalogInStatus(_handle, Convert.ToInt32(false), out sts);
                //    Console.WriteLine($"Wait for stsArm, (In): {sts}");
                //    if (sts == stsRdy || sts == stsArm || sts == stsDone) break;
                //}
                //Thread.Sleep(1000); Thread.Yield();

                _clockingStopwatch.Restart();
                backgroundWorkerCV.ReportProgress((int)statusMeasurement.MeasurementGotStarted);

                Array.Clear(_recordingSeries[0], 0, 8192);
                Array.Clear(_recordingSeries[1], 0, 8192);
                Array.Clear(_recordingSeries[2], 0, 8192);

                int cSamples = 0;
                int cAvailable = 0, cLost = 0, cCorrupted = 0, cnt = 0, wait = 2 * (int)(8192 / (_hertzInitial * 1000) * 1000 / 5); wait = (wait > 10) ? wait : 10;
                while (true)
                {
                    if (backgroundWorkerCV.CancellationPending)
                    {
                        e.Cancel = true;

                        break;
                    }

                    Console.WriteLine($"Waiting for acquisition to get started... ({wait} ms)");
                    Thread.Sleep(wait);

                    if(_clockingStopwatch.ElapsedMilliseconds < (wait * 5)) {   continue;  }

                    if (FDwfAnalogInStatus(_handle, Convert.ToInt32(true), out sts) == 0)
                    {
                        Console.WriteLine("error");
                        return;
                    }
                    if ((sts == stsCfg || sts == stsPrefill || sts == stsArm))
                    {
                        // Acquisition not yet started.
                        Console.WriteLine($"Waiting for trigger... (In): {sts}");
                        continue;
                    }
                    if ((sts != stsDone))
                    {
                        // Acquisition not yet started.
                        Console.WriteLine("Waiting for acquisition to be over...");
                        continue;
                    }

                    FDwfAnalogInStatusRecord(_handle, out cAvailable, out cLost, out cCorrupted);

                    //cSamples += cLost;
                    //cSamples += cCorrupted;

                    if (cCorrupted > 0)
                    {
                        Console.WriteLine("Data corrupted!");
                    }

                    if (cLost > 0)
                    {
                        Console.WriteLine("Data lost!");
                    }

                    if (cAvailable > 0)
                    {
                        FDwfAnalogInStatusData(_handle, (CHANNEL_POTENTIAL - 1), _readingBuffers[(CHANNEL_POTENTIAL - 1)], cAvailable);
                        FDwfAnalogInStatusData(_handle, (CHANNEL_CURRENT - 1), _readingBuffers[(CHANNEL_CURRENT - 1)], cAvailable);
                        //Array.Copy(_readingBuffers[(CHANNEL_POTENTIAL - 1)], 0, _recordingSeries[CHANNEL_POTENTIAL], cSamples, cAvailable);
                        //Array.Copy(_readingBuffers[(CHANNEL_CURRENT - 1)], 0, _recordingSeries[CHANNEL_CURRENT], cSamples, cAvailable);
                        for (int i = 0; i < cAvailable; i++)
                        {
                            _recordingSeries[0][i] = (double)(i) / (_hertzAcquisition) * 1000;

                            _recordingSeries[CHANNEL_POTENTIAL][i] += _readingBuffers[(CHANNEL_POTENTIAL - 1)][i];
                            _recordingSeries[CHANNEL_CURRENT][i] += _readingBuffers[(CHANNEL_CURRENT - 1)][i];
                        }

                        cSamples = cAvailable;
                        if (cSamples > 8192) cSamples = 8192;

                        Console.WriteLine($"cSamples: {cSamples}");

                        cnt++;
                        toolStripStatusLabelCycle.Text = "(Cycle " + (cnt).ToString() + " out of " + _countRepeat.ToString() + ")";
                        if (cnt == _countRepeat)
                        {
                            //backgroundWorkerCV.ReportProgress(cSamples);

                            break;
                        }
                        else
                        {
                            backgroundWorkerCV.ReportProgress(cSamples);
                        }

                        FDwfAnalogInConfigure(_handle, Convert.ToInt32(true), Convert.ToInt32(true));
                        _clockingStopwatch.Restart();
                    }
                }

                if(cnt >= 1)
                {
                    for (int i = 0; i < cSamples; i++)
                    {
                        _recordingSeries[CHANNEL_POTENTIAL][i] /= cnt;
                        _recordingSeries[CHANNEL_CURRENT][i] /= cnt;
                    }
                }
                backgroundWorkerCV.ReportProgress(cSamples);


                Console.WriteLine("Finalizing...");
                FDwfAnalogOutConfigure(_handle, 0, Convert.ToInt32(false));
                FDwfAnalogOutReset(_handle, 0);
                //FDwfDeviceAutoConfigureSet(_handle, Convert.ToInt32(false));
                FDwfAnalogInConfigure(_handle, Convert.ToInt32(false), Convert.ToInt32(false));

                FDwfAnalogInReset(_handle);
                //SelectPotentialRange(_selectedPotentialRange);

                backgroundWorkerCV.ReportProgress((int)statusMeasurement.MeasuremetWasDone);
            }
            else
            {

            }

            SetCircuit(open:true);
            switch (_selectedSubModule)
            {
                case typeSubModule.RDE:
                    SetRotation(0);            
                    SetPurging(on: true);
                    break;

                case typeSubModule.Synchronous_switching:
                    break;

                default:
                    break;
            }
        }

        private void backgroundWorkerCV_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int progress = e.ProgressPercentage;
            //int step5mV = (int)Math.Round(6.2 / _millivoltScanrate * (1000.0 / _intervalMeasurement));
            //step5mV = (step5mV != 0) ? step5mV : 1;
            //int middle = (int)Math.Round((double)step5mV / 2.0);

            //if(_selectedMethod == methodMeasurement.Series_of_RDE)
            //{
            //}

            switch (progress)
            {
                case (int)statusMeasurement.Initialized: // initialized
                    toolStripStatusLabelStatus.Text = "Initialized";

                    _itrRecording = 0;
                    _is_warned_about_potential_limit = false;
                    _coulombPassingThroughCell = 0.0;

                    chartVoltammogram.Series[1].Points.Clear();
                    chartVoltammogram.Series[2].Points.Clear();
                    chartVoltammogram.Series[3].Points.Clear();
                    chartVoltammogram.Series[4].Points.Clear();
                    chartVoltammogram.Series[5].Points.Clear();

                    chartVoltammogram.Series[6].Points.SuspendUpdates();
                        chartVoltammogram.Series[6].Points.Clear();
                        chartVoltammogram.Series[6].Points.AddXY(1,1);
                    chartVoltammogram.Series[6].Points.ResumeUpdates();

                    chartVoltammogram.Update();

                    if (_file_path != null)
                    {
                        _series = _voltammogram.AddNewSeries(true, System.IO.Path.GetFileName(_file_path), System.IO.Path.GetFileName(_file_path), true);
                    }
                    else
                    {
                        _series = _voltammogram.AddNewSeries(true, "Scan", "Scan", true);
                    }

                    _exp_conditions = new XMLDataHolder("experimantal-conditions");
                    //_exp_conditions.SetDatum("version", VERSION_string)
                    _exp_conditions.SetDatum("version", this.Text);
                    if (   _selectedMethod == methodMeasurement.BulkElectrolysis 
                        || _selectedMethod == methodMeasurement.ConstantCurrent
                        )
                    {
                        _exp_conditions.SetDatum("potential", (_millivoltInitial_raw).ToString());  // ファイルに記録する情報は入力値そのものであるべき
                        _exp_conditions.SetDatum("duration", _secDuration.ToString());
                        _exp_conditions.SetDatum("sampling-interval", _secInterval.ToString());
                    }
                    else if (_selectedMethod == methodMeasurement.DPSCA)
                    {
                        _exp_conditions.SetDatum("initial-potential", (_millivoltInitial_raw).ToString());
                        _exp_conditions.SetDatum("stepin-potential", (_millivoltVertex_raw).ToString());
                        _exp_conditions.SetDatum("duration", _secDuration.ToString());
                    }
                    else
                    {
                    //if (   _selectedMethod == methodMeasurement.Cyclicvoltammetry
                    //    || _selectedMethod == methodMeasurement.LSV
                    //    || _selectedMethod == methodMeasurement.Series_of_RDE_CV
                    //    || _selectedMethod == methodMeasurement.Series_of_RDE_LSV
                    //    || _selectedMethod == methodMeasurement.OSWV
                    //    || _selectedMethod == methodMeasurement.DPSCA
                    //    )
                        _exp_conditions.SetDatum("initial-potential", (_millivoltInitial_raw ).ToString());
                        _exp_conditions.SetDatum("vertex", (_millivoltVertex_raw ).ToString());
                        _exp_conditions.SetDatum("scanrate", (_millivoltScanrate_raw ).ToString());
                        _exp_conditions.SetDatum("rpm", _rpmRDE.ToString());
                        _exp_conditions.SetDatum("freq", _hertzAcquisition.ToString());
                    }
                    _voltammogram.AddExpConditions(_series, _exp_conditions);

                    //_writer_temp = new StreamWriter(Application.StartupPath + @"\_temp.mpt", false);
                    ////_writer_temp.WriteLine("time/s, i/uA");
                    //_writer_temp.WriteLine("EC-Lab ASCII FILE");
                    //_writer_temp.WriteLine("Nb header lines : 4");
                    //_writer_temp.WriteLine(System.DateTime.Now.ToString());
                    //_writer_temp.WriteLine("Ewe/V	I/mA	time/s");

                    if (_file_path == null || DEBUG_VOLTAMMOGRAM)
                    {

                    }
                    else if (
                                _selectedMethod == methodMeasurement.Cyclicvoltammetry
                             || _selectedMethod == methodMeasurement.CyclicvoltammetryQuick
                             || _selectedMethod == methodMeasurement.LSV
                             || _selectedMethod == methodMeasurement.Series_of_RDE_CV
                             || _selectedMethod == methodMeasurement.Series_of_RDE_LSV
                             || _selectedMethod == methodMeasurement.Cyclicgalvanometry
                            )
                    {
                        _writer = new StreamWriter(_file_path, false);
                        _writer.WriteLine("EC-Lab ASCII FILE");
                        _writer.WriteLine("Nb header lines : 4");
                        _writer.WriteLine(_exp_conditions.GetData().OuterXml);
                        _writer.WriteLine("Ewe/V	I/mA	time/s");
                    }
                    else if (_selectedMethod == methodMeasurement.OSWV)
                    {
                    }
                    else if (   _selectedMethod == methodMeasurement.EIS
                             || _selectedMethod == methodMeasurement.EIS_Open_Circuit
                             || _selectedMethod == methodMeasurement.EIS_Short_Circuit
                             || _selectedMethod == methodMeasurement.EIS_MottSchottkey
                            )
                    {
                        _writer = new StreamWriter(_file_path, false);
                        _writer.WriteLine("EC-Lab ASCII FILE");
                        _writer.WriteLine("Nb header lines : 4");
                        _writer.WriteLine(_exp_conditions.GetData().OuterXml);
                        _writer.WriteLine("-Im(Z)/Ohm	Re(Z)/Ohm	freq/Hz	Ewe/V");
                    }
                    else //if (_selectedMethod == methodMeasurement.BulkElectrolysis)
                    {
                        _writer = new StreamWriter(_file_path, false);
                        _writer.WriteLine("EC-Lab ASCII FILE");
                        _writer.WriteLine("Nb header lines : 4");
                        _writer.WriteLine(_exp_conditions.GetData().OuterXml);
                        _writer.WriteLine("Ewe/V	I/mA	time/s");
                    }

                    //_herzAcquisition_prev = _herzAcquisition;
                    //if (_selectedMethod == methodMeasurement.DPSCA
                    //    || _selectedMethod == methodMeasurement.IRC
                    //    || _selectedMethod == methodMeasurement.OSWV
                    //    )
                    //{
                    //    _herzAcquisition = 400;
                    //}
                    _digital_filter_notch.Notch(_hertzTargetFilteringFrequency, _hertzAcquisition, 3.0);
                    _digital_filter_lowpass.LowPass(_hertzTargetFilteringFrequency * 1, _hertzAcquisition);
                    break;

                case (int)statusMeasurement.CAGotStarted: // CA got started
                    toolStripStatusLabelStatus.Text = "CA got started";
                    break;

                case (int)statusMeasurement.MeasurementGotStarted: // Actual measurement got started
                    toolStripStatusLabelStatus.Text = "During measurement";
                    break;

                case (int)statusMeasurement.MeasuremetWasDone: // Measurement was done
                    if (_selectedMethod == methodMeasurement.IRC)  
                    {
                        // 全体の時間はsecCAing (2.0s) + _millivoltScanrate (標準では0.1s)かかる
                        // _millivoltScanrateの間に(4096/256)回だけ電位を+-_millivoltVertex [mV]だけ変化させる
                        // 最初の2区間はデータとしては使用しない
                        // 3区間目のデータを解析に使用する
                        // 3区間目の範囲の時間は、[2.0s + (0.1s / (4096/256)) * 2] から [2.0s + (0.1s / (4096/256)) * 3] である。
                        // 時間の範囲を、chartVoltammogram.Series[1].Points[i].XValueでのiの範囲に変換する

                        int trigger = 0, begin = 0, end = 0;

                        double cap = 0.0, res = 0.0; int cnt = 0;

                        double millivolt_actual_initial_voltage = Double.NaN;// -5.6
                        double voltage_scanning = Double.NaN;
                        const int k = 16;

                        for (int i = 0; i < _itrRecording; i++)
                        {
                            if (!Double.IsNaN(voltage_scanning) && Double.IsNaN(millivolt_actual_initial_voltage))
                            {
                                // 初期電圧の取り方: 移動平均からのずれが5mV以上だと、測定が始まったと解釈する
                                if (Math.Abs(((_recordingSeries[CHANNEL_POTENTIAL][i] * POTENTIAL_SCALE - POTENTIAL_OFFSET_OSC) / POTENTIAL_SLOPE_OSC) - voltage_scanning) > 5.0)
                                {
                                    millivolt_actual_initial_voltage = voltage_scanning;
                                }
                            }

                            if (i >= (k - 1))
                            {
                                double v = 0.0;
                                for (int j = 0; j < k; j++)
                                {
                                    v += (_recordingSeries[CHANNEL_POTENTIAL][i - j] * POTENTIAL_SCALE - POTENTIAL_OFFSET_OSC) / POTENTIAL_SLOPE_OSC;
                                }
                                v /= k;
                                voltage_scanning = v;
                            }

                            if (!Double.IsNaN(millivolt_actual_initial_voltage))
                            {
                                if (trigger == 0 && (Math.Abs(((_recordingSeries[CHANNEL_POTENTIAL][i] * POTENTIAL_SCALE - POTENTIAL_OFFSET_OSC) / POTENTIAL_SLOPE_OSC) - (double)(millivolt_actual_initial_voltage + (_millivoltVertex / POTENTIAL_SLOPE_OSC))) < 5.0))
                                {
                                    // 電位が+25mVになったときにtriggerを設定する
                                    trigger = i + (int)Math.Round(0.005 * _hertzAcquisition); // デッドタイムは5msほどか？
                                }

                                if (trigger != 0)
                                {
                                    //if( begin == 0 && ((_recordingSeries[0][i] / 1000) >= ((_recordingSeries[0][trigger] / 1000) + (_millivoltScanrate / (4096 / 256)) * 1)) )
                                    //{
                                    //    begin = i + 1;
                                    //}
                                    if (end == 0 && ((_recordingSeries[0][i] / 1000) >= ((_recordingSeries[0][trigger] / 1000) + (_secDuration / (4096 / 256)) * 1))) 
                                    {
                                        end = i - 2 - (int)Math.Round(0.005 * _hertzAcquisition) * 4;

                                        Console.WriteLine("Analysis of IRC curve (trigger, {0} - {1})", trigger, end);
                                        // 計算に使用したデータ点をプロットしておく
                                        chartVoltammogram.Series[5].Points.AddXY(_recordingSeries[0][trigger] / 1000, (_recordingSeries[CHANNEL_CURRENT][trigger] - CURRENT_OFFSET) * ((double)_selectedCurrentFactor));
                                        chartVoltammogram.Series[5].Points.AddXY(_recordingSeries[0][end] / 1000, (_recordingSeries[CHANNEL_CURRENT][end] - CURRENT_OFFSET) * ((double)_selectedCurrentFactor));

                                        // beginとendの範囲でtの関数でfittingされる。begin0は、本当のt0の推定値。デッドタイムの倍数にしてあるが、何倍がよいか？もう少し検討が必要
                                        if (_nlr.perform(ref _recordingSeries[0], ref _recordingSeries[CHANNEL_CURRENT], (double)_selectedCurrentFactor, (double)_millivoltVertex, trigger, end, _recordingSeries[0][trigger - (int)Math.Round(0.005 * 1 * _hertzAcquisition)], out double c, out double r))
                                        {
                                            if (cnt > 0) { cap += c; res += r; }

                                            cnt++;

                                            if (cnt == 8) { cnt--; break; }
                                        }

                                        i += 1;
                                        trigger = 0; end = 0;
                                    }
                                }
                            }
                        }

                        //_recordingSeries[CHANNEL_POTENTIAL][i] * -1000; // E [mV]
                        //_recordingSeries[CHANNEL_CURRENT][i] * ((double)_selectedRange); // I [uA]

                        //double cap = 0.0, res = 0.0;
                        if(cnt != 0)
                        {
                            Console.WriteLine("Measurement was done (Ru: " + (res / cnt).ToString("0.00") + " kOhm, Cd: " + (cap / cnt).ToString("0.00") + " uF)");
                            toolStripStatusLabelStatus.Text = "Measurement was done (Ru: " + (res/cnt).ToString("0.00") + " kOhm, Cd: " + (cap/cnt).ToString("0.00") + " uF; set the dial to <" + ((res / cnt) / (1000 / (double)_selectedCurrentFactor) * 1000).ToString("0") + ")";
                            //Console.WriteLine("Analysis of IRC curve (trigger, {0}): {1} - {2}", trigger, begin, end);
                            //if(_nlr.perform(ref _recordingSeries[0], ref _recordingSeries[CHANNEL_CURRENT], (double)_selectedRange, (double)_millivoltVertex, trigger, end, out cap, out res))
                            //{
                            //    toolStripStatusLabelStatus.Text = "Measurement was done (Ru: " + res.ToString("0.0") + " ohm, Cd: " + (cap).ToString("0.0") + " uF)";
                            //}
                            //else
                            //{
                            //    toolStripStatusLabelStatus.Text = "Measurement was done (IRC constant cannot be determined)";
                            //}
                        }
                        else
                        {
                            toolStripStatusLabelStatus.Text = "Measurement was done (IRC constant cannot be determined)";
                        }
                    }
                    else if(_selectedMethod == methodMeasurement.OSWV) 
                    {
                        // OSWV測定後、正方向と負方向のパルス印加時に記録された電流を引き算し、OSWVを得る
                        // 演算対象の電流のタイミングは、記録された電位の変化から決定する。

                        // TODO: ステップ電位stepを固定値でなくす必要がある。

                        int trigger = 0, begin = 0, end = 0;

                        //double cap = 0.0, res = 0.0; int cnt = 0;
                        double sampling_percentage = 0.35; // 0.3だとエッジ検出が甘いせいで、次のパルスの先頭に採取点がきたりする
                        double herz = _hertzAcquisition;
                        int direction = (_millivoltVertex > _millivoltInitial) ? 1 : -1;
                        double step = 10.0 * direction; double pulse = 25.0 * direction; // ここの電位は、設定電位のままで良い(=電位の変換は不要)はず
                        int c = 1; int sampling = (int)Math.Round(step / (_millivoltScanrate/POTENTIAL_SLOPE_OSC) / 2 * herz * sampling_percentage);
                        Console.WriteLine("OSWV sampling pos.: {0} ({1} from end of pulse)", sampling, sampling_percentage);

                        double millivolt_actual_initial_voltage = Double.NaN;// -5.6
                        double voltage_scanning = Double.NaN;
                        const int k = 16;

                        for (int i = 0; i < _itrRecording; i++)
                        {
                            if(!Double.IsNaN(voltage_scanning) && Double.IsNaN(millivolt_actual_initial_voltage))
                            {
                                // 初期電圧の取り方: 移動平均からのずれが5mV以上だと、測定が始まったと解釈する
                                if(Math.Abs(((_recordingSeries[CHANNEL_POTENTIAL][i] * POTENTIAL_SCALE - POTENTIAL_OFFSET_OSC) / POTENTIAL_SLOPE_OSC) - voltage_scanning) > Math.Abs(step / 2))
                                {           
                                    millivolt_actual_initial_voltage = voltage_scanning;
                                }
                            }

                            if (i >= (k - 1))
                            {
                                double v = 0.0;
                                for (int j = 0; j < k; j++)
                                {
                                    v += (_recordingSeries[CHANNEL_POTENTIAL][i - j] * POTENTIAL_SCALE - POTENTIAL_OFFSET_OSC) / POTENTIAL_SLOPE_OSC;
                                }
                                v /= k;
                                voltage_scanning = v;
                            }

                            if(!Double.IsNaN(millivolt_actual_initial_voltage))
                            {
                                //double v1 = _recordingSeries[CHANNEL_POTENTIAL][i] * POTENTIAL_SCALE;
                                //double v2 = millivolt_actual_initial_voltage + (c * step - pulse);
                                //double v3 = millivolt_actual_initial_voltage + ((c + 1) * step + pulse);
                                //Console.WriteLine($"current: {v1}; trigger: {v2}; end: {v3}; delta1: {v1-v2}; delta2: {v1-v3}");
                                if (trigger == 0 && (Math.Abs(((_recordingSeries[CHANNEL_POTENTIAL][i] * POTENTIAL_SCALE - POTENTIAL_OFFSET_OSC) / POTENTIAL_SLOPE_OSC) - (double)(millivolt_actual_initial_voltage + (c * step - pulse))) < Math.Abs(pulse))) // この閾値(pulse = 25.0)によって結構検出されなかったりされたりする...
                                {
                                    Console.WriteLine($"FOUND TRIGGER! at {i}");
                                    // 電位が(基準スイープ電圧) + (-25mV)になったときにtrigger(下向きのエッジに相当)を設定する
                                    trigger = i + 0;
                                }

                                if (trigger != 0)
                                {
                                    //if( begin == 0 && ((_recordingSeries[0][i] / 1000) >= ((_recordingSeries[0][trigger] / 1000) + (_millivoltScanrate / (4096 / 256)) * 1)) )
                                    //{
                                    //    begin = i + 1;
                                    //}
                                    //if ( end == 0 && ((_recordingSeries[0][i] / 1000) >= ((_recordingSeries[0][trigger] / 1000) + (10.0 / _millivoltScanrate) / 2)) ) // triggerの時間に+/-パルスの時間幅を加えたのが、パルスの終了端の時間
                                    if ( end == 0 && (Math.Abs(((_recordingSeries[CHANNEL_POTENTIAL][i] * POTENTIAL_SCALE - POTENTIAL_OFFSET_OSC) / POTENTIAL_SLOPE_OSC) - (double)(millivolt_actual_initial_voltage + ((c+1) * step + pulse))) < Math.Abs(pulse))) // この閾値(pulse = 25.0)によって結構検出されなかったりされたりする...
                                    {
                                        Console.WriteLine($"FOUND END! at {i}");

                                        end = i - 0;

                                        // 計算に使用したデータ点をプロットしておく
                                        chartVoltammogram.Series[5].Points.AddXY(_recordingSeries[0][trigger - sampling]/1000, (_recordingSeries[CHANNEL_CURRENT][trigger - sampling] - CURRENT_OFFSET) * ((double)_selectedCurrentFactor));
                                        chartVoltammogram.Series[5].Points.AddXY(_recordingSeries[0][end - sampling]/1000, (_recordingSeries[CHANNEL_CURRENT][end - sampling] - CURRENT_OFFSET) * ((double)_selectedCurrentFactor));

                                        _voltammogram.AddDataToCurrentSeries(
                                            _series,
                                            true,
                                            (millivolt_actual_initial_voltage + (c * step)),
                                            formVoltammogram.typeAxisX.Potential_in_mV,
                                            (double)(1*(_recordingSeries[CHANNEL_CURRENT][trigger-sampling] - _recordingSeries[CHANNEL_CURRENT][end-sampling]) * ((double)_selectedCurrentFactor)),
                                            formVoltammogram.typeAxisY.Current_in_uA,
                                            _recordingSeries[0][trigger] / 1000.0
                                        );

                                        i += 1; c++;
                                        trigger = 0; end = 0;
                                    }
                                }
                            }
                        }

                        toolStripStatusLabelStatus.Text = "Measurement was done";
                    }
                    else
                    {
                        toolStripStatusLabelStatus.Text = "Measurement was done";
                    }

                    //if (   _selectedMethod != methodMeasurement.BulkElectrolysis
                    //    && _selectedMethod != methodMeasurement.EIS
                    //    && _selectedMethod != methodMeasurement.EIS_Open_Circuit
                    //    && _selectedMethod != methodMeasurement.EIS_Short_Circuit
                    //   )
                    //{
                    //    for (int i = 1; i < chartVoltammogram.Series[1].Points.Count; i++)
                    //    {
                    //        _writer_temp.Write("{0}\t{1}\t{2}", chartVoltammogram.Series[1].Points[i].YValues[0] / 1000.00, chartVoltammogram.Series[2].Points[i].YValues[0] / 1000.000, chartVoltammogram.Series[1].Points[i].XValue);
                    //        //writer.Write("{0}\t{1}\t{2}", chartVoltammogram.Series[1].Points[i].YValues[0], chartVoltammogram.Series[2].Points[i].YValues[0], chartVoltammogram.Series[1].Points[i].XValue);
                    //        _writer_temp.WriteLine();
                    //    }
                    //}
                    //_writer_temp.Close();

                    //_herzAcquisition = _herzAcquisition_prev;

                    if (_file_path == null || DEBUG_VOLTAMMOGRAM)
                    {

                    }
                    else if (
                                _selectedMethod == methodMeasurement.Cyclicvoltammetry
                             || _selectedMethod == methodMeasurement.CyclicvoltammetryQuick
                             || _selectedMethod == methodMeasurement.LSV
                             || _selectedMethod == methodMeasurement.Series_of_RDE_CV
                             || _selectedMethod == methodMeasurement.Series_of_RDE_LSV
                             || _selectedMethod == methodMeasurement.Cyclicgalvanometry
                            )
                    {
                        for (int i = 1; i < chartVoltammogram.Series[1].Points.Count; i++)
                        {
                            _writer.Write("{0}\t{1}\t{2}", chartVoltammogram.Series[1].Points[i].YValues[0] / 1000.00, chartVoltammogram.Series[2].Points[i].YValues[0] / 1000.000, chartVoltammogram.Series[1].Points[i].XValue);
                            _writer.WriteLine();
                        }
                        _writer.Close();
                    }
                    else if (_selectedMethod == methodMeasurement.OSWV)
                    {
                        _voltammogram.SaveDataOfCurrentSeries(_file_path, _exp_conditions);
                    }
                    else if (   _selectedMethod == methodMeasurement.EIS
                             || _selectedMethod == methodMeasurement.EIS_Open_Circuit
                             || _selectedMethod == methodMeasurement.EIS_Short_Circuit
                             || _selectedMethod == methodMeasurement.EIS_MottSchottkey
                            )
                    {
                        for (int i = 1; i < chartVoltammogram.Series[1].Points.Count; i++)
                        {
                            _writer.Write(
                                "{0}\t{1}\t{2}\t{3}", 
                                chartVoltammogram.Series[1].Points[i].YValues[0], 
                                chartVoltammogram.Series[1].Points[i].XValue, 
                                chartVoltammogram.Series[6].Points[i].XValue,
                                (_recordingSeries[CHANNEL_POTENTIAL][i] * POTENTIAL_SCALE - POTENTIAL_OFFSET_OSC) / POTENTIAL_SLOPE_OSC / 1000); 
                            _writer.WriteLine();
                        }
                        _writer.Close();
                    }
                    else if (   _selectedMethod == methodMeasurement.BulkElectrolysis
                             || _selectedMethod == methodMeasurement.ConstantCurrent
                            )
                    {
                        _writer.Close();
                    }
                    else
                    {
                        for (int i = 1; i < chartVoltammogram.Series[1].Points.Count; i++)
                        {
                            _writer.Write("{0}\t{1}\t{2}", chartVoltammogram.Series[1].Points[i].YValues[0] / 1000.00, chartVoltammogram.Series[2].Points[i].YValues[0] / 1000.000, chartVoltammogram.Series[1].Points[i].XValue);
                            _writer.WriteLine();
                        }
                        _writer.Close();
                    }
                    _voltammogram.EndRecording();
                    _itrRecording = -1;
                    break;

                case (int)statusMeasurement.MeasuremetWasCancelled: //
                    _voltammogram.EndRecording();
                    break;

                case (int)statusMeasurement.BackgroundProcessWasDone: //
                    _itrRecording = -1;
                    break;

                default:
                    //if(_recordingSeries[CHANNEL_POTENTIAL][_itrRecording] == Double.NaN)
                    //{
                    //    _itrRecording = progress
                    //    break;
                    //}
                    //chartVoltammogram.Series[1].Points.AddXY(_recordingSeries[0][progress] / 1000.00, _recordingSeries[1][progress]); // E [mV]
                    //chartVoltammogram.Series[2].Points.AddXY(_recordingSeries[0][progress] / 1000.00, (double)_recordingSeries[2][progress] * ((double)_selectedRange / 1000.0)); // I [uA]

                    chartVoltammogram.SuspendLayout();

                    double factorCurrent = 0.0;
                    switch (_selectedMode)
                    {
                        case modeMeasurement.voltammetry: factorCurrent = (double)_selectedCurrentFactor; break;
                        case modeMeasurement.galvanometry: factorCurrent = 1000.0; break; // (double)rangeCurrent.RangeRAW; break; (RangeRAW is 999, which is for the sake of expedience.)
                        case modeMeasurement.eis: break;
                    }

                    if (   _selectedMethod == methodMeasurement.Cyclicvoltammetry
                        || _selectedMethod == methodMeasurement.CyclicvoltammetryQuick
                        || _selectedMethod == methodMeasurement.LSV
                        || _selectedMethod == methodMeasurement.Series_of_RDE_CV
                        || _selectedMethod == methodMeasurement.Series_of_RDE_LSV
                        || _selectedMethod == methodMeasurement.Cyclicgalvanometry
                       )
                    {
                        //if (_flag_digitalfilter)
                        //{
                        if (true)
                        {
                            int k = (_is_using_digitalfilter) ? (int)Math.Round(_hertzAcquisition / _hertzTargetFilteringFrequency * 2) : 1;
                            if (k < 1) k = 1;
                            //const int k = 16;
                            if (progress >= (k - 1))
                            {
                                for (int i = ((_itrRecording > 0) ? _itrRecording : (k - 1)); i < progress; i++)
                                {
                                    double p = 0.0, c = 0.0;
                                    p = (_recordingSeries[CHANNEL_POTENTIAL][i] * POTENTIAL_SCALE - POTENTIAL_OFFSET_OSC); // E [mV]
                                    for (int j = 0; j < k; j++)
                                    {
                                        c += _recordingSeries[CHANNEL_CURRENT][i - j] - CURRENT_OFFSET;
                                    }
                                    c /= k;
                                    c *= (factorCurrent); // I [uA]

                                    if (_selectedMode == modeMeasurement.galvanometry)
                                    {
                                        double polarity = -1;
                                        p *= polarity / POTENTIAL_SLOPE_OSC / POTENTIAL_SLOPE_AWG;
                                        c *= polarity / POTENTIAL_SLOPE_OSC / POTENTIAL_SLOPE_AWG;
                                        //c = _millivoltInitial_raw;
                                    }
                                    else
                                    {
                                        p /= POTENTIAL_SLOPE_OSC;
                                        c /= CURRENT_SLOPE * (1 + _calibrate_potentiostat.ohmInternalResistance / (1000000 / factorCurrent));
                                    }

                                    chartVoltammogram.Series[1].Points.AddXY(_recordingSeries[0][i] / 1000.00, p); // E [mV]
                                    chartVoltammogram.Series[2].Points.AddXY(_recordingSeries[0][i] / 1000.00, c); // I [uA]

                                    _voltammogram.AddDataToCurrentSeries(
                                        _series,
                                        true,
                                        p,
                                        formVoltammogram.typeAxisX.Potential_in_mV,
                                        c,
                                        formVoltammogram.typeAxisY.Current_in_uA,
                                        _recordingSeries[0][i] / 1000.00
                                    );
                                }
                            }
                        }
                        else
                        {
                            for (int i = _itrRecording; i < progress; i++)
                            {
                                double c = _digital_filter_notch.Process(_recordingSeries[CHANNEL_CURRENT][i] - CURRENT_OFFSET);
                                c = _digital_filter_lowpass.Process(_recordingSeries[CHANNEL_CURRENT][i] - CURRENT_OFFSET);

                                chartVoltammogram.Series[1].Points.AddXY(_recordingSeries[0][i] / 1000.00, _recordingSeries[CHANNEL_POTENTIAL][i] * POTENTIAL_SCALE); // E [mV]
                                chartVoltammogram.Series[2].Points.AddXY(_recordingSeries[0][i] / 1000.00, (double)c * (factorCurrent)); // I [uA]

                                _voltammogram.AddDataToCurrentSeries(
                                    _series,
                                    true,
                                    _recordingSeries[CHANNEL_POTENTIAL][i] * POTENTIAL_SCALE,
                                    formVoltammogram.typeAxisX.Potential_in_mV,
                                    (double)c * (factorCurrent),
                                    formVoltammogram.typeAxisY.Current_in_uA,
                                    _recordingSeries[0][i] / 1000.00
                                );
                            }
                        }
                        //}
                        //else
                        //{
                        //    for (int i = _itrRecording; i < progress; i++)
                        //    {
                        //        double p = 0.0, c = 0.0;
                        //        p = (_recordingSeries[CHANNEL_POTENTIAL][i] * POTENTIAL_SCALE - POTENTIAL_OFFSET_OSC); // E [mV]
                        //        c = (_recordingSeries[CHANNEL_CURRENT][i] - CURRENT_OFFSET) * factorCurrent; // I [uA]

                        //        if (_selectedMode == modeMeasurement.galvanometry)
                        //        {
                        //            double polarity = -1;
                        //            p *= polarity / POTENTIAL_SLOPE_OSC / POTENTIAL_SLOPE_AWG;
                        //            c *= polarity / POTENTIAL_SLOPE_OSC / POTENTIAL_SLOPE_AWG;
                        //            //c = _millivoltInitial_raw;
                        //        }
                        //        else
                        //        {
                        //            p /= POTENTIAL_SLOPE_OSC;
                        //            c /= CURRENT_SLOPE * (1 + _calibrate_potentiostat.ohmInternalResistance / (1000000 / factorCurrent));
                        //        }

                        //        chartVoltammogram.Series[1].Points.AddXY(_recordingSeries[0][i] / 1000.00, p); // E [mV]
                        //        chartVoltammogram.Series[2].Points.AddXY(_recordingSeries[0][i] / 1000.00, c); // I [uA]

                        //        // Show the actual voltammogram
                        //        //_voltammogram.AddDataToCurrentSeries(_recordingSeries[1][progress], (double)_recordingSeries[2][progress] * ((double)_selectedRange / 1000.0));
                        //        _voltammogram.AddDataToCurrentSeries(
                        //            _series,
                        //            true,
                        //            p,
                        //            formVoltammogram.typeAxisX.Potential_in_mV,
                        //            c,
                        //            formVoltammogram.typeAxisY.Current_in_uA,
                        //            _recordingSeries[0][i] / 1000.00
                        //        );
                        //    }
                        //}
                    }
                    else if (   _selectedMethod == methodMeasurement.BulkElectrolysis
                             || _selectedMethod == methodMeasurement.ConstantCurrent
                            )
                    {
                        if (Math.Abs(_recordingSeries[CHANNEL_CURRENT][_itrRecording] - CURRENT_OFFSET) > (_voltAnalogInChannelRange_Current / 2 - 0.05)
                            || Math.Abs(_recordingSeries[CHANNEL_CURRENT][_itrRecording] - CURRENT_OFFSET) > 11.6)
                        {
                            if (!_is_warned_about_potential_limit)
                            {
                                _is_warned_about_potential_limit = true;


                                // MessageBox.Showはモーダルなので、処理を継続するにはここでThreadStartする必要がある
                                new Thread(new ThreadStart(delegate
                                {
                                    MessageBox.Show
                                    (
                                      "The potential of the current follower indicated that the current might be overflowed. Try to switch the potential range to +-25V.",
                                      "Warning",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Warning
                                    );
                                })).Start();
                            }
                        }

                        //double polarity = (_selectedMethod != methodMeasurement.ConstantCurrent) ? 1 : -1;
                        for (int i = _itrRecording; i < progress; i++)
                        {
                            double p = 0.0, c_raw = 0.0, c = 0.0;
                            p = (_recordingSeries[CHANNEL_POTENTIAL][i] * POTENTIAL_SCALE - POTENTIAL_OFFSET_OSC); // E [mV]
                            c_raw = (_recordingSeries[CHANNEL_CURRENT][i] - CURRENT_OFFSET) * (factorCurrent); // I [uA]
                            Console.WriteLine("{0}, {1}", c_raw, _recordingSeries[CHANNEL_CURRENT][i] - CURRENT_OFFSET);

                            if (_selectedMode == modeMeasurement.galvanometry)
                            {
                                double polarity = -1;
                                p *= polarity / POTENTIAL_SLOPE_OSC / POTENTIAL_SLOPE_AWG;
                                c_raw *= polarity / POTENTIAL_SLOPE_OSC / POTENTIAL_SLOPE_AWG;
                                c = _millivoltInitial_raw; // _millivoltInitialはカレントフォロアの設定に応じた値となっている
                            }
                            else
                            {
                                p /= POTENTIAL_SLOPE_OSC;
                                c_raw /= CURRENT_SLOPE * (1 + _calibrate_potentiostat.ohmInternalResistance / (1000000 / factorCurrent));
                                c = c_raw;
                            }

                            chartVoltammogram.Series[1].Points.AddXY(_recordingSeries[0][i] / 1000.00, p); // E [mV]
                            chartVoltammogram.Series[2].Points.AddXY(_recordingSeries[0][i] / 1000.00, c_raw); // I [uA]
                            //Console.WriteLine("{0}, {1}", _recordingSeries[CHANNEL_CURRENT][i], CURRENT_OFFSET);

                            //c_actual = -1 * _millivoltInitial * (double)_selectedCurrentFactor / 1000

                            _voltammogram.AddDataToCurrentSeries(
                                _series,
                                false,
                                p,
                                formVoltammogram.typeAxisX.Potential_in_mV,
                                c,
                                formVoltammogram.typeAxisY.Current_in_uA,
                                _recordingSeries[0][i] / 1000.00 // [s]
                            );

                            // 所定の電荷量に達したら、CPEを終了する
                            double dt = _recordingSeries[0][i] - ((i > 0) ? _recordingSeries[0][i - 1] : 0); // [ms]
                            _coulombPassingThroughCell += c * (dt / 1000.0) / 1000000.0; // [C]
                            if ((_countRepeat > 0.0) && Math.Abs(_coulombPassingThroughCell) >= _countRepeat)
                            {
                                backgroundWorkerCV.CancelAsync();
                            }

                            if (_file_path != null && !DEBUG_VOLTAMMOGRAM)
                            {
                                _writer.WriteLine(
                                    "{0}\t{1}\t{2}",
                                    p / 1000,
                                    c / 1000,
                                    _recordingSeries[0][i] / 1000.00
                                );

                                if (((progress + 1) % 10) == 0)
                                {
                                    _writer.Flush();
                                }
                            }
                        }
                    }
                    else if (_selectedMethod == methodMeasurement.EIS
                             || _selectedMethod == methodMeasurement.EIS_Open_Circuit
                             || _selectedMethod == methodMeasurement.EIS_Short_Circuit
                             || _selectedMethod == methodMeasurement.EIS_MottSchottkey
                            )
                    {
                        for (int i = _itrRecording; i < progress; i++)
                        {
                            chartVoltammogram.Series[1].Points.AddXY(_recordingSeries[CHANNEL_VIRTUAL_REAL_Z][i], _recordingSeries[CHANNEL_VIRTUAL_IM_Z][i]);
                            chartVoltammogram.Series[6].Points.AddXY(_recordingSeries[CHANNEL_VIRTUAL_FREQ][i], Math.Sqrt(Math.Pow(_recordingSeries[CHANNEL_VIRTUAL_REAL_Z][i], 2) + Math.Pow(_recordingSeries[CHANNEL_VIRTUAL_IM_Z][i], 2))); //CHANNEL_VIRTUAL_ATTN

                            _voltammogram.AddDataToCurrentSeries(
                                _series,
                                true,
                                (_recordingSeries[CHANNEL_POTENTIAL][i] * POTENTIAL_SCALE - POTENTIAL_OFFSET_OSC) / POTENTIAL_SLOPE_OSC, // 電位の変換が必要 //_millivoltInitial + (POTENTIAL_OFFSET_AWG),
                                formVoltammogram.typeAxisX.Potential_in_mV,
                                0,
                                formVoltammogram.typeAxisY.Current_in_uA,
                                _recordingSeries[0][i] / 1000.00, // [s]
                                _recordingSeries[CHANNEL_VIRTUAL_REAL_Z][i],
                                _recordingSeries[CHANNEL_VIRTUAL_IM_Z][i],
                                _recordingSeries[CHANNEL_VIRTUAL_FREQ][i]
                            );
                        }

                        //Console.WriteLine("_itrRecording: {0}", _itrRecording);
                        if (_itrRecording == 1) chartVoltammogram.Series[6].Points.RemoveAt(0);


                        double order = Math.Truncate(Math.Log10(Math.Abs(_recordingSeries[CHANNEL_VIRTUAL_REAL_Z][_itrRecording])));
                        double interval = Math.Pow(10, order - 1);
                        double interval_f = Math.Log10(Math.Abs(_recordingSeries[CHANNEL_VIRTUAL_REAL_Z][_itrRecording])) % 1;


                        //double m = Math.Ceiling(Math.Pow(10, interval_f));
                        double m = (interval_f < 0.35) ? 2.5 : ((interval_f < 0.7) ? 5 : 10);

                        //double min = Math.Pow(10, 1+Math.Floor(Math.Log10(Math.Abs(_recordingSeries[CHANNEL_VIRTUAL_REAL_Z][_itrRecording]))));
                        double min = Math.Pow(10, order) * m;
                        //Console.WriteLine("{0}, {1}, {2}, {3}", order, interval_f, m, min);

                        double order2 = Math.Floor(Math.Log10((_recordingSeries[CHANNEL_VIRTUAL_FREQ][_itrRecording])));
                        double min2 = Math.Pow(10, order2);

                        if (_itrRecording >= 1)
                        {
                            if (Double.IsNaN(chartVoltammogram.ChartAreas[0].AxisX.Maximum))
                            {
                                chartVoltammogram.ChartAreas[0].AxisX.Maximum = +1 * min;
                            }
                            else
                            {
                                if (chartVoltammogram.ChartAreas[0].AxisX.Maximum < (+1 * min)) chartVoltammogram.ChartAreas[0].AxisX.Maximum = +1 * min;
                            }

                            if (Double.IsNaN(chartVoltammogram.ChartAreas[0].AxisX2.Minimum))
                            {
                                chartVoltammogram.ChartAreas[0].AxisX2.Minimum = min2;
                            }
                            else
                            {
                                if (chartVoltammogram.ChartAreas[0].AxisX2.Minimum >= (min2)) chartVoltammogram.ChartAreas[0].AxisX2.Minimum = min2;
                            }
                        }
                        else if (_itrRecording == 0)
                        {
                            chartVoltammogram.ChartAreas[0].AxisX.Maximum = +1 * min;
                            chartVoltammogram.ChartAreas[0].AxisX2.Minimum = min2;
                        }

                        //chartVoltammogram.ResumeLayout();
                        //chartVoltammogram.Update();
                    }
                    else if (_selectedMethod == methodMeasurement.Oscilloscope
                            )
                    {
                        // ほかのmethodとは違って、積算する。積算自体は、backgroundWorkerCV_DoWorkで行い、ここでは表示のみを担当する
                        // 毎回、全データを更新することになる

                        chartVoltammogram.Series[1].Points.Clear();
                        chartVoltammogram.Series[2].Points.Clear();

                        for (int i = 0; i < progress; i++) // 毎回、全データを更新
                        {
                            double p = 0.0, c_raw = 0.0, c = 0.0;
                            p = (_recordingSeries[CHANNEL_POTENTIAL][i] * POTENTIAL_SCALE - POTENTIAL_OFFSET_OSC); // E [mV]
                            c_raw = (_recordingSeries[CHANNEL_CURRENT][i] - CURRENT_OFFSET) * (factorCurrent); // I [uA]

                            p /= POTENTIAL_SLOPE_OSC;
                            c_raw /= CURRENT_SLOPE * (1 + _calibrate_potentiostat.ohmInternalResistance / (1000000 / factorCurrent));
                            c = c_raw;

                            // 横軸はとりあえずmsスケールでやってみる
                            chartVoltammogram.Series[1].Points.AddXY(_recordingSeries[0][i] / 1.0, p); // E [mV]
                            chartVoltammogram.Series[2].Points.AddXY(_recordingSeries[0][i] / 1.0, c_raw); // I [uA]
                        }
                    }
                    //else if (_selectedMethod == methodMeasurement.
                    //        )
                    //{
                    //}
                    else
                    {
                        for (int i = _itrRecording; i < progress; i++)
                        {
                            double p = 0.0, c = 0.0;
                            p = (_recordingSeries[CHANNEL_POTENTIAL][i] * POTENTIAL_SCALE - POTENTIAL_OFFSET_OSC); // E [mV]
                            c = (_recordingSeries[CHANNEL_CURRENT][i] - CURRENT_OFFSET) * factorCurrent; // I [uA]

                            if (_selectedMode == modeMeasurement.galvanometry)
                            {
                                double polarity = -1;
                                p *= polarity / POTENTIAL_SLOPE_OSC / POTENTIAL_SLOPE_AWG;
                                c *= polarity / POTENTIAL_SLOPE_OSC / POTENTIAL_SLOPE_AWG;
                            }
                            else
                            {
                                if (_selectedMethod == methodMeasurement.OCP)
                                {
                                    // TODO: OCP測定に正確に対応する必要がある
                                    p /= (1.0);
                                }
                                else
                                {
                                    p /= (POTENTIAL_SLOPE_OSC);
                                }
                                c /= CURRENT_SLOPE * (1 + _calibrate_potentiostat.ohmInternalResistance / (1000000 / factorCurrent));
                            }

                            chartVoltammogram.Series[1].Points.AddXY(_recordingSeries[0][i] / 1000.00, p); // E [mV]
                            chartVoltammogram.Series[2].Points.AddXY(_recordingSeries[0][i] / 1000.00, c); // I [uA]
                        }
                    }
                    chartVoltammogram.ResumeLayout();
                    //chartVoltammogram.Update();

                    if(!
                        (   _selectedMethod == methodMeasurement.EIS
                         || _selectedMethod == methodMeasurement.EIS_Open_Circuit
                         || _selectedMethod == methodMeasurement.EIS_Short_Circuit
                         || _selectedMethod == methodMeasurement.EIS_MottSchottkey
                        )
                      )
                    {
                        if ((true) && (((progress+1) % 10) == 0))
                        {
                            //toolStripStatusCurrentEandI.Text = "("
                            //                                    + (_recordingSeries[0][progress] / 1000.00).ToString("0.0") + " s, "
                            //                                    + (_recordingSeries[1][progress]).ToString("0.0") + " mV, "
                            //                                    + ((double)_recordingSeries[2][progress] * ((double)_selectedRange / 1000.0)).ToString("0.0") + " uA"
                            //                                    + ")";

                            //double polarity = (_selectedMode != modeMeasurement.galvanometry) ? 1 : -1;
                            double p = 0.0, c = 0.0;
                            for (int i = 0; i < 10; i++)
                            {
                                p += (_recordingSeries[CHANNEL_POTENTIAL][progress - i] * POTENTIAL_SCALE - POTENTIAL_OFFSET_OSC);
                                c += ((_recordingSeries[CHANNEL_CURRENT][progress - i] - CURRENT_OFFSET) * factorCurrent);
                            }
                            if(_selectedMode == modeMeasurement.galvanometry)
                            {
                                double polarity = -1;
                                p /= polarity * (10 * POTENTIAL_SLOPE_OSC) * POTENTIAL_SLOPE_AWG;
                                c /= polarity * (10 * POTENTIAL_SLOPE_OSC) * POTENTIAL_SLOPE_AWG;
                                //c /= polarity * (10 * CURRENT_SLOPE) / (CURRENT_SLOPE / POTENTIAL_SLOPE_OSC) * POTENTIAL_SLOPE_AWG;
                            }
                            else
                            {
                                if(_selectedMethod == methodMeasurement.OCP)
                                {
                                    // TODO: OCP測定に正確に対応する必要がある
                                    p /= (10 * 1.0);
                                }
                                else 
                                {
                                    p /= (10 * POTENTIAL_SLOPE_OSC);
                                }
                                c /= (10 * CURRENT_SLOPE) * (1 + _calibrate_potentiostat.ohmInternalResistance / (1000000 / factorCurrent)); //c *= (1 + 0.5 / (1000000 / (double)Potentiostat.rangeCurrent.Range20mA));
                            }

                            toolStripStatusCurrentEandI.Text = "("
                                                                + (_recordingSeries[0][progress] / 1000.00).ToString("0.0") + " s, "
                                                                + p.ToString("0.00") + " mV, "
                                                                + c.ToString("0.00") + " uA"
                                                                + ")";
                        }
                    }

                    _itrRecording = progress; // this means that the last datum is not recorded to chartVoltammogram, because of "for (int i = _itrRecording; i < progress; i++)"
                    break;
            }
            Thread.Sleep(1);
            Thread.Yield();
        }

        private void backgroundWorkerCV_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            toolStripButtonRecord.Enabled = true;
            toolStripButtonRecord.Text = "&Record"; toolStripButtonRecord.Image = global::Voltammogrammer.Properties.Resources.Run;
            toolStripButtonScan.Enabled = true;
            toolStripComboBoxMethod.Enabled = true;
            toolStripComboBoxRange.Enabled = true;
            toolStripComboBoxReferenceForInitialPotential.Enabled = true;

            toolStripTextBoxInitialV.Enabled = true;
            toolStripTextBoxVertexV.Enabled = true;
            toolStripTextBoxScanrate.Enabled = true;
            toolStripTextBoxRepeat.Enabled = true;

            //backgroundWorkerCV.ReportProgress((int)statusMeasurement.BackgroundProcessWasDone);

            if (!DEBUG_VOLTAMMOGRAM) timerCurrentEandI.Enabled = true;

            //toolStripStatusLabelStatus.Text = "Ready";
        }

        //
        // Event Handlers for Other Controls
        //

        private void toolStripButtonConnect_Click(object sender, EventArgs e)
        {
            if(DEBUG_VOLTAMMOGRAM)
            {
                Console.WriteLine($"DEBUG_VOLTAMMOGRAM");

                _calibrate_potentiostat.setID("");
                this.Text = "Voltammogrammer " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()
                          + " for PocketPotentiostat " + VERSION_string
                          + " (demo)";

                _readingBuffers = new double[MAX_CHANNELS][];

                for (int i = 0; i < MAX_CHANNELS; i++)
                {
                    _readingBuffers[i] = new double[BUFFER_SIZE];
                }

                _recordingSeries = new double[13][];

                for (int i = 0; i < _recordingSeries.GetLength(0); i++)
                {
                    _recordingSeries[i] = new double[BUFFER_SIZE];// 1024*32];
                }

                //SetGalvanostat(false);
                //SetEIS(false);
                SetMode(modeMeasurement.voltammetry);

                toolStripMenuItemSubModuleOption3Param1.SelectedIndex = 0;
                toolStripComboBoxMethod.SelectedIndex = 0;
                toolStripComboBoxRange.SelectedIndex = 3;

                toolStripButtonRecord.Enabled = true;
                toolStripButtonScan.Enabled = true;
                toolStripButtonConnect.Enabled = false;

                //FDwfAnalogInConfigure(_handle, Convert.ToInt32(false), Convert.ToInt32(false));
                //timerCurrentEandI.Enabled = true;

                //this.Show();
                this.Focus();

                return;
            }


            //_tableDevice.Rows.Clear();

            //FDwfEnum(enumfilterAll, out int c);

            //for (int i = 0; i < c; i++)
            //{
            //    FDwfEnumDeviceType(i, out int type, out int rev);
            //    if (type == devidDiscovery2)
            //    {
            //        FDwfEnumSN(i, out string sn);

            //        _tableDevice.Rows.Add(sn, i);
            //    }
            //}

            int idxDevice;

            if(toolStripComboBoxSerialPort.ComboBox.SelectedValue == null)
            {
                idxDevice = -1;
            }
            else
            {
                idxDevice = (int)toolStripComboBoxSerialPort.ComboBox.SelectedValue;
            }
            Console.WriteLine($"Connected device: {toolStripComboBoxSerialPort.ComboBox.Text}");

            // Open connection to device
            status = FDwfDeviceOpen(idxDevice, out _handle);
            FDwfGetLastErrorMsg(out string err);

            // If handle is zero or -1 there is an issue, will also need to change power source if the power supply is not connected
            // (for PicoScope 3400/ 3400D models) or if not using a USB 3.0 port
            if (_handle == 0 || _handle == -1)
            {
                //Console.WriteLine("Cannot open device - status code: " + status.ToString());
                MessageBox.Show(this, "Cannot open device - status code: \n" + err, "Error Opening Device", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //this.Close();

                return;
            }
            //else if (status != StatusCodes.PICO_OK)
            //{
            //    status = Imports.ChangePowerSource(_handle, status);
            //}
            _calibrate_potentiostat.setID(toolStripComboBoxSerialPort.ComboBox.Text);
            this.Text = "Voltammogrammer " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() 
                      + " for PocketPotentiostat " + VERSION_string 
                      + " (" + toolStripComboBoxSerialPort.ComboBox.Text + ")";

            //System.Text.StringBuilder sb = new System.Text.StringBuilder(32);
            //FDwfGetVersion(sb);
            ////char[] trim = { ' ' };
            ////toolStripStatusLabelModelOfPicoscope.Text = "WaveForms " + (sb.ToString().TrimEnd(trim));
            //toolStripStatusLabelModelOfPicoscope.Text = "WaveForms " + (sb.ToString());

            //Console.WriteLine("Connected to AnalogDiscovery.");

            // 波形をオシロするための準備
            FDwfAnalogInChannelEnableSet(_handle, (CHANNEL_POTENTIAL - 1), Convert.ToInt32(true));// for potential monitoring
            //FDwfAnalogInChannelOffsetSet(_handle, 0, 0.0);
            //FDwfAnalogInChannelRangeSet(_handle, 0, 5.0); // +-5Vレンジに設定
            FDwfAnalogInChannelEnableSet(_handle, (CHANNEL_CURRENT - 1), Convert.ToInt32(true)); // for current monitoring
            //FDwfAnalogInChannelOffsetSet(_handle, 1, 0.0);
            //FDwfAnalogInChannelRangeSet(_handle, 1, 5.0); // +-0.5Vレンジに設定
            SelectPotentialRange(_selectedPotentialRange);

            double min=0, max=0, step=0;
            FDwfAnalogInChannelRangeInfo(_handle, out min, out max, out step);
            Console.WriteLine($"Channel In Range: min:{min}, max:{max}, step:{step}");

            FDwfAnalogInFrequencyInfo(_handle, out min, out max);
            Console.WriteLine($"ADC Range: min:{min}, max:{max} Hz");

            int smin = 0, smax = 0, size=0;
            FDwfAnalogInBufferSizeInfo(_handle, out smin, out smax);
            FDwfAnalogInBufferSizeGet(_handle, out size);
            Console.WriteLine($"BufferSize: min:{smin}, max:{smax}, current:{size}");

            FDwfAnalogOutNodeOffsetInfo(_handle, 0, AnalogOutNodeCarrier, out min, out max);
            Console.WriteLine($"Channel Output Range: min:{min}, max:{max}");

            // AWGの準備
            // enable first channel
            FDwfAnalogOutNodeEnableSet(_handle, 0, AnalogOutNodeCarrier, Convert.ToInt32(true));
            FDwfAnalogOutNodeEnableSet(_handle, 1, AnalogOutNodeCarrier, Convert.ToInt32(true));

            _readingBuffers = new double[MAX_CHANNELS][];

            for (int i = 0; i < MAX_CHANNELS; i++)
            {
                _readingBuffers[i] = new double[BUFFER_SIZE];
            }

            _recordingSeries = new double[13][];

            for (int i = 0; i < _recordingSeries.GetLength(0); i++)
            {
                _recordingSeries[i] = new double[BUFFER_SIZE];// 1024*32];
            }

            _recordingSeries[CHANNEL_VIRTUAL_OPEN_RS][0] = Double.NaN;
            _recordingSeries[CHANNEL_VIRTUAL_OPEN_XS][0] = Double.NaN;
            _recordingSeries[CHANNEL_VIRTUAL_SHORT_RS][0] = Double.NaN;
            _recordingSeries[CHANNEL_VIRTUAL_SHORT_XS][0] = Double.NaN;
            _recordingSeries[CHANNEL_VIRTUAL_PHASE][0] = Double.NaN;


            // Establish connection to Arduino, or Digital I/O from AD2


            if (DIGITALIO_FROM_ARDUINO)
            {
            }
            else
            {
                if (VERSION_0_9d || VERSION_0_9e || VERSION_0_9k)
                {
                    FDwfDigitalIOOutputEnableSet(_handle, 0b1111111111111111);
                }
            }

            if (TurnPowerSupply(true))
            {
                //System.Text.StringBuilder sb = new System.Text.StringBuilder(32);
                FDwfGetVersion(out string sb);
                //char[] trim = { ' ' };
                //toolStripStatusLabelModelOfPicoscope.Text = "WaveForms " + (sb.ToString().TrimEnd(trim));
                toolStripStatusLabelModelOfPicoscope.Text = "WaveForms " + (sb.ToString());
                Console.WriteLine("Connected to AnalogDiscovery.");
            }
            else
            {
                toolStripStatusLabelModelOfPicoscope.Text = "Failed to power PocketPotentiostat on";
                FDwfDeviceClose(_handle);
                MessageBox.Show(this, "Failed to power PocketPotentiostat on. \nPlease check the auxiliary power supply.", "Error Opening Device", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int value;
            FDwfParamGet(DwfParamOnClose, out value);
            Console.WriteLine($"Device close behavior: {value}");
            FDwfParamGet(DwfParamUsbLimit, out value);
            Console.WriteLine($"USB current limitation in mA: {value}");

            FDwfParamSet(DwfParamOnClose, 2);

            if (DIGITALIO_FROM_ARDUINO)
            {
                int index = toolStripComboBoxSerialPort.SelectedIndex;
                if (index != -1)
                {
                    _arduino.Open(toolStripComboBoxSerialPort.Items[index].ToString());
                }
                _arduino.selectCurrentRange(_selectedCurrentRange);
            }
            else
            {
                if (VERSION_0_5)
                {
                    FDwfDigitalIOOutputEnableSet(_handle, 0b1011111100111111);
                    FDwfDigitalIOOutputSet(_handle, 0b0000000000000000);
                }
                else if (VERSION_0_9c)
                {
                    FDwfDigitalIOOutputEnableSet(_handle, 0b1111111101111111);
                    FDwfDigitalIOOutputSet(_handle, 0b0000000000000000);

                    //  // self test of latching...
                    //  ////////////////////////////////////////////  CBA     CBA
                    ////FDwfDigitalIOOutputSet(_handle,           0b0011100000111000); Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                    //  FDwfDigitalIOOutputSet(_handle,           0b0010000000000000); Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                    //  FDwfDigitalIOOutputSet(_handle,           0b0001000000000000); Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                    //  FDwfDigitalIOOutputSet(_handle,           0b0000100000000000); Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);

                    //  Console.WriteLine("Now, check if the latching sounds 6 times.");
                    //  Thread.Sleep(2000);

                    //  Console.WriteLine("C-up?"); FDwfDigitalIOOutputSet(_handle,           0b0000000000100000); Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                    //  Console.WriteLine("B-up?"); FDwfDigitalIOOutputSet(_handle,           0b0000000000010000); Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                    //  Console.WriteLine("A-up?"); FDwfDigitalIOOutputSet(_handle,           0b0000000000001000); Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                    //  Console.WriteLine("C-dn?"); FDwfDigitalIOOutputSet(_handle,           0b0010000000000000); Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                    //  Console.WriteLine("B-dn?"); FDwfDigitalIOOutputSet(_handle,           0b0001000000000000); Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                    //  Console.WriteLine("A-dn?"); FDwfDigitalIOOutputSet(_handle,           0b0000100000000000); Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);

                    //  Console.WriteLine("Did sound 6 times?");
                    //  Thread.Sleep(2000);
                }
                else if (VERSION_0_9d)
                {
                    //FDwfDigitalIOOutputEnableSet(_handle, 0b1111111111111111);
                    //FDwfDigitalIOOutputSet(_handle, 0b0000000010000000);
                }
                else if (VERSION_0_9e)
                {
                    //FDwfDigitalIOOutputEnableSet(_handle, 0b1111111111111111);
                    //FDwfDigitalIOOutputSet(_handle, 0b0000000010000000);

                    //  // self test of latching...
                    //  //////////////////////////////////////////// CBA     CBA
                    ////FDwfDigitalIOOutputSet(_handle,           0b0111000001110000); Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                    //  FDwfDigitalIOOutputSet(_handle,           0b0100000000000000); Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                    //  FDwfDigitalIOOutputSet(_handle,           0b0010000000000000); Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                    //  FDwfDigitalIOOutputSet(_handle,           0b0001000000000000); Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);

                    //  Console.WriteLine("Now, check if the latching sounds 6 times.");
                    //  Thread.Sleep(2000);

                    //  Console.WriteLine("C-up?"); FDwfDigitalIOOutputSet(_handle,           0b0000000001000000); Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                    //  Console.WriteLine("B-up?"); FDwfDigitalIOOutputSet(_handle,           0b0000000000100000); Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                    //  Console.WriteLine("A-up?"); FDwfDigitalIOOutputSet(_handle,           0b0000000000010000); Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                    //  Console.WriteLine("C-dn?"); FDwfDigitalIOOutputSet(_handle,           0b0100000000000000); Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                    //  Console.WriteLine("B-dn?"); FDwfDigitalIOOutputSet(_handle,           0b0010000000000000); Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                    //  Console.WriteLine("A-dn?"); FDwfDigitalIOOutputSet(_handle,           0b0001000000000000); Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);

                    //  Console.WriteLine("Did sound 6 times?");
                    //  Thread.Sleep(2000);
                }
                else if (VERSION_0_9k)
                {
                }
                else
                {
                    // enable output/mask on IO pins, from DIO 0,1,2 and 8,9,10 (this code was talken from "digitalio.cpp")
                    //FDwfDigitalIOOutputEnableSet(_handle, 0b0000011100000111); // bit order = 0b(15, 14, ..., 2, 1, 0)
                    FDwfDigitalIOOutputEnableSet(_handle, 0b0000000011111111); // bit order = 0b(15, 14, ..., 2, 1, 0) for Electro-lab v0.2
                }

                //SetGalvanostat(false);
                //SetEIS(false);
                SetMode(modeMeasurement.voltammetry);

                SetCircuit(open:true);
                switch (_selectedSubModule)
                {
                    case typeSubModule.RDE:
                        SetRotation(0);
                        SetPurging(on: true);
                        break;

                    case typeSubModule.Synchronous_switching:
                        break;

                    default:
                        break;
                }
                SetDCVoltageCH1(0);

                SelectCurrentRange(_selectedCurrentRange);
            }
            toolStripMenuItemSubModuleOption3Param1.SelectedIndex = 0;
            toolStripComboBoxMethod.SelectedIndex = 0;
            toolStripComboBoxRange.SelectedIndex = 3;

            toolStripButtonRecord.Enabled = true;
            toolStripButtonScan.Enabled = true;
            toolStripButtonConnect.Enabled = false;

            FDwfAnalogInConfigure(_handle, Convert.ToInt32(false), Convert.ToInt32(false));
            timerCurrentEandI.Enabled = true;

            //this.Show();
            this.Focus();
        }

        private void toolStripButtonScan_Click(object sender, EventArgs e)
        {
            StartAcquisition(false);
        }

        private void toolStripButtonRecord_Click(object sender, EventArgs e)
        {
            StartAcquisition(true);
        }

        private void toolStripComboBoxMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            //_selectedMethod = (methodMeasurement)toolStripComboBoxMethod.SelectedIndex;
            if (toolStripComboBoxMethod.ComboBox.SelectedValue == null) return;
            _selectedMethod_previous = _selectedMethod;
            _selectedMethod = (methodMeasurement)toolStripComboBoxMethod.ComboBox.SelectedValue; Console.WriteLine(_selectedMethod);

            //if (_selectedMethod == _selectedMethod_previous) return;

            updateComboBoxMethod();
        }

        private void toolStripComboBoxRange_SelectedIndexChanged(object sender, EventArgs e)
        {
            //switch (toolStripComboBoxRange.SelectedIndex)
            //{
            //    case 0: _selectedCurrentFactor = rangeCurrent.Range20mA; break;
            //    case 1: _selectedCurrentFactor = rangeCurrent.Range2mA; break;
            //    case 2: _selectedCurrentFactor = rangeCurrent.Range200uA; break;
            //    case 3: _selectedCurrentFactor = rangeCurrent.RangeRAW; break;
            //    case 4: _selectedCurrentFactor = rangeCurrent.Range20uA; break;
            //    case 5: _selectedCurrentFactor = rangeCurrent.Range2uA; break;
            //    default: _selectedCurrentFactor = rangeCurrent.RangeNone; break;
            //}

            if (toolStripComboBoxRange.ComboBox.SelectedValue == null) return;
            //_selectedMethod_previous = _selectedMethod;

            _selectedCurrentRange = (rangeCurrent)toolStripComboBoxRange.ComboBox.SelectedValue;
            _selectedCurrentFactor = 1000000.0 / Double.Parse(_configure_potentiostat.tableResistors.Rows[toolStripComboBoxRange.ComboBox.SelectedIndex][1].ToString());
            Console.WriteLine("_selectedCurrentFactor: {0}", _selectedCurrentFactor);

            if (DIGITALIO_FROM_ARDUINO)
            {
                _arduino.selectCurrentRange(_selectedCurrentRange);
            }
            else
            {
                SelectCurrentRange(_selectedCurrentRange);
            }
        }

        private void toolStripMenuItemOCV_Click(object sender, EventArgs e)
        {
            if (DIGITALIO_FROM_ARDUINO)
            {
                _arduino.closeCircuit();
            }
            else
            {
                SetCircuit(!toolStripMenuItemOCV.Checked);
            }
        }

        private void toolStripMenuItemSubModule_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            int idx = item.DropDownItems.IndexOf(e.ClickedItem);

            SetSubmodule(idx);
        }

        private void toolStripMenuItemSubModuleOption1_Click(object sender, EventArgs e)
        {
            switch (_selectedSubModule)
            {
                case typeSubModule.RDE:
                    SetPurging(!toolStripMenuItemSubModuleOption1.Checked);
                    break;

                case typeSubModule.Synchronous_switching:
                    SetSwitchOnAsync(!toolStripMenuItemSubModuleOption1.Checked);
                    //toolStripMenuItemSubModuleOption1.Checked = !toolStripMenuItemSubModuleOption1.Checked;
                    break;

                default:
                    break;
            }
        }

        private void toolStripMenuItemSubModuleOption2_Click(object sender, EventArgs e)
        {
            switch (_selectedSubModule)
            {
                case typeSubModule.RDE:
                    break;

                case typeSubModule.Synchronous_switching:
                    break;

                default:
                    break;
            }
        }

        private void toolStripMenuItemSubModuleOption3_Click(object sender, EventArgs e)
        {
            switch (_selectedSubModule)
            {
                case typeSubModule.RDE:
                    if (toolStripMenuItemSubModuleOption3.Checked)
                    {
                        SetRotation(0);
                    }
                    else
                    {
                        string rpm = toolStripMenuItemSubModuleOption3Param1.Items[toolStripMenuItemSubModuleOption3Param1.SelectedIndex].ToString();
                        SetRotation(Convert.ToUInt16(rpm));
                    }
                    break;

                case typeSubModule.Synchronous_switching:
                    toolStripMenuItemSubModuleOption3.Checked = (_is_using_automatical_switcher = !toolStripMenuItemSubModuleOption3.Checked);
                    break;

                default:
                    break;
            }
        }

        //private int SwitchingInterval
        //{
        //    set;
        //    get;
        //} = 0;
        //private bool IsSwitchingAutomatically
        //{
        //    set
        //    {
        //        toolStripMenuItemSubModuleOption3.Checked = value;
        //    }
        //    get
        //    {
        //        return IsSwitchingAutomatically;
        //   }
        //}
        //private bool IsSwitchingFinallyOn
        //{
        //    set
        //    {
        //        toolStripMenuItemSubModuleOption3Param2.Checked = value;
        //    }
        //    get
        //    {
        //        return IsSwitchingFinallyOn;
        //    }
        //}
        //private async void SwitchingStateAsync(bool? value_to_be_set = null)
        //{
        //    if (value_to_be_set.HasValue)
        //    {
        //        // GUI操作は非同期で呼び出す
        //        await Task.Run(() => { SetPurging(value_to_be_set.Value); }); //SetPurging(on); 
        //    }
        //    else
        //    {
        //    }
        //}
        //public static readonly Func<bool?, string> GetDefinition = (new Func<Func<bool?, string>>(() =>
        //{
        //    string def = string.Empty;
        //    return (x) => string.IsNullOrEmpty(def) ? def = "ret" : def;
        //}))();
        //public bool GetSwitchingStatus(bool on)
        //{
        //    return (bool)Invoke((Func<bool>)delegate ()
        //    {
        //        return toolStripMenuItemSubModuleOption1.Checked;
        //    });
        //}
        //public bool SwitchingStateAsync
        //{
        //    set
        //    {
        //        ((Action)(async () =>
        //        {
        //            await Task.Run(() =>
        //            {
        //                SetPurging(value);
        //            });
        //        }))();
        //    }
        //    get
        //    {
        //        return (bool)Invoke((Func<bool>)delegate ()
        //        {
        //            return toolStripMenuItemSubModuleOption1.Checked;
        //        });
        //    }
        //}
        
        private void toolStripMenuItemSubModuleOption3Param1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (_selectedSubModule)
            {
                case typeSubModule.RDE:
                    if (toolStripMenuItemSubModuleOption3.Checked)
                    {
                        string rpm = toolStripMenuItemSubModuleOption3Param1.Items[toolStripMenuItemSubModuleOption3Param1.SelectedIndex].ToString();
                        SetRotation(Convert.ToUInt16(rpm));
                    }
                    break;

                case typeSubModule.Synchronous_switching:
                    break;

                default:
                    break; 
            }
        }

        private void toolStripMenuItemSubModuleOption3Param1_TextUpdate(object sender, EventArgs e)
        {
            switch (_selectedSubModule)
            {
                case typeSubModule.RDE:
                    break;

                case typeSubModule.Synchronous_switching:
                    if (double.TryParse(toolStripMenuItemSubModuleOption3Param1.Text, out double value)
                        && (value >= 1.0))
                    {
                        _secSwitchingInterval = value;
                        Console.WriteLine("Switcing Interval: {0}", _secSwitchingInterval);
                    }
                    else {  }
                    break;

                default:
                    break;
            }
        }

        private void toolStripMenuItemSubModuleOption3Param2_Click(object sender, EventArgs e)
        {
            switch (_selectedSubModule)
            {
                case typeSubModule.RDE:
                    break;

                case typeSubModule.Synchronous_switching:
                    toolStripMenuItemSubModuleOption3Param2.Checked = (_is_applying_switching_on_finally = !toolStripMenuItemSubModuleOption3Param2.Checked);
                    break;

                default:
                    break;
            }
        }

        private void toolStripMenuItemSubModuleOption4_Click(object sender, EventArgs e)
        {
            switch (_selectedSubModule)
            {
                case typeSubModule.RDE:
                    _select_rotation_speeds.Show();
                    break;

                case typeSubModule.Synchronous_switching:
                    break;

                default:
                    break;
            }
        }

        private void toolStripMenuItemSaveAvaragedData_Click(object sender, EventArgs e)
        {
            toolStripMenuItemSaveAvaragedData.Checked = (_is_using_digitalfilter = !toolStripMenuItemSaveAvaragedData.Checked);
        }

        private void timerCurrentEandI_Tick(object sender, EventArgs e)
        {
            double value;
            SampleSingleValue((CHANNEL_POTENTIAL - 1), out value);
            toolStripStatusCurrentEandI.Text = "(" + (value * -1 * POTENTIAL_SCALE).ToString("0.0") + " mV)";


            int r = FDwfAnalogIOStatus(_handle);
            //Console.WriteLine("FDwfAnalogIOStatus ({0})", r);
            int sts = 0;
            int ret = FDwfAnalogIOEnableStatus(_handle, out sts);
            //Console.WriteLine("FDwfAnalogIOEnableStatus ({0}): sts = {1}", ret, sts);
            if (r == 0 || ret == 0 || sts == 0)
            {
                timerCurrentEandI.Enabled = false;
                toolStripButtonRecord.Enabled = false;
                toolStripButtonScan.Enabled =false;
                toolStripButtonConnect.Enabled = true;
                FDwfDeviceClose(_handle);
                Console.WriteLine("Failed to power PocketPotentiostat on");
                MessageBox.Show(this, "Failed to power PocketPotentiostat on. \nPlease check the auxiliary power supply.\nOr, re-connect to PocketPotentiostat.", "Error Opening Device", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //
        // Event Handlers for Chart
        //

        //double _chartselectionY2max, _chartselectionY2min;
        //bool _chartselectionTuringOn = false;

        private void chartVoltammogram_AxisViewChanged(object sender, System.Windows.Forms.DataVisualization.Charting.ViewEventArgs e)
        {
            //return;

            ////double Y2max = e.ChartArea.AxisY2.PositionToValue(e.ChartArea.AxisY.ValueToPosition(e.Axis.ScaleView.ViewMaximum));
            ////double Y2min = e.ChartArea.AxisY2.PositionToValue(e.ChartArea.AxisY.ValueToPosition(e.Axis.ScaleView.ViewMinimum));

            ////Console.WriteLine(" X: " + e.Axis.ScaleView.ViewMaximum.ToString() + ", " + e.Axis.ScaleView.ViewMinimum.ToString());
            ////Console.WriteLine("Y2: " + Y2max + ", " + Y2min);

            //if (e.Axis.AxisName.ToString() == "Y")
            //{
            //    Console.WriteLine(e.Axis.AxisName + " changed" + e.ChartArea.AxisY.ScaleView.IsZoomed);
            //    if (!_chartselectionTuringOn) return;

            //    if(_chartselectionY2max > _chartselectionY2min)
            //    {
            //        e.ChartArea.AxisY2.ScaleView.Zoom(_chartselectionY2min, _chartselectionY2max);
            //    }
            //    else
            //    {
            //        e.ChartArea.AxisY2.ScaleView.Zoom(_chartselectionY2max, _chartselectionY2min);
            //    }
            //    _chartselectionTuringOn = false;
            //    //e.ChartArea.AxisY2.ScaleView.Zoom((_chartselectionY2max), (_chartselectionY2min));
            //    //e.ChartArea.AxisY2.ScaleView.Zoom(10,20);
            //    //e.ChartArea.AxisY2.ScaleView.Zoom(e.ChartArea.AxisY2.ValueToPosition(_chartselectionY2max), e.ChartArea.AxisY2.ValueToPosition(_chartselectionY2min));
            //    //Console.WriteLine("Zoom: " + e.ChartArea.AxisY2.ValueToPosition(_chartselectionY2max) + ", " + e.ChartArea.AxisY2.ValueToPosition(_chartselectionY2min));
            //    //Console.WriteLine("Zoom: " + (_chartselectionY2max) + ", " + (_chartselectionY2min));
            //}
        }

        private void chartVoltammogram_AxisViewChanging(object sender, System.Windows.Forms.DataVisualization.Charting.ViewEventArgs e)
        {
            //double Y2max = e.ChartArea.AxisY2.PositionToValue(e.ChartArea.AxisY.ValueToPosition(e.Axis.ScaleView.ViewMaximum));
            //double Y2min = e.ChartArea.AxisY2.PositionToValue(e.ChartArea.AxisY.ValueToPosition(e.Axis.ScaleView.ViewMinimum));

            //Console.WriteLine(e.Axis.AxisName);
            //Console.WriteLine(" X: " + e.Axis.ScaleView.ViewMaximum.ToString() + ", " + e.Axis.ScaleView.ViewMinimum.ToString());
            //Console.WriteLine("Y2: " + Y2max + ", " + Y2min);
        }

        private void chartVoltammogram_SelectionRangeChanged(object sender, System.Windows.Forms.DataVisualization.Charting.CursorEventArgs e)
        {
            Console.WriteLine("chartVoltammogram_SelectionRangeChanged: " + e.NewPosition);
            Console.WriteLine("    {0}", e.Axis.ToString());
            Console.WriteLine("    e.NewSelection: start {0}; end {1}", e.NewSelectionStart, e.NewSelectionEnd);
            Console.WriteLine("    e.NewSelection (%): start {0}; end {1}", e.Axis.ValueToPosition(e.NewSelectionStart), e.Axis.ValueToPosition(e.NewSelectionEnd));
            Console.WriteLine("    e.Axis.ScaleView.Size: {0}", e.Axis.ScaleView.Size);
            Console.WriteLine("    e.NewPosition: {0}", e.NewPosition);
            Console.WriteLine("    e.ChartArea.CursorX.Position: X {0}; Y {1}", e.ChartArea.CursorX.Position, e.ChartArea.CursorY.Position);

            if (chartVoltammogram.Series.Count == 0) return;
            if ((chartVoltammogram.Series[0]).Points.Count == 0) return;

            if (Math.Abs(e.Axis.ValueToPosition(e.NewSelectionStart) - e.Axis.ValueToPosition(e.NewSelectionEnd)) < 1)
            {
            }
            else
            {
                //Console.WriteLine("    Zooming");
                try
                {
                    System.Windows.Forms.DataVisualization.Charting.ChartArea ca = chartVoltammogram.ChartAreas[0];

                    if (e.Axis.ToString().StartsWith("Axis-X"))
                    {
                        double p = Math.Min(e.NewSelectionStart, e.ChartArea.CursorX.Position);
                        double size = Math.Abs(e.NewSelectionStart - e.NewSelectionEnd);
                        //p = 100000;
                        //size = 125000;
                        double order = Math.Truncate(Math.Log10(Math.Abs(size)));
                        double interval = Math.Pow(10, order - 1);
                        //double interval_m = RescaleTicksX(size, p, is_zooming: true);
                        //double new_size = Math.Ceiling(size / (interval_m)) * interval_m;
                        double new_p = Math.Round(p / (interval)) * (interval);
                                                
                        double p_x2;
                        double start_x2;
                        double end_x2;
                        double size_x2;
                        double order_x2;
                        double interval_y2;
                        double new_p_x2;
                        if(_selectedMode != modeMeasurement.eis)
                        {
                            p_x2 = ca.AxisX2.PositionToValue(ca.AxisX.ValueToPosition(p));
                            start_x2 = ca.AxisX2.PositionToValue(ca.AxisX.ValueToPosition(e.NewSelectionStart));
                            end_x2 = ca.AxisX2.PositionToValue(ca.AxisX.ValueToPosition(e.NewSelectionEnd));
                            size_x2 = Math.Abs(start_x2 - end_x2);
                            order_x2 = Math.Truncate(Math.Log10(Math.Abs(size_x2)));
                            interval_y2 = Math.Pow(10, order_x2 - 1);
                            new_p_x2 = Math.Round(p_x2 / (interval_y2)) * (interval_y2);
                        }
                        else
                        {
                            // X2は対数軸なので、対数値を指定する
                            p_x2 = ca.AxisX2.PositionToValue(ca.AxisX.ValueToPosition(p));
                            start_x2 = ca.AxisX2.PositionToValue(ca.AxisX.ValueToPosition(e.NewSelectionStart));
                            end_x2 = ca.AxisX2.PositionToValue(ca.AxisX.ValueToPosition(e.NewSelectionEnd));
                            size_x2 = Math.Abs(start_x2 - end_x2);
                            //order_x2 = Math.Truncate(Math.Abs(size_x2));
                            //interval_y2 = order_x2 - 1;
                            //new_p_x2 = Math.Round(p_x2 / (interval_y2)) * (interval_y2);
                            new_p_x2 = p_x2;
                        }

                        // 座標軸の計算が全て終わってから、Zoomを実行
                        ca.AxisX.ScaleView.Zoom(new_p, size, System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number, false);
                        ca.AxisX2.ScaleView.Zoom(new_p_x2, size_x2, System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number, false);

                        //Console.WriteLine("    After zooming: ViewMinimum {0}; size {1}; offset {2}; new_p {3}",
                        //    chartVoltammogram.ChartAreas[0].AxisX.ScaleView.ViewMinimum,
                        //    chartVoltammogram.ChartAreas[0].AxisX.ScaleView.Size,
                        //    chartVoltammogram.ChartAreas[0].AxisX.LabelStyle.IntervalOffset,
                        //    new_p);

                        // Chartコントロールのバグ？Zoom後に少しずれるのを補正する
                        double delta = ca.AxisX.ScaleView.ViewMinimum - new_p;
                        ca.AxisX.LabelStyle.IntervalOffset = -delta;
                        ca.AxisX.MajorTickMark.IntervalOffset = -delta;
                        ca.AxisX.MinorTickMark.IntervalOffset = -delta;
                        double delta_x2 = (ca.AxisX2.ScaleView.ViewMinimum - new_p_x2);
                        ca.AxisX2.LabelStyle.IntervalOffset = -delta_x2;
                        ca.AxisX2.MajorTickMark.IntervalOffset = -delta_x2;
                        ca.AxisX2.MinorTickMark.IntervalOffset = -delta_x2;

                        if (_selectedMode != modeMeasurement.eis)
                        {
                            // 軸ラベル位置をキリのよいところ＝主目盛り間隔で始まるようにする
                            //double delta2 = (Math.Ceiling(new_p / (interval_m)) * (interval_m)) - new_p;
                            //chartVoltammogram.ChartAreas[0].AxisX.LabelStyle.IntervalOffset += delta2;
                            //chartVoltammogram.ChartAreas[0].AxisX.MajorTickMark.IntervalOffset += delta2;
                            //chartVoltammogram.ChartAreas[0].AxisX.MinorTickMark.IntervalOffset += delta2;
                            //chartVoltammogram.ChartAreas[0].AxisX2.MajorTickMark.IntervalOffset += delta2;
                            //chartVoltammogram.ChartAreas[0].AxisX2.MinorTickMark.IntervalOffset += delta2;

                            // 副目盛りは、主目盛りよりもoffsetとしては小さくなるので (現状、副目盛り間隔は、主目盛りよりも5倍狭い) 
                            // Offsetは、グラフの左下が基準
                            //double delta3 = (new_p + delta2) - (Math.Ceiling(new_p / (interval_m / 5)) * (interval_m / 5));
                            //chartVoltammogram.ChartAreas[0].AxisX.MinorTickMark.IntervalOffset -= delta3;
                            //chartVoltammogram.ChartAreas[0].AxisX2.MinorTickMark.IntervalOffset -= delta3;
                        }
                        else
                        {
                            double delta2_x2 = Math.Ceiling(new_p_x2) - new_p_x2;
                            ca.AxisX2.LabelStyle.IntervalOffset += delta2_x2;
                            ca.AxisX2.MajorTickMark.IntervalOffset += delta2_x2;
                            ca.AxisX2.MinorTickMark.IntervalOffset += delta2_x2;
                            //double delta3_x2 = (Math.Ceiling(new_p_x2 / (1.0/10)) * (1.0/10)) - new_p_x2;
                            ca.AxisX2.MinorTickMark.IntervalOffset += -1;
                        }

                        // selectioの解除
                        double new_x = (e.NewSelectionStart + e.NewSelectionEnd) / 2;
                        e.ChartArea.CursorX.Position = new_x;
                        e.ChartArea.CursorX.SetSelectionPosition(new_x, new_x);

                        // selectionの範囲が狭い時には、zoomしないので、ここでselectionを解除しておく
                        if (Math.Abs(
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
                        //   2. 最小値と最大値に基づく目盛り間隔の更新
                        double p = Math.Min(e.NewSelectionStart, e.ChartArea.CursorY.Position);
                        double size = Math.Abs(e.NewSelectionStart - e.NewSelectionEnd);
                        //p = -100;
                        //size = 100;
                        double order = Math.Truncate(Math.Log10(Math.Abs(size)));
                        double interval = Math.Pow(10, order - 1);
                        //double interval_m = RescaleTicksY(size, p, is_zooming: true);
                        //double new_size = Math.Ceiling(size / (interval_m)) * interval_m;
                        double new_p = Math.Round(p / (interval)) * (interval);

                        // Y2は軸のスケールが違うので要変換
                        double p_y2 = ca.AxisY2.PositionToValue(ca.AxisY.ValueToPosition(p));
                        double start_y2 = ca.AxisY2.PositionToValue(ca.AxisY.ValueToPosition(e.NewSelectionStart));
                        double end_y2 = ca.AxisY2.PositionToValue(ca.AxisY.ValueToPosition(e.NewSelectionEnd));
                        double size_y2 = Math.Abs(start_y2 - end_y2);
                        double order_y2 = Math.Truncate(Math.Log10(Math.Abs(size_y2)));
                        double interval_y2 = Math.Pow(10, order_y2 - 1);
                        double new_p_y2 = Math.Round(p_y2 / (interval_y2)) * (interval_y2);
                        if (_selectedMode != modeMeasurement.eis)
                        {
                            p_y2 = ca.AxisY2.PositionToValue(ca.AxisY.ValueToPosition(p));
                            start_y2 = ca.AxisY2.PositionToValue(ca.AxisY.ValueToPosition(e.NewSelectionStart));
                            end_y2 = ca.AxisY2.PositionToValue(ca.AxisY.ValueToPosition(e.NewSelectionEnd));
                            size_y2 = Math.Abs(start_y2 - end_y2);
                            order_y2 = Math.Truncate(Math.Log10(Math.Abs(size_y2)));
                            interval_y2 = Math.Pow(10, order_y2 - 1);
                            new_p_y2 = Math.Round(p_y2 / (interval_y2)) * (interval_y2);
                        }
                        else
                        {
                            p_y2 = ca.AxisY2.PositionToValue(ca.AxisY.ValueToPosition(p));
                            start_y2 = ca.AxisY2.PositionToValue(ca.AxisY.ValueToPosition(e.NewSelectionStart));
                            end_y2 = ca.AxisY2.PositionToValue(ca.AxisY.ValueToPosition(e.NewSelectionEnd));
                            size_y2 = Math.Abs(start_y2 - end_y2);
                            //order_y2 = Math.Truncate(Math.Log10(Math.Abs(size_y2)));
                            //interval_y2 = Math.Pow(10, order_y2 - 1);
                            new_p_y2 = p_y2; // Math.Round(p_y2 / (interval_y2)) * (interval_y2);
                        }

                        // 座標軸の計算が全て終わってから、Zoomを実行
                        ca.AxisY.ScaleView.Zoom(new_p, size, System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number, false);
                        ca.AxisY2.ScaleView.Zoom(new_p_y2, size_y2, System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number, false);

                        //Console.WriteLine("    After zooming: ViewMinimum {0}; (ScaleView)Size {1}; IntervalOffset {2}; new_p {3}; size {4}",
                        //    chartVoltammogram.ChartAreas[0].AxisY.ScaleView.ViewMinimum - chartVoltammogram.ChartAreas[0].AxisY.ScaleView.ViewMaximum,
                        //    chartVoltammogram.ChartAreas[0].AxisY.ScaleView.Size,
                        //    chartVoltammogram.ChartAreas[0].AxisY.LabelStyle.IntervalOffset,
                        //    new_p, size);

                        double delta = (ca.AxisY.ScaleView.ViewMinimum - new_p);
                        ca.AxisY.LabelStyle.IntervalOffset = -delta;
                        ca.AxisY.MajorTickMark.IntervalOffset = -delta;
                        ca.AxisY.MinorTickMark.IntervalOffset = -delta;
                        double delta_y2 = (ca.AxisY2.ScaleView.ViewMinimum - new_p_y2);
                        ca.AxisY2.LabelStyle.IntervalOffset = -delta_y2;
                        ca.AxisY2.MajorTickMark.IntervalOffset = -delta_y2;
                        ca.AxisY2.MinorTickMark.IntervalOffset = -delta_y2;

                        if (_selectedMode != modeMeasurement.eis)
                        {
                            // 軸ラベル位置をキリのよいところ＝主目盛り間隔で始まるようにする
                            double delta2 = (AbsCeiling(new_p / (ca.AxisY.MajorTickMark.Interval)) * (ca.AxisY.MajorTickMark.Interval)) - new_p;
                            ca.AxisY.LabelStyle.IntervalOffset += delta2;
                            ca.AxisY.MajorTickMark.IntervalOffset += delta2;
                            ca.AxisY.MinorTickMark.IntervalOffset += delta2;
                            double delta2_y2 = (AbsCeiling(new_p_y2 / (ca.AxisY2.MajorTickMark.Interval)) * (ca.AxisY2.MajorTickMark.Interval)) - new_p_y2;
                            ca.AxisY2.LabelStyle.IntervalOffset += delta2_y2;
                            ca.AxisY2.MajorTickMark.IntervalOffset += delta2_y2;
                            ca.AxisY2.MinorTickMark.IntervalOffset += delta2_y2;

                            // 副目盛りは、主目盛りよりもoffsetとしては小さくなるので (現状、副目盛り間隔は、主目盛りよりも2倍狭い) 
                            // Offsetは、グラフの左下が基準
                            //double delta3 = (new_p + delta2) - (Math.Ceiling(new_p / (interval_m / 2)) * (interval_m / 2));
                            //chartVoltammogram.ChartAreas[0].AxisY.MinorTickMark.IntervalOffset -= delta3;
                            //chartVoltammogram.ChartAreas[0].AxisY2.MinorTickMark.IntervalOffset -= delta3;
                        }
                        else
                        {
                            double delta2_y2 = Math.Ceiling(new_p_y2) - new_p_y2;
                            ca.AxisY2.LabelStyle.IntervalOffset += delta2_y2;
                            ca.AxisY2.MajorTickMark.IntervalOffset += delta2_y2;
                            ca.AxisY2.MinorTickMark.IntervalOffset += delta2_y2;
                            //double delta3_y2 = (Math.Ceiling(new_p_y2 / (1.0 / 10)) * (1.0 / 10)) - new_p_y2;
                            ca.AxisY2.MinorTickMark.IntervalOffset += -1;// delta3_y2;
                        }
                            
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
                catch (System.Exception error)
                {
                    Console.WriteLine("Exception: {0}", error.Message);
                }
            }
        }

        private void chartVoltammogram_SelectionRangeChanging(object sender, System.Windows.Forms.DataVisualization.Charting.CursorEventArgs e)
        {
            //return;

            //if(e.Axis.AxisName.ToString() == "Y")
            //{
            //    //Console.WriteLine("Changing:");
            //    //Console.WriteLine(e.Axis.AxisName + ": " + e.NewSelectionStart.ToString());
            //    //Console.WriteLine(e.Axis.AxisName + ": " + e.NewSelectionEnd.ToString());

            //    //_chartselectionY2max = e.ChartArea.AxisY.ValueToPosition(e.NewSelectionStart);
            //    //_chartselectionY2min = e.ChartArea.AxisY.ValueToPosition(e.NewSelectionEnd);
            //    _chartselectionY2max = e.ChartArea.AxisY2.PositionToValue(e.ChartArea.AxisY.ValueToPosition(e.NewSelectionStart));
            //    _chartselectionY2min = e.ChartArea.AxisY2.PositionToValue(e.ChartArea.AxisY.ValueToPosition(e.NewSelectionEnd));

            //    //Console.WriteLine("Ymax: " + _chartselectionY2max + ", Ymin:" + _chartselectionY2min);

            //    _chartselectionTuringOn = true;
            //}
        }
        
        private void chartVoltammogram_CursorPositionChanged(object sender, System.Windows.Forms.DataVisualization.Charting.CursorEventArgs e)
        {
            try
            {
                double X = e.ChartArea.CursorX.Position;
                double Y1 = e.ChartArea.CursorY.Position;
                double X2 = Math.Pow(10, e.ChartArea.AxisX2.PositionToValue(e.ChartArea.AxisX.ValueToPosition(e.ChartArea.CursorX.Position)));
                double Y2 = e.ChartArea.AxisY2.PositionToValue(e.ChartArea.AxisY.ValueToPosition(e.ChartArea.CursorY.Position));// / 1000 * ((long)_selectedRange / 1000);
                double F1 = 0.0;

                Console.WriteLine($"x1, y1, f1, x2, y2: {X}, {Y1}, {F1}, {X2}, {Y2}");

                string XY = "";
                switch (_selectedMode)
                {
                    case modeMeasurement.eis:
                        for (int i = 0; i < chartVoltammogram.Series[1].Points.Count; i++)
                        {
                            if (chartVoltammogram.Series[1].Points[i].XValue < X)
                            {
                                if (i > 0)
                                {
                                    // Bode plot: chartVoltammogram.Series[6].Points
                                    // Cole-Cole plot: chartVoltammogram.Series[1].Points

                                    // indexがiとi-1のデータ間にXがある

                                    double F1_1 = chartVoltammogram.Series[6].Points[i].XValue;
                                    double F1_0 = chartVoltammogram.Series[6].Points[i - 1].XValue;
                                    double X1_1 = chartVoltammogram.Series[1].Points[i].XValue;
                                    double X1_0 = chartVoltammogram.Series[1].Points[i - 1].XValue;

                                    F1 = (F1_1 - F1_0) / (X1_1 - X1_0) * (X - X1_0) + F1_0;

                                    break;
                                }
                            }
                        }
                        //XY = "(" + X + " ohm, " + Y1 + " ohm, " + Math.Sqrt(Math.Pow(X,2) + Math.Pow(Y1, 2)).ToString("0") + " ohm)";
                        XY = "[(" + X + " ohm, " + Y1 + " ohm, " + F1.ToString("0.0") + " Hz), (" + X2.ToString("0.0") + " Hz, " + Math.Pow(10, Y2).ToString("0.0") + " ohm)]";
                        break;
                    default:
                        XY = "(" + X + " s, " + Y1 + " mV, " + Y2.ToString("0.000") + " uA)";
                        break;
                }

                toolStripStatusCursor.Text = XY;
            }
            catch (System.Exception error)
            {
                Console.WriteLine("Exception: {0}", error.Message);
            }
        }

        #region Functions for digital I/O pins from AnalogDiscovery 2
        //
        // Functions for digital I/O pins from AnalogDiscovery 2
        //

        public void SetPurging(bool on)
        {
            uint dwRead = 0b0000000000000000;
            // fetch digital IO information from the device
            FDwfDigitalIOStatus(_handle);
            // read state of all pins, regardless of output enable
            FDwfDigitalIOInputStatus(_handle,  out dwRead);

            // set value on enabled IO pins
            FDwfDigitalIOOutputSet(_handle, 0b0000000000000000);

            if (VERSION_0_5)
            {
                if (on)
                {
                    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b0111111111111111) ^ 0b1000000000000000);
                }
                else
                {
                    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b0111111111111111) ^ 0b0000000000000000);
                }
            }
            else if (VERSION_0_9c)
            {
                if (on)
                {
                    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b0111111111111111) ^ 0b1000000000000000);
                }
                else
                {
                    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b0111111111111111) ^ 0b0000000000000000);
                }
            }
            else if (VERSION_0_9d || VERSION_0_9e || VERSION_0_9k)
            {
                if (on)
                {
                    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b0111111111111111) ^ 0b1000000000000000);
                }
                else
                {
                    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b0111111111111111) ^ 0b0000000000000000);
                }
            }
            else
            {
                if (on)
                {
                    // FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111111111011) ^ 0b0000000000000100); // for Electro-lab v0.1
                    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111101111111) ^ 0b0000000010000000); // for Electro-lab v0.2
                }
                else
                {
                    //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111111111011) ^ 0b0000000000000000); // for Electro-lab v0.1
                    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111101111111) ^ 0b0000000000000000); // for Electro-lab v0.2
                }
            }

            BeginInvoke((Action)delegate ()  
            {
                toolStripMenuItemSubModuleOption1.Checked = on;
            });
        }

        //public void StopPurging()
        //{
        //    uint dwRead = 0b0000000000000000;
        //    // fetch digital IO information from the device
        //    FDwfDigitalIOStatus(_handle);
        //    // read state of all pins, regardless of output enable
        //    FDwfDigitalIOInputStatus(_handle, ref dwRead);

        //    // set value on enabled IO pins
        //    FDwfDigitalIOOutputSet(_handle, 0b0000000000000000);

        //    if (VERSION_0_5)
        //    {
        //        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b0111111111111111) ^ 0b0000000000000000);
        //    }
        //    else if (VERSION_0_9)
        //    {
        //    }
        //    else
        //    {
        //        //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111111111011) ^ 0b0000000000000000); // for Electro-lab v0.1
        //        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111101111111) ^ 0b0000000000000000); // for Electro-lab v0.2
        //    }

        //    //this.Dispatcher.Invoke((Action)(() =>
        //    //{
        //    //    CtrlText.Text = "進捗：" + i.ToString() + "%";
        //    //}));

        //    Invoke((Action)delegate ()
        //    {
        //        toolStripMenuItemPurge.Checked = false;
        //    });

        //    //toolStripMenuItemPurge.Checked = false;
        //}

        public void SetRotation(uint rpm)
        {
            SetDCVoltageCH2(Convert.ToInt32(rpm * 1000));
            
            Invoke((Action)delegate ()
            {
                if (rpm > 0)
                {
                    toolStripMenuItemSubModuleOption3.Checked = true;
                    _rpmRDE = rpm;
                }
                else
                {
                    toolStripMenuItemSubModuleOption3.Checked = false;
                    _rpmRDE = 0;
                }
            });
        }

        //public void OpenCircuit()
        //{
        //    uint dwRead = 0b0000000000000000;
        //    // fetch digital IO information from the device
        //    FDwfDigitalIOStatus(_handle);
        //    // read state of all pins, regardless of output enable
        //    FDwfDigitalIOInputStatus(_handle, ref dwRead);

        //    FDwfDigitalIOOutputSet(_handle, 0b0000000000000000);

        //    if (VERSION_0_5)
        //    {
        //        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111110111111101) ^ 0b0000000000000010);
        //        Thread.Sleep(100);
        //        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111110111111101) ^ 0b0000000000000000);
        //    }
        //    else if (VERSION_0_9)
        //    {
        //    }
        //    else
        //    {
        //        //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111110111111101) ^ 0b0000000000000010);  // for Electro-lab v0.1
        //        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111111111100) ^ 0b0000000000000001);  // for Electro-lab v0.2
        //        // set value on enabled IO pins
        //        //FDwfDigitalIOOutputSet(_handle, 0b0000000000000010);
        //    }

        //    Invoke((Action)delegate ()
        //    {
        //        toolStripMenuItemOCV.Checked = true;
        //    });
        //}

        public void SetCircuit(bool open)
        {
            uint dwRead = 0b0000000000000000;
            // fetch digital IO information from the device
            FDwfDigitalIOStatus(_handle);
            // read state of all pins, regardless of output enable
            FDwfDigitalIOInputStatus(_handle, out dwRead);

            FDwfDigitalIOOutputSet(_handle, 0b0000000000000000);

            if(VERSION_0_5)
            {
                if(open)
                {
                    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111110111111101) ^ 0b0000000000000010);
                }
                else
                {
                    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111110111111101) ^ 0b0000001000000000);
                }
                    Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111110111111101) ^ 0b0000000000000000);
            }
            else if (VERSION_0_9c)
            {
                if (open)
                {
                    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111110011111100) ^ 0b0000001100000000);
                }
                else
                {
                    Invoke((Action)delegate ()
                    {
                        if (!toolStripMenuGalvanoStat.Checked)
                        {
                            FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111110011111100) ^ 0b0000000000000011);
                        }
                        else
                        {
                            open = true;
                        }
                    });
                }
                    Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111110011111100) ^ 0b0000000000000000);
            }
            else if (VERSION_0_9d)
            {
                if (open)
                {
                    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111011111110) ^ 0b0000000100000000);
                }
                else
                {
                    Invoke((Action)delegate ()
                    {
                        if (!toolStripMenuGalvanoStat.Checked)
                        {
                            FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111011111110) ^ 0b0000000000000001);
                        }
                        else
                        {
                            open = true;
                        }
                    });
                }
                    Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111011111110) ^ 0b0000000000000000);
            }
            else if (VERSION_0_9e)
            {
                if (open)
                {
                    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111101011111010) ^ 0b0000010000000001);// 6/29/2021, 0 -> 1 @ 1st bit (for OCP measurement)
                }
                else
                {
                    Invoke((Action)delegate ()
                    {
                        if (!toolStripMenuGalvanoStat.Checked)
                        {
                            if (toolStripMenuEIS.Checked)
                            {
                                FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111101011111010) ^ 0b0000000000000101);
                            }
                            else
                            {
                                FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111101011111010) ^ 0b0000000100000100);
                            }
                        }
                        else
                        {
                            open = true;
                        }
                    });
                }
                Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111101011111010) ^ 0b0000000000000000);
            }
            else if (VERSION_0_9k) // DONE
            {
                if (open)
                {
                    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111111111010) ^ 0b0000000000000100); // 6/29/2021, (n/a) -> 0 @ 1 bit (for OCP measurement)
                }
                else
                {
                    Invoke((Action)delegate ()
                    {
                        if (!toolStripMenuGalvanoStat.Checked)// && !)
                        {
                            if(toolStripMenuEIS.Checked)
                            {
                                FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111111111011) ^ 0b0000000000000000);
                            }
                            else
                            {
                                FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111111111010) ^ 0b0000000000000001); // 6/29/2021, 0 -> 1 @ 1st bit (for OCP measurement)
                            }
                        }
                        else
                        {
                            FDwfDigitalIOOutputSet(_handle, dwRead); // Digital I/Oの状態を元の状態に戻す
                            open = true;
                        }
                    });
                }
            }
            else
            {
                if (open)
                {
                    //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111110111111101) ^ 0b0000000000000010);  // for Electro-lab v0.1
                    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111111111100) ^ 0b0000000000000001);  // for Electro-lab v0.2
                    // set value on enabled IO pins
                    //FDwfDigitalIOOutputSet(_handle, 0b0000000000000010);
                }
                else
                {
                    //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111110111111101) ^ 0b0000001000000000); // for Electro-lab v0.1
                    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111111111100) ^ 0b0000000000000010); // for Electro-lab v0.2
                    // set value on enabled IO pins
                    //FDwfDigitalIOOutputSet(_handle, 0b0000001000000000);
                }
            }

            Invoke((Action)delegate ()
            {
                toolStripMenuItemOCV.Checked = open;
            });
        }

        public void SelectPotentialRange(Potentiostat.rangePotential range)
        {
            Console.WriteLine("Set Channel(1 & 2) Range: " + ((double)range/1000).ToString());

            FDwfAnalogInChannelOffsetSet(_handle, (CHANNEL_POTENTIAL - 1), 0.0);
            if(range == rangePotential.Range50V && toolStripMenuItemRange25VonlyforCurrent.Checked)
            {
                FDwfAnalogInChannelRangeSet(_handle, (CHANNEL_POTENTIAL - 1), ((double)5000 / 1000)); // +-5Vレンジに設定
            }
            else
            {
                FDwfAnalogInChannelRangeSet(_handle, (CHANNEL_POTENTIAL - 1), ((double)range / 1000)); // +-5Vレンジに設定
            }
            FDwfAnalogInChannelOffsetSet(_handle, (CHANNEL_CURRENT - 1), 0.0);
            FDwfAnalogInChannelRangeSet(_handle, (CHANNEL_CURRENT - 1), ((double)range / 1000)); // +-0.5Vレンジに設定


            double actual = 0.0;
            FDwfAnalogInChannelRangeGet(_handle, (CHANNEL_POTENTIAL - 1), out actual);
            Console.WriteLine("Actual Channel(0) Range: " + actual.ToString());
            FDwfAnalogInChannelRangeGet(_handle, (CHANNEL_CURRENT - 1), out actual);
            Console.WriteLine("Actual Channel(1) Range: " + actual.ToString());
            _voltAnalogInChannelRange_Current = actual;


            for (int i = 0; i < _toolstripmenuitemsRange.Length; i++)
            {
                int idx = int.Parse(_toolstripmenuitemsRange[i].Tag.ToString());

                if ((rangePotential)idx == range)
                {
                    _toolstripmenuitemsRange[i].Checked = true;
                }
                else
                {
                    _toolstripmenuitemsRange[i].Checked = false;
                }
            }

            _selectedPotentialRange = range;
        }

        public void SelectCurrentRange(Potentiostat.rangeCurrent range)
        {
            uint dwRead = 0b0000000000000000;
            // fetch digital IO information from the device
            FDwfDigitalIOStatus(_handle);
            // read state of all pins, regardless of output enable
            FDwfDigitalIOInputStatus(_handle, out dwRead);

            FDwfDigitalIOOutputSet(_handle, 0b0000000000000000);

            if (VERSION_0_5)
            {
                switch (range)
                {
                    case Potentiostat.rangeCurrent.Range20mA:
                        // 4+12
                        ////////////////////////////////////////////   CBA     CBA           CBA     CBA
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1110001111100011) ^ 0b0001100000000100);
                        //// 6+14
                        //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1011111110111111) ^ 0b0100000001000000);
                        //Thread.Sleep(100);
                        //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1011111110111111) ^ 0b0000000000000000);
                        break;

                    case Potentiostat.rangeCurrent.Range2mA:
                        // 4+12
                        ////////////////////////////////////////////   CBA     CBA           CBA     CBA
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1110001111100011) ^ 0b0001000000001100);
                        //// 5+13
                        //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1101111111011111) ^ 0b0010000000100000);
                        //Thread.Sleep(100);
                        //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1101111111011111) ^ 0b0000000000000000);
                        break;

                    case Potentiostat.rangeCurrent.Range200uA:
                        // 3+11
                        ////////////////////////////////////////////   CBA     CBA           CBA     CBA
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1110001111100011) ^ 0b0000000000011100);
                        //// 5+13
                        //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1101111111011111) ^ 0b0010000000100000);
                        //Thread.Sleep(100);
                        //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1101111111011111) ^ 0b0000000000000000);
                        break;

                    case Potentiostat.rangeCurrent.Range20uA:
                        // 3+11
                        ////////////////////////////////////////////   CBA     CBA           CBA     CBA
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1110001111100011) ^ 0b0000110000000000);
                        //// 6+14
                        //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1011111110111111) ^ 0b0100000001000000);
                        //Thread.Sleep(100);
                        //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1011111110111111) ^ 0b0000000000000000);
                        break;

                    case Potentiostat.rangeCurrent.Range2uA:
                        // 3+11
                        ////////////////////////////////////////////   CBA     CBA           CBA     CBA
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1110001111100011) ^ 0b0000010000001000);
                        //// 6+14
                        //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1011111110111111) ^ 0b0100000001000000);
                        //Thread.Sleep(100);
                        //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1011111110111111) ^ 0b0000000000000000);
                        break;

                    default: // Potentiostat.rangeMeasurement.RangeRAW:
                        ////////////////////////////////////////////   CBA     CBA           CBA     CBA
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1110001111100011) ^ 0b0000000000011100);
                        break;
                }
                        Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1110001111100011) ^ 0b0000000000000000);
            }
            else if (VERSION_0_9c)
            {
                switch (range)
                {
                    case Potentiostat.rangeCurrent.Range200mA:
                        ////////////////////////////////////////////  CBA     CBA           CBA     CBA
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1100011111000111) ^ 0b0001000000101000); break;

                    case Potentiostat.rangeCurrent.Range20mA:
                        ////////////////////////////////////////////  CBA     CBA           CBA     CBA
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1100011111000111) ^ 0b0011000000001000); break;

                    case Potentiostat.rangeCurrent.Range2mA:
                        ////////////////////////////////////////////  CBA     CBA           CBA     CBA
                      //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1100011111000111) ^ 0b0010000000011000); break;
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1100011111000111) ^ 0b0010000000000000); Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1100011111000111) ^ 0b0000000000010000); Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1100011111000111) ^ 0b0000000000001000);
                        break;

                    case Potentiostat.rangeCurrent.Range200uA:
                        ////////////////////////////////////////////  CBA     CBA           CBA     CBA
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1100011111000111) ^ 0b0000000000111000); break;

                    case Potentiostat.rangeCurrent.Range20uA:
                        ////////////////////////////////////////////  CBA     CBA           CBA     CBA
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1100011111000111) ^ 0b0001100000000000); break;

                    case Potentiostat.rangeCurrent.Range2uA:
                        ////////////////////////////////////////////  CBA     CBA           CBA     CBA
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1100011111000111) ^ 0b0000100000010000); break;

                    default: // Potentiostat.rangeMeasurement.RangeRAW:
                        ////////////////////////////////////////////  CBA     CBA           CBA     CBA
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1100011111000111) ^ 0b0000000000111000); break;
                }
                        Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1100011111000111) ^ 0b0000000000000000);
            }
            else if (VERSION_0_9d || VERSION_0_9e)
            {
                switch (range)
                {
                    case Potentiostat.rangeCurrent.Range200mA:
                        //////////////////////////////////////////// CBA     CBA           CBA     CBA
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1000111110001111) ^ 0b0010000001010000); break;

                    case Potentiostat.rangeCurrent.Range20mA:
                        //////////////////////////////////////////// CBA     CBA           CBA     CBA
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1000111110001111) ^ 0b0110000000010000); break;

                    case Potentiostat.rangeCurrent.Range2mA:
                        //////////////////////////////////////////// CBA     CBA           CBA     CBA
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1000111110001111) ^ 0b0100000000110000); break;

                    case Potentiostat.rangeCurrent.Range200uA:
                        //////////////////////////////////////////// CBA     CBA           CBA     CBA
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1000111110001111) ^ 0b0000000001110000); break;

                    case Potentiostat.rangeCurrent.Range20uA:
                        //////////////////////////////////////////// CBA     CBA           CBA     CBA
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1000111110001111) ^ 0b0011000000000000); break;

                    case Potentiostat.rangeCurrent.Range2uA:
                        //////////////////////////////////////////// CBA     CBA           CBA     CBA
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1000111110001111) ^ 0b0001000000100000); break;

                    default: // Potentiostat.rangeMeasurement.RangeRAW:
                        //////////////////////////////////////////// CBA     CBA           CBA     CBA
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1000111110001111) ^ 0b0000000001110000); break;
                }
                        Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1000111110001111) ^ 0b0000000000000000);
            }
            else if (VERSION_0_9k)
            {
                switch (range) // C1とC2は無いので、強制的にC3扱いにする
                {
                    //case Potentiostat.rangeCurrent.Range200mA:
                    //    //////////////////////////////////////////// CBA     CBA           CBA     CBA
                    //    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1000111110001111) ^ 0b0010000001010000); break;

                    //case Potentiostat.rangeCurrent.Range20mA:
                    //    //////////////////////////////////////////// CBA     CBA           CBA     CBA
                    //    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1000111110001111) ^ 0b0110000000010000); break;

                    case Potentiostat.rangeCurrent.Range2mA:
                        ////////////////////////////////////////////          BA                    BA
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111111001111) ^ 0b0000000000100000); break;

                    case Potentiostat.rangeCurrent.Range200uA:
                        ////////////////////////////////////////////          BA                    BA
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111111001111) ^ 0b0000000000000000); break;

                    case Potentiostat.rangeCurrent.Range20uA:
                        ////////////////////////////////////////////          BA                    BA
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111111001111) ^ 0b0000000000110000); break;

                    case Potentiostat.rangeCurrent.Range2uA:
                        ////////////////////////////////////////////          BA                    BA
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111111001111) ^ 0b0000000000010000); break;

                    default: // Potentiostat.rangeMeasurement.RangeRAW; .Range200mA; .Range20mA (same as the Range200uA-case)
                        ////////////////////////////////////////////          BA                    BA
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111111001111) ^ 0b0000000000000000); break;
                }
            }
            else
            {
                switch (range)
                {
                    case Potentiostat.rangeCurrent.Range20mA:
                        //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111101011111110) ^ 0b0000000100000000);  // for Electro-lab v0.1
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111111000011) ^ 0b0000000000010000); // for Electro-lab v0.2
                        // set value on enabled IO pins
                        //FDwfDigitalIOOutputSet(_handle, 0b0000000100000000);
                        break;

                    case Potentiostat.rangeCurrent.Range2mA:
                        //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111101011111110) ^ 0b0000000000000001);  // for Electro-lab v0.1
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111111000011) ^ 0b0000000000001000); // for Electro-lab v0.2
                        // set value on enabled IO pins
                        //FDwfDigitalIOOutputSet(_handle, 0b0000000100000000);
                        break;

                    case Potentiostat.rangeCurrent.Range200uA:
                        //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111101011111110) ^ 0b0000010000000000);  // for Electro-lab v0.1
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111111000011) ^ 0b0000000000000100); // for Electro-lab v0.2
                        // set value on enabled IO pins
                        //FDwfDigitalIOOutputSet(_handle, 0b0000000000000001);
                        break;

                    default:
                        //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111101011111110) ^ 0b0000010000000000);  // for Electro-lab v0.1
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111111000011) ^ 0b0000000000000100); // for Electro-lab v0.2
                        // set value on enabled IO pins
                        //FDwfDigitalIOOutputSet(_handle, 0b0000000000000001);
                        break;
                }
            }
        }

        public bool TurnPowerSupply(bool on)
        {
            if(on)
            {
                // set up analog IO channel nodes
                // enable positive supply
                FDwfAnalogIOChannelNodeSet(_handle, 0, 0, 1);
                // set positive voltage
                FDwfAnalogIOChannelNodeSet(_handle, 0, 1, 5.0);
                // enable negative supply
                FDwfAnalogIOChannelNodeSet(_handle, 1, 0, 1);
                FDwfAnalogIOChannelNodeSet(_handle, 1, 1, -5.0);
                // master enable
                FDwfAnalogIOEnableSet(_handle, Convert.ToInt32(true));

                Thread.Sleep(500);

                if(VERSION_0_9d || VERSION_0_9e || VERSION_0_9k)
                {
                    FDwfDigitalIOOutputSet(_handle, 0b0000000010000000);
                }
            }
            else
            {
                // set up analog IO channel nodes
                // enable positive supply
                FDwfAnalogIOChannelNodeSet(_handle, 0, 0, 0);
                // enable negative supply
                FDwfAnalogIOChannelNodeSet(_handle, 1, 0, 0);
                // master enable
                FDwfAnalogIOEnableSet(_handle, Convert.ToInt32(false));
            }

            Thread.Sleep(1000);

            FDwfAnalogIOStatus(_handle);
            int sts = 0;
            FDwfAnalogIOEnableStatus(_handle, out sts);

            return (sts == 1);
        }

        public void SetGalvanostat(bool on)
        {
            if (VERSION_0_5 || VERSION_0_9c || VERSION_0_9d || VERSION_0_9e || VERSION_0_9k)
            {
                uint dwRead = 0b0000000000000000;
                // fetch digital IO information from the device
                FDwfDigitalIOStatus(_handle);
                // read state of all pins, regardless of output enable
                FDwfDigitalIOInputStatus(_handle, out dwRead);

                FDwfDigitalIOOutputSet(_handle, 0b0000000000000000);

                if (VERSION_0_5)
                {
                    if (on)
                    {
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111011111110) ^ 0b0000000100000000);
                    }
                    else
                    {
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111011111110) ^ 0b0000000000000001);
                    }
                        Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111011111110) ^ 0b0000000000000000);
                }
                else if (VERSION_0_9c)
                {
                    if (on)
                    {
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111101111111011) ^ 0b0000010000000000);
                    }
                    else
                    {
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111101111111011) ^ 0b0000000000000100);
                    }
                        Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111101111111011) ^ 0b0000000000000000);
                }
                else if (VERSION_0_9d)
                {
                    if (on)
                    {                                                                  // 1111000111110001
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111000111110001) ^ 0b0000101000000100);
                        //SetEIS(true);
                    }
                    else
                    {
                        //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111010111110101) ^ 0b0000000000001010);
                        Invoke((Action)delegate ()
                        {
                            if (!toolStripMenuEIS.Checked)
                            {                                                                // 0b1111000111110001
                                FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111000111110001) ^ 0b0000010000001010);
                            }
                            else
                            {                                                                // 0b1111010111110101
                                FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111010111110101) ^ 0b0000000000001010);
                            }
                        });
                    }
                        Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111010111110101) ^ 0b0000000000000000);
                }
                else if (VERSION_0_9e)
                {
                    if (on)
                    {
                        //// 強制的にEISはoffにする
                        //Invoke((Action)delegate ()
                        //{
                        //    toolStripMenuEIS.Checked = false;
                        //});

                        //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111000011110000) ^ 0b0000111000000001);
                          FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111000011110000) ^ 0b0000110000000000);
                          Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                          FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111000011110000) ^ 0b0000000000000000);

                          FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111000011110000) ^ 0b0000001000000001);
                          Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                          FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111000011110000) ^ 0b0000000000000000);
                    }
                    else
                    {
                        //Invoke((Action)delegate ()
                        //{
                        //    if (!toolStripMenuEIS.Checked) // non-EIS mode
                        //    {
                        //        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111000011110000) ^ 0b0000000100001010);
                        //    }
                        //    //else  // EIS mode
                        //    //{
                        //    //    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111000011110000) ^ 0b0000000000001011);
                        //    //    //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111000011110000) ^ 0b0000000000001010);
                        //    //    //Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                        //    //    //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111000011110000) ^ 0b0000000000000000);

                        //    //    //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111000011110000) ^ 0b0000000000000001);
                        //    //    //Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                        //    //    //FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111000011110000) ^ 0b0000000000000000);
                        //    //}
                        //});
                    }
                    Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111000011110000) ^ 0b0000000000000000);
                }
                else if (VERSION_0_9k) // DONE
                {
                    if (on)
                    {
                        // 強制的にEISはoffにする
                        Invoke((Action)delegate ()
                        {
                            toolStripMenuEIS.Checked = false;
                        });

                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111111110000) ^ 0b0000000000001110);
                    }
                    else
                    {
                        Invoke((Action)delegate ()
                        {
                            if (!toolStripMenuEIS.Checked) // non-EIS mode
                            {
                                FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111111110100) ^ 0b0000000000000001);
                            }
                        });
                    }
                }

                if (false)
                {
                    _selectedMode_previous = _selectedMode;
                    if (on)
                    {
                        _selectedMode = modeMeasurement.galvanometry;
                    }
                    else
                    {
                        _selectedMode = modeMeasurement.voltammetry;
                    }

                    Invoke((Action)delegate ()
                    {
                        toolStripMenuGalvanoStat.Checked = on;

                        toolStripComboBoxMethod.ComboBox.DataSource = null;
                        toolStripComboBoxMethod.ComboBox.DataSource = _tablesMethods[(int)_selectedMode];
                        toolStripComboBoxMethod.ComboBox.DisplayMember = "name";
                        toolStripComboBoxMethod.ComboBox.ValueMember = "index";

                        toolStripComboBoxRange.ComboBox.DataSource = null;
                        toolStripComboBoxRange.ComboBox.DataSource = _tablesRanges[(int)_selectedMode];
                        toolStripComboBoxRange.ComboBox.DisplayMember = "name";
                        toolStripComboBoxRange.ComboBox.ValueMember = "index";

                        if (on)
                        {
                            toolStripComboBoxMethod.SelectedIndex = 1;
                        }
                        else
                        {
                            toolStripComboBoxMethod.SelectedIndex = 0;
                        }
                    });
                }

                //updateComboBoxMethod();
            }
        }

        public void SetPotentiostat(bool on)
        {
            if (VERSION_0_5 || VERSION_0_9c || VERSION_0_9d || VERSION_0_9e || VERSION_0_9k)
            {
                uint dwRead = 0b0000000000000000;
                // fetch digital IO information from the device
                FDwfDigitalIOStatus(_handle);
                // read state of all pins, regardless of output enable
                FDwfDigitalIOInputStatus(_handle, out dwRead);

                FDwfDigitalIOOutputSet(_handle, 0b0000000000000000);

                if (VERSION_0_9e)
                {
                    if (on)
                    {
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111010011110100) ^ 0b0000000100001010);
                    }
                    else
                    {
                    }
                    Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111010011110100) ^ 0b0000000000000000);
                }
            }
        }

        public void SetEIS(bool on)
        {
            if (VERSION_0_5 || VERSION_0_9c || VERSION_0_9d || VERSION_0_9e || VERSION_0_9k)
            {
                uint dwRead = 0b0000000000000000;
                // fetch digital IO information from the device
                FDwfDigitalIOStatus(_handle);
                // read state of all pins, regardless of output enable
                FDwfDigitalIOInputStatus(_handle, out dwRead);

                FDwfDigitalIOOutputSet(_handle, 0b0000000000000000);

                if (VERSION_0_5)
                {
                    if (on)
                    {
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1101111111011111) ^ 0b0000000000100000);
                    }
                    else
                    {
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1101111111011111) ^ 0b0010000000000000);
                    }
                        Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1101111111011111) ^ 0b0000000000000000);
                }
                else if (VERSION_0_9c)
                {
                    if (on)
                    {
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1011111110111111) ^ 0b0000000001000000);
                    }
                    else
                    {
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1011111110111111) ^ 0b0100000000000000);
                    }
                        Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1011111110111111) ^ 0b0000000000000000);
                }
                else if (VERSION_0_9d)
                {
                    if (on)
                    {
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111101111111011) ^ 0b0000000000000100);
                    }
                    else
                    {
                        Invoke((Action)delegate ()
                        {
                            if (!toolStripMenuGalvanoStat.Checked)
                            {
                                FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111101111111011) ^ 0b0000010000000000);
                            }
                            else
                            {
                                on = true;
                            }
                        });
                    }
                        Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111101111111011) ^ 0b0000000000000000);
                }
                else if (VERSION_0_9e)
                {
                    if (on)
                    {
                        //// 強制的にG/Sはoffにする
                        //Invoke((Action)delegate ()
                        //{
                        //    toolStripMenuGalvanoStat.Checked = false;
                        //});

                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111010011110100) ^ 0b0000000000001011);
                    }
                    else
                    {
                        //Invoke((Action)delegate ()
                        //{
                        //    if (!toolStripMenuGalvanoStat.Checked) // Potentiostat mode, non-EIS mode
                        //    {
                        //        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111010011110100) ^ 0b0000000100001010);
                        //    }
                        //    //else // Galvanostat mode
                        //    //{
                        //    //    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111110011111100) ^ 0b0000001000000001);
                        //    //}
                        //});
                    }
                    Thread.Sleep(DELAY_TIME_SWITCHING_RELAY);
                    FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111010011110100) ^ 0b0000000000000000);
                }
                else if (VERSION_0_9k) // DONE
                {
                    if (on)
                    {
                        // 強制的にG/Sはoffにする
                        Invoke((Action)delegate ()
                        {
                            toolStripMenuGalvanoStat.Checked = false;
                        });

                        FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111111110100) ^ 0b0000000000000000);
                    }
                    else
                    {
                        Invoke((Action)delegate ()
                        {
                            if (!toolStripMenuGalvanoStat.Checked) // Potentiostat mode, non-EIS mode
                            {
                                FDwfDigitalIOOutputSet(_handle, (dwRead & 0b1111111111110100) ^ 0b0000000000000001);
                            }
                        });
                    }
                }

                if(false)
                {
                    _selectedMode_previous = _selectedMode;
                    if (on)
                    {
                        _selectedMode = modeMeasurement.eis;
                    }
                    else
                    {
                        _selectedMode = modeMeasurement.voltammetry;
                    }

                    Invoke((Action)delegate ()
                    {
                        toolStripMenuEIS.Checked = on;

                        toolStripComboBoxMethod.ComboBox.DataSource = null;
                        toolStripComboBoxMethod.ComboBox.DataSource = _tablesMethods[(int)_selectedMode];
                        toolStripComboBoxMethod.ComboBox.DisplayMember = "name";
                        toolStripComboBoxMethod.ComboBox.ValueMember = "index";

                        toolStripComboBoxRange.ComboBox.DataSource = null;
                        toolStripComboBoxRange.ComboBox.DataSource = _tablesRanges[(int)_selectedMode];
                        toolStripComboBoxRange.ComboBox.DisplayMember = "name";
                        toolStripComboBoxRange.ComboBox.ValueMember = "index";

                        if(on)
                        {
                            toolStripComboBoxMethod.SelectedIndex = 2;
                        }
                        else
                        {
                            toolStripComboBoxMethod.SelectedIndex = 0;
                        }
                    });

                }

                //updateComboBoxMethod();
            }
        }

        #endregion

        //
        // Methods
        //

        public void SetCalibrationData(
            double potential_awg, double potential_osc, double current,
            double potential_slope_awg, double potential_slope_osc, double current_slope_osc)
        {
            POTENTIAL_OFFSET_AWG = potential_awg;
            POTENTIAL_OFFSET_OSC = potential_osc;
            CURRENT_OFFSET = current;
            POTENTIAL_SLOPE_AWG = potential_slope_awg;
            POTENTIAL_SLOPE_OSC = potential_slope_osc;
            CURRENT_SLOPE = current_slope_osc;
            Console.WriteLine("--");
            Console.WriteLine("POTENTIAL_OFFSET_AWG: {0}", POTENTIAL_OFFSET_AWG);
            Console.WriteLine("POTENTIAL_OFFSET_OSC: {0}", POTENTIAL_OFFSET_OSC);
            Console.WriteLine("CURRENT_OFFSET: {0}", CURRENT_OFFSET);
            Console.WriteLine("POTENTIAL_SLOPE_AWG: {0}", POTENTIAL_SLOPE_AWG);
            Console.WriteLine("POTENTIAL_SLOPE_OSC: {0}", POTENTIAL_SLOPE_OSC);
            Console.WriteLine("CURRENT_SLOPE: {0}", CURRENT_SLOPE);
        }

        public void UpdateResistors(DataTable tableResistor)
        {
            //InitializeResistors();
            //OverrideResistorsWithCustomValues();

            for (int i = 0; i < 6; i++)
            {
                // Override with a custom value
                _tablesRanges[(int)modeMeasurement.voltammetry].Rows[i][0] = tableResistor.Rows[i][2];
            }

            _tablesRanges[(int)modeMeasurement.galvanometry] = _tablesRanges[(int)modeMeasurement.voltammetry];

            for (int i = 0; i < 6; i++)
            {
                // Override with a custom value
                _tablesRanges[(int)modeMeasurement.eis].Rows[i][0] = tableResistor.Rows[i][3];
            }

            SetSubmodule(_configure_potentiostat.submodule);
            SetFilteringMethod(_configure_potentiostat.filtering_method);
        }

        private void SetFrequencyOfAcquisition(int index)
        {
            for (int i = 0; i < _toolstripmenuitemsFrequencyOfAcquisition.Length; i++)
            {
                if(i == index)
                {
                    _toolstripmenuitemsFrequencyOfAcquisition[i].Checked = true;
                    _hertzAcquisition = double.Parse(_toolstripmenuitemsFrequencyOfAcquisition[i].Tag.ToString())
                                       * (_hertzTargetFilteringFrequency / 50.0); // for the aligmment of SINC filter (50 or 60 Hz?)
                }
                else
                {
                    _toolstripmenuitemsFrequencyOfAcquisition[i].Checked = false;
                }
            }

            Console.WriteLine("FrequencyOfAcquisition: {0} Hz", _hertzAcquisition);
        }

        private void SetFilteringMethod(int index)
        {
            ToolStripItemCollection items = toolStripMenuItemSaveAvaragedData.DropDownItems;

            for (int i = 0; i < items.Count; i++)
            {
                if (i == index)
                {
                    ((ToolStripMenuItem)items[i]).Checked = true;
                    _hertzTargetFilteringFrequency = double.Parse(((ToolStripMenuItem)items[i]).Tag.ToString());
                }
                else
                {
                    ((ToolStripMenuItem)items[i]).Checked = false;
                }
            }

            Console.WriteLine("FilteringMethod: {0} Hz", _hertzTargetFilteringFrequency);
        }

        private void SetMode(modeMeasurement mode)
        {
            if (mode == _selectedMode) return;

            _selectedMode_previous = _selectedMode;
            _selectedMode = mode;

            for (int i = 0; i < _toolstripmenuitemsMode.Length; i++)
            {
                if ((modeMeasurement)i == mode)
                {
                    _toolstripmenuitemsMode[i].Checked = true;
                }
                else
                {
                    _toolstripmenuitemsMode[i].Checked = false;
                }
            }

            toolStripComboBoxMethod.ComboBox.DataSource = null;
            toolStripComboBoxMethod.ComboBox.DataSource = _tablesMethods[(int)_selectedMode];
            toolStripComboBoxMethod.ComboBox.DisplayMember = "name";
            toolStripComboBoxMethod.ComboBox.ValueMember = "index";

            toolStripComboBoxRange.ComboBox.DataSource = null;
            toolStripComboBoxRange.ComboBox.DataSource = _tablesRanges[(int)_selectedMode];
            toolStripComboBoxRange.ComboBox.DisplayMember = "name";
            toolStripComboBoxRange.ComboBox.ValueMember = "index";

            switch(_selectedMode)
            {
                case modeMeasurement.voltammetry:
                    Console.WriteLine("Switching mode to Potentiostat");
                    toolStripComboBoxMethod.SelectedIndex = 0;

                    if(VERSION_0_9e)
                    {
                        SetPotentiostat(true);
                    }
                    else
                    {
                        SetGalvanostat(false);
                        SetEIS(false);
                    }
                    SelectPotentialRange(rangePotential.Range5V);

                    break;

                case modeMeasurement.galvanometry:
                    Console.WriteLine("Switching mode to Galvanostat");

                    toolStripComboBoxMethod.SelectedIndex = 1;
                    SetGalvanostat(true);
                    SelectPotentialRange(rangePotential.Range50V);

                    break;

                case modeMeasurement.eis:
                    Console.WriteLine("Switching mode to EIS");

                    toolStripComboBoxMethod.SelectedIndex = 2;
                    SetEIS(true);
                    SelectPotentialRange(rangePotential.Range5V);

                    break;
            }

            //Console.WriteLine("FilteringMethod: {0} Hz", _target_filtering_frequency);
        }

        public void SetSequenceOfRDE(int[] speeds)
        {
            _rotation_speeds = speeds;
        }

        public async void SetSwitchOnAsync(bool value_to_be_set)
        {
            // GUI操作は非同期で呼び出す
            await Task.Run(() => { _is_switched_on = value_to_be_set; SetPurging(value_to_be_set); });
        }

        private void SetSubmodule(int idx)
        {
            ToolStripItemCollection items = toolStripMenuItemSubModule.DropDownItems;

            for (int i = 0; i < items.Count; i++)
            {
                if (i == idx)
                {
                    ((ToolStripMenuItem)items[i]).Checked = true;
                    _selectedSubModule = (typeSubModule)i;
                }
                else
                {
                    ((ToolStripMenuItem)items[i]).Checked = false;
                }
            }

            // TODO: _selectedSubModuleに合わせて、メニュー項目を動的に変更する
            // TODO:     メニューのイベントハンドラ内：_selectedSubModuleに合わせて、処理内容を変更するようにする
            // TODO:     backgroundWorkerCV_DoWork 内：_selectedSubModuleに合わせて、処理内容を変更するようにする (SetPurging＋SetRotation)
            // TODO:     DoVoltammetry 内：_selectedSubModuleに合わせて、処理内容を変更するようにする

            Console.WriteLine("Submodule selected: {0}, {1}", _selectedSubModule, items[idx].Text);

            switch (_selectedSubModule)
            {
                case typeSubModule.RDE:
                    toolStripMenuItemSubModuleOption1.Visible = true;
                    toolStripMenuItemSubModuleOption2.Visible = false;
                    toolStripMenuItemSubModuleOption3.Visible = true;
                    toolStripMenuItemSubModuleOption3Param1.Visible = true;
                    toolStripMenuItemSubModuleOption3Param2.Visible = false;
                    toolStripMenuItemSubModuleOption4.Visible = true;

                    toolStripMenuItemSubModuleOption1.Text = "    Purge";
                    toolStripMenuItemSubModuleOption2.Text = "    ";
                    toolStripMenuItemSubModuleOption3.Text = "    Rotate disk at the speed [rpm] of";
                    toolStripMenuItemSubModuleOption3Param2.Text = "";
                    toolStripMenuItemSubModuleOption4.Text = "    Set a sequence of rotating speeds...";

                    toolStripMenuItemSubModuleOption1.Checked = false;
                    toolStripMenuItemSubModuleOption2.Checked = false;
                    toolStripMenuItemSubModuleOption3.Checked = false;
                    toolStripMenuItemSubModuleOption3Param2.Checked = false;
                    toolStripMenuItemSubModuleOption4.Checked = false;

                    break;

                case typeSubModule.Synchronous_switching:
                    toolStripMenuItemSubModuleOption1.Visible = true;
                    toolStripMenuItemSubModuleOption2.Visible = false;
                    toolStripMenuItemSubModuleOption3.Visible = true;
                    toolStripMenuItemSubModuleOption3Param1.Visible = true;
                    toolStripMenuItemSubModuleOption3Param2.Visible = true;
                    toolStripMenuItemSubModuleOption4.Visible = false;

                    toolStripMenuItemSubModuleOption1.Text = "    Switch on";
                    toolStripMenuItemSubModuleOption2.Text = "    ";
                    toolStripMenuItemSubModuleOption3.Text = "    Switch on/off at intervals [s] of ";
                    toolStripMenuItemSubModuleOption3Param2.Text = "Finally on";
                    toolStripMenuItemSubModuleOption4.Text = "    ";

                    toolStripMenuItemSubModuleOption1.Checked = false;
                    toolStripMenuItemSubModuleOption2.Checked = false;
                    toolStripMenuItemSubModuleOption3.Checked = false;
                    toolStripMenuItemSubModuleOption3Param2.Checked = false;
                    toolStripMenuItemSubModuleOption4.Checked = false;

                    break;

                default:
                    break;
            }
        }

        private void StartAcquisition(bool fSave)
        {
            if (toolStripButtonRecord.Text == "&Record")
            {
                double channel_range = 0;

                _millivoltInitial = Double.NaN; _millivoltInitial_raw = Double.NaN;
                _millivoltVertex = Double.NaN; _millivoltVertex_raw = Double.NaN;
                _millivoltLimit = Double.NaN; _millivoltLimit_raw = Double.NaN;
                _countRepeat = Double.NaN;
                _millivoltScanrate = Double.NaN; _millivoltScanrate_raw = Double.NaN;
                _millivoltAmplitude = Double.NaN; _millivoltAmplitude_raw = Double.NaN;
                _millivoltStep = Double.NaN; _millivoltStep_raw = Double.NaN;
                _countStep = Double.NaN;
                _hertzInitial = Double.NaN;
                _hertzFinal = Double.NaN;
                _secDuration = Double.NaN; _secPreelectrolysis = Double.NaN;
                _secInterval = Double.NaN;

                switch (_selectedMethod)
                {
                    case methodMeasurement.Cyclicgalvanometry:
                        if (double.TryParse(toolStripTextBoxInitialV.Text, out _millivoltInitial)
                            && (_millivoltInitial <= 50000)
                            && (_millivoltInitial >= -50000))
                        {
                            _millivoltInitial_raw = _millivoltInitial;

                            _millivoltInitial *= (-1000.0 / (_selectedCurrentFactor));

                            _millivoltInitial -= POTENTIAL_OFFSET_AWG;
                            _millivoltInitial /= POTENTIAL_SLOPE_AWG;
                        }
                        else { MessageBox.Show(this, "The value of Initial [uA] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxVertexV.Text, out _millivoltVertex)
                            && (_millivoltVertex <= 50000)
                            && (_millivoltVertex >= -50000))
                        {
                            _millivoltVertex_raw = _millivoltVertex;

                            _millivoltVertex *= (-1000.0 / (_selectedCurrentFactor));

                            _millivoltVertex -= POTENTIAL_OFFSET_AWG;
                            _millivoltVertex /= POTENTIAL_SLOPE_AWG;
                        }
                        else { MessageBox.Show(this, "The value of Vertex [uA] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxScanrate.Text, out _millivoltScanrate)
                            && (_millivoltScanrate <= 1000000)
                            && (_millivoltScanrate > 0))
                        {
                            _millivoltScanrate_raw = _millivoltScanrate;

                            if (hzToolStripMenuItemAuto.Checked)
                            {
                                // 取り込み速度を自動決定する。目安としては0.25mV/point。高速掃引時には2.5mV/ptでもよいか？？
                                // すなわち掃引速度が100mV/sの時には400Hzで、10000mV/s (10V/s)の時には4kHzで取り込めばよいことになる

                                SetFrequencyOfAcquisition(0);
                                for (int i = (_toolstripmenuitemsFrequencyOfAcquisition.Length - 1); i >= 0; i--)
                                {
                                    if ((_millivoltScanrate * 4) >= Double.Parse(_toolstripmenuitemsFrequencyOfAcquisition[i].Tag.ToString()))
                                    {
                                        SetFrequencyOfAcquisition(i);
                                        break;
                                    }
                                }
                            }

                            _millivoltScanrate *= (1000 / (_selectedCurrentFactor));
                            _millivoltScanrate /= POTENTIAL_SLOPE_AWG;
                        }
                        else { MessageBox.Show(this, "The value of Scan rate [uA/s] is invalid."); return; }

                        if (Math.Abs(_millivoltVertex - _millivoltInitial) > 50000)
                        {
                            MessageBox.Show(this, "The value of Scanning range [uA] is too wide (> 50000 uV)."); return;
                        }


                        FDwfAnalogInChannelOffsetSet(_handle, (CHANNEL_POTENTIAL - 1), 0.0);

                        break;

                    case methodMeasurement.ConstantCurrent:
                        if (double.TryParse(toolStripTextBoxInitialV.Text, out _millivoltInitial)
                            && (_millivoltInitial <= 200000)
                            && (_millivoltInitial >= -200000))
                        {
                            _millivoltInitial_raw = _millivoltInitial;
                            _millivoltInitial = (_millivoltInitial * (-1000.0 / (double)_selectedCurrentFactor));

                            _millivoltInitial -= POTENTIAL_OFFSET_AWG;// - POTENTIAL_OFFSET_OSC;
                            _millivoltInitial /= POTENTIAL_SLOPE_AWG;
                            //_millivoltInitial *= CURRENT_SLOPE / (_selectedCurrentFactor / 10000) / POTENTIAL_SLOPE_OSC;
                            //_millivoltInitial *= CURRENT_SLOPE / POTENTIAL_SLOPE_OSC;

                            // TODO: (in case methodMeasurement.ConstantCurrent) これでOK？
                            _millivoltInitial /= POTENTIAL_SLOPE_OSC;
                            _millivoltInitial *= (1 + _calibrate_potentiostat.ohmInternalResistance / (1000000 / _selectedCurrentFactor));
                            Console.WriteLine("_millivoltInitial (corrected): {0}", _millivoltInitial);
                        }
                        else { MessageBox.Show(this, "The value of Current [uA] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxVertexV.Text, out _secDuration)
                            && (_secDuration >= 1))
                        {
                            //_millivoltVertex += (int)POTENTIAL_OFFSET;
                        }
                        else { MessageBox.Show(this, "The value of Time [min] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxScanrate.Text, out _secInterval)
                            && (_secInterval <= 60)
                            && (_secInterval >= 1))
                        {
                        }
                        else { MessageBox.Show(this, "The value of Sampling Interval [s] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxRepeat.Text, out _countRepeat)
                            && (_countRepeat >= 0.0))
                        {
                        }
                        else { MessageBox.Show(this, "The target Q [C] (as an absolute value) must be >= 0."); toolStripTextBoxRepeat.Text = "0"; return; }

                        if (double.TryParse(toolStripTextBoxStep.Text, out _millivoltLimit)
                            && (_millivoltLimit >= 0.0))
                        {
                            _millivoltLimit_raw = _millivoltLimit;

                            _millivoltLimit /= RATIO_POTENTIAL;
                            _millivoltLimit -= POTENTIAL_OFFSET_AWG;
                            _millivoltLimit /= POTENTIAL_SLOPE_AWG;
                        }
                        else { MessageBox.Show(this, "The potential limit, E [mV] for S electrode (as an absolute value) must be >= 0."); toolStripTextBoxRepeat.Text = "0"; return; }

                        FDwfAnalogInChannelOffsetSet(_handle, (CHANNEL_POTENTIAL - 1), 0.0);

                        break;

                    case methodMeasurement.EIS:
                    case methodMeasurement.EIS_Open_Circuit:
                    case methodMeasurement.EIS_Short_Circuit:
                        if (double.TryParse(toolStripTextBoxInitialV.Text, out _millivoltInitial)
                            && (_millivoltInitial <= 5000)
                            && (_millivoltInitial >= -5000))
                        {
                            _millivoltInitial_raw = _millivoltInitial;

                            _millivoltInitial /= RATIO_POTENTIAL;
                            _millivoltInitial -= POTENTIAL_OFFSET_AWG;
                            _millivoltInitial /= POTENTIAL_SLOPE_AWG;
                        }
                        else { MessageBox.Show(this, "The value of Potential [mV] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxVertexV.Text, out _millivoltAmplitude)
                            && (_millivoltAmplitude <= 500)
                            && (_millivoltAmplitude >= 1))
                        {
                            _millivoltAmplitude_raw = _millivoltAmplitude;

                            _millivoltAmplitude /= RATIO_POTENTIAL;
                            //_millivoltAmplitude -= POTENTIAL_OFFSET_AWG;
                            _millivoltAmplitude /= POTENTIAL_SLOPE_AWG;
                        }
                        else { MessageBox.Show(this, "The value of Amplitude [mV] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxScanrate.Text, out _hertzInitial)
                            && (_hertzInitial <= 10000000)
                            && (_hertzInitial >= 0.0001))
                        {
                        }
                        else { MessageBox.Show(this, "The value of Initial Scanning Frequency [Hz] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxScanrate2.Text, out _hertzFinal)
                            && (_hertzFinal <= 10000000)
                            && (_hertzFinal >= 0.0001))
                        {
                        }
                        else { MessageBox.Show(this, "The value of Final Scanning Frequency [Hz] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxRepeat.Text, out _countRepeat)
                            && (_countRepeat <= 4096)
                            && (_countRepeat >= 1))
                        {
                        }
                        else { MessageBox.Show(this, "The value of Accumulation Time is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxStep.Text, out _countStep)
                            && (_countStep <= 1000)
                            && (_countStep >= 1))
                        {
                        }
                        else { MessageBox.Show(this, "The value of Steps is invalid."); return; }

                        break;

                    case methodMeasurement.EIS_MottSchottkey:
                        if (double.TryParse(toolStripTextBoxInitialV.Text, out _millivoltInitial)
                            && (_millivoltInitial <= 5000)
                            && (_millivoltInitial >= -5000))
                        {
                            _millivoltInitial_raw = _millivoltInitial;

                            _millivoltInitial /= RATIO_POTENTIAL;
                            _millivoltInitial -= POTENTIAL_OFFSET_AWG;
                            _millivoltInitial /= POTENTIAL_SLOPE_AWG;
                        }
                        else { MessageBox.Show(this, "The value of Initial potential [mV] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxFinalV.Text, out _millivoltVertex)
                            && (_millivoltVertex <= 5000)
                            && (_millivoltVertex >= -5000))
                        {
                            _millivoltVertex_raw = _millivoltVertex;

                            _millivoltVertex /= RATIO_POTENTIAL;
                            _millivoltVertex -= POTENTIAL_OFFSET_AWG;
                            _millivoltVertex /= POTENTIAL_SLOPE_AWG;
                        }
                        else { MessageBox.Show(this, "The value of Final potential [mV] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxVertexV.Text, out _millivoltAmplitude)
                            && (_millivoltAmplitude <= 500)
                            && (_millivoltAmplitude >= 1))
                        {
                            _millivoltAmplitude_raw = _millivoltAmplitude;

                            _millivoltAmplitude /= RATIO_POTENTIAL;
                            //_millivoltAmplitude -= POTENTIAL_OFFSET_AWG;
                            _millivoltAmplitude /= POTENTIAL_SLOPE_AWG;
                        }
                        else { MessageBox.Show(this, "The value of Amplitude [mV] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxScanrate.Text, out _hertzInitial)
                            && (_hertzInitial <= 100000)
                            && (_hertzInitial >= 0.001))
                        {
                        }
                        else { MessageBox.Show(this, "The value of Frequency [Hz] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxRepeat.Text, out _countRepeat)
                            && (_countRepeat <= 4096)
                            && (_countRepeat >= 1))
                        {
                        }
                        else { MessageBox.Show(this, "The value of Accumulation Time is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxStep.Text, out _millivoltStep)
                            && (_millivoltStep <= 100000)
                            && (_millivoltStep >= 0.001))
                        {
                            _millivoltStep_raw = _millivoltStep;

                            _millivoltStep /= RATIO_POTENTIAL;
                            //_millivoltAmplitude -= POTENTIAL_OFFSET_AWG;
                            _millivoltStep /= POTENTIAL_SLOPE_AWG;
                        }
                        else { MessageBox.Show(this, "The value of Step [mV] is invalid."); return; }

                        break;

                    case methodMeasurement.Oscilloscope:
                        if (double.TryParse(toolStripTextBoxInitialV.Text, out _millivoltInitial)
                            && (_millivoltInitial <= 5000)
                            && (_millivoltInitial >= -5000))
                        {
                            _millivoltInitial_raw = _millivoltInitial;

                            _millivoltInitial /= RATIO_POTENTIAL;
                            _millivoltInitial -= POTENTIAL_OFFSET_AWG;
                            _millivoltInitial /= POTENTIAL_SLOPE_AWG;
                        }
                        else { MessageBox.Show(this, "The value of Initial potential [mV] is invalid."); return; }

                        //if (double.TryParse(toolStripTextBoxFinalV.Text, out _millivoltVertex)
                        //    && (_millivoltVertex <= 5000)
                        //    && (_millivoltVertex >= -5000))
                        //{
                        //    _millivoltVertex -= POTENTIAL_OFFSET_AWG;
                        //    _millivoltVertex /= POTENTIAL_SLOPE_AWG;
                        //}
                        //else { MessageBox.Show(this, "The value of Final potential [mV] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxVertexV.Text, out _millivoltAmplitude)
                            && (_millivoltAmplitude <= 500)
                            && (_millivoltAmplitude >= 1))
                        {
                            _millivoltAmplitude_raw = _millivoltAmplitude;

                            _millivoltAmplitude /= RATIO_POTENTIAL;
                            //_millivoltAmplitude -= POTENTIAL_OFFSET_AWG;
                            _millivoltAmplitude /= POTENTIAL_SLOPE_AWG;
                        }
                        else { MessageBox.Show(this, "The value of Amplitude [mV] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxScanrate.Text, out _hertzInitial)
                            && (_hertzInitial <= 100000)
                            && (_hertzInitial >= 0.001))
                        {
                        }
                        else { MessageBox.Show(this, "The value of Frequency [Hz] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxRepeat.Text, out _countRepeat)
                            && (_countRepeat <= 4096)
                            && (_countRepeat >= 1))
                        {
                        }
                        else { MessageBox.Show(this, "The value of Accumulation Time is invalid."); return; }

                        //if (double.TryParse(toolStripTextBoxStep.Text, out _millivoltStep)
                        //    && (_millivoltStep <= 100000)
                        //    && (_millivoltStep >= 0.001))
                        //{
                        //}
                        //else { MessageBox.Show(this, "The value of Step [mV] is invalid."); return; }

                        if (hzToolStripMenuItemCustom.Checked)
                        {
                            if (double.TryParse(toolStripTextBoxFreqOfAcquisition.Text, out double ret))
                            {
                                _hertzAcquisition = ret;
                            }
                            else
                            {
                                hzToolStripMenuItemAuto.Checked = true;
                                hzToolStripMenuItemCustom.Checked = false;
                            }
                        }

                        if (hzToolStripMenuItemAuto.Checked)
                        {
                            //double millivoltHeight = Math.Abs(_millivoltVertex - _millivoltInitial);
                            //double secRecording = (2 * millivoltHeight) / _millivoltScanrate;
                            _hertzAcquisition = _hertzInitial * 1000;
                        }

                        FDwfAnalogInChannelRangeGet(_handle, (CHANNEL_POTENTIAL - 1), out channel_range);
                        Console.WriteLine("Channel range for potential measurement: {0}", channel_range);

                        if ((Math.Abs(_millivoltVertex - _millivoltInitial) > (2700)) && (channel_range <= 5.6))
                        {
                            if (true)
                            {
                                Console.WriteLine("Need to adjust ChannelOffset...");
                                // Need to adjust ChannelOffset...
                                double center = (_millivoltVertex - _millivoltInitial) / 2;

                                FDwfAnalogInChannelOffsetSet(_handle, (CHANNEL_POTENTIAL - 1), (center / 1000.0));
                                FDwfAnalogInChannelRangeSet(_handle, (CHANNEL_POTENTIAL - 1), 5.0); // for E measurement
                            }
                        }
                        else
                        {
                            FDwfAnalogInChannelOffsetSet(_handle, (CHANNEL_POTENTIAL - 1), 0.0);
                        }

                        break;

                    case methodMeasurement.Cyclicvoltammetry:
                    case methodMeasurement.Series_of_RDE_CV:
                    case methodMeasurement.LSV:
                    case methodMeasurement.Series_of_RDE_LSV:
                    case methodMeasurement.OSWV:
                        if (double.TryParse(toolStripTextBoxInitialV.Text, out _millivoltInitial)
                            && (_millivoltInitial <= 5000)
                            && (_millivoltInitial >= -5000))
                        {
                            _millivoltInitial_raw = _millivoltInitial;

                            _millivoltInitial /= RATIO_POTENTIAL;
                            _millivoltInitial -= POTENTIAL_OFFSET_AWG;
                            _millivoltInitial /= POTENTIAL_SLOPE_AWG;
                        }
                        else { MessageBox.Show(this, "The value of Initial [mV] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxVertexV.Text, out _millivoltVertex)
                            && (_millivoltVertex <= 5000)
                            && (_millivoltVertex >= -5000))
                        {
                            _millivoltVertex_raw = _millivoltVertex;

                            _millivoltVertex /= RATIO_POTENTIAL;
                            _millivoltVertex -= POTENTIAL_OFFSET_AWG;
                            _millivoltVertex /= POTENTIAL_SLOPE_AWG;
                        }
                        else { MessageBox.Show(this, "The value of Vertex [mV] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxScanrate.Text, out _millivoltScanrate)
                            && (_millivoltScanrate <= 1000000000)
                            && (_millivoltScanrate > 0))
                        {
                            _millivoltScanrate_raw = _millivoltScanrate;

                            _millivoltScanrate /= RATIO_POTENTIAL;
                            _millivoltScanrate /= POTENTIAL_SLOPE_AWG;
                        }
                        else { MessageBox.Show(this, "The value of Scan rate [mV/s] is invalid."); return; }

                        //if (Math.Abs(_millivoltVertex - _millivoltInitial) > 5000)
                        //{
                        //    MessageBox.Show(this, "The value of Scanning range [mV] is too wide (> 5000 mV)."); return;
                        //}

                        if (double.TryParse(toolStripTextBoxStep.Text, out _secPreelectrolysis)
                            && (_secPreelectrolysis >= 1))
                        {
                            //_millivoltVertex += (int)POTENTIAL_OFFSET;
                        }
                        else { MessageBox.Show(this, "The value of pre-electrolysis time [s] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxRepeat.Text, out _countRepeat)
                            && (_countRepeat <= 100)
                            && (_countRepeat >= 1))
                        {
                        }
                        else { MessageBox.Show(this, "The number of repeat (>= 1) is invalid."); toolStripTextBoxRepeat.Text = "1"; return; }


                        if (hzToolStripMenuItemCustom.Checked)
                        {
                            if (double.TryParse(toolStripTextBoxFreqOfAcquisition.Text, out double ret))
                            {
                                _hertzAcquisition = ret;
                            }
                            else
                            {
                                hzToolStripMenuItemAuto.Checked = true;
                                hzToolStripMenuItemCustom.Checked = false;
                            }
                        }

                        if (hzToolStripMenuItemAuto.Checked)
                        {
                            // 取り込み速度を自動決定する。目安としては0.25mV/point。高速掃引時には2.5mV/ptでもよいか？？
                            // すなわち掃引速度が100mV/sの時には400Hzで、10000mV/s (10V/s)の時には4kHzで取り込めばよいことになる

                            SetFrequencyOfAcquisition(0);
                            for (int i = (_toolstripmenuitemsFrequencyOfAcquisition.Length - 1); i >= 0; i--)
                            {
                                if (((_millivoltScanrate / POTENTIAL_SLOPE_OSC) * 4) >= Double.Parse(_toolstripmenuitemsFrequencyOfAcquisition[i].Tag.ToString())) // 100mV/s => 100Hzにしてみる
                                {
                                    SetFrequencyOfAcquisition(i);
                                    break;
                                }
                            }
                        }

                        //double min = 0, max = 0, step = 0;
                        //FDwfAnalogInChannelOffsetInfo(_handle, ref min, ref max, ref step); min/max was -25/+25, step was 16xxx.

                        FDwfAnalogInChannelRangeGet(_handle, (CHANNEL_POTENTIAL - 1), out channel_range);
                        Console.WriteLine("Channel range for potential measurement: {0}", channel_range);

                        if ((Math.Abs(_millivoltVertex - _millivoltInitial) > (2700)) && (channel_range <= 5.6))
                        {
                            if (Math.Abs(_millivoltVertex - _millivoltInitial) < 5000)
                            {
                                Console.WriteLine("Need to adjust ChannelOffset...");
                                // Need to adjust ChannelOffset...
                                double center = (_millivoltVertex - _millivoltInitial) / 2;

                                FDwfAnalogInChannelOffsetSet(_handle, (CHANNEL_POTENTIAL - 1), (center / 1000.0));
                                FDwfAnalogInChannelRangeSet(_handle, (CHANNEL_POTENTIAL - 1), 5.0); // for E measurement
                            }
                            else
                            {
                                MessageBox.Show(this, "The value of Scanning range [mV] is too wide (> 5000 mV). Try to switch the potential range to +-25V."); return;
                            }
                        }
                        else
                        {
                            FDwfAnalogInChannelOffsetSet(_handle, (CHANNEL_POTENTIAL - 1), 0.0);
                        }

                        //_millivoltInitial = +1 * _millivoltInitial / 2;
                        //_millivoltVertex = +1 * _millivoltVertex / 2;
                        //_millivoltScanrate = _millivoltScanrate / 2;

                        break;

                    case methodMeasurement.CyclicvoltammetryQuick: // TODO: (in case methodMeasurement.CyclicvoltammetryQuick) RATIO_POTENTIALに起因する問題は未検討
                        if (double.TryParse(toolStripTextBoxInitialV.Text, out _millivoltInitial)
                            && (_millivoltInitial <= 5000)
                            && (_millivoltInitial >= -5000))
                        {
                            _millivoltInitial_raw = _millivoltInitial;

                            _millivoltInitial -= POTENTIAL_OFFSET_AWG;
                            _millivoltInitial /= POTENTIAL_SLOPE_AWG;
                        }
                        else { MessageBox.Show(this, "The value of Initial [mV] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxVertexV.Text, out _millivoltVertex)
                            && (_millivoltVertex <= 5000)
                            && (_millivoltVertex >= -5000))
                        {
                            _millivoltVertex_raw = _millivoltVertex;

                            _millivoltVertex -= POTENTIAL_OFFSET_AWG;
                            _millivoltVertex /= POTENTIAL_SLOPE_AWG;
                        }
                        else { MessageBox.Show(this, "The value of Vertex [mV] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxScanrate.Text, out _millivoltScanrate)
                            && (_millivoltScanrate <= 1000000)
                            && (_millivoltScanrate >= 1))
                        {
                            _millivoltScanrate_raw = _millivoltScanrate;

                            _millivoltScanrate *= 1000;
                            _millivoltScanrate /= POTENTIAL_SLOPE_AWG;
                        }
                        else { MessageBox.Show(this, "The value of Scan rate [V/s] is invalid."); return; }

                        if (Math.Abs(_millivoltVertex - _millivoltInitial) > 5000)
                        {
                            MessageBox.Show(this, "The value of Scanning range [mV] is too wide (> 5000 mV)."); return;
                        }


                        if (hzToolStripMenuItemCustom.Checked)
                        {
                            if (double.TryParse(toolStripTextBoxFreqOfAcquisition.Text, out double ret))
                            {
                                _hertzAcquisition = ret;
                            }
                            else
                            {
                                hzToolStripMenuItemAuto.Checked = true;
                                hzToolStripMenuItemCustom.Checked = false;
                            }
                        }

                        if (hzToolStripMenuItemAuto.Checked)
                        {
                            double millivoltHeight = Math.Abs(_millivoltVertex - _millivoltInitial);
                            double secRecording = (2 * millivoltHeight) / _millivoltScanrate;
                            _hertzAcquisition = (1.0 / secRecording) * 6000;
                        }


                        FDwfAnalogInChannelRangeGet(_handle, (CHANNEL_POTENTIAL - 1), out channel_range);
                        Console.WriteLine("Channel range for potential measurement: {0}", channel_range);

                        if ((Math.Abs(_millivoltVertex - _millivoltInitial) > (2700)) && (channel_range <= 5.6))
                        {
                            if (Math.Abs(_millivoltVertex - _millivoltInitial) < 5000)
                            {
                                Console.WriteLine("Need to adjust ChannelOffset...");
                                // Need to adjust ChannelOffset...
                                double center = (_millivoltVertex - _millivoltInitial) / 2;

                                FDwfAnalogInChannelOffsetSet(_handle, (CHANNEL_POTENTIAL - 1), (center / 1000.0));
                                FDwfAnalogInChannelRangeSet(_handle, (CHANNEL_POTENTIAL - 1), 5.0); // for E measurement
                            }
                            else
                            {
                                MessageBox.Show(this, "The value of Scanning range [mV] is too wide (> 5000 mV). Try to switch the potential range to +-25V."); return;
                            }
                        }
                        else
                        {
                            FDwfAnalogInChannelOffsetSet(_handle, (CHANNEL_POTENTIAL - 1), 0.0);
                        }

                        break;

                    case methodMeasurement.BulkElectrolysis:
                        if (double.TryParse(toolStripTextBoxInitialV.Text, out _millivoltInitial)
                            && (_millivoltInitial <= 11000)
                            && (_millivoltInitial >= -11000))
                        {
                            _millivoltInitial_raw = _millivoltInitial;

                            _millivoltInitial /= RATIO_POTENTIAL;
                            _millivoltInitial -= POTENTIAL_OFFSET_AWG;
                            _millivoltInitial /= POTENTIAL_SLOPE_AWG;
                        }
                        else { MessageBox.Show(this, "The value of Potential [mV] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxVertexV.Text, out _secDuration)
                            && (_secDuration >= 1))
                        {
                            //_millivoltVertex += (int)POTENTIAL_OFFSET;
                        }
                        else { MessageBox.Show(this, "The value of Duration [min] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxScanrate.Text, out _secInterval)
                            && (_secInterval <= 60)
                            && (_secInterval >= 1))
                        {
                        }
                        else { MessageBox.Show(this, "The value of Sampling Interval [s] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxRepeat.Text, out _countRepeat)
                            && (_countRepeat >= 0.0))
                        {
                        }
                        else { MessageBox.Show(this, "The target Q [C] (as an absolute value) must be >= 0."); toolStripTextBoxRepeat.Text = "0"; return; }

                        _is_using_potential_switcher = toolStripMenuItemDetail1.Checked;
                        if(_is_using_potential_switcher)
                        {
                            if (double.TryParse(toolStripLabeledTextBoxDetail1Option1.Text, out _millivoltSwitchingPotential)
                                && (_millivoltSwitchingPotential <= 11000)
                                && (_millivoltSwitchingPotential >= -11000))
                            {
                            }
                            else { MessageBox.Show(this, "The value of Switching Potential [mV] is invalid."); toolStripLabeledTextBoxDetail1Option1.Text = "0"; return; }

                            if (double.TryParse(toolStripLabeledTextBoxDetail1Option2.Text, out _secPotentialSwitchingInterval)
                                && (_secPotentialSwitchingInterval >= 1)
                                && (_secPotentialSwitchingInterval > _secInterval) )
                            {
                            }
                            else { MessageBox.Show(this, "The value of Switching Interval [s] is invalid. It must be larger than 1 sec and the Sampling Interval. "); toolStripLabeledTextBoxDetail1Option2.Text = "1"; return; }
                        }


                        FDwfAnalogInChannelRangeGet(_handle, (CHANNEL_POTENTIAL - 1), out channel_range);

                        if ((Math.Abs(_millivoltInitial) > (2700)) && (channel_range <= 5.6))
                        {
                            if (Math.Abs(_millivoltInitial) < 5000)
                            {
                                Console.WriteLine("Need to adjust ChannelOffset...");
                                // Need to adjust ChannelOffset...
                                double center = _millivoltInitial;

                                FDwfAnalogInChannelOffsetSet(_handle, (CHANNEL_POTENTIAL - 1), (center / 1000.0));
                                FDwfAnalogInChannelRangeSet(_handle, (CHANNEL_POTENTIAL - 1), 5.0); // for E measurement
                            }
                            else
                            {
                                MessageBox.Show(this, "The value of potential [mV] is out of range (> 5000 mV). Try to switch the potential range to +-25V."); return;
                            }
                        }
                        else
                        {
                            FDwfAnalogInChannelOffsetSet(_handle, (CHANNEL_POTENTIAL - 1), 0.0);
                        }

                        //_millivoltInitial = +1 * _millivoltInitial / 2;

                        break;

                    case methodMeasurement.OCP:
                        if (double.TryParse(toolStripTextBoxVertexV.Text, out _secDuration)
                            && (_secDuration >= 0))
                        {
                        }
                        else { MessageBox.Show(this, "The value of Duration [min] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxScanrate.Text, out _secInterval)
                            && (_secInterval <= 10)
                            && (_secInterval >= 1))
                        {
                        }
                        else { MessageBox.Show(this, "The value of Sampling Interval [s] is invalid."); return; }

                        break;

                    case methodMeasurement.DPSCA:
                    case methodMeasurement.IRC:
                        if (double.TryParse(toolStripTextBoxInitialV.Text, out _millivoltInitial)
                            && (_millivoltInitial <= 5000)
                            && (_millivoltInitial >= -5000))
                        {
                            _millivoltInitial_raw = _millivoltInitial;

                            _millivoltInitial /= RATIO_POTENTIAL;
                            _millivoltInitial -= POTENTIAL_OFFSET_AWG;
                            _millivoltInitial /= POTENTIAL_SLOPE_AWG;
                        }
                        else { MessageBox.Show(this, "The value of Initial [mV] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxVertexV.Text, out _millivoltVertex)
                            && (_millivoltVertex <= 5000)
                            && (_millivoltVertex >= -5000))
                        {
                            _millivoltVertex_raw = _millivoltVertex;

                            if (_selectedMethod == methodMeasurement.DPSCA)
                            {
                                //_millivoltVertex /= RATIO_POTENTIAL;
                                _millivoltVertex -= POTENTIAL_OFFSET_AWG;
                                //_millivoltVertex /= POTENTIAL_SLOPE_AWG;
                            }
                            _millivoltVertex /= POTENTIAL_SLOPE_AWG;
                        }
                        else { MessageBox.Show(this, "The value of Step-in (or Amplitude) [mV] is invalid."); return; }

                        if (double.TryParse(toolStripTextBoxScanrate.Text, out _secDuration)
                            && (_secDuration <= 120)
                            && (_secDuration >= 1))
                        {
                        }
                        else { MessageBox.Show(this, "The value of Duration [s] is invalid."); return; }

                        FDwfAnalogInChannelRangeGet(_handle, (CHANNEL_POTENTIAL - 1), out channel_range);

                        double range = 0;
                        if (_selectedMethod == methodMeasurement.DPSCA)
                        {
                            range = _millivoltVertex - _millivoltInitial;
                        }
                        else
                        {
                            range = _millivoltInitial;
                        }
                        if ((Math.Abs(range) > (2700)) && (channel_range <= 5.6))
                        {
                            if (Math.Abs(_millivoltInitial) < 5000)
                            {
                                Console.WriteLine("Need to adjust ChannelOffset...");
                                // Need to adjust ChannelOffset...
                                double center = (range) / 2;

                                FDwfAnalogInChannelOffsetSet(_handle, (CHANNEL_POTENTIAL - 1), (center / 1000.0));
                                FDwfAnalogInChannelRangeSet(_handle, (CHANNEL_POTENTIAL - 1), 5.0); // for E measurement
                            }
                            else
                            {
                                MessageBox.Show(this, "The value of potential [mV] is out of range (> 5000 mV). Try to switch the potential range to +-25V."); return;
                            }
                        }
                        else
                        {
                            FDwfAnalogInChannelOffsetSet(_handle, (CHANNEL_POTENTIAL - 1), 0.0);
                        }

                        //_millivoltInitial = +1 * _millivoltInitial / 2;
                        //_millivoltVertex = +1 * _millivoltVertex / 2;

                        break;
                }

                //if ((_selectedMethod = (methodMeasurement)toolStripComboBoxMethod.SelectedIndex) == methodMeasurement.none)
                //{
                //    MessageBox.Show("Method is not selected."); return;
                //}

                _file_path = null;

                if (fSave)
                {
                    saveFileDialog1.Filter = "EC-Lab Text file (*.mpt)|*.mpt|All types(*.*)|*.*";
                    saveFileDialog1.FilterIndex = 1;
                    saveFileDialog1.Title = "Save As";
                    saveFileDialog1.FileName = "";
                    saveFileDialog1.ShowHelp = true;

                    if (saveFileDialog1.InitialDirectory == null)
                    {
                        saveFileDialog1.InitialDirectory = @"c:\";
                    }

                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        _file_path = saveFileDialog1.FileName;
                        //string _tmp = System.IO.Path.GetExtension(_file_path);
                        if (System.IO.Path.GetExtension(_file_path) != ".mpt") _file_path += ".mpt";

                        toolStripStatusLabelFileName.Text = PathCompactEx.Converter.ShrinkPath(_file_path, 70);
                        toolStripStatusLabelFileName.ToolTipText = _file_path;

                        //saveFileDialog1.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
                    }
                }
                else
                {
                    _file_path = null;

                    toolStripStatusLabelFileName.Text = "(not to be saved)";
                }

                if (fSave && _file_path == null)
                {
                    return;
                }

                //
                // 直ぐに測定を始める、もしくは一定時間待機してから始める
                //

                // ここで別スレッドを立ち上げる必要はない。その場合、async void StartAcquisitionとする
                new Thread(new ThreadStart(() => // delegate
                {
                    Invoke((Action)async delegate ()
                    {
                        bool fCancel = false;

                        if (toolStripMenuItemDelay.Checked)
                        {
                            WaitDialog wd = new WaitDialog();
                            //wd.ShowWithTimeout(this, 10);

                            //Invoke((Action)delegate ()
                            //{
                            //    //this.Enabled = false;

                            //    wd.Test(this);

                            //    //this.Enabled = true;
                            //});

                            //this.Enabled = false;
                            toolStrip1.Enabled = false;
                            toolStrip2.Enabled = false;

                            //wd.Show();

                            //Task HogeTask = Task.Run(
                            //    async () => { await Task.Delay(5000); }
                            //);
                            //await HogeTask;

                            if (int.TryParse(toolStripTextBoxDelayTime.Text, out int timeout))
                            {
                                fCancel = await wd.ShowAsync(timeout);
                            }
                            else
                            {
                                fCancel = false;
                            }

                            //this.Enabled = true;
                            toolStrip1.Enabled = true;
                            toolStrip2.Enabled = true;
                        }

                        if (!fCancel)
                        {
                            toolStripButtonRecord.Text = "&Stop"; toolStripButtonRecord.Image = global::Voltammogrammer.Properties.Resources.Stop;
                            toolStripButtonScan.Enabled = false;
                            toolStripComboBoxMethod.Enabled = false;
                            toolStripComboBoxRange.Enabled = false;
                            toolStripComboBoxReferenceForInitialPotential.Enabled = false;

                            toolStripTextBoxInitialV.Enabled = false;
                            if (_selectedMethod != methodMeasurement.BulkElectrolysis && _selectedMethod != methodMeasurement.ConstantCurrent)
                            {
                                toolStripTextBoxVertexV.Enabled = false;
                                toolStripTextBoxScanrate.Enabled = false;
                                toolStripTextBoxRepeat.Enabled = false;
                            }

                            timerCurrentEandI.Enabled = false;

                            Console.WriteLine("Collect block immediate...");
                            backgroundWorkerCV.RunWorkerAsync();
                        }
                        else
                        {
                            Console.WriteLine("Collect block was cancelled.");
                        }
                    });
                })).Start();
            }
            else
            {
                backgroundWorkerCV.CancelAsync();

                toolStripButtonRecord.Enabled = false;
                toolStripButtonRecord.Text = "Stopping...";
            }
        }

        private void updateComboBoxMethod()
        {
            string unit1 = "", unit2 = "";

            if (_selectedMode != _selectedMode_previous)
            {
            }

            //if(_selectedMode != _selectedMode_previous)
            //{
            switch (_selectedMode)
            {
                case modeMeasurement.voltammetry:
                    unit1 = "mV"; unit2 = "mV/s";
                    if (_selectedMethod != methodMeasurement.Oscilloscope)
                    {
                        chartVoltammogram.ChartAreas[0].AxisX.Title = "Time / s";
                    }
                    else
                    {
                        chartVoltammogram.ChartAreas[0].AxisX.Title = "Time / ms";
                    }
                    chartVoltammogram.ChartAreas[0].AxisX.Maximum = Double.NaN;
                    chartVoltammogram.ChartAreas[0].AxisX.Minimum = 0;
                    chartVoltammogram.ChartAreas[0].AxisY.Title = "Potential / mV";
                    chartVoltammogram.ChartAreas[0].AxisX2.Title = "";
                    chartVoltammogram.ChartAreas[0].AxisX2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.False;
                    chartVoltammogram.ChartAreas[0].AxisX2.IsLogarithmic = false;
                    chartVoltammogram.ChartAreas[0].AxisY2.Title = "Current / uA";
                    chartVoltammogram.ChartAreas[0].AxisY2.IsLogarithmic = false;
                    chartVoltammogram.ChartAreas[0].AxisY2.MinorTickMark.Enabled = false;

                    toolStripComboBoxReferenceForInitialPotential.Enabled = true;

                    if (_selectedMode_previous != modeMeasurement.voltammetry) toolStripComboBoxRange.SelectedIndex = 3;

                    break;

                case modeMeasurement.galvanometry:
                    unit1 = "uA"; unit2 = "uA/s";
                    chartVoltammogram.ChartAreas[0].AxisX.Title = "Time / s";
                    chartVoltammogram.ChartAreas[0].AxisX.Maximum = Double.NaN;
                    chartVoltammogram.ChartAreas[0].AxisX.Minimum = 0;
                    chartVoltammogram.ChartAreas[0].AxisY.Title = "Potential of S. / mV\nvs Ref. electrode";
                    chartVoltammogram.ChartAreas[0].AxisX2.Title = "";
                    chartVoltammogram.ChartAreas[0].AxisX2.IsLogarithmic = false;
                    chartVoltammogram.ChartAreas[0].AxisX2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.False;
                    chartVoltammogram.ChartAreas[0].AxisY2.Title = "Potential of W. / mV\nvs C. electrode";
                    chartVoltammogram.ChartAreas[0].AxisY2.IsLogarithmic = false;
                    chartVoltammogram.ChartAreas[0].AxisY2.MinorTickMark.Enabled = false;

                    toolStripComboBoxReferenceForInitialPotential.Enabled = false;

                    if (_selectedMode_previous != modeMeasurement.galvanometry) toolStripComboBoxRange.SelectedIndex = 2;

                    break;

                case modeMeasurement.eis:
                    chartVoltammogram.Series[1].Points.Clear();
                    chartVoltammogram.Series[2].Points.Clear();
                    chartVoltammogram.Series[3].Points.Clear();
                    chartVoltammogram.Series[4].Points.Clear();
                    chartVoltammogram.Series[5].Points.Clear();

                    chartVoltammogram.Series[6].Points.SuspendUpdates();
                    chartVoltammogram.Series[6].Points.Clear();
                    chartVoltammogram.Series[6].Points.AddXY(1, 1);
                    chartVoltammogram.Series[6].Points.ResumeUpdates();

                    chartVoltammogram.Update();

                    unit1 = "mV"; unit2 = "Hz";
                    chartVoltammogram.ChartAreas[0].AxisX.Title = "Re[Z] / ohm";
                    chartVoltammogram.ChartAreas[0].AxisX.Maximum = Double.NaN;
                    chartVoltammogram.ChartAreas[0].AxisX.Minimum = 0;// Double.NaN;
                    chartVoltammogram.ChartAreas[0].AxisY.Title = "Im[Z] / ohm";
                    chartVoltammogram.ChartAreas[0].AxisX2.Title = "Frequency / Hz";
                    chartVoltammogram.ChartAreas[0].AxisX2.Minimum = Double.NaN;
                    //chartVoltammogram.ChartAreas[0].AxisX2.Minimum = 1;
                    //chartVoltammogram.ChartAreas[0].AxisX2.Maximum = 1000000;
                    chartVoltammogram.ChartAreas[0].AxisX2.IsLogarithmic = true;
                    chartVoltammogram.ChartAreas[0].AxisX2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
                    chartVoltammogram.ChartAreas[0].AxisY2.Title = "|Z| / ohm";
                    chartVoltammogram.ChartAreas[0].AxisY2.IsLogarithmic = true;
                    chartVoltammogram.ChartAreas[0].AxisY2.MinorTickMark.Enabled = true;
                    chartVoltammogram.ChartAreas[0].AxisY2.MinorTickMark.LineColor = System.Drawing.Color.Red;
                    chartVoltammogram.ChartAreas[0].AxisY2.MinorTickMark.Size = 0.5F;
                    chartVoltammogram.ChartAreas[0].AxisY2.MinorTickMark.Interval = 1;

                    toolStripComboBoxReferenceForInitialPotential.Enabled = true;

                    if (_selectedMode_previous != modeMeasurement.eis) toolStripComboBoxRange.SelectedIndex = 2;

                    break;

                default:
                    break;
            }
            //}
            //_selectedMode_previous = _selectedMode;

            toolStripLabel1.ToolTipText = ""; toolStripLabel1.AutoToolTip = true;
            toolStripLabel1.Visible = true; toolStripTextBoxInitialV.Visible = true; toolStripComboBoxReferenceForInitialPotential.Visible = true;
            toolStripLabel6.Visible = false;
            toolStripTextBoxFinalV.Visible = false;
            toolStripLabel7.Visible = false;
            toolStripTextBoxScanrate2.Visible = false;
            toolStripLabel8.Visible = false;
            toolStripTextBoxStep.Visible = false;

            toolStripDropDownButtonDetails.Visible = false;

            if (_selectedMode == modeMeasurement.voltammetry || _selectedMode == modeMeasurement.galvanometry)
            {
                switch (_selectedMethod)
                {
                    case methodMeasurement.Cyclicvoltammetry:
                    case methodMeasurement.Series_of_RDE_CV:
                    case methodMeasurement.Cyclicgalvanometry:
                        toolStripLabel1.Text = "Initial [" + unit1 + "]:";
                        toolStripLabel2.Text = "Vertex [" + unit1 + "]:";
                        toolStripTextBoxVertexV.Text = "500";
                        toolStripLabel3.Text = "Scanning rate [" + unit1 + "/s]:";
                        toolStripTextBoxScanrate.Text = "100";
                        toolStripLabel8.Text = "Pre-electrolysis [s]"; toolStripLabel8.Visible = true;
                        toolStripTextBoxStep.Text = "2"; toolStripTextBoxStep.Visible = true;
                        toolStripLabel4.Text = "Repeat:";
                        toolStripTextBoxRepeat.Text = "1";
                        break;

                    case methodMeasurement.CyclicvoltammetryQuick:
                        toolStripLabel1.Text = "Initial [" + unit1 + "]:";
                        toolStripLabel2.Text = "Vertex [" + unit1 + "]:";
                        toolStripTextBoxVertexV.Text = "500";
                        toolStripLabel3.Text = "Scanning rate [V/s]:";
                        toolStripTextBoxScanrate.Text = "1";
                        toolStripLabel4.Text = "Repeat:";
                        toolStripTextBoxRepeat.Text = "1";
                        break;

                    case methodMeasurement.BulkElectrolysis:
                        toolStripLabel1.Text = "Potential [" + unit1 + "]:";
                        //toolStripLabel6.Text = "Potential [" + unit1 + "]:"; toolStripLabel6.Visible = true;
                        //toolStripTextBoxFinalV.Text = ""; toolStripTextBoxFinalV.Visible = true;
                        toolStripLabel2.Text = "Duration [min]:";
                        toolStripTextBoxVertexV.Text = "60";
                        toolStripLabel3.Text = "Sampling Interval [s]:";
                        toolStripTextBoxScanrate.Text = "1";
                        //toolStripLabel7.Text = "Potential switching interval [s]:"; toolStripLabel7.Visible = true;
                        //toolStripTextBoxScanrate2.Text = "0"; toolStripTextBoxScanrate2.Visible = true;
                        toolStripLabel4.Text = "Target |Q| [C]:";
                        toolStripTextBoxRepeat.Text = "0";

                        toolStripDropDownButtonDetails.Visible = true;

                        break;

                    case methodMeasurement.ConstantCurrent:
                        toolStripLabel1.Text = "Current [" + unit1 + "]:"; toolStripLabel1.ToolTipText = "Set the amplitude of a current flowed out from the working to current electrodes";
                        toolStripLabel2.Text = "Duration [min]:";
                        toolStripTextBoxVertexV.Text = "60";
                        toolStripLabel3.Text = "Sampling Interval [s]:";
                        toolStripTextBoxScanrate.Text = "1";
                        toolStripLabel4.Text = "Target |Q| [C]:";
                        toolStripTextBoxRepeat.Text = "0";
                        toolStripLabel8.Text = "Limit of |E| for S [mV]:"; toolStripLabel8.Visible = true;
                        toolStripTextBoxStep.Text = "0"; toolStripTextBoxStep.Visible = true;
                        break;

                    case methodMeasurement.LSV:
                    case methodMeasurement.Series_of_RDE_LSV:
                    case methodMeasurement.OSWV:
                        toolStripLabel1.Text = "Initial [" + unit1 + "]:";
                        toolStripLabel2.Text = "Final [" + unit1 + "]:";
                        toolStripTextBoxVertexV.Text = "500";
                        toolStripLabel3.Text = "Scanning rate [" + unit1 + "/s]:";
                        toolStripTextBoxScanrate.Text = "100";
                        toolStripLabel8.Text = "Pre-electrolysis [s]"; toolStripLabel8.Visible = true;
                        toolStripTextBoxStep.Text = "2"; toolStripTextBoxStep.Visible = true;
                        toolStripLabel4.Text = "Repeat:";
                        toolStripTextBoxRepeat.Text = "1";
                        break;

                    case methodMeasurement.DPSCA:
                        toolStripLabel1.Text = "Initial [" + unit1 + "]:";
                        toolStripLabel2.Text = "Step in [" + unit1 + "]:";
                        toolStripTextBoxVertexV.Text = "500";
                        toolStripLabel3.Text = "Duration [s]:";
                        toolStripTextBoxScanrate.Text = "5";
                        toolStripLabel4.Text = "Repeat:";
                        toolStripTextBoxRepeat.Text = "1";
                        break;

                    case methodMeasurement.IRC:
                        toolStripLabel1.Text = "Initial [" + unit1 + "]:";
                        toolStripLabel2.Text = "Amplitude [" + unit1 + "]:";
                        toolStripTextBoxVertexV.Text = "25";
                        toolStripLabel3.Text = "Duration [s]:";
                        toolStripTextBoxScanrate.Text = "1";
                        toolStripLabel4.Text = "Repeat:";
                        toolStripTextBoxRepeat.Text = "1";
                        break;

                    case methodMeasurement.OCP:
                        //toolStripLabel1.Text = "N/A:"; 
                        toolStripLabel1.Visible = false; toolStripTextBoxInitialV.Visible = false; toolStripComboBoxReferenceForInitialPotential.Visible = false;
                        toolStripLabel2.Text = "Duration [min]:";
                        toolStripTextBoxVertexV.Text = "60";
                        toolStripLabel3.Text = "Sampling Interval [s]:";
                        toolStripTextBoxScanrate.Text = "1";
                        toolStripLabel4.Text = "Repeat:";
                        toolStripTextBoxRepeat.Text = "1";
                        break;

                    case methodMeasurement.Oscilloscope:
                        toolStripLabel1.Text = "Potential [" + unit1 + "]:";
                        toolStripTextBoxInitialV.Text = "0";
                        toolStripLabel2.Text = "Amplitude [" + unit1 + "]:";
                        toolStripTextBoxVertexV.Text = "10";
                        toolStripLabel3.Text = "Frequency [" + "Hz" + "]:";
                        toolStripTextBoxScanrate.Text = "1000";
                        toolStripLabel4.Text = "Accumulation:";
                        toolStripTextBoxRepeat.Text = "64";
                        break;
                }
                toolStripLabel5.Text = "Current Range:";
            }
            else if (_selectedMode == modeMeasurement.eis)// && _selectedMode_previous != modeMeasurement.eis)
            {
                //
                // TODO: methodを選択すると値が初期化されるのは良くないが、異なるModeからの変更では、初期値を入れる必要がある
                //
                //if(
                //       _selectedMethod_previous != methodMeasurement.EIS_Open_Circuit
                //    && _selectedMethod_previous != methodMeasurement.EIS_Short_Circuit
                //    && _selectedMethod_previous != methodMeasurement.EIS
                //    && _selectedMethod_previous != methodMeasurement.EIS_MottSchottkey
                //)
                //{
                switch (_selectedMethod)
                {
                    case methodMeasurement.EIS_Open_Circuit:
                    case methodMeasurement.EIS_Short_Circuit:
                    case methodMeasurement.EIS:
                        toolStripLabel1.Text = "Potential [" + unit1 + "]:";
                        toolStripLabel2.Text = "Amplitude [" + unit1 + "]:";
                        toolStripTextBoxVertexV.Text = "100";
                        toolStripLabel3.Text = "Scanning Frequency [" + unit2 + "] from:";
                        toolStripTextBoxScanrate.Text = "1";
                        toolStripLabel7.Text = "to:"; toolStripLabel7.Visible = true;
                        toolStripTextBoxScanrate2.Text = "1000000"; toolStripTextBoxScanrate2.Visible = true;
                        toolStripLabel4.Text = "Accumulation:";
                        toolStripTextBoxRepeat.Text = "64";
                        toolStripLabel8.Text = "Steps/dec.:"; toolStripLabel8.Visible = true;
                        toolStripTextBoxStep.Text = "10"; toolStripTextBoxStep.Visible = true;
                        toolStripLabel5.Text = "Reference Resistor:";

                        toolStripComboBoxRange.SelectedIndex = 2;

                        break;

                    case methodMeasurement.EIS_MottSchottkey:
                        toolStripLabel1.Text = "Initial [" + unit1 + "]:";
                        toolStripTextBoxInitialV.Text = "0";
                        toolStripLabel6.Text = "Final [mV]:"; toolStripLabel6.Visible = true;
                        toolStripTextBoxFinalV.Text = "-1000"; toolStripTextBoxFinalV.Visible = true;
                        toolStripLabel2.Text = "Amplitude [" + unit1 + "]:";
                        toolStripTextBoxVertexV.Text = "10";
                        toolStripLabel3.Text = "Frequency [" + unit2 + "]:";
                        toolStripTextBoxScanrate.Text = "1000";
                        toolStripLabel4.Text = "Accumulation:";
                        toolStripTextBoxRepeat.Text = "64";
                        toolStripLabel8.Text = "Step [mV]:"; toolStripLabel8.Visible = true;
                        toolStripTextBoxStep.Text = "10"; toolStripTextBoxStep.Visible = true;
                        toolStripLabel5.Text = "Reference Resistor:";

                        toolStripComboBoxRange.SelectedIndex = 1;

                        break;
                }
                //}
            }
            else
            {

            }

            _selectedMode_previous = _selectedMode;
        }

        private void undoZoom()
        {
            //if (_currentIndex == -1) return;

            chartVoltammogram.SuspendLayout();

            chartVoltammogram.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
            chartVoltammogram.ChartAreas[0].AxisX2.ScaleView.ZoomReset(0);
            chartVoltammogram.ChartAreas[0].AxisY.ScaleView.ZoomReset(0);
            chartVoltammogram.ChartAreas[0].AxisY2.ScaleView.ZoomReset(0);

            double delta = 0;
            chartVoltammogram.ChartAreas[0].AxisY.LabelStyle.IntervalOffset = -delta;
            chartVoltammogram.ChartAreas[0].AxisY.MajorTickMark.IntervalOffset = -delta;
            chartVoltammogram.ChartAreas[0].AxisY.MinorTickMark.IntervalOffset = -delta;
            chartVoltammogram.ChartAreas[0].AxisY2.LabelStyle.IntervalOffset = -delta;
            chartVoltammogram.ChartAreas[0].AxisY2.MajorTickMark.IntervalOffset = -delta;
            chartVoltammogram.ChartAreas[0].AxisY2.MinorTickMark.IntervalOffset = -delta;


            chartVoltammogram.ChartAreas[0].AxisX.LabelStyle.IntervalOffset = -delta;
            chartVoltammogram.ChartAreas[0].AxisX.MajorTickMark.IntervalOffset = -delta;
            chartVoltammogram.ChartAreas[0].AxisX.MinorTickMark.IntervalOffset = -delta;
            chartVoltammogram.ChartAreas[0].AxisX2.LabelStyle.IntervalOffset = -delta;
            chartVoltammogram.ChartAreas[0].AxisX2.MajorTickMark.IntervalOffset = -delta;
            chartVoltammogram.ChartAreas[0].AxisX2.MinorTickMark.IntervalOffset = -delta;

            chartVoltammogram.ResumeLayout();
            chartVoltammogram.PerformLayout();
        }

        #region Miscellaneous event handlers
        //
        // Miscellaneous event handlers
        //

        private void toolStripMenuItem_Click(object sender, EventArgs e)
        {
            string senderName = ((ToolStripMenuItem)(sender)).Name;

            //ボタンのベース名　長さの取得に使用
            string strBut = "hzToolStripMenuItem";

            //Buttonxxのxxを取得して数字に直している
            int index = int.Parse(senderName.Substring(strBut.Length, senderName.Length - strBut.Length));

            if (((ToolStripMenuItem)sender).Checked)
            {
            }
            else
            {
                SetFrequencyOfAcquisition(index - 1);
            }
        }

        private void toolStripMenuItemMode_Click(object sender, EventArgs e)
        {
            modeMeasurement mode = (modeMeasurement)int.Parse(((ToolStripMenuItem)sender).Tag.ToString());
            SetMode(mode);
        }

        //private void toolStripMenuGalvanoStat_Click(object sender, EventArgs e)
        //{
        //    //SetGalvanostat(!toolStripMenuGalvanoStat.Checked);
        //    SetMode(modeMeasurement.galvanometry);
        //}
        //private void toolStripMenuEIS_Click(object sender, EventArgs e)
        //{
        //    //if(toolStripMenuEIS.Checked)
        //    //{
        //    //    SetEIS(false);
        //    //}
        //    //else
        //    //{
        //    //    SetEIS(true);
        //    //}
        //    //SetEIS(!toolStripMenuEIS.Checked);
        //    SetMode(modeMeasurement.eis);
        //}

        private void toolStripMenuItemDebug_Click(object sender, EventArgs e)
        {
            toolStripMenuItemDebug.Checked = !toolStripMenuItemDebug.Checked;
            DEBUG_VOLTAMMOGRAM = toolStripMenuItemDebug.Checked;
        }

        private void toolStripMenuItemAuto_Click(object sender, EventArgs e)
        {
            hzToolStripMenuItemAuto.Checked = !hzToolStripMenuItemAuto.Checked;
            if(hzToolStripMenuItemAuto.Checked)
            {
                hzToolStripMenuItemCustom.Checked = false;
            }
        }

        private void toolStripMenuItemCustom_Click(object sender, EventArgs e)
        {
            hzToolStripMenuItemCustom.Checked = !hzToolStripMenuItemCustom.Checked;
            if(hzToolStripMenuItemCustom.Checked)
            {
                hzToolStripMenuItemAuto.Checked = false;
            }
        }

        private void toolStripMenuClearComp_Click(object sender, EventArgs e)
        {
            toolStripMenuOpenComp.Checked = false;
            toolStripMenuShortComp.Checked = false;

            _recordingSeries[CHANNEL_VIRTUAL_OPEN_RS][0] = Double.NaN;
            _recordingSeries[CHANNEL_VIRTUAL_SHORT_RS][0] = Double.NaN;
        }

        private void toolStripButtonConnect_DropDownItemClicked(object sender, EventArgs e)
        {
            int idx = toolStripComboBoxSerialPort.SelectedIndex;

            _tableDevice.Rows.Clear();

            FDwfEnum(enumfilterAll, out int c);

            for (int i = 0; i < c; i++)
            {
                FDwfEnumDeviceType(i, out int type, out int rev);
                if (type == devidDiscovery2)
                {
                    FDwfEnumSN(i, out string sn);

                    _tableDevice.Rows.Add(sn, i);
                }
            }

            if (idx != -1 && idx < c) toolStripComboBoxSerialPort.SelectedIndex = idx;
        }

        private void contextMenuItemUndoZoom_Click(object sender, EventArgs e)
        {
            undoZoom();
        }

        private void toolStripTextBoxScanrate_Validating(object sender, CancelEventArgs e)
        {
            if (!backgroundWorkerCV.IsBusy) return; 

            if(_selectedMethod == methodMeasurement.BulkElectrolysis || _selectedMethod == methodMeasurement.ConstantCurrent)
            {
                if (double.TryParse(toolStripTextBoxScanrate.Text, out double value)
                    && (value <= 60)
                    && (value >= 1))
                {
                    _secInterval = value;
                    MessageBox.Show(this, $"The value of Sampling Interval [s] is now: {_secInterval}");
                }
                else { MessageBox.Show(this, "The value of Sampling Interval [s] is invalid."); e.Cancel = true; return; }
            }
        }

        private void toolStripTextBoxStep_Validating(object sender, CancelEventArgs e)
        {
            if (!backgroundWorkerCV.IsBusy) return; 

            if(_selectedMethod == methodMeasurement.ConstantCurrent)
            {
                if (int.TryParse(toolStripTextBoxStep.Text, out int value)
                    && (value >= 1))
                {
                    _millivoltLimit = value;
                    MessageBox.Show(this, $"The potential limit, |E| [mV] for S electrode (as an absolute value) is now: {_millivoltLimit}");
                }
                else { MessageBox.Show(this, "The potential limit, |E| [mV] for S electrode (as an absolute value) is invalid."); e.Cancel = true; return; }
            }
        }

        private void toolStripTextBoxVertexV_Validating(object sender, CancelEventArgs e)
        {
            if (!backgroundWorkerCV.IsBusy) return; 

            if(_selectedMethod == methodMeasurement.BulkElectrolysis || _selectedMethod == methodMeasurement.ConstantCurrent)
            {
                if (int.TryParse(toolStripTextBoxVertexV.Text, out int value)
                    && (value >= 1))
                {
                    _secDuration = value;
                    MessageBox.Show(this, $"The value of Duration [min] is now: {_secDuration}");
                }
                else { MessageBox.Show(this, "The value of Duration [min] is invalid."); e.Cancel = true; return; }
            }
        }

        private void toolStripTextBoxRepeat_Validating(object sender, CancelEventArgs e)
        {
            if (!backgroundWorkerCV.IsBusy) return; 

            if(_selectedMethod == methodMeasurement.BulkElectrolysis || _selectedMethod == methodMeasurement.ConstantCurrent)
            {
                if (double.TryParse(toolStripTextBoxRepeat.Text, out double value)
                    && (value >= 0.0))
                {
                    _countRepeat = value;
                    MessageBox.Show(this, $"The target |Q| [C] is now: {_countRepeat}");
                }
                else { MessageBox.Show(this, "The target |Q| [C] should be >= 0."); e.Cancel = true; return; }
            }
        }

        private void toolStripMenuItemRange25VonlyforCurrent_CheckedChanged(object sender, EventArgs e)
        {
            SelectPotentialRange(_selectedPotentialRange);
        }

        private void toolStripMenuItemConfigure_Click(object sender, EventArgs e)
        {
            _configure_potentiostat.Show();
        }

        private void toolStripMenuItemCalibrate_Click(object sender, EventArgs e)
        {
            _calibrate_potentiostat.Show();
        }

        private void toolStripMenuItemSaveAvaragedData_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            SetFilteringMethod(item.DropDownItems.IndexOf(e.ClickedItem));
        }

        private void toolStripComboBoxReferenceForInitialPotential_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(toolStripComboBoxReferenceForInitialPotential.SelectedIndex != -1)
            {
                Properties.Settings.Default.configure_referencing_for_initial_potential = toolStripComboBoxReferenceForInitialPotential.SelectedIndex;
                Properties.Settings.Default.Save();

                if(_selectedMode == modeMeasurement.voltammetry)
                {
                    switch(toolStripComboBoxReferenceForInitialPotential.SelectedIndex)
                    {
                        case 0: // vs REF
                            toolStripLabel8.Enabled = true;
                            toolStripTextBoxStep.Enabled = true;
                            break;
                        case 1: // vs OCP
                            toolStripLabel8.Enabled = false;
                            toolStripTextBoxStep.Enabled = false;
                            break;
                    }
                }
            }
        }

        private void toolStripMenuItemRange_Click(object sender, EventArgs e)
        {
            //string senderName = ((ToolStripMenuItem)(sender)).Name;

            ////ボタンのベース名　長さの取得に使用
            //string strBut = "toolStripMenuItemRange";

            ////Buttonxxのxxを取得して数字に直している
            //int index = int.Parse(senderName.Substring(strBut.Length, senderName.Length - strBut.Length));

            if (((ToolStripMenuItem)sender).Checked)
            {
            }
            else
            {
                SelectPotentialRange((rangePotential)int.Parse(((ToolStripMenuItem)(sender)).Tag.ToString()));

                //for (int i = 0; i < _toolstripmenuitemsRange.Length; i++)
                //{
                //    if ((i + 1) == index)
                //    {
                //        _toolstripmenuitemsRange[i].Checked = true;
                //        int range = int.Parse(_toolstripmenuitemsRange[i].Tag.ToString());

                //        //FDwfAnalogInChannelOffsetSet(_handle, 0, 0.0);
                //        //FDwfAnalogInChannelRangeSet(_handle, 0, range); // for E measurement
                //        //FDwfAnalogInChannelOffsetSet(_handle, 1, 0.0);
                //        //FDwfAnalogInChannelRangeSet(_handle, 1, range); // for I measurement
                //        switch(range)
                //        {
                //            case 1: _selectedPotentialRange = rangePotential.Range50V; break;
                //            case 2: _selectedPotentialRange = rangePotential.Range5V; break;
                //            case 3: _selectedPotentialRange = rangePotential.Range500mV; break;
                //            default: _selectedPotentialRange = rangePotential.Range5V;  break;
                //        }
                //        SelectPotentialRange(_selectedPotentialRange);
                //    }
                //    else
                //    {
                //        _toolstripmenuitemsRange[i].Checked = false
                //    }
                //}
            }
        }

        #endregion
    }

    [System.Windows.Forms.Design.ToolStripItemDesignerAvailability(
      System.Windows.Forms.Design.ToolStripItemDesignerAvailability.ContextMenuStrip |
      System.Windows.Forms.Design.ToolStripItemDesignerAvailability.ToolStrip |
      System.Windows.Forms.Design.ToolStripItemDesignerAvailability.MenuStrip |
      System.Windows.Forms.Design.ToolStripItemDesignerAvailability.StatusStrip)]
    public class ToolStripLabeledTextBox : ToolStripControlHost
    {
        TextBox textbox;
        Label label;
        public ToolStripLabeledTextBox() : base(new FlowLayoutPanel())
        {
            textbox = new TextBox();
            label = new Label();

            label.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;// | AnchorStyles.Left;
            label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            label.AutoSize = true;
            label.Margin = new Padding(0);
            label.Padding = new Padding(0);

            label.Text = "label";
            textbox.Text = "text";

            FlowLayoutPanel flp = ((FlowLayoutPanel)base.Control);

            flp.Controls.Add(label);
            flp.Controls.Add(textbox);

            flp.BackColor = System.Drawing.Color.Transparent;
            flp.AutoSize = true;
            flp.Padding = new Padding(0);
            flp.Margin = new Padding(0);

            base.AutoSize = true;
        }

        public ToolStripLabeledTextBox(string label_text, string text) : this()
        {
            label.Text = label_text;
            textbox.Text = text;
        }

        public override string Text
        {
            get
            {
                return textbox.Text;
            }
            set
            {
                textbox.Text = value;
            }
        }

        public string Caption
        {
            get
            {
                return label.Text;
            }
            set
            {
                label.Text = value;

                base.Size = new System.Drawing.Size(label.Width + textbox.Width, base.Size.Height);
            }
        }

        public override System.Drawing.Size Size
        {
            get
            {
                return base.Size;
            }
            set
            {
                if(label != null && textbox != null)
                {
                    base.Size = new System.Drawing.Size(label.Width + textbox.Width, base.Size.Height);
                }
            }
        }
    }

    namespace PathCompactEx
    {
        public static class Converter
        {
            [DllImport("shlwapi.dll", CallingConvention = CallingConvention.StdCall, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
            extern static bool PathCompactPathExW([Out] System.Text.StringBuilder ResultPath, String SourcePath, int HowManyLetters, int Delimiter);

            public static System.String EllipsisCompactor(String Path, Int16 HowManyLetters)
            {
                System.Text.StringBuilder b = new System.Text.StringBuilder(HowManyLetters + 1);
                PathCompactPathExW(b, Path, HowManyLetters + 1, '\\');
                return b.ToString();
            }

            // Taken form "Function to shrink file path to be more human readable" @ Stackoverflow
            public static string ShrinkPath(string path, int maxLength)
            {
                var parts = path.Split('\\');
                var output = String.Join("\\", parts, 0, parts.Length);
                var endIndex = (parts.Length - 1);
                var startIndex = endIndex / 2;
                var index = startIndex;
                var step = 0;

                while (output.Length >= maxLength && index != 0 && index != endIndex)
                {
                    parts[index] = "...";
                    output = String.Join("\\", parts, 0, parts.Length);
                    if (step >= 0) step++;
                    step = (step * -1);
                    index = startIndex + step;
                }
                return output;
            }
        }
    }
}
