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
            this.label2 = new System.Windows.Forms.Label();
            this.lstLockedTimers = new System.Windows.Forms.ListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radNew = new System.Windows.Forms.RadioButton();
            this.lblRunning = new System.Windows.Forms.Label();
            this.cmbRecentJiras = new System.Windows.Forms.ComboBox();
            this.radSelected = new System.Windows.Forms.RadioButton();
            this.btnOK = new System.Windows.Forms.Button();
            this.radRunning = new System.Windows.Forms.RadioButton();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
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
            this.lstLockedTimers.Location = new System.Drawing.Point(17, 37);
            this.lstLockedTimers.Name = "lstLockedTimers";
            this.lstLockedTimers.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstLockedTimers.Size = new System.Drawing.Size(532, 174);
            this.lstLockedTimers.TabIndex = 2;
            this.lstLockedTimers.SelectedIndexChanged += new System.EventHandler(this.lstIdleTimers_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radNew);
            this.groupBox1.Controls.Add(this.lblRunning);
            this.groupBox1.Controls.Add(this.cmbRecentJiras);
            this.groupBox1.Controls.Add(this.radSelected);
            this.groupBox1.Controls.Add(this.btnOK);
            this.groupBox1.Controls.Add(this.radRunning);
            this.groupBox1.Location = new System.Drawing.Point(13, 225);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(607, 110);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Locked Time Destination";
            // 
            // radNew
            // 
            this.radNew.AutoSize = true;
            this.radNew.Location = new System.Drawing.Point(6, 78);
            this.radNew.Name = "radNew";
            this.radNew.Size = new System.Drawing.Size(136, 21);
            this.radNew.TabIndex = 4;
            this.radNew.Text = "Add To New Timer";
            this.radNew.UseVisualStyleBackColor = true;
            // 
            // lblRunning
            // 
            this.lblRunning.Location = new System.Drawing.Point(161, 26);
            this.lblRunning.Name = "lblRunning";
            this.lblRunning.Size = new System.Drawing.Size(369, 21);
            this.lblRunning.TabIndex = 1;
            this.lblRunning.Text = "N/A";
            // 
            // cmbRecentJiras
            // 
            this.cmbRecentJiras.DropDownWidth = 460;
            this.cmbRecentJiras.FormattingEnabled = true;
            this.cmbRecentJiras.Location = new System.Drawing.Point(134, 50);
            this.cmbRecentJiras.Name = "cmbRecentJiras";
            this.cmbRecentJiras.Size = new System.Drawing.Size(396, 25);
            this.cmbRecentJiras.TabIndex = 3;
            this.cmbRecentJiras.SelectedIndexChanged += new System.EventHandler(this.cmbRecentJiras_SelectedIndexChanged);
            // 
            // radSelected
            // 
            this.radSelected.AutoSize = true;
            this.radSelected.Location = new System.Drawing.Point(6, 51);
            this.radSelected.Name = "radSelected";
            this.radSelected.Size = new System.Drawing.Size(115, 21);
            this.radSelected.TabIndex = 2;
            this.radSelected.Text = "Add To Recent:";
            this.radSelected.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Image = global::Gallifrey.UI.Classic.Properties.Resources.Check_48x48;
            this.btnOK.Location = new System.Drawing.Point(536, 26);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(65, 65);
            this.btnOK.TabIndex = 5;
            this.btnOK.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // radRunning
            // 
            this.radRunning.AutoSize = true;
            this.radRunning.Location = new System.Drawing.Point(6, 24);
            this.radRunning.Name = "radRunning";
            this.radRunning.Size = new System.Drawing.Size(160, 21);
            this.radRunning.TabIndex = 0;
            this.radRunning.Text = "Add To Running Timer:";
            this.radRunning.UseVisualStyleBackColor = true;
            // 
            // btnRemove
            // 
            this.btnRemove.Image = global::Gallifrey.UI.Classic.Properties.Resources.Delete_48x48;
            this.btnRemove.Location = new System.Drawing.Point(555, 81);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(65, 65);
            this.btnRemove.TabIndex = 6;
            this.btnRemove.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // btnClose
            // 
            this.btnClose.Image = global::Gallifrey.UI.Classic.Properties.Resources.Cancel_48x48;
            this.btnClose.Location = new System.Drawing.Point(286, 341);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(65, 65);
            this.btnClose.TabIndex = 7;
            this.btnClose.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // LockedTimerWindow
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 421);
            this.ControlBox = false;
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lstLockedTimers);
            this.Controls.Add(this.label2);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LockedTimerWindow";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Gallifrey - Locked Timers";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ListBox lstLockedTimers;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblRunning;
        private System.Windows.Forms.ComboBox cmbRecentJiras;
        private System.Windows.Forms.RadioButton radSelected;
        private System.Windows.Forms.RadioButton radRunning;
        private System.Windows.Forms.RadioButton radNew;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnClose;
    }
}