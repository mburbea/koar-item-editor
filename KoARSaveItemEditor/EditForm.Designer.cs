namespace KoARSaveItemEditor
{
    partial class EditForm
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
            this.btnAdd = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.btnDeleteAttribute = new System.Windows.Forms.Button();
            this.txtAttCode = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cboExtendAttIndex = new System.Windows.Forms.ComboBox();
            this.cboAddAttribute = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblAttCount = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txtCurrentDurability = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtMaxDurability = new System.Windows.Forms.TextBox();
            this.btnSaveAll = new System.Windows.Forms.Button();
            this.btnAddAttByInput = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.txtAttCodeInput = new System.Windows.Forms.TextBox();
            this.txtName = new System.Windows.Forms.TextBox();
            this.numAddByInput = new System.Windows.Forms.NumericUpDown();
            this.numAddBySelect = new System.Windows.Forms.NumericUpDown();
            this.numDelete = new System.Windows.Forms.NumericUpDown();
            this.chkEditName = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.lblCodeCount = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numAddByInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAddBySelect)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDelete)).BeginInit();
            this.SuspendLayout();
            // 
            // btnAdd
            // 
            this.btnAdd.Enabled = false;
            this.btnAdd.Location = new System.Drawing.Point(489, 93);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(96, 25);
            this.btnAdd.TabIndex = 18;
            this.btnAdd.Text = "Add Property";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 100);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "Property:";
            // 
            // btnDeleteAttribute
            // 
            this.btnDeleteAttribute.Location = new System.Drawing.Point(489, 62);
            this.btnDeleteAttribute.Name = "btnDeleteAttribute";
            this.btnDeleteAttribute.Size = new System.Drawing.Size(96, 25);
            this.btnDeleteAttribute.TabIndex = 16;
            this.btnDeleteAttribute.Text = "Delete Property";
            this.btnDeleteAttribute.UseVisualStyleBackColor = true;
            this.btnDeleteAttribute.Click += new System.EventHandler(this.btnDeleteAttribute_Click);
            // 
            // txtAttCode
            // 
            this.txtAttCode.Location = new System.Drawing.Point(77, 65);
            this.txtAttCode.Name = "txtAttCode";
            this.txtAttCode.ReadOnly = true;
            this.txtAttCode.Size = new System.Drawing.Size(234, 20);
            this.txtAttCode.TabIndex = 15;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "Code:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Attribute:";
            // 
            // cboExtendAttIndex
            // 
            this.cboExtendAttIndex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboExtendAttIndex.FormattingEnabled = true;
            this.cboExtendAttIndex.Location = new System.Drawing.Point(76, 36);
            this.cboExtendAttIndex.Name = "cboExtendAttIndex";
            this.cboExtendAttIndex.Size = new System.Drawing.Size(509, 21);
            this.cboExtendAttIndex.TabIndex = 12;
            this.cboExtendAttIndex.SelectedIndexChanged += new System.EventHandler(this.cboExtendAttIndex_SelectedIndexChanged);
            // 
            // cboAddAttribute
            // 
            this.cboAddAttribute.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAddAttribute.FormattingEnabled = true;
            this.cboAddAttribute.Location = new System.Drawing.Point(76, 97);
            this.cboAddAttribute.Name = "cboAddAttribute";
            this.cboAddAttribute.Size = new System.Drawing.Size(333, 21);
            this.cboAddAttribute.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 13);
            this.label5.TabIndex = 20;
            this.label5.Text = "Name:";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(497, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 26;
            this.label1.Text = "Attributes:";
            // 
            // lblAttCount
            // 
            this.lblAttCount.AutoSize = true;
            this.lblAttCount.Location = new System.Drawing.Point(557, 10);
            this.lblAttCount.Name = "lblAttCount";
            this.lblAttCount.Size = new System.Drawing.Size(34, 13);
            this.lblAttCount.TabIndex = 27;
            this.lblAttCount.Text = "count";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(332, 10);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 13);
            this.label8.TabIndex = 28;
            this.label8.Text = "Durability:";
            this.label8.Click += new System.EventHandler(this.label8_Click);
            // 
            // txtCurrentDurability
            // 
            this.txtCurrentDurability.Location = new System.Drawing.Point(391, 5);
            this.txtCurrentDurability.Name = "txtCurrentDurability";
            this.txtCurrentDurability.Size = new System.Drawing.Size(38, 20);
            this.txtCurrentDurability.TabIndex = 29;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(435, 10);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(12, 13);
            this.label9.TabIndex = 30;
            this.label9.Text = "/";
            // 
            // txtMaxDurability
            // 
            this.txtMaxDurability.Location = new System.Drawing.Point(453, 5);
            this.txtMaxDurability.Name = "txtMaxDurability";
            this.txtMaxDurability.Size = new System.Drawing.Size(38, 20);
            this.txtMaxDurability.TabIndex = 31;
            // 
            // btnSaveAll
            // 
            this.btnSaveAll.Location = new System.Drawing.Point(489, 155);
            this.btnSaveAll.Name = "btnSaveAll";
            this.btnSaveAll.Size = new System.Drawing.Size(96, 25);
            this.btnSaveAll.TabIndex = 32;
            this.btnSaveAll.Text = "Ok";
            this.btnSaveAll.UseVisualStyleBackColor = true;
            this.btnSaveAll.Click += new System.EventHandler(this.btnSaveAll_Click);
            // 
            // btnAddAttByInput
            // 
            this.btnAddAttByInput.Location = new System.Drawing.Point(489, 124);
            this.btnAddAttByInput.Name = "btnAddAttByInput";
            this.btnAddAttByInput.Size = new System.Drawing.Size(96, 25);
            this.btnAddAttByInput.TabIndex = 33;
            this.btnAddAttByInput.Text = "Add Property";
            this.btnAddAttByInput.UseVisualStyleBackColor = true;
            this.btnAddAttByInput.Click += new System.EventHandler(this.btnAddAttByInput_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(12, 131);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(63, 13);
            this.label10.TabIndex = 34;
            this.label10.Text = "Hex Code:";
            // 
            // txtAttCodeInput
            // 
            this.txtAttCodeInput.Location = new System.Drawing.Point(77, 128);
            this.txtAttCodeInput.Name = "txtAttCodeInput";
            this.txtAttCodeInput.Size = new System.Drawing.Size(333, 20);
            this.txtAttCodeInput.TabIndex = 35;
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(77, 5);
            this.txtName.Name = "txtName";
            this.txtName.ReadOnly = true;
            this.txtName.Size = new System.Drawing.Size(150, 20);
            this.txtName.TabIndex = 36;
            // 
            // numAddByInput
            // 
            this.numAddByInput.Location = new System.Drawing.Point(414, 128);
            this.numAddByInput.Name = "numAddByInput";
            this.numAddByInput.Size = new System.Drawing.Size(69, 20);
            this.numAddByInput.TabIndex = 37;
            // 
            // numAddBySelect
            // 
            this.numAddBySelect.Location = new System.Drawing.Point(415, 97);
            this.numAddBySelect.Name = "numAddBySelect";
            this.numAddBySelect.Size = new System.Drawing.Size(68, 20);
            this.numAddBySelect.TabIndex = 38;
            // 
            // numDelete
            // 
            this.numDelete.Location = new System.Drawing.Point(415, 65);
            this.numDelete.Name = "numDelete";
            this.numDelete.Size = new System.Drawing.Size(68, 20);
            this.numDelete.TabIndex = 39;
            // 
            // chkEditName
            // 
            this.chkEditName.AutoSize = true;
            this.chkEditName.Location = new System.Drawing.Point(233, 9);
            this.chkEditName.Name = "chkEditName";
            this.chkEditName.Size = new System.Drawing.Size(93, 17);
            this.chkEditName.TabIndex = 40;
            this.chkEditName.Text = "Change Name";
            this.chkEditName.UseVisualStyleBackColor = true;
            this.chkEditName.CheckedChanged += new System.EventHandler(this.chkEditName_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(317, 68);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(73, 13);
            this.label6.TabIndex = 41;
            this.label6.Text = "# of Instances:";
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // lblCodeCount
            // 
            this.lblCodeCount.AutoSize = true;
            this.lblCodeCount.Location = new System.Drawing.Point(396, 68);
            this.lblCodeCount.Name = "lblCodeCount";
            this.lblCodeCount.Size = new System.Drawing.Size(13, 13);
            this.lblCodeCount.TabIndex = 42;
            this.lblCodeCount.Text = "0";
            // 
            // EditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(597, 190);
            this.Controls.Add(this.lblCodeCount);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.chkEditName);
            this.Controls.Add(this.numDelete);
            this.Controls.Add(this.numAddBySelect);
            this.Controls.Add(this.numAddByInput);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.txtAttCodeInput);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.btnAddAttByInput);
            this.Controls.Add(this.btnSaveAll);
            this.Controls.Add(this.txtMaxDurability);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtCurrentDurability);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.lblAttCount);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnDeleteAttribute);
            this.Controls.Add(this.txtAttCode);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cboExtendAttIndex);
            this.Controls.Add(this.cboAddAttribute);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Equipment Modification (Modify unknown equipment at your own risk!)";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.numAddByInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAddBySelect)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDelete)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnDeleteAttribute;
        private System.Windows.Forms.TextBox txtAttCode;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboExtendAttIndex;
        private System.Windows.Forms.ComboBox cboAddAttribute;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblAttCount;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtCurrentDurability;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtMaxDurability;
        private System.Windows.Forms.Button btnSaveAll;
        private System.Windows.Forms.Button btnAddAttByInput;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtAttCodeInput;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.NumericUpDown numAddByInput;
        private System.Windows.Forms.NumericUpDown numAddBySelect;
        private System.Windows.Forms.NumericUpDown numDelete;
        private System.Windows.Forms.CheckBox chkEditName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblCodeCount;
    }
}