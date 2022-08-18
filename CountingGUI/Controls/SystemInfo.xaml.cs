using System.Windows.Controls;
using CountingLibrary.Core;
using CountingLibrary.Events;
using CountingLibrary.Handlers;

namespace CountingGUI.Controls
{
    public partial class SystemInfo : UserControl, IEventHandlerChangeProcessingType
    {
        public SystemInfo()
        {
            InitializeComponent();
            DataContext = Workspace.WorkspaceInstance;
            Action.Main.Manage.ManageInstance.RegisterAllEvents(this);
        }

        public void OnChangeProcessingType(ChangeProcessingTypeEvent changeProcessingTypeEvent)
        {
            switch (changeProcessingTypeEvent.ProcessingType)
            {
                case ProcessingType.OneSymbol:
                    ProcessedSymbolsCount.Text = "Обработанных знаков";
                    WrongSymbolsCount.Text = "Необработанных знаков";
                    break;
                case ProcessingType.TwoSymbols:
                    ProcessedSymbolsCount.Text = "Обработанных пар знаков";
                    WrongSymbolsCount.Text = "Необработанных пар знаков";
                    break;
                case ProcessingType.Word:
                    ProcessedSymbolsCount.Text = "Обработанных слов";
                    WrongSymbolsCount.Text = "Необработанных слов";
                    break;
            }
        }
    }
}