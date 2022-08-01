namespace CountingLibrary.Main
{
    public class Info
    {
        public char[] Numbers { get; private set; } = Enumerable.Range(0, 10).Select(i => Convert.ToChar(i.ToString())).ToArray();
        public char[] Symbols { get; private set; } = new char[] { '\n', ' ', '-', '+', '=', '.', ',', ';', ':', '"', '!', '?', '%', '№', '#', '~', '*', '@', '(', ')', '[', ']', '{', '}', '<', '>', '/', '\\' };
        public Alphabet Alphabet { get; private set; } = Alphabet.En;
        public string[] FileExtensions { get; private set; } = new string[] { ".txt", ".doc", ".docs", ".rtf", ".ibooks", ".odt", ".pdf", ".wps", ".wpd", ".pages", ".tex", ".htm", ".html", ".xhtml", ".cfm", ".jsp", ".php" };

        public static Info Default = new();
    }
}