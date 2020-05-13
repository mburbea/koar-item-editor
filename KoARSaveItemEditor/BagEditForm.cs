using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KoARSaveItemEditor
{
    public partial class BagEditForm : Form
    {
        private AmalurSaveEditor editor;
        bool isEdit = false;

        public BagEditForm(AmalurSaveEditor editor)
        {
            InitializeComponent();
            this.editor = editor;
            FormatAll();
        }

        private void FormatAll()
        {
            txtCurrentBag.Text = editor.GetMaxBagCount().ToString();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtBag.Text, out int count) && count > 0 && count <= 99999999)
            {
                editor.EditMaxBagCount(count);
                isEdit = true;
                MessageBox.Show("Modification successful!");
            }
            else
            {
                MessageBox.Show("Invalid input! Data will not be modified.");
            }
        }

        private void BagEditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isEdit)
            {
                DialogResult = DialogResult.Yes;
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void TxtCurrentBag_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
