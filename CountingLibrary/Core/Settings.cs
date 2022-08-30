using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Xml.Serialization;

namespace CountingLibrary.Core
{
    [Serializable]
    public class Settings : INotifyPropertyChanged
    {
        [XmlIgnore]
        public int MaxFontSize { get; private set; } = 14;
        [XmlIgnore]
        public int MinFontSize { get; private set; } = 8;
        public int FontSize
        {
            get { return fontSize; }
            set
            {
                fontSize = value;
                OnPropertyChanged();
            }
        }
        private int fontSize = 12;
        public string FontFamily
        {
            get { return fontFamily; }
            set
            {
                fontFamily = value;
                OnPropertyChanged();
            }
        }
        private string fontFamily = "Times New Roman";
        public string Scheme
        {
            get { return scheme; }
            set
            {
                scheme = value;
                if (!Schemes.ContainsKey(scheme))
                    scheme = "Серый";
                SolidColorBrush = Schemes[scheme];
                OnPropertyChanged();
            }
        }
        private string scheme = "Серый";
        public bool UpdateInRealTime
        {
            get { return updateInRealTime; }
            set
            {
                updateInRealTime = value;
                OnPropertyChanged();
            }
        }
        private bool updateInRealTime = true;

        [XmlIgnore]
        public SolidColorBrush SolidColorBrush
        {
            get { return solidColorBrush; }
            private set
            {
                solidColorBrush = value;
                OnPropertyChanged();
            }
        }
        private SolidColorBrush solidColorBrush = new(Colors.DarkGray);

        [XmlIgnore]
        public Dictionary<string, SolidColorBrush> Schemes { get; private set; } = new()
        {
            { "Серый", new SolidColorBrush(Colors.DarkGray) },
            { "Синий", new SolidColorBrush(Colors.RoyalBlue) },
            { "Зелено-голубой", new SolidColorBrush(Colors.CadetBlue)},
            { "Розовый", new SolidColorBrush(Colors.Pink) }
        };

        public string ProcessingType
        {
            get { return processingType; }
            set
            {
                processingType = value;
                OnPropertyChanged();            
            }
        }
        private string processingType = "Знак";
        [XmlIgnore]
        public Dictionary<string, ProcessingType> ProcessingTypes { get; private set; } = new()
        {
            { "Знак", Core.ProcessingType.OneSymbol },
            { "Два знака", Core.ProcessingType.TwoSymbols },
            { "Слово", Core.ProcessingType.Word }
        };

        public string Word
        {
            get { return word; }
            set
            {
                word = value;
                OnPropertyChanged();
            }
        }
        private string word = "Слово";     

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public ProcessingType GetProcessingType()
        {
            return ProcessingTypes[ProcessingType];
        }
        public Settings() { }
    }
    public enum ProcessingType
    {
        OneSymbol, TwoSymbols, Word
    }
}