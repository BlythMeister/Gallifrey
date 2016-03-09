namespace Gallifrey.UI.Classic
{
    partial class AddTimerWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddTimerWindow));
            this.txtJiraRef = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.calStartDate = new System.Windows.Forms.DateTimePicker();
            this.txtStartHours = new System.Windows.Forms.TextBox();
            this.txtStartMins = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.btnCancelAddTimer = new System.Windows.Forms.Button();
            this.btnAddTimer = new System.Windows.Forms.Button();
            this.chkStartNow = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.btnSearch = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.label8 = new System.Windows.Forms.Label();
            this.chkInProgress = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.chkAssign = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // txtJiraRef
            // 
            this.txtJiraRef.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtJiraRef.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtJiraRef.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtJiraRef.Location = new System.Drawing.Point(123, 47);
            this.txtJiraRef.Name = "txtJiraRef";
            this.txtJiraRef.Size = new System.Drawing.Size(249, 25);
            this.txtJiraRef.TabIndex = 2;
            this.toolTip.SetToolTip(this.txtJiraRef, "Enter Jira Reference For New Timer");
            this.txtJiraRef.TextChanged += new System.EventHandler(this.txtJiraRef_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Jira Reference";
            // 
            // calStartDate
            // 
            this.calStartDate.Location = new System.Drawing.Point(123, 78);
            this.calStartDate.Name = "calStartDate";
            this.calStartDate.Size = new System.Drawing.Size(249, 25);
            this.calStartDate.TabIndex = 4;
            this.toolTip.SetToolTip(this.calStartDate, "Choose Valid Date For Timer");
            this.calStartDate.ValueChanged += new System.EventHandler(this.calStartDate_ValueChanged);
            // 
            // txtStartHours
            // 
            this.txtStartHours.Location = new System.Drawing.Point(123, 109);
            this.txtStartHours.Name = "txtStartHours";
            this.txtStartHours.Size = new System.Drawing.Size(30, 25);
            this.txtStartHours.TabIndex = 6;
            this.txtStartHours.Text = "00";
            this.txtStartHours.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip.SetToolTip(this.txtStartHours, "Pre Load Timer With Hours");
            // 
            // txtStartMins
            // 
            this.txtStartMins.Location = new System.Drawing.Point(208, 109);
            this.txtStartMins.Name = "txtStartMins";
            this.txtStartMins.Size = new System.Drawing.Size(30, 25);
            this.txtStartMins.TabIndex = 8;
            this.txtStartMins.Text = "00";
            this.txtStartMins.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip.SetToolTip(this.txtStartMins, "Pre Load Timer With Minutes");
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(105, 25);
            this.label2.TabIndex = 0;
            this.label2.Text = "New Timer";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(35, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 17);
            this.label3.TabIndex = 3;
            this.label3.Text = "Starting Date";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 112);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(107, 17);
            this.label4.TabIndex = 5;
            this.label4.Text = "Pre Loaded Time";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(159, 112);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(43, 17);
            this.label5.TabIndex = 7;
            this.label5.Text = "Hours";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(244, 112);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(36, 17);
            this.label6.TabIndex = 9;
            this.label6.Text = "Mins";
            // 
            // btnCancelAddTimer
            // 
            this.btnCancelAddTimer.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelAddTimer.Image = global::Gallifrey.UI.Classic.Properties.Resources.Cancel_48x48;
            this.btnCancelAddTimer.Location = new System.Drawing.Point(162, 204);
            this.btnCancelAddTimer.Name = "btnCancelAddTimer";
            this.btnCancelAddTimer.Size = new System.Drawing.Size(65, 65);
            this.btnCancelAddTimer.TabIndex = 13;
            this.btnCancelAddTimer.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip.SetToolTip(this.btnCancelAddTimer, "Cancel");
            this.btnCancelAddTimer.UseVisualStyleBackColor = true;
            this.btnCancelAddTimer.Click += new System.EventHandler(this.btnCancelAddTimer_Click);
            // 
            // btnAddTimer
            // 
            this.btnAddTimer.Image = global::Gallifrey.UI.Classic.Properties.Resources.Check_48x48;
            this.btnAddTimer.Location = new System.Drawing.Point(87, 204);
            this.btnAddTimer.Name = "btnAddTimer";
            this.btnAddTimer.Size = new System.Drawing.Size(65, 65);
            this.btnAddTimer.TabIndex = 12;
            this.btnAddTimer.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip.SetToolTip(this.btnAddTimer, "Add Timer");
            this.btnAddTimer.UseVisualStyleBackColor = true;
            this.btnAddTimer.Click += new System.EventHandler(this.btnAddTimer_Click);
            // 
            // chkStartNow
            // 
            this.chkStartNow.AutoSize = true;
            this.chkStartNow.Location = new System.Drawing.Point(123, 140);
            this.chkStartNow.Name = "chkStartNow";
            this.chkStartNow.Size = new System.Drawing.Size(15, 14);
            this.chkStartNow.TabIndex = 11;
            this.toolTip.SetToolTip(this.chkStartNow, "Tick To Start Timer Straight Away");
            this.chkStartNow.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(45, 138);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(72, 17);
            this.label7.TabIndex = 10;
            this.label7.Text = "Start Now?";
            // 
            // btnSearch
            // 
            this.btnSearch.Image = global::Gallifrey.UI.Classic.Properties.Resources.Search_48x48;
            this.btnSearch.Location = new System.Drawing.Point(233, 204);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(65, 65);
            this.btnSearch.TabIndex = 14;
            this.btnSearch.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip.SetToolTip(this.btnSearch, "Search Jira");
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(15, 176);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(102, 17);
            this.label8.TabIndex = 15;
            this.label8.Text = "Set In Progress?";
            // 
            // chkInProgress
            // 
            this.chkInProgress.AutoSize = true;
            this.chkInProgress.Location = new System.Drawing.Point(123, 178);
            this.chkInProgress.Name = "chkInProgress";
            this.chkInProgress.Size = new System.Drawing.Size(15, 14);
            this.chkInProgress.TabIndex = 16;
            this.toolTip.SetToolTip(this.chkInProgress, "Tick To Start Timer Straight Away");
            this.chkInProgress.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(23, 157);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(94, 17);
            this.label9.TabIndex = 17;
            this.label9.Text = "Assign To Me?";
            // 
            // chkAssign
            // 
            this.chkAssign.AutoSize = true;
            this.chkAssign.Location = new System.Drawing.Point(123, 158);
            this.chkAssign.Name = "chkAssign";
            this.chkAssign.Size = new System.Drawing.Size(15, 14);
            this.chkAssign.TabIndex = 18;
            this.toolTip.SetToolTip(this.chkAssign, "Tick To Start Timer Straight Away");
            this.chkAssign.UseVisualStyleBackColor = true;
            // 
            // AddTimerWindow
            // 
            this.AcceptButton = this.btnAddTimer;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancelAddTimer;
            this.ClientSize = new System.Drawing.Size(384, 283);
            this.ControlBox = false;
            this.Controls.Add(this.label9);
            this.Controls.Add(this.chkAssign);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.chkInProgress);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.chkStartNow);
            this.Controls.Add(this.btnCancelAddTimer);
            this.Controls.Add(this.btnAddTimer);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtStartMins);
            this.Controls.Add(this.txtStartHours);
            this.Controls.Add(this.calStartDate);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtJiraRef);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddTimerWindow";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Gallifrey - Add Timer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtJiraRef;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker calStartDate;
        private System.Windows.Forms.TextBox txtStartHours;
        private System.Windows.Forms.TextBox txtStartMins;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnAddTimer;
        private System.Windows.Forms.Button btnCancelAddTimer;
        private System.Windows.Forms.CheckBox chkStartNow;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox chkInProgress;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox chkAssign;
    }
}