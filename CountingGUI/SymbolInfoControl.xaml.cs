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
            DataContext = workspace.SymbolInfos.Where(x => x.Symbol == info).First();
        }
    }
}