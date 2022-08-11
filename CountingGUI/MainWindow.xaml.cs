using System.Threading;
using System.Windows;
using System.Windows.Controls;
using CountingLibrary.Core;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;
using System;
using CountingLibrary.Handlers;
using CountingLibrary.Events;
using CountingLibrary.Main;

namespace CountingGUI
{
    public partial class MainWindow : Window, IEventHandlerSort
    {
        private Workspace Workspace { get; set; } = new(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        private List<SymbolInfoControl> SymbolInfoControls { get; set; } = new();
        private bool IsWindowClosing { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            SystemInfoControl.DataContext = Workspace;
            GenerateGrids();
            Action.Main.Manage.ManageInstance.RegisterAllEvents(this);
        }

        #region Non GUI
        private void GenerateGrids()
        {
            for (int i = 0; i < Workspace.Symbols.Count; i++)
            {
                CreateObject(Workspace.Symbols[i], i, (int)Math.Ceiling(Workspace.Symbols.Count / 2.0));
            }
        }
        private void CreateObject(char symbolInfo, int index, int rowsCount)
        {
            SymbolInfoControl symbolInfoControl = new(char.ToLower(symbolInfo), Workspace);
            
            if (index < rowsCount)
            {
                Grid.SetRow(symbolInfoControl, DynamicGridOne.RowDefinitions.Count);
                DynamicGridOne.RowDefinitions.Add(new RowDefinition());
                DynamicGridOne.Children.Add(symbolInfoControl);
            }
            else
            {
                Grid.SetRow(symbolInfoControl, DynamicGridTwo.RowDefinitions.Count);
                DynamicGridTwo.RowDefinitions.Add(new RowDefinition());
                DynamicGridTwo.Children.Add(symbolInfoControl);
            }
            SymbolInfoControls.Add(symbolInfoControl);
        }
        private void DisablePauseButton(Thread thread)
        {
            Dispatcher.Invoke(() => Pause.IsEnabled = true);
            thread.Join();
            if (!IsWindowClosing)
            {
                Dispatcher.Invoke(() => Pause.IsEnabled = false);
                Dispatcher.Invoke(() => Pause.Content = "Пауза");
                Dispatcher.Invoke(() => Start.Content = "Старт");
            }
        }
        private void Sort()
        {
            if (AlphabetMenuItem.IsChecked)
            {
                Workspace.SortBy(CountingLibrary.Main.Sort.Alphabet);
            }
            else if (CountMenuItem.IsChecked)
            {
                Workspace.SortBy(CountingLibrary.Main.Sort.Count);
            }
            Action.Main.Manage.ManageInstance.ExecuteEvent<IEventHandlerSort>(new SortEvent());
        }
        public void OnSort(SortEvent sortEvent)
        {
            ReBindingSymbolsDataContext();
        }
        private void ReBindingSymbolsDataContext()
        {
            for (int i = 0; i < SymbolInfoControls.Count; i++)
            {
                Dispatcher.Invoke(() => SymbolInfoControls[i].DataContext = Workspace.SymbolInfos[i]);
            }
        }
        #endregion

        #region Click
        private void Settings_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (Workspace.ManualResetEvent.WaitOne(0))
            {
                Workspace.Pause();
                Pause.Content = "Продолжить";
            }
            else
            {
                Pause.Content = "Пауза";
                Workspace.Continue();
            }
        }
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (Workspace.IsRunning)
            {
                Workspace.Stop();
                Start.Content = "Старт";
                Pause.Content = "Пауза";
            }
            else
            {
                Thread thread = new(Workspace.Start);
                Thread thread1 = new(() => DisablePauseButton(thread));
                Start.Content = "Стоп";
                thread.Start();
                thread1.Start();
            }
        }
        private void SelectWorkspace_Click(object sender, RoutedEventArgs e)
        {
            if (Workspace.IsRunning)
                return;
            var dlg = new CommonOpenFileDialog
            {
                Title = "My Title",
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
            dlg.IsFolderPicker = true;
            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Sort oldSort = Workspace.Sort;
                Workspace = new(dlg.FileName);
                Workspace.SortBy(oldSort);
                SystemInfoControl.DataContext = Workspace;
                ReBindingSymbolsDataContext();
            }
        }
        private void AlphabetMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (AlphabetMenuItem.IsChecked)
                return;
            AlphabetMenuItem.IsChecked = true;
            CountMenuItem.IsChecked = false;
            Sort();
        }
        private void CountMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CountMenuItem.IsChecked)
                return;
            CountMenuItem.IsChecked = true;
            AlphabetMenuItem.IsChecked = false;
            Sort();
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsWindowClosing = true;
            if (Workspace.IsRunning)
                Workspace.Stop();
        }
    }
}