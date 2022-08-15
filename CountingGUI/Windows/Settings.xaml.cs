using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CountingLibrary.Core;

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
            foreach (Scheme scheme in Workspace.WorkspaceInstance.Settings.Schemes)
            {
                SolidColorBrushComboBox.Items.Add(scheme.Name);
            }
            FontSizeComboBox.SelectedValue = Workspace.WorkspaceInstance.Settings.FontSize;
            FontFamilyComboBox.SelectedValue = Workspace.WorkspaceInstance.Settings.FontFamily;
            SolidColorBrushComboBox.SelectedValue = Workspace.WorkspaceInstance.Settings.Scheme.Name;
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
            Workspace.WorkspaceInstance.Settings.Scheme = Workspace.WorkspaceInstance.Settings.Schemes.First(x => x.Name == (string)((ComboBox)sender).SelectedValue);
        }

        private void UpdateInRealTime_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
                Workspace.WorkspaceInstance.Settings.UpdateInRealTime = (bool)((CheckBox)sender).IsChecked;
        }
    }
}