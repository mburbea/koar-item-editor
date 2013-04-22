using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KingdomsofAmalurReckoningSaveEditer
{
    public partial class EditForm : Form
    {
        bool isEdit = false;

        AmalurSaveEditer editer = null;
        List<AttributeInfo> attributeList = null;
        WeaponMemoryInfo weaponInfo = null;

        public EditForm(AmalurSaveEditer editer, List<AttributeInfo> attList, WeaponMemoryInfo weaponInfo)
        {
            InitializeComponent();
            this.editer = editer;
            this.attributeList = attList;
            this.weaponInfo = weaponInfo;

            FormatAll();
        }

        private void FormatAll()
        {
            cboAddAttribute.DataSource = attributeList;
            cboAddAttribute.ValueMember = "AttributeId";
            cboAddAttribute.DisplayMember = "AttributeText";

            txtName.Text = weaponInfo.WeaponName;
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
            List<AttributeMemoryInfo> attList = editer.getAttList(weaponInfo, attributeList);
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
                foreach (AttributeMemoryInfo att in weaponInfo.WeaponAttList)
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
                MessageBox.Show("Failed to delete，please select an attribute to remove");
            }
            List<AttributeMemoryInfo> attList = weaponInfo.WeaponAttList;
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
            weaponInfo.WeaponAttList = attList;
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
                MessageBox.Show("Invalid attribute ID");
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
                MessageBox.Show("Invalid Durability Input");
                return;
            }
            if (curDur == 100 || maxDur == 100)
            {
                MessageBox.Show("为了方便列出所有装备 请勿将耐久度设置为100,不予修改");
                return;
            }
            if (curDur>maxDur || curDur > 99999 || curDur<0 || maxDur<=0)
            {
                MessageBox.Show("Invalid Durability Input");
                return;
            }
            if (!txtName.ReadOnly)
            {
                if (txtName.Text.Trim() == "")
                {
                    MessageBox.Show("No name entered, will use default Item-name");
                }
                foreach (char c in txtName.Text)
                {
                    if (c < 20 || c > 127)
                    {
                        MessageBox.Show("Name can only contain English charakters");
                        return;
                    }
                }
            }
            if (!txtName.ReadOnly)
            {
                weaponInfo.WeaponName = txtName.Text.Trim();
            }
            weaponInfo.CurrentDurability = curDur;
            weaponInfo.MaxDurability = maxDur;

            editer.WriteWeaponByte(weaponInfo);
            MessageBox.Show("Modification Successful, please save!");
            isEdit = true;
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void EditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isEdit)
            {
                this.DialogResult = DialogResult.Yes;
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
            List<AttributeMemoryInfo> attList = weaponInfo.WeaponAttList;
            for (int i = 0; i < count; i++)
            {
                AttributeMemoryInfo attInfo = new AttributeMemoryInfo();
                attInfo.Code = attCode;
                attList.Add(attInfo);
            }
            weaponInfo.WeaponAttList = attList;
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
    }
}
