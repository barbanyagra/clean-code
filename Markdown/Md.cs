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
			private const char EndOfInput = '\0';

			private readonly string markdown;
			private int position;
			private IEnumerable<Token> tokens;

			public TokenParser(string markdown)
			{
				this.markdown = markdown;
				position = 0;
			}

			public IEnumerable<Token> GetTokens()
			{
				if (tokens != null) return tokens;
				
				var result = new List<Token>();
				bool hasOpenBold = false;
				bool hasOpenItalic = false;

				while (PeekChar() != EndOfInput)
				{
					if (NextIsBold())
					{
						result.Add(NextAsBold(!hasOpenBold));
						hasOpenBold ^= true;
					} 
					else if (NextIsItalic())
					{
						result.Add(NextAsItalic(!hasOpenItalic));
						hasOpenItalic ^= true;
					}
					else
					{
						NextAsText();
					}
				}

				return tokens = result;
			}

			private bool NextIsBold()
			{
				return false; // TODO
			}

			private Token NextAsBold(bool isOpen)
			{
				position += 2;
				return new Token(isOpen ? TokenType.BoldOpen : TokenType.BoldClose, "__");
			}

			private bool NextIsItalic()
			{
				return false; // TODO
			}
			
			private Token NextAsItalic(bool isOpen)
			{
				position += 1;
				return new Token(isOpen ? TokenType.ItalicOpen : TokenType.ItalicClose, "_");
			}
			
			private Token NextAsText()
			{
				return new Token(TokenType.Text, PollChar().ToString());
			}

			private char PollChar()
			{
				return position < markdown.Length ? markdown[position++] : EndOfInput;
			}

			private char PeekChar()
			{
				return position < markdown.Length ? markdown[position] : EndOfInput;
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