using System.ComponentModel;
using System.Runtime.CompilerServices;
using CountingLibrary.Main;
using System.Windows.Media;

namespace CountingLibrary.Core
{
    public class Settings : INotifyPropertyChanged
    {
        public List<string> FileExtensions { get; private set; } = Info.Default.FileExtensions.ToList();
        public bool IncludeSubfolders { get; set; } = true;
        public Alphabet Alphabet { get; set; } = Alphabet.Ru;
        public int MaxFontSize { get; private set; } = 14;
        public int MinFontSize { get; private set; } = 8;

        private int fontSize = 12;
        public int FontSize
        {
            get { return fontSize; }
            set
            {
                fontSize = value;
                OnPropertyChanged();
            }
        }
        private string fontFamily { get; set; } = "Times New Roman";
        public string FontFamily
        {
            get { return fontFamily; }
            set
            {
                fontFamily = value;
                OnPropertyChanged();
            }
        }

        private Scheme scheme { get; set; } = new Scheme("Стандартный", new SolidColorBrush(Colors.RoyalBlue));
        public Scheme Scheme
        {
            get { return scheme; }
            set
            {
                scheme = value;
                OnPropertyChanged();
            }
        }
        public List<Scheme> Schemes { get; private set; } = new()
        {
            new Scheme("Стандартный", new SolidColorBrush(Colors.RoyalBlue)),
            new Scheme("Серый", new SolidColorBrush(Colors.DarkGray)),
            new Scheme("Зелено-голубой", new SolidColorBrush(Colors.CadetBlue))
        };

        private bool updateInRealTime = true;
        public bool UpdateInRealTime
        {
            get { return updateInRealTime; }
            set
            {
                updateInRealTime = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}