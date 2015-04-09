namespace Gallifrey.UI.Classic
{
    partial class SearchWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchWindow));
            this.txtSearchText = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbUserFilters = new System.Windows.Forms.ComboBox();
            this.lstResults = new System.Windows.Forms.ListBox();
            this.btnCancelAddTimer = new System.Windows.Forms.Button();
            this.btnAddTimer = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtSearchText
            // 
            this.txtSearchText.Location = new System.Drawing.Point(95, 24);
            this.txtSearchText.Name = "txtSearchText";
            this.txtSearchText.Size = new System.Drawing.Size(357, 25);
            this.txtSearchText.TabIndex = 1;
            this.toolTip.SetToolTip(this.txtSearchText, "Enter Search Term (Press Enter To Submit)");
            this.txtSearchText.TextChanged += new System.EventHandler(this.txtSearchText_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Search Term";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(104, 25);
            this.label2.TabIndex = 0;
            this.label2.Text = "Jira Search";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnRefresh);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.cmbUserFilters);
            this.groupBox1.Controls.Add(this.txtSearchText);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(17, 49);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(458, 163);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Search Details";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Image = global::Gallifrey.UI.Classic.Properties.Resources.Search_48x48;
            this.btnRefresh.Location = new System.Drawing.Point(206, 86);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(65, 65);
            this.btnRefresh.TabIndex = 4;
            this.btnRefresh.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip.SetToolTip(this.btnRefresh, "Search Jira");
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(85, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "Search Filters";
            // 
            // cmbUserFilters
            // 
            this.cmbUserFilters.FormattingEnabled = true;
            this.cmbUserFilters.Location = new System.Drawing.Point(95, 55);
            this.cmbUserFilters.Name = "cmbUserFilters";
            this.cmbUserFilters.Size = new System.Drawing.Size(357, 25);
            this.cmbUserFilters.TabIndex = 3;
            this.toolTip.SetToolTip(this.cmbUserFilters, "Choose One Of Your Saved Filters");
            this.cmbUserFilters.SelectedIndexChanged += new System.EventHandler(this.cmbUserFilters_SelectedIndexChanged);
            // 
            // lstResults
            // 
            this.lstResults.FormattingEnabled = true;
            this.lstResults.HorizontalScrollbar = true;
            this.lstResults.ItemHeight = 17;
            this.lstResults.Location = new System.Drawing.Point(12, 219);
            this.lstResults.Name = "lstResults";
            this.lstResults.Size = new System.Drawing.Size(463, 242);
            this.lstResults.TabIndex = 2;
            this.toolTip.SetToolTip(this.lstResults, "Jira Search Results");
            this.lstResults.DoubleClick += new System.EventHandler(this.lstResults_DoubleClick);
            // 
            // btnCancelAddTimer
            // 
            this.btnCancelAddTimer.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelAddTimer.Image = global::Gallifrey.UI.Classic.Properties.Resources.Cancel_48x48;
            this.btnCancelAddTimer.Location = new System.Drawing.Point(234, 470);
            this.btnCancelAddTimer.Name = "btnCancelAddTimer";
            this.btnCancelAddTimer.Size = new System.Drawing.Size(65, 65);
            this.btnCancelAddTimer.TabIndex = 4;
            this.btnCancelAddTimer.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip.SetToolTip(this.btnCancelAddTimer, "Close Search");
            this.btnCancelAddTimer.UseVisualStyleBackColor = true;
            this.btnCancelAddTimer.Click += new System.EventHandler(this.btnCancelAddTimer_Click);
            // 
            // btnAddTimer
            // 
            this.btnAddTimer.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnAddTimer.Image = global::Gallifrey.UI.Classic.Properties.Resources.Check_48x48;
            this.btnAddTimer.Location = new System.Drawing.Point(138, 470);
            this.btnAddTimer.Name = "btnAddTimer";
            this.btnAddTimer.Size = new System.Drawing.Size(65, 65);
            this.btnAddTimer.TabIndex = 3;
            this.btnAddTimer.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip.SetToolTip(this.btnAddTimer, "Add Timer For Selected Jira");
            this.btnAddTimer.UseVisualStyleBackColor = true;
            this.btnAddTimer.Click += new System.EventHandler(this.btnAddTimer_Click);
            // 
            // SearchWindow
            // 
            this.AcceptButton = this.btnRefresh;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancelAddTimer;
            this.ClientSize = new System.Drawing.Size(487, 547);
            this.ControlBox = false;
            this.Controls.Add(this.lstResults);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnCancelAddTimer);
            this.Controls.Add(this.btnAddTimer);
            this.Controls.Add(this.label2);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SearchWindow";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Gallifrey - Jira Search";
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.SearchWindow_KeyUp);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtSearchText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnAddTimer;
        private System.Windows.Forms.Button btnCancelAddTimer;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbUserFilters;
        private System.Windows.Forms.ListBox lstResults;
        private System.Windows.Forms.ToolTip toolTip;
    }
}