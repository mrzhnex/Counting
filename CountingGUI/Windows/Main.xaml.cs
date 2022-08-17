using System.Threading;
using System.Windows;
using CountingLibrary.Core;
using Microsoft.WindowsAPICodePack.Dialogs;
using CountingLibrary.Handlers;
using CountingLibrary.Events;
using CountingLibrary.Main;
using System.ComponentModel;
using CountingGUI.Controls;
using System.Windows.Controls;

namespace CountingGUI.Windows
{
    public partial class Main : Window, IEventHandlerSort, IEventHandlerChangeProcessingType
    {
        private bool IsWindowClosing { get; set; }
        private OneSymbol OneSymbolControl { get; set; } = new();
        private TwoSymbols TwoSymbolsControl { get; set; } = new();
        public Main()
        {
            InitializeComponent();
            DataContext = Workspace.WorkspaceInstance;
            Grid.SetRow(OneSymbolControl, 2);
            Grid.SetRow(TwoSymbolsControl, 2);
            Action.Main.Manage.ManageInstance.RegisterAllEvents(this);
            Action.Main.Manage.ManageInstance.ExecuteEvent<IEventHandlerChangeProcessingType>(new ChangeProcessingTypeEvent(Workspace.WorkspaceInstance.Settings.ProcessingTypes[Workspace.WorkspaceInstance.Settings.ProcessingType]));
        }

        #region Non GUI
        private void DisablePauseButton(Thread thread)
        {
            Dispatcher.Invoke(() => Pause.IsEnabled = true);
            thread.Join();
            if (!IsWindowClosing)
            {
                Dispatcher.Invoke(() => Pause.IsEnabled = false);
                Dispatcher.Invoke(() => Pause.Header = "Приостановить");
                Dispatcher.Invoke(() => Start.Header = "Обработка");
            }
        }
        private void Sort()
        {
            if (AlphabetMenuItem.IsChecked)
            {
                Workspace.WorkspaceInstance.SortBy(CountingLibrary.Main.Sort.Alphabet);
            }
            else if (CountMenuItem.IsChecked)
            {
                Workspace.WorkspaceInstance.SortBy(CountingLibrary.Main.Sort.Count);
            }
            else if (DefaultMenuItem.IsChecked)
            {
                Workspace.WorkspaceInstance.SortBy(CountingLibrary.Main.Sort.Default);
            }
        }
        public void OnSort(SortEvent sortEvent)
        {
            if (IsWindowClosing)
                return;
            switch (Workspace.WorkspaceInstance.Settings.ProcessingTypes[Workspace.WorkspaceInstance.Settings.ProcessingType])
            {
                case ProcessingType.OneSymbol:
                    OneSymbolControl.ReBindingSymbolsDataContext();
                    break;
                case ProcessingType.TwoSymbols:
                    TwoSymbolsControl.SortList(sortEvent.Sort);
                    break;
                case ProcessingType.Word:

                    break;
            }
        }
        
        #endregion

        #region Click
        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (Workspace.WorkspaceInstance.ManualResetEvent.WaitOne(0))
            {
                Workspace.WorkspaceInstance.Pause();
                Pause.Header = "Продолжить";
            }
            else
            {
                Pause.Header = "Приостановить";
                Workspace.WorkspaceInstance.Continue();
            }
        }
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (Workspace.WorkspaceInstance.IsRunning)
            {
                Workspace.WorkspaceInstance.Stop();
                Start.Header = "Обработка";
                Pause.Header = "Приостановить";
            }
            else
            {
                Thread thread = new(Workspace.WorkspaceInstance.Start);
                Thread thread1 = new(() => DisablePauseButton(thread));
                Start.Header = "Остановить";
                thread.Start();
                thread1.Start();
            }
        }
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            new Help() { Owner = this }.ShowDialog();
        }
        private void SelectWorkspace_Click(object sender, RoutedEventArgs e)
        {
            if (Workspace.WorkspaceInstance.IsRunning)
                return;
            CommonFileDialog commonFileDialog = new CommonOpenFileDialog
            {
                Title = "Массив данных",
                IsFolderPicker = true,
                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };
            if (commonFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Sort oldSort = Workspace.WorkspaceInstance.Sort;
                Workspace.WorkspaceInstance = new(commonFileDialog.FileName, Workspace.WorkspaceInstance.Settings);
                DataContext = Workspace.WorkspaceInstance;
                Workspace.WorkspaceInstance.SortBy(oldSort);
                SystemInfoControl.DataContext = Workspace.WorkspaceInstance;               
                OneSymbolControl.ReBindingSymbolsDataContext();
            }
        }

        #endregion

        #region Settings
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            new Settings() { Owner = this }.ShowDialog();
        }
        private void AlphabetMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (AlphabetMenuItem.IsChecked)
                return;
            AlphabetMenuItem.IsChecked = true;
            CountMenuItem.IsChecked = false;
            DefaultMenuItem.IsChecked = false;
            Sort();
        }
        private void CountMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CountMenuItem.IsChecked)
                return;
            CountMenuItem.IsChecked = true;
            AlphabetMenuItem.IsChecked = false;
            DefaultMenuItem.IsChecked = false;
            Sort();
        }
        private void DefaultMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (DefaultMenuItem.IsChecked)
                return;
            DefaultMenuItem.IsChecked = true;
            CountMenuItem.IsChecked = false;
            AlphabetMenuItem.IsChecked = false;
            Sort();
        }
        #endregion

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            IsWindowClosing = true;
            Workspace.WorkspaceInstance.SaveSettings();
            if (Workspace.WorkspaceInstance.IsRunning)
                Workspace.WorkspaceInstance.Stop();
        }

        public void OnChangeProcessingType(ChangeProcessingTypeEvent changeProcessingTypeEvent)
        {
            switch (changeProcessingTypeEvent.ProcessingType)
            {
                case ProcessingType.OneSymbol:
                    MainGrid.Children.Add(OneSymbolControl);
                    if (MainGrid.Children.Contains(TwoSymbolsControl))
                        MainGrid.Children.Remove(TwoSymbolsControl);
                    break;
                case ProcessingType.TwoSymbols:
                    MainGrid.Children.Add(TwoSymbolsControl);
                    if (MainGrid.Children.Contains(OneSymbolControl))
                        MainGrid.Children.Remove(OneSymbolControl);
                    break;
                case ProcessingType.Word:

                    break;
            }
        }
    }
}