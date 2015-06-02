using System.Windows;
using Microsoft.Win32;

namespace R4nd0mApps.TddStud10.Hosts.Console.TddStud10App.ViewModel
{
    public class UIServices : IUIServices
    {
        #region IUIServices Members

        public string OpenFile(string filter)
        {
            var openFileDialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                Filter = filter,
            };

            if ((bool)openFileDialog.ShowDialog())
            {
                return openFileDialog.FileName;
            }

            return null;
        }

        public void ShowMessageBox(string text)
        {
            MessageBox.Show(text);
        }

        #endregion
    }
}
