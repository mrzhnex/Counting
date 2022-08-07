using System.Threading;
using System.Windows;
using System.Windows.Controls;
using CountingLibrary.Core;
using CountingLibrary.Main;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;
using System;

namespace CountingGUI
{
    public partial class MainWindow : Window
    {
        private Workspace Workspace { get; set; }
        private List<SymbolInfoControl> SymbolInfoControls { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();
            Workspace = new(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            SystemInfoControl.DataContext = Workspace;
            GenerateGrids();
        }

        #region Non GUI
        private void GenerateGrids()
        {
            int gridCount = 0;
            for (int i = 0; i < Info.Default.Symbols.Length; i++)
            {
                CreateObject(Info.Default.Symbols[i], gridCount);
                gridCount++;
            }
            for (int i = 0; i < Info.Default.Numbers.Length; i++)
            {
                CreateObject(Info.Default.Numbers[i], gridCount);
                gridCount++;
            }
            for (int i = 0; i < Info.Default.Alphabet.Letters.Length; i++)
            {
                CreateObject(Info.Default.Alphabet.Letters[i], gridCount);
                gridCount++;
            }
        }
        private void ClearGrids()
        {
            for (int i = 0; i < SymbolInfoControls.Count; i++)
            {
                if (DynamicGridOne.Children.Contains(SymbolInfoControls[i]))
                    DynamicGridOne.Children.Remove(SymbolInfoControls[i]);
                if (DynamicGridTwo.Children.Contains(SymbolInfoControls[i]))
                    DynamicGridTwo.Children.Remove(SymbolInfoControls[i]);
            }
            DynamicGridOne.RowDefinitions.RemoveRange(1, DynamicGridOne.RowDefinitions.Count - 1);
            DynamicGridTwo.RowDefinitions.RemoveRange(1, DynamicGridTwo.RowDefinitions.Count - 1);

            SymbolInfoControls.Clear();
        }
        private void CreateObject(char info, int index)
        {
            SymbolInfoControl symbolInfoControl = new(info, Workspace);

            if (index % 2 == 0)
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
            Dispatcher.Invoke(() => Pause.IsEnabled = false);
            Dispatcher.Invoke(() => Pause.Content = "Пауза");
            Dispatcher.Invoke(() => Start.Content = "Старт");
        }
        #endregion

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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Workspace.IsRunning)
                Workspace.Stop();
        }


        private void SelectWorkspace_Click(object sender, RoutedEventArgs e)
        {
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
                Workspace = new(dlg.FileName);
                SystemInfoControl.DataContext = Workspace;
                ClearGrids();
                GenerateGrids();
            }
        }
    }
}