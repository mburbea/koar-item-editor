using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace KoARSaveItemEditor
{
    public partial class MainForm : Form
    {
        AmalurSaveEditor editor = null;
        List<AttributeInfo> attributeList = null;
        List<WeaponMemoryInfo> weaponList = null;
        string searchType = "";

        public MainForm()
        {
            InitializeComponent();
            lvMain.Columns[0].Width = 100;
            lvMain.Columns[1].Width = 100;
            lvMain.Columns[2].Width = -2;
            lvMain.Columns[3].Width = -2;
            lvMain.Columns[4].Width = -2;
        }

        private void TsmiOpen_Click(object sender, EventArgs e)
        {
            if (opfMain.ShowDialog() == DialogResult.OK)
            {
                lvMain.Items.Clear();
                String fileName = opfMain.FileName;
                editor = new AmalurSaveEditor();
                editor.ReadFile(fileName);
                tslblFileLocal.Text = fileName;
                btnSearchAll.PerformClick();
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            searchType = "name";
            if (txtSearch.Text == "")
            {
                return;
            }
            lvMain.Items.Clear();

            foreach (WeaponMemoryInfo w in weaponList)
            {
                if (w.WeaponName.ToUpper().Contains(txtSearch.Text.ToUpper()))
                {
                    ListViewItem item = new ListViewItem();
                    item.Name = w.WeaponIndex.ToString();
                    item.Text = w.WeaponIndex.ToString();
                    item.SubItems.Add(w.WeaponName);
                    item.SubItems.Add(w.CurrentDurability.ToString());
                    item.SubItems.Add(w.MaxDurability.ToString());
                    item.SubItems.Add(w.AttCount.ToString());
                    item.Tag = w;
                    lvMain.Items.Add(item);
                }
            }
            lvMain.SelectedItems.Clear();
        }

        private void BtnSearchByDur_Click(object sender, EventArgs e)
        {
            if (txtCurrendDur.Text.Trim() == "" && txtMaxDur.Text.Trim() == "")
            {
                return;
            }
            float curDur = 0;
            float maxDur = 0;

            if (txtCurrendDur.Text.Trim() != "")
            {
                try
                {
                    curDur = float.Parse(txtCurrendDur.Text);
                }
                catch
                {
                    MessageBox.Show("No durability entered!");
                    return;
                }
            }
            if (txtMaxDur.Text.Trim() != "")
            {
                try
                {
                    maxDur = float.Parse(txtMaxDur.Text);
                }
                catch
                {
                    MessageBox.Show("No durability entered!");
                    return;
                }
            }

            searchType = "dur";
            lvMain.Items.Clear();

            foreach (WeaponMemoryInfo w in weaponList)
            {
                if (txtCurrendDur.Text.Trim() != "" && txtMaxDur.Text.Trim() == "")
                {
                    if (curDur != w.CurrentDurability)
                    {
                        continue;
                    }
                }
                else if (txtCurrendDur.Text.Trim() == "" && txtMaxDur.Text.Trim() != "")
                {
                    if (maxDur != w.MaxDurability)
                    {
                        continue;
                    }
                }
                else
                {
                    if (maxDur != w.MaxDurability || curDur!=w.CurrentDurability)
                    {
                        continue;
                    }
                }
                ListViewItem item = new ListViewItem();
                item.Name = w.WeaponIndex.ToString();
                item.Text = w.WeaponIndex.ToString();
                item.SubItems.Add(w.WeaponName);
                item.SubItems.Add(w.CurrentDurability.ToString());
                item.SubItems.Add(w.MaxDurability.ToString());
                item.SubItems.Add(w.AttCount.ToString());
                item.Tag = w;
                lvMain.Items.Add(item);
            }
            lvMain.SelectedItems.Clear();
        }

        private void BtnShowAll_Click(object sender, EventArgs e)
        {
            lvMain.Items.Clear();
            if (editor == null)
            {
                MessageBox.Show("No save file opened! Click OK to open a save file.");
                tsmiOpen.PerformClick();
            }
            else
            {
                List<WeaponMemoryInfo> weaponTemp = editor.GetAllWeapon();

                weaponList = new List<WeaponMemoryInfo>();
                foreach (WeaponMemoryInfo w in weaponTemp)
                {
                    if (w.WeaponName == "Unknown")
                    {
                        weaponList.Add(w);
                    }
                    else
                    {
                        weaponList.Insert(0, w);
                    }
                }
                foreach (WeaponMemoryInfo w in weaponList)
                {
                    ListViewItem item = new ListViewItem();
                    item.Name = w.WeaponIndex.ToString();
                    item.Text = w.WeaponIndex.ToString();
                    item.SubItems.Add(w.WeaponName);
                    item.SubItems.Add(w.CurrentDurability.ToString());
                    item.SubItems.Add(w.MaxDurability.ToString());
                    item.SubItems.Add(w.AttCount.ToString());
                    item.Tag = w;
                    lvMain.Items.Add(item);
                }
                btnSearchByDur.Enabled = true;
                btnSearchByName.Enabled = true;
                tsmiBag.Visible = true;
                btnPrint.Enabled = false;
                btnEdit.Enabled = false;
                btnDelete.Enabled = false;
                btnSave.Enabled = false;
                lvMain.SelectedItems.Clear();
            }
        }

        private void AmalurEditer_Load(object sender, EventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            attributeList = new List<AttributeInfo>();
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
                btnEdit.Enabled = true;
                btnDelete.Enabled = true;
                btnPrint.Enabled = true;
            }
            else
            {
                btnEdit.Enabled = false;
                btnDelete.Enabled = false;
                btnPrint.Enabled = false;
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
            WeaponMemoryInfo weaponInfo = (WeaponMemoryInfo)lvMain.SelectedItems[0].Tag;

            EditForm form = new EditForm(editor, attributeList, weaponInfo);
            btnPrint.Enabled = false;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
            if (form.ShowDialog() == DialogResult.Yes)
            {
                CanSave();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Removing equipment forcefully may lead to bugs. Removing equipped items will lead to an invalid save. It is recommended not to use this feature.\n\nAre you sure you want to delete this item?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                WeaponMemoryInfo weaponInfo = (WeaponMemoryInfo)lvMain.SelectedItems[0].Tag;
                editor.DeleteWeapon(weaponInfo);
                CanSave();
            }
        }

        private void CanSave()
        {
            btnSearchAll.PerformClick();
            if (searchType == "name")
            {
                btnSearchByName.PerformClick();
            }
            else if (searchType == "dur")
            {
                btnSearchByDur.PerformClick();
            }
            tslblEditState.Text = "Modified";
            btnSave.Enabled = true;
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            btnPrint.Enabled = false;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
            WeaponBytesForm form = new WeaponBytesForm(editor,lvMain.SelectedItems[0].Tag as WeaponMemoryInfo);
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
    }
}
