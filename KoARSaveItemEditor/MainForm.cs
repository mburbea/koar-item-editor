using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Linq;
using System.Drawing;

namespace KoARSaveItemEditor
{
    public partial class MainForm : Form
    {
        AmalurSaveEditor editor = null;
        List<AttributeInfo> attributeList = null;
        List<ItemMemoryInfo> itemList = null;
        ItemMemoryInfo selectedItem = null;

        public MainForm()
        {
            InitializeComponent();
        }

        private void LoadSaveFile(object sender, EventArgs e)
        {
            if (opfMain.ShowDialog() == DialogResult.OK)
            {
                lvMain.Items.Clear();
                String fileName = opfMain.FileName;
                editor = new AmalurSaveEditor();
                editor.ReadFile(fileName);
                tslblFileLocal.Text = fileName;
                //inventorySizeTextBox.Text = editor.GetMaxBagCount().ToString();
                btnSearchAll.PerformClick();
            }
        }

        private void ResetFilterFields()
        {
            this.txtFilterItemName.Clear();
            this.txtFilterCurrentDur.Clear();
            this.txtFilterMaxDur.Clear();

            this.lvMain.SelectedItems.Clear();
        }

        private void RefreshListOnFilterUpdate()
        {
            String itemName = txtFilterItemName.Text != "" ? txtFilterItemName.Text.ToUpper() : "";
            float currDur = Single.TryParse(txtFilterCurrentDur.Text, out currDur) ? currDur : 0;
            float maxDur = Single.TryParse(txtFilterMaxDur.Text, out maxDur) ? maxDur : 0;

            var query = from w in itemList select w;
            if (itemName != "")
                query = query.Where(w => w.ItemName.ToUpper().Contains(itemName));
            if (currDur > 0)
                query = query.Where(w => w.CurrentDurability == currDur);
            if (maxDur > 0)
                query = query.Where(w => w.MaxDurability == maxDur);

            lvMain.Items.Clear();
            foreach (var element in query)
            {
                ListViewItem item = new ListViewItem();
                item.Name = element.ItemIndex.ToString();
                item.Text = element.ItemIndex.ToString();
                item.SubItems.Add(element.ItemName);
                item.SubItems.Add(element.CurrentDurability.ToString());
                item.SubItems.Add(element.MaxDurability.ToString());
                item.SubItems.Add(element.AttCount.ToString());
                item.Tag = element;
                lvMain.Items.Add(item);
            }

            lvMain.SelectedItems.Clear();
        }

        private void LoadItemAttributesOnClick()
        {
            ItemMemoryInfo itemInfo = (ItemMemoryInfo)lvMain.SelectedItems[0].Tag;
            List<AttributeMemoryInfo> itemAttList = editor.GetAttList(itemInfo, this.attributeList);
            this.selectedItem = itemInfo;

            this.txtPropName.Text = itemInfo.ItemName;
            this.txtPropCurrDur.Text = itemInfo.CurrentDurability.ToString();
            this.txtPropMaxDur.Text = itemInfo.MaxDurability.ToString();
            this.txtPropAttCount.Text = itemInfo.AttCount.ToString();

            this.comboExistingAttList.DisplayMember = "Detail";
            this.comboExistingAttList.DataSource = itemAttList;

            this.comboAddAttList.DisplayMember = "AttributeText";
            this.comboAddAttList.ValueMember = "AttributeId";
            this.comboAddAttList.DataSource = this.attributeList;
        }

        private void DeleteItemAttribute()
        {
            ItemMemoryInfo itemInfo = (ItemMemoryInfo)lvMain.SelectedItems[0].Tag;
            List<AttributeMemoryInfo> itemAttList = itemInfo.ItemAttList;
            AttributeMemoryInfo selectedAttribute = (AttributeMemoryInfo) comboExistingAttList.SelectedItem;

            foreach (AttributeMemoryInfo att in itemAttList)
            {
                if (att.Code.ToUpper() == selectedAttribute.Code.ToUpper())
                {
                    itemAttList.Remove(att);
                    break;
                }
            }

            itemInfo.ItemAttList = itemAttList;
            this.txtPropAttCount.Text = itemAttList.Count.ToString();
            if (itemAttList.Count <= 0)
            {
                this.txtPropSelectedAttributeHexCode.Text = "";
            }
            LoadItemAttributesOnClick();
        }

        private void BtnShowAll_Click(object sender, EventArgs e)
        {
            ResetFilterFields();
            lvMain.Items.Clear();
            if (editor == null)
            {
                MessageBox.Show("No save file opened! Click OK to open a save file.");
                tsmiOpen.PerformClick();
            }
            else
            {
                List<ItemMemoryInfo> weaponTemp = editor.GetAllWeapon();

                itemList = new List<ItemMemoryInfo>();
                foreach (ItemMemoryInfo w in weaponTemp)
                {
                    if (w.ItemName == "Unknown")
                    {
                        itemList.Add(w);
                    }
                    else
                    {
                        itemList.Insert(0, w);
                    }
                }
                foreach (ItemMemoryInfo w in itemList)
                {
                    ListViewItem item = new ListViewItem();
                    item.Name = w.ItemIndex.ToString();
                    item.Text = w.ItemIndex.ToString();
                    item.SubItems.Add(w.ItemName);
                    item.SubItems.Add(w.CurrentDurability.ToString());
                    item.SubItems.Add(w.MaxDurability.ToString());
                    item.SubItems.Add(w.AttCount.ToString());
                    item.Tag = w;
                    lvMain.Items.Add(item);
                }
                btnPrint.Enabled = false;
                btnDelete.Enabled = false;
                btnSave.Enabled = false;
                txtFilterItemName.Text = "";
                txtFilterMaxDur.Text = "";
                txtFilterCurrentDur.Text = "";
                lvMain.SelectedItems.Clear();
                this.selectedItem = null;
            }
        }

        private void LoadAmalurEditor(object sender, EventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            List<AttributeInfo> attributeList = new List<AttributeInfo>();
            try
            {
                doc.Load(Application.StartupPath + @"\Data\properties.xml");
                XmlNodeList nodes = doc.DocumentElement.ChildNodes;
                foreach (XmlNode n in nodes)
                {
                    AttributeInfo att = new AttributeInfo();
                    att.AttributeId = n.Attributes["id"].Value.ToUpper();
                    att.AttributeText = n.InnerText.ToUpper();
                    attributeList.Add(att);
                }
                this.attributeList = attributeList;
            }
            catch
            {
                MessageBox.Show("Failed to load property list. Please check if \"Data\" folder is in the editor's directory and properties.xml is inside.");
                Application.Exit();
            }
        }

        private void LoadItemAttributes(ItemMemoryInfo itemInfo)
        {
            List<AttributeMemoryInfo> attList = editor.GetAttList(itemInfo, this.attributeList);
            List<AttributeMemoryInfo> temp = new List<AttributeMemoryInfo>();

            foreach (AttributeMemoryInfo att in attList)
            {
                bool isAtt = false;
                foreach (AttributeMemoryInfo t in temp)
                {
                    if (t.Code == att.Code)
                    {
                        isAtt = true;
                        break;
                    }
                }
                if (!isAtt)
                {
                    temp.Add(att);
                }
            }
            comboExistingAttList.DataSource = null;
            comboExistingAttList.DataSource = temp;
        }

        private void lvMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvMain.SelectedItems.Count > 0)
            {
                LoadItemAttributesOnClick();
            }            
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (editor != null)
            {
                File.Copy(opfMain.FileName, opfMain.FileName + ".bak", true);
                editor.SaveFile(opfMain.FileName);
                tslblEditState.Text = "Unmodified";
                btnSave.Enabled = false;
                MessageBox.Show("Save successful! Original save backed up as " + opfMain.FileName + ".bak.");
            }
        }

        private void TsmiHelp_Click(object sender, EventArgs e)
        {
            HelpForm form = new HelpForm();
            form.ShowDialog();
        }

        private void TsmiBag_Click(object sender, EventArgs e)
        {
            BagEditForm form = new BagEditForm(editor);
            if (form.ShowDialog() == DialogResult.Yes)
            {
                btnSave.Enabled = true;
            }
        } 

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            ItemMemoryInfo weaponInfo = (ItemMemoryInfo)lvMain.SelectedItems[0].Tag;

            //EditForm form = new EditForm(editor, attributeList, itemInfo);
            //btnPrint.Enabled = false;
            //btnEdit.Enabled = false;
            //btnDelete.Enabled = false;
            //if (form.ShowDialog() == DialogResult.Yes)
            //{
            //    CanSave();
            //}

        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Removing equipment forcefully may lead to bugs. Removing equipped items will lead to an invalid save. It is recommended not to use this feature.\n\nAre you sure you want to delete this item?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                ItemMemoryInfo weaponInfo = (ItemMemoryInfo)lvMain.SelectedItems[0].Tag;
                editor.DeleteWeapon(weaponInfo);
            }
        }

        private void CanSave()
        {
            btnSearchAll.PerformClick();
            tslblEditState.Text = "Modified";
            btnSave.Enabled = true;
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            btnPrint.Enabled = false;
            btnDelete.Enabled = false;
            ItemBytesForm form = new ItemBytesForm(editor,lvMain.SelectedItems[0].Tag as ItemMemoryInfo);
            if (form.ShowDialog() == DialogResult.Yes)
            {
                CanSave();
            }
        }

        private void BtnBag_Click(object sender, EventArgs e)
        {
            BagEditForm form = new BagEditForm(editor);
            if (form.ShowDialog() == DialogResult.Yes)
            {
                btnSave.Enabled = true;
            }
        }

        private void TslblFileLocal_Click(object sender, EventArgs e)
        {

        }

        private void TslblEditState_Click(object sender, EventArgs e)
        {

        }

        private void Label2_Click(object sender, EventArgs e)
        {

        }

        private void Label6_Click(object sender, EventArgs e)
        {

        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (editor == null)
            {
                MessageBox.Show("No save file opened! Click OK to open a save file.");
                tsmiOpen.PerformClick();
            }
            else
            {
                RefreshListOnFilterUpdate();
            }
        }

        private void GroupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void TxtCurrentDur_TextChanged(object sender, EventArgs e)
        {
            if (editor == null)
            {
                MessageBox.Show("No save file opened! Click OK to open a save file.");
                tsmiOpen.PerformClick();
            }
            else
            {
                RefreshListOnFilterUpdate();
            }
        }

        private void TxtMaxDur_TextChanged(object sender, EventArgs e)
        {
            if (editor == null)
            {
                MessageBox.Show("No save file opened! Click OK to open a save file.");
                tsmiOpen.PerformClick();
            }
            else
            {
                RefreshListOnFilterUpdate();
            }
        }

        private void Label1_Click(object sender, EventArgs e)
        {

        }

        private void Label2_Click_1(object sender, EventArgs e)
        {

        }

        private void ButtonPropDeleteAttribute_Click(object sender, EventArgs e)
        {
            DeleteItemAttribute();
        }

        private void ComboAttList_SelectedIndexChanged(object sender, EventArgs e)
        {
            AttributeMemoryInfo itemAttribute = (AttributeMemoryInfo)comboExistingAttList.SelectedItem;
            this.txtPropSelectedAttributeHexCode.Text = itemAttribute.Code;
        }

        private void TxtPropAddAttributeHexCode_TextChanged(object sender, EventArgs e)
        {
            string hexCode = txtPropAddAttributeHexCode.Text;
            int index = 0;
            if (hexCode == "" || hexCode.Length != 6)
            {
                return;
            }

            try
            {
                AttributeInfo matchingAttribute = comboAddAttList.Items.Cast<AttributeInfo>().Where(x => x.AttributeId == hexCode).FirstOrDefault();
                index = comboAddAttList.Items.IndexOf(matchingAttribute);
            }
            catch
            {
                index = 0;
            }
            finally
            {
                comboAddAttList.SelectedIndex = index;
            }           
        }

        private void ButtonPropAddAttribute_Click(object sender, EventArgs e)
        {
            string hexCode = txtPropAddAttributeHexCode.Text;
            if (hexCode == "" || hexCode.Length != 6)
            {
                MessageBox.Show("Invalid hex code. Attribute will not be added.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            long validCode;
            bool isValidCode = long.TryParse(hexCode, System.Globalization.NumberStyles.HexNumber, null, out validCode);
            if (isValidCode)
            {
                AddAttribute(this.selectedItem, hexCode);
            }
        }

        private void AddAttribute(ItemMemoryInfo selectedItem, string attCode)
        {
            List<AttributeMemoryInfo> attList = selectedItem.ItemAttList;
            AttributeMemoryInfo attInfo = new AttributeMemoryInfo();
            attInfo.Code = attCode;
            attList.Add(attInfo);
            selectedItem.ItemAttList = attList;
            LoadItemAttributesOnClick();
        }

        private void CheckBoxUnlockName_CheckedChanged(object sender, EventArgs e)
        {
            if (this.txtPropName.Text == "Unknown" && this.checkBoxUnlockName.Checked == true)
            {
                this.checkBoxUnlockName.Checked = false;
                MessageBox.Show("Editing not allowed.");
                return;
            }
            txtPropName.ReadOnly = !txtPropName.ReadOnly;
        }
    }
}
