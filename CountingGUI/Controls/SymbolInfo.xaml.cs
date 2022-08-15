using System.Linq;
using System.Windows.Controls;
using CountingLibrary.Core;

namespace CountingGUI.Controls
{
    public partial class SymbolInfo : UserControl
    {
        public SymbolInfo(char info)
        {
            InitializeComponent();
            DataContext = Workspace.WorkspaceInstance.SymbolInfos.First(x => x.Symbol == info);
        }
    }
}