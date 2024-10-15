using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CefSharp;
using DREXCreateFunctionForTrussLink.Utils;
using DREXTrussLibForTruss.Models.Json.FinalDesign;
using DREXTrussLibForTruss.Models.Routines.Network.FinalDesign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DREXCreateFunctionForTrussLink.Command.Handler.Cmd.Routines
{
    internal class RoutineTruss
    {
        static public MessageBoxResult ClearTrussIdNotExistTrussIdInProject(UIApplication app, int ProjectId, JsonFinalDesignData recvDataRetCur = null)
        {
            MessageBoxResult ret = MessageBoxResult.None;
            Document doc = app.ActiveUIDocument.Document;

            // プロジェクトのTrussIDと、TrussのTrussIDをチェックする。
            using (Transaction txCheck = new Transaction(doc, "Trussプロジェクトチェック"))
            {
                txCheck.Start();

                JsonFinalDesignData recvDataRet = recvDataRetCur;
                if (recvDataRet == null)
                {
                    try
                    {
                        // Trussからデータを取得
                        // JsonFinalDesignData recvData = this.RecieveData(projectID);

                        {
                            RoutineAPIFinalDesignTrussToRevit routine = new RoutineAPIFinalDesignTrussToRevit();
                            RoutineAPIFinalDesignTrussToRevit.Result result = routine.Execute(new RoutineAPIFinalDesignTrussToRevit.Args(
                                    CommonTruss.GetUserID(),
                                    ProjectId.ToString(),
                                    CommonTruss.GetTrussTokenAPI(),
                                    true
                                )).Result;
                            if (result.ExecResult)
                            {
                                recvDataRet = result.Data.Result.Data;
                            }
                        }
                        if (recvDataRet == null)
                        {
                            MessageBox.Show("Trussからのデータ取得に失敗しました。");
                            return ret;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Trussへの情報の作成に失敗しました。");
                        return ret;
                    }
                }

                txCheck.Commit();


                if (recvDataRet != null)
                {
                    List<string> listRet = new List<string>();
                    HdlCmdTrussIDClear.GetNotExsistProjectTrussIdInTruss(recvDataRet, doc, ref listRet);

                    if (listRet.Count > 0)
                    {
                        listRet = listRet.Distinct().ToList();
                        ret = MessageBox.Show(listRet.Count + "個のプロジェクト内の要素が、Trussプロジェクトに紐づいていません。\nプロジェクト内の要素のTrussIDをクリアしますか？\n\nはい：クリアしてTruss連携実行\nいいえ：クリアせずにTruss連携実行\nキャンセル：処理のキャンセル", "", MessageBoxButton.YesNoCancel);
                        if (ret == MessageBoxResult.Yes)
                        {
                            HdlCmdTrussIDClear clr = new HdlCmdTrussIDClear(listRet);
                            clr.Execute(app);
                        }
                    }
                }
            }

            return ret;
        }
    }
}
