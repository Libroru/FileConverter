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
using FFmpeg.NET;
using Ookii.Dialogs.Wpf;
using System.IO;

namespace FileConverter
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

        private async void FileSelectorButton_Click(object sender, RoutedEventArgs e)
        {
            if(ComboBoxFrom.SelectedItem == null) return;
            if(ComboBoxTo.SelectedItem == null) return;

            var dialog = new Microsoft.Win32.OpenFileDialog();
            object selectedValue = ((ComboBoxItem)ComboBoxFrom.SelectedItem).Content;
            dialog.Filter = String.Format("{0} |{1}", selectedValue.ToString().ToUpper(), String.Format("*.{0}", selectedValue.ToString().ToLower()));

            bool? result = dialog.ShowDialog();
            if (result == false) return;

            ProgressBar.Value = 0;
            await ConvertFile(dialog.FileName, ComboBoxFrom, ComboBoxTo);
            ProgressBar.Value = 100;
            MessageBox.Show("Successfully converted file!", "File Converter", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static async Task ConvertFile(string filename, ComboBox comboBoxFrom, ComboBox comboBoxTo)
        {
            var ffmpeg = new Engine(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg\\ffmpeg.exe"));
            var inputFile = new InputFile(filename);

            var comboBoxFromValue = ((ComboBoxItem)comboBoxFrom.SelectedItem).Content.ToString().ToLower();
            var comboBoxToValue = ((ComboBoxItem)comboBoxTo.SelectedItem).Content.ToString().ToLower();
            var outputFile = new OutputFile(filename.Remove(filename.Length - comboBoxFromValue.Length) + comboBoxToValue);


            await ffmpeg.ConvertAsync(inputFile, outputFile, default);
        }

        private async void FolderSelectorButton_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBoxFrom.SelectedItem == null) return;
            if (ComboBoxTo.SelectedItem == null) return;

            var dialog = new VistaFolderBrowserDialog();

            bool? result = dialog.ShowDialog();

            if (result == false) return;

            int index = 0;
            int skipped = 0;

            int directorySize = Directory.GetFiles(dialog.SelectedPath).Length;
            ProgressBar.Value = 0;

            foreach (string s in Directory.GetFiles(dialog.SelectedPath))
            {     
                index++;
                ProgressBar.Value = (index * 100) / directorySize;
                if (index == directorySize) MessageBox.Show(String.Format("Successfully converted {0} files!", directorySize - skipped), "File Converter", MessageBoxButton.OK, MessageBoxImage.Information);
                if (s.EndsWith(((ComboBoxItem)ComboBoxTo.SelectedItem).Content.ToString().ToLower()))
                {
                    skipped++;
                    continue;
                }
                await ConvertFile(s, ComboBoxFrom, ComboBoxTo);
            }
        }

        private void CheckForValuesPresent()
        {
            bool Requirement = ((ComboBoxFrom.SelectedItem != null) && (ComboBoxTo.SelectedItem != null));

            if (Requirement) FolderSelectorButton.IsEnabled = FileSelectorButton.IsEnabled = true;
            else FolderSelectorButton.IsEnabled = FileSelectorButton.IsEnabled = false;
        }

        private void ComboBoxTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckForValuesPresent();
        }

        private void ComboBoxFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckForValuesPresent();
        }
    }
}
