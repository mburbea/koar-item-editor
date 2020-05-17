using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using KoAR.Core;

namespace KoARSaveItemEditor
{
    public partial class ItemBytesForm : Form
    {
        private readonly ItemMemoryInfo weapon;
        private readonly AmalurSaveEditor editor;

        public ItemBytesForm(AmalurSaveEditor editor, ItemMemoryInfo weapon)
        {
            InitializeComponent();
            this.weapon = weapon;
            this.editor = editor;
            FormatAll();
        }

        private void FormatAll()
        {
            txtByte.Text = string.Join(" ", Array.ConvertAll(weapon.ItemBytes, x => x.ToString("X2")));
            txtByte.Select(txtByte.Text.Length, 0);
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
            foreach (string line in txtByte.Lines)
            {
                string[] str = line.Trim().Split(' ');
                foreach (string s in str)
                {
                    if (s.Trim().Length != 2)
                    {
                        MessageBox.Show("输入的代码格式不正确");
                        return;
                    }
                    byte b = byte.Parse(s.Trim(), NumberStyles.HexNumber);
                    btList.Add(b);
                }
            }
            weapon.ItemBytes = btList.ToArray();
            editor.WriteEquipmentBytes(weapon, out _);
            DialogResult = DialogResult.Yes;
            btnSave.Enabled = false;
        }
    }
}
