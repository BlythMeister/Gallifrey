namespace Gallifrey.UI.Classic
{
    partial class RenameTimerWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RenameTimerWindow));
            this.txtJiraRef = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.calStartDate = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtJiraRef
            // 
            this.txtJiraRef.Location = new System.Drawing.Point(123, 47);
            this.txtJiraRef.Name = "txtJiraRef";
            this.txtJiraRef.Size = new System.Drawing.Size(157, 25);
            this.txtJiraRef.TabIndex = 2;
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
            this.calStartDate.Enabled = false;
            this.calStartDate.Location = new System.Drawing.Point(123, 78);
            this.calStartDate.Name = "calStartDate";
            this.calStartDate.Size = new System.Drawing.Size(157, 25);
            this.calStartDate.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(136, 25);
            this.label2.TabIndex = 0;
            this.label2.Text = "Rename Timer";
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
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::Gallifrey.UI.Classic.Properties.Resources.Cancel_48x48;
            this.btnCancel.Location = new System.Drawing.Point(168, 118);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(65, 65);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Image = global::Gallifrey.UI.Classic.Properties.Resources.Check_48x48;
            this.btnOK.Location = new System.Drawing.Point(55, 118);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(65, 65);
            this.btnOK.TabIndex = 5;
            this.btnOK.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // RenameTimerWindow
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(296, 198);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.calStartDate);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtJiraRef);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximumSize = new System.Drawing.Size(312, 236);
            this.MinimumSize = new System.Drawing.Size(312, 236);
            this.Name = "RenameTimerWindow";
            this.Text = "Gallifrey - Rename Timer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtJiraRef;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker calStartDate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}