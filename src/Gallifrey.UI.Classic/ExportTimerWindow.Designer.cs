namespace Gallifrey.UI.Classic
{
    partial class ExportTimerWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportTimerWindow));
            this.txtJiraRef = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtTotalHours = new System.Windows.Forms.TextBox();
            this.txtTotalMinutes = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label19 = new System.Windows.Forms.Label();
            this.txtRemainingHours = new System.Windows.Forms.TextBox();
            this.txtRemainingMinutes = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.txtSetValueHours = new System.Windows.Forms.TextBox();
            this.radAutoAdjust = new System.Windows.Forms.RadioButton();
            this.txtSetValueMins = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.radLeaveRemaining = new System.Windows.Forms.RadioButton();
            this.label14 = new System.Windows.Forms.Label();
            this.radSetValue = new System.Windows.Forms.RadioButton();
            this.label6 = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label15 = new System.Windows.Forms.Label();
            this.calExportDate = new System.Windows.Forms.DateTimePicker();
            this.label10 = new System.Windows.Forms.Label();
            this.txtExportHours = new System.Windows.Forms.TextBox();
            this.txtExportMins = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtExportedHours = new System.Windows.Forms.TextBox();
            this.txtExportedMins = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.lblParentDesc = new System.Windows.Forms.Label();
            this.txtParentDesc = new System.Windows.Forms.TextBox();
            this.lblParentRef = new System.Windows.Forms.Label();
            this.txtParentRef = new System.Windows.Forms.TextBox();
            this.txtComment = new ExtendedTextBox.ExtTextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtJiraRef
            // 
            this.txtJiraRef.Enabled = false;
            this.txtJiraRef.Location = new System.Drawing.Point(108, 47);
            this.txtJiraRef.Name = "txtJiraRef";
            this.txtJiraRef.Size = new System.Drawing.Size(173, 25);
            this.txtJiraRef.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 17);
            this.label1.TabIndex = 5;
            this.label1.Text = "Jira Reference";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(122, 25);
            this.label2.TabIndex = 4;
            this.label2.Text = "Export Timer";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(39, 31);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 17);
            this.label3.TabIndex = 0;
            this.label3.Text = "Total";
            // 
            // txtTotalHours
            // 
            this.txtTotalHours.Enabled = false;
            this.txtTotalHours.Location = new System.Drawing.Point(86, 28);
            this.txtTotalHours.Name = "txtTotalHours";
            this.txtTotalHours.Size = new System.Drawing.Size(39, 25);
            this.txtTotalHours.TabIndex = 1;
            this.txtTotalHours.Text = "00";
            this.txtTotalHours.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip.SetToolTip(this.txtTotalHours, "Hours Clocked In Timer");
            // 
            // txtTotalMinutes
            // 
            this.txtTotalMinutes.Enabled = false;
            this.txtTotalMinutes.Location = new System.Drawing.Point(172, 28);
            this.txtTotalMinutes.Name = "txtTotalMinutes";
            this.txtTotalMinutes.Size = new System.Drawing.Size(39, 25);
            this.txtTotalMinutes.TabIndex = 3;
            this.txtTotalMinutes.Text = "00";
            this.txtTotalMinutes.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip.SetToolTip(this.txtTotalMinutes, "Minutes Clocked In Timer");
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(124, 31);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 17);
            this.label4.TabIndex = 2;
            this.label4.Text = "Hours";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(217, 31);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(54, 17);
            this.label5.TabIndex = 4;
            this.label5.Text = "Minutes";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label19);
            this.groupBox1.Controls.Add(this.txtRemainingHours);
            this.groupBox1.Controls.Add(this.txtRemainingMinutes);
            this.groupBox1.Controls.Add(this.label17);
            this.groupBox1.Controls.Add(this.label18);
            this.groupBox1.Controls.Add(this.txtSetValueHours);
            this.groupBox1.Controls.Add(this.radAutoAdjust);
            this.groupBox1.Controls.Add(this.txtSetValueMins);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.radLeaveRemaining);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.radSetValue);
            this.groupBox1.Location = new System.Drawing.Point(284, 172);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(293, 173);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Remaining Time";
            this.toolTip.SetToolTip(this.groupBox1, "Set Action For Remaining Time");
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(7, 27);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(69, 17);
            this.label19.TabIndex = 0;
            this.label19.Text = "Remaining";
            // 
            // txtRemainingHours
            // 
            this.txtRemainingHours.Enabled = false;
            this.txtRemainingHours.Location = new System.Drawing.Point(85, 24);
            this.txtRemainingHours.Name = "txtRemainingHours";
            this.txtRemainingHours.Size = new System.Drawing.Size(39, 25);
            this.txtRemainingHours.TabIndex = 1;
            this.txtRemainingHours.Text = "00";
            this.txtRemainingHours.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip.SetToolTip(this.txtRemainingHours, "Remaining Hours");
            // 
            // txtRemainingMinutes
            // 
            this.txtRemainingMinutes.Enabled = false;
            this.txtRemainingMinutes.Location = new System.Drawing.Point(172, 24);
            this.txtRemainingMinutes.Name = "txtRemainingMinutes";
            this.txtRemainingMinutes.Size = new System.Drawing.Size(39, 25);
            this.txtRemainingMinutes.TabIndex = 3;
            this.txtRemainingMinutes.Text = "00";
            this.txtRemainingMinutes.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip.SetToolTip(this.txtRemainingMinutes, "Remaining Minutes");
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(123, 27);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(43, 17);
            this.label17.TabIndex = 2;
            this.label17.Text = "Hours";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(217, 27);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(54, 17);
            this.label18.TabIndex = 4;
            this.label18.Text = "Minutes";
            // 
            // txtSetValueHours
            // 
            this.txtSetValueHours.Location = new System.Drawing.Point(34, 137);
            this.txtSetValueHours.Name = "txtSetValueHours";
            this.txtSetValueHours.Size = new System.Drawing.Size(39, 25);
            this.txtSetValueHours.TabIndex = 8;
            this.txtSetValueHours.Text = "00";
            this.txtSetValueHours.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip.SetToolTip(this.txtSetValueHours, "Remaining Hours");
            // 
            // radAutoAdjust
            // 
            this.radAutoAdjust.AutoSize = true;
            this.radAutoAdjust.Location = new System.Drawing.Point(10, 55);
            this.radAutoAdjust.Name = "radAutoAdjust";
            this.radAutoAdjust.Size = new System.Drawing.Size(143, 21);
            this.radAutoAdjust.TabIndex = 5;
            this.radAutoAdjust.Text = "Automatically Adjust";
            this.radAutoAdjust.UseVisualStyleBackColor = true;
            // 
            // txtSetValueMins
            // 
            this.txtSetValueMins.Location = new System.Drawing.Point(121, 137);
            this.txtSetValueMins.Name = "txtSetValueMins";
            this.txtSetValueMins.Size = new System.Drawing.Size(39, 25);
            this.txtSetValueMins.TabIndex = 10;
            this.txtSetValueMins.Text = "00";
            this.txtSetValueMins.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip.SetToolTip(this.txtSetValueMins, "Remaining Minutes");
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(72, 140);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(43, 17);
            this.label13.TabIndex = 9;
            this.label13.Text = "Hours";
            // 
            // radLeaveRemaining
            // 
            this.radLeaveRemaining.AutoSize = true;
            this.radLeaveRemaining.Location = new System.Drawing.Point(10, 82);
            this.radLeaveRemaining.Name = "radLeaveRemaining";
            this.radLeaveRemaining.Size = new System.Drawing.Size(124, 21);
            this.radLeaveRemaining.TabIndex = 6;
            this.radLeaveRemaining.Text = "Leave Remaining";
            this.radLeaveRemaining.UseVisualStyleBackColor = true;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(166, 140);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(54, 17);
            this.label14.TabIndex = 11;
            this.label14.Text = "Minutes";
            // 
            // radSetValue
            // 
            this.radSetValue.AutoSize = true;
            this.radSetValue.Checked = true;
            this.radSetValue.Location = new System.Drawing.Point(10, 109);
            this.radSetValue.Name = "radSetValue";
            this.radSetValue.Size = new System.Drawing.Size(99, 21);
            this.radSetValue.TabIndex = 7;
            this.radSetValue.TabStop = true;
            this.radSetValue.Text = "Set To Value";
            this.radSetValue.UseVisualStyleBackColor = true;
            this.radSetValue.CheckedChanged += new System.EventHandler(this.radSetValue_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(4, 81);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(98, 17);
            this.label6.TabIndex = 7;
            this.label6.Text = "Jira Description";
            // 
            // txtDescription
            // 
            this.txtDescription.Enabled = false;
            this.txtDescription.Location = new System.Drawing.Point(108, 78);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(173, 81);
            this.txtDescription.TabIndex = 8;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label15);
            this.groupBox2.Controls.Add(this.calExportDate);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.txtExportHours);
            this.groupBox2.Controls.Add(this.txtExportMins);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.txtExportedHours);
            this.groupBox2.Controls.Add(this.txtExportedMins);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.txtTotalHours);
            this.groupBox2.Controls.Add(this.txtTotalMinutes);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Location = new System.Drawing.Point(7, 172);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(274, 173);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = " Export Info";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(7, 127);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(77, 17);
            this.label15.TabIndex = 15;
            this.label15.Text = "Export Date";
            // 
            // calExportDate
            // 
            this.calExportDate.CustomFormat = "dd/MM/yyyy HH:mm";
            this.calExportDate.Enabled = false;
            this.calExportDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.calExportDate.Location = new System.Drawing.Point(86, 121);
            this.calExportDate.Name = "calExportDate";
            this.calExportDate.Size = new System.Drawing.Size(182, 25);
            this.calExportDate.TabIndex = 16;
            this.toolTip.SetToolTip(this.calExportDate, "Enter Date/Time For Exported Time");
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(11, 93);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(65, 17);
            this.label10.TabIndex = 10;
            this.label10.Text = "To Export";
            // 
            // txtExportHours
            // 
            this.txtExportHours.Location = new System.Drawing.Point(86, 90);
            this.txtExportHours.Name = "txtExportHours";
            this.txtExportHours.Size = new System.Drawing.Size(39, 25);
            this.txtExportHours.TabIndex = 11;
            this.txtExportHours.Text = "00";
            this.txtExportHours.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip.SetToolTip(this.txtExportHours, "Enter Hours To Export");
            // 
            // txtExportMins
            // 
            this.txtExportMins.Location = new System.Drawing.Point(172, 90);
            this.txtExportMins.Name = "txtExportMins";
            this.txtExportMins.Size = new System.Drawing.Size(39, 25);
            this.txtExportMins.TabIndex = 13;
            this.txtExportMins.Text = "00";
            this.txtExportMins.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip.SetToolTip(this.txtExportMins, "Enter Minutes To Export");
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(124, 93);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(43, 17);
            this.label11.TabIndex = 12;
            this.label11.Text = "Hours";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(217, 93);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(54, 17);
            this.label12.TabIndex = 14;
            this.label12.Text = "Minutes";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(15, 62);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(61, 17);
            this.label7.TabIndex = 5;
            this.label7.Text = "Exported";
            // 
            // txtExportedHours
            // 
            this.txtExportedHours.Enabled = false;
            this.txtExportedHours.Location = new System.Drawing.Point(86, 59);
            this.txtExportedHours.Name = "txtExportedHours";
            this.txtExportedHours.Size = new System.Drawing.Size(39, 25);
            this.txtExportedHours.TabIndex = 6;
            this.txtExportedHours.Text = "00";
            this.txtExportedHours.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip.SetToolTip(this.txtExportedHours, "Hours Already Exported");
            // 
            // txtExportedMins
            // 
            this.txtExportedMins.Enabled = false;
            this.txtExportedMins.Location = new System.Drawing.Point(172, 59);
            this.txtExportedMins.Name = "txtExportedMins";
            this.txtExportedMins.Size = new System.Drawing.Size(39, 25);
            this.txtExportedMins.TabIndex = 8;
            this.txtExportedMins.Text = "00";
            this.txtExportedMins.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip.SetToolTip(this.txtExportedMins, "Minutes Already Exported");
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(124, 62);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(43, 17);
            this.label8.TabIndex = 7;
            this.label8.Text = "Hours";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(217, 62);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(54, 17);
            this.label9.TabIndex = 9;
            this.label9.Text = "Minutes";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(12, 348);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(64, 17);
            this.label16.TabIndex = 0;
            this.label16.Text = "Comment";
            // 
            // btnOK
            // 
            this.btnOK.Image = global::Gallifrey.UI.Classic.Properties.Resources.Check_48x48;
            this.btnOK.Location = new System.Drawing.Point(210, 438);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(65, 65);
            this.btnOK.TabIndex = 2;
            this.btnOK.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip.SetToolTip(this.btnOK, "Export To Jira");
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::Gallifrey.UI.Classic.Properties.Resources.Cancel_48x48;
            this.btnCancel.Location = new System.Drawing.Point(318, 438);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(65, 65);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip.SetToolTip(this.btnCancel, "Cancel Export");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblParentDesc
            // 
            this.lblParentDesc.AutoSize = true;
            this.lblParentDesc.Location = new System.Drawing.Point(283, 81);
            this.lblParentDesc.Name = "lblParentDesc";
            this.lblParentDesc.Size = new System.Drawing.Size(115, 17);
            this.lblParentDesc.TabIndex = 11;
            this.lblParentDesc.Text = "Parent Description";
            // 
            // txtParentDesc
            // 
            this.txtParentDesc.Enabled = false;
            this.txtParentDesc.Location = new System.Drawing.Point(404, 78);
            this.txtParentDesc.Multiline = true;
            this.txtParentDesc.Name = "txtParentDesc";
            this.txtParentDesc.Size = new System.Drawing.Size(173, 81);
            this.txtParentDesc.TabIndex = 12;
            // 
            // lblParentRef
            // 
            this.lblParentRef.AutoSize = true;
            this.lblParentRef.Location = new System.Drawing.Point(291, 50);
            this.lblParentRef.Name = "lblParentRef";
            this.lblParentRef.Size = new System.Drawing.Size(107, 17);
            this.lblParentRef.TabIndex = 9;
            this.lblParentRef.Text = "Parent Reference";
            // 
            // txtParentRef
            // 
            this.txtParentRef.Enabled = false;
            this.txtParentRef.Location = new System.Drawing.Point(404, 47);
            this.txtParentRef.Name = "txtParentRef";
            this.txtParentRef.Size = new System.Drawing.Size(173, 25);
            this.txtParentRef.TabIndex = 10;
            // 
            // txtComment
            // 
            this.txtComment.AcceptsReturn = true;
            this.txtComment.AcceptsTab = true;
            this.txtComment.AutoSize = true;
            this.txtComment.ChangedColour = System.Drawing.SystemColors.Window;
            this.txtComment.Location = new System.Drawing.Point(82, 351);
            this.txtComment.MaxLength = 0;
            this.txtComment.Name = "txtComment";
            this.txtComment.OriginalText = "";
            this.txtComment.Size = new System.Drawing.Size(493, 81);
            this.txtComment.SpellCheck = true;
            this.txtComment.TabIndex = 1;
            this.txtComment.TextCase = System.Windows.Forms.CharacterCasing.Normal;
            this.txtComment.TextType = ExtendedTextBox.ExtTextBox.TextTypes.String;
            this.toolTip.SetToolTip(this.txtComment, "Comment To Be Added To Work Log");
            this.txtComment.Wrapping = false;
            // 
            // ExportTimerWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(587, 520);
            this.ControlBox = false;
            this.Controls.Add(this.txtComment);
            this.Controls.Add(this.lblParentDesc);
            this.Controls.Add(this.txtParentDesc);
            this.Controls.Add(this.lblParentRef);
            this.Controls.Add(this.txtParentRef);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtJiraRef);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExportTimerWindow";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Gallifrey - Export Time";
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ExportTimerWindow_KeyUp);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtJiraRef;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtTotalHours;
        private System.Windows.Forms.TextBox txtTotalMinutes;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radLeaveRemaining;
        private System.Windows.Forms.RadioButton radSetValue;
        private System.Windows.Forms.TextBox txtSetValueHours;
        private System.Windows.Forms.RadioButton radAutoAdjust;
        private System.Windows.Forms.TextBox txtSetValueMins;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtExportHours;
        private System.Windows.Forms.TextBox txtExportMins;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtExportedHours;
        private System.Windows.Forms.TextBox txtExportedMins;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.DateTimePicker calExportDate;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox txtRemainingHours;
        private System.Windows.Forms.TextBox txtRemainingMinutes;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label lblParentDesc;
        private System.Windows.Forms.TextBox txtParentDesc;
        private System.Windows.Forms.Label lblParentRef;
        private System.Windows.Forms.TextBox txtParentRef;
        private ExtendedTextBox.ExtTextBox txtComment;
    }
}