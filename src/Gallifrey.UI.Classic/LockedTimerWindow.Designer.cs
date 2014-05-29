namespace Gallifrey.UI.Classic
{
    partial class LockedTimerWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LockedTimerWindow));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.lstIdleTimers = new System.Windows.Forms.ListBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblRunning = new System.Windows.Forms.Label();
            this.cmbDayTimers = new System.Windows.Forms.ComboBox();
            this.radSelected = new System.Windows.Forms.RadioButton();
            this.radRunning = new System.Windows.Forms.RadioButton();
            this.radDelete = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Idle Timers:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(106, 25);
            this.label2.TabIndex = 0;
            this.label2.Text = "Idle Timers";
            // 
            // btnOK
            // 
            this.btnOK.Image = global::Gallifrey.UI.Classic.Properties.Resources.Check_48x48;
            this.btnOK.Location = new System.Drawing.Point(167, 341);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(65, 65);
            this.btnOK.TabIndex = 4;
            this.btnOK.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lstIdleTimers
            // 
            this.lstIdleTimers.FormattingEnabled = true;
            this.lstIdleTimers.ItemHeight = 17;
            this.lstIdleTimers.Location = new System.Drawing.Point(94, 50);
            this.lstIdleTimers.Name = "lstIdleTimers";
            this.lstIdleTimers.Size = new System.Drawing.Size(573, 157);
            this.lstIdleTimers.TabIndex = 2;
            this.lstIdleTimers.SelectedIndexChanged += new System.EventHandler(this.lstIdleTimers_SelectedIndexChanged);
            // 
            // btnCancel
            // 
            this.btnCancel.Image = global::Gallifrey.UI.Classic.Properties.Resources.Cancel_48x48;
            this.btnCancel.Location = new System.Drawing.Point(272, 341);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(65, 65);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblRunning);
            this.groupBox1.Controls.Add(this.cmbDayTimers);
            this.groupBox1.Controls.Add(this.radSelected);
            this.groupBox1.Controls.Add(this.radRunning);
            this.groupBox1.Controls.Add(this.radDelete);
            this.groupBox1.Location = new System.Drawing.Point(13, 225);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(654, 110);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Idle Time Destination";
            // 
            // lblRunning
            // 
            this.lblRunning.AutoSize = true;
            this.lblRunning.Location = new System.Drawing.Point(162, 54);
            this.lblRunning.Name = "lblRunning";
            this.lblRunning.Size = new System.Drawing.Size(31, 17);
            this.lblRunning.TabIndex = 2;
            this.lblRunning.Text = "N/A";
            // 
            // cmbDayTimers
            // 
            this.cmbDayTimers.FormattingEnabled = true;
            this.cmbDayTimers.Location = new System.Drawing.Point(135, 78);
            this.cmbDayTimers.Name = "cmbDayTimers";
            this.cmbDayTimers.Size = new System.Drawing.Size(513, 25);
            this.cmbDayTimers.TabIndex = 4;
            // 
            // radSelected
            // 
            this.radSelected.AutoSize = true;
            this.radSelected.Location = new System.Drawing.Point(4, 79);
            this.radSelected.Name = "radSelected";
            this.radSelected.Size = new System.Drawing.Size(125, 21);
            this.radSelected.TabIndex = 3;
            this.radSelected.Text = "Add To Selected:";
            this.radSelected.UseVisualStyleBackColor = true;
            // 
            // radRunning
            // 
            this.radRunning.AutoSize = true;
            this.radRunning.Location = new System.Drawing.Point(7, 52);
            this.radRunning.Name = "radRunning";
            this.radRunning.Size = new System.Drawing.Size(160, 21);
            this.radRunning.TabIndex = 1;
            this.radRunning.Text = "Add To Running Timer:";
            this.radRunning.UseVisualStyleBackColor = true;
            // 
            // radDelete
            // 
            this.radDelete.AutoSize = true;
            this.radDelete.Checked = true;
            this.radDelete.Location = new System.Drawing.Point(7, 25);
            this.radDelete.Name = "radDelete";
            this.radDelete.Size = new System.Drawing.Size(120, 21);
            this.radDelete.TabIndex = 0;
            this.radDelete.TabStop = true;
            this.radDelete.Text = "Delete Idle Time";
            this.radDelete.UseVisualStyleBackColor = true;
            // 
            // IdleTimerWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(679, 418);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lstIdleTimers);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "IdleTimerWindow";
            this.Text = "Gallifrey - Idle Timers";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ListBox lstIdleTimers;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblRunning;
        private System.Windows.Forms.ComboBox cmbDayTimers;
        private System.Windows.Forms.RadioButton radSelected;
        private System.Windows.Forms.RadioButton radRunning;
        private System.Windows.Forms.RadioButton radDelete;
    }
}