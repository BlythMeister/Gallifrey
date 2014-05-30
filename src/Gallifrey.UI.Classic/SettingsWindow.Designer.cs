namespace Gallifrey.UI.Classic
{
    partial class SettingsWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsWindow));
            this.txtJiraUrl = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtJiraUsername = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtJiraPassword = new System.Windows.Forms.TextBox();
            this.grpJira = new System.Windows.Forms.GroupBox();
            this.grpAppSettings = new System.Windows.Forms.GroupBox();
            this.chkAlwaysTop = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtTargetHours = new System.Windows.Forms.TextBox();
            this.txtTargetMins = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.chkAlert = new System.Windows.Forms.CheckBox();
            this.txtTimerDays = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtAlertMins = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.btnCancelEditSettings = new System.Windows.Forms.Button();
            this.btnSaveSettings = new System.Windows.Forms.Button();
            this.grpJira.SuspendLayout();
            this.grpAppSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtJiraUrl
            // 
            this.txtJiraUrl.Location = new System.Drawing.Point(102, 30);
            this.txtJiraUrl.Name = "txtJiraUrl";
            this.txtJiraUrl.Size = new System.Drawing.Size(194, 25);
            this.txtJiraUrl.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(41, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Jira URL";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 25);
            this.label2.TabIndex = 0;
            this.label2.Text = "Settings";
            // 
            // txtJiraUsername
            // 
            this.txtJiraUsername.Location = new System.Drawing.Point(102, 61);
            this.txtJiraUsername.Name = "txtJiraUsername";
            this.txtJiraUsername.Size = new System.Drawing.Size(194, 25);
            this.txtJiraUsername.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(91, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "Jira Username";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 95);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(88, 17);
            this.label4.TabIndex = 4;
            this.label4.Text = "Jira Password";
            // 
            // txtJiraPassword
            // 
            this.txtJiraPassword.Location = new System.Drawing.Point(102, 92);
            this.txtJiraPassword.Name = "txtJiraPassword";
            this.txtJiraPassword.Size = new System.Drawing.Size(194, 25);
            this.txtJiraPassword.TabIndex = 5;
            this.txtJiraPassword.UseSystemPasswordChar = true;
            // 
            // grpJira
            // 
            this.grpJira.Controls.Add(this.txtJiraPassword);
            this.grpJira.Controls.Add(this.label4);
            this.grpJira.Controls.Add(this.txtJiraUrl);
            this.grpJira.Controls.Add(this.label1);
            this.grpJira.Controls.Add(this.label3);
            this.grpJira.Controls.Add(this.txtJiraUsername);
            this.grpJira.Location = new System.Drawing.Point(12, 247);
            this.grpJira.Name = "grpJira";
            this.grpJira.Size = new System.Drawing.Size(302, 130);
            this.grpJira.TabIndex = 2;
            this.grpJira.TabStop = false;
            this.grpJira.Text = "Jira Connection Settings";
            // 
            // grpAppSettings
            // 
            this.grpAppSettings.Controls.Add(this.chkAlwaysTop);
            this.grpAppSettings.Controls.Add(this.label11);
            this.grpAppSettings.Controls.Add(this.txtTargetHours);
            this.grpAppSettings.Controls.Add(this.txtTargetMins);
            this.grpAppSettings.Controls.Add(this.label9);
            this.grpAppSettings.Controls.Add(this.label10);
            this.grpAppSettings.Controls.Add(this.label8);
            this.grpAppSettings.Controls.Add(this.chkAlert);
            this.grpAppSettings.Controls.Add(this.txtTimerDays);
            this.grpAppSettings.Controls.Add(this.label5);
            this.grpAppSettings.Controls.Add(this.txtAlertMins);
            this.grpAppSettings.Controls.Add(this.label6);
            this.grpAppSettings.Controls.Add(this.label7);
            this.grpAppSettings.Location = new System.Drawing.Point(12, 46);
            this.grpAppSettings.Name = "grpAppSettings";
            this.grpAppSettings.Size = new System.Drawing.Size(302, 195);
            this.grpAppSettings.TabIndex = 1;
            this.grpAppSettings.TabStop = false;
            this.grpAppSettings.Text = "App Settings";
            // 
            // chkAlwaysTop
            // 
            this.chkAlwaysTop.AutoSize = true;
            this.chkAlwaysTop.Location = new System.Drawing.Point(158, 168);
            this.chkAlwaysTop.Name = "chkAlwaysTop";
            this.chkAlwaysTop.Size = new System.Drawing.Size(15, 14);
            this.chkAlwaysTop.TabIndex = 12;
            this.chkAlwaysTop.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(2, 166);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(150, 17);
            this.label11.TabIndex = 11;
            this.label11.Text = "Pop-Up Always On Top?";
            // 
            // txtTargetHours
            // 
            this.txtTargetHours.Location = new System.Drawing.Point(158, 106);
            this.txtTargetHours.Name = "txtTargetHours";
            this.txtTargetHours.Size = new System.Drawing.Size(39, 25);
            this.txtTargetHours.TabIndex = 7;
            this.txtTargetHours.Text = "00";
            this.txtTargetHours.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtTargetMins
            // 
            this.txtTargetMins.Location = new System.Drawing.Point(158, 137);
            this.txtTargetMins.Name = "txtTargetMins";
            this.txtTargetMins.Size = new System.Drawing.Size(39, 25);
            this.txtTargetMins.TabIndex = 9;
            this.txtTargetMins.Text = "00";
            this.txtTargetMins.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(196, 109);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(43, 17);
            this.label9.TabIndex = 8;
            this.label9.Text = "Hours";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(203, 140);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(54, 17);
            this.label10.TabIndex = 10;
            this.label10.Text = "Minutes";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(16, 106);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(135, 17);
            this.label8.TabIndex = 6;
            this.label8.Text = "Taget Export Per Day:";
            // 
            // chkAlert
            // 
            this.chkAlert.AutoSize = true;
            this.chkAlert.Location = new System.Drawing.Point(158, 24);
            this.chkAlert.Name = "chkAlert";
            this.chkAlert.Size = new System.Drawing.Size(15, 14);
            this.chkAlert.TabIndex = 1;
            this.chkAlert.UseVisualStyleBackColor = true;
            // 
            // txtTimerDays
            // 
            this.txtTimerDays.Location = new System.Drawing.Point(158, 75);
            this.txtTimerDays.Name = "txtTimerDays";
            this.txtTimerDays.Size = new System.Drawing.Size(138, 25);
            this.txtTimerDays.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 78);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(136, 17);
            this.label5.TabIndex = 4;
            this.label5.Text = "Keep Timers For Days";
            // 
            // txtAlertMins
            // 
            this.txtAlertMins.Location = new System.Drawing.Point(158, 44);
            this.txtAlertMins.Name = "txtAlertMins";
            this.txtAlertMins.Size = new System.Drawing.Size(138, 25);
            this.txtAlertMins.TabIndex = 3;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(4, 51);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(150, 17);
            this.label6.TabIndex = 2;
            this.label6.Text = "Idle Time Alert (Minutes)";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(49, 21);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(103, 17);
            this.label7.TabIndex = 0;
            this.label7.Text = "Alert When Idle?";
            // 
            // btnCancelEditSettings
            // 
            this.btnCancelEditSettings.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelEditSettings.Image = global::Gallifrey.UI.Classic.Properties.Resources.Cancel_48x48;
            this.btnCancelEditSettings.Location = new System.Drawing.Point(170, 383);
            this.btnCancelEditSettings.Name = "btnCancelEditSettings";
            this.btnCancelEditSettings.Size = new System.Drawing.Size(65, 65);
            this.btnCancelEditSettings.TabIndex = 4;
            this.btnCancelEditSettings.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnCancelEditSettings.UseVisualStyleBackColor = true;
            this.btnCancelEditSettings.Click += new System.EventHandler(this.btnCancelEditSettings_Click);
            // 
            // btnSaveSettings
            // 
            this.btnSaveSettings.Image = global::Gallifrey.UI.Classic.Properties.Resources.Check_48x48;
            this.btnSaveSettings.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnSaveSettings.Location = new System.Drawing.Point(89, 383);
            this.btnSaveSettings.Name = "btnSaveSettings";
            this.btnSaveSettings.Size = new System.Drawing.Size(65, 65);
            this.btnSaveSettings.TabIndex = 3;
            this.btnSaveSettings.UseVisualStyleBackColor = true;
            this.btnSaveSettings.Click += new System.EventHandler(this.btnSaveSettings_Click);
            // 
            // SettingsWindow
            // 
            this.AcceptButton = this.btnSaveSettings;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancelEditSettings;
            this.ClientSize = new System.Drawing.Size(331, 464);
            this.Controls.Add(this.grpAppSettings);
            this.Controls.Add(this.grpJira);
            this.Controls.Add(this.btnCancelEditSettings);
            this.Controls.Add(this.btnSaveSettings);
            this.Controls.Add(this.label2);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximumSize = new System.Drawing.Size(347, 502);
            this.MinimumSize = new System.Drawing.Size(347, 502);
            this.Name = "SettingsWindow";
            this.Text = "Gallifrey - Settings";
            this.grpJira.ResumeLayout(false);
            this.grpJira.PerformLayout();
            this.grpAppSettings.ResumeLayout(false);
            this.grpAppSettings.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtJiraUrl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSaveSettings;
        private System.Windows.Forms.Button btnCancelEditSettings;
        private System.Windows.Forms.TextBox txtJiraUsername;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtJiraPassword;
        private System.Windows.Forms.GroupBox grpJira;
        private System.Windows.Forms.GroupBox grpAppSettings;
        private System.Windows.Forms.TextBox txtTimerDays;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtAlertMins;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox chkAlert;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtTargetHours;
        private System.Windows.Forms.TextBox txtTargetMins;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox chkAlwaysTop;
        private System.Windows.Forms.Label label11;
    }
}