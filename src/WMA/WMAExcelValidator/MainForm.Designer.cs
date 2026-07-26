namespace WMAExcelValidator
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button btnValidate;
        private System.Windows.Forms.TextBox textFileName;
        private System.Windows.Forms.TextBox textNotes;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnValidate = new System.Windows.Forms.Button();
            this.textFileName = new System.Windows.Forms.TextBox();
            this.textNotes = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnOpen
            // 
            this.btnOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOpen.Location = new System.Drawing.Point(12, 12);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(90, 28);
            this.btnOpen.TabIndex = 0;
            this.btnOpen.Text = "Open...";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.BtnOpen_Click);
            // 
            // btnValidate
            // 
            this.btnValidate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.btnValidate.Enabled = false;
            this.btnValidate.Location = new System.Drawing.Point(110, 12);
            this.btnValidate.Name = "btnValidate";
            this.btnValidate.Size = new System.Drawing.Size(90, 28);
            this.btnValidate.TabIndex = 1;
            this.btnValidate.Text = "Validate";
            this.btnValidate.UseVisualStyleBackColor = true;
            this.btnValidate.Click += new System.EventHandler(this.BtnValidate_Click);
            // 
            // textFileName
            // 
            this.textFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textFileName.Location = new System.Drawing.Point(208, 14);
            this.textFileName.Name = "textFileName";
            this.textFileName.ReadOnly = true;
            this.textFileName.Size = new System.Drawing.Size(664, 22);
            this.textFileName.TabIndex = 2;
            // 
            // textNotes
            // 
            this.textNotes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textNotes.Location = new System.Drawing.Point(12, 52);
            this.textNotes.Multiline = true;
            this.textNotes.Name = "textNotes";
            this.textNotes.ReadOnly = true;
            this.textNotes.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textNotes.Size = new System.Drawing.Size(860, 497);
            this.textNotes.TabIndex = 3;
            this.textNotes.Text = "Open or drop an Excel file here.";
            this.textNotes.WordWrap = false;
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 561);
            this.Controls.Add(this.textNotes);
            this.Controls.Add(this.textFileName);
            this.Controls.Add(this.btnValidate);
            this.Controls.Add(this.btnOpen);
            this.MinimumSize = new System.Drawing.Size(640, 420);
            this.Name = "MainForm";
            this.Text = "WMA Excel Validator";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
