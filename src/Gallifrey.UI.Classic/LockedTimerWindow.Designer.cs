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
            this.lstLockedTimers = new System.Windows.Forms.ListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radNew = new System.Windows.Forms.RadioButton();
            this.lblRunning = new System.Windows.Forms.Label();
            this.cmbDayTimers = new System.Windows.Forms.ComboBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.radSelected = new System.Windows.Forms.RadioButton();
            this.radRunning = new System.Windows.Forms.RadioButton();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Locked Timers";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(134, 25);
            this.label2.TabIndex = 0;
            this.label2.Text = "Locked Timers";
            // 
            // lstLockedTimers
            // 
            this.lstLockedTimers.FormattingEnabled = true;
            this.lstLockedTimers.ItemHeight = 17;
            this.lstLockedTimers.Location = new System.Drawing.Point(113, 50);
            this.lstLockedTimers.Name = "lstLockedTimers";
            this.lstLockedTimers.Size = new System.Drawing.Size(507, 157);
            this.lstLockedTimers.TabIndex = 2;
            this.lstLockedTimers.SelectedIndexChanged += new System.EventHandler(this.lstIdleTimers_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radNew);
            this.groupBox1.Controls.Add(this.lblRunning);
            this.groupBox1.Controls.Add(this.cmbDayTimers);
            this.groupBox1.Controls.Add(this.btnOK);
            this.groupBox1.Controls.Add(this.radSelected);
            this.groupBox1.Controls.Add(this.radRunning);
            this.groupBox1.Location = new System.Drawing.Point(13, 225);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(607, 123);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Locked Time Destination";
            // 
            // radNew
            // 
            this.radNew.AutoSize = true;
            this.radNew.Location = new System.Drawing.Point(6, 85);
            this.radNew.Name = "radNew";
            this.radNew.Size = new System.Drawing.Size(136, 21);
            this.radNew.TabIndex = 4;
            this.radNew.Text = "Add To New Timer";
            this.radNew.UseVisualStyleBackColor = true;
            // 
            // lblRunning
            // 
            this.lblRunning.Location = new System.Drawing.Point(161, 33);
            this.lblRunning.Name = "lblRunning";
            this.lblRunning.Size = new System.Drawing.Size(360, 21);
            this.lblRunning.TabIndex = 1;
            this.lblRunning.Text = "N/A";
            // 
            // cmbDayTimers
            // 
            this.cmbDayTimers.DropDownWidth = 460;
            this.cmbDayTimers.FormattingEnabled = true;
            this.cmbDayTimers.Location = new System.Drawing.Point(134, 57);
            this.cmbDayTimers.Name = "cmbDayTimers";
            this.cmbDayTimers.Size = new System.Drawing.Size(387, 25);
            this.cmbDayTimers.TabIndex = 3;
            // 
            // btnOK
            // 
            this.btnOK.Image = global::Gallifrey.UI.Classic.Properties.Resources.Save_48x48;
            this.btnOK.Location = new System.Drawing.Point(527, 33);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(65, 65);
            this.btnOK.TabIndex = 5;
            this.btnOK.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // radSelected
            // 
            this.radSelected.AutoSize = true;
            this.radSelected.Location = new System.Drawing.Point(6, 58);
            this.radSelected.Name = "radSelected";
            this.radSelected.Size = new System.Drawing.Size(125, 21);
            this.radSelected.TabIndex = 2;
            this.radSelected.Text = "Add To Selected:";
            this.radSelected.UseVisualStyleBackColor = true;
            // 
            // radRunning
            // 
            this.radRunning.AutoSize = true;
            this.radRunning.Location = new System.Drawing.Point(6, 31);
            this.radRunning.Name = "radRunning";
            this.radRunning.Size = new System.Drawing.Size(160, 21);
            this.radRunning.TabIndex = 0;
            this.radRunning.Text = "Add To Running Timer:";
            this.radRunning.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::Gallifrey.UI.Classic.Properties.Resources.Cancel_48x48;
            this.btnCancel.Location = new System.Drawing.Point(304, 354);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(65, 65);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // LockedTimerWindow
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(634, 430);
            this.ControlBox = false;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lstLockedTimers);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LockedTimerWindow";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Gallifrey - Locked Timers";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ListBox lstLockedTimers;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblRunning;
        private System.Windows.Forms.ComboBox cmbDayTimers;
        private System.Windows.Forms.RadioButton radSelected;
        private System.Windows.Forms.RadioButton radRunning;
        private System.Windows.Forms.RadioButton radNew;
    }
}