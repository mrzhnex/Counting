﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CountingLibrary.Core
{
    public class SymbolInfo : INotifyPropertyChanged
    {
        public char Symbol { get; private set; }
        public string SymbolView { get; private set; }

        private int count;
        public int Count
        {
            get { return count; }
            private set
            {
                count = value;
                OnPropertyChanged();
                UpdatePercent();
            }
        }
        private float percent;
        public float Percent
        {
            get { return percent; }
            private set
            {
                percent = value;
                OnPropertyChanged();
            }
        }

        internal SymbolInfo(char symbol)
        {
            Symbol = symbol;
            SymbolView = symbol.ToString();
            if (symbol == '\n')
                SymbolView = "строка";
            else if (symbol == ' ')
                SymbolView = "пробел";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public void UpdatePercent()
        {
            if (Count == 0)
            {
                Percent = 0;
                return;
            }
            Percent = Workspace.WorkspaceInstance.SymbolInfos.Any(x => x.Symbol == Symbol) ? Workspace.WorkspaceInstance.SymbolInfos.First(x => x.Symbol == Symbol).Count / (float)Workspace.WorkspaceInstance.SymbolsCount * 100 : 0.0f;
        }
        internal void AddCount()
        {
            Count++;
        }
        internal void ResetCount()
        {
            Count = 0;
        }
        public override string ToString()
        {
            return Symbol.ToString();
        }
    }
}