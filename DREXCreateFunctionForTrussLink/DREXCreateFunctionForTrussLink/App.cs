using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CefSharp;
using DREXCreateFunctionForTrussLink.Command.Handler;
using DREXCreateFunctionForTrussLink.UI;
using DREXCreateFunctionForTrussLink.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DREXCreateFunctionForTrussLink
{
    internal class App : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            //string assemblyPath = Assembly.GetExecutingAssembly().Location;

            //application.CreateRibbonTab("Truss");
            ////Create ribbon panel
            //RibbonPanel singleHangerPanel = application.CreateRibbonPanel("Truss", "TrusSSO");
            //PushButtonData singleHangerSettingData = new PushButtonData("btnSingleHangerSetting", "Truss SS0", assemblyPath, "DREXCreateFunctionForTrussLink.Command.Command");
            //var hangerPlacementsplitButton = singleHangerPanel.AddItem(singleHangerSettingData) as SplitButton;

            application.SelectionChanged += Application_SelectionChanged;
            return Result.Succeeded;
        }

        private void Application_SelectionChanged(object sender, Autodesk.Revit.UI.Events.SelectionChangedEventArgs e)
        {
            var elem = e.GetSelectedElements();
            if (elem == null)
            {
                return;
            }
            if (elem.Count == 0)
            {
                return;
            }
            Autodesk.Revit.DB.Document doc = e.GetDocument();
            if (doc == null)
            {
                return;
            }

            List<String> idTrussInstance = new List<String>();
            List<String> idTrussType = new List<String>();
            HdlCmdSelectTrussRevit.GetTrussId(doc, elem.ToList(), ref idTrussInstance, ref idTrussType);

            FormTrussDoorWindow.SelectToTruss(null, string.Join(",", idTrussType));
        }
    }
}