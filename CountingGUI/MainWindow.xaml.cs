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

            SymbolsCountTextTextBlock.Text = $"Файл {manager.Workspace.Files.First().FullName}";

            Binding binding = new()
            {
                Source = manager.Workspace,
                Path = new PropertyPath(nameof(Workspace.SymbolsCount)),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            SymbolsCountTextBlock.SetBinding(TextBlock.TextProperty, binding);

            int gridCount = 0;
            for (int i = 0; i < Info.Default.Symbols.Length; i++)
            {
                CreateObject(Info.Default.Symbols[i], $"symbol{i}", gridCount);
                gridCount++;
            }
            for (int i = 0; i < Info.Default.Numbers.Length; i++)
            {
                CreateObject(Info.Default.Numbers[i], $"number{i}", gridCount);
                gridCount++;
            }
            for (int i = 0; i < Info.Default.Alphabet.Letters.Length; i++)
            {
                CreateObject(Info.Default.Alphabet.Letters[i], $"letter{i}", gridCount);
                gridCount++;
            }
        }

        private void CreateObject(char info, string subInfo, int index)
        {
            Border border = new()
            {
                BorderThickness = new Thickness(2),
                BorderBrush = Brushes.Black
            };
            TextBlock textBlock = new()
            {
                TextAlignment = TextAlignment.Center,
                FontSize = 14,
                Text = $"{info}:"
            };

            TextBlock textBlock1 = new()
            {
                TextAlignment = TextAlignment.Center,
                FontSize = 14,
                Name = subInfo
            };


            Binding binding = new()
            {
                Source = manager.Workspace.SymbolInfos.Where(x => x.Symbol == info).First(),
                Path = new PropertyPath(nameof(SymbolInfo.Count)),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            textBlock1.SetBinding(TextBlock.TextProperty, binding);


            StackPanel stackPanel = new()
            {
                Orientation = Orientation.Horizontal
            };
            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(textBlock1);

            border.Child = stackPanel;

            if (index % 2 == 0)
            {
                Grid.SetRow(border, DynamicGrid.RowDefinitions.Count);
                Grid.SetColumn(border, 0);
            }
            else
            {
                Grid.SetRow(border, DynamicGrid.RowDefinitions.Count);
                Grid.SetColumn(border, 1);
                DynamicGrid.RowDefinitions.Add(new RowDefinition());
            }
            DynamicGrid.Children.Add(border);
        }

        private void test_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(manager.Workspace.Scan);
            thread.Start();
        }
    }
}
