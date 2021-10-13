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

            List<ViewDrafting> lstViewDrafting = new List<ViewDrafting>();
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

                ViewDrafting draftView = ViewDrafting.Create(doc, vd.Id);
                var nameDraftingView = Common.GetViewNameOriginal(doc);
                draftView.Name = Common.GetViewDraftingName(doc, nameDraftingView);
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

                Dictionary<ViewDrafting, List<FittingData>> dictFitting = fittingService.GetSymbolToPlaceToViewDrafting(draftView, vd, fittingDatas, maxRow, maxCol, nameDraftingView);
                lstViewDrafting = dictFitting.Keys.ToList();
                fittingService.PlaceFittingToDraftingView(dictFitting, maxRow, maxCol, width_one, height_one, orgMaxPoint);
            }
            else
            {
                Common.ShowWarning("新たに配置するタイプはありませんでした");
                tx.RollBack();
                return Result.Cancelled;
            }

            tx.Commit();

            string nameDraftitngViewCreated = "";
            for (int i = 0; i < lstViewDrafting.Count; i++)
            {
                if (i == 0)
                    nameDraftitngViewCreated += lstViewDrafting[i].Name;
                else
                    nameDraftitngViewCreated += "," + lstViewDrafting[i].Name;
            }
            Common.ShowInfor("製図ビュー:" + nameDraftitngViewCreated + "を作成しました");

            return Result.Succeeded;
        }
    }
}