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
            this.btnCancelEditSettings = new System.Windows.Forms.Button();
            this.btnSaveSettings = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtJiraUrl
            // 
            this.txtJiraUrl.Location = new System.Drawing.Point(111, 47);
            this.txtJiraUrl.Name = "txtJiraUrl";
            this.txtJiraUrl.Size = new System.Drawing.Size(194, 25);
            this.txtJiraUrl.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(50, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Jira URL";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 25);
            this.label2.TabIndex = 5;
            this.label2.Text = "Settings";
            // 
            // txtJiraUsername
            // 
            this.txtJiraUsername.Location = new System.Drawing.Point(111, 78);
            this.txtJiraUsername.Name = "txtJiraUsername";
            this.txtJiraUsername.Size = new System.Drawing.Size(194, 25);
            this.txtJiraUsername.TabIndex = 12;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(91, 17);
            this.label3.TabIndex = 13;
            this.label3.Text = "Jira Username";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 112);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(88, 17);
            this.label4.TabIndex = 15;
            this.label4.Text = "Jira Password";
            // 
            // txtJiraPassword
            // 
            this.txtJiraPassword.Location = new System.Drawing.Point(111, 109);
            this.txtJiraPassword.Name = "txtJiraPassword";
            this.txtJiraPassword.Size = new System.Drawing.Size(194, 25);
            this.txtJiraPassword.TabIndex = 14;
            this.txtJiraPassword.UseSystemPasswordChar = true;
            // 
            // btnCancelEditSettings
            // 
            this.btnCancelEditSettings.Image = global::Gallifrey.UI.Classic.Properties.Resources.Cancel_48x48;
            this.btnCancelEditSettings.Location = new System.Drawing.Point(172, 150);
            this.btnCancelEditSettings.Name = "btnCancelEditSettings";
            this.btnCancelEditSettings.Size = new System.Drawing.Size(65, 65);
            this.btnCancelEditSettings.TabIndex = 11;
            this.btnCancelEditSettings.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnCancelEditSettings.UseVisualStyleBackColor = true;
            this.btnCancelEditSettings.Click += new System.EventHandler(this.btnCancelEditSettings_Click);
            // 
            // btnSaveSettings
            // 
            this.btnSaveSettings.Image = global::Gallifrey.UI.Classic.Properties.Resources.Check_48x48;
            this.btnSaveSettings.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnSaveSettings.Location = new System.Drawing.Point(91, 150);
            this.btnSaveSettings.Name = "btnSaveSettings";
            this.btnSaveSettings.Size = new System.Drawing.Size(65, 65);
            this.btnSaveSettings.TabIndex = 10;
            this.btnSaveSettings.UseVisualStyleBackColor = true;
            this.btnSaveSettings.Click += new System.EventHandler(this.btnSaveSettings_Click);
            // 
            // SettingsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(325, 227);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtJiraPassword);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtJiraUsername);
            this.Controls.Add(this.btnCancelEditSettings);
            this.Controls.Add(this.btnSaveSettings);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtJiraUrl);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximumSize = new System.Drawing.Size(341, 265);
            this.MinimumSize = new System.Drawing.Size(341, 265);
            this.Name = "SettingsWindow";
            this.Text = "Gallifrey - Settings";
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
    }
}