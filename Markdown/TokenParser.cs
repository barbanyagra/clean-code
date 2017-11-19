using System;
using System.Collections.Generic;
using System.Linq;

namespace Markdown
{
	public class TokenParser
	{
		public static IEnumerable<Token> GetTokens(string markdown)
			=> new TokenParser(markdown).GetTokens();
		
		private const char OutOfInputChar = '\0';
		private const string BoldString = "__";
		private const string ItalicString = "_";

		private readonly string markdown;
		private int position;
		private readonly List<Token> tokens;

		private int lastOpenBoldIndex = -1;
		private int lastOpenItalicIndex = -1;
		private char lastChar = OutOfInputChar;
		
		private TokenParser(string markdown)
		{
			this.markdown = markdown;
			position = 0;
			tokens = new List<Token>();
			ParseTokens();
		}

		private void ParseTokens()
		{
			while (PeekChar() != OutOfInputChar)
			{
				if (NextIs(BoldString) && BoldCanOpen())
					ParseNextAsBold(isOpen: true);
				else if (NextIs(BoldString) && BoldCanClose())
					ParseNextAsBold(isOpen: false);
				else if (NextIs(ItalicString) && ItalicCanOpen())
					ParseNextAsItalic(isOpen: true);
				else if (NextIs(ItalicString) && ItalicCanClose())
					ParseNextAsItalic(isOpen: false);
				else
				{
					if (NextIsEscapedUnderScope()) PollChar();
					ParseNextCharAsText();
				}
			}
			ChangeUnclosedTokensToText();
		}

		// TODO
		private bool BoldCanOpen() => !IsItalicOpened() && 
		                              !IsBoldOpened() && 
		                              IsDefinedAndNotWhiteSpace(PeekChar(BoldString.Length));
		private bool BoldCanClose() => IsBoldOpened() && 
		                               !IsItalicOpened() &&
		                               IsDefinedAndNotWhiteSpace(PeekChar(-1));
		private bool ItalicCanOpen() => !IsItalicOpened() && 
		                                IsDefinedAndNotWhiteSpace(PeekChar(ItalicString.Length));
		private bool ItalicCanClose() => IsItalicOpened() && 
		                                 IsDefinedAndNotWhiteSpace(PeekChar(-1));

		private bool IsDefinedAndNotWhiteSpace(char ch)
			=> ch != OutOfInputChar && !char.IsWhiteSpace(ch);
		
		private void ChangeUnclosedTokensToText()
		{
			if (IsItalicOpened())
				tokens[lastOpenItalicIndex] = new Token(Token.TokenType.Text, tokens[lastOpenItalicIndex].Text);

			if (IsBoldOpened())
				tokens[lastOpenBoldIndex] = new Token(Token.TokenType.Text, tokens[lastOpenBoldIndex].Text);
		}
		
		private IEnumerable<Token> GetTokens() => tokens;

		private bool IsBoldOpened() => lastOpenBoldIndex != -1;
		private bool IsItalicOpened() => lastOpenItalicIndex != -1;

		private bool NextIs(string s) => !s.Where((t, i) => t != PeekChar(i)).Any();
		private bool NextIsEscapedUnderScope() => NextIs("\\_");
		
		private void ParseNextAsBold(bool isOpen)
		{
			var type = isOpen ? Token.TokenType.BoldOpen : Token.TokenType.BoldClose;
			tokens.Add(new Token(type, PollString(BoldString)));
			lastOpenBoldIndex = isOpen ? tokens.Count - 1 : -1;
		}

		private void ParseNextAsItalic(bool isOpen)
		{
			var type = isOpen ? Token.TokenType.ItalicOpen : Token.TokenType.ItalicClose;
			tokens.Add(new Token(type, PollString(ItalicString)));
			lastOpenItalicIndex = isOpen ? tokens.Count - 1 : -1;
		}
		
		private void ParseNextCharAsText()
		{
			tokens.Add(new Token(Token.TokenType.Text, PollChar().ToString()));
		}

		private char PollChar()
		{
			return lastChar = (position < markdown.Length ? markdown[position++] : OutOfInputChar);
		}

		private string PollString(string s)
		{
			foreach (var ch in s)
				if (PollChar() != ch)
					throw new Exception("expected: " + ch + ", got: " + 
										(lastChar != OutOfInputChar ? lastChar.ToString() : "OutOfInput"));
			return s;
		}

		private char PeekChar(int offset = 0)
		{
			var index = position + offset;
			return 0 <= index && index < markdown.Length 
				? markdown[index]
				: OutOfInputChar;
		}
	}
}