using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DREXFittingTool.Data
{
    public class FittingData
    {
        public FamilySymbol m_symbol;
        public string m_markFitting;
        public double m_height;

        public FittingData(FamilySymbol fsymbol, string markFitting, double height)
        {
            m_symbol = fsymbol;
            m_markFitting = markFitting;
            m_height = height;
        }
    }
}