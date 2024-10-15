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
<<<<<<< HEAD
=======
        List<string> m_clearTrussId = null;
        public HdlCmdTrussIDClear(List<string> clearTrussId = null)
        {
            m_clearTrussId = clearTrussId;
        }

        public static bool GetNotExsistProjectTrussIdInTruss(JsonFinalDesignData recvDataRet, Document doc, ref List<String> listInstanceTypeId)
        {
            List<string> listTrussTypeId = new List<string>();
            List<string> listTrussInstanceId = new List<string>();
            foreach (var sht in recvDataRet.ShutterList)
            {
                listTrussTypeId.Add(sht.TrussID);
                foreach (var ins in sht.InstanceList)
                {
                    listTrussInstanceId.Add(ins.TrussID);
                }
            }
            foreach (var sht in recvDataRet.DoorList)
            {
                listTrussTypeId.Add(sht.TrussID);
                foreach (var ins in sht.InstanceList)
                {
                    listTrussInstanceId.Add(ins.TrussID);
                }
            }
            foreach (var sht in recvDataRet.WindowList)
            {
                listTrussTypeId.Add(sht.TrussID);
                foreach (var ins in sht.InstanceList)
                {
                    listTrussInstanceId.Add(ins.TrussID);
                }
            }

            List<String> listInstanceIdInRevit = new List<string>(), listTypeIdInRevit = new List<string>();
            GetTrussIdInProject(doc, ref listInstanceIdInRevit, ref listTypeIdInRevit);

            listInstanceTypeId = listInstanceIdInRevit.Where(x => !listTrussInstanceId.Contains(x)).ToList();
            listInstanceTypeId.AddRange(listTypeIdInRevit.Where(x => !listTrussTypeId.Contains(x)).ToList());

            return true;
        }

        static private bool GetTrussIdInProject(Document doc, ref List<String> listInstanceId, ref List<String> listTypeId)
        {
            List<BuiltInCategory> listCat = new List<BuiltInCategory>();
            listCat.Add(BuiltInCategory.OST_Doors);
            listCat.Add(BuiltInCategory.OST_Windows);
            ElementMulticategoryFilter mulFilter = new ElementMulticategoryFilter(listCat);

            // FamilyInstance
            FilteredElementCollector colInst = new FilteredElementCollector(doc);
            var listInstance = colInst.OfClass(typeof(FamilyInstance)).WherePasses(mulFilter).Cast<FamilyInstance>().ToList();
            foreach (var insCur in listInstance)
            {
                string sTrussId = ParameterUtilities.GetParameterString(insCur, "trussBDInstanceID");

                if (!string.IsNullOrEmpty(sTrussId))
                {
                    listInstanceId.Add(sTrussId);
                }
            }

            // FamilySymbol
            FilteredElementCollector colSym = new FilteredElementCollector(doc);
            var listSym = colSym.OfClass(typeof(FamilySymbol)).WherePasses(mulFilter).Cast<FamilySymbol>().ToList();
            foreach (var symCur in listSym)
            {
                string sTrussId = ParameterUtilities.GetParameterString(symCur, "trussBDTypeID");

                if (!string.IsNullOrEmpty(sTrussId))
                {
                    listTypeId.Add(sTrussId);
                }
            }

            return true;
        }
>>>>>>> English

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

<<<<<<< HEAD
                    var ret = MessageBox.Show("TrussIDクリアを実行しますか？モデルのTrussとの紐づけが解除されます。", "情報", MessageBoxButton.YesNo);
                    if (ret == MessageBoxResult.No)
                    {
                        return;
=======
                    if (m_clearTrussId == null)
                    {
                        var ret = MessageBox.Show("TrussIDクリアを実行しますか？モデルのTrussとの紐づけが解除されます。", "情報", MessageBoxButton.YesNo);
                        if (ret == MessageBoxResult.No)
                        {
                            return;
                        }
>>>>>>> English
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
<<<<<<< HEAD
=======
                            string sTrussId = ParameterUtilities.GetParameterString(insCur, "trussBDInstanceID");
                            if (m_clearTrussId != null && !m_clearTrussId.Contains(sTrussId))
                            {
                                continue;
                            }
>>>>>>> English
                            ParameterUtilities.SetParameterString(insCur, "TrussInstanceID", "");
                            ParameterUtilities.SetParameterString(insCur, "trussBDInstanceID", "");
                            ParameterUtilities.SetParameterString(insCur, "trussRevitElementInstanceID", "");
                        }

                        // FamilySymbol
                        FilteredElementCollector colSym = new FilteredElementCollector(doc);
                        var listSym = colSym.OfClass(typeof(FamilySymbol)).WherePasses(mulFilter).Cast<FamilySymbol>().ToList();
                        foreach (var symCur in listSym)
                        {
<<<<<<< HEAD
=======
                            string sTrussId = ParameterUtilities.GetParameterString(symCur, "trussBDTypeID");
                            if (m_clearTrussId != null && !m_clearTrussId.Contains(sTrussId))
                            {
                                continue;
                            }
>>>>>>> English
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
<<<<<<< HEAD
            MessageBox.Show("パラメータのクリアが完了しました。");
=======

            if(m_clearTrussId == null)
            {
                MessageBox.Show("パラメータのクリアが完了しました。");
            }
>>>>>>> English
        }
    }
}
