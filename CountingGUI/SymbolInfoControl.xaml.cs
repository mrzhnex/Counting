using System.Linq;
using System.Windows.Controls;
using CountingLibrary.Core;

namespace CountingGUI
{
    public partial class SymbolInfoControl : UserControl
    {
        public SymbolInfoControl(char info, Workspace workspace)
        {
            InitializeComponent();
            DataContext = workspace.SymbolInfos.First(x => x.Symbol == info);
        }
    }
}