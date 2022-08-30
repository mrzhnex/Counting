using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CountingLibrary.Core;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace CountingGUI.Windows
{
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            DataContext = Workspace.WorkspaceInstance;
            SetBinding();
        }

        private void SetBinding()
        {
            for (int i = Workspace.WorkspaceInstance.Settings.MinFontSize; i <= Workspace.WorkspaceInstance.Settings.MaxFontSize; i++)
            {
                FontSizeComboBox.Items.Add(i);
            }
            foreach (FontFamily fontFamily in Fonts.SystemFontFamilies)
            {
                FontFamilyComboBox.Items.Add(fontFamily.ToString());
            }
            foreach (KeyValuePair<string, SolidColorBrush> keyValuePair in Workspace.WorkspaceInstance.Settings.Schemes)
            {
                SolidColorBrushComboBox.Items.Add(keyValuePair.Key);
            }
            foreach (KeyValuePair<string, ProcessingType> keyValuePair in Workspace.WorkspaceInstance.Settings.ProcessingTypes)
            {
                ProcessingType.Items.Add(keyValuePair.Key);
            }
            FontSizeComboBox.SelectedValue = Workspace.WorkspaceInstance.Settings.FontSize;
            FontFamilyComboBox.SelectedValue = Workspace.WorkspaceInstance.Settings.FontFamily;
            SolidColorBrushComboBox.SelectedValue = Workspace.WorkspaceInstance.Settings.Scheme;
            ProcessingType.SelectedValue = Workspace.WorkspaceInstance.Settings.ProcessingType;
            if (Workspace.WorkspaceInstance.IsRunning)
            {
                Word.IsEnabled = false;
                ProcessingType.IsEnabled = false;
                SaveInPDF.IsEnabled = false;
            }
            if (Workspace.WorkspaceInstance.IsPaused())
            {
                SaveInPDF.IsEnabled = true;
            }
            UpdateWordTextBox();
        }
        private void UpdateWordTextBox()
        {
            if (Workspace.WorkspaceInstance.Settings.GetProcessingType() == CountingLibrary.Core.ProcessingType.Word)
            {
                Word.Visibility = Visibility.Visible;
                Word.Text = Workspace.WorkspaceInstance.Settings.Word;
                Grid.SetColumnSpan(ProcessingType, 1);
            }
            else
            {
                Word.Visibility = Visibility.Hidden;
                Grid.SetColumnSpan(ProcessingType, 2);
            }
        }

        private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Workspace.WorkspaceInstance.Settings.FontSize = (int)((ComboBox)sender).SelectedValue;
        }
        private void FontFamilyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Workspace.WorkspaceInstance.Settings.FontFamily = (string)((ComboBox)sender).SelectedValue;
        }
        private void SolidColorBrushComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Workspace.WorkspaceInstance.Settings.Scheme = (string)((ComboBox)sender).SelectedValue;
        }
        private void UpdateInRealTime_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
                Workspace.WorkspaceInstance.Settings.UpdateInRealTime = ((CheckBox)sender).IsChecked ?? false;
        }
        private void ProcessingType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Workspace.WorkspaceInstance.Settings.ProcessingType = (string)((ComboBox)sender).SelectedValue;
                Workspace.WorkspaceInstance.ChangeProcessingType();
                UpdateWordTextBox();
            }
        }
        private void Word_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Workspace.WorkspaceInstance.Settings.Word = ((TextBox)sender).Text;
                Workspace.WorkspaceInstance.FullReset();
            }
        }
        private void SaveInPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CommonFileDialog commonFileDialog = new CommonOpenFileDialog
                {
                    Title = "Сохранить результат в PDF",
                    IsFolderPicker = false,
                    AddToMostRecentlyUsedList = false,
                    AllowNonFileSystemItems = false,
                    EnsureFileExists = false,
                    EnsurePathExists = true,
                    EnsureReadOnly = false,
                    EnsureValidNames = true,
                    Multiselect = false,
                    ShowPlacesList = true
                };
                if (commonFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    Workspace.WorkspaceInstance.Save(commonFileDialog.FileName + ".pdf");
                }
            }
            catch (Exception)
            {

            }
        }
    }
}