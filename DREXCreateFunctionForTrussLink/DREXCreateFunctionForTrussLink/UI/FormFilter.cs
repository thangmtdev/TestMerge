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
<<<<<<< HEAD
        public List<DataFilter> _lstSaveDataFilterNonCheck = new List<DataFilter>();
=======
>>>>>>> English

        public FormFilter(List<Data.DataFilter> lstDataFilter)
        {
            InitializeComponent();

            _lstDataFilter = lstDataFilter;
            Init();
        }

        private void Init()
        {
<<<<<<< HEAD
            if (_lstDataFilter?.Count > 0)
            {
                foreach (DataFilter data in _lstDataFilter.OrderBy(x => x.Name).ToList())
                {
                    var index = clbFilter.Items.Add(data.Name);
                    clbFilter.SetItemChecked(index, data.IsDisplay);
                }
=======
            foreach (DataFilter data in _lstDataFilter)
            {
                var index = clbFilter.Items.Add(data.Name);
                clbFilter.SetItemChecked(index, data.IsDisplay);
>>>>>>> English
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
<<<<<<< HEAD
            foreach (var item in clbFilter.Items)
            {
                DataFilter data = new DataFilter();
                data.Name = item.ToString();

                // Kiểm tra nếu item không được check
                if (!clbFilter.GetItemChecked(clbFilter.Items.IndexOf(item)))
                {
                    data.IsDisplay = false;
                    _lstSaveDataFilterNonCheck.Add(data);
                }
                else
                {
                    data.IsDisplay = true;
                    _lstFilter.Add(item.ToString());
                }
=======
            foreach (var item in clbFilter.CheckedItems)
            {
                _lstFilter.Add(item.ToString());
>>>>>>> English
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