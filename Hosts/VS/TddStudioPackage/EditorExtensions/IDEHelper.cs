/*
Copyright (c) 2015 Raghavendra Nagaraj

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Platform.WindowManagement;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VSLangProj;
using VSLangProj80;
using R4nd0mApps.TddStud10.Hosts.VS;
using R4nd0mApps.TddStud10.Hosts.VS.Diagnostics;

namespace R4nd0mApps.TddStud10.Hosts.VS.Helpers
{
    internal static class IDEHelper
    {
        private const string BASE_IMAGE_PREFIX = "/BuildProgressBar;component/";

        private static EnvDTE.DTE DTE;

        /// <summary>
        /// Initializes the <see cref="IDEHelper"/> class.
        /// </summary>
        static IDEHelper()
        {
            DTE = (Package.GetGlobalService(typeof(EnvDTE.DTE))) as EnvDTE.DTE;
        }

        /// <summary>
        /// Opens the file in Visual Studio.
        /// </summary>
        /// <param name="file">The file path.</param>
        internal static void OpenFile(EnvDTE.DTE DTE, string file)
        {
            try
            {
                if (System.IO.File.Exists(file))
                {
                    DTE.ItemOperations.OpenFile(file);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Closes the file.
        /// </summary>
        /// <param name="DTE">The DTE.</param>
        /// <param name="fileName">Name of the file.</param>
        internal static void CloseFile(EnvDTE.DTE DTE, string fileName)
        {
            foreach (EnvDTE.Document document in DTE.Documents)
            {
                if (fileName.Equals(document.FullName, StringComparison.InvariantCultureIgnoreCase))
                {
                    document.Close();
                    break;
                }
            }
        }

        /// <summary>
        /// Moves the caret to message number.
        /// </summary>
        /// <param name="DTE">The DTE.</param>
        /// <param name="lineNumber">The message number.</param>
        internal static void GoToLine(EnvDTE.DTE DTE, int lineNumber)
        {
            DTE.ExecuteCommand("GotoLn", lineNumber.ToString());
        }

        public static string GetSolutionPath()
        {
            return DTE.Solution.FullName;
        }

        /// <summary>
        /// Finds all the dlls in the project with reference to UnitTestFramework.dll
        /// </summary>
        /// <returns>List of all dlls which might contain tests</returns>
        internal static IEnumerable<string> GetPotentialTestDLLs()
        {
            string mstestPath = "Microsoft.VisualStudio.QualityTools.UnitTestFramework.dll";
            string nunitPath = "nunit.Framework.dll";

            List<EnvDTE.Project> projects = new List<EnvDTE.Project>();

            GetProjects(DTE.Solution.Projects, projects);

            foreach (var currentProject in projects)
            {
                var vsProject2 = currentProject.Object as VSProject2;
                bool isTestProject = false;

                if (vsProject2 != null)
                {
                    foreach (Reference reference in vsProject2.References)
                    {
                        var referenceFile = Path.GetFileName(reference.Path);
                        if (mstestPath.Equals(referenceFile, StringComparison.InvariantCultureIgnoreCase) || nunitPath.Equals(referenceFile, StringComparison.InvariantCultureIgnoreCase))
                        {
                            isTestProject = true;
                            break;
                        }
                    }

                    if (isTestProject)
                    {
                        yield return GetOutputPath(currentProject);
                    }
                }
            }

        }

        /// <summary>
        /// Search for a class + meth in the opened _solution. When found, the corresponding file will
        /// be opened, and the specified meth will be shown.
        /// </summary>
        /// <param name="fullyQualifiedMethodName">Fully qualified meth to search for.</param>
        internal static void OpenFileByFullyQualifiedMethodName(string fullyQualifiedMethodName)
        {
            List<EnvDTE.Project> projects = new List<EnvDTE.Project>();
            
            GetProjects(DTE.Solution.Projects, projects);

            foreach (EnvDTE.Project project in projects)
            {
                var projectItems = project.ProjectItems;
                var found = ScanProjectItems(fullyQualifiedMethodName, projectItems);
                if (found) 
                {
                    if (Debugger.IsAttached)
                    {
                        Logger.I.LogError("Method found, stopping _solution search");
                    }

                    return; 
                }
            }

            Logger.I.LogInfo("Could not find meth '{0}' in the current _solution", fullyQualifiedMethodName);
        }

        private static bool ScanProjectItems(string fullyQualifiedMethodName, EnvDTE.ProjectItems projectItems)
        {
            foreach (EnvDTE.ProjectItem projectItem in projectItems)
            {
                if (Debugger.IsAttached)
                {
                    Logger.I.LogInfo("Processing projectItem: {0}", projectItem.Name);
                }

                if (projectItem.FileCodeModel != null)
                {
                    var codeModel = (EnvDTE.FileCodeModel)projectItem.FileCodeModel;
                    foreach (EnvDTE.CodeElement codeElement in codeModel.CodeElements)
                    {
                        EnvDTE.CodeElement discoveredMethodElement;
                        if (FindMethodInCodeElement(codeElement, fullyQualifiedMethodName, out discoveredMethodElement))
                        {
                            var filepath = (string)projectItem.Properties.Item("FullPath").Value;

                            Logger.I.LogInfo("Method '{0}' found, opening file: '{1}'", fullyQualifiedMethodName, filepath);
                            OpenFile(DTE, filepath);

                            int methodStartLine = discoveredMethodElement.StartPoint.Line;
                            Logger.I.LogInfo("Moving to meth on message: {0}", methodStartLine);
                            GoToLine(DTE, discoveredMethodElement.StartPoint.Line);
                            return true;
                        }
                    }
                }
                else if (projectItem.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder)
                {
                    if (Debugger.IsAttached)
                    {
                        Logger.I.LogInfo("Scanning subfolder: {0}", projectItem.Name);
                    }

                    var found = ScanProjectItems(fullyQualifiedMethodName, projectItem.ProjectItems);
                    if (found) 
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool FindMethodInCodeElement(EnvDTE.CodeElement codeElement, string fullyQualifiedMethodName, 
            out EnvDTE.CodeElement discoveredMethodElement)
        {
            if (codeElement.Kind == EnvDTE.vsCMElement.vsCMElementClass)
            {
                if (Debugger.IsAttached)
                {
                    Logger.I.LogInfo("Processing class: {0}", codeElement.FullName);
                }

                foreach (EnvDTE.CodeElement classChildCodeElement in codeElement.Children)
                {
                    if (classChildCodeElement.Kind == EnvDTE.vsCMElement.vsCMElementFunction)
                    {
                        if (fullyQualifiedMethodName == classChildCodeElement.FullName)
                        {
                            discoveredMethodElement = classChildCodeElement;
                            return true;
                        }
                    }
                }
            }

            foreach (EnvDTE.CodeElement childElement in codeElement.Children)
            {
                if (FindMethodInCodeElement(childElement, fullyQualifiedMethodName, out discoveredMethodElement))
                {
                    return true;
                }
            }

            discoveredMethodElement = null;
            return false;
        }

        private static void GetProjects(EnvDTE.Projects projects, List<EnvDTE.Project> projectList)
        {
            foreach (EnvDTE.Project project in projects)
                GetProjects(project, projectList);
        }

        private static void GetProjects(EnvDTE.Project project, List<EnvDTE.Project> projectList)
        {
            if (project == null)
                return;

            if (project.Kind.Contains("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC") || project.Kind.Contains("F184B08F-C81C-45F6-A57F-5ABD9991F28F"))
                projectList.Add(project);
            
            if (project.ProjectItems == null || project.ProjectItems.Count == 0)
                return;

            foreach (EnvDTE.ProjectItem proj in project.ProjectItems)
            {
                var DTEProject = proj.Object as EnvDTE.Project;
                if (DTEProject != null)
                    GetProjects(DTEProject, projectList);
            }
        }

        internal static string GetImageURL(string url)
        {
            return String.Format("{0}{1}", BASE_IMAGE_PREFIX, url);
        }

        /// <summary>
        /// Returns the output path of the project.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns>Output path</returns>
        internal static string GetOutputPath(EnvDTE.Project project)
        {
            string outputPath = project.ConfigurationManager != null && project.ConfigurationManager.ActiveConfiguration != null
                ? project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString() : null;

            if (outputPath == null)
            {
                return null;
            }

            string absoluteOutputPath;
            string projectFolder;

            if (outputPath.StartsWith(String.Format("{0}{0}", Path.DirectorySeparatorChar)))
            {
                // This is the case 1: "\\server\folder"
                absoluteOutputPath = outputPath;
            }
            else if (outputPath.Length >= 2 && outputPath[1] == Path.VolumeSeparatorChar)
            {
                // This is the case 2: "drive:\folder"
                absoluteOutputPath = outputPath;
            }
            else if (outputPath.IndexOf("..\\") != -1)
            {
                // This is the case 3: "..\..\folder"
                projectFolder = Path.GetDirectoryName(project.FullName);
                while (outputPath.StartsWith("..\\"))
                {
                    outputPath = outputPath.Substring(3);
                    projectFolder = Path.GetDirectoryName(projectFolder);
                }

                absoluteOutputPath = System.IO.Path.Combine(projectFolder, outputPath);
            }
            else
            {
                // This is the case 4: "folder"
                projectFolder = System.IO.Path.GetDirectoryName(project.FullName);
                absoluteOutputPath = System.IO.Path.Combine(projectFolder, outputPath);
            }

            return Path.Combine(absoluteOutputPath, project.Properties.Item("OutputFileName").Value.ToString());
        }

        /// <summary>
        /// Returns the property value .
        /// </summary>
        /// <typeparam name="T">Generic Type for value of the property</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Value of the property as T</returns>
        private static T GetPropertyValue<T>(Object obj, string propertyName) where T : class
        {
            return obj.GetType().GetProperty(propertyName).GetValue(obj) as T;
        }


        /// <summary>
        /// Returns the document file name of the text view.
        /// </summary>
        /// <param name="view">The view instance.</param>
        /// <returns></returns>
        internal static string GetFileName(ITextView view)
        {
            ITextBuffer TextBuffer = view.TextBuffer;

            ITextDocument TextDocument = GetTextDocument(TextBuffer);

            if (TextDocument == null || TextDocument.FilePath == null || TextDocument.FilePath.Equals("Temp.txt"))
            {
                return null;
            }

            return TextDocument.FilePath;
        }

        /// <summary>
        /// Retrives the ITextDocument from the text buffer.
        /// </summary>
        /// <param name="TextBuffer">The text buffer instance.</param>
        /// <returns></returns>
        private static ITextDocument GetTextDocument(ITextBuffer TextBuffer)
        {
            if (TextBuffer == null)
                return null;

            ITextDocument textDoc;
            var rc = TextBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out textDoc);

            if (rc == true)
                return textDoc;
            else
                return null;
        }

        /// Given an IWpfTextViewHost representing the currently selected editor pane,
        /// return the ITextDocument for that view. That's useful for learning things 
        /// like the filename of the document, its creation date, and so on.
        internal static ITextDocument GetTextDocumentForView(IWpfTextViewHost viewHost)
        {
            ITextDocument document;
            viewHost.TextView.TextDataModel.DocumentBuffer.Properties.TryGetProperty(typeof(ITextDocument), out document);
            return document;
        }

        ///// <summary>
        ///// Refreshes/Repaints the active file in Visual Studio.
        ///// </summary>
        //internal static void RefreshActiveDocument(EnvDTE.DTE DTE)
        //{
        //    try
        //    {                
        //        IWpfTextViewHost host = TddStud10Package.Instance.GetCurrentViewHost();
        //        if (host != null)
        //        {
        //            var doc = GetTextDocumentForView(host);
        //            doc.UpdateDirtyState(true, DateTime.Now);
        //        }
                            
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex.Message);
        //    }
        //}

       
    }
}
