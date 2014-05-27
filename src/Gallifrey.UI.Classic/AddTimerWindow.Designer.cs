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
            this.SuspendLayout();
            // 
            // txtJiraRef
            // 
            this.txtJiraRef.Location = new System.Drawing.Point(123, 47);
            this.txtJiraRef.Name = "txtJiraRef";
            this.txtJiraRef.Size = new System.Drawing.Size(157, 25);
            this.txtJiraRef.TabIndex = 0;
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
            this.calStartDate.Size = new System.Drawing.Size(157, 25);
            this.calStartDate.TabIndex = 2;
            this.calStartDate.ValueChanged += new System.EventHandler(this.calStartDate_ValueChanged);
            // 
            // txtStartHours
            // 
            this.txtStartHours.Location = new System.Drawing.Point(123, 109);
            this.txtStartHours.Name = "txtStartHours";
            this.txtStartHours.Size = new System.Drawing.Size(30, 25);
            this.txtStartHours.TabIndex = 3;
            this.txtStartHours.Text = "00";
            this.txtStartHours.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtStartMins
            // 
            this.txtStartMins.Location = new System.Drawing.Point(208, 109);
            this.txtStartMins.Name = "txtStartMins";
            this.txtStartMins.Size = new System.Drawing.Size(30, 25);
            this.txtStartMins.TabIndex = 4;
            this.txtStartMins.Text = "00";
            this.txtStartMins.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(105, 25);
            this.label2.TabIndex = 5;
            this.label2.Text = "New Timer";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(35, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 17);
            this.label3.TabIndex = 6;
            this.label3.Text = "Starting Date";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 112);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(107, 17);
            this.label4.TabIndex = 7;
            this.label4.Text = "Pre Loaded Time";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(159, 112);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(43, 17);
            this.label5.TabIndex = 8;
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
            this.btnCancelAddTimer.Image = global::Gallifrey.UI.Classic.Properties.Resources.Cancel_48x48;
            this.btnCancelAddTimer.Location = new System.Drawing.Point(173, 172);
            this.btnCancelAddTimer.Name = "btnCancelAddTimer";
            this.btnCancelAddTimer.Size = new System.Drawing.Size(65, 65);
            this.btnCancelAddTimer.TabIndex = 11;
            this.btnCancelAddTimer.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnCancelAddTimer.UseVisualStyleBackColor = true;
            this.btnCancelAddTimer.Click += new System.EventHandler(this.btnCancelAddTimer_Click);
            // 
            // btnAddTimer
            // 
            this.btnAddTimer.Image = global::Gallifrey.UI.Classic.Properties.Resources.Check_48x48;
            this.btnAddTimer.Location = new System.Drawing.Point(60, 172);
            this.btnAddTimer.Name = "btnAddTimer";
            this.btnAddTimer.Size = new System.Drawing.Size(65, 65);
            this.btnAddTimer.TabIndex = 10;
            this.btnAddTimer.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnAddTimer.UseVisualStyleBackColor = true;
            this.btnAddTimer.Click += new System.EventHandler(this.btnAddTimer_Click);
            // 
            // chkStartNow
            // 
            this.chkStartNow.AutoSize = true;
            this.chkStartNow.Location = new System.Drawing.Point(123, 140);
            this.chkStartNow.Name = "chkStartNow";
            this.chkStartNow.Size = new System.Drawing.Size(15, 14);
            this.chkStartNow.TabIndex = 12;
            this.chkStartNow.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(45, 138);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(72, 17);
            this.label7.TabIndex = 13;
            this.label7.Text = "Start Now?";
            // 
            // AddTimerWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(296, 251);
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
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximumSize = new System.Drawing.Size(312, 289);
            this.MinimumSize = new System.Drawing.Size(312, 289);
            this.Name = "AddTimerWindow";
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
    }
}