using System.Windows.Forms;

namespace KoARSaveItemEditor
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">Should release the managed resources. true；Otherwise false。</param>
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
        /// Devices designed to support - not
        /// Modify the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.tsmiFile = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.makeAllItemsSellable = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.opfMain = new System.Windows.Forms.OpenFileDialog();
            this.txtFilterItemName = new System.Windows.Forms.TextBox();
            this.itemViewGroupBox = new System.Windows.Forms.GroupBox();
            this.lvMain = new System.Windows.Forms.ListView();
            this.id = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.curDur = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.maxDur = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.attCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnSearchAll = new System.Windows.Forms.Button();
            this.btnPrint = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tslblFileLocal = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tslblEditState = new System.Windows.Forms.ToolStripStatusLabel();
            this.txtFilterCurrentDur = new System.Windows.Forms.TextBox();
            this.txtFilterMaxDur = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.filterGroupBox = new System.Windows.Forms.GroupBox();
            this.durabilityGroupBox = new System.Windows.Forms.GroupBox();
            this.nameGroupBox = new System.Windows.Forms.GroupBox();
            this.propertiesGroupBox = new System.Windows.Forms.GroupBox();
            this.groupBoxPropAttributes = new System.Windows.Forms.GroupBox();
            this.groupBoxAddAttribute = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtPropAddAttributeHexCode = new System.Windows.Forms.TextBox();
            this.comboAddAttList = new System.Windows.Forms.ComboBox();
            this.buttonPropAddAttribute = new System.Windows.Forms.Button();
            this.groupBoxPropExistingAttributes = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtPropSelectedAttributeHexCode = new System.Windows.Forms.TextBox();
            this.comboExistingAttList = new System.Windows.Forms.ComboBox();
            this.buttonPropDeleteAttribute = new System.Windows.Forms.Button();
            this.txtPropAttCount = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBoxPropDurability = new System.Windows.Forms.GroupBox();
            this.txtPropCurrDur = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtPropMaxDur = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBoxPropName = new System.Windows.Forms.GroupBox();
            this.checkBoxUnlockName = new System.Windows.Forms.CheckBox();
            this.txtPropName = new System.Windows.Forms.TextBox();
            this.invSizeGroupBox = new System.Windows.Forms.GroupBox();
            this.inventorySizeText = new System.Windows.Forms.NumericUpDown();
            this.buttonInvSizeLocate = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.itemViewGroupBox.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.filterGroupBox.SuspendLayout();
            this.durabilityGroupBox.SuspendLayout();
            this.nameGroupBox.SuspendLayout();
            this.propertiesGroupBox.SuspendLayout();
            this.groupBoxPropAttributes.SuspendLayout();
            this.groupBoxAddAttribute.SuspendLayout();
            this.groupBoxPropExistingAttributes.SuspendLayout();
            this.groupBoxPropDurability.SuspendLayout();
            this.groupBoxPropName.SuspendLayout();
            this.invSizeGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inventorySizeText)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiFile,
            this.makeAllItemsSellable,
            this.tsmiHelp});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(815, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // tsmiFile
            // 
            this.tsmiFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiOpen});
            this.tsmiFile.Name = "tsmiFile";
            this.tsmiFile.Size = new System.Drawing.Size(37, 20);
            this.tsmiFile.Text = "File";
            // 
            // tsmiOpen
            // 
            this.tsmiOpen.Name = "tsmiOpen";
            this.tsmiOpen.Size = new System.Drawing.Size(103, 22);
            this.tsmiOpen.Text = "Open";
            this.tsmiOpen.Click += new System.EventHandler(this.LoadSaveFile);
            // 
            // makeAllItemsSellable
            // 
            this.makeAllItemsSellable.Enabled = false;
            this.makeAllItemsSellable.Name = "makeAllItemsSellable";
            this.makeAllItemsSellable.Size = new System.Drawing.Size(140, 20);
            this.makeAllItemsSellable.Text = "Make All Items Sellable";
            this.makeAllItemsSellable.Click += new System.EventHandler(this.MakeAllItemsSellable_Click);
            // 
            // tsmiHelp
            // 
            this.tsmiHelp.Name = "tsmiHelp";
            this.tsmiHelp.Size = new System.Drawing.Size(44, 20);
            this.tsmiHelp.Text = "Help";
            this.tsmiHelp.Click += new System.EventHandler(this.TsmiHelp_Click);
            // 
            // opfMain
            // 
            this.opfMain.FileName = "openFileDialog1";
            this.opfMain.Filter = "Save File Archive|*.sav";
            // 
            // txtFilterItemName
            // 
            this.txtFilterItemName.Location = new System.Drawing.Point(6, 21);
            this.txtFilterItemName.Name = "txtFilterItemName";
            this.txtFilterItemName.Size = new System.Drawing.Size(156, 22);
            this.txtFilterItemName.TabIndex = 2;
            this.txtFilterItemName.TextChanged += new System.EventHandler(this.TxtSearch_TextChanged);
            // 
            // itemViewGroupBox
            // 
            this.itemViewGroupBox.Controls.Add(this.lvMain);
            this.itemViewGroupBox.Location = new System.Drawing.Point(12, 111);
            this.itemViewGroupBox.Name = "itemViewGroupBox";
            this.itemViewGroupBox.Size = new System.Drawing.Size(560, 409);
            this.itemViewGroupBox.TabIndex = 5;
            this.itemViewGroupBox.TabStop = false;
            this.itemViewGroupBox.Text = "Items";
            // 
            // lvMain
            // 
            this.lvMain.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.lvMain.BackgroundImageTiled = true;
            this.lvMain.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.id,
            this.name,
            this.curDur,
            this.maxDur,
            this.attCount});
            this.lvMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.lvMain.FullRowSelect = true;
            this.lvMain.GridLines = true;
            this.lvMain.HideSelection = false;
            this.lvMain.Location = new System.Drawing.Point(3, 18);
            this.lvMain.MultiSelect = false;
            this.lvMain.Name = "lvMain";
            this.lvMain.Size = new System.Drawing.Size(554, 385);
            this.lvMain.TabIndex = 3;
            this.lvMain.UseCompatibleStateImageBehavior = false;
            this.lvMain.View = System.Windows.Forms.View.Details;
            this.lvMain.SelectedIndexChanged += new System.EventHandler(this.lvMain_SelectedIndexChanged);
            this.lvMain.DoubleClick += new System.EventHandler(this.BtnEdit_Click);
            // 
            // id
            // 
            this.id.Text = "ID";
            this.id.Width = 100;
            // 
            // name
            // 
            this.name.Text = "Item Name";
            this.name.Width = 150;
            // 
            // curDur
            // 
            this.curDur.Text = "Current Dur.";
            this.curDur.Width = 100;
            // 
            // maxDur
            // 
            this.maxDur.Text = "Max Dur.";
            this.maxDur.Width = 100;
            // 
            // attCount
            // 
            this.attCount.Text = "# Properties";
            this.attCount.Width = 100;
            // 
            // btnSearchAll
            // 
            this.btnSearchAll.Location = new System.Drawing.Point(491, 21);
            this.btnSearchAll.Name = "btnSearchAll";
            this.btnSearchAll.Size = new System.Drawing.Size(63, 49);
            this.btnSearchAll.TabIndex = 14;
            this.btnSearchAll.Text = "Reset Filter";
            this.btnSearchAll.UseVisualStyleBackColor = true;
            this.btnSearchAll.Click += new System.EventHandler(this.BtnShowAll_Click);
            // 
            // btnPrint
            // 
            this.btnPrint.Enabled = false;
            this.btnPrint.Location = new System.Drawing.Point(7, 379);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(68, 22);
            this.btnPrint.TabIndex = 17;
            this.btnPrint.Text = "Hex Edit";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.BtnPrint_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Enabled = false;
            this.btnDelete.Location = new System.Drawing.Point(81, 379);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(61, 22);
            this.btnDelete.TabIndex = 16;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(148, 379);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 22);
            this.btnSave.TabIndex = 11;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.tslblFileLocal,
            this.toolStripStatusLabel3,
            this.tslblEditState});
            this.statusStrip1.Location = new System.Drawing.Point(0, 525);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(815, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(61, 17);
            this.toolStripStatusLabel1.Text = "Save path:";
            // 
            // tslblFileLocal
            // 
            this.tslblFileLocal.AutoSize = false;
            this.tslblFileLocal.Name = "tslblFileLocal";
            this.tslblFileLocal.Size = new System.Drawing.Size(500, 17);
            this.tslblFileLocal.Text = "Open";
            this.tslblFileLocal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.tslblFileLocal.Click += new System.EventHandler(this.TslblFileLocal_Click);
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(62, 17);
            this.toolStripStatusLabel3.Text = "Save state:";
            // 
            // tslblEditState
            // 
            this.tslblEditState.Name = "tslblEditState";
            this.tslblEditState.Size = new System.Drawing.Size(70, 17);
            this.tslblEditState.Text = "Unmodified";
            this.tslblEditState.Click += new System.EventHandler(this.TslblEditState_Click);
            // 
            // txtFilterCurrentDur
            // 
            this.txtFilterCurrentDur.Location = new System.Drawing.Point(58, 21);
            this.txtFilterCurrentDur.Name = "txtFilterCurrentDur";
            this.txtFilterCurrentDur.Size = new System.Drawing.Size(87, 22);
            this.txtFilterCurrentDur.TabIndex = 9;
            this.txtFilterCurrentDur.TextChanged += new System.EventHandler(this.TxtCurrentDur_TextChanged);
            // 
            // txtFilterMaxDur
            // 
            this.txtFilterMaxDur.Location = new System.Drawing.Point(213, 21);
            this.txtFilterMaxDur.Name = "txtFilterMaxDur";
            this.txtFilterMaxDur.Size = new System.Drawing.Size(86, 22);
            this.txtFilterMaxDur.TabIndex = 10;
            this.txtFilterMaxDur.TextChanged += new System.EventHandler(this.TxtMaxDur_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 24);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(46, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Current";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(151, 24);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Maximum";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label6.Click += new System.EventHandler(this.Label6_Click);
            // 
            // filterGroupBox
            // 
            this.filterGroupBox.Controls.Add(this.durabilityGroupBox);
            this.filterGroupBox.Controls.Add(this.btnSearchAll);
            this.filterGroupBox.Controls.Add(this.nameGroupBox);
            this.filterGroupBox.Location = new System.Drawing.Point(12, 27);
            this.filterGroupBox.Name = "filterGroupBox";
            this.filterGroupBox.Size = new System.Drawing.Size(560, 78);
            this.filterGroupBox.TabIndex = 14;
            this.filterGroupBox.TabStop = false;
            this.filterGroupBox.Text = "Filters";
            this.filterGroupBox.Enter += new System.EventHandler(this.GroupBox2_Enter);
            // 
            // durabilityGroupBox
            // 
            this.durabilityGroupBox.Controls.Add(this.txtFilterCurrentDur);
            this.durabilityGroupBox.Controls.Add(this.label5);
            this.durabilityGroupBox.Controls.Add(this.label6);
            this.durabilityGroupBox.Controls.Add(this.txtFilterMaxDur);
            this.durabilityGroupBox.Location = new System.Drawing.Point(180, 21);
            this.durabilityGroupBox.Name = "durabilityGroupBox";
            this.durabilityGroupBox.Size = new System.Drawing.Size(305, 49);
            this.durabilityGroupBox.TabIndex = 4;
            this.durabilityGroupBox.TabStop = false;
            this.durabilityGroupBox.Text = "Durability";
            // 
            // nameGroupBox
            // 
            this.nameGroupBox.Controls.Add(this.txtFilterItemName);
            this.nameGroupBox.Location = new System.Drawing.Point(6, 21);
            this.nameGroupBox.Name = "nameGroupBox";
            this.nameGroupBox.Size = new System.Drawing.Size(168, 49);
            this.nameGroupBox.TabIndex = 3;
            this.nameGroupBox.TabStop = false;
            this.nameGroupBox.Text = "Name";
            // 
            // propertiesGroupBox
            // 
            this.propertiesGroupBox.Controls.Add(this.groupBoxPropAttributes);
            this.propertiesGroupBox.Controls.Add(this.groupBoxPropDurability);
            this.propertiesGroupBox.Controls.Add(this.btnDelete);
            this.propertiesGroupBox.Controls.Add(this.btnSave);
            this.propertiesGroupBox.Controls.Add(this.btnPrint);
            this.propertiesGroupBox.Controls.Add(this.groupBoxPropName);
            this.propertiesGroupBox.Location = new System.Drawing.Point(578, 111);
            this.propertiesGroupBox.Name = "propertiesGroupBox";
            this.propertiesGroupBox.Size = new System.Drawing.Size(229, 409);
            this.propertiesGroupBox.TabIndex = 19;
            this.propertiesGroupBox.TabStop = false;
            this.propertiesGroupBox.Text = "Properties";
            // 
            // groupBoxPropAttributes
            // 
            this.groupBoxPropAttributes.Controls.Add(this.groupBoxAddAttribute);
            this.groupBoxPropAttributes.Controls.Add(this.groupBoxPropExistingAttributes);
            this.groupBoxPropAttributes.Controls.Add(this.txtPropAttCount);
            this.groupBoxPropAttributes.Controls.Add(this.label3);
            this.groupBoxPropAttributes.Location = new System.Drawing.Point(7, 173);
            this.groupBoxPropAttributes.Name = "groupBoxPropAttributes";
            this.groupBoxPropAttributes.Size = new System.Drawing.Size(216, 200);
            this.groupBoxPropAttributes.TabIndex = 6;
            this.groupBoxPropAttributes.TabStop = false;
            this.groupBoxPropAttributes.Text = "Attributes";
            // 
            // groupBoxAddAttribute
            // 
            this.groupBoxAddAttribute.Controls.Add(this.label7);
            this.groupBoxAddAttribute.Controls.Add(this.txtPropAddAttributeHexCode);
            this.groupBoxAddAttribute.Controls.Add(this.comboAddAttList);
            this.groupBoxAddAttribute.Controls.Add(this.buttonPropAddAttribute);
            this.groupBoxAddAttribute.Location = new System.Drawing.Point(6, 121);
            this.groupBoxAddAttribute.Name = "groupBoxAddAttribute";
            this.groupBoxAddAttribute.Size = new System.Drawing.Size(204, 74);
            this.groupBoxAddAttribute.TabIndex = 24;
            this.groupBoxAddAttribute.TabStop = false;
            this.groupBoxAddAttribute.Text = "Add Attribute/s";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 53);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(56, 13);
            this.label7.TabIndex = 23;
            this.label7.Text = "Hex Code";
            // 
            // txtPropAddAttributeHexCode
            // 
            this.txtPropAddAttributeHexCode.Location = new System.Drawing.Point(68, 48);
            this.txtPropAddAttributeHexCode.Name = "txtPropAddAttributeHexCode";
            this.txtPropAddAttributeHexCode.Size = new System.Drawing.Size(49, 22);
            this.txtPropAddAttributeHexCode.TabIndex = 22;
            this.txtPropAddAttributeHexCode.TextChanged += new System.EventHandler(this.TxtPropAddAttributeHexCode_TextChanged);
            // 
            // comboAddAttList
            // 
            this.comboAddAttList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboAddAttList.DropDownWidth = 500;
            this.comboAddAttList.FormattingEnabled = true;
            this.comboAddAttList.Location = new System.Drawing.Point(6, 21);
            this.comboAddAttList.Name = "comboAddAttList";
            this.comboAddAttList.Size = new System.Drawing.Size(192, 21);
            this.comboAddAttList.TabIndex = 20;
            this.comboAddAttList.SelectedIndexChanged += new System.EventHandler(this.ComboAddAttList_SelectedIndexChanged);
            // 
            // buttonPropAddAttribute
            // 
            this.buttonPropAddAttribute.Location = new System.Drawing.Point(123, 48);
            this.buttonPropAddAttribute.Name = "buttonPropAddAttribute";
            this.buttonPropAddAttribute.Size = new System.Drawing.Size(75, 22);
            this.buttonPropAddAttribute.TabIndex = 21;
            this.buttonPropAddAttribute.Text = "Add";
            this.buttonPropAddAttribute.UseVisualStyleBackColor = true;
            this.buttonPropAddAttribute.Click += new System.EventHandler(this.ButtonPropAddAttribute_Click);
            // 
            // groupBoxPropExistingAttributes
            // 
            this.groupBoxPropExistingAttributes.Controls.Add(this.label4);
            this.groupBoxPropExistingAttributes.Controls.Add(this.txtPropSelectedAttributeHexCode);
            this.groupBoxPropExistingAttributes.Controls.Add(this.comboExistingAttList);
            this.groupBoxPropExistingAttributes.Controls.Add(this.buttonPropDeleteAttribute);
            this.groupBoxPropExistingAttributes.Location = new System.Drawing.Point(6, 41);
            this.groupBoxPropExistingAttributes.Name = "groupBoxPropExistingAttributes";
            this.groupBoxPropExistingAttributes.Size = new System.Drawing.Size(204, 74);
            this.groupBoxPropExistingAttributes.TabIndex = 22;
            this.groupBoxPropExistingAttributes.TabStop = false;
            this.groupBoxPropExistingAttributes.Text = "Existing Attributes";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 23;
            this.label4.Text = "Hex Code";
            // 
            // txtPropSelectedAttributeHexCode
            // 
            this.txtPropSelectedAttributeHexCode.Location = new System.Drawing.Point(68, 48);
            this.txtPropSelectedAttributeHexCode.Name = "txtPropSelectedAttributeHexCode";
            this.txtPropSelectedAttributeHexCode.ReadOnly = true;
            this.txtPropSelectedAttributeHexCode.Size = new System.Drawing.Size(49, 22);
            this.txtPropSelectedAttributeHexCode.TabIndex = 22;
            // 
            // comboExistingAttList
            // 
            this.comboExistingAttList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboExistingAttList.DropDownWidth = 500;
            this.comboExistingAttList.FormattingEnabled = true;
            this.comboExistingAttList.Location = new System.Drawing.Point(6, 21);
            this.comboExistingAttList.Name = "comboExistingAttList";
            this.comboExistingAttList.Size = new System.Drawing.Size(192, 21);
            this.comboExistingAttList.TabIndex = 20;
            this.comboExistingAttList.SelectedIndexChanged += new System.EventHandler(this.ComboAttList_SelectedIndexChanged);
            // 
            // buttonPropDeleteAttribute
            // 
            this.buttonPropDeleteAttribute.Location = new System.Drawing.Point(123, 48);
            this.buttonPropDeleteAttribute.Name = "buttonPropDeleteAttribute";
            this.buttonPropDeleteAttribute.Size = new System.Drawing.Size(75, 22);
            this.buttonPropDeleteAttribute.TabIndex = 21;
            this.buttonPropDeleteAttribute.Text = "Delete";
            this.buttonPropDeleteAttribute.UseVisualStyleBackColor = true;
            this.buttonPropDeleteAttribute.Click += new System.EventHandler(this.ButtonPropDeleteAttribute_Click);
            // 
            // txtPropAttCount
            // 
            this.txtPropAttCount.Cursor = System.Windows.Forms.Cursors.No;
            this.txtPropAttCount.Enabled = false;
            this.txtPropAttCount.Location = new System.Drawing.Point(105, 13);
            this.txtPropAttCount.Name = "txtPropAttCount";
            this.txtPropAttCount.Size = new System.Drawing.Size(105, 22);
            this.txtPropAttCount.TabIndex = 14;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 18);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "No. of Attributes";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBoxPropDurability
            // 
            this.groupBoxPropDurability.Controls.Add(this.txtPropCurrDur);
            this.groupBoxPropDurability.Controls.Add(this.label1);
            this.groupBoxPropDurability.Controls.Add(this.txtPropMaxDur);
            this.groupBoxPropDurability.Controls.Add(this.label2);
            this.groupBoxPropDurability.Location = new System.Drawing.Point(7, 95);
            this.groupBoxPropDurability.Name = "groupBoxPropDurability";
            this.groupBoxPropDurability.Size = new System.Drawing.Size(216, 72);
            this.groupBoxPropDurability.TabIndex = 5;
            this.groupBoxPropDurability.TabStop = false;
            this.groupBoxPropDurability.Text = "Durability";
            // 
            // txtPropCurrDur
            // 
            this.txtPropCurrDur.Location = new System.Drawing.Point(73, 15);
            this.txtPropCurrDur.Name = "txtPropCurrDur";
            this.txtPropCurrDur.Size = new System.Drawing.Size(137, 22);
            this.txtPropCurrDur.TabIndex = 13;
            this.txtPropCurrDur.Leave += new System.EventHandler(this.txtPropCurrDur_Leave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Current";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtPropMaxDur
            // 
            this.txtPropMaxDur.Location = new System.Drawing.Point(74, 43);
            this.txtPropMaxDur.Name = "txtPropMaxDur";
            this.txtPropMaxDur.Size = new System.Drawing.Size(136, 22);
            this.txtPropMaxDur.TabIndex = 14;
            this.txtPropMaxDur.Leave += new System.EventHandler(this.txtPropMaxDur_Leave);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "Maximum";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBoxPropName
            // 
            this.groupBoxPropName.Controls.Add(this.checkBoxUnlockName);
            this.groupBoxPropName.Controls.Add(this.txtPropName);
            this.groupBoxPropName.Location = new System.Drawing.Point(7, 21);
            this.groupBoxPropName.Name = "groupBoxPropName";
            this.groupBoxPropName.Size = new System.Drawing.Size(216, 68);
            this.groupBoxPropName.TabIndex = 4;
            this.groupBoxPropName.TabStop = false;
            this.groupBoxPropName.Text = "Name";
            // 
            // checkBoxUnlockName
            // 
            this.checkBoxUnlockName.AutoSize = true;
            this.checkBoxUnlockName.Location = new System.Drawing.Point(6, 49);
            this.checkBoxUnlockName.Name = "checkBoxUnlockName";
            this.checkBoxUnlockName.Size = new System.Drawing.Size(178, 17);
            this.checkBoxUnlockName.TabIndex = 3;
            this.checkBoxUnlockName.Text = "Unlock (only if not Unknown)";
            this.checkBoxUnlockName.UseVisualStyleBackColor = true;
            this.checkBoxUnlockName.CheckedChanged += new System.EventHandler(this.CheckBoxUnlockName_CheckedChanged);
            // 
            // txtPropName
            // 
            this.txtPropName.Location = new System.Drawing.Point(6, 21);
            this.txtPropName.Name = "txtPropName";
            this.txtPropName.ReadOnly = true;
            this.txtPropName.Size = new System.Drawing.Size(204, 22);
            this.txtPropName.TabIndex = 2;
            // 
            // invSizeGroupBox
            // 
            this.invSizeGroupBox.Controls.Add(this.inventorySizeText);
            this.invSizeGroupBox.Controls.Add(this.buttonInvSizeLocate);
            this.invSizeGroupBox.Controls.Add(this.label8);
            this.invSizeGroupBox.Location = new System.Drawing.Point(578, 27);
            this.invSizeGroupBox.Name = "invSizeGroupBox";
            this.invSizeGroupBox.Size = new System.Drawing.Size(229, 78);
            this.invSizeGroupBox.TabIndex = 20;
            this.invSizeGroupBox.TabStop = false;
            this.invSizeGroupBox.Text = "Inventory Size";
            // 
            // inventorySizeText
            // 
            this.inventorySizeText.Location = new System.Drawing.Point(89, 21);
            this.inventorySizeText.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.inventorySizeText.Name = "inventorySizeText";
            this.inventorySizeText.Size = new System.Drawing.Size(76, 22);
            this.inventorySizeText.TabIndex = 25;
            // 
            // buttonInvSizeLocate
            // 
            this.buttonInvSizeLocate.Location = new System.Drawing.Point(171, 21);
            this.buttonInvSizeLocate.Name = "buttonInvSizeLocate";
            this.buttonInvSizeLocate.Size = new System.Drawing.Size(52, 22);
            this.buttonInvSizeLocate.TabIndex = 24;
            this.buttonInvSizeLocate.Text = "Save";
            this.buttonInvSizeLocate.UseVisualStyleBackColor = true;
            this.buttonInvSizeLocate.Click += new System.EventHandler(this.buttonInvSizeLocate_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 26);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(79, 13);
            this.label8.TabIndex = 16;
            this.label8.Text = "Enter Desired:";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(815, 547);
            this.Controls.Add(this.invSizeGroupBox);
            this.Controls.Add(this.propertiesGroupBox);
            this.Controls.Add(this.filterGroupBox);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.itemViewGroupBox);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Kingdoms of Amalur: Reckoning Save Item Editor";
            this.Load += new System.EventHandler(this.LoadAmalurEditor);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.itemViewGroupBox.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.filterGroupBox.ResumeLayout(false);
            this.durabilityGroupBox.ResumeLayout(false);
            this.durabilityGroupBox.PerformLayout();
            this.nameGroupBox.ResumeLayout(false);
            this.nameGroupBox.PerformLayout();
            this.propertiesGroupBox.ResumeLayout(false);
            this.groupBoxPropAttributes.ResumeLayout(false);
            this.groupBoxPropAttributes.PerformLayout();
            this.groupBoxAddAttribute.ResumeLayout(false);
            this.groupBoxAddAttribute.PerformLayout();
            this.groupBoxPropExistingAttributes.ResumeLayout(false);
            this.groupBoxPropExistingAttributes.PerformLayout();
            this.groupBoxPropDurability.ResumeLayout(false);
            this.groupBoxPropDurability.PerformLayout();
            this.groupBoxPropName.ResumeLayout(false);
            this.groupBoxPropName.PerformLayout();
            this.invSizeGroupBox.ResumeLayout(false);
            this.invSizeGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inventorySizeText)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiFile;
        private System.Windows.Forms.ToolStripMenuItem tsmiOpen;
        private System.Windows.Forms.OpenFileDialog opfMain;
        private System.Windows.Forms.TextBox txtFilterItemName;
        private System.Windows.Forms.GroupBox itemViewGroupBox;
        private System.Windows.Forms.ListView lvMain;
        private System.Windows.Forms.ColumnHeader id;
        private System.Windows.Forms.ColumnHeader name;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.ToolStripMenuItem tsmiHelp;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel tslblFileLocal;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.ToolStripStatusLabel tslblEditState;
        private System.Windows.Forms.TextBox txtFilterCurrentDur;
        private System.Windows.Forms.TextBox txtFilterMaxDur;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnSearchAll;
        private System.Windows.Forms.ColumnHeader curDur;
        private System.Windows.Forms.ColumnHeader maxDur;
        private System.Windows.Forms.ColumnHeader attCount;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.GroupBox filterGroupBox;
        private System.Windows.Forms.GroupBox nameGroupBox;
        private System.Windows.Forms.GroupBox durabilityGroupBox;
        private GroupBox propertiesGroupBox;
        private GroupBox groupBoxPropName;
        private TextBox txtPropName;
        private GroupBox groupBoxPropDurability;
        private TextBox txtPropCurrDur;
        private Label label1;
        private TextBox txtPropMaxDur;
        private Label label2;
        private GroupBox groupBoxPropAttributes;
        private Label label3;
        private TextBox txtPropAttCount;
        private ComboBox comboExistingAttList;
        private GroupBox invSizeGroupBox;
        private Button buttonPropDeleteAttribute;
        private GroupBox groupBoxPropExistingAttributes;
        private TextBox txtPropSelectedAttributeHexCode;
        private Label label4;
        private GroupBox groupBoxAddAttribute;
        private Label label7;
        private TextBox txtPropAddAttributeHexCode;
        private ComboBox comboAddAttList;
        private Button buttonPropAddAttribute;
        private CheckBox checkBoxUnlockName;
        private Button buttonInvSizeLocate;
        private Label label8;
        private NumericUpDown inventorySizeText;
        private ToolStripMenuItem makeAllItemsSellable;
    }
}

