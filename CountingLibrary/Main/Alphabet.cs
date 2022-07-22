namespace CountingLibrary.Main
{
    public class Alphabet
    {
        public string Signature { get; private set; } = string.Empty;
        public char[] Letters { get; private set; } = Array.Empty<char>();

        public static Alphabet Ru = new("Ru", Enumerable.Range('А', 'Я' - 'А' + 1).Select(i => (char)i).ToArray());
        public static Alphabet En = new("En", Enumerable.Range('A', 'Z' - 'A' + 1).Select(i => (char)i).ToArray());

        public Alphabet(string signature, char[] letters)
        {
            Signature = signature;
            Letters = letters;
        }
    }
}