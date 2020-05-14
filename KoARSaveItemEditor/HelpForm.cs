using System;
using System.Windows.Forms;

namespace KoARSaveItemEditor
{
    public partial class HelpForm : Form
    {
        public HelpForm() => InitializeComponent();

        private void OKButton_Click(object sender, EventArgs e) => Close();
    }
}
