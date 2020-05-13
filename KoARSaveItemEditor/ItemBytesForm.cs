using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KoARSaveItemEditor
{
    public partial class ItemBytesForm : Form
    {
        private ItemMemoryInfo weapon;
        private AmalurSaveEditor editer;

        public ItemBytesForm(AmalurSaveEditor editer, ItemMemoryInfo weapon)
        {
            InitializeComponent();
            this.weapon = weapon;
            this.editer = editer;
            FormatAll();
        }

        private void FormatAll()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < weapon.ItemBytes.Length; i++)
            {
                string s = Convert.ToString(weapon.ItemBytes[i], 16).ToUpper();
                if (s.Length == 1)
                {
                    s = "0" + s;
                }
                sb.Append(s+" ");
            }
            txtByte.Text = sb.ToString();
            txtByte.Select(txtByte.Text.Length,0);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            btnSave.Enabled = true;
            txtByte.ReadOnly = false;
            btnEdit.Enabled = false;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            List<byte> btList = new List<byte>();
            foreach(string line in txtByte.Lines)
            {
                string[] str = line.Trim().Split(' ');
                foreach (string s in str)
                {
                    if(s.Trim().Length !=2)
                    {
                        MessageBox.Show("输入的代码格式不正确");
                        return;
                    }
                    byte b = byte.Parse(s.Trim(), System.Globalization.NumberStyles.HexNumber);
                    btList.Add(b);
                }
            }
            weapon.ItemBytes = btList.ToArray();
            editer.WriteWeaponByte(weapon);
            DialogResult = DialogResult.Yes;
            btnSave.Enabled = false;
        }
    }
}
