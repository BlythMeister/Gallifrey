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
            this.lblVersion = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tabTimerDays = new System.Windows.Forms.TabControl();
            this.formTimer = new System.Windows.Forms.Timer(this.components);
            this.lblCurrentTime = new System.Windows.Forms.Label();
            this.btnRename = new System.Windows.Forms.Button();
            this.btnTimeEdit = new System.Windows.Forms.Button();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnRemoveTimer = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnAddTimer = new System.Windows.Forms.Button();
            this.grpTarget = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.grpExportStats = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.grpTarget.SuspendLayout();
            this.grpExportStats.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(92, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(163, 50);
            this.label1.TabIndex = 1;
            this.label1.Text = "Gallifrey";
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVersion.Location = new System.Drawing.Point(98, 79);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(92, 13);
            this.lblVersion.TabIndex = 3;
            this.lblVersion.Text = "v0.0.0.0 (manual)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(98, 59);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(183, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "Jira Time Logging Companion";
            // 
            // tabTimerDays
            // 
            this.tabTimerDays.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabTimerDays.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabTimerDays.Location = new System.Drawing.Point(12, 169);
            this.tabTimerDays.Name = "tabTimerDays";
            this.tabTimerDays.SelectedIndex = 0;
            this.tabTimerDays.Size = new System.Drawing.Size(784, 414);
            this.tabTimerDays.TabIndex = 6;
            // 
            // formTimer
            // 
            this.formTimer.Interval = 500;
            this.formTimer.Tick += new System.EventHandler(this.formTimer_Tick);
            // 
            // lblCurrentTime
            // 
            this.lblCurrentTime.BackColor = System.Drawing.Color.Black;
            this.lblCurrentTime.Font = new System.Drawing.Font("Stencil", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentTime.ForeColor = System.Drawing.Color.Red;
            this.lblCurrentTime.Location = new System.Drawing.Point(510, 98);
            this.lblCurrentTime.Name = "lblCurrentTime";
            this.lblCurrentTime.Size = new System.Drawing.Size(286, 65);
            this.lblCurrentTime.TabIndex = 12;
            this.lblCurrentTime.Text = "00:00:00";
            this.lblCurrentTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnRename
            // 
            this.btnRename.Image = global::Gallifrey.UI.Classic.Properties.Resources.Rename_48x48;
            this.btnRename.Location = new System.Drawing.Point(296, 98);
            this.btnRename.Name = "btnRename";
            this.btnRename.Size = new System.Drawing.Size(65, 65);
            this.btnRename.TabIndex = 11;
            this.btnRename.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnRename.UseVisualStyleBackColor = true;
            this.btnRename.Click += new System.EventHandler(this.btnRename_Click);
            // 
            // btnTimeEdit
            // 
            this.btnTimeEdit.Image = global::Gallifrey.UI.Classic.Properties.Resources.Edit_48x48;
            this.btnTimeEdit.Location = new System.Drawing.Point(225, 98);
            this.btnTimeEdit.Name = "btnTimeEdit";
            this.btnTimeEdit.Size = new System.Drawing.Size(65, 65);
            this.btnTimeEdit.TabIndex = 10;
            this.btnTimeEdit.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnTimeEdit.UseVisualStyleBackColor = true;
            // 
            // btnSearch
            // 
            this.btnSearch.Image = global::Gallifrey.UI.Classic.Properties.Resources.Search_48x48;
            this.btnSearch.Location = new System.Drawing.Point(154, 98);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(65, 65);
            this.btnSearch.TabIndex = 9;
            this.btnSearch.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnSearch.UseVisualStyleBackColor = true;
            // 
            // btnExport
            // 
            this.btnExport.Image = global::Gallifrey.UI.Classic.Properties.Resources.Synchronize_48x48;
            this.btnExport.Location = new System.Drawing.Point(367, 98);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(65, 65);
            this.btnExport.TabIndex = 8;
            this.btnExport.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnExport.UseVisualStyleBackColor = true;
            // 
            // btnSettings
            // 
            this.btnSettings.Image = global::Gallifrey.UI.Classic.Properties.Resources.Settings_48x48;
            this.btnSettings.Location = new System.Drawing.Point(438, 98);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(65, 65);
            this.btnSettings.TabIndex = 7;
            this.btnSettings.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // btnRemoveTimer
            // 
            this.btnRemoveTimer.Image = global::Gallifrey.UI.Classic.Properties.Resources.Delete_48x48;
            this.btnRemoveTimer.Location = new System.Drawing.Point(83, 98);
            this.btnRemoveTimer.Name = "btnRemoveTimer";
            this.btnRemoveTimer.Size = new System.Drawing.Size(65, 65);
            this.btnRemoveTimer.TabIndex = 5;
            this.btnRemoveTimer.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnRemoveTimer.UseVisualStyleBackColor = true;
            this.btnRemoveTimer.Click += new System.EventHandler(this.btnRemoveTimer_Click);
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
            // btnAddTimer
            // 
            this.btnAddTimer.Image = global::Gallifrey.UI.Classic.Properties.Resources.Add_48x48;
            this.btnAddTimer.Location = new System.Drawing.Point(12, 98);
            this.btnAddTimer.Name = "btnAddTimer";
            this.btnAddTimer.Size = new System.Drawing.Size(65, 65);
            this.btnAddTimer.TabIndex = 0;
            this.btnAddTimer.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnAddTimer.UseVisualStyleBackColor = true;
            this.btnAddTimer.Click += new System.EventHandler(this.btnAddTimer_Click);
            // 
            // grpTarget
            // 
            this.grpTarget.Controls.Add(this.progressBar1);
            this.grpTarget.Controls.Add(this.label4);
            this.grpTarget.Controls.Add(this.label2);
            this.grpTarget.Location = new System.Drawing.Point(511, 9);
            this.grpTarget.Name = "grpTarget";
            this.grpTarget.Size = new System.Drawing.Size(285, 83);
            this.grpTarget.TabIndex = 13;
            this.grpTarget.TabStop = false;
            this.grpTarget.Text = "Target Export Week To Date";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(6, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 17);
            this.label2.TabIndex = 14;
            this.label2.Text = "Target: 10:00";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(107, 18);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(92, 17);
            this.label4.TabIndex = 15;
            this.label4.Text = "Exported: 0:00";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(9, 42);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(270, 35);
            this.progressBar1.TabIndex = 16;
            // 
            // grpExportStats
            // 
            this.grpExportStats.Controls.Add(this.label6);
            this.grpExportStats.Controls.Add(this.label5);
            this.grpExportStats.Location = new System.Drawing.Point(296, 12);
            this.grpExportStats.Name = "grpExportStats";
            this.grpExportStats.Size = new System.Drawing.Size(207, 80);
            this.grpExportStats.TabIndex = 14;
            this.grpExportStats.TabStop = false;
            this.grpExportStats.Text = "Export Stats";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(6, 23);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(87, 17);
            this.label5.TabIndex = 17;
            this.label5.Text = "Exported: 0/0";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(6, 47);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(145, 17);
            this.label6.TabIndex = 18;
            this.label6.Text = "Un-Exported Time: 0:00";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(808, 595);
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
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnAddTimer);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainWindow";
            this.Text = "Gallifrey";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainWindow_FormClosed);
            this.Load += new System.EventHandler(this.MainWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.grpTarget.ResumeLayout(false);
            this.grpTarget.PerformLayout();
            this.grpExportStats.ResumeLayout(false);
            this.grpExportStats.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnAddTimer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblVersion;
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
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox grpExportStats;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
    }
}

