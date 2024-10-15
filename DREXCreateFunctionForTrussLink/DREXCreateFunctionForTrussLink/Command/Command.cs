#region Name spaces

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DREXCreateFunctionForTrussLink.UI;
using DREXCreateFunctionForTrussLink.Utils;
using System.Diagnostics;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;

#endregion Name spaces

namespace DREXCreateFunctionForTrussLink.Command
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        private UIDocument _uiDoc;
        private Document _doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (FormTrussDoorWindow.IsShowDlg())
            {
                System.Windows.MessageBox.Show("既に建具Truss連携のダイアログが開かれています。");
                return Result.Cancelled;
            }
            UIApplication uiapp = commandData.Application;
            _uiDoc = uiapp.ActiveUIDocument;
            _doc = _uiDoc.Document;

            MdlSetting.SetInitSettingTrussURL();

            var token = CommonTruss.GetTrussTokenAPI();
            if (token == string.Empty)
            {
                System.Windows.Forms.MessageBox.Show("認証情報が認識できません。Trussログインを実行してください。");
                return Result.Cancelled;
            }

            var userInfo = CommonTruss.GetUserID();
            if (userInfo != string.Empty)
            {
                Process process = Process.GetCurrentProcess();

                IntPtr h = process.MainWindowHandle;
                UI.FormTrussDoorWindow frmMain = new UI.FormTrussDoorWindow(userInfo, token, uiapp);
                frmMain.Show(new JtWindowHandle(h));
            }
            else
                System.Windows.Forms.MessageBox.Show("認証情報が認識できません。Trussログインを実行してください。");

            return Result.Succeeded;
        }
    }

    public class JtWindowHandle : IWin32Window
    {
        private IntPtr _hwnd;

        public JtWindowHandle(IntPtr h)
        {
            Debug.Assert(IntPtr.Zero != h,
              "expected non-null window handle");

            _hwnd = h;
        }

        public IntPtr Handle
        {
            get
            {
                return _hwnd;
            }
        }
    }
}