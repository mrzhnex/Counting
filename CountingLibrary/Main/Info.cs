namespace CountingLibrary.Main
{
    public class Info
    {
        public char[] Numbers { get; private set; } = new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
        public char[] Symbols { get; private set; } = new char[] { ' ', '\n', '-', '+', '=', '.', ',', ';', ':', '"', '!', '?', '%', '№', '#', '~', '*', '@', '(', ')', '[', ']', '{', '}', '<', '>', '/', '\\' };
        public Alphabet Alphabet { get; private set; } = Alphabet.Ru;
        public string[] FileExtensions { get; private set; } = new string[] { ".txt", ".doc", ".docs", ".rtf", ".ibooks", ".odt", ".pdf", ".wps", ".wpd", ".pages", ".tex", ".htm", ".html", ".xhtml", ".cfm", ".jsp", ".php" };
        public string InitialTime { get; private set; } = "00:00:00:00";
        public string TimeParseString { get; private set; } = @"hh\:mm\:ss\.ff";
        public static Info Default { get; private set; } = new();
    }
    public enum Sort
    {
        Default, Alphabet, Count
    }
}