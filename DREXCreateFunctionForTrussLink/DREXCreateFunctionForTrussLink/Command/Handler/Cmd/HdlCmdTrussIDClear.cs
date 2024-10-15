using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DREXCommonLibForTruss.Models.Message;
using DREXTrussLibForTruss.Models.Json.FinalDesign;
using DREXTrussLibForTruss.Models.Routines.Network.FinalDesign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using WindowZEBForTruss.Models.Csv.ParameterData;
using WindowZEBForTruss.Models.Json.SettingsFile;
using WindowZEBForTruss.Models.Routines.FinalDesign.RevitData;
using WindowZEBForTruss.Models.Routines.FinalDesign;
using WindowZEBForTruss.Models.Settings;
using DREXCreateFunctionForTrussLink.UI;
using static DREXCreateFunctionForTrussLink.UI.FormSetParameterWindowDoor;
using DREXCreateFunctionForTrussLink.Utils;
using System.Windows;
using DREXTrussLibForTruss.Util;

namespace DREXCreateFunctionForTrussLink.Command.Handler
{
    class HdlCmdTrussIDClear
    {

        public void Execute(UIApplication appOrg)
        {
            UIApplication uiapp = appOrg;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            using (TransactionGroup tgr = new TransactionGroup(doc, "Export Revit To Truss"))
            {
                try
                {
                    tgr.Start();

                    var ret = MessageBox.Show("TrussIDクリアを実行しますか？モデルのTrussとの紐づけが解除されます。", "情報", MessageBoxButton.YesNo);
                    if (ret == MessageBoxResult.No)
                    {
                        return;
                    }

                    using (Transaction tx = new Transaction(doc, "新建具連携TrussID削除"))
                    {
                        tx.Start();

                        List<BuiltInCategory> listCat = new List<BuiltInCategory>();
                        listCat.Add(BuiltInCategory.OST_Doors);
                        listCat.Add(BuiltInCategory.OST_Windows);
                        ElementMulticategoryFilter mulFilter = new ElementMulticategoryFilter(listCat);

                        // FamilyInstance
                        FilteredElementCollector colInst = new FilteredElementCollector(doc);
                        var listInstance = colInst.OfClass(typeof(FamilyInstance)).WherePasses(mulFilter).Cast<FamilyInstance>().ToList();
                        foreach (var insCur in listInstance)
                        {
                            ParameterUtilities.SetParameterString(insCur, "TrussInstanceID", "");
                            ParameterUtilities.SetParameterString(insCur, "trussBDInstanceID", "");
                            ParameterUtilities.SetParameterString(insCur, "trussRevitElementInstanceID", "");
                        }

                        // FamilySymbol
                        FilteredElementCollector colSym = new FilteredElementCollector(doc);
                        var listSym = colSym.OfClass(typeof(FamilySymbol)).WherePasses(mulFilter).Cast<FamilySymbol>().ToList();
                        foreach (var symCur in listSym)
                        {
                            ParameterUtilities.SetParameterString(symCur, "TrussTypeID", "");
                            ParameterUtilities.SetParameterString(symCur, "trussBDTypeID", "");
                            ParameterUtilities.SetParameterString(symCur, "trussRevitElementTypeID", "");
                            ParameterUtilities.SetParameterString(symCur, "truss付加情報", "");
                            ParameterUtilities.SetParameterString(symCur, "建具_TRUSSZEB子ファミリ情報", "");
                        }
                        tx.Commit();
                    }

                    tgr.Assimilate();
                }
                catch (Exception ex)
                {
                    return;
                }
            }
            MessageBox.Show("パラメータのクリアが完了しました。");
        }
    }
}
