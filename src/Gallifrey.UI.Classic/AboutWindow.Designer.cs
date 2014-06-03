namespace Gallifrey.UI.Classic
{
    partial class AboutWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutWindow));
            this.label2 = new System.Windows.Forms.Label();
            this.lblCurrentVersion = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblContributors = new System.Windows.Forms.Label();
            this.timerContributor = new System.Windows.Forms.Timer(this.components);
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnTwitter = new System.Windows.Forms.Button();
            this.btnEmail = new System.Windows.Forms.Button();
            this.btnGitHub = new System.Windows.Forms.Button();
            this.btnPayPal = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 25);
            this.label2.TabIndex = 0;
            this.label2.Text = "About";
            // 
            // lblCurrentVersion
            // 
            this.lblCurrentVersion.AutoSize = true;
            this.lblCurrentVersion.Location = new System.Drawing.Point(13, 38);
            this.lblCurrentVersion.Name = "lblCurrentVersion";
            this.lblCurrentVersion.Size = new System.Drawing.Size(143, 17);
            this.lblCurrentVersion.TabIndex = 1;
            this.lblCurrentVersion.Text = "Current Version: 0.0.0.0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 157);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(170, 25);
            this.label1.TabIndex = 3;
            this.label1.Text = "Owner/Maintainer";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(14, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(356, 93);
            this.label3.TabIndex = 2;
            this.label3.Text = "Gallifrey is a time logging companion designed to work with Jira time logs.\r\n\r\nGa" +
    "llifrey is open sourced on GitHub and free to download and contribute!";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(14, 182);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(356, 57);
            this.label4.TabIndex = 4;
            this.label4.Text = "Chris Blyth\r\nTwitter: @BlythMeister\r\nGitHub: @BlythMeister";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(11, 239);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(243, 25);
            this.label5.TabIndex = 5;
            this.label5.Text = "Contributors (With Thanks)";
            // 
            // lblContributors
            // 
            this.lblContributors.Location = new System.Drawing.Point(14, 264);
            this.lblContributors.Name = "lblContributors";
            this.lblContributors.Size = new System.Drawing.Size(356, 55);
            this.lblContributors.TabIndex = 6;
            // 
            // timerContributor
            // 
            this.timerContributor.Enabled = true;
            this.timerContributor.Interval = 1000;
            this.timerContributor.Tick += new System.EventHandler(this.timerContributor_Tick);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(12, 319);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(80, 25);
            this.label6.TabIndex = 7;
            this.label6.Text = "Support";
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(13, 344);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(270, 57);
            this.label7.TabIndex = 8;
            this.label7.Text = "Contact Us By Email Or Social Networks!\r\nEmail: GallifreyApp@gmail.com\r\nTwitter: " +
    "@GallifreyApp";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(11, 398);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(75, 25);
            this.label8.TabIndex = 11;
            this.label8.Text = "Donate";
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(14, 423);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(323, 55);
            this.label9.TabIndex = 12;
            this.label9.Text = "Help us keep the project alive by donating via PayPal.  To donate, click here ->";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(10, 478);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(119, 25);
            this.label10.TabIndex = 14;
            this.label10.Text = "Source Code";
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(12, 503);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(299, 42);
            this.label11.TabIndex = 15;
            this.label11.Text = "Download and Fork on GitHub, Click the Octocat!";
            // 
            // label12
            // 
            this.label12.Location = new System.Drawing.Point(14, 560);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(356, 46);
            this.label12.TabIndex = 18;
            this.label12.Text = "For details of the 3rd party Libraries and Resources used check the readme on our" +
    " GitHub Page";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(11, 535);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(168, 25);
            this.label13.TabIndex = 17;
            this.label13.Text = "3rd Party Libraries";
            // 
            // btnOK
            // 
            this.btnOK.Image = global::Gallifrey.UI.Classic.Properties.Resources.Check_48x48;
            this.btnOK.Location = new System.Drawing.Point(150, 609);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(65, 65);
            this.btnOK.TabIndex = 19;
            this.btnOK.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnTwitter
            // 
            this.btnTwitter.Image = global::Gallifrey.UI.Classic.Properties.Resources.Twitter_logo_blue;
            this.btnTwitter.Location = new System.Drawing.Point(322, 355);
            this.btnTwitter.Name = "btnTwitter";
            this.btnTwitter.Size = new System.Drawing.Size(48, 46);
            this.btnTwitter.TabIndex = 10;
            this.btnTwitter.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnTwitter.UseVisualStyleBackColor = true;
            this.btnTwitter.Click += new System.EventHandler(this.btnTwitter_Click);
            // 
            // btnEmail
            // 
            this.btnEmail.Image = global::Gallifrey.UI.Classic.Properties.Resources.Mail_32x32;
            this.btnEmail.Location = new System.Drawing.Point(268, 355);
            this.btnEmail.Name = "btnEmail";
            this.btnEmail.Size = new System.Drawing.Size(48, 46);
            this.btnEmail.TabIndex = 9;
            this.btnEmail.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnEmail.UseVisualStyleBackColor = true;
            this.btnEmail.Click += new System.EventHandler(this.btnEmail_Click);
            // 
            // btnGitHub
            // 
            this.btnGitHub.Image = global::Gallifrey.UI.Classic.Properties.Resources.GitHub_Mark_32px;
            this.btnGitHub.Location = new System.Drawing.Point(322, 499);
            this.btnGitHub.Name = "btnGitHub";
            this.btnGitHub.Size = new System.Drawing.Size(48, 46);
            this.btnGitHub.TabIndex = 16;
            this.btnGitHub.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnGitHub.UseVisualStyleBackColor = true;
            this.btnGitHub.Click += new System.EventHandler(this.btnGitHub_Click);
            // 
            // btnPayPal
            // 
            this.btnPayPal.Image = global::Gallifrey.UI.Classic.Properties.Resources.pp_logo_100px;
            this.btnPayPal.Location = new System.Drawing.Point(183, 442);
            this.btnPayPal.Name = "btnPayPal";
            this.btnPayPal.Size = new System.Drawing.Size(111, 36);
            this.btnPayPal.TabIndex = 13;
            this.btnPayPal.UseVisualStyleBackColor = true;
            this.btnPayPal.Click += new System.EventHandler(this.btnPayPal_Click);
            // 
            // AboutWindow
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 686);
            this.Controls.Add(this.btnTwitter);
            this.Controls.Add(this.btnEmail);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.btnGitHub);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.btnPayPal);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.lblContributors);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblCurrentVersion);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.label2);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximumSize = new System.Drawing.Size(398, 724);
            this.MinimumSize = new System.Drawing.Size(398, 724);
            this.Name = "AboutWindow";
            this.Text = "Gallifrey - About";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblCurrentVersion;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblContributors;
        private System.Windows.Forms.Timer timerContributor;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btnPayPal;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btnGitHub;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button btnEmail;
        private System.Windows.Forms.Button btnTwitter;
    }
}