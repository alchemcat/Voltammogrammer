namespace Voltammogrammer
{
    partial class Calibrate_Potentiostat
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxPotential = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxCurrent = new System.Windows.Forms.TextBox();
            this.buttonApply = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(72, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Potential";
            //
            // textBoxPotential
            //
            this.textBoxPotential.Location = new System.Drawing.Point(166, 26);
            this.textBoxPotential.Name = "textBoxPotential";
            this.textBoxPotential.Size = new System.Drawing.Size(182, 22);
            this.textBoxPotential.TabIndex = 1;
            //
            // label2
            //
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(79, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Current";
            //
            // textBoxCurrent
            //
            this.textBoxCurrent.Location = new System.Drawing.Point(165, 97);
            this.textBoxCurrent.Name = "textBoxCurrent";
            this.textBoxCurrent.Size = new System.Drawing.Size(182, 22);
            this.textBoxCurrent.TabIndex = 3;
            //
            // buttonApply
            //
            this.buttonApply.Location = new System.Drawing.Point(245, 149);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(103, 28);
            this.buttonApply.TabIndex = 4;
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            //
            // label3
            //
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(365, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(27, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "mV";
            //
            // label4
            //
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(364, 99);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(16, 15);
            this.label4.TabIndex = 6;
            this.label4.Text = "V";
            //
            // Calibrate_Potentiostat
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(437, 189);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.textBoxCurrent);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxPotential);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Calibrate_Potentiostat";
            this.ShowInTaskbar = false;
            this.Text = "Calibrate_Potentiostat";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Calibrate_Potentiostat_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxPotential;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxCurrent;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
    }
}
