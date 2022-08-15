﻿using System.Threading;
using System.Windows;
using System.Windows.Controls;
using CountingLibrary.Core;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;
using System;
using CountingLibrary.Handlers;
using CountingLibrary.Events;
using CountingLibrary.Main;

namespace CountingGUI.Windows
{
    public partial class Main : Window, IEventHandlerSort
    {
        private Workspace Workspace { get; set; } = new(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        private List<Controls.SymbolInfo> SymbolInfos { get; set; } = new();
        private bool IsWindowClosing { get; set; }

        public Main()
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
        private void CreateObject(char symbol, int index, int rowsCount)
        {
            Controls.SymbolInfo symbolInfo = new(char.ToLower(symbol), Workspace);
            
            if (index < rowsCount)
            {
                Grid.SetRow(symbolInfo, DynamicGridOne.RowDefinitions.Count);
                DynamicGridOne.RowDefinitions.Add(new RowDefinition());
                DynamicGridOne.Children.Add(symbolInfo);
            }
            else
            {
                Grid.SetRow(symbolInfo, DynamicGridTwo.RowDefinitions.Count);
                DynamicGridTwo.RowDefinitions.Add(new RowDefinition());
                DynamicGridTwo.Children.Add(symbolInfo);
            }
            SymbolInfos.Add(symbolInfo);
        }
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
                Workspace.SortBy(CountingLibrary.Main.Sort.Alphabet);
            }
            else if (CountMenuItem.IsChecked)
            {
                Workspace.SortBy(CountingLibrary.Main.Sort.Count);
            }
            else if (DefaultMenuItem.IsChecked)
            {
                Workspace.SortBy(CountingLibrary.Main.Sort.Default);
            }
            Action.Main.Manage.ManageInstance.ExecuteEvent<IEventHandlerSort>(new SortEvent());
        }
        public void OnSort(SortEvent sortEvent)
        {
            if (!IsWindowClosing)
                ReBindingSymbolsDataContext();
        }
        private void ReBindingSymbolsDataContext()
        {
            for (int i = 0; i < SymbolInfos.Count; i++)
            {
                if (!IsWindowClosing)
                    Dispatcher.Invoke(() => SymbolInfos[i].DataContext = Workspace.SymbolInfos[i]);
            }
        }
        #endregion

        #region Click
        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (Workspace.ManualResetEvent.WaitOne(0))
            {
                Workspace.Pause();
                Pause.Header = "Продолжить";
            }
            else
            {
                Pause.Header = "Приостановить";
                Workspace.Continue();
            }
        }
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (Workspace.IsRunning)
            {
                Workspace.Stop();
                Start.Header = "Обработка";
                Pause.Header = "Приостановить";
            }
            else
            {
                Thread thread = new(Workspace.Start);
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
            if (Workspace.IsRunning)
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
                Sort oldSort = Workspace.Sort;
                Workspace = new(commonFileDialog.FileName);
                Workspace.SortBy(oldSort);
                SystemInfoControl.DataContext = Workspace;
                ReBindingSymbolsDataContext();
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsWindowClosing = true;
            if (Workspace.IsRunning)
                Workspace.Stop();
        }
    }
}