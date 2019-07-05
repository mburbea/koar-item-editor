namespace KoARSaveItemEditor
{
    partial class BagEditForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtCurrentBag = new System.Windows.Forms.TextBox();
            this.txtBag = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Current Inventory Limit:";
            // 
            // txtCurrentBag
            // 
            this.txtCurrentBag.Location = new System.Drawing.Point(132, 12);
            this.txtCurrentBag.Name = "txtCurrentBag";
            this.txtCurrentBag.ReadOnly = true;
            this.txtCurrentBag.Size = new System.Drawing.Size(283, 20);
            this.txtCurrentBag.TabIndex = 1;
            this.txtCurrentBag.TabStop = false;
            this.txtCurrentBag.TextChanged += new System.EventHandler(this.TxtCurrentBag_TextChanged);
            // 
            // txtBag
            // 
            this.txtBag.Location = new System.Drawing.Point(132, 38);
            this.txtBag.Name = "txtBag";
            this.txtBag.Size = new System.Drawing.Size(283, 20);
            this.txtBag.TabIndex = 2;
            this.txtBag.Text = "Do not exceed 8 digits.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(103, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "New Inventory Limit:";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(421, 12);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(46, 46);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // BagEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(481, 70);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtBag);
            this.Controls.Add(this.txtCurrentBag);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BagEditForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Inventory Space Modification";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BagEditForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtCurrentBag;
        private System.Windows.Forms.TextBox txtBag;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSave;
    }
}