using System;
using System.Windows;
using R4nd0mApps.TddStud10.Engine;

namespace R4nd0mApps.TddStud10.Hosts.Console.TddStud10App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IEngineHost
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var slnPath = solutionPath.Text;
            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                new Engine.Engine(this, slnPath, DateTime.UtcNow).Start();
            }, null);
        }

        #region IEngineHost Members

        public bool CanStart()
        {
            throw new NotImplementedException();
        }

        public void RunStarting()
        {
            throw new NotImplementedException();
        }

        public void RunStepStarting(string stepDetails)
        {
            throw new NotImplementedException();
        }

        public void RunEnded()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
