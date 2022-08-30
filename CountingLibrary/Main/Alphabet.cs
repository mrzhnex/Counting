namespace CountingLibrary.Main
{
    public class Alphabet
    {
        public string Signature { get; private set; } = string.Empty;
        public char[] Letters { get; private set; } = Array.Empty<char>();
        public static Alphabet Ru { get; private set; } = new("Ru", new char[] { 'а', 'б', 'в', 'г', 'д', 'е', 'ё', 'ж', 'з', 'и', 'й', 'к', 'л', 'м', 'н', 'о', 'п', 'р', 'с', 'т', 'у', 'ф', 'х', 'ц', 'ч', 'ш', 'щ', 'ъ', 'ы', 'ь', 'э', 'ю', 'я' });
        public static Alphabet En { get; private set; } = new("En", Enumerable.Range('A', 'Z' - 'A' + 1).Select(i => (char)i).ToArray());

        public Alphabet(string signature, char[] letters)
        {
            Signature = signature;
            Letters = letters;
        }
    }
}