using DREXCreateFunctionForTrussLink.Data;
using DREXTrussLibForTruss.Const;
using Microsoft.Win32;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DREXCreateFunctionForTrussLink.Utils
{
    internal class CommonTruss
    {
        public static string BASE_API_URL()
        {
            return "https://" + TrussInfo.SERVER_HOST + "/";
        }

        public static string BASE_API_REVIT()
        {
            return BASE_API_URL() + "api";
        }

        public const string API_USER_INFO = "users/me";
        public const string API_USER_INFO_NEW = "users/me?api_token=usertoken";
        public const string API_PROJECT = "users/userid/get-projects?api_token=usertoken";
        public const string API_PROJECT_LIST = "users/{0}/get-projects";

        /// <summary>
        /// Get User ID From Registry
        /// </summary>
        /// <returns></returns>
        public static string GetUserID()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Truss\\Login");
                if (key == null)
                {
                    return string.Empty;
                }
                else
                {
                    var obj = key.GetValue("UserID");
                    if (obj != null && key.GetValueKind("UserID") == RegistryValueKind.String)
                    {
                        return obj.ToString();
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static List<ProjectModel> GetProjectsToken(string userid, string token)
        {
            var client = new RestClient(BASE_API_REVIT());
            var request = new RestRequest(API_PROJECT.Replace("userid", userid).Replace("usertoken", token), Method.GET);

            try
            {
                var reponse = client.Execute<GetProjectListResponse>(request);
                if (reponse.StatusCode == HttpStatusCode.OK && reponse.Data != null)
                    return reponse.Data.Projects;
                else
                    return new List<ProjectModel>();
            }
            catch (Exception ex)
            {
                return new List<ProjectModel>();
            }
        }
<<<<<<< HEAD

=======
>>>>>>> English
        /*
        /// <summary>
        /// Get List Project From Token
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static List<ProjectModel> GetProjectsToken(string userid, string token)
        {
            var client = new RestClient(BASE_API_REVIT);
            var request = new RestRequest(string.Format(API_PROJECT_LIST, userid), Method.GET);
            var splitToken = token.Split(';');
            foreach (var item in splitToken.Where(s => !string.IsNullOrEmpty(s)))
            {
                var cookies = item.Trim().Split('=');
                request.AddCookie(cookies[0], cookies[1]);
            }

            try
            {
                var reponse = client.Execute<GetProjectListResponse>(request);
                if (reponse.StatusCode == HttpStatusCode.OK && reponse.Data != null)
                    return reponse.Data.Projects;
                else
                    return new List<ProjectModel>();
            }
            catch (Exception ex)
            {
                return new List<ProjectModel>();
            }
        }
        */

        /// <summary>
        /// Get Truss Token From Registry
        /// </summary>
        /// <returns></returns>
        public static string GetTrussToken()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Truss\\Login");
                if (key == null)
                {
                    return string.Empty;
                }
                else
                {
                    var obj = key.GetValue("Token");
                    if (obj != null && key.GetValueKind("Token") == RegistryValueKind.String)
                    {
                        return obj.ToString();
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static string GetTrussTokenAPI()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Truss\\Login");
                if (key == null)
                {
                    return string.Empty;
                }
                else
                {
                    var obj = key.GetValue("TokenAPI");
                    if (obj != null && key.GetValueKind("TokenAPI") == RegistryValueKind.String)
                    {
                        return obj.ToString();
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }
}