using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace DREXCreateFunctionForTrussLink
{
    public class HdlCmdSelectTrussRevit
    {
        public string sTrussId = "";

        public void Execute(UIApplication app)
        {
            if (sTrussId == null || sTrussId.Length == 0)
            {
                return;
            }

            List<string> listTrussId = sTrussId.Split(',').ToList();

            List<BuiltInCategory> listCat = new List<BuiltInCategory>();
            listCat.Add(BuiltInCategory.OST_Doors);
            listCat.Add(BuiltInCategory.OST_Windows);

            ElementMulticategoryFilter mulFilter = new ElementMulticategoryFilter(listCat);

            FilteredElementCollector col = new FilteredElementCollector(app.ActiveUIDocument.Document);
            var listElem = col.OfClass(typeof(FamilyInstance)).WherePasses(mulFilter).Cast<FamilyInstance>().ToList();

            List<ElementId> elemIds = new List<ElementId>();
            foreach(var insCur in listElem)
            {
                if (insCur.Symbol == null)
                {
                    continue;
                }

                Parameter prmIns = insCur.LookupParameter("trussBDInstanceID");
                String sParamIns = prmIns == null ? "" : prmIns.AsString();

                Parameter prmTyp = insCur.Symbol.LookupParameter("trussBDTypeID");
                String sParamTyp = prmTyp == null ? "" : prmTyp.AsString();
                if (sParamIns == null || sParamTyp == null)
                {
                    continue;
                }

                if ( (sParamIns.Length > 0 && listTrussId.Contains(sParamIns)) ||
                    (sParamTyp.Length > 0 && listTrussId.Contains(sParamTyp))
                    )
                {
                    elemIds.Add(insCur.Id);
                }
            }
            app.ActiveUIDocument.Selection.SetElementIds(elemIds);
        }

        static public void GetTrussId(Document doc, List<ElementId> elem, ref List<String> idTrussInstance, ref List<String> idTrussType)
        {
            foreach (var elmId in elem)
            {
                Element elm = doc.GetElement(elmId);
                if (elm == null)
                {
                    continue;
                }
                FamilyInstance ins = elm as Autodesk.Revit.DB.FamilyInstance;
                if (ins == null)
                {
                    continue;
                }
                if (ins.Symbol == null)
                {
                    continue;
                }
                if (ins.Category.Id == new ElementId(BuiltInCategory.OST_Doors) || ins.Category.Id == new ElementId(BuiltInCategory.OST_Windows))
                {
                    Parameter prmIns = ins.LookupParameter("trussBDInstanceID");
                    String sParamIns = prmIns == null ? "" : prmIns.AsString();
                    if (sParamIns != null && sParamIns.Length > 0)
                    {
                        idTrussInstance.Add(sParamIns);
                    }
                    Parameter prmTyp = ins.Symbol.LookupParameter("trussBDTypeID");
                    String sParamTyp = prmTyp == null ? "" : prmTyp.AsString();
                    if (sParamTyp != null && sParamTyp.Length > 0)
                    {
                        idTrussType.Add(sParamTyp);
                    }
                }
            }
            idTrussInstance = idTrussInstance.Distinct().ToList();
            idTrussType = idTrussType.Distinct().ToList();
        }

    }
}