using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace UltraScopeCSVFileFixer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Model model = new Model();
        private string aboutText = "The program is used for fixing CSV-files" + Environment.NewLine +
            Environment.NewLine +
            "which created by Rigol UltraScope (https://www.rigolna.com/download/)" + Environment.NewLine +
            Environment.NewLine +
            "and will be opened by CSV See (https://alex-exe.ru/programm/utility-csv-see/)" + Environment.NewLine +
            Environment.NewLine +
            "(press Ctrl + C to copy the message to the clipboard)";

        public MainWindow()
        {
            InitializeComponent();
            model.PropertyChanged += Model_PropertyChanged;
        }

        private void PathExploreButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV files (*.csv)|*.csv";
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
            {
                model.SetFilePath(openFileDialog.FileNames);
                PathTextBox.Text = openFileDialog.FileName;
                FixButton.IsEnabled = true;
            }
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(this, aboutText, "About UltraScope CSV File Fixer",
                MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.None, MessageBoxOptions.None);
        }

        private void FixButton_Click(object sender, RoutedEventArgs e)
        {
            model.FixFile(
                PointsQuantityTextBox.Text,
                (ZeroOffsetCheckBox.IsChecked != null) ? (bool)ZeroOffsetCheckBox.IsChecked : false,
                (EmptyPointsCheckBox.IsChecked != null) ? (bool)EmptyPointsCheckBox.IsChecked : false);
        }

        private void Model_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model.StatusText))
                StatusTextBlock.Text = model.StatusText;
        }
    }
}
