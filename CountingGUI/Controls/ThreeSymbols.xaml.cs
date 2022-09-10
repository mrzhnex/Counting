using System.ComponentModel;
using System.Windows.Controls;
using CountingLibrary.Main;

namespace CountingGUI.Controls
{
    public partial class ThreeSymbols : UserControl
    {
        public ThreeSymbols()
        {
            InitializeComponent();
        }

        public void SortList(Sort sort)
        {
            switch (sort)
            {
                case Sort.Alphabet:
                    SymbolInfosList.Dispatcher.BeginInvoke(() => SymbolInfosList.Items.SortDescriptions.Clear());
                    SymbolInfosList.Dispatcher.BeginInvoke(() => SymbolInfosList.Items.SortDescriptions.Add(new SortDescription("Symbol", ListSortDirection.Ascending)));
                    break;
                case Sort.Count:
                    SymbolInfosList.Dispatcher.BeginInvoke(() => SymbolInfosList.Items.SortDescriptions.Clear());
                    SymbolInfosList.Dispatcher.BeginInvoke(() => SymbolInfosList.Items.SortDescriptions.Add(new SortDescription("Count", ListSortDirection.Descending)));
                    break;
                case Sort.Default:
                    SymbolInfosList.Dispatcher.BeginInvoke(() => SymbolInfosList.Items.SortDescriptions.Clear());
                    SymbolInfosList.Dispatcher.BeginInvoke(() => SymbolInfosList.Items.SortDescriptions.Add(new SortDescription("Symbol", ListSortDirection.Ascending)));
                    break;
            }
        }
        public void ChangeSymboMainlInfoSymbolText(string text)
        {
            SymbolMainInfoControl.Symbol.Text = text;
        }
    }
}