using System.Windows;
using CountingLibrary.Core;

namespace CountingGUI.Windows
{
    public partial class Help : Window
    {
        public Help()
        {
            InitializeComponent();
            DataContext = Workspace.WorkspaceInstance;
        }
    }
}