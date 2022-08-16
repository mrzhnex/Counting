namespace CountingLibrary.Main
{
    internal class Info
    {
        internal char[] Numbers { get; private set; } = new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
        internal char[] Symbols { get; private set; } = new char[] { ' ', '\n', '-', '+', '=', '.', ',', ';', ':', '"', '!', '?', '%', '№', '#', '~', '*', '@', '(', ')', '[', ']', '{', '}', '<', '>', '/', '\\' };
        internal Alphabet Alphabet { get; private set; } = Alphabet.Ru;
        internal string[] FileExtensions { get; private set; } = new string[] { ".txt", ".doc", ".docs", ".rtf", ".ibooks", ".odt", ".pdf", ".wps", ".wpd", ".pages", ".tex", ".htm", ".html", ".xhtml", ".cfm", ".jsp", ".php" };
        internal string InitialTime { get; private set; } = "00:00:00:00";
        internal string TimeParseString { get; private set; } = @"hh\:mm\:ss\.ff";
        internal string SettingsFilePath { get; private set; } = "Settings.xml";
        internal static Info Default { get; private set; } = new();
    }

    public enum Sort
    {
        Default, Alphabet, Count
    }
}