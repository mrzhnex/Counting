using System.Linq;
using System.Windows.Controls;
using CountingLibrary.Core;

namespace CountingGUI.Controls
{
    public partial class SymbolInfo : UserControl
    {
        public SymbolInfo(char info, Workspace workspace)
        {
            InitializeComponent();
            DataContext = workspace.SymbolInfos.First(x => x.Symbol == info);
        }
    }
}