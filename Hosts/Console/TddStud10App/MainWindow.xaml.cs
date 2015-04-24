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

namespace R4nd0mApps.TddStud10.Hosts.Console.TddStud10App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                new Engine(@"d:\src\r4nd0mkatas\fizzbuzz\FizzBuzz.sln").
                    DisplayFileSystemWatcherInfo(text => Dispatcher.BeginInvoke(new Action(() => this.textBlock.Text += (text + "\n"))));
            }, null);
        }
    }
}
