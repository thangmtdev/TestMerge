using Autodesk.Revit.DB;
using DREXFittingTool.Data;
using DREXFittingTool.Utils;
using System.Collections.Generic;
using System.Linq;

namespace DREXFittingTool.Services
{
    public class FittingService
    {
        private Document m_doc;
        private List<FamilyInstance> m_girders;
        private FamilySymbol m_firstSymbol;
        private List<MappingData> m_MappingDatas;

        public FittingService(Document doc, List<FamilyInstance> girders, FamilySymbol firstSymbol, List<MappingData> mappingData)
        {
            m_doc = doc;
            m_girders = girders;
            m_firstSymbol = firstSymbol;
            m_MappingDatas = mappingData;
        }

        /// <summary>
        /// Get fitting data
        /// </summary>
        /// <returns></returns>
        public List<FittingData> GetFittingData()
        {
            List<FittingData> retVal = new List<FittingData>();

            foreach (FamilyInstance girder in m_girders)
            {
                FamilySymbol fSymbol = girder.Symbol;
                if (fSymbol == null)
                    continue;

                //Validate parameter
                double centerWidth = 0.0;
                Parameter centerWidthParam = fSymbol.LookupParameter("幅");
                if (centerWidthParam != null && centerWidthParam.StorageType == StorageType.Double)
                    centerWidth = UnitUtils.ConvertFromInternalUnits(centerWidthParam.AsDouble(), DisplayUnitType.DUT_MILLIMETERS);

                double centerHeightVal = 0.0;
                Parameter centerHeightParam = fSymbol.LookupParameter("高さ");
                if (centerHeightParam != null && centerHeightParam.StorageType == StorageType.Double)
                    centerHeightVal = UnitUtils.ConvertFromInternalUnits(centerHeightParam.AsDouble(), DisplayUnitType.DUT_MILLIMETERS);

                double centerWebThickness = 0.0;
                Parameter centerWebThicknessParam = fSymbol.LookupParameter("ウェブ厚さ");
                if (centerWebThicknessParam != null && centerWebThicknessParam.StorageType == StorageType.Double)
                    centerWebThickness = UnitUtils.ConvertFromInternalUnits(centerWebThicknessParam.AsDouble(), DisplayUnitType.DUT_MILLIMETERS);

                double centerFThickness = 0.0;
                Parameter centerFThicknessParam = fSymbol.LookupParameter("フランジ厚さ");
                if (centerFThicknessParam != null && centerFThicknessParam.StorageType == StorageType.Double)
                    centerFThickness = UnitUtils.ConvertFromInternalUnits(centerFThicknessParam.AsDouble(), DisplayUnitType.DUT_MILLIMETERS);

                string centerFMaterial = "";
                Parameter centerFMaterialParam = fSymbol.LookupParameter("中央_フランジ_マテリアル");
                if (centerFMaterialParam != null && centerFMaterialParam.StorageType == StorageType.ElementId)
                {
                    var matId = centerFMaterialParam.AsElementId();
                    if (matId != ElementId.InvalidElementId)
                    {
                        var material = m_doc.GetElement(matId);
                        if (material != null)
                            centerFMaterial = material.Name;
                    }
                }

                string markFitting = "";
                Parameter markFittingParam = fSymbol.LookupParameter("始端_継手符号");
                if (markFittingParam != null && markFittingParam.StorageType == StorageType.String)
                    markFitting = markFittingParam.AsString();

                if (markFitting == "")
                    markFitting = "H" + "-" + centerHeightVal + "x" + centerWidth + "x" + centerWebThickness + "x" + centerFThickness;

                MappingData data = m_MappingDatas.FirstOrDefault(x => Common.IsEqual(Common.ConvertToDouble(x.m_centerHeight.m_value), centerHeightVal) &&
                                                                      Common.IsEqual(Common.ConvertToDouble(x.m_centerWidh.m_value), centerWidth) &&
                                                                      Common.IsEqual(Common.ConvertToDouble(x.m_webThickness.m_value), centerWebThickness) &&
                                                                      Common.IsEqual(Common.ConvertToDouble(x.m_FThickness.m_value), centerFThickness) &&
                                                                      centerFMaterial == x.m_FMaterial.m_value);

                if (data != null)
                {
                    FamilySymbol fsymbol = new FilteredElementCollector(m_doc).OfClass(typeof(FamilySymbol))
                                                                              .Cast<FamilySymbol>()
                                                                              .FirstOrDefault(x => x.Family.Name == Define.FamilyName &&
                                                                                              x.Name == markFitting);

                    if (fsymbol == null)
                        fsymbol = m_firstSymbol.Duplicate(markFitting) as FamilySymbol;

                    if (fsymbol != null)
                    {
                        foreach (var item in data.m_dictParameter)
                        {
                            var param = fsymbol.LookupParameter(item.Key);
                            if (param != null && param.StorageType == StorageType.String)
                                param.Set(item.Value);
                        }

                        //Get parameter mark
                        Parameter paramMark = fsymbol.LookupParameter("d_継手符号");
                        if (paramMark != null && paramMark.StorageType == StorageType.String)
                            paramMark.Set(markFitting);

                        if (!retVal.Any(x => x.m_symbol.Name == fsymbol.Name))
                        {
                            FittingData fData = new FittingData(fsymbol, markFitting, centerHeightVal, centerWidth);
                            retVal.Add(fData);
                        }
                    }
                }
            }

            return retVal;
        }

        /// <summary>
        /// Get symbol to place to view drafting
        /// </summary>
        /// <param name="fittingDatas"></param>
        /// <returns></returns>
        public Dictionary<ViewDrafting, List<FittingData>> GetSymbolToPlaceToViewDrafting(ViewDrafting viewDft, ViewFamilyType vd, List<FittingData> fittingDatas, int row, int col, string orgName)
        {
            Dictionary<ViewDrafting, List<FittingData>> retVal = new Dictionary<ViewDrafting, List<FittingData>>();

            int cost = 0;
            int totalCost = row * col;
            for (int i = 0; i < fittingDatas.Count; i++)
            {
                cost += 1;
                if (cost <= totalCost)
                {
                    if (!retVal.ContainsKey(viewDft))
                    {
                        List<FittingData> fits = new List<FittingData>();
                        fits.Add(fittingDatas[i]);
                        retVal.Add(viewDft, fits);
                    }
                    else
                        retVal[viewDft].Add(fittingDatas[i]);
                }
                else
                {
                    cost = 1;
                    var newDraftView = ViewDrafting.Create(m_doc, vd.Id);
                    newDraftView.Name = Common.GetViewDraftingName(m_doc, orgName);
                    viewDft = newDraftView;
                    if (!retVal.ContainsKey(viewDft))
                    {
                        List<FittingData> fits = new List<FittingData>();
                        fits.Add(fittingDatas[i]);
                        retVal.Add(viewDft, fits);
                    }
                    else
                        retVal[viewDft].Add(fittingDatas[i]);
                }
            }

            return retVal;
        }

        /// <summary>
        /// Place fitting
        /// </summary>
        /// <param name="dictFitting"></param>
        /// <param name="max_row"></param>
        /// <param name="max_col"></param>
        public void PlaceFittingToDraftingView(Dictionary<ViewDrafting, List<FittingData>> dictFitting, int max_row, int max_col, double width_one, double height_one, XYZ orgmaxPt)
        {
            foreach (var item in dictFitting)
            {
                ViewDrafting vDraft = item.Key;
                item.Key.Scale = 10;

                double rangeX = orgmaxPt.X;
                double rangeY = orgmaxPt.Y;

                List<Cell> all_cells = new List<Cell>();

                for (int j = 1; j <= max_row; j++)
                {
                    for (int i = 1; i <= max_col; i++)
                    {
                        Cell cell = new Cell(j, i);
                        all_cells.Add(cell);
                    }
                }

                int index = 0;
                int rowCheck = 0;
                foreach (FittingData fData in item.Value)
                {
                    int col = 1;
                    int row = 1;

                    int row_index = -1;
                    int col_index = -1;

                    bool isTurnOnTitle = false;
                    //Get row and col position
                    if (index == 0)
                        isTurnOnTitle = true;

                    GetPosition(all_cells, row, max_row, col, max_col, ref row_index, ref col_index);

                    if (row_index > rowCheck)
                        isTurnOnTitle = true;

                    rowCheck = row_index;

                    if (row_index == -1 || col_index == -1)
                    {
                        continue;
                    }

                    if (row_index > max_row)
                        continue;

                    if (col_index > max_col)
                        continue;

                    SetCellToDone(row_index, row_index + row - 1, col_index, col_index + col - 1, all_cells);

                    //Calculate location
                    double dHeight = (row_index - 1) * height_one + (row * height_one) / 2;
                    double dWidth = (col_index - 1) * width_one + (col * width_one) / 2;

                    XYZ center = new XYZ(rangeX + dWidth, rangeY - dHeight, 0);
                    FamilyInstance newinstance = m_doc.Create.NewFamilyInstance(center, fData.m_symbol, item.Key);

                    if (newinstance != null)
                    {
                        if (isTurnOnTitle)
                        {
                            Common.SetTitleParameter(newinstance, 1);
                            m_doc.Regenerate();
                            double titleLength = Common.GetTitleLength(newinstance);
                            //Move to left
                            var locPoint = newinstance.Location as LocationPoint;
                            var ptMove = locPoint.Point;
                            var ptToMove = ptMove + titleLength * vDraft.RightDirection.Negate();
                            ElementTransformUtils.MoveElement(m_doc, newinstance.Id, ptToMove - ptMove);
                        }
                        else
                        {
                            Common.SetTitleParameter(newinstance, 0);
                        }
                    }

                    index++;
                }
            }
        }

        /// <summary>
        /// Update fitting
        /// </summary>
        /// <param name="dictFitting"></param>
        /// <param name="max_row"></param>
        /// <param name="max_col"></param>
        public void UpdateFittingToDraftingView(Dictionary<ViewDrafting, List<FittingData>> dictFitting, int max_row, int max_col, double width_one, double height_one, XYZ orgmaxPt)
        {
            foreach (var item in dictFitting)
            {
                ViewDrafting vDraft = item.Key;
                item.Key.Scale = 10;

                List<FamilyInstance> fittingInstances = new FilteredElementCollector(m_doc, vDraft.Id).OfClass(typeof(FamilyInstance))
                                                                                                      .Cast<FamilyInstance>()
                                                                                                      .Where(x => x.Symbol != null && x.Symbol.Family.Name == Define.FamilyName)
                                                                                                      .ToList();

                if (fittingInstances.Count > 0)
                {
                    List<XYZ> lstPoint = new List<XYZ>();
                    foreach (var fittingInstance in fittingInstances)
                    {
                        BoundingBoxXYZ box = fittingInstance.get_BoundingBox(vDraft);
                        lstPoint.Add(box.Max);
                        lstPoint.Add(box.Min);
                    }

                    double maxX = lstPoint.Max(x => x.X);
                    double maxY = lstPoint.Max(x => x.Y);
                    double maxZ = lstPoint.Max(x => x.Z);
                    double minX = lstPoint.Min(x => x.X);
                    double minY = lstPoint.Min(x => x.Y);
                    double minZ = lstPoint.Min(x => x.Z);

                    var minPt = new XYZ(minX, minY, minZ);
                    var maxPt = new XYZ(maxX, maxY, maxZ);

                    orgmaxPt = new XYZ(orgmaxPt.X, minPt.Y - (100 / 304.8), orgmaxPt.Z);
                }

                double rangeX = orgmaxPt.X;
                double rangeY = orgmaxPt.Y;

                List<Cell> all_cells = new List<Cell>();

                for (int j = 1; j <= max_row; j++)
                {
                    for (int i = 1; i <= max_col; i++)
                    {
                        Cell cell = new Cell(j, i);
                        all_cells.Add(cell);
                    }
                }

                int index = 0;
                int rowCheck = 0;
                foreach (FittingData fData in item.Value)
                {
                    int col = 1;
                    int row = 1;

                    int row_index = -1;
                    int col_index = -1;

                    bool isTurnOnTitle = false;
                    //Get row and col position
                    if (index == 0)
                        isTurnOnTitle = true;

                    GetPosition(all_cells, row, max_row, col, max_col, ref row_index, ref col_index);

                    if (row_index > rowCheck)
                        isTurnOnTitle = true;

                    rowCheck = row_index;

                    if (row_index == -1 || col_index == -1)
                    {
                        continue;
                    }

                    if (row_index > max_row)
                        continue;

                    if (col_index > max_col)
                        continue;

                    SetCellToDone(row_index, row_index + row - 1, col_index, col_index + col - 1, all_cells);

                    //Calculate location
                    double dHeight = (row_index - 1) * height_one + (row * height_one) / 2;
                    double dWidth = (col_index - 1) * width_one + (col * width_one) / 2;

                    XYZ center = new XYZ(rangeX + dWidth, rangeY - dHeight, 0);
                    FamilyInstance newinstance = m_doc.Create.NewFamilyInstance(center, fData.m_symbol, item.Key);

                    if (newinstance != null)
                    {
                        if (isTurnOnTitle)
                        {
                            Common.SetTitleParameter(newinstance, 1);
                            m_doc.Regenerate();
                            double titleLength = Common.GetTitleLength(newinstance);
                            //Move to left
                            var locPoint = newinstance.Location as LocationPoint;
                            var ptMove = locPoint.Point;
                            var ptToMove = ptMove + titleLength * vDraft.RightDirection.Negate();
                            ElementTransformUtils.MoveElement(m_doc, newinstance.Id, ptToMove - ptMove);
                        }
                        else
                        {
                            Common.SetTitleParameter(newinstance, 0);
                        }
                    }

                    index++;
                }
            }
        }

        /// <summary>
        /// Get cell position
        /// </summary>
        /// <param name="items"></param>
        /// <param name="countRow"></param>
        /// <param name="max_row"></param>
        /// <param name="countCol"></param>
        /// <param name="maxCol"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        public void GetPosition(List<Cell> items, int countRow, int max_row, int countCol, int maxCol, ref int row, ref int col)
        {
            int colBegin = 1;
            for (int i = 0; i < items.Count; i++)
            {
                var cell = items[i];

                if (cell.m_DONE == false)
                {
                    int r = cell.m_Row + countRow - 1;
                    int c = cell.m_Col + countCol - 1;

                    if (c > maxCol)
                        return;

                    if (r <= max_row)
                    {
                        //Next cell at next col
                        bool flagCol = true;
                        if (countCol > 1)
                        {
                            for (int j = 1; j < countCol; j++)
                            {
                                var index_cell_next_col = cell.m_Col + j;

                                List<Cell> cells = items.Where(item => item.m_Col == index_cell_next_col && item.m_Row == cell.m_Row && item.m_DONE == true).ToList();
                                if (cells.Count > 0)
                                {
                                    flagCol = false;
                                    break;
                                }
                            }
                        }

                        //Next cell at next col
                        bool flagRow = true;
                        if (countRow > 1)
                        {
                            for (int j = 1; j < countRow; j++)
                            {
                                var index_cell_next_Row = cell.m_Row + j;

                                List<Cell> cells = items.Where(item => item.m_Row == index_cell_next_Row && item.m_Col == cell.m_Col && item.m_DONE == true).ToList();
                                if (cells.Count > 0)
                                {
                                    flagRow = false;
                                    break;
                                }
                            }
                        }

                        if (flagCol == true && flagRow == true)
                        {
                            row = cell.m_Row;
                            col = cell.m_Col;
                            break;
                        }
                    }
                    else
                    {
                        colBegin++;
                        if (colBegin > maxCol)
                            return;
                    }
                }
                else
                {
                    if (cell.m_Row == max_row)
                        colBegin++;
                }
            }
        }

        /// <summary>
        /// Set cell to done
        /// </summary>
        /// <param name="fromRow"></param>
        /// <param name="toRow"></param>
        /// <param name="fromCol"></param>
        /// <param name="toCol"></param>
        /// <param name="items"></param>
        public void SetCellToDone(int fromRow, int toRow, int fromCol, int toCol, List<Cell> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var cell = items[i];

                if (cell.m_Row >= fromRow && cell.m_Row <= toRow && cell.m_Col >= fromCol && cell.m_Col <= toCol)
                {
                    if (cell.m_DONE == false)
                    {
                        cell.m_DONE = true;
                    }
                }
            }
        }
    }

    public class Cell
    {
        public int m_Row = -1;
        public int m_Col = -1;
        public bool m_DONE = false;

        public Cell(int row, int col)
        {
            m_Row = row;
            m_Col = col;
        }
    }
}