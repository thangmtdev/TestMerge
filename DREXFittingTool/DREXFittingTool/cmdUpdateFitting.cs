using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DREXFittingTool.Data;
using DREXFittingTool.Services;
using DREXFittingTool.UI;
using DREXFittingTool.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DREXFittingTool
{
    [Transaction(TransactionMode.Manual)]
    public class cmdUpdateFitting : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            Transaction tx = new Transaction(doc);

            string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "鉄骨継手マッピングファイル_FJ(一般形鋼）.csv");

            if (!File.Exists(filePath))
            {
                message = "鉄骨継手マッピングファイル_FJ(一般形鋼）.csvファイルが存在しません。";
                return Result.Cancelled;
            }

            //Config mapping file
            string[] fileData = File.ReadAllLines(filePath, Encoding.GetEncoding("SHIFT_JIS"));
            if (fileData[0].Length < 6)
            {
                message = "鉄骨継手マッピングファイル_FJ(一般形鋼）.csv は無です。";
                return Result.Cancelled;
            }

            Family fittingFamily = new FilteredElementCollector(doc).OfClass(typeof(Family))
                                                                    .Cast<Family>()
                                                                    .FirstOrDefault(x => x.Name == Define.FamilyName);

            if (fittingFamily == null)
            {
                message = "詳細項目のファミリが存在しません。";
                return Result.Cancelled;
            }

            //Get view drafting to update
            List<ViewDrafting> viewDrafts = new FilteredElementCollector(doc).OfClass(typeof(ViewDrafting))
                                                                             .Cast<ViewDrafting>()
                                                                             .Where(x => Common.GetActualName(x.Name).Contains(Define.ViewName))
                                                                             .ToList();

            if (viewDrafts.Count == 0)
            {
                message = "更新する製図ビューがありません。";
                return Result.Cancelled;
            }

            List<string> lstViewName = viewDrafts.Select(x => Common.GetActualName(x.Name)).Distinct().ToList();

            FormUpdateFitting frmFitting = new FormUpdateFitting(lstViewName);
            frmFitting.ShowDialog();

            if (frmFitting.DialogResult != System.Windows.Forms.DialogResult.OK)
                return Result.Cancelled;

            List<MappingData> mappingDatas = GetDataMapping(fileData);

            List<FamilyInstance> girders = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance))
                                                                            .OfCategory(BuiltInCategory.OST_StructuralFraming)
                                                                            .Cast<FamilyInstance>()
                                                                            .Where(x => Common.ValidateFramingParameter(x))
                                                                            .ToList();

            //Config data fitting
            ElementId firstSymbolId = fittingFamily.GetFamilySymbolIds().FirstOrDefault();
            FamilySymbol firstSymbol = doc.GetElement(firstSymbolId) as FamilySymbol;

            //Create type and order
            tx.Start("Create fitting tool");
            FittingService fittingService = new FittingService(doc, girders, firstSymbol, mappingDatas);
            List<FittingData> fittingDatas = fittingService.GetFittingData();

            //Filter fitting data from exist
            var lstDeleteInstance = FilterFromExistData(doc, fittingDatas, frmFitting.m_SelectedViewName);

            if (fittingDatas.Count > 0)
            {
                if (Common.ValidateSizeFitting(fittingDatas.FirstOrDefault(), frmFitting.m_Width, frmFitting.m_Height))
                {
                    Common.ShowError("高さ（幅）が小さすぎて配置できません");
                    tx.RollBack();
                    return Result.Cancelled;
                }

                if (frmFitting.IsFitting)
                    fittingDatas = fittingDatas.OrderBy(x => x.m_markFitting).ToList();
                else
                    fittingDatas = fittingDatas.OrderBy(x => x.m_height).ToList();

                //Get last view drafting
                ViewDrafting viewDraft = viewDrafts.Where(x => x.Name.Contains(frmFitting.m_SelectedViewName)).OrderByDescending(x => x.Name).FirstOrDefault();

                //Get data row, col, width, height to input
                int maxRow = 0;
                int maxCol = 0;
                double width_one = 0.0;
                double height_one = 0.0;
                XYZ orgMaxPoint = XYZ.Zero;

                Common.GetMaxRowAndCol(doc,
                                       viewDraft,
                                       fittingDatas.FirstOrDefault(),
                                       frmFitting.m_Width,
                                       frmFitting.m_Height,
                                       ref maxRow,
                                       ref maxCol,
                                       ref width_one,
                                       ref height_one,
                                       ref orgMaxPoint);

                fittingService.UpdateFittingToDraftingView(fittingDatas, viewDraft, maxRow, maxCol, width_one, height_one, orgMaxPoint, frmFitting.m_SelectedViewName);
            }
            else
                Common.ShowWarning("配置するタイプがありません。");

            doc.Delete(lstDeleteInstance.Select(x => x.Id).ToList());

            tx.Commit();

            return Result.Succeeded;
        }

        /// <summary>
        /// Filter from exist data
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="fittingDatas"></param>
        private List<FamilyInstance> FilterFromExistData(Document doc, List<FittingData> fittingDatas, string vName)
        {
            List<FamilyInstance> retVal = new List<FamilyInstance>();

            //Get list drafting view
            List<FamilyInstance> fittingInstances = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance))
                                                                                     .Cast<FamilyInstance>()
                                                                                     .Where(x => x.Symbol != null && x.Symbol.Family.Name == Define.FamilyName)
                                                                                     .Where(x => Common.GetOwnerViewName(doc, x).Contains(vName))
                                                                                     .ToList();

            foreach (FamilyInstance fittingInstance in fittingInstances)
            {
                var found = fittingDatas.FirstOrDefault(x => x.m_symbol.Name == fittingInstance.Symbol.Name);
                if (found != null)
                {
                    fittingDatas.Remove(found);
                }
                else
                    retVal.Add(fittingInstance);
            }

            return retVal;
        }

        /// <summary>
        /// Get data mapping
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>
        private List<MappingData> GetDataMapping(string[] fileData)
        {
            List<MappingData> retVal = new List<MappingData>();

            //Get list Parameter
            string[] paramSplit = fileData[0].Split(',');
            List<string> paramNames = new List<string>();
            for (int i = 0; i < paramSplit.Length; i++)
            {
                paramNames.Add(paramSplit[i]);
            }

            for (int i = 1; i < fileData.Length; i++)
            {
                try
                {
                    string[] data = fileData[i].Split(',');

                    KeyData key1 = new KeyData(paramNames[0], data[0]);
                    KeyData key2 = new KeyData(paramNames[1], data[1]);
                    KeyData key3 = new KeyData(paramNames[2], data[2]);
                    KeyData key4 = new KeyData(paramNames[3], data[3]);
                    KeyData key5 = new KeyData(paramNames[4], data[4]);
                    MappingData mData = new MappingData(key2, key1, key3, key4, key5);

                    for (int j = 0; j < data.Length; j++)
                    {
                        if (!mData.m_dictParameter.ContainsKey(paramNames[j]))
                            mData.m_dictParameter.Add(paramNames[j], data[j]);
                    }

                    retVal.Add(mData);
                }
                catch
                {
                }
            }
            return retVal;
        }
    }
}