using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
<<<<<<< HEAD
using Autodesk.Revit.DB.Visual;
=======
>>>>>>> English
using CefSharp;
using CefSharp.DevTools.Database;
using DREXCreateFunctionForTrussLink.Command;
using DREXCreateFunctionForTrussLink.Data;
using DREXCreateFunctionForTrussLink.UI.DataFormSetParameterWindowDoor;
using DREXCreateFunctionForTrussLink.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
<<<<<<< HEAD
using System.Linq;
=======
using System.IO;
using System.Linq;
using System.Reflection;
>>>>>>> English
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace DREXCreateFunctionForTrussLink.UI
{
    public partial class FormSetParameterWindowDoor : System.Windows.Forms.Form
    {
        private Autodesk.Revit.DB.Document _doc;
        public List<RowDoorWindow> RetDoorWindow = new List<RowDoorWindow>();

        private String[] ListWindowPulldown = { "AW", "SW", "SSW", "WW", "AG", "SG", "SSG", "WG" };
<<<<<<< HEAD
        public String[] ListDoorPulldown = { "AD", "SD", "SSD", "PD", "WD", "FU", "SJ", "MD", "TB" };
        public String[] ListShutterPulldown = { "SS", "LS", "SHS" };
=======
        private String[] ListDoorPulldown = { "AD", "SD", "SSD", "PD", "WD", "FU", "SJ", "MD", "TB" };
        private String[] ListShutterPulldown = { "SS", "LS", "SHS" };
>>>>>>> English

        private Dictionary<string, List<string>> m_dictDoorWindowPulldown = new Dictionary<string, List<string>>();

        public FormSetParameterWindowDoor(Autodesk.Revit.DB.Document doc)
        {
            _doc = doc;

            InitializeComponent();

            dgList.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            FilteredElementCollector col = new FilteredElementCollector(doc);
            List<FamilyInstance> listFamily = col.OfCategory(BuiltInCategory.OST_GenericAnnotation).
                OfClass(typeof(FamilyInstance)).
                Cast<FamilyInstance>().Where(x => x.Symbol != null && (x.Symbol.FamilyName == "仕様 建具" || x.Symbol.FamilyName == "仕様_建具") && (x.Symbol.Name == "建具 種類" || x.Symbol.Name == "建具_種類")).ToList();

            if (listFamily.Count > 0)
            {
                listFamily.Sort((x, y) =>
                {
                    LocationPoint posX = x.Location as LocationPoint;
                    LocationPoint posY = y.Location as LocationPoint;
                    return posY.Point.Y.CompareTo(posX.Point.Y);
                });

<<<<<<< HEAD
                foreach (var family in listFamily)
=======
                foreach (var  family in listFamily)
>>>>>>> English
                {
                    Parameter prmMark = family.LookupParameter("記号");
                    Parameter prmType = family.LookupParameter("備考");
                    if (prmMark == null || prmType == null || string.IsNullOrEmpty(prmMark.AsString()) || string.IsNullOrEmpty(prmType.AsString()))
                    {
                        continue;
                    }

                    String[] listType = prmType.AsString().Split(',');
<<<<<<< HEAD
                    foreach (var typeCur in listType)
                    {
                        if (m_dictDoorWindowPulldown.ContainsKey(typeCur))
                        {
                            m_dictDoorWindowPulldown[typeCur].Add(prmMark.AsString());
                        }
                        else
                        {
                            m_dictDoorWindowPulldown.Add(typeCur, new List<string>() { prmMark.AsString() });
=======
                    foreach(var typeCur in listType)
                    {
                        if(m_dictDoorWindowPulldown.ContainsKey(typeCur))
                        {
                            m_dictDoorWindowPulldown[typeCur].Add(prmMark.AsString());
                        } else
                        {
                            m_dictDoorWindowPulldown.Add(typeCur, new List<string>(){ prmMark.AsString() });
>>>>>>> English
                        }
                    }
                }
            }

            if (m_dictDoorWindowPulldown.ContainsKey("窓"))
            {
                ListWindowPulldown = m_dictDoorWindowPulldown["窓"].ToArray();
            }

            if (m_dictDoorWindowPulldown.ContainsKey("ドア"))
            {
                ListDoorPulldown = m_dictDoorWindowPulldown["ドア"].ToArray();
            }

            if (m_dictDoorWindowPulldown.ContainsKey("シャッター"))
            {
                ListShutterPulldown = m_dictDoorWindowPulldown["シャッター"].ToArray();
            }
        }

        private bool UpdateDlg(ref List<String> listKigoBango)
        {
            bool isError = false;

            listKigoBango = new List<string>();

            // 建具凡例から、設定する必要があるドア・窓・シャッターの建具種類をリスト化する。

            int nColFamily = FormUtil.GetColumnInDataGrid(dgList, "ファミリ名");
            int nColType = FormUtil.GetColumnInDataGrid(dgList, "タイプ名");
            int nColError = FormUtil.GetColumnInDataGrid(dgList, "メッセージ");

            SortableBindingList<RowDoorWindow> listDoorWindow = dgList.DataSource as SortableBindingList<RowDoorWindow>;
            for (int ii = 0; ii < listDoorWindow.Count; ii++)
            {
                string sEvalKigo = listDoorWindow[ii].ClmTateguShurui + listDoorWindow[ii].ClmTateguBango;
                if (string.IsNullOrEmpty(listDoorWindow[ii].ClmTateguShurui) || string.IsNullOrEmpty(listDoorWindow[ii].ClmTateguBango))
                {
                    isError = true;
                    listDoorWindow[ii].ClmMessage = "建具種類・番号が入っていません。";
                }
                else if (listKigoBango.Contains(sEvalKigo))
                {
                    isError = true;
                    listDoorWindow[ii].ClmMessage = "建具種類・番号が重複しています。";
                }
                else if (listDoorWindow[ii].ClmCategory == "窓")
                {
                    if (!ListWindowPulldown.Contains(listDoorWindow[ii].ClmTateguShurui))
                    {
                        isError = true;
                        listDoorWindow[ii].ClmMessage = "建具種類は" + string.Join("、", ListWindowPulldown) + "のどれかを設定する必要があります。";
                    }
                    else
                    {
                        listKigoBango.Add(sEvalKigo);
                        listDoorWindow[ii].ClmMessage = "";
                    }
                }
                else if (listDoorWindow[ii].ClmCategory == "ドア")
                {
                    if (!ListDoorPulldown.Contains(listDoorWindow[ii].ClmTateguShurui))
                    {
                        isError = true;
                        listDoorWindow[ii].ClmMessage = "建具種類は" + string.Join("、", ListDoorPulldown) + "のどれかを設定する必要があります。";
                    }
                    else
                    {
                        listKigoBango.Add(sEvalKigo);
                        listDoorWindow[ii].ClmMessage = "";
                    }
                }
                else if (listDoorWindow[ii].ClmCategory == "シャッター")
                {
                    if (!ListShutterPulldown.Contains(listDoorWindow[ii].ClmTateguShurui))
                    {
                        isError = true;
                        listDoorWindow[ii].ClmMessage = "建具種類は" + string.Join("、", ListShutterPulldown) + "のどれかを設定する必要があります。";
                    }
                    else
                    {
                        listKigoBango.Add(sEvalKigo);
                        listDoorWindow[ii].ClmMessage = "";
                    }
                }
                else
                {
                    listKigoBango.Add(sEvalKigo);
                    listDoorWindow[ii].ClmMessage = "";
                }

                foreach (var cur in dgList.Rows)
                {
                    DataGridViewRow rowCur = (DataGridViewRow)cur;

                    DataGridViewTextBoxCell cellFamily = (DataGridViewTextBoxCell)rowCur.Cells[nColFamily];
                    DataGridViewTextBoxCell cellType = (DataGridViewTextBoxCell)rowCur.Cells[nColType];
                    DataGridViewTextBoxCell cellError = (DataGridViewTextBoxCell)rowCur.Cells[nColError];
                    CurrencyManager currencyManager1 = (CurrencyManager)BindingContext[dgList.DataSource];
                    currencyManager1.SuspendBinding();

                    bool isVisible = rowCur.Visible;
                    if (cbFilterOnlyError.Checked)
                    {
                        if (cellError.Value != null && cellError.Value.ToString().Length > 0)
                        {
                            isVisible = true;
                        }
                        else
                        {
                            isVisible = false;
                        }
                    }
                    else
                    {
                        isVisible = true;
                    }

                    try
                    {
                        rowCur.Visible = isVisible;
<<<<<<< HEAD
                    }
                    catch (Exception ex)
=======
                    } catch(Exception ex)
>>>>>>> English
                    {
                    }

                    if (cellFamily.Value.ToString() == listDoorWindow[ii].ClmFamilyName &&
                        cellType.Value.ToString() == listDoorWindow[ii].ClmTypeName
                        )
                    {
                        rowCur.Cells[nColError].Value = listDoorWindow[ii].ClmMessage;
                    }
                    currencyManager1.ResumeBinding();
                }
            }

            dgList.Refresh();

            return isError;
        }

        private void btunOK_Click(object sender, EventArgs e)
        {
            List<String> listKigoBango = new List<string>();
            bool isError = UpdateDlg(ref listKigoBango);

            SortableBindingList<RowDoorWindow> listDoorWindow = dgList.DataSource as SortableBindingList<RowDoorWindow>;
            if (isError)
            {
                MessageBox.Show("エラーがあります。メッセージ列を見てください。");
                return;
            }

            foreach (RowDoorWindow dataCur in listDoorWindow)
            {
                RetDoorWindow.Add(dataCur);
            }

<<<<<<< HEAD
=======
            // 適用と同じに変更
            btnApply_Click(sender, e);

>>>>>>> English
            for (var ii = 0; ii < dgList.Columns.Count; ii++)
            {
                switch (ii)
                {
<<<<<<< HEAD
                    case 0: // カテゴリ
                        Properties.Settings.Default.DlgWidthCol0 = dgList.Columns[ii].Width;
                        break;

                    case 1: // ファミリ
                        Properties.Settings.Default.DlgWidthCol1 = dgList.Columns[ii].Width;
                        break;

                    case 2: // タイプ
                        Properties.Settings.Default.DlgWidthCol2 = dgList.Columns[ii].Width;
                        break;

                    case 3: // 個数
                        Properties.Settings.Default.DlgWidthCol3 = dgList.Columns[ii].Width;
                        break;

                    case 4: // 部屋から
                        Properties.Settings.Default.DlgWidthCol4 = dgList.Columns[ii].Width;
                        break;

                    case 5: // 部屋へ
                        Properties.Settings.Default.DlgWidthCol5 = dgList.Columns[ii].Width;
                        break;

                    case 6: // 建具種類
                        Properties.Settings.Default.DlgWidthCol6 = dgList.Columns[ii].Width;
                        break;

                    case 7: // 建具番号
                        Properties.Settings.Default.DlgWidthCol7 = dgList.Columns[ii].Width;
                        break;

                    case 8: // メッセージ
                        Properties.Settings.Default.DlgWidthCol8 = dgList.Columns[ii].Width;
                        break;
=======
                    case 0: // チェック
                        Properties.Settings.Default.DlgWidthCol0 = dgList.Columns[ii].Width;
                        break;

                    case 1: // カテゴリ
                        Properties.Settings.Default.DlgWidthCol1 = dgList.Columns[ii].Width;
                        break;

                    case 2: // ファミリ
                        Properties.Settings.Default.DlgWidthCol2 = dgList.Columns[ii].Width;
                        break;

                    case 3: // タイプ
                        Properties.Settings.Default.DlgWidthCol3 = dgList.Columns[ii].Width;
                        break;

                    case 4: // 個数
                        Properties.Settings.Default.DlgWidthCol4 = dgList.Columns[ii].Width;
                        break;

                    case 5: // 部屋から
                        Properties.Settings.Default.DlgWidthCol5 = dgList.Columns[ii].Width;
                        break;

                    case 6: // 部屋へ
                        Properties.Settings.Default.DlgWidthCol6 = dgList.Columns[ii].Width;
                        break;

                    case 7: // 建具種類
                        Properties.Settings.Default.DlgWidthCol7 = dgList.Columns[ii].Width;
                        break;

                    case 8: // 建具番号
                        Properties.Settings.Default.DlgWidthCol8 = dgList.Columns[ii].Width;
                        break;

                    case 9: // メッセージ
                        Properties.Settings.Default.DlgWidthCol9 = dgList.Columns[ii].Width;
                        break;
>>>>>>> English
                }
            }
            Properties.Settings.Default.Save();

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private int heightRow;
        private SortableBindingList<RowDoorWindow> listDoorWindow = new SortableBindingList<RowDoorWindow>();
        private SortableBindingList<RowDoorWindow> listDoorWindowFilter = new SortableBindingList<RowDoorWindow>();

        private void FormSetParameterWindowDoor_Load(object sender, EventArgs e)
        {
            List<BuiltInCategory> listCat = new List<BuiltInCategory>();
            listCat.Add(BuiltInCategory.OST_Doors);
            listCat.Add(BuiltInCategory.OST_Windows);

            ElementMulticategoryFilter mulFilter = new ElementMulticategoryFilter(listCat);

            FilteredElementCollector col = new FilteredElementCollector(_doc);
            var listElem = col.OfClass(typeof(FamilyInstance)).WherePasses(mulFilter).Cast<FamilyInstance>().ToList();

<<<<<<< HEAD
=======
            List<RowDoorWindow> listDoorWindowCur = new List<RowDoorWindow>();
>>>>>>> English
            foreach (var insCur in listElem)
            {
                if (insCur.Symbol == null)
                {
                    continue;
                }

                RowDoorWindow datCur = new RowDoorWindow(insCur);

<<<<<<< HEAD
                //if (datCur.IsTargetFamily() == false)
                //{
                //    continue;
                //}

                RowDoorWindow doorExist = null;
                foreach (var chk in listDoorWindow)
=======
                if (datCur.IsTargetFamily() == false)
                {
                    continue;
                }

                RowDoorWindow doorExist = null;
                foreach (var chk in listDoorWindowCur)
>>>>>>> English
                {
                    if (chk.IsSameFamilySymbol(insCur.Symbol))
                    {
                        doorExist = chk;
                        break;
                    }
                }
                if (doorExist != null)
                {
                    doorExist.AddInstance(insCur);
                    continue;
                }

<<<<<<< HEAD
                listDoorWindow.Add(datCur);
            }

            dgList.DataSource = listDoorWindow;
=======
                listDoorWindowCur.Add(datCur);
            }

            listDoorWindowCur.Sort((x, y) => (x.ClmCategory + "__" + x.ClmFamilyName + "__" + x.ClmIsIgnoreType).CompareTo(y.ClmCategory + "__" + y.ClmFamilyName + "__" + y.ClmIsIgnoreType));

            dgList.DataSource = new SortableBindingList<RowDoorWindow>(listDoorWindowCur);
>>>>>>> English
            listDoorWindowFilter = listDoorWindow;

            for (var ii = 0; ii < dgList.Columns.Count; ii++)
            {
                switch (ii)
                {
<<<<<<< HEAD
                    case 0: // カテゴリ
                        dgList.Columns[ii].Width = Properties.Settings.Default.DlgWidthCol0;
                        dgList.Columns[ii].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        break;

                    case 1: // ファミリ
                        dgList.Columns[ii].Width = Properties.Settings.Default.DlgWidthCol1;
                        dgList.Columns[ii].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        break;

                    case 2: // タイプ
                        dgList.Columns[ii].Width = Properties.Settings.Default.DlgWidthCol2;
                        dgList.Columns[ii].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        break;

                    case 3: // 個数
                        dgList.Columns[ii].Width = Properties.Settings.Default.DlgWidthCol3;
                        dgList.Columns[ii].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        break;

                    case 4: // 部屋から
                        dgList.Columns[ii].Width = Properties.Settings.Default.DlgWidthCol4;
                        dgList.Columns[ii].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        break;

                    case 5: // 部屋へ
                        dgList.Columns[ii].Width = Properties.Settings.Default.DlgWidthCol5;
                        dgList.Columns[ii].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        break;

                    case 6: // 建具種類
                        dgList.Columns[ii].Width = Properties.Settings.Default.DlgWidthCol6;
                        dgList.Columns[ii].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        break;

                    case 7: // 建具番号
                        dgList.Columns[ii].Width = Properties.Settings.Default.DlgWidthCol7;
                        dgList.Columns[ii].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        break;

                    case 8: // メッセージ
                        dgList.Columns[ii].Width = Properties.Settings.Default.DlgWidthCol8;
                        dgList.Columns[ii].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        break;
                }
=======
                    case 0: // チェック
                        dgList.Columns[ii].Width = Properties.Settings.Default.DlgWidthCol0;
                        break;

                    case 1: // 分類
                        dgList.Columns[ii].Width = Properties.Settings.Default.DlgWidthCol1;
                        break;

                    case 2: // ファミリ
                        dgList.Columns[ii].Width = Properties.Settings.Default.DlgWidthCol2;
                        break;

                    case 3: // タイプ
                        dgList.Columns[ii].Width = Properties.Settings.Default.DlgWidthCol3;
                        break;

                    case 4: // 個数
                        dgList.Columns[ii].Width = Properties.Settings.Default.DlgWidthCol4;
                        break;

                    case 5: // 部屋から
                        dgList.Columns[ii].Width = Properties.Settings.Default.DlgWidthCol5;
                        break;

                    case 6: // 部屋へ
                        dgList.Columns[ii].Width = Properties.Settings.Default.DlgWidthCol6;
                        break;

                    case 7: // 建具種類
                        dgList.Columns[ii].Width = Properties.Settings.Default.DlgWidthCol7;
                        break;

                    case 8: // 建具番号
                        dgList.Columns[ii].Width = Properties.Settings.Default.DlgWidthCol8;
                        break;

                    case 9: // メッセージ
                        dgList.Columns[ii].Width = Properties.Settings.Default.DlgWidthCol9;
                        break;
                }

                dgList.Columns[ii].FillWeight = dgList.Columns[ii].Width;
                dgList.Columns[ii].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
>>>>>>> English
            }

            for (var ii = 0; ii < dgList.Rows.Count; ii++)
            {
                heightRow = (int)(dgList.Rows[ii].Height * 1.5);
                dgList.Rows[ii].Height = heightRow;
            }

            List<String> listKigoBango = new List<string>();
            UpdateDlg(ref listKigoBango);
        }

        private void dgList_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            List<String> listKigoBango = new List<string>();
            bool isError = UpdateDlg(ref listKigoBango);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            dgList.ClearSelection();

            List<String> listKigoBango = new List<string>();
            bool isError = UpdateDlg(ref listKigoBango);
        }

        private void dgList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1 || e.ColumnIndex == -1)
                return;

<<<<<<< HEAD
            if (e.ColumnIndex == 0)
            {
                bool isShutter2Door = false;
                string sText = dgList.Rows[e.RowIndex].Cells[0].Value.ToString();
=======
            int nColBunrui = FormUtil.GetColumnInDataGrid(dgList, "分類");

            if (e.ColumnIndex == 0)
            {
                bool isShutter2Door = false;
                string sText = dgList.Rows[e.RowIndex].Cells[nColBunrui].Value.ToString();
>>>>>>> English
                if (sText == "ドア")
                {
                }
                else if (sText == "シャッター")
                {
                    isShutter2Door = true;
                }
                else
                {
                    return;
                }

                var retDlg = MessageBox.Show((isShutter2Door ? "ドア" : "シャッター") + "に変更しますか？", "警告", MessageBoxButtons.YesNo);
                if (retDlg == DialogResult.Yes)
                {
                    SortableBindingList<RowDoorWindow> listDoorWindow = dgList.DataSource as SortableBindingList<RowDoorWindow>;
                    RowDoorWindow rowDoorWindow = listDoorWindow[e.RowIndex];
                    rowDoorWindow.ClmCategory = isShutter2Door ? "ドア" : "シャッター";

                    List<String> listKigoBango = new List<string>();
                    bool isError = UpdateDlg(ref listKigoBango);
                }
            }
        }

        private void dgList_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            for (var ii = 0; ii < dgList.Rows.Count; ii++)
            {
                dgList.Rows[ii].Height = heightRow;
            }
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            SortableBindingList<RowDoorWindow> listDoorWindow = dgList.DataSource as SortableBindingList<RowDoorWindow>;

            foreach (RowDoorWindow dataCur in listDoorWindow)
            {
                RetDoorWindow.Add(dataCur);
            }

<<<<<<< HEAD
=======
            TransactionGroup trg = new TransactionGroup(_doc, "建具連携適用");
            trg.Start();

>>>>>>> English
            // まずは建具　記号と種類を設定する。
            using (Transaction txCur = new Transaction(_doc, "建具パラメータ設定"))
            {
                txCur.Start();
<<<<<<< HEAD
=======

                string sBackupSharedParameter = _doc.Application.SharedParametersFilename;
                string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string sharedParamTempPath = Path.Combine(assemblyFolder, "TrussWindowSharedParameters.txt");

                List<Category> listCate = new List<Category>();
                listCate.Add(Category.GetCategory(_doc, BuiltInCategory.OST_Windows));
                listCate.Add(Category.GetCategory(_doc, BuiltInCategory.OST_Doors));
                Common.CreateParameter(_doc, sharedParamTempPath, listCate, GroupTypeId.Geometry, Define.TRUSS_TYPE, false);

>>>>>>> English
                foreach (RowDoorWindow rowCur in RetDoorWindow)
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
<<<<<<< HEAD
=======
                    Common.SetParameterInteger(rowCur.GetSymbol(), Define.TRUSS_TYPE, rowCur.ClmIsIgnoreType ? 1 : 0);
>>>>>>> English

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
<<<<<<< HEAD
        }

        private List<RowDoorWindow> lstDoorWindowHide = new List<RowDoorWindow>();

        private List<DataFilter> lstDataFilterCategory = null;

        private void btnCategory_Click(object sender, EventArgs e)
        {
            List<string> lstObject = new List<string>();

            var lstDoorWdDatagrid = dgList.DataSource as SortableBindingList<RowDoorWindow>;
            if (lstDoorWdDatagrid == null)
                return;

            lstObject = lstDoorWdDatagrid.Select(x => x.ClmCategory).ToList();
            if (lstDataFilterCategory != null)
                lstObject.AddRange(lstDataFilterCategory.Select(x => x.Name).ToList());

            var lstDataFilter = GetListDataFilter(lstObject, 0);
=======

            trg.Assimilate();

        }

        private void btnCategory_Click(object sender, EventArgs e)
        {
            int nCol = FormUtil.GetColumnInDataGrid(dgList, "分類");
            var lstDataFilter = GetListDataFilter(listDoorWindow.Select(x => x.ClmCategory).ToList(), nCol);
>>>>>>> English

            FormFilter frm = new FormFilter(lstDataFilter);
            frm.ShowDialog(new JtWindowHandle(this.Handle));
            if (frm.DialogResult == DialogResult.OK)
            {
                SortableBindingList<RowDoorWindow> listDoorWindowFilter = new SortableBindingList<RowDoorWindow>();

<<<<<<< HEAD
                lstDataFilterCategory = new List<DataFilter>();
                foreach (var item in listDoorWindow)
                {
                    if (frm._lstFilter.Contains(item.ClmCategory))
                    {
                        listDoorWindowFilter.Add(item);

                        if (lstDoorWindowHide.Contains(item))
                            lstDoorWindowHide.Remove(item);
                    }
                    else
                    {
                        if (!lstDoorWindowHide.Contains(item))
                            lstDoorWindowHide.Add(item);
                    }
                }

                lstDataFilterCategory = frm._lstSaveDataFilterNonCheck;

                if (lstDataFilterCategory?.Count > 0)
                    btnCategory.Text = btnCategory.Text + " " + "*";
                else
                {
                    if (btnCategory.Text.Contains("*"))
                        btnCategory.Text = btnCategory.Text.Replace("*", "");
=======
                foreach (var item in listDoorWindow)
                {
                    if (frm._lstFilter.Contains(item.ClmCategory))
                        listDoorWindowFilter.Add(item);
>>>>>>> English
                }

                UpdateDatagrid(listDoorWindowFilter);
            }
        }

<<<<<<< HEAD
        private List<DataFilter> lstDataFilterFmlName = null;

        private void btnFamilyName_Click(object sender, EventArgs e)
        {
            List<string> lstObject = new List<string>();

            var lstDoorWdDatagrid = dgList.DataSource as SortableBindingList<RowDoorWindow>;
            if (lstDoorWdDatagrid == null)
                return;

            lstObject = lstDoorWdDatagrid.Select(x => x.ClmFamilyName).ToList();
            if (lstDataFilterFmlName != null)
                lstObject.AddRange(lstDataFilterFmlName.Select(x => x.Name).ToList());

            var lstDataFilter = GetListDataFilter(lstObject, 1);
=======
        private void btnFamilyName_Click(object sender, EventArgs e)
        {
            int nCol = FormUtil.GetColumnInDataGrid(dgList, "ファミリ名");
            var lstDataFilter = GetListDataFilter(listDoorWindow.Select(x => x.ClmFamilyName).ToList(), nCol);
>>>>>>> English

            FormFilter frm = new FormFilter(lstDataFilter);
            frm.ShowDialog(new JtWindowHandle(this.Handle));
            if (frm.DialogResult == DialogResult.OK)
            {
                SortableBindingList<RowDoorWindow> listDoorWindowFilter = new SortableBindingList<RowDoorWindow>();

                foreach (var item in listDoorWindow)
                {
                    if (frm._lstFilter.Contains(item.ClmFamilyName))
<<<<<<< HEAD
                    {
                        listDoorWindowFilter.Add(item);

                        if (lstDoorWindowHide.Contains(item))
                            lstDoorWindowHide.Remove(item);
                    }
                    else
                    {
                        if (!lstDoorWindowHide.Contains(item))
                            lstDoorWindowHide.Add(item);
                    }
                }
                lstDataFilterFmlName = frm._lstSaveDataFilterNonCheck;

                if (lstDataFilterFmlName?.Count > 0)
                    btnFamilyName.Text = btnFamilyName.Text + " " + "*";
                else
                {
                    if (btnFamilyName.Text.Contains("*"))
                        btnFamilyName.Text = btnFamilyName.Text.Replace("*", "");
=======
                        listDoorWindowFilter.Add(item);
>>>>>>> English
                }

                UpdateDatagrid(listDoorWindowFilter);
            }
        }

<<<<<<< HEAD
        private List<DataFilter> lstDataFilterTypeName = null;

        private void btnTypeName_Click(object sender, EventArgs e)
        {
            List<string> lstObject = new List<string>();

            var lstDoorWdDatagrid = dgList.DataSource as SortableBindingList<RowDoorWindow>;
            if (lstDoorWdDatagrid == null)
                return;

            lstObject = lstDoorWdDatagrid.Select(x => x.ClmTypeName).ToList();
            if (lstDataFilterTypeName != null)
                lstObject.AddRange(lstDataFilterTypeName.Select(x => x.Name).ToList());

            var lstDataFilter = GetListDataFilter(lstObject, 2);
=======
        private void btnTypeName_Click(object sender, EventArgs e)
        {
            int nCol = FormUtil.GetColumnInDataGrid(dgList, "タイプ名");
            var lstDataFilter = GetListDataFilter(listDoorWindow.Select(x => x.ClmTypeName).ToList(), nCol);
>>>>>>> English

            FormFilter frm = new FormFilter(lstDataFilter);
            frm.ShowDialog(new JtWindowHandle(this.Handle));
            if (frm.DialogResult == DialogResult.OK)
            {
                SortableBindingList<RowDoorWindow> listDoorWindowFilter = new SortableBindingList<RowDoorWindow>();

                foreach (var item in listDoorWindow)
                {
                    if (frm._lstFilter.Contains(item.ClmTypeName))
<<<<<<< HEAD
                    {
                        listDoorWindowFilter.Add(item);
                        if (lstDoorWindowHide.Contains(item))
                            lstDoorWindowHide.Remove(item);
                    }
                    else
                    {
                        if (!lstDoorWindowHide.Contains(item))
                            lstDoorWindowHide.Add(item);
                    }
                }

                lstDataFilterTypeName = frm._lstSaveDataFilterNonCheck;

                if (lstDataFilterTypeName?.Count > 0)
                    btnTypeName.Text = btnTypeName.Text + " " + "*";
                else
                {
                    if (btnTypeName.Text.Contains("*"))
                        btnTypeName.Text = btnTypeName.Text.Replace("*", "");
=======
                        listDoorWindowFilter.Add(item);
>>>>>>> English
                }

                UpdateDatagrid(listDoorWindowFilter);
            }
        }

<<<<<<< HEAD
        private List<DataFilter> lstDataFilterRoomName = null;

        private void btnRoomName_Click(object sender, EventArgs e)
        {
            List<string> lstDorWdString = new List<string>();

            var lstDoorWdDatagrid = dgList.DataSource as SortableBindingList<RowDoorWindow>;
            if (lstDoorWdDatagrid == null)
                return;

            lstDorWdString.AddRange(lstDoorWdDatagrid.Select(x => x.ClmFromRoomName).ToList());
            lstDorWdString.AddRange(lstDoorWdDatagrid.Select(x => x.ClmToRoomName).ToList());
            if (lstDataFilterRoomName != null)
                lstDorWdString.AddRange(lstDataFilterRoomName.Select(x => x.Name).ToList());
=======
        private void btnRoomName_Click(object sender, EventArgs e)
        {
            List<string> lstDorWdString = new List<string>();
            lstDorWdString.AddRange(listDoorWindow.Select(x => x.ClmFromRoomName).ToList());
            lstDorWdString.AddRange(listDoorWindow.Select(x => x.ClmToRoomName).ToList());

>>>>>>> English
            var lstDataFilter = GetListDataFilterColumnRoom(lstDorWdString);

            FormFilter frm = new FormFilter(lstDataFilter);
            frm.ShowDialog(new JtWindowHandle(this.Handle));
            if (frm.DialogResult == DialogResult.OK)
            {
                SortableBindingList<RowDoorWindow> listDoorWindowFilter = new SortableBindingList<RowDoorWindow>();

                foreach (var item in listDoorWindow)
                {
<<<<<<< HEAD
                    if (lstDoorWindowHide.Contains(item))
                        continue;

                    if (frm._lstFilter.Contains(item.ClmFromRoomName) || frm._lstFilter.Contains(item.ClmToRoomName))
                        listDoorWindowFilter.Add(item);
                }
                lstDataFilterRoomName = frm._lstSaveDataFilterNonCheck;

                if (lstDataFilterRoomName?.Count > 0)
                    btnRoomName.Text = btnRoomName.Text + " " + "*";
                else
                {
                    if (btnRoomName.Text.Contains("*"))
                        btnRoomName.Text = btnRoomName.Text.Replace("*", "");
                }
=======
                    if (frm._lstFilter.Contains(item.ClmFromRoomName) || frm._lstFilter.Contains(item.ClmToRoomName))
                        listDoorWindowFilter.Add(item);
                }
>>>>>>> English

                UpdateDatagrid(listDoorWindowFilter);
            }
        }

<<<<<<< HEAD
        private List<DataFilter> lstDataFilterTategu = null;

        private void btnTateguShurui_Click(object sender, EventArgs e)
        {
            List<string> lstObject = new List<string>();
            var lstDoorWdDatagrid = dgList.DataSource as SortableBindingList<RowDoorWindow>;
            if (lstDoorWdDatagrid == null)
                return;
            lstObject = lstDoorWdDatagrid.Select(x => x.ClmTateguShurui).ToList();
            if (lstDataFilterTategu != null)
                lstObject.AddRange(lstDataFilterTategu.Select(x => x.Name).ToList());

            var lstDataFilter = GetListDataFilter(lstObject, 6);
=======
        private void btnTateguShurui_Click(object sender, EventArgs e)
        {
            int nCol = FormUtil.GetColumnInDataGrid(dgList, "建具種類");
            var lstDataFilter = GetListDataFilter(listDoorWindow.Select(x => x.ClmTateguShurui).ToList(), nCol);
>>>>>>> English

            FormFilter frm = new FormFilter(lstDataFilter);
            frm.ShowDialog(new JtWindowHandle(this.Handle));
            if (frm.DialogResult == DialogResult.OK)
            {
                SortableBindingList<RowDoorWindow> listDoorWindowFilter = new SortableBindingList<RowDoorWindow>();

                foreach (var item in listDoorWindow)
                {
                    string val = string.Empty;
                    if (item.ClmTateguShurui != null)
                        val = item.ClmTateguShurui;

                    if (frm._lstFilter.Contains(val))
<<<<<<< HEAD
                    {
                        listDoorWindowFilter.Add(item);
                        if (lstDoorWindowHide.Contains(item))
                            lstDoorWindowHide.Remove(item);
                    }
                    else
                    {
                        if (!lstDoorWindowHide.Contains(item))
                            lstDoorWindowHide.Add(item);
                    }
                }
                lstDataFilterTategu = frm._lstSaveDataFilterNonCheck;

                if (lstDataFilterTategu?.Count > 0)
                    btnTateguShurui.Text = btnTateguShurui.Text + " " + "*";
                else
                {
                    if (btnTateguShurui.Text.Contains("*"))
                        btnTateguShurui.Text = btnTateguShurui.Text.Replace("*", "");
=======
                        listDoorWindowFilter.Add(item);
>>>>>>> English
                }

                UpdateDatagrid(listDoorWindowFilter);
            }
        }

        private void UpdateDatagrid(SortableBindingList<RowDoorWindow> listDoorWindow)
        {
            dgList.DataSource = listDoorWindow;
            for (var ii = 0; ii < dgList.Rows.Count; ii++)
            {
                dgList.Rows[ii].Height = heightRow;
            }

            listDoorWindowFilter = listDoorWindow;
        }

        private List<DataFilter> GetListDataFilter(List<string> lstDoorWdString, int indexCol)
        {
            List<DataFilter> lstDataFilter = new List<DataFilter>();
            List<string> lstName = new List<string>();

            for (int i = 0; i < dgList.RowCount; i++)
            {
                var val = dgList.Rows[i].Cells[indexCol].Value;
                if (val == null)
                {
                    var str = string.Empty;
                    lstName.Add(str);
                    continue;
                }

                if (!lstName.Contains(val.ToString()) && !string.IsNullOrEmpty(val.ToString()))
                    lstName.Add(val.ToString());
            }

            foreach (var item in lstDoorWdString)
            {
                Data.DataFilter dataFilter = new Data.DataFilter();

                if (item == null)
                    dataFilter.Name = string.Empty;
                else
                    dataFilter.Name = item;

                if (lstName.Contains(dataFilter.Name))
                    dataFilter.IsDisplay = true;
                else
                    dataFilter.IsDisplay = false;

                if (!lstDataFilter.Select(x => x.Name).Contains(dataFilter.Name))
                    lstDataFilter.Add(dataFilter);
            }

            return lstDataFilter;
        }

        private List<DataFilter> GetListDataFilterColumnRoom(List<string> lstDoorWdString)
        {
            List<DataFilter> lstDataFilter = new List<DataFilter>();
            List<string> lstName = new List<string>();

<<<<<<< HEAD
            for (int i = 0; i < dgList.RowCount; i++)
            {
                var val4 = dgList.Rows[i].Cells[4].Value;
=======
            int nColFromRoom = FormUtil.GetColumnInDataGrid(dgList, "部屋から");
            int nColFromTo = FormUtil.GetColumnInDataGrid(dgList, "部屋へ");
            for (int i = 0; i < dgList.RowCount; i++)
            {
                var val4 = dgList.Rows[i].Cells[nColFromRoom].Value;
>>>>>>> English
                if (val4 == null)
                    continue;
                if (!lstName.Contains(val4.ToString()))
                    lstName.Add(val4.ToString());

<<<<<<< HEAD
                var val5 = dgList.Rows[i].Cells[5].Value;
=======
                var val5 = dgList.Rows[i].Cells[nColFromTo].Value;
>>>>>>> English
                if (val5 == null)
                    continue;
                if (!lstName.Contains(val5.ToString()))
                    lstName.Add(val5.ToString());
            }

            foreach (var item in lstDoorWdString)
            {
                Data.DataFilter dataFilter = new Data.DataFilter();

                dataFilter.Name = item;

                if (lstName.Contains(item))
                    dataFilter.IsDisplay = true;
                else
                    dataFilter.IsDisplay = false;

                if (!lstDataFilter.Select(x => x.Name).Contains(item))
                    lstDataFilter.Add(dataFilter);
            }

            return lstDataFilter;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            SortableBindingList<RowDoorWindow> listNewDoorWindowFilter = new SortableBindingList<RowDoorWindow>();
            foreach (var item in listDoorWindowFilter)
            {
                if ((item.ClmCategory != null && item.ClmCategory.Contains(textBox1.Text.Trim())) ||
                  (item.ClmTypeName != null && item.ClmTypeName.Contains(textBox1.Text.Trim())) ||
                  (item.ClmFamilyName != null && item.ClmFamilyName.Contains(textBox1.Text.Trim())) ||
                  (item.ClmFromRoomName != null && item.ClmFromRoomName.Contains(textBox1.Text.Trim())) ||
                  (item.ClmToRoomName != null && item.ClmToRoomName.Contains(textBox1.Text.Trim())) ||
                  (item.ClmTateguShurui != null && item.ClmTateguShurui.Contains(textBox1.Text.Trim())))
                    listNewDoorWindowFilter.Add(item);
            }

            dgList.DataSource = listNewDoorWindowFilter;
            for (var ii = 0; ii < dgList.Rows.Count; ii++)
            {
                dgList.Rows[ii].Height = heightRow;
            }
        }
    }
}