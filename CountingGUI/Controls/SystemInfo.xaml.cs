using System.Windows.Controls;
using CountingLibrary.Core;

namespace CountingGUI.Controls
{
    public partial class SystemInfo : UserControl
    {
        public SystemInfo()
        {
            InitializeComponent();
            DataContext = Workspace.WorkspaceInstance;
        }
    }
}