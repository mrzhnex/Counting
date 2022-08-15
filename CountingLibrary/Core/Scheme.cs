using System.Windows.Media;

namespace CountingLibrary.Core
{
    public class Scheme
    {
        public string Name { get; set; }
        public SolidColorBrush SolidColorBrush { get; set; }
        public Scheme(string name, SolidColorBrush solidColorBrush)
        {
            Name = name;
            SolidColorBrush = solidColorBrush;
        }
    }
}