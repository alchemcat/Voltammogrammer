namespace Voltammogrammer
{
    partial class Export_DataAsCSV
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
            this.buttonExport = new System.Windows.Forms.Button();
            this.checkBoxDataReduction = new System.Windows.Forms.CheckBox();
            this.checkBoxForExcel = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            //
            // buttonExport
            //
            this.buttonExport.Location = new System.Drawing.Point(358, 91);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(118, 37);
            this.buttonExport.TabIndex = 0;
            this.buttonExport.Text = "Export";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
            //
            // checkBoxDataReduction
            //
            this.checkBoxDataReduction.AutoSize = true;
            this.checkBoxDataReduction.Location = new System.Drawing.Point(25, 29);
            this.checkBoxDataReduction.Name = "checkBoxDataReduction";
            this.checkBoxDataReduction.Size = new System.Drawing.Size(123, 19);
            this.checkBoxDataReduction.TabIndex = 1;
            this.checkBoxDataReduction.Text = "Data reduction";
            this.checkBoxDataReduction.UseVisualStyleBackColor = true;
            //
            // checkBoxForExcel
            //
            this.checkBoxForExcel.AutoSize = true;
            this.checkBoxForExcel.Location = new System.Drawing.Point(25, 63);
            this.checkBoxForExcel.Name = "checkBoxForExcel";
            this.checkBoxForExcel.Size = new System.Drawing.Size(89, 19);
            this.checkBoxForExcel.TabIndex = 2;
            this.checkBoxForExcel.Text = "For Excel";
            this.checkBoxForExcel.UseVisualStyleBackColor = true;
            //
            // Export_DataAsCSV
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(488, 140);
            this.Controls.Add(this.checkBoxForExcel);
            this.Controls.Add(this.checkBoxDataReduction);
            this.Controls.Add(this.buttonExport);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "Export_DataAsCSV";
            this.Text = "Export all data as a CSV file";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Export_DataAsCSV_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.CheckBox checkBoxDataReduction;
        private System.Windows.Forms.CheckBox checkBoxForExcel;
    }
}
