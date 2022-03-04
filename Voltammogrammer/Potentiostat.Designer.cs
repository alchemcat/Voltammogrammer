namespace Voltammogrammer
{
    partial class Potentiostat
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Potentiostat));
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series5 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series6 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series7 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.DataPoint dataPoint1 = new System.Windows.Forms.DataVisualization.Charting.DataPoint(1D, 1D);
            this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.RightToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.LeftToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.ContentPanel = new System.Windows.Forms.ToolStripContentPanel();
            this.backgroundWorkerCV = new System.ComponentModel.BackgroundWorker();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonConnect = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripComboBoxSerialPort = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemConfigure = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemDebug = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonRecord = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripMenuItemOCV = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemDelay = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripTextBoxDelayTime = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemRDE = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripComboBoxRDESpeed = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemPurge = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemSaveAvaragedData = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemFiltering60Hz = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemFiltering50Hz = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.hzToolStripMenuItemAuto = new System.Windows.Forms.ToolStripMenuItem();
            this.hzToolStripMenuItemCustom = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripTextBoxFreqOfAcquisition = new System.Windows.Forms.ToolStripTextBox();
            this.hzToolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
            this.hzToolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            this.hzToolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.hzToolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.hzToolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.hzToolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.hzToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.hzToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemRange3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRange1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRange2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRange25VonlyforCurrent = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuPotentioStat = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuGalvanoStat = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuEIS = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuOpenComp = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuShortComp = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuClearComp = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuSignalInspector = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemCalibrate = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButtonScan = new System.Windows.Forms.ToolStripButton();
            this.toolStripComboBoxMethod = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel5 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxRange = new System.Windows.Forms.ToolStripComboBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabelStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelCycle = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelModelOfPicoscope = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelFileName = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusCursor = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusCurrentEandI = new System.Windows.Forms.ToolStripStatusLabel();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.chartVoltammogram = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.contextMenuItemUndoZoom = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripTextBoxInitialV = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripComboBoxReferenceForInitialPotential = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel6 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripTextBoxFinalV = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripTextBoxVertexV = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripTextBoxScanrate = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripLabel7 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripTextBoxScanrate2 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripLabel8 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripTextBoxStep = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripLabel4 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripTextBoxRepeat = new System.Windows.Forms.ToolStripTextBox();
            this.timerCurrentEandI = new System.Windows.Forms.Timer(this.components);
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartVoltammogram)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // BottomToolStripPanel
            // 
            this.BottomToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.BottomToolStripPanel.Name = "BottomToolStripPanel";
            this.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.BottomToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.BottomToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // TopToolStripPanel
            // 
            this.TopToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.TopToolStripPanel.Name = "TopToolStripPanel";
            this.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.TopToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.TopToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // RightToolStripPanel
            // 
            this.RightToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.RightToolStripPanel.Name = "RightToolStripPanel";
            this.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.RightToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.RightToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // LeftToolStripPanel
            // 
            this.LeftToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.LeftToolStripPanel.Name = "LeftToolStripPanel";
            this.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.LeftToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.LeftToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // ContentPanel
            // 
            this.ContentPanel.AutoScroll = true;
            this.ContentPanel.Size = new System.Drawing.Size(727, 338);
            // 
            // backgroundWorkerCV
            // 
            this.backgroundWorkerCV.WorkerReportsProgress = true;
            this.backgroundWorkerCV.WorkerSupportsCancellation = true;
            this.backgroundWorkerCV.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerCV_DoWork);
            this.backgroundWorkerCV.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerCV_ProgressChanged);
            this.backgroundWorkerCV.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerCV_RunWorkerCompleted);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonConnect,
            this.toolStripSeparator1,
            this.toolStripButtonRecord,
            this.toolStripButtonScan,
            this.toolStripComboBoxMethod,
            this.toolStripSeparator2,
            this.toolStripLabel5,
            this.toolStripComboBoxRange});
            this.toolStrip1.Location = new System.Drawing.Point(4, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.toolStrip1.Size = new System.Drawing.Size(782, 31);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButtonConnect
            // 
            this.toolStripButtonConnect.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripComboBoxSerialPort,
            this.toolStripSeparator11,
            this.toolStripMenuItemConfigure,
            this.toolStripSeparator12,
            this.toolStripMenuItemDebug});
            this.toolStripButtonConnect.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonConnect.Image")));
            this.toolStripButtonConnect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonConnect.Name = "toolStripButtonConnect";
            this.toolStripButtonConnect.Size = new System.Drawing.Size(102, 28);
            this.toolStripButtonConnect.Text = "&Connect";
            this.toolStripButtonConnect.ButtonClick += new System.EventHandler(this.toolStripButtonConnect_Click);
            this.toolStripButtonConnect.DropDownOpening += new System.EventHandler(this.toolStripButtonConnect_DropDownItemClicked);
            // 
            // toolStripComboBoxSerialPort
            // 
            this.toolStripComboBoxSerialPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxSerialPort.DropDownWidth = 240;
            this.toolStripComboBoxSerialPort.Items.AddRange(new object[] {
            "55",
            "66"});
            this.toolStripComboBoxSerialPort.Name = "toolStripComboBoxSerialPort";
            this.toolStripComboBoxSerialPort.Size = new System.Drawing.Size(200, 28);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(290, 6);
            // 
            // toolStripMenuItemConfigure
            // 
            this.toolStripMenuItemConfigure.Name = "toolStripMenuItemConfigure";
            this.toolStripMenuItemConfigure.Size = new System.Drawing.Size(293, 26);
            this.toolStripMenuItemConfigure.Text = "Configure PocketPotentiostat...";
            this.toolStripMenuItemConfigure.Click += new System.EventHandler(this.toolStripMenuItemConfigure_Click);
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(290, 6);
            // 
            // toolStripMenuItemDebug
            // 
            this.toolStripMenuItemDebug.Name = "toolStripMenuItemDebug";
            this.toolStripMenuItemDebug.Size = new System.Drawing.Size(293, 26);
            this.toolStripMenuItemDebug.Text = "Debug mode";
            this.toolStripMenuItemDebug.Click += new System.EventHandler(this.toolStripMenuItemDebug_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 31);
            // 
            // toolStripButtonRecord
            // 
            this.toolStripButtonRecord.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemOCV,
            this.toolStripSeparator14,
            this.toolStripMenuItemDelay,
            this.toolStripSeparator3,
            this.toolStripMenuItemRDE,
            this.toolStripComboBoxRDESpeed,
            this.toolStripMenuItem1,
            this.toolStripSeparator4,
            this.toolStripMenuItemPurge,
            this.toolStripSeparator5,
            this.toolStripMenuItemSaveAvaragedData,
            this.toolStripSeparator6,
            this.hzToolStripMenuItemAuto,
            this.hzToolStripMenuItemCustom,
            this.hzToolStripMenuItem8,
            this.hzToolStripMenuItem7,
            this.hzToolStripMenuItem6,
            this.hzToolStripMenuItem5,
            this.hzToolStripMenuItem4,
            this.hzToolStripMenuItem3,
            this.hzToolStripMenuItem2,
            this.hzToolStripMenuItem1,
            this.toolStripSeparator7,
            this.toolStripMenuItemRange3,
            this.toolStripMenuItemRange1,
            this.toolStripMenuItemRange2,
            this.toolStripSeparator8,
            this.toolStripMenuPotentioStat,
            this.toolStripMenuGalvanoStat,
            this.toolStripMenuEIS,
            this.toolStripSeparator9,
            this.toolStripMenuItemCalibrate});
            this.toolStripButtonRecord.Enabled = false;
            this.toolStripButtonRecord.Image = global::Voltammogrammer.Properties.Resources.Run;
            this.toolStripButtonRecord.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonRecord.Name = "toolStripButtonRecord";
            this.toolStripButtonRecord.Size = new System.Drawing.Size(95, 28);
            this.toolStripButtonRecord.Text = "&Record";
            this.toolStripButtonRecord.ButtonClick += new System.EventHandler(this.toolStripButtonRecord_Click);
            // 
            // toolStripMenuItemOCV
            // 
            this.toolStripMenuItemOCV.Checked = true;
            this.toolStripMenuItemOCV.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItemOCV.Name = "toolStripMenuItemOCV";
            this.toolStripMenuItemOCV.Size = new System.Drawing.Size(338, 26);
            this.toolStripMenuItemOCV.Text = "OCP mode";
            this.toolStripMenuItemOCV.Click += new System.EventHandler(this.toolStripMenuItemOCV_Click);
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(335, 6);
            // 
            // toolStripMenuItemDelay
            // 
            this.toolStripMenuItemDelay.CheckOnClick = true;
            this.toolStripMenuItemDelay.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripTextBoxDelayTime});
            this.toolStripMenuItemDelay.Name = "toolStripMenuItemDelay";
            this.toolStripMenuItemDelay.Size = new System.Drawing.Size(338, 26);
            this.toolStripMenuItemDelay.Text = "Delay [s] in starting to record";
            // 
            // toolStripTextBoxDelayTime
            // 
            this.toolStripTextBoxDelayTime.Font = new System.Drawing.Font("Yu Gothic UI", 9F);
            this.toolStripTextBoxDelayTime.Name = "toolStripTextBoxDelayTime";
            this.toolStripTextBoxDelayTime.Size = new System.Drawing.Size(100, 27);
            this.toolStripTextBoxDelayTime.Text = "0";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(335, 6);
            // 
            // toolStripMenuItemRDE
            // 
            this.toolStripMenuItemRDE.Name = "toolStripMenuItemRDE";
            this.toolStripMenuItemRDE.Size = new System.Drawing.Size(338, 26);
            this.toolStripMenuItemRDE.Text = "Rotate disk at the speed [rpm] below";
            this.toolStripMenuItemRDE.Click += new System.EventHandler(this.toolStripMenuItemRDE_Click);
            // 
            // toolStripComboBoxRDESpeed
            // 
            this.toolStripComboBoxRDESpeed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxRDESpeed.Items.AddRange(new object[] {
            "100",
            "200",
            "400",
            "800",
            "1600",
            "2500",
            "3600"});
            this.toolStripComboBoxRDESpeed.Name = "toolStripComboBoxRDESpeed";
            this.toolStripComboBoxRDESpeed.Size = new System.Drawing.Size(121, 28);
            this.toolStripComboBoxRDESpeed.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBoxRDESpeed_SelectedIndexChanged);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(338, 26);
            this.toolStripMenuItem1.Text = "Set a sequence of rotating speeds...";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItemConfigureRotationSpeed_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(335, 6);
            // 
            // toolStripMenuItemPurge
            // 
            this.toolStripMenuItemPurge.Checked = true;
            this.toolStripMenuItemPurge.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItemPurge.Name = "toolStripMenuItemPurge";
            this.toolStripMenuItemPurge.Size = new System.Drawing.Size(338, 26);
            this.toolStripMenuItemPurge.Text = "Purge";
            this.toolStripMenuItemPurge.Click += new System.EventHandler(this.toolStripMenuItemPurge_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(335, 6);
            // 
            // toolStripMenuItemSaveAvaragedData
            // 
            this.toolStripMenuItemSaveAvaragedData.Checked = true;
            this.toolStripMenuItemSaveAvaragedData.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItemSaveAvaragedData.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemFiltering60Hz,
            this.toolStripMenuItemFiltering50Hz});
            this.toolStripMenuItemSaveAvaragedData.Name = "toolStripMenuItemSaveAvaragedData";
            this.toolStripMenuItemSaveAvaragedData.Size = new System.Drawing.Size(338, 26);
            this.toolStripMenuItemSaveAvaragedData.Text = "Apply a digital SINC filter (1st order)";
            this.toolStripMenuItemSaveAvaragedData.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.toolStripMenuItemSaveAvaragedData_DropDownItemClicked);
            this.toolStripMenuItemSaveAvaragedData.Click += new System.EventHandler(this.toolStripMenuItemSaveAvaragedData_Click);
            // 
            // toolStripMenuItemFiltering60Hz
            // 
            this.toolStripMenuItemFiltering60Hz.Checked = true;
            this.toolStripMenuItemFiltering60Hz.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItemFiltering60Hz.Name = "toolStripMenuItemFiltering60Hz";
            this.toolStripMenuItemFiltering60Hz.Size = new System.Drawing.Size(126, 26);
            this.toolStripMenuItemFiltering60Hz.Tag = "60.0";
            this.toolStripMenuItemFiltering60Hz.Text = "60Hz";
            // 
            // toolStripMenuItemFiltering50Hz
            // 
            this.toolStripMenuItemFiltering50Hz.Name = "toolStripMenuItemFiltering50Hz";
            this.toolStripMenuItemFiltering50Hz.Size = new System.Drawing.Size(126, 26);
            this.toolStripMenuItemFiltering50Hz.Tag = "50.0";
            this.toolStripMenuItemFiltering50Hz.Text = "50Hz";
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(335, 6);
            // 
            // hzToolStripMenuItemAuto
            // 
            this.hzToolStripMenuItemAuto.Checked = true;
            this.hzToolStripMenuItemAuto.CheckState = System.Windows.Forms.CheckState.Checked;
            this.hzToolStripMenuItemAuto.Name = "hzToolStripMenuItemAuto";
            this.hzToolStripMenuItemAuto.Size = new System.Drawing.Size(338, 26);
            this.hzToolStripMenuItemAuto.Text = "Auto";
            this.hzToolStripMenuItemAuto.Click += new System.EventHandler(this.toolStripMenuItemAuto_Click);
            // 
            // hzToolStripMenuItemCustom
            // 
            this.hzToolStripMenuItemCustom.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripTextBoxFreqOfAcquisition});
            this.hzToolStripMenuItemCustom.Name = "hzToolStripMenuItemCustom";
            this.hzToolStripMenuItemCustom.Size = new System.Drawing.Size(338, 26);
            this.hzToolStripMenuItemCustom.Text = "Custom";
            this.hzToolStripMenuItemCustom.Click += new System.EventHandler(this.toolStripMenuItemCustom_Click);
            // 
            // toolStripTextBoxFreqOfAcquisition
            // 
            this.toolStripTextBoxFreqOfAcquisition.Font = new System.Drawing.Font("Yu Gothic UI", 9F);
            this.toolStripTextBoxFreqOfAcquisition.Name = "toolStripTextBoxFreqOfAcquisition";
            this.toolStripTextBoxFreqOfAcquisition.Size = new System.Drawing.Size(100, 27);
            this.toolStripTextBoxFreqOfAcquisition.Text = "1000";
            // 
            // hzToolStripMenuItem8
            // 
            this.hzToolStripMenuItem8.Name = "hzToolStripMenuItem8";
            this.hzToolStripMenuItem8.Size = new System.Drawing.Size(338, 26);
            this.hzToolStripMenuItem8.Tag = "40000";
            this.hzToolStripMenuItem8.Text = "40kHz";
            // 
            // hzToolStripMenuItem7
            // 
            this.hzToolStripMenuItem7.Name = "hzToolStripMenuItem7";
            this.hzToolStripMenuItem7.Size = new System.Drawing.Size(338, 26);
            this.hzToolStripMenuItem7.Tag = "12800";
            this.hzToolStripMenuItem7.Text = "12.8kHz";
            this.hzToolStripMenuItem7.Click += new System.EventHandler(this.toolStripMenuItem_Click);
            // 
            // hzToolStripMenuItem6
            // 
            this.hzToolStripMenuItem6.Name = "hzToolStripMenuItem6";
            this.hzToolStripMenuItem6.Size = new System.Drawing.Size(338, 26);
            this.hzToolStripMenuItem6.Tag = "6400";
            this.hzToolStripMenuItem6.Text = "6.4kHz";
            this.hzToolStripMenuItem6.Click += new System.EventHandler(this.toolStripMenuItem_Click);
            // 
            // hzToolStripMenuItem5
            // 
            this.hzToolStripMenuItem5.Name = "hzToolStripMenuItem5";
            this.hzToolStripMenuItem5.Size = new System.Drawing.Size(338, 26);
            this.hzToolStripMenuItem5.Tag = "1600";
            this.hzToolStripMenuItem5.Text = "1.6kHz";
            this.hzToolStripMenuItem5.Click += new System.EventHandler(this.toolStripMenuItem_Click);
            // 
            // hzToolStripMenuItem4
            // 
            this.hzToolStripMenuItem4.Checked = true;
            this.hzToolStripMenuItem4.CheckState = System.Windows.Forms.CheckState.Checked;
            this.hzToolStripMenuItem4.Name = "hzToolStripMenuItem4";
            this.hzToolStripMenuItem4.Size = new System.Drawing.Size(338, 26);
            this.hzToolStripMenuItem4.Tag = "400";
            this.hzToolStripMenuItem4.Text = "400Hz";
            this.hzToolStripMenuItem4.Click += new System.EventHandler(this.toolStripMenuItem_Click);
            // 
            // hzToolStripMenuItem3
            // 
            this.hzToolStripMenuItem3.Name = "hzToolStripMenuItem3";
            this.hzToolStripMenuItem3.Size = new System.Drawing.Size(338, 26);
            this.hzToolStripMenuItem3.Tag = "100";
            this.hzToolStripMenuItem3.Text = "100Hz";
            this.hzToolStripMenuItem3.Click += new System.EventHandler(this.toolStripMenuItem_Click);
            // 
            // hzToolStripMenuItem2
            // 
            this.hzToolStripMenuItem2.Name = "hzToolStripMenuItem2";
            this.hzToolStripMenuItem2.Size = new System.Drawing.Size(338, 26);
            this.hzToolStripMenuItem2.Tag = "25";
            this.hzToolStripMenuItem2.Text = "25Hz";
            this.hzToolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem_Click);
            // 
            // hzToolStripMenuItem1
            // 
            this.hzToolStripMenuItem1.Name = "hzToolStripMenuItem1";
            this.hzToolStripMenuItem1.Size = new System.Drawing.Size(338, 26);
            this.hzToolStripMenuItem1.Tag = "5";
            this.hzToolStripMenuItem1.Text = "5Hz";
            this.hzToolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(335, 6);
            // 
            // toolStripMenuItemRange3
            // 
            this.toolStripMenuItemRange3.Enabled = false;
            this.toolStripMenuItemRange3.Name = "toolStripMenuItemRange3";
            this.toolStripMenuItemRange3.Size = new System.Drawing.Size(338, 26);
            this.toolStripMenuItemRange3.Tag = "500";
            this.toolStripMenuItemRange3.Text = "Range: +-0.25V";
            this.toolStripMenuItemRange3.Click += new System.EventHandler(this.toolStripMenuItemRange_Click);
            // 
            // toolStripMenuItemRange1
            // 
            this.toolStripMenuItemRange1.Checked = true;
            this.toolStripMenuItemRange1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItemRange1.Name = "toolStripMenuItemRange1";
            this.toolStripMenuItemRange1.Size = new System.Drawing.Size(338, 26);
            this.toolStripMenuItemRange1.Tag = "5000";
            this.toolStripMenuItemRange1.Text = "Range: +-2.5V";
            this.toolStripMenuItemRange1.Click += new System.EventHandler(this.toolStripMenuItemRange_Click);
            // 
            // toolStripMenuItemRange2
            // 
            this.toolStripMenuItemRange2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemRange25VonlyforCurrent});
            this.toolStripMenuItemRange2.Name = "toolStripMenuItemRange2";
            this.toolStripMenuItemRange2.Size = new System.Drawing.Size(338, 26);
            this.toolStripMenuItemRange2.Tag = "50000";
            this.toolStripMenuItemRange2.Text = "Range: +-25V";
            this.toolStripMenuItemRange2.Click += new System.EventHandler(this.toolStripMenuItemRange_Click);
            // 
            // toolStripMenuItemRange25VonlyforCurrent
            // 
            this.toolStripMenuItemRange25VonlyforCurrent.Checked = true;
            this.toolStripMenuItemRange25VonlyforCurrent.CheckOnClick = true;
            this.toolStripMenuItemRange25VonlyforCurrent.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItemRange25VonlyforCurrent.Name = "toolStripMenuItemRange25VonlyforCurrent";
            this.toolStripMenuItemRange25VonlyforCurrent.Size = new System.Drawing.Size(328, 26);
            this.toolStripMenuItemRange25VonlyforCurrent.Text = "Keep the range for potantial +-2.5V";
            this.toolStripMenuItemRange25VonlyforCurrent.CheckedChanged += new System.EventHandler(this.toolStripMenuItemRange25VonlyforCurrent_CheckedChanged);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(335, 6);
            // 
            // toolStripMenuPotentioStat
            // 
            this.toolStripMenuPotentioStat.Name = "toolStripMenuPotentioStat";
            this.toolStripMenuPotentioStat.Size = new System.Drawing.Size(338, 26);
            this.toolStripMenuPotentioStat.Tag = "0";
            this.toolStripMenuPotentioStat.Text = "Potentiostat mode";
            this.toolStripMenuPotentioStat.Click += new System.EventHandler(this.toolStripMenuItemMode_Click);
            // 
            // toolStripMenuGalvanoStat
            // 
            this.toolStripMenuGalvanoStat.Name = "toolStripMenuGalvanoStat";
            this.toolStripMenuGalvanoStat.Size = new System.Drawing.Size(338, 26);
            this.toolStripMenuGalvanoStat.Tag = "1";
            this.toolStripMenuGalvanoStat.Text = "Galvanostat mode";
            this.toolStripMenuGalvanoStat.Click += new System.EventHandler(this.toolStripMenuGalvanoStat_Click);
            // 
            // toolStripMenuEIS
            // 
            this.toolStripMenuEIS.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuOpenComp,
            this.toolStripMenuShortComp,
            this.toolStripSeparator10,
            this.toolStripMenuClearComp,
            this.toolStripSeparator13,
            this.toolStripMenuSignalInspector});
            this.toolStripMenuEIS.Name = "toolStripMenuEIS";
            this.toolStripMenuEIS.Size = new System.Drawing.Size(338, 26);
            this.toolStripMenuEIS.Tag = "2";
            this.toolStripMenuEIS.Text = "EIS mode (potentiostatic)";
            this.toolStripMenuEIS.Click += new System.EventHandler(this.toolStripMenuEIS_Click);
            // 
            // toolStripMenuOpenComp
            // 
            this.toolStripMenuOpenComp.Name = "toolStripMenuOpenComp";
            this.toolStripMenuOpenComp.Size = new System.Drawing.Size(206, 26);
            this.toolStripMenuOpenComp.Text = "Open Comp";
            // 
            // toolStripMenuShortComp
            // 
            this.toolStripMenuShortComp.Name = "toolStripMenuShortComp";
            this.toolStripMenuShortComp.Size = new System.Drawing.Size(206, 26);
            this.toolStripMenuShortComp.Text = "Short Comp";
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(203, 6);
            // 
            // toolStripMenuClearComp
            // 
            this.toolStripMenuClearComp.Name = "toolStripMenuClearComp";
            this.toolStripMenuClearComp.Size = new System.Drawing.Size(206, 26);
            this.toolStripMenuClearComp.Text = "Clear Comp";
            this.toolStripMenuClearComp.Click += new System.EventHandler(this.toolStripMenuClearComp_Click);
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(203, 6);
            // 
            // toolStripMenuSignalInspector
            // 
            this.toolStripMenuSignalInspector.Name = "toolStripMenuSignalInspector";
            this.toolStripMenuSignalInspector.Size = new System.Drawing.Size(206, 26);
            this.toolStripMenuSignalInspector.Text = "Signal inspector...";
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(335, 6);
            // 
            // toolStripMenuItemCalibrate
            // 
            this.toolStripMenuItemCalibrate.Name = "toolStripMenuItemCalibrate";
            this.toolStripMenuItemCalibrate.Size = new System.Drawing.Size(338, 26);
            this.toolStripMenuItemCalibrate.Text = "Calibrate PocketPotentiostat...";
            this.toolStripMenuItemCalibrate.Click += new System.EventHandler(this.toolStripMenuItemCalibrate_Click);
            // 
            // toolStripButtonScan
            // 
            this.toolStripButtonScan.Enabled = false;
            this.toolStripButtonScan.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonScan.Image")));
            this.toolStripButtonScan.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonScan.Name = "toolStripButtonScan";
            this.toolStripButtonScan.Size = new System.Drawing.Size(64, 28);
            this.toolStripButtonScan.Text = "&Scan";
            this.toolStripButtonScan.Click += new System.EventHandler(this.toolStripButtonScan_Click);
            // 
            // toolStripComboBoxMethod
            // 
            this.toolStripComboBoxMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxMethod.DropDownWidth = 350;
            this.toolStripComboBoxMethod.Items.AddRange(new object[] {
            "Cyclic Voltammetry (CV)",
            "Bulk Electrolysis (CPE)",
            "Linear Scan Voltammetry (LSV)",
            "Series of RDE (CV mode)",
            "Series of RDE (LSV mode)",
            "Double Potential Step ChronoAmperometry ",
            "IR Compensation",
            "Osteryoung Square Wave Voltammetry (OSWV)",
            "Open Circut Potential"});
            this.toolStripComboBoxMethod.Name = "toolStripComboBoxMethod";
            this.toolStripComboBoxMethod.Size = new System.Drawing.Size(250, 31);
            this.toolStripComboBoxMethod.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBoxMethod_SelectedIndexChanged);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 31);
            // 
            // toolStripLabel5
            // 
            this.toolStripLabel5.Name = "toolStripLabel5";
            this.toolStripLabel5.Size = new System.Drawing.Size(102, 28);
            this.toolStripLabel5.Text = "Current range:";
            // 
            // toolStripComboBoxRange
            // 
            this.toolStripComboBoxRange.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxRange.DropDownWidth = 200;
            this.toolStripComboBoxRange.Items.AddRange(new object[] {
            "+-20 mA",
            "+-2 mA",
            "+-200 uA",
            "+-200 uA (raw output)",
            "+-20 uA",
            "+- 2 uA"});
            this.toolStripComboBoxRange.Name = "toolStripComboBoxRange";
            this.toolStripComboBoxRange.Size = new System.Drawing.Size(140, 31);
            this.toolStripComboBoxRange.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBoxRange_SelectedIndexChanged);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelStatus,
            this.toolStripStatusLabelCycle,
            this.toolStripStatusLabelModelOfPicoscope,
            this.toolStripStatusLabel2,
            this.toolStripStatusLabelFileName,
            this.toolStripStatusCursor,
            this.toolStripStatusCurrentEandI});
            this.statusStrip1.Location = new System.Drawing.Point(0, 393);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusStrip1.ShowItemToolTips = true;
            this.statusStrip1.Size = new System.Drawing.Size(1349, 26);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabelStatus
            // 
            this.toolStripStatusLabelStatus.Name = "toolStripStatusLabelStatus";
            this.toolStripStatusLabelStatus.Size = new System.Drawing.Size(50, 20);
            this.toolStripStatusLabelStatus.Text = "Ready";
            // 
            // toolStripStatusLabelCycle
            // 
            this.toolStripStatusLabelCycle.Name = "toolStripStatusLabelCycle";
            this.toolStripStatusLabelCycle.Size = new System.Drawing.Size(67, 20);
            this.toolStripStatusLabelCycle.Text = "(cycle: 1)";
            // 
            // toolStripStatusLabelModelOfPicoscope
            // 
            this.toolStripStatusLabelModelOfPicoscope.Name = "toolStripStatusLabelModelOfPicoscope";
            this.toolStripStatusLabelModelOfPicoscope.Size = new System.Drawing.Size(114, 20);
            this.toolStripStatusLabelModelOfPicoscope.Text = "(not connected)";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(69, 20);
            this.toolStripStatusLabel2.Text = "saving to";
            // 
            // toolStripStatusLabelFileName
            // 
            this.toolStripStatusLabelFileName.Name = "toolStripStatusLabelFileName";
            this.toolStripStatusLabelFileName.Size = new System.Drawing.Size(132, 20);
            this.toolStripStatusLabelFileName.Text = " (not specified yet)";
            // 
            // toolStripStatusCursor
            // 
            this.toolStripStatusCursor.Name = "toolStripStatusCursor";
            this.toolStripStatusCursor.Size = new System.Drawing.Size(114, 20);
            this.toolStripStatusCursor.Text = "(0 s, 0 mV, 0 uA)";
            // 
            // toolStripStatusCurrentEandI
            // 
            this.toolStripStatusCurrentEandI.Name = "toolStripStatusCurrentEandI";
            this.toolStripStatusCurrentEandI.Size = new System.Drawing.Size(783, 20);
            this.toolStripStatusCurrentEandI.Spring = true;
            this.toolStripStatusCurrentEandI.Text = "(0 s, 0 mV, 0 uA)";
            this.toolStripStatusCurrentEandI.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // toolStripContainer1
            // 
            this.toolStripContainer1.BottomToolStripPanelVisible = false;
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.chartVoltammogram);
            this.toolStripContainer1.ContentPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(1349, 331);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.LeftToolStripPanelVisible = false;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.RightToolStripPanelVisible = false;
            this.toolStripContainer1.Size = new System.Drawing.Size(1349, 393);
            this.toolStripContainer1.TabIndex = 4;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip2);
            this.toolStripContainer1.TopToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            // 
            // chartVoltammogram
            // 
            this.chartVoltammogram.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.AxisX.MajorGrid.Enabled = false;
            chartArea1.AxisX.MajorTickMark.Size = 3F;
            chartArea1.AxisX.MinorTickMark.Enabled = true;
            chartArea1.AxisX.ScrollBar.Size = 20D;
            chartArea1.AxisX.Title = "Time / s";
            chartArea1.AxisX.TitleFont = new System.Drawing.Font("Arial", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea1.AxisX2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea1.AxisX2.IsLogarithmic = true;
            chartArea1.AxisX2.IsStartedFromZero = false;
            chartArea1.AxisX2.MajorGrid.Enabled = false;
            chartArea1.AxisX2.MajorTickMark.Interval = 1D;
            chartArea1.AxisX2.MajorTickMark.Size = 3F;
            chartArea1.AxisX2.Maximum = 1000000D;
            chartArea1.AxisX2.Minimum = 1D;
            chartArea1.AxisX2.MinorTickMark.Enabled = true;
            chartArea1.AxisX2.MinorTickMark.Interval = 1D;
            chartArea1.AxisX2.TitleFont = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea1.AxisY.IsStartedFromZero = false;
            chartArea1.AxisY.LabelStyle.ForeColor = System.Drawing.Color.Blue;
            chartArea1.AxisY.MajorGrid.Enabled = false;
            chartArea1.AxisY.MajorGrid.LineColor = System.Drawing.Color.Blue;
            chartArea1.AxisY.MajorTickMark.LineColor = System.Drawing.Color.Blue;
            chartArea1.AxisY.MinorTickMark.Size = 0.5F;
            chartArea1.AxisY.ScrollBar.Size = 20D;
            chartArea1.AxisY.Title = "Potential / mV";
            chartArea1.AxisY.TitleFont = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea1.AxisY.TitleForeColor = System.Drawing.Color.Blue;
            chartArea1.AxisY2.IsStartedFromZero = false;
            chartArea1.AxisY2.LabelStyle.ForeColor = System.Drawing.Color.Red;
            chartArea1.AxisY2.MajorGrid.Enabled = false;
            chartArea1.AxisY2.MajorTickMark.Interval = 0D;
            chartArea1.AxisY2.MajorTickMark.LineColor = System.Drawing.Color.Red;
            chartArea1.AxisY2.ScrollBar.Size = 20D;
            chartArea1.AxisY2.Title = "Current / uA";
            chartArea1.AxisY2.TitleFont = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea1.AxisY2.TitleForeColor = System.Drawing.Color.Red;
            chartArea1.BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            chartArea1.CursorX.Interval = 0.01D;
            chartArea1.CursorX.IsUserEnabled = true;
            chartArea1.CursorX.IsUserSelectionEnabled = true;
            chartArea1.CursorX.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.CursorY.Interval = 0.01D;
            chartArea1.CursorY.IsUserEnabled = true;
            chartArea1.CursorY.IsUserSelectionEnabled = true;
            chartArea1.CursorY.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.Name = "ChartArea1";
            this.chartVoltammogram.ChartAreas.Add(chartArea1);
            this.chartVoltammogram.ContextMenuStrip = this.contextMenuStrip1;
            legend1.Enabled = false;
            legend1.Name = "Legend1";
            this.chartVoltammogram.Legends.Add(legend1);
            this.chartVoltammogram.Location = new System.Drawing.Point(0, 0);
            this.chartVoltammogram.Margin = new System.Windows.Forms.Padding(0);
            this.chartVoltammogram.Name = "chartVoltammogram";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series2.Color = System.Drawing.Color.Blue;
            series2.Legend = "Legend1";
            series2.Name = "Series2";
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series3.Color = System.Drawing.Color.Red;
            series3.Legend = "Legend1";
            series3.Name = "Series3";
            series3.YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            series4.ChartArea = "ChartArea1";
            series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series4.Legend = "Legend1";
            series4.Name = "Series4";
            series5.ChartArea = "ChartArea1";
            series5.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series5.Legend = "Legend1";
            series5.Name = "Series5";
            series6.ChartArea = "ChartArea1";
            series6.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            series6.Legend = "Legend1";
            series6.MarkerBorderColor = System.Drawing.Color.White;
            series6.MarkerBorderWidth = 0;
            series6.MarkerColor = System.Drawing.Color.Black;
            series6.MarkerSize = 4;
            series6.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
            series6.Name = "Series6";
            series6.YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            series7.ChartArea = "ChartArea1";
            series7.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series7.Color = System.Drawing.Color.Red;
            series7.Legend = "Legend1";
            series7.Name = "Series7_for_EIS_dB";
            series7.Points.Add(dataPoint1);
            series7.XAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            series7.YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            this.chartVoltammogram.Series.Add(series1);
            this.chartVoltammogram.Series.Add(series2);
            this.chartVoltammogram.Series.Add(series3);
            this.chartVoltammogram.Series.Add(series4);
            this.chartVoltammogram.Series.Add(series5);
            this.chartVoltammogram.Series.Add(series6);
            this.chartVoltammogram.Series.Add(series7);
            this.chartVoltammogram.Size = new System.Drawing.Size(1349, 331);
            this.chartVoltammogram.TabIndex = 1;
            this.chartVoltammogram.Text = "chart1";
            this.chartVoltammogram.CursorPositionChanged += new System.EventHandler<System.Windows.Forms.DataVisualization.Charting.CursorEventArgs>(this.chartVoltammogram_CursorPositionChanged);
            this.chartVoltammogram.SelectionRangeChanging += new System.EventHandler<System.Windows.Forms.DataVisualization.Charting.CursorEventArgs>(this.chartVoltammogram_SelectionRangeChanging);
            this.chartVoltammogram.SelectionRangeChanged += new System.EventHandler<System.Windows.Forms.DataVisualization.Charting.CursorEventArgs>(this.chartVoltammogram_SelectionRangeChanged);
            this.chartVoltammogram.AxisViewChanging += new System.EventHandler<System.Windows.Forms.DataVisualization.Charting.ViewEventArgs>(this.chartVoltammogram_AxisViewChanging);
            this.chartVoltammogram.AxisViewChanged += new System.EventHandler<System.Windows.Forms.DataVisualization.Charting.ViewEventArgs>(this.chartVoltammogram_AxisViewChanged);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contextMenuItemUndoZoom});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(159, 28);
            // 
            // contextMenuItemUndoZoom
            // 
            this.contextMenuItemUndoZoom.Name = "contextMenuItemUndoZoom";
            this.contextMenuItemUndoZoom.Size = new System.Drawing.Size(158, 24);
            this.contextMenuItemUndoZoom.Text = "Undo Zoom";
            this.contextMenuItemUndoZoom.Click += new System.EventHandler(this.contextMenuItemUndoZoom_Click);
            // 
            // toolStrip2
            // 
            this.toolStrip2.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip2.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.toolStripTextBoxInitialV,
            this.toolStripComboBoxReferenceForInitialPotential,
            this.toolStripLabel6,
            this.toolStripTextBoxFinalV,
            this.toolStripLabel2,
            this.toolStripTextBoxVertexV,
            this.toolStripLabel3,
            this.toolStripTextBoxScanrate,
            this.toolStripLabel7,
            this.toolStripTextBoxScanrate2,
            this.toolStripLabel8,
            this.toolStripTextBoxStep,
            this.toolStripLabel4,
            this.toolStripTextBoxRepeat});
            this.toolStrip2.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStrip2.Location = new System.Drawing.Point(4, 31);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.toolStrip2.Size = new System.Drawing.Size(861, 31);
            this.toolStrip2.TabIndex = 3;
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(85, 28);
            this.toolStripLabel1.Text = "Initial [mV]:";
            // 
            // toolStripTextBoxInitialV
            // 
            this.toolStripTextBoxInitialV.Font = new System.Drawing.Font("Yu Gothic UI", 9F);
            this.toolStripTextBoxInitialV.Name = "toolStripTextBoxInitialV";
            this.toolStripTextBoxInitialV.Size = new System.Drawing.Size(70, 31);
            this.toolStripTextBoxInitialV.Text = "0";
            // 
            // toolStripComboBoxReferenceForInitialPotential
            // 
            this.toolStripComboBoxReferenceForInitialPotential.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxReferenceForInitialPotential.DropDownWidth = 75;
            this.toolStripComboBoxReferenceForInitialPotential.Items.AddRange(new object[] {
            "vs Ref",
            "vs OCP"});
            this.toolStripComboBoxReferenceForInitialPotential.Name = "toolStripComboBoxReferenceForInitialPotential";
            this.toolStripComboBoxReferenceForInitialPotential.Size = new System.Drawing.Size(90, 31);
            this.toolStripComboBoxReferenceForInitialPotential.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBoxReferenceForInitialPotential_SelectedIndexChanged);
            // 
            // toolStripLabel6
            // 
            this.toolStripLabel6.Name = "toolStripLabel6";
            this.toolStripLabel6.Size = new System.Drawing.Size(76, 25);
            this.toolStripLabel6.Text = "Final [mV]";
            this.toolStripLabel6.Visible = false;
            // 
            // toolStripTextBoxFinalV
            // 
            this.toolStripTextBoxFinalV.Font = new System.Drawing.Font("Yu Gothic UI", 9F);
            this.toolStripTextBoxFinalV.Name = "toolStripTextBoxFinalV";
            this.toolStripTextBoxFinalV.Size = new System.Drawing.Size(70, 28);
            this.toolStripTextBoxFinalV.Text = "-1000";
            this.toolStripTextBoxFinalV.Visible = false;
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(89, 28);
            this.toolStripLabel2.Text = "Vertex [mV]:";
            // 
            // toolStripTextBoxVertexV
            // 
            this.toolStripTextBoxVertexV.Font = new System.Drawing.Font("Yu Gothic UI", 9F);
            this.toolStripTextBoxVertexV.Name = "toolStripTextBoxVertexV";
            this.toolStripTextBoxVertexV.Size = new System.Drawing.Size(70, 31);
            this.toolStripTextBoxVertexV.Text = "500";
            this.toolStripTextBoxVertexV.Validating += new System.ComponentModel.CancelEventHandler(this.toolStripTextBoxVertexV_Validating);
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(121, 28);
            this.toolStripLabel3.Text = "Scan rate [mV/s]:";
            // 
            // toolStripTextBoxScanrate
            // 
            this.toolStripTextBoxScanrate.Font = new System.Drawing.Font("Yu Gothic UI", 9F);
            this.toolStripTextBoxScanrate.Name = "toolStripTextBoxScanrate";
            this.toolStripTextBoxScanrate.Size = new System.Drawing.Size(70, 31);
            this.toolStripTextBoxScanrate.Text = "100";
            this.toolStripTextBoxScanrate.Validating += new System.ComponentModel.CancelEventHandler(this.toolStripTextBoxScanrate_Validating);
            // 
            // toolStripLabel7
            // 
            this.toolStripLabel7.Name = "toolStripLabel7";
            this.toolStripLabel7.Size = new System.Drawing.Size(85, 25);
            this.toolStripLabel7.Text = "Scan rate 2:";
            this.toolStripLabel7.Visible = false;
            // 
            // toolStripTextBoxScanrate2
            // 
            this.toolStripTextBoxScanrate2.Font = new System.Drawing.Font("Yu Gothic UI", 9F);
            this.toolStripTextBoxScanrate2.Name = "toolStripTextBoxScanrate2";
            this.toolStripTextBoxScanrate2.Size = new System.Drawing.Size(70, 28);
            this.toolStripTextBoxScanrate2.Visible = false;
            // 
            // toolStripLabel8
            // 
            this.toolStripLabel8.Name = "toolStripLabel8";
            this.toolStripLabel8.Size = new System.Drawing.Size(42, 28);
            this.toolStripLabel8.Text = "Step:";
            // 
            // toolStripTextBoxStep
            // 
            this.toolStripTextBoxStep.Font = new System.Drawing.Font("Yu Gothic UI", 9F);
            this.toolStripTextBoxStep.Name = "toolStripTextBoxStep";
            this.toolStripTextBoxStep.Size = new System.Drawing.Size(70, 31);
            this.toolStripTextBoxStep.Text = "64";
            // 
            // toolStripLabel4
            // 
            this.toolStripLabel4.Name = "toolStripLabel4";
            this.toolStripLabel4.Size = new System.Drawing.Size(59, 28);
            this.toolStripLabel4.Text = "Repeat:";
            // 
            // toolStripTextBoxRepeat
            // 
            this.toolStripTextBoxRepeat.Font = new System.Drawing.Font("Yu Gothic UI", 9F);
            this.toolStripTextBoxRepeat.Name = "toolStripTextBoxRepeat";
            this.toolStripTextBoxRepeat.Size = new System.Drawing.Size(70, 31);
            this.toolStripTextBoxRepeat.Text = "1";
            this.toolStripTextBoxRepeat.Validating += new System.ComponentModel.CancelEventHandler(this.toolStripTextBoxRepeat_Validating);
            // 
            // timerCurrentEandI
            // 
            this.timerCurrentEandI.Interval = 1000;
            this.timerCurrentEandI.Tick += new System.EventHandler(this.timerCurrentEandI_Tick);
            // 
            // Potentiostat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1349, 419);
            this.Controls.Add(this.toolStripContainer1);
            this.Controls.Add(this.statusStrip1);
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.Name = "Potentiostat";
            this.Text = "Voltammogrammer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Potentiostat_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Potentiostat_FormClosed);
            this.Load += new System.EventHandler(this.Potentiostat_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Potentiostat_KeyPress);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartVoltammogram)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.ComponentModel.BackgroundWorker backgroundWorkerCV;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxMethod;
        private System.Windows.Forms.ToolStripPanel BottomToolStripPanel;
        private System.Windows.Forms.ToolStripPanel TopToolStripPanel;
        private System.Windows.Forms.ToolStripPanel RightToolStripPanel;
        private System.Windows.Forms.ToolStripPanel LeftToolStripPanel;
        private System.Windows.Forms.ToolStripContentPanel ContentPanel;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelStatus;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelFileName;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelModelOfPicoscope;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusCursor;
        private System.Windows.Forms.ToolStripSplitButton toolStripButtonConnect;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartVoltammogram;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxSerialPort;
        private System.Windows.Forms.ToolStripSplitButton toolStripButtonRecord;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOCV;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRDE;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxRDESpeed;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPurge;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSaveAvaragedData;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusCurrentEandI;
        private System.Windows.Forms.Timer timerCurrentEandI;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxInitialV;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxVertexV;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxScanrate;
        private System.Windows.Forms.ToolStripLabel toolStripLabel4;
        private System.Windows.Forms.ToolStripButton toolStripButtonScan;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem hzToolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem hzToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem hzToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem hzToolStripMenuItem3;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelCycle;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRange1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRange2;
        private System.Windows.Forms.ToolStripMenuItem hzToolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem hzToolStripMenuItem7;
        private System.Windows.Forms.ToolStripMenuItem hzToolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRange3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuEIS;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuGalvanoStat;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem hzToolStripMenuItemAuto;
        private System.Windows.Forms.ToolStripMenuItem hzToolStripMenuItem8;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCalibrate;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuOpenComp;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuShortComp;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuClearComp;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDebug;
        private System.Windows.Forms.ToolStripLabel toolStripLabel5;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxRange;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxRepeat;
        private System.Windows.Forms.ToolStripMenuItem hzToolStripMenuItemCustom;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxFreqOfAcquisition;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem contextMenuItemUndoZoom;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRange25VonlyforCurrent;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFiltering60Hz;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFiltering50Hz;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemConfigure;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxReferenceForInitialPotential;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuPotentioStat;
        private System.Windows.Forms.ToolStripLabel toolStripLabel6;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxFinalV;
        private System.Windows.Forms.ToolStripLabel toolStripLabel7;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxScanrate2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel8;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxStep;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator13;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuSignalInspector;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator14;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDelay;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxDelayTime;
    }
}

