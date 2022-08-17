using System;
using System.Collections.Generic;
using System.Windows.Controls;
using CountingLibrary.Core;

namespace CountingGUI.Controls
{
    public partial class OneSymbol : UserControl
    {
        private List<SymbolInfo> SymbolInfos { get; set; } = new();
        public OneSymbol()
        {
            InitializeComponent();
            DataContext = Workspace.WorkspaceInstance;
            GenerateGrids();
        }

        private void GenerateGrids()
        {
            for (int i = 0; i < Workspace.WorkspaceInstance.Symbols.Count; i++)
            {
                CreateObject(Workspace.WorkspaceInstance.Symbols[i], i, (int)Math.Ceiling(Workspace.WorkspaceInstance.Symbols.Count / 2.0));
            }
        }
        private void CreateObject(string symbol, int index, int rowsCount)
        {
            SymbolInfo symbolInfo = new(symbol);

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
        public void ReBindingSymbolsDataContext()
        {
            for (int i = 0; i < SymbolInfos.Count; i++)
            {
                Dispatcher.Invoke(() => SymbolInfos[i].DataContext = Workspace.WorkspaceInstance.SymbolInfos[i]);
            }
        }
    }
}