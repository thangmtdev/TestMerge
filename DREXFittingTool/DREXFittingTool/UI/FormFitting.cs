using DREXFittingTool.Utils;
using System;
using System.Windows.Forms;

namespace DREXFittingTool.UI
{
    public partial class FormFitting : Form
    {
        public bool IsFitting = false;
        public double m_Height = 0;
        public double m_Width = 0;

        public FormFitting()
        {
            InitializeComponent();
            this.Text = "DREXCreateFittingTool";
        }

        private void FormFitting_Load(object sender, EventArgs e)
        {
            radFitting.Checked = true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (txtWidth.Text.Trim() == "")
            {
                Common.ShowError("横改行幅を入力してください。");
                return;
            }

            if (txtHeight.Text.Trim() == "")
            {
                Common.ShowError("縦改ページ幅を入力してください。");
                return;
            }

            if (radFitting.Checked)
                IsFitting = true;
            else
                IsFitting = false;

            if (Convert.ToInt32(txtWidth.Text) == 0 || Convert.ToInt32(txtHeight.Text) == 0)
            {
                Common.ShowError("横改行幅と縦改ページ幅は0より大きくする必要があります。");
                return;
            }

            m_Height = Convert.ToDouble(txtHeight.Text) / 304.8;
            m_Width = Convert.ToDouble(txtWidth.Text) / 304.8;
            this.DialogResult = DialogResult.OK;
        }

        private void txtRow_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as System.Windows.Forms.TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void txtCol_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as System.Windows.Forms.TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }
    }
}