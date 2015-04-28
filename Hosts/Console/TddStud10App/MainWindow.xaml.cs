using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
