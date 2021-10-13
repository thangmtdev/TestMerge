using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using DREXFittingTool.Data;

namespace DREXFittingTool.Utils
{
    internal class Common
    {
        private const double TOLERANCE = 0.0000000001;

        public const double DUT_MILLIMETER = 304.8;

        public const double TITLE_LENGTH = 450 / DUT_MILLIMETER;

        public static void GetMaxRowAndCol(Document doc,
                                           ViewDrafting draftView,
                                           FittingData data,
                                           double maxW,
                                           double maxH,
                                           ref int maxRow,
                                           ref int maxCol,
                                           ref double width_one,
                                           ref double height_one,
                                           ref XYZ orgmaxPt)
        {
            //Get max row and max col
            XYZ point = new XYZ(0, 0, 0);

            FamilyInstance checkInstance = doc.Create.NewFamilyInstance(point, data.m_symbol, draftView);
            var paramCheckTitle = checkInstance.LookupParameter("枠_タイトル有");
            paramCheckTitle.Set(1);

            doc.Regenerate();

            var paramTitle = checkInstance.LookupParameter("制御_枠タイトル幅");
            double titleVal = paramTitle.AsDouble();
            BoundingBoxXYZ box = checkInstance.get_BoundingBox(draftView);
            width_one = 0.0;
            height_one = 0.0;

            GetFittingWithHeight(data.m_symbol, out width_one, out height_one);
            orgmaxPt = box.Max;

            maxW = maxW - titleVal;
            double valC = Math.Round(maxW / width_one);
            double valH = Math.Round(maxH / height_one);

            int valLastC = 0;
            if (Common.IsEqual(valC, maxW / width_one))
                valLastC = Convert.ToInt32(valC);
            else
                valLastC = (int)(maxW / width_one);

            int valLastR = 0;
            if (Common.IsEqual(valH, maxH / height_one))
                valLastR = Convert.ToInt32(valH);
            else
                valLastR = (int)(maxH / height_one);

            maxCol = valLastC;
            maxRow = valLastR;

            doc.Delete(checkInstance.Id);
        }

        /// <summary>
        /// Get fitting width and height
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="width_one"></param>
        /// <param name="height_one"></param>
        private static void GetFittingWithHeight(FamilySymbol symbol, out double width_one, out double height_one)
        {
            width_one = 0.0;
            height_one = 0.0;

            var paramW = symbol.LookupParameter("枠_幅");
            var paramH = symbol.LookupParameter("枠_高さ");

            if (paramW != null)
                width_one = paramW.AsDouble();

            if (paramH != null)
                height_one = paramH.AsDouble();
        }

        /// <summary>
        /// <para>Determine if 2 float numbers are equal within a given tolerance</para>
        /// <para>2値が誤差の範囲内で一致するかどうか判断する</para>
        /// </summary>
        /// <param name="first">比較する数値</param>
        /// <param name="second">比較する数値</param>
        /// <param name="tolerance">誤差</param>
        /// <returns></returns>
        public static bool IsEqual(double first, double second, double tolerance = TOLERANCE)
        {
            double result = Math.Abs(first - second);
            return result < tolerance;
        }

        /// <summary>
        /// <para>Determine if 2 vectors or points are euqal within a given tolerance</para>
        /// <para>2つの座標またはベクトルが、各軸ごとに誤差の範囲内で一致するかどうか判断する</para>
        /// </summary>
        /// <param name="first">比較するXYZオブジェクト</param>
        /// <param name="second">比較するXYZオブジェクト</param>
        /// <param name="tolerance">誤差</param>
        /// <returns></returns>
        public static bool IsEqual(XYZ first, XYZ second, double tolerance = TOLERANCE)
        {
            return IsEqual(first.X, second.X)
                && IsEqual(first.Y, second.Y)
                && IsEqual(first.Z, second.Z);
        }

        /// <summary>
        /// Show exception information
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="title"></param>
        public static void ShowException(Exception ex, string title = "エラー")
        {
            string message = ex.Message + "\n" + ex.StackTrace.ToString();
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Show exception information
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="title"></param>
        public static void ShowError(string content, string title = "エラー")
        {
            MessageBox.Show(content, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Show warning
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="title"></param>
        public static void ShowWarning(string content, string title = "警告")
        {
            MessageBox.Show(content, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Show Information
        /// </summary>
        /// <param name="content"></param>
        /// <param name="title"></param>
        public static void ShowInfor(string content, string title = "情報")
        {
            MessageBox.Show(content, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Validate framing parameter
        /// </summary>
        /// <param name="fi"></param>
        /// <returns></returns>
        public static bool ValidateFramingParameter(FamilyInstance fi)
        {
            var fSymbol = fi.Symbol;

            var paramCenter = fSymbol.LookupParameter("中央_断面形状記号");
            if (paramCenter != null && paramCenter.StorageType == StorageType.String)
            {
                string val = paramCenter.AsString();
                if (val == "H")
                    return true;
            }

            var paramStart = fSymbol.LookupParameter("始端_断面形状記号");
            if (paramStart != null && paramStart.StorageType == StorageType.String)
            {
                string val = paramStart.AsString();
                if (val == "H")
                    return true;
            }

            var paramEnd = fSymbol.LookupParameter("終端_断面形状記号");
            if (paramEnd != null && paramEnd.StorageType == StorageType.String)
            {
                string val = paramEnd.AsString();
                if (val == "H")
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Convert to double
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double ConvertToDouble(string value)
        {
            double retVal = 0.0;
            if (double.TryParse(value, out retVal))
                return retVal;

            return 0.0;
        }

        /// <summary>
        /// Get drafting view name
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static string GetViewDraftingName(Document doc, string nameOrg)
        {
            int index = 1;
            string name = nameOrg + "-" + index;

            var lstViewDraft = new FilteredElementCollector(doc).OfClass(typeof(ViewDrafting)).Cast<ViewDrafting>().ToList();

            while (lstViewDraft.Any(x => x.Name == name))
            {
                name = nameOrg + "-" + index;
                index++;
            }

            return name;
        }

        /// <summary>
        /// Get view name original
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static string GetViewNameOriginal(Document doc)
        {
            int index = 1;
            string name = Define.ViewName + index;

            var lstViewDraft = new FilteredElementCollector(doc).OfClass(typeof(ViewDrafting)).Cast<ViewDrafting>().Where(x => x.Name.Contains(Define.ViewName)).ToList();

            while (lstViewDraft.Any(x => GetActualName(x.Name) == name))
            {
                name = Define.ViewName + index;
                index++;
            }

            return name;
        }

        /// <summary>
        /// Get actual name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetActualName(string name)
        {
            string[] split = name.Split('-');

            return split[0];
        }

        /// <summary>
        /// Get bounding box width height
        /// </summary>
        /// <param name="bbBox"></param>
        /// <param name="view"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void GetBoundingBoxSize(BoundingBoxXYZ bbBox, Autodesk.Revit.DB.View view, out double width, out double height)
        {
            XYZ minPT = bbBox.Min;
            XYZ maxPT = bbBox.Max;

            width = 0;
            height = 0;

            if (view.ViewDirection.IsAlmostEqualTo(XYZ.BasisX) || view.ViewDirection.IsAlmostEqualTo(XYZ.BasisX.Negate()))
            {
                width = Math.Abs(maxPT.Y - minPT.Y);
                height = Math.Abs(maxPT.Z - minPT.Z);
            }
            else if (view.ViewDirection.IsAlmostEqualTo(XYZ.BasisY) || view.ViewDirection.IsAlmostEqualTo(XYZ.BasisY.Negate()))
            {
                width = Math.Abs(maxPT.X - minPT.X);
                height = Math.Abs(maxPT.Z - minPT.Z);
            }
            else if (view.ViewDirection.IsAlmostEqualTo(XYZ.BasisZ) || view.ViewDirection.IsAlmostEqualTo(XYZ.BasisZ.Negate()))
            {
                width = Math.Abs(maxPT.X - minPT.X);
                height = Math.Abs(maxPT.Y - minPT.Y);
            }
            else
            {
                XYZ p1 = new XYZ(maxPT.X, maxPT.Y, minPT.Z);
                width = Line.CreateBound(minPT, p1).Length;
                height = Line.CreateBound(maxPT, p1).Length;
            }
        }

        /// <summary>
        /// Set title parameter
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="val"></param>
        public static void SetTitleParameter(FamilyInstance instance, int val)
        {
            var param = instance.LookupParameter("枠_タイトル有");
            if (param != null && param.StorageType == StorageType.Integer)
                param.Set(val);
        }

        /// <summary>
        /// Get center instance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static XYZ GetCenterPointInstance(FamilyInstance instance)
        {
            var locPoint = instance.Location as LocationPoint;
            return locPoint.Point;
        }

        /// <summary>
        /// Get owner view name
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static string GetOwnerViewName(Document doc, FamilyInstance instance)
        {
            var viewId = instance.OwnerViewId;
            var view = doc.GetElement(viewId);
            if (view != null)
                return view.Name;

            return "";
        }

        /// <summary>
        /// Validate size fitting
        /// </summary>
        /// <param name="data"></param>
        /// <param name="maxW"></param>
        /// <param name="maxH"></param>
        /// <returns></returns>
        public static bool ValidateSizeFitting(FittingData data, double maxW, double maxH)
        {
            var paramW = data.m_symbol.LookupParameter("枠_幅");
            var paramH = data.m_symbol.LookupParameter("枠_高さ");

            if (paramW != null && paramH != null)
            {
                if (paramW.AsDouble() + TITLE_LENGTH < maxW || paramH.AsDouble() + TITLE_LENGTH < maxH)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get title length
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static double GetTitleLength(FamilyInstance instance)
        {
            var paramTitle = instance.LookupParameter("制御_枠タイトル幅");
            return paramTitle.AsDouble(); ;
        }

        /// <summary>
        /// Get data mapping
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>
        public static List<MappingData> GetDataMapping(string[] fileData)
        {
            List<MappingData> retVal = new List<MappingData>();

            //Get list Parameter
            string[] paramSplit = fileData[0].Split(',');
            List<string> paramNames = new List<string>();
            for (int i = 0; i < paramSplit.Length; i++)
            {
                paramNames.Add(paramSplit[i]);
            }

            for (int i = 1; i < fileData.Length; i++)
            {
                try
                {
                    string[] data = fileData[i].Split(',');

                    KeyData key1 = new KeyData(paramNames[0], data[0]);
                    KeyData key2 = new KeyData(paramNames[1], data[1]);
                    KeyData key3 = new KeyData(paramNames[2], data[2]);
                    KeyData key4 = new KeyData(paramNames[3], data[3]);
                    KeyData key5 = new KeyData(paramNames[4], data[4]);
                    MappingData mData = new MappingData(key2, key1, key3, key4, key5);

                    for (int j = 0; j < data.Length; j++)
                    {
                        if (!mData.m_dictParameter.ContainsKey(paramNames[j]))
                            mData.m_dictParameter.Add(paramNames[j], data[j]);
                    }

                    retVal.Add(mData);
                }
                catch
                {
                }
            }
            return retVal;
        }
    }
}