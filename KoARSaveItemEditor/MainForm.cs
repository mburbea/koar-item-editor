using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using KoAR.Core;

namespace KoARSaveItemEditor
{
    public partial class MainForm : Form
    {
        private List<EffectInfo> attributeList;
        private AmalurSaveEditor editor;
        private List<ItemMemoryInfo> itemList;
        private ItemMemoryInfo selectedItem;

        public MainForm()
        {
            InitializeComponent();
        }

        private void AddAttribute(ItemMemoryInfo selectedItem, string attCode)
        {
            List<EffectInfo> effects = selectedItem.ReadEffects();
            effects.Add(new EffectInfo
            {
                Code = attCode
            });
            selectedItem.WriteEffects(effects);
            editor.WriteEquipmentBytes(selectedItem);
            RebindAttrList(selectedItem);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Removing equipment forcefully may lead to bugs. Removing equipped items will lead to an invalid save. It is recommended not to use this feature.\n\nAre you sure you want to delete this item?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                editor.DeleteEquipment((ItemMemoryInfo)lvMain.SelectedItems[0].Tag);
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            btnPrint.Enabled = false;
            btnDelete.Enabled = false;
            using ItemBytesForm form = new ItemBytesForm(editor, lvMain.SelectedItems[0].Tag as ItemMemoryInfo);
            if (form.ShowDialog() == DialogResult.Yes)
            {
                CanSave();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (editor == null)
            {
                return;
            }
            File.Copy(opfMain.FileName, opfMain.FileName + ".bak", true);
            editor.SaveFile(opfMain.FileName);
            tslblEditState.Text = "Unmodified";
            btnSave.Enabled = false;
            MessageBox.Show("Save successful! Original save backed up as " + opfMain.FileName + ".bak.");
        }

        private void BtnShowAll_Click(object sender, EventArgs e)
        {
            ResetFilterFields();
            lvMain.Items.Clear();
            if (editor == null)
            {
                MessageBox.Show("No save file opened! Click OK to open a save file.");
                tsmiOpen.PerformClick();
                return;
            }

            itemList = new List<ItemMemoryInfo>();

            foreach (ItemMemoryInfo info in editor.GetAllEquipment())
            {
                if (info.ItemName == "Unknown")
                {
                    itemList.Add(info);
                }
                else
                {
                    itemList.Insert(0, info);
                }
            }

            foreach (ItemMemoryInfo info in itemList)
            {
                lvMain.Items.Add(new ListViewItem
                {
                    Name = info.ItemIndex.ToString(),
                    Text = info.ItemIndex.ToString(),
                    Tag = info,
                    SubItems =
                    {
                        info.ItemName,
                        info.CurrentDurability.ToString(),
                        info.MaxDurability.ToString(),
                        info.EffectCount.ToString()
                    }
                });
            }

            makeAllItemsSellable.Enabled = true;
            txtFilterItemName.Text = txtFilterMaxDur.Text = txtFilterCurrentDur.Text = string.Empty;
            btnPrint.Enabled = btnDelete.Enabled = btnSave.Enabled = txtPropCurrDur.Enabled = txtPropMaxDur.Enabled = false;
            lvMain.SelectedItems.Clear();
            selectedItem = null;
        }

        private void buttonInvSizeLocate_Click(object sender, EventArgs e)
        {
            var value = (int)inventorySizeText.Value;
            editor.EditMaxBagCount(value);
            CanSave();
        }

        private void ButtonPropAddAttribute_Click(object sender, EventArgs e)
        {
            string hexCode = txtPropAddAttributeHexCode.Text;
            if (hexCode.Length != 6)
            {
                MessageBox.Show("Invalid hex code. Attribute will not be added.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (uint.TryParse(hexCode, NumberStyles.HexNumber, null, out _))
            {
                AddAttribute(selectedItem, hexCode);
                CanSave();
            }
        }

        private void ButtonPropDeleteAttribute_Click(object sender, EventArgs e)
        {
            DeleteItemAttribute();
        }

        private void CanSave()
        {
            int? selected = selectedItem?.ItemIndex;
            btnSearchAll.PerformClick();

            if (selected != null)
            {
                ListViewItem elem = lvMain.Items.Cast<ListViewItem>().FirstOrDefault(x => (x.Tag as ItemMemoryInfo).ItemIndex == selected);
                elem.Focused = true;
                elem.Selected = true;
                selectedItem = elem.Tag as ItemMemoryInfo;
            }
            tslblEditState.Text = "Modified";
            btnSave.Enabled = true;
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

        private void ComboAttList_SelectedIndexChanged(object sender, EventArgs e)
        {
            EffectInfo itemAttribute = (EffectInfo)comboExistingAttList.SelectedItem;
            txtPropSelectedAttributeHexCode.Text = itemAttribute.Code;
        }

        private void DeleteItemAttribute()
        {
            ItemMemoryInfo itemInfo = (ItemMemoryInfo)lvMain.SelectedItems[0].Tag;
            List<EffectInfo> effects = itemInfo.ReadEffects();
            EffectInfo selectedAttribute = (EffectInfo)comboExistingAttList.SelectedItem;

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i].Code.Equals(selectedAttribute.Code, StringComparison.OrdinalIgnoreCase))
                {
                    effects.RemoveAt(i);
                    break;
                }
            }

            itemInfo.WriteEffects(effects);
            txtPropAttCount.Text = effects.Count.ToString();
            if (effects.Count == 0)
            {
                txtPropSelectedAttributeHexCode.Text = string.Empty;
            }
            editor.WriteEquipmentBytes(selectedItem);
            RebindAttrList(itemInfo);
            CanSave();
        }

        private void DurabilityTextBox_TextChanged(object sender, EventArgs e)
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

        private void LoadAmalurEditor(object sender, EventArgs e)
        {
            try
            {
                this.attributeList = XDocument.Load(Application.StartupPath + @"\Data\properties.xml").Root.Elements().Select(element => new EffectInfo
                {
                    Code = element.Attribute("id").Value.ToUpper(),
                    DisplayText = element.Value.ToUpper()
                }).ToList();
            }
            catch
            {
                MessageBox.Show("Failed to load property list. Please check if \"Data\" folder is in the editor's directory and properties.xml is inside.");
                Application.Exit();
            }
        }

        private void LoadItemAttributesOnClick()
        {
            ItemMemoryInfo itemInfo = (ItemMemoryInfo)lvMain.SelectedItems[0].Tag;
            RebindAttrList(itemInfo);
        }

        private void LoadSaveFile(object sender, EventArgs e)
        {
            if (opfMain.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            lvMain.Items.Clear();
            string fileName = opfMain.FileName;
            editor = new AmalurSaveEditor();
            editor.ReadFile(fileName);
            tslblFileLocal.Text = fileName;
            inventorySizeText.Text = editor.GetMaxBagCount().ToString();
            btnSearchAll.PerformClick();
        }

        private void lvMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvMain.SelectedItems.Count > 0)
            {
                LoadItemAttributesOnClick();
            }
        }

        private void MakeAllItemsSellable_Click(object sender, EventArgs e)
        {
            int count = 0;
            foreach (ItemMemoryInfo item in lvMain.Items.Cast<ListViewItem>().Select(x => x.Tag as ItemMemoryInfo))
            {
                if (!item.IsUnsellable)
                {
                    continue;
                }
                item.IsUnsellable = false;
                editor.WriteEquipmentBytes(item);
                count++;
            }
            MessageBox.Show($"Modified {count} items.");
            if (count > 0)
            {
                CanSave();
            }
        }

        private void OnDurabilityTextLeave(object sender, Func<ItemMemoryInfo, float> getDurability, Action<ItemMemoryInfo, float> setDurability)
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

            TextBox textBox = (TextBox)sender;
            float currentValue = getDurability(selectedItem);
            if (!float.TryParse(textBox.Text, out var newValue) || !editor.IsValidDurability(newValue))
            {
                MessageBox.Show($"Invalid value '{textBox.Text}'. Durability must be a number such that, 0 < durability < 100.");
                textBox.Text = currentValue.ToString();
                textBox.Focus();
                textBox.SelectAll();
            }
            else if (newValue != currentValue)
            {
                setDurability(selectedItem, newValue);
                editor.WriteEquipmentBytes(selectedItem);
                CanSave();
            }
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
            txtPropName.ReadOnly = !itemInfo.HasCustomName;

            comboExistingAttList.DisplayMember = nameof(EffectInfo.DisplayText);
            comboExistingAttList.DataSource = itemAttList;

            comboAddAttList.DisplayMember = nameof(EffectInfo.DisplayText);
            comboAddAttList.ValueMember = nameof(EffectInfo.Code);
            comboAddAttList.DataSource = attributeList;
            btnPrint.Enabled = true;
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

        private void ResetFilterFields()
        {
            txtFilterItemName.Clear();
            txtFilterCurrentDur.Clear();
            txtFilterMaxDur.Clear();

            lvMain.SelectedItems.Clear();
        }

        private void TsmiHelp_Click(object sender, EventArgs e)
        {
            using HelpForm form = new HelpForm();
            form.ShowDialog();
        }

        private void TxtPropAddAttributeHexCode_TextChanged(object sender, EventArgs e)
        {
            string hexCode = txtPropAddAttributeHexCode.Text;
            if (hexCode.Length != 6)
            {
                return;
            }

            int index = 0;
            try
            {
                EffectInfo matchingAttribute = comboAddAttList.Items.Cast<EffectInfo>().FirstOrDefault(x => x.Code == hexCode);
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

        private void TextPropCurrDur_Leave(object sender, EventArgs e)
        {
            OnDurabilityTextLeave(sender, info => info.CurrentDurability, (info, value) => info.CurrentDurability = value);
        }

        private void TextPropMaxDur_Leave(object sender, EventArgs e)
        {
            OnDurabilityTextLeave(sender, info => info.MaxDurability, (info, value) => info.MaxDurability = value);
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

        private void TxtPropName_Leave(object sender, EventArgs e)
        {
            if(selectedItem != null && selectedItem.HasCustomName && selectedItem.ItemName != txtPropName.Text)
            {
                selectedItem.ItemName = txtPropName.Text;
                editor.WriteEquipmentBytes(selectedItem);
                CanSave();
            }
        }
    }
}