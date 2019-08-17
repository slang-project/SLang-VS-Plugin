using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace SLangPlugin
{
    // TODO: this is legacy approach, move to new API
    // https://github.com/microsoft/VSSDK-Extensibility-Samples/tree/master/ErrorList

    class ErrorList
    {
        // No use except getter
        static private ErrorListProvider __errorListProvider;
        
        // Single instance for whole solution
        static ErrorListProvider _errorListProvider
        {
            get {
                if (__errorListProvider == null)
                    __errorListProvider = new ErrorListProvider(SLangPluginPackage.Instance);
                return __errorListProvider;
            }
        }

        IVsSolution _vsSolution;
        EnvDTE80.DTE2 _dte;

        public ErrorList()
        {
            // Get services
            _vsSolution = (IVsSolution)Package.GetGlobalService(typeof(IVsSolution));
            _dte = (EnvDTE80.DTE2)Package.GetGlobalService(typeof(EnvDTE.DTE));
        }

        // TODO: delete, testing purposes function
        public void PerformTest()
        {
            var currentProject = _dte.ActiveDocument.ProjectItem.ContainingProject;
            var someFile = currentProject.ProjectItems.Item(1).FileNames[1];
            AddEntry("Test Message 1", TaskErrorCategory.Error, TaskCategory.BuildCompile, someFile);
            AddEntry("Test Message 2", TaskErrorCategory.Warning, TaskCategory.BuildCompile, someFile);
            AddEntry("Test Message 3", TaskErrorCategory.Message, TaskCategory.BuildCompile, someFile);
            AddEntry("Test Message 4", TaskErrorCategory.Error, TaskCategory.CodeSense, someFile);
            AddEntry("Test Message 5", TaskErrorCategory.Warning, TaskCategory.CodeSense, someFile);
            AddEntry("Test Message 6", TaskErrorCategory.Message, TaskCategory.CodeSense, someFile);
        }

        public void AddEntry(string message, TaskErrorCategory errorCategory, 
                             TaskCategory category, string filepath, int line = 0, int column = 0)
        {
            // Get containing project information
            var projectItem = _dte.Solution.FindProjectItem(filepath);
            var uniqueProjectName = projectItem?.ContainingProject.UniqueName;

            // Get first project IVsHierarchy item (needed to link the task with a project)
            IVsHierarchy hierarchyItem;
            _vsSolution.GetProjectOfUniqueName(uniqueProjectName, out hierarchyItem);

            var error = new ErrorTask()
            {
                Text = message,
                ErrorCategory = errorCategory,
                Category = category,
                Document = filepath,
                Line = line,
                Column = column,
                HierarchyItem = hierarchyItem,
                
            };

            error.Navigate += (sender, e) =>
            {
                // There are two bugs in the errorListProvider.Navigate method:
                // > Line number needs adjusting
                // > Column is not shown
                error.Line++;
                _errorListProvider.Navigate(error, new Guid(EnvDTE.Constants.vsViewKindCode));
                error.Line--;
            };

            _errorListProvider.Tasks.Add(error);
            _errorListProvider.Show();
        }

        // TODO: consider this mechanism for unsubsribing from navigation
        //foreach (object task in this.errorListProvider.Tasks)
        //    {
        //        ErrorListItem errorTask = task as ErrorListItem;

        //        if (errorTask != null)
        //        {
        //            errorTask.Navigate -= this.OnErrorListItemNavigate;
        //        }
        //}
public void ClearAll()
        {
            _errorListProvider.Tasks.Clear();    // clear previously created
        }
    }
}
