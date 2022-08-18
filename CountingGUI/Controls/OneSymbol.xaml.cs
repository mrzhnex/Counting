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
        }

        public void GenerateGrids()
        {
            RemoveOldGrids();
            for (int i = 0; i < Workspace.WorkspaceInstance.SymbolsOne.Count; i++)
            {
                CreateObject(Workspace.WorkspaceInstance.SymbolsOne[i], i, (int)Math.Ceiling(Workspace.WorkspaceInstance.SymbolsOne.Count / 2.0));
            }
        }
        private void CreateObject(string symbol, int index, int rowsCount)
        {
            SymbolInfo symbolInfo = new();
            symbolInfo.BindingDataContext(symbol);

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
        private void RemoveOldGrids()
        {
            foreach (SymbolInfo symbolInfo in SymbolInfos)
            {
                if (DynamicGridOne.Children.Contains(symbolInfo))
                {
                    DynamicGridOne.Children.Remove(symbolInfo);
                    DynamicGridOne.RowDefinitions.RemoveAt(DynamicGridOne.RowDefinitions.Count - 1);
                }
                if (DynamicGridTwo.Children.Contains(symbolInfo))
                {
                    DynamicGridTwo.Children.Remove(symbolInfo);
                    DynamicGridTwo.RowDefinitions.RemoveAt(DynamicGridTwo.RowDefinitions.Count - 1);
                }
            }
            SymbolInfos.Clear();
        }
    }
}