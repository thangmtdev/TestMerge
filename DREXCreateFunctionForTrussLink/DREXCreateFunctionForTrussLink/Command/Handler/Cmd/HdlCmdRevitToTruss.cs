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

namespace DREXCreateFunctionForTrussLink.Command.Handler
{
    internal class HdlCmdRevitToTruss
    {
        public int ProjectId = -1;

        public void Execute(UIApplication app)
        {
            if (ProjectId == -1)
            {
                return;
            }

            FormSetParameterWindowDoor dlgTruss = new FormSetParameterWindowDoor(app.ActiveUIDocument.Document);
            var ret = dlgTruss.ShowDialog();
            if (ret == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }

            UIApplication uiapp = app;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application appCur = uiapp.Application;
            Document doc = uidoc.Document;

            JsonFinalDesignSettingsData data = new JsonFinalDesignSettingsData();
            data.LinkedDoor = true;
            data.LinkedShutter = true;
            data.LinkedWindow = true;
            data.WindowDupDir = true;
            data.FilterInstance = true;
            data.LinkedCommonBuildings = true;
            data.DoorDupDir = false;
            data.ShutterDupDir = false;
            data.SelectedProjectID = this.ProjectId;

            RevitProjectData ProjectData = null;
            CsvParameters ParameterSettings = null;
            {
                RoutineCheckEnableProject routine = new RoutineCheckEnableProject();
                RoutineCheckEnableProject.Result result = routine.Execute(new RoutineCheckEnableProject.Args(doc));
                ProjectData = result.Data;
                ParameterSettings = result.ParameterSettings;
            }

            using (TransactionGroup tgr = new TransactionGroup(doc, "Export Revit To Truss"))
            {
                try
                {
                    tgr.Start();

                    // まずは建具　記号と種類を設定する。
                    using (Transaction txCur = new Transaction(doc, "建具パラメータ設定"))
                    {
                        txCur.Start();
                        foreach (RowDoorWindow rowCur in dlgTruss.RetDoorWindow)
                        {
                            if (rowCur.GetSymbol() == null)
                            {
                                continue;
                            }

                            if (rowCur.ClmCategory == "シャッター")
                            {
                                Common.SetParameter(rowCur.GetSymbol(), "建具_シャッター類", 1);
                            }
                            else if (rowCur.ClmCategory == "ドア")
                            {
                                Common.SetParameter(rowCur.GetSymbol(), "建具_シャッター類", 0);
                            }

                            Common.SetParameter(rowCur.GetSymbol(), "建具_種類", rowCur.ClmTateguShurui);
                            Common.SetParameter(rowCur.GetSymbol(), "建具_番号", rowCur.ClmTateguBango);

                            Parameter paramTrussInfo = Common.GetParameter(rowCur.GetSymbol(), "建具_TRUSSZEB子ファミリ情報");
                            if (paramTrussInfo != null)
                            {
                                Parameter paramTrussId = Common.GetParameter(rowCur.GetSymbol(), "trussBDTypeID");

                                String sTrussId = paramTrussId == null ? null : paramTrussId.AsString();
                                String sTrussInfo = paramTrussInfo.AsString();
                                if (string.IsNullOrEmpty(sTrussId) && !string.IsNullOrEmpty(sTrussInfo))
                                {
                                    paramTrussInfo.Set("");
                                }
                            }
                        }
                        txCur.Commit();
                    }

                    // ③	送信用データを作成
                    // JsonFinalDesignData sendData = this.CreateSendData(doc, data);
                    JsonFinalDesignData sendData = null;
                    {
                        JsonFinalDesignData jsonData = new JsonFinalDesignData();
                        RoutineCreateSendData routine = new RoutineCreateSendData();
                        RoutineCreateSendData.Result result = routine.Execute(new RoutineCreateSendData.Args(
                            doc,
                            data,
                            ProjectData,
                            ParameterSettings));
                        if (result.ExecResult)
                        {
                            sendData = result.SendData;
                        }
                    }

                    if (sendData == null)
                    {
                        tgr.RollBack();
                        MessageBox.Show("Trussへの情報の作成に失敗しました。");
                        return;
                    }

                    // ④	Trussに対して、RevitのデータをTrussへ送信
                    int projectID = data.SelectedProjectID;
                    // JsonFinalDesignData recvData = this.SendData(doc, projectID, sendData);
                    JsonFinalDesignData recvData = null;
                    {
                        RoutineAPIFinalDesignRevitToTruss routine = new RoutineAPIFinalDesignRevitToTruss();
                        RoutineAPIFinalDesignRevitToTruss.Result result = routine.Execute(
                            new RoutineAPIFinalDesignRevitToTruss.Args(
                                CommonTruss.GetUserID(),
                                projectID.ToString(),
                                CommonTruss.GetTrussTokenAPI(),
                                sendData,
                                true
                                )
                            ).Result;

                        if (result.ExecResult)
                        {
                            // recvData = this.MergeJsonData(doc, sendData, result.Data.Result.Data);
                            // 送信データのignoredデータを設定する
                            RoutineMergeJsonData routineMerge = new RoutineMergeJsonData();
                            RoutineMergeJsonData.Result resultMerge = routineMerge.Execute(new RoutineMergeJsonData.Args(
                                doc,
                                sendData,
                                result.Data.Result.Data));
                            recvData = resultMerge.MergedData;
                        }
                    }

                    // ⑤ ④の処理で返ってきたTrussIDを更新 ==============================
                    if (recvData == null)
                    {
                        tgr.RollBack();
                        MessageBox.Show("Trussへの送信に失敗しました。");
                        return;
                    }

                    {
                        RoutineSetRecvData routine = new RoutineSetRecvData();
                        RoutineSetRecvData.Result result = routine.Execute(new RoutineSetRecvData.Args(doc, recvData, ParameterSettings));

                        if (result.ExecResult == false)
                        {
                            tgr.RollBack();
                            MessageBox.Show("Trussへの送信後のデータの設定に失敗しました。");
                            return;
                        }
                    }

                    MessageBox.Show("完了しました。");
                    tgr.Assimilate();

                    FormTrussDoorWindow.Reload();
                }
                catch (Exception ex)
                {
                    tgr.RollBack();
                }
            }

            ProjectId = -1;
        }
    }
}