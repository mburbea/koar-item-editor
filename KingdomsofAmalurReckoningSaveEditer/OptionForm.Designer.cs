namespace KingdomsofAmalurReckoningSaveEditer
{
    partial class OptionForm
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
            this.DIalogConfirm = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // DIalogConfirm
            // 
            this.DIalogConfirm.AutoSize = true;
            this.DIalogConfirm.Checked = true;
            this.DIalogConfirm.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DIalogConfirm.Location = new System.Drawing.Point(12, 12);
            this.DIalogConfirm.Name = "DIalogConfirm";
            this.DIalogConfirm.Size = new System.Drawing.Size(94, 17);
            this.DIalogConfirm.TabIndex = 1;
            this.DIalogConfirm.Text = "Confirm Dialog";
            this.DIalogConfirm.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(197, 227);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.DIalogConfirm);
            this.Name = "Form1";
            this.Text = "Options";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox DIalogConfirm;
        private System.Windows.Forms.Button button1;

    }
}