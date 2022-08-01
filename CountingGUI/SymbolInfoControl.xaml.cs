using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CountingLibrary.Main;

namespace CountingGUI
{
    /// <summary>
    /// Логика взаимодействия для SymbolInfoControl.xaml
    /// </summary>
    public partial class SymbolInfoControl : UserControl
    {
        public SymbolInfoControl(char info, Manager manager)
        {
            InitializeComponent();
            this.DataContext = manager.Workspace.SymbolInfos.Where(x => x.Symbol == info).First();
        }
    }
}
