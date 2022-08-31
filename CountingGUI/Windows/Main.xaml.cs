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
using System;
using Microsoft.Win32;
using System.Windows.Data;

namespace CountingGUI.Windows
{
    public partial class Main : Window, IEventHandlerSort, IEventHandlerChangeProcessingType
    {
        private bool IsWindowClosing { get; set; }
        private OneSymbol OneSymbolControl { get; set; } = new();
        private TwoSymbols TwoSymbolsControl { get; set; } = new();
        private Word WordControl { get; set; } = new();

        public Main()
        {
            InitializeComponent();
            DataContext = Workspace.WorkspaceInstance;
            Grid.SetRow(OneSymbolControl, 2);
            Grid.SetRow(TwoSymbolsControl, 2);
            Grid.SetRow(WordControl, 2);
            Action.Main.Manage.ManageInstance.RegisterAllEvents(this);
            Action.Main.Manage.ManageInstance.ExecuteEvent<IEventHandlerChangeProcessingType>(new ChangeProcessingTypeEvent(Workspace.WorkspaceInstance.Settings.GetProcessingType()));
        }

        #region Non GUI
        private void DisablePauseButton(Thread thread)
        {
            Dispatcher.Invoke(() => Pause.IsEnabled = true);
            Dispatcher.Invoke(() => SelectWorkspace.IsEnabled = false);
            thread.Join();
            if (!IsWindowClosing)
            {
                Dispatcher.Invoke(() => Pause.IsEnabled = false);
                Dispatcher.Invoke(() => SelectWorkspace.IsEnabled = true);
                Dispatcher.Invoke(() => Pause.Header = "Приостановить");
                if (Dispatcher.Invoke(() => Start.Header.ToString() != "Обработка"))
                    Dispatcher.Invoke(() => Start.Header = "Обработано");
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
            switch (Workspace.WorkspaceInstance.Settings.GetProcessingType())
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
        public void OnChangeProcessingType(ChangeProcessingTypeEvent changeProcessingTypeEvent)
        {
            switch (changeProcessingTypeEvent.ProcessingType)
            {
                case ProcessingType.OneSymbol:
                    MainGrid.Children.Add(OneSymbolControl);
                    if (MainGrid.Children.Contains(TwoSymbolsControl))
                        MainGrid.Children.Remove(TwoSymbolsControl);
                    if (MainGrid.Children.Contains(WordControl))
                        MainGrid.Children.Remove(WordControl);
                    OneSymbolControl.GenerateGrids();
                    break;
                case ProcessingType.TwoSymbols:
                    MainGrid.Children.Add(TwoSymbolsControl);
                    if (MainGrid.Children.Contains(OneSymbolControl))
                        MainGrid.Children.Remove(OneSymbolControl);
                    if (MainGrid.Children.Contains(WordControl))
                        MainGrid.Children.Remove(WordControl);
                    TwoSymbolsControl.ChangeSymboMainlInfoSymbolText("Пара");
                    break;
                case ProcessingType.Word:
                    MainGrid.Children.Add(WordControl);
                    if (MainGrid.Children.Contains(OneSymbolControl))
                        MainGrid.Children.Remove(OneSymbolControl);
                    if (MainGrid.Children.Contains(TwoSymbolsControl))
                        MainGrid.Children.Remove(TwoSymbolsControl);
                    break;
            }
            Dispatcher.Invoke(() => Start.Header = "Обработка");
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
        private void SelecFoldersWorkspace_Click(object sender, RoutedEventArgs e)
        {
            if (Workspace.WorkspaceInstance.IsRunning)
                return;
            try
            {
                CommonFileDialog commonFileDialog = new CommonOpenFileDialog
                {
                    Title = "Выбор данных",
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
                    if (Workspace.WorkspaceInstance.Settings.GetProcessingType() == ProcessingType.OneSymbol)
                        OneSymbolControl.GenerateGrids();
                    if (!Start.IsEnabled)
                        Start.IsEnabled = true;
                    Dispatcher.Invoke(() => Start.Header = "Обработка");
                    Binding binding = new()
                    {
                        Path = new PropertyPath("Settings.SolidColorBrush")
                    };
                    Dispatcher.Invoke(() => BindingOperations.ClearBinding(SelectWorkspace, BackgroundProperty));
                    Dispatcher.Invoke(() => SelectWorkspace.SetBinding(BackgroundProperty, binding));
                }
            }
            catch (Exception) { }
        }
        private void SelecFilesWorkspace_Click(object sender, RoutedEventArgs e)
        {
            if (Workspace.WorkspaceInstance.IsRunning)
                return;
            string filter = string.Join(";*", new string[] { "*.txt", ".doc", ".docx", ".docs", ".rtf", ".ibooks", ".odt", ".wps", ".wpd", ".pages", ".tex", ".htm", ".html", ".xhtml", ".cfm", ".jsp", ".php" });
            try
            {
                OpenFileDialog openFileDialog = new()
                {
                    CheckFileExists = true,
                    CheckPathExists = true,
                    Multiselect = true,
                    Title = "Выбор данных",
                    Filter = $"{filter}|{filter}"
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    Sort oldSort = Workspace.WorkspaceInstance.Sort;

                    Workspace.WorkspaceInstance = new(openFileDialog.FileNames, Workspace.WorkspaceInstance.Settings);
                    DataContext = Workspace.WorkspaceInstance;
                    Workspace.WorkspaceInstance.SortBy(oldSort);
                    SystemInfoControl.DataContext = Workspace.WorkspaceInstance;
                    if (Workspace.WorkspaceInstance.Settings.GetProcessingType() == ProcessingType.OneSymbol)
                        OneSymbolControl.GenerateGrids();
                    if (!Start.IsEnabled)
                        Start.IsEnabled = true;
                    Dispatcher.Invoke(() => Start.Header = "Обработка");
                    Binding binding = new()
                    {
                        Path = new PropertyPath("Settings.SolidColorBrush")
                    };
                    Dispatcher.Invoke(() => BindingOperations.ClearBinding(SelectWorkspace, BackgroundProperty));
                    Dispatcher.Invoke(() => SelectWorkspace.SetBinding(BackgroundProperty, binding));
                }
            }
            catch (Exception) { }
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
    }
}