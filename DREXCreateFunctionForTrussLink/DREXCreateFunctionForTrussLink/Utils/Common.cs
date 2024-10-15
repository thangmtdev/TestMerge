using Autodesk.Revit.DB;
using System;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace DREXCreateFunctionForTrussLink.Utils
{
    internal class Common
    {
        /// <summary>
        /// Show information to user
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        public static void ShowInfor(string message, string title = "情報")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Show warning to user
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        public static void ShowWarning(string message, string title = "警告")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Show error to user
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        public static void ShowError(string message, string title = "エラー")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static Parameter GetParameter(Element elm, string sParamName)
        {
            Parameter param = elm.LookupParameter(sParamName);
            if (param == null)
            {
                param = elm.LookupParameter(sParamName.Replace(" ", "_"));
            }
            if (param == null)
            {
                param = elm.LookupParameter(sParamName.Replace("_", " "));
            }
            return param;
        }

        public static bool SetParameter(Element elm, string sParamName, string sParamValue)
        {
            Parameter param = GetParameter(elm, sParamName);
            if (param == null)
            {
                return false;
            }

            try
            {
                param.Set(sParamValue);
            } catch(Exception ex)
            {
                return false;
            }
            return true;
        }

        public static bool SetParameter(Element elm, string sParamName, int sParamValue)
        {
            Parameter param = GetParameter(elm, sParamName);
            if (param == null)
            {
                return false;
            }

            try
            {
                param.Set(sParamValue);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}