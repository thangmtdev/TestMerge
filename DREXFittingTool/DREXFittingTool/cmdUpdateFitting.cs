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

            FormUpdateFitting frmFitting = new FormUpdateFitting(uidoc, lstViewName);
            frmFitting.ShowDialog();

            if (frmFitting.DialogResult != System.Windows.Forms.DialogResult.OK)
                return Result.Cancelled;

            List<MappingData> mappingDatas = Common.GetDataMapping(fileData);

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
            List<FamilyInstance> lstDeleteInstance = FilterFromExistData(doc, fittingDatas, frmFitting.m_SelectedViewName);

            Dictionary<ViewDrafting, List<FittingData>> dictFitting = new Dictionary<ViewDrafting, List<FittingData>>();
            if (fittingDatas.Count > 0)
            {
                if (Common.ValidateSizeFitting(fittingDatas.FirstOrDefault(), frmFitting.m_Width, frmFitting.m_Height))
                {
                    Common.ShowError("横改行幅もしくは縦改ページ幅の値が小さいため配置できません");
                    tx.RollBack();
                    return Result.Cancelled;
                }

                if (frmFitting.IsFitting)
                    fittingDatas = fittingDatas.OrderBy(x => x.m_markFitting).ToList();
                else
                    fittingDatas = fittingDatas.OrderBy(x => x.m_height).ThenBy(x => x.m_width).ToList();

                ViewFamilyType vd = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType))
                                                                     .Cast<ViewFamilyType>()
                                                                     .FirstOrDefault(q => q.ViewFamily == ViewFamily.Drafting);

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

                dictFitting = fittingService.GetSymbolToPlaceToViewDrafting(viewDraft, vd, fittingDatas, maxRow, maxCol, frmFitting.m_SelectedViewName);

                fittingService.UpdateFittingToDraftingView(dictFitting, maxRow, maxCol, width_one, height_one, orgMaxPoint);
            }
            else
                Common.ShowWarning("新たに配置するタイプはありませんでした");

            //Get messgae fitting instance to delete and delete it
            string fitDeleteName = GetMessageDeleteFitting(doc, lstDeleteInstance);
            doc.Delete(lstDeleteInstance.Select(x => x.Id).ToList());

            tx.Commit();

            //Get messgae fitting which update to drafting view
            string fitCreateName = GetMessageUpdateFitting(dictFitting);

            //Show message fitting deleted
            if (fitDeleteName != "")
                Common.ShowWarning(fitDeleteName);

            //Show message fitting created
            if (fitCreateName != "")
                Common.ShowInfor(fitCreateName);

            return Result.Succeeded;
        }

        /// <summary>
        /// Get message update fitting
        /// </summary>
        /// <param name="dictFitting"></param>
        /// <returns></returns>
        private string GetMessageUpdateFitting(Dictionary<ViewDrafting, List<FittingData>> dictFitting)
        {
            List<string> lstMessage = new List<string>();
            foreach (var item in dictFitting)
            {
                string fittingName = "";
                for (int i = 0; i < item.Value.Count; i++)
                {
                    if (i == 0)
                        fittingName += item.Value[i].m_symbol.Name;
                    else
                        fittingName += "," + item.Value[i].m_symbol.Name;
                }

                string message = fittingName + "を" + item.Key.Name + "の中に記入しました";
                lstMessage.Add(message);
            }

            //Get final message to show
            string finalMessage = "";
            for (int i = 0; i < lstMessage.Count; i++)
            {
                if (i == 0)
                    finalMessage += lstMessage[i];
                else
                    finalMessage += "\n" + lstMessage[i];
            }

            return finalMessage;
        }

        /// <summary>
        /// Get message delete fiting instance
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="lstDeleteInstance"></param>
        /// <returns></returns>
        private string GetMessageDeleteFitting(Document doc, List<FamilyInstance> lstDeleteInstance)
        {
            //Get fitting instance by view name
            Dictionary<string, List<string>> dictCheck = new Dictionary<string, List<string>>();

            foreach (FamilyInstance instance in lstDeleteInstance)
            {
                ElementId viewId = instance.OwnerViewId;
                Element viewElem = doc.GetElement(viewId);
                if (viewElem != null)
                {
                    if (!dictCheck.ContainsKey(viewElem.Name))
                    {
                        List<string> lstFitName = new List<string>();
                        lstFitName.Add(instance.Symbol.Name);
                        dictCheck.Add(viewElem.Name, lstFitName);
                    }
                    else
                        dictCheck[viewElem.Name].Add(instance.Symbol.Name);
                }
            }

            //Get list message to show
            List<string> lstMessage = new List<string>();

            foreach (var item in dictCheck)
            {
                string fittingName = "";
                for (int i = 0; i < item.Value.Count; i++)
                {
                    if (i == 0)
                        fittingName += item.Value[i];
                    else
                        fittingName += "," + item.Value[i];
                }

                string message = fittingName + "を" + item.Key + "の中から消去しました";
                lstMessage.Add(message);
            }

            //get final message to show
            string finalMessage = "";
            for (int i = 0; i < lstMessage.Count; i++)
            {
                if (i == 0)
                    finalMessage += lstMessage[i];
                else
                    finalMessage += "\n" + lstMessage[i];
            }

            return finalMessage;
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
                    fittingDatas.Remove(found);
                else
                    retVal.Add(fittingInstance);
            }

            return retVal;
        }
    }
}