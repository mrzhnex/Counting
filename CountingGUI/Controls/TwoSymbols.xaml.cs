using System.ComponentModel;
using System.Windows.Controls;
using CountingLibrary.Main;

namespace CountingGUI.Controls
{
    public partial class TwoSymbols : UserControl
    {
        public TwoSymbols()
        {
            InitializeComponent();
        }

        public void SortList(Sort sort)
        {
            switch (sort)
            {
                case Sort.Alphabet:
                    Dispatcher.Invoke(() => SymbolInfosList.Items.SortDescriptions.Add(new SortDescription("Symbol", ListSortDirection.Ascending)));
                    break;
                case Sort.Count:
                    Dispatcher.Invoke(() => SymbolInfosList.Items.SortDescriptions.Add(new SortDescription("Count", ListSortDirection.Descending)));
                    break;
                case Sort.Default:

                    break;
            }
        }
        public void ChangeSymboMainlInfoSymbolText(string text)
        {
            SymbolMainInfoControl.Symbol.Text = text;
        }
    }
}