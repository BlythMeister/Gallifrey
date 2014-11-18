namespace Gallifrey.UI.Classic
{
    partial class MainWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tabTimerDays = new System.Windows.Forms.TabControl();
            this.formTimer = new System.Windows.Forms.Timer(this.components);
            this.lblCurrentTime = new System.Windows.Forms.Label();
            this.grpTarget = new System.Windows.Forms.GroupBox();
            this.progExportTarget = new System.Windows.Forms.ProgressBar();
            this.lblExportedWeek = new System.Windows.Forms.Label();
            this.lblExportTargetWeek = new System.Windows.Forms.Label();
            this.grpExportStats = new System.Windows.Forms.GroupBox();
            this.lblUnexportedTime = new System.Windows.Forms.Label();
            this.lblExportStat = new System.Windows.Forms.Label();
            this.notifyAlert = new System.Windows.Forms.NotifyIcon(this.components);
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.btnAbout = new System.Windows.Forms.Button();
            this.btnIdle = new System.Windows.Forms.Button();
            this.btnRename = new System.Windows.Forms.Button();
            this.btnTimeEdit = new System.Windows.Forms.Button();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnRemoveTimer = new System.Windows.Forms.Button();
            this.btnAddTimer = new System.Windows.Forms.Button();
            this.lblUpdate = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.grpUpdates = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lblTwitter = new System.Windows.Forms.LinkLabel();
            this.label4 = new System.Windows.Forms.Label();
            this.lblEmail = new System.Windows.Forms.LinkLabel();
            this.label5 = new System.Windows.Forms.Label();
            this.lblDonate = new System.Windows.Forms.LinkLabel();
            this.label6 = new System.Windows.Forms.Label();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.lblGitHub = new System.Windows.Forms.LinkLabel();
            this.grpTarget.SuspendLayout();
            this.grpExportStats.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.grpUpdates.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(92, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(163, 50);
            this.label1.TabIndex = 0;
            this.label1.Text = "Gallifrey";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(98, 59);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(183, 17);
            this.label3.TabIndex = 1;
            this.label3.Text = "Jira Time Logging Companion";
            // 
            // tabTimerDays
            // 
            this.tabTimerDays.AllowDrop = true;
            this.tabTimerDays.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabTimerDays.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabTimerDays.Location = new System.Drawing.Point(12, 169);
            this.tabTimerDays.Name = "tabTimerDays";
            this.tabTimerDays.SelectedIndex = 0;
            this.tabTimerDays.Size = new System.Drawing.Size(951, 414);
            this.tabTimerDays.TabIndex = 15;
            this.tabTimerDays.DragDrop += new System.Windows.Forms.DragEventHandler(this.tabTimerDays_DragDrop);
            this.tabTimerDays.DragOver += new System.Windows.Forms.DragEventHandler(this.tabTimerDays_DragOver);
            // 
            // formTimer
            // 
            this.formTimer.Interval = 500;
            this.formTimer.Tick += new System.EventHandler(this.formTimer_Tick);
            // 
            // lblCurrentTime
            // 
            this.lblCurrentTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCurrentTime.BackColor = System.Drawing.Color.Black;
            this.lblCurrentTime.Font = new System.Drawing.Font("OCR A Extended", 40F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentTime.ForeColor = System.Drawing.Color.Red;
            this.lblCurrentTime.Location = new System.Drawing.Point(651, 98);
            this.lblCurrentTime.Name = "lblCurrentTime";
            this.lblCurrentTime.Size = new System.Drawing.Size(312, 65);
            this.lblCurrentTime.TabIndex = 14;
            this.lblCurrentTime.Text = "00:00:00";
            this.lblCurrentTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip.SetToolTip(this.lblCurrentTime, "Double Click Jump To Running (CTRL+J)");
            this.lblCurrentTime.DoubleClick += new System.EventHandler(this.lblCurrentTime_DoubleClick);
            // 
            // grpTarget
            // 
            this.grpTarget.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpTarget.Controls.Add(this.progExportTarget);
            this.grpTarget.Controls.Add(this.lblExportedWeek);
            this.grpTarget.Controls.Add(this.lblExportTargetWeek);
            this.grpTarget.Location = new System.Drawing.Point(651, 9);
            this.grpTarget.Name = "grpTarget";
            this.grpTarget.Size = new System.Drawing.Size(312, 83);
            this.grpTarget.TabIndex = 4;
            this.grpTarget.TabStop = false;
            this.grpTarget.Text = "Target Export Week To Date";
            // 
            // progExportTarget
            // 
            this.progExportTarget.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progExportTarget.Location = new System.Drawing.Point(9, 42);
            this.progExportTarget.Name = "progExportTarget";
            this.progExportTarget.Size = new System.Drawing.Size(297, 35);
            this.progExportTarget.TabIndex = 2;
            // 
            // lblExportedWeek
            // 
            this.lblExportedWeek.AutoSize = true;
            this.lblExportedWeek.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExportedWeek.Location = new System.Drawing.Point(107, 18);
            this.lblExportedWeek.Name = "lblExportedWeek";
            this.lblExportedWeek.Size = new System.Drawing.Size(92, 17);
            this.lblExportedWeek.TabIndex = 1;
            this.lblExportedWeek.Text = "Exported: 0:00";
            // 
            // lblExportTargetWeek
            // 
            this.lblExportTargetWeek.AutoSize = true;
            this.lblExportTargetWeek.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExportTargetWeek.Location = new System.Drawing.Point(6, 18);
            this.lblExportTargetWeek.Name = "lblExportTargetWeek";
            this.lblExportTargetWeek.Size = new System.Drawing.Size(84, 17);
            this.lblExportTargetWeek.TabIndex = 0;
            this.lblExportTargetWeek.Text = "Target: 10:00";
            // 
            // grpExportStats
            // 
            this.grpExportStats.Controls.Add(this.lblUnexportedTime);
            this.grpExportStats.Controls.Add(this.lblExportStat);
            this.grpExportStats.Location = new System.Drawing.Point(438, 12);
            this.grpExportStats.Name = "grpExportStats";
            this.grpExportStats.Size = new System.Drawing.Size(207, 80);
            this.grpExportStats.TabIndex = 3;
            this.grpExportStats.TabStop = false;
            this.grpExportStats.Text = "Export Stats";
            // 
            // lblUnexportedTime
            // 
            this.lblUnexportedTime.AutoSize = true;
            this.lblUnexportedTime.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUnexportedTime.Location = new System.Drawing.Point(6, 47);
            this.lblUnexportedTime.Name = "lblUnexportedTime";
            this.lblUnexportedTime.Size = new System.Drawing.Size(145, 17);
            this.lblUnexportedTime.TabIndex = 1;
            this.lblUnexportedTime.Text = "Un-Exported Time: 0:00";
            this.toolTip.SetToolTip(this.lblUnexportedTime, "Click To Show Oldest Un-Exported Timer");
            this.lblUnexportedTime.Click += new System.EventHandler(this.lblUnexportedTime_Click);
            // 
            // lblExportStat
            // 
            this.lblExportStat.AutoSize = true;
            this.lblExportStat.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExportStat.Location = new System.Drawing.Point(6, 23);
            this.lblExportStat.Name = "lblExportStat";
            this.lblExportStat.Size = new System.Drawing.Size(87, 17);
            this.lblExportStat.TabIndex = 0;
            this.lblExportStat.Text = "Exported: 0/0";
            // 
            // notifyAlert
            // 
            this.notifyAlert.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyAlert.BalloonTipText = "No Previous Alert";
            this.notifyAlert.BalloonTipTitle = "Timer Not Running";
            this.notifyAlert.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyAlert.Icon")));
            this.notifyAlert.Text = "Gallifrey";
            this.notifyAlert.Visible = true;
            this.notifyAlert.BalloonTipClicked += new System.EventHandler(this.notifyAlert_BalloonTipClicked);
            this.notifyAlert.DoubleClick += new System.EventHandler(this.notifyAlert_DoubleClick);
            // 
            // btnAbout
            // 
            this.btnAbout.Image = global::Gallifrey.UI.Classic.Properties.Resources.Information_48x48;
            this.btnAbout.Location = new System.Drawing.Point(509, 98);
            this.btnAbout.Name = "btnAbout";
            this.btnAbout.Size = new System.Drawing.Size(65, 65);
            this.btnAbout.TabIndex = 12;
            this.btnAbout.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip.SetToolTip(this.btnAbout, "About (CTRL+I)");
            this.btnAbout.UseVisualStyleBackColor = true;
            this.btnAbout.Click += new System.EventHandler(this.btnAbout_Click);
            // 
            // btnIdle
            // 
            this.btnIdle.Image = global::Gallifrey.UI.Classic.Properties.Resources.Key_48x48;
            this.btnIdle.Location = new System.Drawing.Point(438, 98);
            this.btnIdle.Name = "btnIdle";
            this.btnIdle.Size = new System.Drawing.Size(65, 65);
            this.btnIdle.TabIndex = 11;
            this.btnIdle.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip.SetToolTip(this.btnIdle, "View Machine Locked Timers (CTRL+L)");
            this.btnIdle.UseVisualStyleBackColor = true;
            this.btnIdle.Click += new System.EventHandler(this.btnIdle_Click);
            // 
            // btnRename
            // 
            this.btnRename.Image = global::Gallifrey.UI.Classic.Properties.Resources.Edit_48x48;
            this.btnRename.Location = new System.Drawing.Point(296, 98);
            this.btnRename.Name = "btnRename";
            this.btnRename.Size = new System.Drawing.Size(65, 65);
            this.btnRename.TabIndex = 9;
            this.btnRename.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip.SetToolTip(this.btnRename, "Change Jira For Timer (CTRL+R)");
            this.btnRename.UseVisualStyleBackColor = true;
            this.btnRename.Click += new System.EventHandler(this.btnRename_Click);
            // 
            // btnTimeEdit
            // 
            this.btnTimeEdit.Image = global::Gallifrey.UI.Classic.Properties.Resources.UpDown_48x48;
            this.btnTimeEdit.Location = new System.Drawing.Point(225, 98);
            this.btnTimeEdit.Name = "btnTimeEdit";
            this.btnTimeEdit.Size = new System.Drawing.Size(65, 65);
            this.btnTimeEdit.TabIndex = 8;
            this.btnTimeEdit.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip.SetToolTip(this.btnTimeEdit, "Edit Current Time (CTRL+C)");
            this.btnTimeEdit.UseVisualStyleBackColor = true;
            this.btnTimeEdit.Click += new System.EventHandler(this.btnTimeEdit_Click);
            // 
            // btnSearch
            // 
            this.btnSearch.Image = global::Gallifrey.UI.Classic.Properties.Resources.Search_48x48;
            this.btnSearch.Location = new System.Drawing.Point(154, 98);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(65, 65);
            this.btnSearch.TabIndex = 7;
            this.btnSearch.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip.SetToolTip(this.btnSearch, "Search Jira (CTRL+F)");
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // btnExport
            // 
            this.btnExport.Image = global::Gallifrey.UI.Classic.Properties.Resources.Upload_48x48;
            this.btnExport.Location = new System.Drawing.Point(367, 98);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(65, 65);
            this.btnExport.TabIndex = 10;
            this.btnExport.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip.SetToolTip(this.btnExport, "Export Time To Jira (CTRL+E)\r\nOr \r\nExport Selected Day To Jira (CTRL + X ) - Keyb" +
        "oard Shortcut ONLY");
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnSettings
            // 
            this.btnSettings.Image = global::Gallifrey.UI.Classic.Properties.Resources.Settings_48x48;
            this.btnSettings.Location = new System.Drawing.Point(580, 98);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(65, 65);
            this.btnSettings.TabIndex = 13;
            this.btnSettings.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip.SetToolTip(this.btnSettings, "Settings (CTRL+S)");
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // btnRemoveTimer
            // 
            this.btnRemoveTimer.Image = global::Gallifrey.UI.Classic.Properties.Resources.Delete_48x48;
            this.btnRemoveTimer.Location = new System.Drawing.Point(83, 98);
            this.btnRemoveTimer.Name = "btnRemoveTimer";
            this.btnRemoveTimer.Size = new System.Drawing.Size(65, 65);
            this.btnRemoveTimer.TabIndex = 6;
            this.btnRemoveTimer.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip.SetToolTip(this.btnRemoveTimer, "Remove Selected Timer (CTRL+D)");
            this.btnRemoveTimer.UseVisualStyleBackColor = true;
            this.btnRemoveTimer.Click += new System.EventHandler(this.btnRemoveTimer_Click);
            // 
            // btnAddTimer
            // 
            this.btnAddTimer.Image = global::Gallifrey.UI.Classic.Properties.Resources.Add_48x48;
            this.btnAddTimer.Location = new System.Drawing.Point(12, 98);
            this.btnAddTimer.Name = "btnAddTimer";
            this.btnAddTimer.Size = new System.Drawing.Size(65, 65);
            this.btnAddTimer.TabIndex = 5;
            this.btnAddTimer.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip.SetToolTip(this.btnAddTimer, "Add New Timer (CTRL+A)");
            this.btnAddTimer.UseVisualStyleBackColor = true;
            this.btnAddTimer.Click += new System.EventHandler(this.btnAddTimer_Click);
            // 
            // lblUpdate
            // 
            this.lblUpdate.BackColor = System.Drawing.SystemColors.Control;
            this.lblUpdate.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.lblUpdate.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.lblUpdate.Location = new System.Drawing.Point(9, 18);
            this.lblUpdate.Name = "lblUpdate";
            this.lblUpdate.Size = new System.Drawing.Size(121, 55);
            this.lblUpdate.TabIndex = 16;
            this.lblUpdate.Text = "Currently Running v99.99.99.99 (beta)\r\nUp To Date!\r\n";
            this.lblUpdate.Click += new System.EventHandler(this.lblUpdate_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Gallifrey.UI.Classic.Properties.Resources.clock_blue_128x128;
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(80, 80);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // grpUpdates
            // 
            this.grpUpdates.BackColor = System.Drawing.SystemColors.Control;
            this.grpUpdates.Controls.Add(this.lblUpdate);
            this.grpUpdates.Location = new System.Drawing.Point(296, 12);
            this.grpUpdates.Name = "grpUpdates";
            this.grpUpdates.Size = new System.Drawing.Size(136, 80);
            this.grpUpdates.TabIndex = 17;
            this.grpUpdates.TabStop = false;
            this.grpUpdates.Text = "Version Info";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 586);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(179, 13);
            this.label2.TabIndex = 18;
            this.label2.Text = "For support contact us on Twitter";
            // 
            // lblTwitter
            // 
            this.lblTwitter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTwitter.AutoSize = true;
            this.lblTwitter.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lblTwitter.Location = new System.Drawing.Point(189, 586);
            this.lblTwitter.Name = "lblTwitter";
            this.lblTwitter.Size = new System.Drawing.Size(60, 13);
            this.lblTwitter.TabIndex = 19;
            this.lblTwitter.TabStop = true;
            this.lblTwitter.Text = "@Gallifrey";
            this.lblTwitter.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblTwitter_LinkClicked);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(246, 586);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(33, 13);
            this.label4.TabIndex = 20;
            this.label4.Text = "or by";
            // 
            // lblEmail
            // 
            this.lblEmail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblEmail.AutoSize = true;
            this.lblEmail.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lblEmail.Location = new System.Drawing.Point(276, 586);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(39, 13);
            this.lblEmail.TabIndex = 21;
            this.lblEmail.TabStop = true;
            this.lblEmail.Text = "E-Mail";
            this.lblEmail.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblEmail_LinkClicked);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(730, 586);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(154, 13);
            this.label5.TabIndex = 22;
            this.label5.Text = "Please help support Gallifrey";
            // 
            // lblDonate
            // 
            this.lblDonate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDonate.AutoSize = true;
            this.lblDonate.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lblDonate.Location = new System.Drawing.Point(880, 586);
            this.lblDonate.Name = "lblDonate";
            this.lblDonate.Size = new System.Drawing.Size(83, 13);
            this.lblDonate.TabIndex = 23;
            this.lblDonate.TabStop = true;
            this.lblDonate.Text = "click to donate";
            this.lblDonate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblDonate_LinkClicked);
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(311, 586);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(205, 13);
            this.label6.TabIndex = 24;
            this.label6.Text = "or join our open source community on";
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 608);
            this.splitter1.TabIndex = 25;
            this.splitter1.TabStop = false;
            // 
            // lblGitHub
            // 
            this.lblGitHub.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblGitHub.AutoSize = true;
            this.lblGitHub.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lblGitHub.Location = new System.Drawing.Point(512, 586);
            this.lblGitHub.Name = "lblGitHub";
            this.lblGitHub.Size = new System.Drawing.Size(44, 13);
            this.lblGitHub.TabIndex = 26;
            this.lblGitHub.TabStop = true;
            this.lblGitHub.Text = "GitHub";
            this.lblGitHub.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblGitHub_LinkClicked);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(975, 608);
            this.Controls.Add(this.lblGitHub);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.lblDonate);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lblEmail);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblTwitter);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.grpUpdates);
            this.Controls.Add(this.btnAbout);
            this.Controls.Add(this.btnIdle);
            this.Controls.Add(this.grpExportStats);
            this.Controls.Add(this.grpTarget);
            this.Controls.Add(this.lblCurrentTime);
            this.Controls.Add(this.btnRename);
            this.Controls.Add(this.btnTimeEdit);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.tabTimerDays);
            this.Controls.Add(this.btnRemoveTimer);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnAddTimer);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(952, 633);
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Gallifrey";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainWindow_FormClosed);
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MainWindow_KeyUp);
            this.grpTarget.ResumeLayout(false);
            this.grpTarget.PerformLayout();
            this.grpExportStats.ResumeLayout(false);
            this.grpExportStats.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.grpUpdates.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnAddTimer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnRemoveTimer;
        private System.Windows.Forms.TabControl tabTimerDays;
        private System.Windows.Forms.Timer formTimer;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnTimeEdit;
        private System.Windows.Forms.Button btnRename;
        private System.Windows.Forms.Label lblCurrentTime;
        private System.Windows.Forms.GroupBox grpTarget;
        private System.Windows.Forms.ProgressBar progExportTarget;
        private System.Windows.Forms.Label lblExportedWeek;
        private System.Windows.Forms.Label lblExportTargetWeek;
        private System.Windows.Forms.GroupBox grpExportStats;
        private System.Windows.Forms.Label lblUnexportedTime;
        private System.Windows.Forms.Label lblExportStat;
        private System.Windows.Forms.NotifyIcon notifyAlert;
        private System.Windows.Forms.Button btnIdle;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Button btnAbout;
        private System.Windows.Forms.Label lblUpdate;
        private System.Windows.Forms.GroupBox grpUpdates;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel lblTwitter;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.LinkLabel lblEmail;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.LinkLabel lblDonate;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.LinkLabel lblGitHub;
    }
}

