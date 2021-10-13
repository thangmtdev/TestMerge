using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DREXFittingTool.Data;
using DREXFittingTool.Services;
using DREXFittingTool.UI;
using DREXFittingTool.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DREXFittingTool
{
    [Transaction(TransactionMode.Manual)]
    public class cmdCreateFitting : IExternalCommand
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

            FormFitting frmFitting = new FormFitting();
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

                ViewFamilyType vd = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType))
                                                                     .Cast<ViewFamilyType>()
                                                                     .FirstOrDefault(q => q.ViewFamily == ViewFamily.Drafting);

                ViewDrafting draftView = ViewDrafting.Create(doc, vd.Id);
                string nameOrg = Common.GetViewNameOriginal(doc);
                draftView.Name = Common.GetViewDraftingName(doc, nameOrg);
                draftView.Scale = 10;

                //Get data row, col, width, height to input
                int maxRow = 0;
                int maxCol = 0;
                double width_one = 0.0;
                double height_one = 0.0;
                XYZ orgMaxPoint = XYZ.Zero;

                Common.GetMaxRowAndCol(doc,
                                       draftView,
                                       fittingDatas.FirstOrDefault(),
                                       frmFitting.m_Width,
                                       frmFitting.m_Height,
                                       ref maxRow,
                                       ref maxCol,
                                       ref width_one,
                                       ref height_one,
                                       ref orgMaxPoint);

                Dictionary<ViewDrafting, List<FittingData>> dictFitting = fittingService.GetSymbolToPlaceToViewDrafting(draftView, vd, fittingDatas, maxRow, maxCol, nameOrg);

                fittingService.PlaceFittingToDraftingView(dictFitting, maxRow, maxCol, width_one, height_one, orgMaxPoint);
            }
            else
            {
                Common.ShowWarning("配置するタイプがありません。");
                tx.RollBack();
                return Result.Cancelled;
            }

            tx.Commit();

            return Result.Succeeded;
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