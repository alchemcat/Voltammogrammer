namespace Voltammogrammer
{
    partial class formVoltammogram
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formVoltammogram));
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxSeries = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripComboBoxSeriesColor = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripComboBoxSeriesLineStyle = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripComboBoxSeriesLineWidth = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxSeriesRefPotential = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel4 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboSeriesShiftY = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel5 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboSeriesScaleY = new System.Windows.Forms.ToolStripComboBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusCursor = new System.Windows.Forms.ToolStripStatusLabel();
            this.chartVoltammogram = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.contextMenuItemUndoZoom = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip3 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxAxisY = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripComboBoxAxisX = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripComboBoxRef = new System.Windows.Forms.ToolStripComboBox();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonNewWindow = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonLoad = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripMenuItemLoadAsVoltammogram = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemAsChronoamperogram = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButtonSave = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripMenuItemExportCSV = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemExportSinglePlot = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripDropDownButtonCopy = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButtonShow = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItemSetXRange = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemFont = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemShowInformation = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemUndoZoom = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartVoltammogram)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.toolStrip3.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.SuspendLayout();
            //
            // toolStripContainer1
            //
            //
            // toolStripContainer1.BottomToolStripPanel
            //
            this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.toolStrip1);
            this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.statusStrip1);
            //
            // toolStripContainer1.ContentPanel
            //
            this.toolStripContainer1.ContentPanel.Controls.Add(this.chartVoltammogram);
            this.toolStripContainer1.ContentPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(1245, 362);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(1245, 470);
            this.toolStripContainer1.TabIndex = 4;
            this.toolStripContainer1.Text = "toolStripContainer1";
            //
            // toolStripContainer1.TopToolStripPanel
            //
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip2);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip3);
            //
            // toolStrip1
            //
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel3,
            this.toolStripComboBoxSeries,
            this.toolStripSeparator3,
            this.toolStripComboBoxSeriesColor,
            this.toolStripComboBoxSeriesLineStyle,
            this.toolStripComboBoxSeriesLineWidth,
            this.toolStripSeparator4,
            this.toolStripLabel1,
            this.toolStripComboBoxSeriesRefPotential,
            this.toolStripLabel4,
            this.toolStripComboSeriesShiftY,
            this.toolStripLabel5,
            this.toolStripComboSeriesScaleY});
            this.toolStrip1.Location = new System.Drawing.Point(8, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1103, 28);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            //
            // toolStripLabel3
            //
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(38, 25);
            this.toolStripLabel3.Text = "Plot:";
            //
            // toolStripComboBoxSeries
            //
            this.toolStripComboBoxSeries.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxSeries.DropDownWidth = 400;
            this.toolStripComboBoxSeries.MaxDropDownItems = 100;
            this.toolStripComboBoxSeries.Name = "toolStripComboBoxSeries";
            this.toolStripComboBoxSeries.Size = new System.Drawing.Size(121, 28);
            this.toolStripComboBoxSeries.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBoxSeries_SelectedIndexChanged);
            //
            // toolStripSeparator3
            //
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 28);
            //
            // toolStripComboBoxSeriesColor
            //
            this.toolStripComboBoxSeriesColor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxSeriesColor.Name = "toolStripComboBoxSeriesColor";
            this.toolStripComboBoxSeriesColor.Size = new System.Drawing.Size(121, 28);
            this.toolStripComboBoxSeriesColor.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBoxSeriesColor_SelectedIndexChanged);
            //
            // toolStripComboBoxSeriesLineStyle
            //
            this.toolStripComboBoxSeriesLineStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxSeriesLineStyle.Name = "toolStripComboBoxSeriesLineStyle";
            this.toolStripComboBoxSeriesLineStyle.Size = new System.Drawing.Size(121, 28);
            this.toolStripComboBoxSeriesLineStyle.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBoxSeriesLineStyle_SelectedIndexChanged);
            //
            // toolStripComboBoxSeriesLineWidth
            //
            this.toolStripComboBoxSeriesLineWidth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxSeriesLineWidth.Name = "toolStripComboBoxSeriesLineWidth";
            this.toolStripComboBoxSeriesLineWidth.Size = new System.Drawing.Size(121, 28);
            this.toolStripComboBoxSeriesLineWidth.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBoxSeriesLineWidth_SelectedIndexChanged);
            //
            // toolStripSeparator4
            //
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 28);
            //
            // toolStripLabel1
            //
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(95, 25);
            this.toolStripLabel1.Text = "Fc(+/0) [mV]:";
            //
            // toolStripComboBoxSeriesRefPotential
            //
            this.toolStripComboBoxSeriesRefPotential.Name = "toolStripComboBoxSeriesRefPotential";
            this.toolStripComboBoxSeriesRefPotential.Size = new System.Drawing.Size(121, 28);
            this.toolStripComboBoxSeriesRefPotential.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBoxSeriesRefPotential_SelectedIndexChanged);
            this.toolStripComboBoxSeriesRefPotential.TextUpdate += new System.EventHandler(this.toolStripComboBoxSeriesRefPotential_TextUpdate);
            this.toolStripComboBoxSeriesRefPotential.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.toolStripComboBoxSeriesRefPotential_KeyPress);
            this.toolStripComboBoxSeriesRefPotential.Validating += new System.ComponentModel.CancelEventHandler(this.toolStripComboBoxSeriesRefPotential_Validating);
            this.toolStripComboBoxSeriesRefPotential.TextChanged += new System.EventHandler(this.toolStripComboBoxSeriesRefPotential_TextChanged);
            //
            // toolStripLabel4
            //
            this.toolStripLabel4.Name = "toolStripLabel4";
            this.toolStripLabel4.Size = new System.Drawing.Size(58, 25);
            this.toolStripLabel4.Text = "+I [uA]:";
            //
            // toolStripComboSeriesShiftY
            //
            this.toolStripComboSeriesShiftY.Name = "toolStripComboSeriesShiftY";
            this.toolStripComboSeriesShiftY.Size = new System.Drawing.Size(121, 28);
            this.toolStripComboSeriesShiftY.SelectedIndexChanged += new System.EventHandler(this.toolStripComboSeriesShiftY_SelectedIndexChanged);
            this.toolStripComboSeriesShiftY.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.toolStripComboSeriesShiftY_KeyPress);
            //
            // toolStripLabel5
            //
            this.toolStripLabel5.Name = "toolStripLabel5";
            this.toolStripLabel5.Size = new System.Drawing.Size(27, 25);
            this.toolStripLabel5.Text = "x I:";
            //
            // toolStripComboSeriesScaleY
            //
            this.toolStripComboSeriesScaleY.Name = "toolStripComboSeriesScaleY";
            this.toolStripComboSeriesScaleY.Size = new System.Drawing.Size(121, 28);
            this.toolStripComboSeriesScaleY.SelectedIndexChanged += new System.EventHandler(this.toolStripComboSeriesScaleY_SelectedIndexChanged);
            this.toolStripComboSeriesScaleY.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.toolStripComboSeriesScaleY_KeyPress);
            //
            // statusStrip1
            //
            this.statusStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusCursor});
            this.statusStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.statusStrip1.Location = new System.Drawing.Point(0, 28);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1245, 25);
            this.statusStrip1.TabIndex = 0;
            //
            // toolStripStatusCursor
            //
            this.toolStripStatusCursor.Name = "toolStripStatusCursor";
            this.toolStripStatusCursor.Size = new System.Drawing.Size(83, 20);
            this.toolStripStatusCursor.Text = "(...mV, ...uA)";
            //
            // chartVoltammogram
            //
            this.chartVoltammogram.AllowDrop = true;
            chartArea1.AxisX.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea1.AxisX.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisX.IsLabelAutoFit = false;
            chartArea1.AxisX.IsStartedFromZero = false;
            chartArea1.AxisX.LabelAutoFitMaxFontSize = 15;
            chartArea1.AxisX.LabelAutoFitMinFontSize = 9;
            chartArea1.AxisX.LabelStyle.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea1.AxisX.LabelStyle.Interval = 100D;
            chartArea1.AxisX.LabelStyle.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisX.LabelStyle.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisX.MajorGrid.Enabled = false;
            chartArea1.AxisX.MajorTickMark.Interval = 500D;
            chartArea1.AxisX.MajorTickMark.IntervalOffset = 0D;
            chartArea1.AxisX.MajorTickMark.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisX.MajorTickMark.Size = 3F;
            chartArea1.AxisX.MinorTickMark.Enabled = true;
            chartArea1.AxisX.MinorTickMark.Interval = 100D;
            chartArea1.AxisX.MinorTickMark.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisX.ScrollBar.Enabled = false;
            chartArea1.AxisX.ScrollBar.Size = 20D;
            chartArea1.AxisX.Title = "Potential / mV vs Fc(+/0)";
            chartArea1.AxisX.TitleFont = new System.Drawing.Font("Arial", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea1.AxisX2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea1.AxisX2.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisX2.IsStartedFromZero = false;
            chartArea1.AxisX2.LabelStyle.Enabled = false;
            chartArea1.AxisX2.LabelStyle.Interval = 0D;
            chartArea1.AxisX2.LabelStyle.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisX2.MajorGrid.Enabled = false;
            chartArea1.AxisX2.MajorTickMark.Interval = 500D;
            chartArea1.AxisX2.MajorTickMark.IntervalOffset = 0D;
            chartArea1.AxisX2.MajorTickMark.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisX2.MajorTickMark.Size = 3F;
            chartArea1.AxisX2.MinorTickMark.Enabled = true;
            chartArea1.AxisX2.MinorTickMark.Interval = 100D;
            chartArea1.AxisX2.MinorTickMark.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisX2.ScrollBar.Enabled = false;
            chartArea1.AxisY.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea1.AxisY.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisY.IsLabelAutoFit = false;
            chartArea1.AxisY.IsStartedFromZero = false;
            chartArea1.AxisY.LabelAutoFitMaxFontSize = 15;
            chartArea1.AxisY.LabelAutoFitMinFontSize = 9;
            chartArea1.AxisY.LabelAutoFitStyle = ((System.Windows.Forms.DataVisualization.Charting.LabelAutoFitStyles)((((System.Windows.Forms.DataVisualization.Charting.LabelAutoFitStyles.IncreaseFont | System.Windows.Forms.DataVisualization.Charting.LabelAutoFitStyles.DecreaseFont)
            | System.Windows.Forms.DataVisualization.Charting.LabelAutoFitStyles.LabelsAngleStep30)
            | System.Windows.Forms.DataVisualization.Charting.LabelAutoFitStyles.WordWrap)));
            chartArea1.AxisY.LabelStyle.Font = new System.Drawing.Font("Arial", 12F);
            chartArea1.AxisY.LabelStyle.Interval = 20D;
            chartArea1.AxisY.LabelStyle.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisY.MajorGrid.Enabled = false;
            chartArea1.AxisY.MajorTickMark.Interval = 20D;
            chartArea1.AxisY.MajorTickMark.IntervalOffset = 0D;
            chartArea1.AxisY.MajorTickMark.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisY.MinorTickMark.Enabled = true;
            chartArea1.AxisY.MinorTickMark.Interval = 10D;
            chartArea1.AxisY.MinorTickMark.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisY.MinorTickMark.Size = 0.6F;
            chartArea1.AxisY.ScrollBar.Enabled = false;
            chartArea1.AxisY.ScrollBar.Size = 20D;
            chartArea1.AxisY.Title = "Current / μA";
            chartArea1.AxisY.TitleFont = new System.Drawing.Font("Arial", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea1.AxisY2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea1.AxisY2.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisY2.IsStartedFromZero = false;
            chartArea1.AxisY2.LabelStyle.Enabled = false;
            chartArea1.AxisY2.LabelStyle.Interval = 0D;
            chartArea1.AxisY2.LabelStyle.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisY2.MajorGrid.Enabled = false;
            chartArea1.AxisY2.MajorTickMark.Interval = 20D;
            chartArea1.AxisY2.MajorTickMark.IntervalOffset = 0D;
            chartArea1.AxisY2.MajorTickMark.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisY2.MinorTickMark.Enabled = true;
            chartArea1.AxisY2.MinorTickMark.Interval = 10D;
            chartArea1.AxisY2.MinorTickMark.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisY2.MinorTickMark.Size = 0.6F;
            chartArea1.AxisY2.ScrollBar.Enabled = false;
            chartArea1.AxisY2.TitleFont = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            this.chartVoltammogram.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartVoltammogram.IsSoftShadows = false;
            legend1.DockedToChartArea = "ChartArea1";
            legend1.IsDockedInsideChartArea = false;
            legend1.Name = "Legend1";
            this.chartVoltammogram.Legends.Add(legend1);
            this.chartVoltammogram.Location = new System.Drawing.Point(0, 0);
            this.chartVoltammogram.Margin = new System.Windows.Forms.Padding(0);
            this.chartVoltammogram.Name = "chartVoltammogram";
            this.chartVoltammogram.Size = new System.Drawing.Size(1245, 362);
            this.chartVoltammogram.SuppressExceptions = true;
            this.chartVoltammogram.TabIndex = 3;
            this.chartVoltammogram.Text = "chart1";
            this.chartVoltammogram.CursorPositionChanged += new System.EventHandler<System.Windows.Forms.DataVisualization.Charting.CursorEventArgs>(this.chartVoltammogram_CursorPositionChanged);
            this.chartVoltammogram.AxisViewChanging += new System.EventHandler<System.Windows.Forms.DataVisualization.Charting.ViewEventArgs>(this.chartVoltammogram_AxisViewChanging);
            this.chartVoltammogram.AxisViewChanged += new System.EventHandler<System.Windows.Forms.DataVisualization.Charting.ViewEventArgs>(this.chartVoltammogram_AxisViewChanged);
            this.chartVoltammogram.DragDrop += new System.Windows.Forms.DragEventHandler(this.chartVoltammogram_DragDrop);
            this.chartVoltammogram.DragEnter += new System.Windows.Forms.DragEventHandler(this.chartVoltammogram_DragEnter);
            this.chartVoltammogram.DoubleClick += new System.EventHandler(this.chartVoltammogram_DoubleClick);
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
            // toolStrip3
            //
            this.toolStrip3.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip3.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel2,
            this.toolStripComboBoxAxisY,
            this.toolStripComboBoxAxisX,
            this.toolStripComboBoxRef});
            this.toolStrip3.Location = new System.Drawing.Point(3, 0);
            this.toolStrip3.Name = "toolStrip3";
            this.toolStrip3.Size = new System.Drawing.Size(452, 28);
            this.toolStrip3.TabIndex = 5;
            //
            // toolStripLabel2
            //
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(43, 25);
            this.toolStripLabel2.Text = "Axes:";
            //
            // toolStripComboBoxAxisY
            //
            this.toolStripComboBoxAxisY.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxAxisY.DropDownWidth = 120;
            this.toolStripComboBoxAxisY.Items.AddRange(new object[] {
            "Current / μA",
            "Current / mA",
            "Coulomb / mC",
            "Coulomb / C",
            "Potential / mV",
            "Current / nA",
            "Im[Z] / ohm"});
            this.toolStripComboBoxAxisY.Name = "toolStripComboBoxAxisY";
            this.toolStripComboBoxAxisY.Size = new System.Drawing.Size(130, 28);
            this.toolStripComboBoxAxisY.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBoxAxisY_SelectedIndexChanged);
            //
            // toolStripComboBoxAxisX
            //
            this.toolStripComboBoxAxisX.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxAxisX.DropDownWidth = 120;
            this.toolStripComboBoxAxisX.Items.AddRange(new object[] {
            "Potential / mV",
            "Potential / V",
            "Time / s",
            "Time / h",
            "Re[Z] / ohm"});
            this.toolStripComboBoxAxisX.Name = "toolStripComboBoxAxisX";
            this.toolStripComboBoxAxisX.Size = new System.Drawing.Size(130, 28);
            this.toolStripComboBoxAxisX.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBoxAxisX_SelectedIndexChanged);
            //
            // toolStripComboBoxRef
            //
            this.toolStripComboBoxRef.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxRef.Items.AddRange(new object[] {
            "",
            "vs Fc(+/0)",
            "vs Ag/AgCl",
            "vs SCE",
            "vs Ag(+/0)"});
            this.toolStripComboBoxRef.Name = "toolStripComboBoxRef";
            this.toolStripComboBoxRef.Size = new System.Drawing.Size(90, 28);
            this.toolStripComboBoxRef.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBoxRef_SelectedIndexChanged);
            //
            // toolStrip2
            //
            this.toolStrip2.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip2.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonNewWindow,
            this.toolStripButtonLoad,
            this.toolStripButtonSave,
            this.toolStripDropDownButton2,
            this.toolStripDropDownButtonCopy,
            this.toolStripDropDownButtonShow,
            this.toolStripSeparator2,
            this.toolStripDropDownButton1});
            this.toolStrip2.Location = new System.Drawing.Point(3, 28);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(486, 27);
            this.toolStrip2.TabIndex = 4;
            //
            // toolStripButtonNewWindow
            //
            this.toolStripButtonNewWindow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonNewWindow.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonNewWindow.Image")));
            this.toolStripButtonNewWindow.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonNewWindow.Name = "toolStripButtonNewWindow";
            this.toolStripButtonNewWindow.Size = new System.Drawing.Size(43, 24);
            this.toolStripButtonNewWindow.Text = "New";
            this.toolStripButtonNewWindow.Click += new System.EventHandler(this.toolStripButtonNewWindow_Click);
            //
            // toolStripButtonLoad
            //
            this.toolStripButtonLoad.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonLoad.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemLoadAsVoltammogram,
            this.toolStripMenuItemAsChronoamperogram});
            this.toolStripButtonLoad.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonLoad.Image")));
            this.toolStripButtonLoad.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonLoad.Name = "toolStripButtonLoad";
            this.toolStripButtonLoad.Size = new System.Drawing.Size(61, 24);
            this.toolStripButtonLoad.Text = "Load";
            this.toolStripButtonLoad.ButtonClick += new System.EventHandler(this.toolStripMenuItemLoadAsVoltammogram_Click);
            //
            // toolStripMenuItemLoadAsVoltammogram
            //
            this.toolStripMenuItemLoadAsVoltammogram.Name = "toolStripMenuItemLoadAsVoltammogram";
            this.toolStripMenuItemLoadAsVoltammogram.Size = new System.Drawing.Size(239, 26);
            this.toolStripMenuItemLoadAsVoltammogram.Text = "As Voltammogram";
            this.toolStripMenuItemLoadAsVoltammogram.Click += new System.EventHandler(this.toolStripMenuItemLoadAsVoltammogram_Click);
            //
            // toolStripMenuItemAsChronoamperogram
            //
            this.toolStripMenuItemAsChronoamperogram.Enabled = false;
            this.toolStripMenuItemAsChronoamperogram.Name = "toolStripMenuItemAsChronoamperogram";
            this.toolStripMenuItemAsChronoamperogram.Size = new System.Drawing.Size(239, 26);
            this.toolStripMenuItemAsChronoamperogram.Text = "As Chronoamperogram";
            this.toolStripMenuItemAsChronoamperogram.Click += new System.EventHandler(this.toolStripMenuItemAsChronoamperogram_Click);
            //
            // toolStripButtonSave
            //
            this.toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonSave.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemExportCSV,
            this.toolStripMenuItemExportSinglePlot});
            this.toolStripButtonSave.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonSave.Image")));
            this.toolStripButtonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSave.Name = "toolStripButtonSave";
            this.toolStripButtonSave.Size = new System.Drawing.Size(79, 24);
            this.toolStripButtonSave.Text = "Save As";
            this.toolStripButtonSave.ButtonClick += new System.EventHandler(this.toolStripButtonSave_Click);
            //
            // toolStripMenuItemExportCSV
            //
            this.toolStripMenuItemExportCSV.Name = "toolStripMenuItemExportCSV";
            this.toolStripMenuItemExportCSV.Size = new System.Drawing.Size(386, 26);
            this.toolStripMenuItemExportCSV.Text = "Export all data as a CSV file";
            this.toolStripMenuItemExportCSV.Click += new System.EventHandler(this.toolStripMenuItemExportCSV_Click);
            //
            // toolStripMenuItemExportSinglePlot
            //
            this.toolStripMenuItemExportSinglePlot.Name = "toolStripMenuItemExportSinglePlot";
            this.toolStripMenuItemExportSinglePlot.Size = new System.Drawing.Size(386, 26);
            this.toolStripMenuItemExportSinglePlot.Text = "Export the selected plot as an EC-Lab Text file";
            this.toolStripMenuItemExportSinglePlot.Click += new System.EventHandler(this.toolStripMenuItemExportSinglePlot_Click);
            //
            // toolStripDropDownButton2
            //
            this.toolStripDropDownButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.toolStripSeparator1});
            this.toolStripDropDownButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton2.Image")));
            this.toolStripDropDownButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton2.Name = "toolStripDropDownButton2";
            this.toolStripDropDownButton2.Size = new System.Drawing.Size(67, 24);
            this.toolStripDropDownButton2.Text = "Delete";
            this.toolStripDropDownButton2.Click += new System.EventHandler(this.toolStripDropDownButton2_Click);
            //
            // toolStripMenuItem2
            //
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(112, 26);
            this.toolStripMenuItem2.Text = "(All)";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            //
            // toolStripSeparator1
            //
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(109, 6);
            //
            // toolStripDropDownButtonCopy
            //
            this.toolStripDropDownButtonCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButtonCopy.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem3});
            this.toolStripDropDownButtonCopy.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButtonCopy.Image")));
            this.toolStripDropDownButtonCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButtonCopy.Name = "toolStripDropDownButtonCopy";
            this.toolStripDropDownButtonCopy.Size = new System.Drawing.Size(62, 24);
            this.toolStripDropDownButtonCopy.Text = "Copy";
            this.toolStripDropDownButtonCopy.ButtonClick += new System.EventHandler(this.toolStripDropDownButtonCopy_ButtonClick);
            //
            // toolStripMenuItem3
            //
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(235, 26);
            this.toolStripMenuItem3.Text = "Metafile (to clipboard)";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.toolStripDropDownButtonCopy_ButtonClick);
            //
            // toolStripDropDownButtonShow
            //
            this.toolStripDropDownButtonShow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButtonShow.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemSetXRange,
            this.toolStripMenuItemFont,
            this.toolStripSeparator5,
            this.toolStripMenuItemShowInformation,
            this.toolStripSeparator6,
            this.toolStripMenuItemUndoZoom});
            this.toolStripDropDownButtonShow.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButtonShow.Image")));
            this.toolStripDropDownButtonShow.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButtonShow.Name = "toolStripDropDownButtonShow";
            this.toolStripDropDownButtonShow.Size = new System.Drawing.Size(59, 24);
            this.toolStripDropDownButtonShow.Text = "Show";
            //
            // toolStripMenuItemSetXRange
            //
            this.toolStripMenuItemSetXRange.Name = "toolStripMenuItemSetXRange";
            this.toolStripMenuItemSetXRange.Size = new System.Drawing.Size(405, 26);
            this.toolStripMenuItemSetXRange.Text = "Set the X- and Y-axes range of voltammograms...";
            this.toolStripMenuItemSetXRange.Click += new System.EventHandler(this.toolStripMenuItemSetXRange_Click);
            //
            // toolStripMenuItemFont
            //
            this.toolStripMenuItemFont.Name = "toolStripMenuItemFont";
            this.toolStripMenuItemFont.Size = new System.Drawing.Size(405, 26);
            this.toolStripMenuItemFont.Text = "Set appearances used in the main plot...";
            this.toolStripMenuItemFont.Click += new System.EventHandler(this.toolStripMenuItemFont_Click);
            //
            // toolStripSeparator5
            //
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(402, 6);
            //
            // toolStripMenuItemShowInformation
            //
            this.toolStripMenuItemShowInformation.Name = "toolStripMenuItemShowInformation";
            this.toolStripMenuItemShowInformation.Size = new System.Drawing.Size(405, 26);
            this.toolStripMenuItemShowInformation.Text = "Show Information about the selected plot... ";
            this.toolStripMenuItemShowInformation.Click += new System.EventHandler(this.toolStripMenuItemShowInformation_Click);
            //
            // toolStripSeparator6
            //
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(402, 6);
            //
            // toolStripMenuItemUndoZoom
            //
            this.toolStripMenuItemUndoZoom.Name = "toolStripMenuItemUndoZoom";
            this.toolStripMenuItemUndoZoom.Size = new System.Drawing.Size(405, 26);
            this.toolStripMenuItemUndoZoom.Text = "Undo zoom";
            this.toolStripMenuItemUndoZoom.Click += new System.EventHandler(this.toolStripMenuItemUndoZoom_Click);
            //
            // toolStripSeparator2
            //
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
            //
            // toolStripDropDownButton1
            //
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(97, 24);
            this.toolStripDropDownButton1.Text = "Calculation";
            //
            // toolStripMenuItem1
            //
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(223, 26);
            this.toolStripMenuItem1.Text = "Half wave potential...";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            //
            // openFileDialog1
            //
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Multiselect = true;
            //
            // formVoltammogram
            //
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1245, 470);
            this.Controls.Add(this.toolStripContainer1);
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "formVoltammogram";
            this.Text = "Actual voltammogram";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formVoltammogram_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.formVoltammogram_FormClosed);
            this.Load += new System.EventHandler(this.formVoltammogram_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.formVoltammogram_KeyPress);
            this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartVoltammogram)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.toolStrip3.ResumeLayout(false);
            this.toolStrip3.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        public System.Windows.Forms.DataVisualization.Charting.Chart chartVoltammogram;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusCursor;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxSeries;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxSeriesColor;
        private System.Windows.Forms.ToolStripSplitButton toolStripDropDownButtonCopy;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxSeriesLineStyle;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxSeriesLineWidth;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxSeriesRefPotential;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripButton toolStripButtonNewWindow;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ToolStrip toolStrip3;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxAxisY;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxAxisX;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private System.Windows.Forms.ToolStripSplitButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExportCSV;
        private System.Windows.Forms.ToolStripLabel toolStripLabel4;
        private System.Windows.Forms.ToolStripComboBox toolStripComboSeriesShiftY;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExportSinglePlot;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxRef;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButtonShow;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSetXRange;
        private System.Windows.Forms.ToolStripSplitButton toolStripButtonLoad;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLoadAsVoltammogram;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAsChronoamperogram;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFont;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemShowInformation;
        private System.Windows.Forms.ToolStripLabel toolStripLabel5;
        private System.Windows.Forms.ToolStripComboBox toolStripComboSeriesScaleY;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUndoZoom;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem contextMenuItemUndoZoom;
    }
}
