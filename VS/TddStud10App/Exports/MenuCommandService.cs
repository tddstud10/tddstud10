using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Windows;

namespace R4nd0mApps.TddStud10.Hosts.Console.TddStud10App.Exports
{
    [Export(typeof(IMenuCommandService))]
    [Name("TddStud10App Menu Command Service")]
    public class MenuCommandService : IMenuCommandService
    {
        public MenuCommandService()
        {

        }

        public DesignerVerbCollection Verbs
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void AddCommand(MenuCommand command)
        {
            throw new NotImplementedException();
        }

        public void AddVerb(DesignerVerb verb)
        {
            throw new NotImplementedException();
        }

        public MenuCommand FindCommand(CommandID commandID)
        {
            throw new NotImplementedException();
        }

        public bool GlobalInvoke(CommandID commandID)
        {
            throw new NotImplementedException();
        }

        public void RemoveCommand(MenuCommand command)
        {
            throw new NotImplementedException();
        }

        public void RemoveVerb(DesignerVerb verb)
        {
            throw new NotImplementedException();
        }

        public void ShowContextMenu(CommandID menuID, int x, int y)
        {
            MessageBox.Show("Show context menu...");
        }
    }
}
