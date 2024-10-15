using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DREXCreateFunctionForTrussLink.Utils
{
    internal class FormUtil
    {
        public static int GetColumnInDataGrid(DataGridView dg, string sColumnName)
        {
<<<<<<< HEAD
            for (int ii = 0; ii < dg.Columns.Count; ii++)
=======
            for (int ii = 0; ii < dg.Columns.Count;ii++)
>>>>>>> English
            {
                if (dg.Columns[ii].HeaderText == sColumnName)
                {
                    return ii;
                }
            }

            return -1;
        }
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> English
