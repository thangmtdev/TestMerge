using DREXCreateFunctionForTrussLink.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Markup;

namespace DREXCreateFunctionForTrussLink.UI
{
    public partial class FormFilter : Form
    {
        private List<Data.DataFilter> _lstDataFilter = new List<Data.DataFilter>();

        public List<string> _lstFilter = new List<string>();

        public FormFilter(List<Data.DataFilter> lstDataFilter)
        {
            InitializeComponent();

            _lstDataFilter = lstDataFilter;
            Init();
        }

        private void Init()
        {
            foreach (DataFilter data in _lstDataFilter)
            {
                var index = clbFilter.Items.Add(data.Name);
                clbFilter.SetItemChecked(index, data.IsDisplay);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            foreach (var item in clbFilter.CheckedItems)
            {
                _lstFilter.Add(item.ToString());
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCheckAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbFilter.Items.Count; i++)
            {
                clbFilter.SetItemChecked(i, true);
            }
        }

        private void btnUnCheck_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbFilter.Items.Count; i++)
            {
                clbFilter.SetItemChecked(i, false);
            }
        }
    }
}