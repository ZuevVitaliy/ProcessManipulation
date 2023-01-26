using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace ProcessManipulation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<string> _availableAssemblies;
        private ObservableCollection<string> _runningProcesses;

        public MainWindow()
        {
            InitializeComponent();
            _availableAssembliesList.ItemsSource = _availableAssemblies = new ObservableCollection<string>();
            _runningProcessesList.ItemsSource = _runningProcesses = new ObservableCollection<string>();
        }

        private void Start(object sender, RoutedEventArgs e)
        {

        }

        private void Stop(object sender, RoutedEventArgs e)
        {

        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {

        }

        private void Refresh(object sender, RoutedEventArgs e)
        {

        }

        private void RunCalc(object sender, RoutedEventArgs e)
        {

        }
    }
}
