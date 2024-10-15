using Autodesk.Revit.DB;
<<<<<<< HEAD
using System;
=======
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
>>>>>>> English
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

<<<<<<< HEAD
=======
        public static bool SetParameterInteger(Element elm, string sParamName, int nParamValue)
        {
            Parameter param = GetParameter(elm, sParamName);
            if (param == null)
            {
                return false;
            }

            try
            {
                param.Set(nParamValue);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

>>>>>>> English
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
<<<<<<< HEAD
=======

        /// <summary>
        /// Create parameter from shared parameter file
        /// </summary>
        /// <returns></returns>
        public static bool CreateParameter(Document doc, string _pathFile, List<Category> lstCategory, ForgeTypeId bipGroup, string sParameterName, bool isInstance)
        {
            string sPathOrg = doc.Application.SharedParametersFilename;

            bool isRet = false;
            try
            {
                doc.Application.SharedParametersFilename = _pathFile;
                DefinitionFile definitionFile = doc.Application.OpenSharedParameterFile();

                if (definitionFile != null)
                {
                    Dictionary<DefinitionGroup, List<Definition>> ParameterData = new Dictionary<DefinitionGroup, List<Definition>>();
                    ParameterData = GetSharedParamFileData(definitionFile);

                    if (ParameterData != null)
                    {
                        foreach (var group in ParameterData.Keys)
                        {
                            foreach (var param in ParameterData[group])
                            {
                                if (param.Name == sParameterName) {
                                    CreateInstanceTypeParameter(doc, definitionFile, group.Name, param, lstCategory, bipGroup, isInstance);
                                }
                            }
                        }
                        isRet = true;
                    }

                }
            }
            catch (Exception ex)
            {
                string mess = ex.Message;
            }

            doc.Application.SharedParametersFilename = sPathOrg;
            return isRet;
        }

        /// <summary>
        /// Store data form Shared Parameter File
        /// </summary>
        /// <param name="definitionFile"></param>
        /// <returns></returns>
        private static Dictionary<DefinitionGroup, List<Definition>> GetSharedParamFileData(DefinitionFile definitionFile)
        {
            Dictionary<DefinitionGroup, List<Definition>> SharedFileData = null;
            List<Definition> listParam = new List<Definition>();
            if (definitionFile != null)
            {
                SharedFileData = new Dictionary<DefinitionGroup, List<Definition>>();
                foreach (var group in definitionFile.Groups)
                    SharedFileData.Add(group, group.Definitions.ToList());

                return SharedFileData;
            }
            return null;
        }

        private static bool CreateInstanceTypeParameter(Document doc,
                                                       DefinitionFile definitionFile,
                                                       string groupName,
                                                       Definition definition,
                                                       List<Category> lstCategory,
                                                       ForgeTypeId group,
                                                       bool isInstance)
        {
            if (doc == null || definitionFile == null || groupName.Length == 0 || definition.Name.Length == 0)
                return false;

            //Create a new group in the shared parameters file
            DefinitionGroups myGroups = definitionFile.Groups;

            DefinitionGroup myGroup;

            if (!myGroups.Select(x => x.Name).Contains(groupName))
                myGroup = myGroups.Create(groupName);
            else
                myGroup = myGroups.get_Item(groupName);

            if (myGroup.Definitions.Contains(definition))
                return false;

            bool insBindOK = false;

            CategorySet categories = doc.Application.Create.NewCategorySet();

            foreach (var category in lstCategory)
            {
                categories.Insert(category);
            }

            //Create an instance of TypeBinding
            Autodesk.Revit.DB.Binding insBinding = doc.Application.Create.NewTypeBinding(categories);

            if (isInstance)
                insBinding = doc.Application.Create.NewInstanceBinding(categories);
            else
                insBinding = doc.Application.Create.NewTypeBinding(categories);

            //Get the BingdingMap of current document.
            BindingMap bindingMap = doc.ParameterBindings;

            if (bindingMap.Contains(definition))
            {
                var oldBinding = bindingMap.get_Item(definition);
                if (oldBinding is InstanceBinding oldInstanceBinding)
                {
                    var newCategorySet = oldInstanceBinding.Categories;
                    var existsNewCategory = false;
                    foreach (Category category in categories)
                    {
                        if (!newCategorySet.Contains(category))
                        {
                            existsNewCategory = true;
                            newCategorySet.Insert(category);
                        }
                    }
                    if (existsNewCategory)
                    {
                        var newBinding = doc.Application.Create.NewInstanceBinding(newCategorySet);
                        insBindOK = bindingMap.ReInsert(definition, newBinding, group);
                    }
                }
                else if (oldBinding is TypeBinding oldTypeBinding)
                {
                    var newCategorySet = oldTypeBinding.Categories;
                    var existsNewCategory = false;
                    foreach (Category category in categories)
                    {
                        if (!newCategorySet.Contains(category))
                        {
                            existsNewCategory = true;
                            newCategorySet.Insert(category);
                        }
                    }
                    if (existsNewCategory)
                    {
                        var newBinding = doc.Application.Create.NewTypeBinding(newCategorySet);
                        insBindOK = bindingMap.ReInsert(definition, newBinding, group);
                    }
                }
            }
            else
            {
                insBindOK = bindingMap.Insert(definition, insBinding, group);
            }

            return insBindOK;
        }
>>>>>>> English
    }
}