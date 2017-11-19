namespace Markdown
{
    public class Token
    {
        public readonly TokenType Type;
        public readonly string Text;

        public Token(TokenType type, string text)
        {
            Type = type;
            Text = text;
        }
			
        public enum TokenType
        {
            BoldOpen, BoldClose, ItalicOpen, ItalicClose, Text
        }
    }
}