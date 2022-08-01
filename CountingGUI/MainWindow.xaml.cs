using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
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
using CountingLibrary.Core;
using CountingLibrary.Main;

namespace CountingGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Manager manager = new();
        public MainWindow()
        {
            InitializeComponent();
            
            manager.AddWorkspace("D:/Workspace/Heap/Counting");
            manager.Workspace.PrepareScan();


            Binding binding = new()
            {
                Source = manager.Workspace,
                Path = new PropertyPath(nameof(Workspace.SymbolsCount)),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            //SymbolsCountTextBlock.SetBinding(TextBlock.TextProperty, binding);
            SystemInfoControl.DataContext = manager.Workspace;
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


        private void CreateObject(char info, int index)
        {
            SymbolInfoControl symbolInfoControl = new(info, manager);

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
        }

        private void test_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new(manager.Workspace.Scan);
            thread.Start();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
