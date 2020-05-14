using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace KoARSaveItemEditor
{
    public partial class MainForm : Form
    {
        AmalurSaveEditor editor = null;
        List<EffectInfo> attributeList = null;
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
                string fileName = opfMain.FileName;
                editor = new AmalurSaveEditor();
                editor.ReadFile(fileName);
                tslblFileLocal.Text = fileName;
                inventorySizeText.Text = editor.GetMaxBagCount().ToString();
                btnSearchAll.PerformClick();
            }
        }

        private void ResetFilterFields()
        {
            txtFilterItemName.Clear();
            txtFilterCurrentDur.Clear();
            txtFilterMaxDur.Clear();

            lvMain.SelectedItems.Clear();
        }

        private void RefreshListOnFilterUpdate()
        {
            string itemName = txtFilterItemName.Text.Length != 0 ? txtFilterItemName.Text : string.Empty;
            float currDur = float.TryParse(txtFilterCurrentDur.Text, out currDur) ? currDur : default;
            float maxDur = float.TryParse(txtFilterMaxDur.Text, out maxDur) ? maxDur : default;

            IEnumerable<ItemMemoryInfo> query = itemList;
            if (itemName.Length != 0)
                query = query.Where(info => info.ItemName.IndexOf(itemName, StringComparison.OrdinalIgnoreCase) != -1);
            if (currDur > 0)
                query = query.Where(info => info.CurrentDurability == currDur);
            if (maxDur > 0)
                query = query.Where(info => info.MaxDurability == maxDur);

            lvMain.Items.Clear();
            foreach (var element in query)
            {
                lvMain.Items.Add(new ListViewItem
                {
                    Name = element.ItemIndex.ToString(),
                    Text = element.ItemIndex.ToString(),
                    Tag = element,
                    SubItems =
                    {
                        element.ItemName,
                        element.CurrentDurability.ToString(),
                        element.MaxDurability.ToString(),
                        element.EffectCount.ToString()
                    }
                });
            }

            lvMain.SelectedItems.Clear();
        }

        private void LoadItemAttributesOnClick()
        {
            ItemMemoryInfo itemInfo = (ItemMemoryInfo)lvMain.SelectedItems[0].Tag;
            RebindAttrList(itemInfo);
        }

        private void RebindAttrList(ItemMemoryInfo itemInfo)
        {
            List<EffectInfo> itemAttList = editor.GetEffectList(itemInfo, attributeList);
            selectedItem = itemInfo;

            txtPropName.Text = itemInfo.ItemName;
            txtPropCurrDur.Text = itemInfo.CurrentDurability.ToString();
            txtPropMaxDur.Text = itemInfo.MaxDurability.ToString();
            txtPropCurrDur.Enabled = true;
            txtPropMaxDur.Enabled = true;
            txtPropAttCount.Text = itemInfo.EffectCount.ToString();

            comboExistingAttList.DisplayMember = nameof(EffectInfo.DisplayText);
            comboExistingAttList.DataSource = itemAttList;

            comboAddAttList.DisplayMember = nameof(EffectInfo.DisplayText);
            comboAddAttList.ValueMember = nameof(EffectInfo.Code);
            comboAddAttList.DataSource = attributeList;
            btnPrint.Enabled = true;
        }

        private void DeleteItemAttribute()
        {
            ItemMemoryInfo itemInfo = (ItemMemoryInfo)lvMain.SelectedItems[0].Tag;
            List<EffectInfo> itemAttList = itemInfo.ItemAttList;
            EffectInfo selectedAttribute = (EffectInfo)comboExistingAttList.SelectedItem;

            foreach (EffectInfo att in itemAttList)
            {
                if (att.Code.Equals(selectedAttribute.Code, StringComparison.OrdinalIgnoreCase))
                {
                    itemAttList.Remove(att);
                    break;
                }
            }

            itemInfo.ItemAttList = itemAttList;
            txtPropAttCount.Text = itemAttList.Count.ToString();
            if (itemAttList.Count <= 0)
            {
                txtPropSelectedAttributeHexCode.Text = "";
            }
            editor.WriteWeaponByte(selectedItem);
            RebindAttrList(itemInfo);
            CanSave();
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
                List<ItemMemoryInfo> weaponTemp = editor.GetAllEquipment();

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
                    lvMain.Items.Add(new ListViewItem
                    {
                        Name = w.ItemIndex.ToString(),
                        Text = w.ItemIndex.ToString(),
                        Tag = w,
                        SubItems =
                        {
                            w.ItemName,
                            w.CurrentDurability.ToString(),
                            w.MaxDurability.ToString(),
                            w.EffectCount.ToString()
                        }
                    });
                }

                btnPrint.Enabled = false;
                btnDelete.Enabled = false;
                btnSave.Enabled = false;
                makeAllItemsSellable.Enabled = true;
                txtFilterItemName.Text = "";
                txtFilterMaxDur.Text = "";
                txtFilterCurrentDur.Text = "";
                txtPropCurrDur.Enabled = false;
                txtPropMaxDur.Enabled = false;
                lvMain.SelectedItems.Clear();
                selectedItem = null;
            }
        }

        private void LoadAmalurEditor(object sender, EventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            List<EffectInfo> attributeList = new List<EffectInfo>();
            try
            {
                doc.Load(Application.StartupPath + @"\Data\properties.xml");
                XmlNodeList nodes = doc.DocumentElement.ChildNodes;
                foreach (XmlNode n in nodes)
                {
                    EffectInfo att = new EffectInfo
                    {
                        Code = n.Attributes["id"].Value.ToUpper(),
                        DisplayText = n.InnerText.ToUpper()
                    };
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
            int? selected = selectedItem?.ItemIndex;
            btnSearchAll.PerformClick();

            if (selected != null)
            {
                var elem = lvMain.Items.Cast<ListViewItem>().FirstOrDefault(x => (x.Tag as ItemMemoryInfo).ItemIndex == selected);
                elem.Focused = true;
                elem.Selected = true;
                selectedItem = elem.Tag as ItemMemoryInfo;
            }
            tslblEditState.Text = "Modified";
            btnSave.Enabled = true;
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            btnPrint.Enabled = false;
            btnDelete.Enabled = false;
            ItemBytesForm form = new ItemBytesForm(editor, lvMain.SelectedItems[0].Tag as ItemMemoryInfo);
            if (form.ShowDialog() == DialogResult.Yes)
            {
                CanSave();
            }
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

        private void ButtonPropDeleteAttribute_Click(object sender, EventArgs e)
        {
            DeleteItemAttribute();
        }

        private void ComboAttList_SelectedIndexChanged(object sender, EventArgs e)
        {
            EffectInfo itemAttribute = (EffectInfo)comboExistingAttList.SelectedItem;
            txtPropSelectedAttributeHexCode.Text = itemAttribute.Code;
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
                EffectInfo matchingAttribute = comboAddAttList.Items.Cast<EffectInfo>().Where(x => x.Code == hexCode).FirstOrDefault();
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
                AddAttribute(selectedItem, hexCode);
                CanSave();
            }
        }

        private void AddAttribute(ItemMemoryInfo selectedItem, string attCode)
        {
            List<EffectInfo> attList = selectedItem.ItemAttList;
            EffectInfo attInfo = new EffectInfo
            {
                Code = attCode
            };
            attList.Add(attInfo);
            selectedItem.ItemAttList = attList;
            editor.WriteWeaponByte(selectedItem);
            RebindAttrList(selectedItem);
        }

        private void CheckBoxUnlockName_CheckedChanged(object sender, EventArgs e)
        {
            if (txtPropName.Text == "Unknown" && checkBoxUnlockName.Checked == true)
            {
                checkBoxUnlockName.Checked = false;
                MessageBox.Show("Editing not allowed.");
                return;
            }
            txtPropName.ReadOnly = !txtPropName.ReadOnly;
        }

        private void ComboAddAttList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox box = (ComboBox)sender;
            EffectInfo info = (EffectInfo)box.SelectedItem;
            if (!string.IsNullOrEmpty(info?.Code))
            {
                txtPropAddAttributeHexCode.Text = info.Code;
            }
        }

        private void buttonInvSizeLocate_Click(object sender, EventArgs e)
        {
            var value = (int)inventorySizeText.Value;
            editor.EditMaxBagCount(value);
            CanSave();
        }

        private void MakeAllItemsSellable_Click(object sender, EventArgs e)
        {
            int count = 0;
            foreach (ItemMemoryInfo item in lvMain.Items.Cast<ListViewItem>().Select(x => x.Tag as ItemMemoryInfo))
            {
                if (item.Unsellable)
                {
                    item.Unsellable = false;
                    editor.WriteWeaponByte(item);
                    count++;
                }
            }
            MessageBox.Show($"Modified {count} items.");
            if (count > 0)
            {
                CanSave();
            }
        }

        private void txtPropCurrDur_Leave(object sender, EventArgs e)
        {
            if (editor == null)
            {
                MessageBox.Show("No save file opened! Click OK to open a save file.");
                tsmiOpen.PerformClick();
            }
            if (selectedItem is null)
            {
                return;
            }

            if (float.TryParse(txtPropCurrDur.Text, out var newValue)
                && newValue > -1 && newValue < 100)
            {
                if (newValue != selectedItem.CurrentDurability)
                {
                    selectedItem.CurrentDurability = newValue;
                    editor.WriteWeaponByte(selectedItem);
                    CanSave();
                }
            }
            else
            {
                MessageBox.Show($"Invalid value '{txtPropCurrDur.Text}'. Durability must be a number such that, 0 ≤ durability < 100.");
                txtPropCurrDur.Text = selectedItem.CurrentDurability.ToString();
                txtPropCurrDur.Focus();
            }
        }

        private void txtPropMaxDur_Leave(object sender, EventArgs e)
        {
            if (editor == null)
            {
                MessageBox.Show("No save file opened! Click OK to open a save file.");
                tsmiOpen.PerformClick();
            }
            if (selectedItem is null)
            {
                return;
            }

            if (float.TryParse(txtPropMaxDur.Text, out var newValue)
                && newValue >= 0 && newValue < 100)
            {
                if (newValue != selectedItem.CurrentDurability)
                {
                    selectedItem.MaxDurability = newValue;
                    editor.WriteWeaponByte(selectedItem);
                    CanSave();
                }
            }
            else
            {
                MessageBox.Show($"Invalid value '{txtPropMaxDur.Text}'. Durability must be a number such that, 0 ≤ durability < 100.");
                txtPropMaxDur.Text = selectedItem.MaxDurability.ToString();
                txtPropMaxDur.Focus();
            }
        }
    }
}
