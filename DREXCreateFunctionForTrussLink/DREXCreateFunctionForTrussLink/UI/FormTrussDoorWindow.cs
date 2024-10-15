using Autodesk.Revit.UI;
using CefSharp.WinForms;
using CefSharp;
using DREXCreateFunctionForTrussLink.Data;
using DREXCreateFunctionForTrussLink.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using CefSharp.Handler;
using Autodesk.Revit.DB;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using DREXCreateFunctionForTrussLink.Command.Handler;
using System.Reflection;
using Autodesk.Revit.DB.Structure;
using System.Timers;

namespace DREXCreateFunctionForTrussLink.UI
{
    public partial class FormTrussDoorWindow : System.Windows.Forms.Form
    {
        private static bool m_isFirst = true;

        private ExternalEvent m_ExEvent = null;
        private HandlerCmdForTruss m_Handler = null;

        private ChromiumWebBrowser chromeBrowser;

        private string m_UserId = string.Empty;
        private string m_Token = string.Empty;

        // private string WEB_BROWSER_INTERIOR = CommonTruss.BASE_API_URL + "/project/{1}/interior?api_token={2}";
        private string WEB_BROWSER()
        {
            return CommonTruss.BASE_API_URL() + "revit/{0}/project/{1}/window-frame?api_token={2}";
        }

        // https://dev.truss.me/revit/97/project/1897/interior?api_token=Eo2Z1f023XaoR7xftbbBfI8NFv0f9tiW1MNhZk3AdRVp8PFjSe4Sfwz4mqGqBY3WHT0muRMpr91ZBY9S

        public static FormTrussDoorWindow _Form = null;

        private UIApplication _uiapp;

        private double dZoomLevel = 0.0;

        static public bool IsFirst()
        {
            return m_isFirst;
        }

        public FormTrussDoorWindow(string userId, string token, UIApplication uiapp)
        {
            InitializeComponent();

            this.m_Handler = new HandlerCmdForTruss();
            this.m_ExEvent = ExternalEvent.Create(this.m_Handler);

            m_UserId = userId;
            m_Token = token;

            _uiapp = uiapp;
            Init();

            _Form = this;
        }

        /// <summary>
        /// Init list project
        /// </summary>
        private void Init()
        {
            if (!Cef.IsInitialized)
            {
                CefSettings settings = new CefSettings();
                Cef.Initialize(settings);
            }

            var lstProjectModel = CommonTruss.GetProjectsToken(m_UserId, m_Token);

            if (lstProjectModel.Count == 0)
            {
                MessageBox.Show("プロジェクトの一覧の取得に失敗しました。Trussログインしているかどうか確認してさい。");
                return;
            }
            cbxListProject.DataSource = lstProjectModel.OrderByDescending(x => x.Updated_at).ToList();
            cbxListProject.DisplayMember = "Name";
            cbxListProject.SelectedIndex = 0;

            this.Width = Properties.Settings.Default.DlgWidth;
            this.Height = Properties.Settings.Default.DlgHeight;
            this.dZoomLevel = Properties.Settings.Default.DlgScale;
        }

        /// <summary>
        /// Event combobox selected index changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxListProject_SelectedIndexChanged(object sender, EventArgs e)
        {
            var project = cbxListProject.SelectedItem as ProjectModel;
            if (project != null)
            {
                string sPrjUrl = string.Format(WEB_BROWSER(), m_UserId, project.Id.ToString(), m_Token);
                if (chromeBrowser == null)
                {
                    string sPrjUrlInterior = string.Format(WEB_BROWSER(), m_UserId, project.Id.ToString(), m_Token);
                    chromeBrowser = new ChromiumWebBrowser(sPrjUrlInterior);
                    chromeBrowser.Dock = DockStyle.Fill;

                    try
                    {
                        chromeBrowser.BrowserSettings.Javascript = CefState.Enabled;
                        chromeBrowser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
                    }
                    catch (Exception ex)
                    {
                    }
                    chromeBrowser.JavascriptObjectRepository.Register("external", new ExternalObjectTruss(this), isAsync: false, options: BindingOptions.DefaultBinder);

                    var isBound = chromeBrowser.JavascriptObjectRepository.IsBound("external");

                    chromeBrowser.FrameLoadEnd += ChromeBrowser_FrameLoadEnd;

                    this.mainTableLayoutPanel.Controls.Clear();
                    this.mainTableLayoutPanel.Controls.Add(chromeBrowser);

                    try
                    {
                        chromeBrowser.SetZoomLevel(dZoomLevel);
                    }
                    catch (Exception ex)
                    {
                    }
                }
                else
                {
                    chromeBrowser.LoadUrl(sPrjUrl);
                }
            }
        }

        /// <summary>
        /// Event handler that will get called when the browser is done loading a frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ChromeBrowser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            if (e.Frame.IsMain)
            {
                await chromeBrowser.EvaluateScriptAsync(@"
                let rows = [];
                    let intervalId = setInterval(function ()
                    {
                        var tables = document.getElementsByTagName('table');
                        if (tables.length > 0) {
                            for (var i = 0; i < tables.length; i++) {
                                var table = tables[i];
                                var tbody = table.getElementsByTagName('tbody')[0];

                                // Event delegation for click events on table rows
                                table.addEventListener('click', function(event) {
                                    var target = event.target;

                                    if (target.nodeName === 'TR' && target.classList.contains('not-child-row')) {
                                        var rows = Array.from(tbody.querySelectorAll('tr.not-child-row'));
                                        var index = rows.indexOf(  target  );
                                        CefSharp.BindObjectAsync('external', 'bound').then(() => window.external.cefSharpCallback(index));
                                    } else if (target.closest('tr')
                                        ) {
                                        target = target.closest('tr');
                                        if (target.classList.contains('not-child-row')) {
                                            var rows = Array.from(tbody.querySelectorAll('tr.not-child-row'));
                                            var index = rows.indexOf(  target  );
                                            CefSharp.BindObjectAsync('external', 'bound').then(() => window.external.cefSharpCallback(index));
                                        }
                                    }
                                });
                            }

                            clearInterval(intervalId)
                            return;
                        }
                    }, 500);
                ");
            }

            if(m_isFirst)
            {
                m_isFirst = false;
                this.Close();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sInstance"></param>
        /// <param name="sType"></param>
        public async void SelectJavascriptToTruss(String sInstance, String sType)
        {
            try
            {
                if (sType != null && sType.Length > 0)
                {
                    await chromeBrowser.EvaluateScriptAsync(@"window.externalService.highlightJoinery('" + sType + "');");
                }
                else if (sInstance != null && sInstance.Length > 0)
                {
                    // await chromeBrowser.EvaluateScriptAsync(@"getRoomToRoomIdsByRowIndex('" + sInstance + "');");
                }
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Trussの要素を選択する（外部から呼び出す想定）
        /// </summary>
        /// <param name="sInstance"></param>
        /// <param name="sType"></param>
        public static void SelectToTruss(String sInstance, String sType)
        {
            if (_Form == null)
            {
                return;
            }

            _Form.SelectJavascriptToTruss(sInstance, sType);
        }

        /// <summary>
        /// Trussの要素を選択する（外部から呼び出す想定）
        /// </summary>
        /// <param name="sInstance"></param>
        /// <param name="sType"></param>
        public static void ShowInstanceLabel(FamilyInstance ins)
        {
            if (_Form == null || ins == null)
            {
                return;
            }
        }

        public static bool IsShowDlg()
        {
            if (_Form == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sInstance"></param>
        /// <param name="sType"></param>
        public async void SelectIndex(int index)
        {
            try
            {
                var ret = await chromeBrowser.EvaluateScriptAsync(@"window.externalService.getJoineryIdByRowIndex(" + index + ");");
                if (ret != null)
                {
                    SelectJavascriptToRevit((string)ret.Result);
                }
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Reloadする
        /// </summary>
        public static void Reload()
        {
            if (_Form == null)
            {
                return;
            }

            ProjectModel model = (ProjectModel)_Form.cbxListProject.SelectedValue;

            if (model != null)
            {
            }

            var lstProjectModel = CommonTruss.GetProjectsToken(_Form.m_UserId, _Form.m_Token).OrderByDescending(x => x.Updated_at).ToList();
            _Form.cbxListProject.DataSource = lstProjectModel;
            _Form.cbxListProject.DisplayMember = "Name";
            _Form.cbxListProject.Refresh();

            try
            {
                int nIndex = 0;
                for (int ii = 0; ii < lstProjectModel.Count; ii++)
                {
                    if (lstProjectModel[ii].Name == model.Name)
                    {
                        nIndex = ii;
                        break;
                    }
                }
                _Form.cbxListProject.SelectedIndex = nIndex;
            }
            catch (Exception ex)
            {
            }

            _Form.chromeBrowser.Reload();
        }

        /// <summary>
        /// Revit の要素を選択する。
        /// </summary>
        /// <param name="sTrussIDs"></param>
        public void SelectJavascriptToRevit(String sTrussIDs)
        {
            if (sTrussIDs == null || sTrussIDs.Length == 0)
            {
                return;
            }

            this.m_Handler.SetData(TrussHandlerType.SelectElement, sTrussIDs);
            this.m_ExEvent.Raise();
        }

        /// <summary>
        /// Revit→Trussを実行する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevit2Truss_Click(object sender, EventArgs e)
        {
            ProjectModel pj = this.cbxListProject.SelectedItem as ProjectModel;
            if (pj != null)
            {
                this.m_Handler.SetData(TrussHandlerType.RevitToTruss, pj.Id);
                this.m_ExEvent.Raise();
            }
        }

        /// <summary>
        /// Truss→Revitを実行する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTruss2Revit_Click(object sender, EventArgs e)
        {
            ProjectModel pj = this.cbxListProject.SelectedItem as ProjectModel;
            if (pj != null)
            {
                this.m_Handler.SetData(TrussHandlerType.TrussToRevit, pj.Id);
                this.m_ExEvent.Raise();
            }
        }

        private void tbTestURL_TextChanged(object sender, EventArgs e)
        {
            chromeBrowser.LoadUrl(tbTestURL.Text);
        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {
        }

        private void buttonScaleP_Click(object sender, EventArgs e)
        {
            dZoomLevel += 1.0;
            chromeBrowser.SetZoomLevel(dZoomLevel);
        }

        private void buttonScaleM_Click(object sender, EventArgs e)
        {
            dZoomLevel -= 1.0;
            chromeBrowser.SetZoomLevel(dZoomLevel);
        }

        private void FormTrussDoorWindow_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '+')
            {
                dZoomLevel += 1.0;
                chromeBrowser.SetZoomLevel(dZoomLevel);
            }
            else if (e.KeyChar == '-')
            {
                dZoomLevel -= 1.0;
                chromeBrowser.SetZoomLevel(dZoomLevel);
            }
        }

        private void FormTrussDoorWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.DlgWidth = this.Width;
            Properties.Settings.Default.DlgHeight = this.Height;
            Properties.Settings.Default.DlgScale = this.dZoomLevel;
            Properties.Settings.Default.Save();
            _Form = null;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            Reload();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.m_Handler.SetData(TrussHandlerType.TrussIDClear, 0);
            this.m_ExEvent.Raise();
        }
    }

    public class ExternalObjectTruss
    {
        private FormTrussDoorWindow _form;

        public ExternalObjectTruss(FormTrussDoorWindow form)
        {
            _form = form;
        }

        //// Method called from JavaScript when clicking on the table row
        public void cefSharpCallback(int index)
        {
            // _form.SelectJavascriptToRevit(idTruss as String);
            _form.SelectIndex(index);
        }
    }
}