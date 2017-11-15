using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Markdown
{
	public class Md
	{
		private class TokenParser
		{
			private const char OutOfInputChar = '\0';
			private const string BoldString = "__";
			private const string ItalicString = "_";

			private readonly string markdown;
			private int position;
			private readonly List<Token> tokens;

			private int lastOpenBoldIndex = -1;
			private int lastOpenItalicIndex = -1;
			private char lastChar = OutOfInputChar;
			
			public TokenParser(string markdown)
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
			private bool BoldCanOpen() => !IsItalicOpened() && false;
			private bool BoldCanClose() => !IsItalicOpened() && false;
			private bool ItalicCanOpen() => false;
			private bool ItalicCanClose() => false;
			
			private void ChangeUnclosedTokensToText()
			{
				if (IsItalicOpened())
					tokens[lastOpenItalicIndex] = new Token(TokenType.Text, tokens[lastOpenItalicIndex].Text);

				if (IsBoldOpened())
					tokens[lastOpenBoldIndex] = new Token(TokenType.Text, tokens[lastOpenBoldIndex].Text);
			}
			
			public IEnumerable<Token> GetTokens() => tokens;

			private bool IsBoldOpened() => lastOpenBoldIndex != -1;
			private bool IsItalicOpened() => lastOpenItalicIndex != -1;

			private bool NextIs(string s) => !s.Where((t, i) => t != PeekChar(i)).Any();
			private bool NextIsEscapedUnderScope() => NextIs("\\_");
			
			private void ParseNextAsBold(bool isOpen)
			{
				var type = isOpen ? TokenType.BoldOpen : TokenType.BoldClose;
				tokens.Add(new Token(type, PollString(BoldString)));
				lastOpenBoldIndex = isOpen ? tokens.Count - 1 : -1;
			}

			private void ParseNextAsItalic(bool isOpen)
			{
				var type = isOpen ? TokenType.ItalicOpen : TokenType.ItalicClose;
				tokens.Add(new Token(type, PollString(ItalicString)));
				lastOpenItalicIndex = isOpen ? tokens.Count - 1 : -1;
			}
			
			private void ParseNextCharAsText()
			{
				tokens.Add(new Token(TokenType.Text, PollChar().ToString()));
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

		private class Token
		{
			public readonly TokenType Type;
			public readonly string Text;

			public Token(TokenType type, string text)
			{
				Type = type;
				Text = text;
			}
		}

		private enum TokenType
		{
			BoldOpen, BoldClose, ItalicOpen, ItalicClose, Text
		}

		private IEnumerable<Token> GetTokens(string markdown)
			=> new TokenParser(markdown).GetTokens();

		private string TokenToString(Token token)
		{
			switch (token.Type)
			{
				case TokenType.BoldOpen:
					return "<strong>";
				case TokenType.BoldClose:
					return "</strong>";

				case TokenType.ItalicOpen:
					return "<em>";
				case TokenType.ItalicClose:
					return "</em>";

				case TokenType.Text:
					return token.Text;
			}
			throw new ArgumentException("unknown token type: " + token.Type);
		}
		
		public string RenderToHtml(string markdown)
		{
			var parts = GetTokens(markdown)
				.Select(TokenToString);
			return string.Concat(parts);
		}
	}

	[TestFixture]
	public class Md_ShouldRender
	{
	}
}