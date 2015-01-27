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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsWindow));
            this.txtJiraUrl = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtJiraUsername = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtJiraPassword = new System.Windows.Forms.TextBox();
            this.grpJira = new System.Windows.Forms.GroupBox();
            this.chkUseRest = new System.Windows.Forms.CheckBox();
            this.label21 = new System.Windows.Forms.Label();
            this.grpAppSettings = new System.Windows.Forms.GroupBox();
            this.chkAutoUpdate = new System.Windows.Forms.CheckBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.chkAlwaysTop = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.chkAlert = new System.Windows.Forms.CheckBox();
            this.txtTimerDays = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtAlertMins = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtTargetHours = new System.Windows.Forms.TextBox();
            this.txtTargetMins = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.btnCancelEditSettings = new System.Windows.Forms.Button();
            this.btnSaveSettings = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label15 = new System.Windows.Forms.Label();
            this.cmdWeekStart = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.chklstWorkingDays = new System.Windows.Forms.CheckedListBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.cmdDefaultExport = new System.Windows.Forms.ComboBox();
            this.txtCommentPrefix = new System.Windows.Forms.TextBox();
            this.chklstExportPrompt = new System.Windows.Forms.CheckedListBox();
            this.chkExportAll = new System.Windows.Forms.CheckBox();
            this.txtDefaultComment = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label22 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label20 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.chkUsageTracking = new System.Windows.Forms.CheckBox();
            this.grpJira.SuspendLayout();
            this.grpAppSettings.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtJiraUrl
            // 
            this.txtJiraUrl.Location = new System.Drawing.Point(102, 24);
            this.txtJiraUrl.Name = "txtJiraUrl";
            this.txtJiraUrl.Size = new System.Drawing.Size(194, 25);
            this.txtJiraUrl.TabIndex = 1;
            this.toolTip.SetToolTip(this.txtJiraUrl, "URL Used To Access Jira");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(41, 27);
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
            this.txtJiraUsername.Location = new System.Drawing.Point(102, 55);
            this.txtJiraUsername.Name = "txtJiraUsername";
            this.txtJiraUsername.Size = new System.Drawing.Size(194, 25);
            this.txtJiraUsername.TabIndex = 3;
            this.toolTip.SetToolTip(this.txtJiraUsername, "Your Jira Username");
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(91, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "Jira Username";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 89);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(88, 17);
            this.label4.TabIndex = 4;
            this.label4.Text = "Jira Password";
            // 
            // txtJiraPassword
            // 
            this.txtJiraPassword.Location = new System.Drawing.Point(102, 86);
            this.txtJiraPassword.Name = "txtJiraPassword";
            this.txtJiraPassword.Size = new System.Drawing.Size(194, 25);
            this.txtJiraPassword.TabIndex = 5;
            this.toolTip.SetToolTip(this.txtJiraPassword, "Your Jira Password");
            this.txtJiraPassword.UseSystemPasswordChar = true;
            // 
            // grpJira
            // 
            this.grpJira.Controls.Add(this.chkUseRest);
            this.grpJira.Controls.Add(this.label21);
            this.grpJira.Controls.Add(this.txtJiraPassword);
            this.grpJira.Controls.Add(this.label4);
            this.grpJira.Controls.Add(this.txtJiraUrl);
            this.grpJira.Controls.Add(this.label1);
            this.grpJira.Controls.Add(this.label3);
            this.grpJira.Controls.Add(this.txtJiraUsername);
            this.grpJira.Location = new System.Drawing.Point(628, 46);
            this.grpJira.Name = "grpJira";
            this.grpJira.Size = new System.Drawing.Size(302, 151);
            this.grpJira.TabIndex = 4;
            this.grpJira.TabStop = false;
            this.grpJira.Text = "Jira Connection Settings";
            // 
            // chkUseRest
            // 
            this.chkUseRest.AutoSize = true;
            this.chkUseRest.Location = new System.Drawing.Point(102, 117);
            this.chkUseRest.Name = "chkUseRest";
            this.chkUseRest.Size = new System.Drawing.Size(15, 14);
            this.chkUseRest.TabIndex = 5;
            this.toolTip.SetToolTip(this.chkUseRest, "Using REST API Gives More Features, Using The Old API Will Result In Some Feature" +
        "s Not Working");
            this.chkUseRest.UseVisualStyleBackColor = true;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(5, 115);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(91, 17);
            this.label21.TabIndex = 4;
            this.label21.Text = "Use REST API?";
            // 
            // grpAppSettings
            // 
            this.grpAppSettings.Controls.Add(this.label23);
            this.grpAppSettings.Controls.Add(this.chkUsageTracking);
            this.grpAppSettings.Controls.Add(this.chkAutoUpdate);
            this.grpAppSettings.Controls.Add(this.label16);
            this.grpAppSettings.Controls.Add(this.label13);
            this.grpAppSettings.Controls.Add(this.label12);
            this.grpAppSettings.Controls.Add(this.chkAlwaysTop);
            this.grpAppSettings.Controls.Add(this.label11);
            this.grpAppSettings.Controls.Add(this.chkAlert);
            this.grpAppSettings.Controls.Add(this.txtTimerDays);
            this.grpAppSettings.Controls.Add(this.label5);
            this.grpAppSettings.Controls.Add(this.txtAlertMins);
            this.grpAppSettings.Controls.Add(this.label6);
            this.grpAppSettings.Controls.Add(this.label7);
            this.grpAppSettings.Location = new System.Drawing.Point(12, 46);
            this.grpAppSettings.Name = "grpAppSettings";
            this.grpAppSettings.Size = new System.Drawing.Size(302, 178);
            this.grpAppSettings.TabIndex = 1;
            this.grpAppSettings.TabStop = false;
            this.grpAppSettings.Text = "App Settings";
            // 
            // chkAutoUpdate
            // 
            this.chkAutoUpdate.AutoSize = true;
            this.chkAutoUpdate.Location = new System.Drawing.Point(178, 112);
            this.chkAutoUpdate.Name = "chkAutoUpdate";
            this.chkAutoUpdate.Size = new System.Drawing.Size(15, 14);
            this.chkAutoUpdate.TabIndex = 9;
            this.toolTip.SetToolTip(this.chkAutoUpdate, "UI Windows Always On Top (Not Main Window)");
            this.chkAutoUpdate.UseVisualStyleBackColor = true;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(84, 133);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(88, 17);
            this.label16.TabIndex = 10;
            this.label16.Text = "Auto Update?";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(219, 81);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(36, 17);
            this.label13.TabIndex = 7;
            this.label13.Text = "Days";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(219, 55);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(54, 17);
            this.label12.TabIndex = 4;
            this.label12.Text = "Minutes";
            // 
            // chkAlwaysTop
            // 
            this.chkAlwaysTop.AutoSize = true;
            this.chkAlwaysTop.Location = new System.Drawing.Point(178, 136);
            this.chkAlwaysTop.Name = "chkAlwaysTop";
            this.chkAlwaysTop.Size = new System.Drawing.Size(15, 14);
            this.chkAlwaysTop.TabIndex = 11;
            this.toolTip.SetToolTip(this.chkAlwaysTop, "UI Windows Always On Top (Not Main Window)");
            this.chkAlwaysTop.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(71, 109);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(101, 17);
            this.label11.TabIndex = 8;
            this.label11.Text = "Always On Top?";
            // 
            // chkAlert
            // 
            this.chkAlert.AutoSize = true;
            this.chkAlert.Location = new System.Drawing.Point(177, 26);
            this.chkAlert.Name = "chkAlert";
            this.chkAlert.Size = new System.Drawing.Size(15, 14);
            this.chkAlert.TabIndex = 1;
            this.toolTip.SetToolTip(this.chkAlert, "Tray Balloon When No Timers Running");
            this.chkAlert.UseVisualStyleBackColor = true;
            this.chkAlert.CheckedChanged += new System.EventHandler(this.chkAlert_CheckedChanged);
            // 
            // txtTimerDays
            // 
            this.txtTimerDays.Location = new System.Drawing.Point(177, 78);
            this.txtTimerDays.Name = "txtTimerDays";
            this.txtTimerDays.Size = new System.Drawing.Size(39, 25);
            this.txtTimerDays.TabIndex = 6;
            this.txtTimerDays.Text = "00";
            this.txtTimerDays.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip.SetToolTip(this.txtTimerDays, "Keep Timers In Gallifrey For X Days");
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(68, 80);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(104, 17);
            this.label5.TabIndex = 5;
            this.label5.Text = "Keep Timers For";
            // 
            // txtAlertMins
            // 
            this.txtAlertMins.Enabled = false;
            this.txtAlertMins.Location = new System.Drawing.Point(177, 48);
            this.txtAlertMins.Name = "txtAlertMins";
            this.txtAlertMins.Size = new System.Drawing.Size(39, 25);
            this.txtAlertMins.TabIndex = 3;
            this.txtAlertMins.Text = "00";
            this.txtAlertMins.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip.SetToolTip(this.txtAlertMins, "Alert Every X Minutes");
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(80, 49);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(92, 17);
            this.label6.TabIndex = 2;
            this.label6.Text = "Idle Time Alert";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(69, 22);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(103, 17);
            this.label7.TabIndex = 0;
            this.label7.Text = "Alert When Idle?";
            // 
            // txtTargetHours
            // 
            this.txtTargetHours.Location = new System.Drawing.Point(148, 24);
            this.txtTargetHours.Name = "txtTargetHours";
            this.txtTargetHours.Size = new System.Drawing.Size(39, 25);
            this.txtTargetHours.TabIndex = 1;
            this.txtTargetHours.Text = "00";
            this.txtTargetHours.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip.SetToolTip(this.txtTargetHours, "Target Export Hours Per Day");
            // 
            // txtTargetMins
            // 
            this.txtTargetMins.Location = new System.Drawing.Point(148, 55);
            this.txtTargetMins.Name = "txtTargetMins";
            this.txtTargetMins.Size = new System.Drawing.Size(39, 25);
            this.txtTargetMins.TabIndex = 3;
            this.txtTargetMins.Text = "00";
            this.txtTargetMins.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip.SetToolTip(this.txtTargetMins, "Target Export Minutes Per Day");
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(190, 27);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(43, 17);
            this.label9.TabIndex = 2;
            this.label9.Text = "Hours";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(190, 58);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(54, 17);
            this.label10.TabIndex = 4;
            this.label10.Text = "Minutes";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(11, 27);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(131, 17);
            this.label8.TabIndex = 0;
            this.label8.Text = " Export Time Per Day";
            // 
            // btnCancelEditSettings
            // 
            this.btnCancelEditSettings.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelEditSettings.Image = global::Gallifrey.UI.Classic.Properties.Resources.Cancel_48x48;
            this.btnCancelEditSettings.Location = new System.Drawing.Point(468, 367);
            this.btnCancelEditSettings.Name = "btnCancelEditSettings";
            this.btnCancelEditSettings.Size = new System.Drawing.Size(65, 65);
            this.btnCancelEditSettings.TabIndex = 7;
            this.btnCancelEditSettings.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip.SetToolTip(this.btnCancelEditSettings, "Cancel Editing Settings");
            this.btnCancelEditSettings.UseVisualStyleBackColor = true;
            this.btnCancelEditSettings.Click += new System.EventHandler(this.btnCancelEditSettings_Click);
            // 
            // btnSaveSettings
            // 
            this.btnSaveSettings.Image = global::Gallifrey.UI.Classic.Properties.Resources.Check_48x48;
            this.btnSaveSettings.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnSaveSettings.Location = new System.Drawing.Point(383, 367);
            this.btnSaveSettings.Name = "btnSaveSettings";
            this.btnSaveSettings.Size = new System.Drawing.Size(65, 65);
            this.btnSaveSettings.TabIndex = 6;
            this.toolTip.SetToolTip(this.btnSaveSettings, "Save Settings");
            this.btnSaveSettings.UseVisualStyleBackColor = true;
            this.btnSaveSettings.Click += new System.EventHandler(this.btnSaveSettings_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label15);
            this.groupBox1.Controls.Add(this.cmdWeekStart);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.chklstWorkingDays);
            this.groupBox1.Controls.Add(this.txtTargetMins);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.txtTargetHours);
            this.groupBox1.Location = new System.Drawing.Point(320, 46);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(302, 305);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Target Export";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(53, 239);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(89, 17);
            this.label15.TabIndex = 7;
            this.label15.Text = "Start Of Week";
            // 
            // cmdWeekStart
            // 
            this.cmdWeekStart.FormattingEnabled = true;
            this.cmdWeekStart.Items.AddRange(new object[] {
            "Monday",
            "Tuesday",
            "Wednesday",
            "Thursday",
            "Friday",
            "Saturday",
            "Sunday"});
            this.cmdWeekStart.Location = new System.Drawing.Point(148, 236);
            this.cmdWeekStart.Name = "cmdWeekStart";
            this.cmdWeekStart.Size = new System.Drawing.Size(148, 25);
            this.cmdWeekStart.TabIndex = 8;
            this.toolTip.SetToolTip(this.cmdWeekStart, "Start Of Week (For Export Per Week Target)");
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(53, 86);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(89, 17);
            this.label14.TabIndex = 5;
            this.label14.Text = "Working Days";
            // 
            // chklstWorkingDays
            // 
            this.chklstWorkingDays.CheckOnClick = true;
            this.chklstWorkingDays.FormattingEnabled = true;
            this.chklstWorkingDays.Items.AddRange(new object[] {
            "Monday",
            "Tuesday",
            "Wednesday",
            "Thursday",
            "Friday",
            "Saturday",
            "Sunday"});
            this.chklstWorkingDays.Location = new System.Drawing.Point(148, 86);
            this.chklstWorkingDays.Name = "chklstWorkingDays";
            this.chklstWorkingDays.Size = new System.Drawing.Size(148, 144);
            this.chklstWorkingDays.TabIndex = 6;
            this.toolTip.SetToolTip(this.chklstWorkingDays, "Valid Working Days To Export");
            // 
            // cmdDefaultExport
            // 
            this.cmdDefaultExport.FormattingEnabled = true;
            this.cmdDefaultExport.Items.AddRange(new object[] {
            "Automatically Adjust",
            "Leave Remaining",
            "Set To Value"});
            this.cmdDefaultExport.Location = new System.Drawing.Point(121, 24);
            this.cmdDefaultExport.Name = "cmdDefaultExport";
            this.cmdDefaultExport.Size = new System.Drawing.Size(175, 25);
            this.cmdDefaultExport.TabIndex = 1;
            this.toolTip.SetToolTip(this.cmdDefaultExport, "Sets What To Do With Remaining Time By Default");
            // 
            // txtCommentPrefix
            // 
            this.txtCommentPrefix.Location = new System.Drawing.Point(121, 55);
            this.txtCommentPrefix.Name = "txtCommentPrefix";
            this.txtCommentPrefix.Size = new System.Drawing.Size(175, 25);
            this.txtCommentPrefix.TabIndex = 3;
            this.toolTip.SetToolTip(this.txtCommentPrefix, "Comment Prefix, Fomat: \"Prefix: Comment\"");
            // 
            // chklstExportPrompt
            // 
            this.chklstExportPrompt.CheckOnClick = true;
            this.chklstExportPrompt.FormattingEnabled = true;
            this.chklstExportPrompt.Items.AddRange(new object[] {
            "Add Idle Time",
            "Manual Timer Adjustment",
            "Stop Timer",
            "Add Pre-Loaded Timer"});
            this.chklstExportPrompt.Location = new System.Drawing.Point(102, 20);
            this.chklstExportPrompt.Name = "chklstExportPrompt";
            this.chklstExportPrompt.Size = new System.Drawing.Size(194, 84);
            this.chklstExportPrompt.TabIndex = 1;
            this.toolTip.SetToolTip(this.chklstExportPrompt, "Valid Export Prompt Scenarios");
            // 
            // chkExportAll
            // 
            this.chkExportAll.AutoSize = true;
            this.chkExportAll.Location = new System.Drawing.Point(102, 110);
            this.chkExportAll.Name = "chkExportAll";
            this.chkExportAll.Size = new System.Drawing.Size(15, 14);
            this.chkExportAll.TabIndex = 3;
            this.toolTip.SetToolTip(this.chkExportAll, "Export Prompt Shows All Time Or Just Changed Time");
            this.chkExportAll.UseVisualStyleBackColor = true;
            // 
            // txtDefaultComment
            // 
            this.txtDefaultComment.Location = new System.Drawing.Point(121, 86);
            this.txtDefaultComment.Name = "txtDefaultComment";
            this.txtDefaultComment.Size = new System.Drawing.Size(175, 25);
            this.txtDefaultComment.TabIndex = 5;
            this.toolTip.SetToolTip(this.txtDefaultComment, "Default Comment When No Comment Is Entered");
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(6, 27);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(109, 17);
            this.label17.TabIndex = 0;
            this.label17.Text = "Remaining Action";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtDefaultComment);
            this.groupBox2.Controls.Add(this.label22);
            this.groupBox2.Controls.Add(this.txtCommentPrefix);
            this.groupBox2.Controls.Add(this.label18);
            this.groupBox2.Controls.Add(this.cmdDefaultExport);
            this.groupBox2.Controls.Add(this.label17);
            this.groupBox2.Location = new System.Drawing.Point(12, 230);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(302, 121);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Export Settings";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(6, 89);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(109, 17);
            this.label22.TabIndex = 4;
            this.label22.Text = "Default Comment";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(15, 58);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(100, 17);
            this.label18.TabIndex = 2;
            this.label18.Text = "Comment Prefix";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.chkExportAll);
            this.groupBox3.Controls.Add(this.label20);
            this.groupBox3.Controls.Add(this.label19);
            this.groupBox3.Controls.Add(this.chklstExportPrompt);
            this.groupBox3.Location = new System.Drawing.Point(628, 202);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(302, 149);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Export Prompt";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(26, 108);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(70, 17);
            this.label20.TabIndex = 2;
            this.label20.Text = "Export All?";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(51, 21);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(45, 17);
            this.label19.TabIndex = 0;
            this.label19.Text = "Events";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(2, 156);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(170, 17);
            this.label23.TabIndex = 12;
            this.label23.Text = "Anonymous Usage Tracking";
            // 
            // chkUsageTracking
            // 
            this.chkUsageTracking.AutoSize = true;
            this.chkUsageTracking.Location = new System.Drawing.Point(178, 159);
            this.chkUsageTracking.Name = "chkUsageTracking";
            this.chkUsageTracking.Size = new System.Drawing.Size(15, 14);
            this.chkUsageTracking.TabIndex = 13;
            this.toolTip.SetToolTip(this.chkUsageTracking, "Allow Gallifrey To Capture Anonymous App Usage Data");
            this.chkUsageTracking.UseVisualStyleBackColor = true;
            // 
            // SettingsWindow
            // 
            this.AcceptButton = this.btnSaveSettings;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancelEditSettings;
            this.ClientSize = new System.Drawing.Size(943, 444);
            this.ControlBox = false;
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.grpAppSettings);
            this.Controls.Add(this.grpJira);
            this.Controls.Add(this.btnCancelEditSettings);
            this.Controls.Add(this.btnSaveSettings);
            this.Controls.Add(this.label2);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsWindow";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Gallifrey - Settings";
            this.TopMost = true;
            this.grpJira.ResumeLayout(false);
            this.grpJira.PerformLayout();
            this.grpAppSettings.ResumeLayout(false);
            this.grpAppSettings.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
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
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.ComboBox cmdWeekStart;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.CheckedListBox chklstWorkingDays;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.CheckBox chkAutoUpdate;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ComboBox cmdDefaultExport;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtCommentPrefix;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.CheckedListBox chklstExportPrompt;
        private System.Windows.Forms.CheckBox chkExportAll;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.CheckBox chkUseRest;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.TextBox txtDefaultComment;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.CheckBox chkUsageTracking;
    }
}