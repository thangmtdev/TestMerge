using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CefSharp;
<<<<<<< HEAD
=======
using DREXCreateFunctionForTrussLink.Command.Handler.Cmd.Routines;
>>>>>>> English
using DREXCreateFunctionForTrussLink.UI;
using DREXCreateFunctionForTrussLink.Utils;
using DREXTrussLibForTruss.Models.Json.FinalDesign;
using DREXTrussLibForTruss.Models.Routines.Network.FinalDesign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using WindowZEBForTruss.Models.Csv.ParameterData;
using WindowZEBForTruss.Models.Json.SettingsFile;
using WindowZEBForTruss.Models.Routines.FinalDesign;
using WindowZEBForTruss.Models.Routines.FinalDesign.RevitData;
using WindowZEBForTruss.Models.Settings;

namespace DREXCreateFunctionForTrussLink.Command.Handler
{
    class HdlCmdTrussToRevit
    {
        public int ProjectId = -1;

        public void Execute(UIApplication app)
        {
            if (ProjectId == -1)
            {
                return;
            }

            var retMsg = MessageBox.Show("Trussから建具情報を、Revitのインスタンスとタイプに取り込みますか？", "情報", MessageBoxButton.YesNo);
            if (retMsg == MessageBoxResult.No)
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
                tgr.Start();
                try
                {
                    // Trussからデータを取得
                    int projectID = data.SelectedProjectID;
                    // JsonFinalDesignData recvData = this.RecieveData(projectID);
                    JsonFinalDesignData recvData = null;
                    {

                        RoutineAPIFinalDesignTrussToRevit routine = new RoutineAPIFinalDesignTrussToRevit();
                        RoutineAPIFinalDesignTrussToRevit.Result result = routine.Execute(new RoutineAPIFinalDesignTrussToRevit.Args(
                                CommonTruss.GetUserID(),
                                projectID.ToString(),
                                CommonTruss.GetTrussTokenAPI(),
                                true
                            )).Result;
                        if (result.ExecResult)
                        {
                            recvData = result.Data.Result.Data;
                        }
                    }
                    if (recvData == null)
                    {
                        MessageBox.Show("Trussからのデータ取得に失敗しました。");
                        tgr.RollBack();
                        return;
                    }

<<<<<<< HEAD
=======
                    MessageBoxResult retDlg = RoutineTruss.ClearTrussIdNotExistTrussIdInProject(uiapp, ProjectId, recvData);
                    if (retDlg == MessageBoxResult.Cancel || retDlg == MessageBoxResult.None)
                    {
                        tgr.RollBack();
                        return;
                    }

>>>>>>> English
                    {
                        RoutineUpdateParameters routine = new RoutineUpdateParameters();
                        RoutineUpdateParameters.Result result = routine.Execute(new RoutineUpdateParameters.Args(
                            app.ActiveUIDocument, 
                            doc,
                            app.Application.VersionNumber,
                            data, 
                            recvData, ParameterSettings));
                        if (result.ExecResult == false)
                        {
                            MessageBox.Show("Trussからのデータ取込に失敗しました。");
                            tgr.RollBack();
                            return;
                        }
                    }
                    MessageBox.Show("完了しました。");
                    tgr.Assimilate();
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
