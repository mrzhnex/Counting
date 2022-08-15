using System.Windows.Controls;
using CountingLibrary.Core;

namespace CountingGUI.Controls
{
    public partial class SymbolMainInfo : UserControl
    {
        public SymbolMainInfo()
        {
            InitializeComponent();
            DataContext = Workspace.WorkspaceInstance;
        }
    }
}