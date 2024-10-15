using DREXTrussLibForTruss.Const;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DREXCreateFunctionForTrussLink.Utils
{
    public class MdlSetting
    {
        public static void SetInitSettingTrussURL()
        {
            string sPathRef = MdlSetting.GetKeyValue("KWRD_TRUSS_DEMO_URL");
            if (String.IsNullOrEmpty(sPathRef))
            {
#if DEBUG
                TrussInfo.SERVER_HOST = "dev.truss.me";
#else
                TrussInfo.SERVER_HOST = "truss.co.jp";
#endif
            }
            else
            {
                TrussInfo.SERVER_HOST = sPathRef;
            }
        }

        /// <summary>
        /// パブリックのドキュメントにD-REXフォルダを作成して、バージョン、Settingの順番にフォルダを作成
        /// </summary>
        /// <returns></returns>
        public static string GetDHSettingPath()
        {
            string pathPubDoc = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            string pathSetting = Path.Combine(
                pathPubDoc,
                "D-REX",
                GetRevitVersion(),
                "Config");
            //パスがなければ作成する
            if( Directory.Exists(pathSetting) == false )
            {
                DirectoryInfo dinfSetting = new DirectoryInfo(pathSetting);
                try
                {
                    dinfSetting.Create();
                }
                catch ( Exception ex )
                {
                    return string.Empty;
                }
            }
            return pathSetting;
        }

        /// <summary>
        /// Revitのバージョンを取得します。
        /// </summary>
        /// <returns>Revitバージョン</returns>
        public static string GetRevitVersion()
        {
            var info = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var sVersionFile = Path.Combine(info.Directory.FullName, "version.txt");
            if (!File.Exists(sVersionFile))
            {
                // 無かったら一個上のフォルダを見る
                sVersionFile = Path.Combine(info.Directory.Parent.FullName, "version.txt");
            }

            if (!File.Exists(sVersionFile))
            {
                return string.Empty;
            }

            var version = string.Empty;

            using (var reader = new StreamReader(sVersionFile, Encoding.GetEncoding("Shift_JIS")))
            {
                while (reader.Peek() != -1)
                {
                    var curLine = reader.ReadLine();
                    if (curLine.IndexOf("RevitVersion:") == 0)
                    {
                        version = curLine.Replace("RevitVersion:", string.Empty);
                    }
                }
            }

            if (string.IsNullOrEmpty(version))
            {

                sVersionFile = Path.Combine(info.Directory.FullName, "RevitVersion.txt");
                if (!File.Exists(sVersionFile))
                {
                    return "2019";
                }

                using (var reader = new StreamReader(sVersionFile, Encoding.GetEncoding("Shift_JIS")))
                {
                    while (reader.Peek() != -1)
                    {
                        var curLine = reader.ReadLine();
                        if (curLine.IndexOf("RevitVersion:") == 0)
                        {
                            version = curLine.Replace("RevitVersion:", string.Empty);
                        }
                    }
                }
            }

            return version;
        }

        /// <summary>
        /// Path.txtファイルからキーワードに対応する値を取得する
        /// </summary>
        /// <param name="keyWord"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string GetKeyValue(string keyWord, string separator = "|")
        {
            // セッティングパス
            string pthSetting = GetDHSettingPath();
            // ファイル名---Path.txt
            string pthDef = Path.Combine(pthSetting, "Path.txt");
            if( File.Exists(pthDef) == false )
            {
                return string.Empty;
            }

            // 文字コード(ここでは、Shift JIS)
            Encoding enc = Encoding.GetEncoding("SHIFT_JIS");
            // ファイルを読む
            string[] contents = File.ReadAllLines(pthDef, enc);
            List<string> lstContents = contents.ToList();

            // キーワードに対応する言葉を探す
            string strValue = string.Empty;
            foreach( string strKey in lstContents )
            {
                if(strKey.StartsWith("//"))
                {
                    continue;
                }
                if( !strKey.Contains(separator)
                    || separator.Length == 0 )
                {
                    continue;
                }
                char[] charSeparator = separator.ToCharArray();
                string[] lstValues = strKey.Split(charSeparator );
                if( lstValues.Length > 1
                    && lstValues[ 0 ].Equals( keyWord, StringComparison.CurrentCultureIgnoreCase) )
                {
                    strValue = lstValues[1];
                    break;
                }
            }
            return strValue;
        }
    }
}
