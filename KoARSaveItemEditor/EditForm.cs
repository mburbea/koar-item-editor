using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KoARSaveItemEditor
{
    public partial class EditForm : Form
    {
        bool isEdit = false;

        AmalurSaveEditor editor = null;
        List<AttributeInfo> attributeList = null;
        ItemMemoryInfo weaponInfo = null;

        public EditForm(AmalurSaveEditor editor, List<AttributeInfo> attList, ItemMemoryInfo weaponInfo)
        {
            InitializeComponent();
            this.editor = editor;
            attributeList = attList;
            this.weaponInfo = weaponInfo;

            FormatAll(attList);
        }

        private void FormatAll(List<AttributeInfo> attList)
        {
            cboAddAttribute.DataSource = attList;
            cboAddAttribute.ValueMember = "AttributeId";
            cboAddAttribute.DisplayMember = "AttributeText";

            txtName.Text = weaponInfo.ItemName;
            lblAttCount.Text = weaponInfo.AttCount.ToString();
            txtCurrentDurability.Text = weaponInfo.CurrentDurability.ToString();
            txtMaxDurability.Text = weaponInfo.MaxDurability.ToString();
            btnAdd.Enabled = true;

            txtAttCode.Text = "";

            DataBinding();

            if (int.Parse(lblAttCount.Text) < 65535)
            {
                btnAdd.Enabled = true;
                btnAddAttByInput.Enabled = true;
            }
            else
            {
                btnAdd.Enabled = false;
                btnAddAttByInput.Enabled = false;
            }
        }

        private void DataBinding()
        {
            List<AttributeMemoryInfo> attList = editor.GetAttList(weaponInfo, attributeList);
            List<AttributeMemoryInfo> temp = new List<AttributeMemoryInfo>();
            
            foreach(AttributeMemoryInfo att in attList)
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
            cboExtendAttIndex.DataSource = null;
            cboExtendAttIndex.DataSource = temp;
            cboExtendAttIndex.DisplayMember = "Detail";
            lblAttCount.Text = attList.Count.ToString();
        }

        private void cboExtendAttIndex_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboExtendAttIndex.SelectedIndex >= 0)
            {
                txtAttCode.Text = (cboExtendAttIndex.SelectedItem as AttributeMemoryInfo).Code;
                int i = 0;
                foreach (AttributeMemoryInfo att in weaponInfo.ItemAttList)
                {
                    if (att.Code.ToUpper() == txtAttCode.Text.ToUpper())
                    {
                        i++;
                    }
                }
                lblCodeCount.Text = i.ToString();
            }
            else
            {
                txtAttCode.Text = "";
                lblCodeCount.Text = "0";
            }
        }

        private void btnDeleteAttribute_Click(object sender, EventArgs e)
        {
            if (txtAttCode.Text == "")
            {
                MessageBox.Show("Failed to delete. Please select another property to remove.");
            }
            List<AttributeMemoryInfo> attList = weaponInfo.ItemAttList;
            for (int i = 0; i < numDelete.Value; i++)
            {
                foreach (AttributeMemoryInfo att in attList)
                {
                    if (att.Code.ToUpper() == txtAttCode.Text.ToUpper())
                    {
                        attList.Remove(att);
                        break;
                    }
                }
            }
            weaponInfo.ItemAttList = attList;
            DataBinding();
            isEdit = true;
            numDelete.Value = 0;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (numAddBySelect.Value <= 0)
            {
                return;
            }
            string id = (cboAddAttribute.SelectedItem as AttributeInfo).AttributeId;
            if (id == null || id.Trim().Length != 6)
            {
                MessageBox.Show("Invalid attribute ID!");
                return;
            }
            AddAttribute((cboAddAttribute.SelectedItem as AttributeInfo).AttributeId, (int)numAddBySelect.Value);
            numAddBySelect.Value = 0;
        }

        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            float curDur = 0;
            float maxDur = 0;
            try
            {
                curDur = float.Parse(txtCurrentDurability.Text);
                maxDur = float.Parse(txtMaxDurability.Text);
            }
            catch
            {
                MessageBox.Show("Invalid durability input.");
                return;
            }
            if (curDur == 100 || maxDur == 100)
            {
                MessageBox.Show("Perfect 100! Congrats for finding this hidden nugget!");
                return;
            }
            if (curDur>maxDur || curDur > 99999 || curDur<0 || maxDur<=0)
            {
                MessageBox.Show("Invalid durability input.");
                return;
            }
            if (!txtName.ReadOnly)
            {
                if (txtName.Text.Trim() == "")
                {
                    MessageBox.Show("No name entered! Reverting to default item name.");
                }
                foreach (char c in txtName.Text)
                {
                    if (c < 20 || c > 127)
                    {
                        MessageBox.Show("Name can only contain English characters.");
                        return;
                    }
                }
            }
            if (!txtName.ReadOnly)
            {
                weaponInfo.ItemName = txtName.Text.Trim();
            }
            weaponInfo.CurrentDurability = curDur;
            weaponInfo.MaxDurability = maxDur;

            editor.WriteWeaponByte(weaponInfo);
            MessageBox.Show("Modification successful. Please save.");
            isEdit = true;
            DialogResult = DialogResult.Yes;
            Close();
        }

        private void EditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isEdit)
            {
                DialogResult = DialogResult.Yes;
            }
        }

        private void btnAddAttByInput_Click(object sender, EventArgs e)
        {
            if (numAddByInput.Value <= 0)
            {
                return;
            }
            bool isTrue = true;

            foreach (char c in txtAttCodeInput.Text.ToUpper())
            {
                if (!(c >= '0' && c <= '9' || c >= 'A' && c <= 'Z'))
                {
                    isTrue = false;
                    break;
                }
            }
            if (txtAttCodeInput.Text.Length != 6 || !isTrue)
            {
                MessageBox.Show("Invalid code");
                return;
            }
            AddAttribute(txtAttCodeInput.Text,(int)numAddByInput.Value);

            txtAttCode.Text="";
            numAddByInput.Value=0;
        }

        private void AddAttribute(string attCode,int count)
        {
            List<AttributeMemoryInfo> attList = weaponInfo.ItemAttList;
            for (int i = 0; i < count; i++)
            {
                AttributeMemoryInfo attInfo = new AttributeMemoryInfo
                {
                    Code = attCode
                };
                attList.Add(attInfo);
            }
            weaponInfo.ItemAttList = attList;
            DataBinding();
            isEdit = true;
        }

        private void chkEditName_CheckedChanged(object sender, EventArgs e)
        {
            if (chkEditName.Checked)
            {
                txtName.ReadOnly = false;
            }
            else
            {
                txtName.ReadOnly = true;
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void TxtCurrentDurability_TextChanged(object sender, EventArgs e)
        {

        }

        private void EditForm_Load(object sender, EventArgs e)
        {

        }

        private void TxtAttCodeInput_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
