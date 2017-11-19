using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Markdown
{
	public class Md
	{
		private string GetHtmlFromToken(Token token)
		{
			switch (token.Type)
			{
				case Token.TokenType.BoldOpen:
					return "<strong>";
				case Token.TokenType.BoldClose:
					return "</strong>";

				case Token.TokenType.ItalicOpen:
					return "<em>";
				case Token.TokenType.ItalicClose:
					return "</em>";

				case Token.TokenType.Text:
					return token.Text;
			}
			throw new ArgumentException("unknown token type: " + token.Type);
		}
		
		public string RenderToHtml(string markdown)
		{
			var parts = TokenParser.GetTokens(markdown)
				.Select(GetHtmlFromToken);
			return string.Concat(parts);
		}
	}

	[TestFixture]
	public class Md_ShouldRender
	{
		private Md md;

		[SetUp]
		public void setUp()
		{
			md = new Md();
		}
		
		[TestCase(
			arg: "Текст _окруженный с двух сторон_  одинарными символами подчерка " +
				 "должен помещаться в HTML-тег em вот так:",
			ExpectedResult = "Текст <em>окруженный с двух сторон</em>  одинарными символами подчерка " +
							 "должен помещаться в HTML-тег em вот так:",
			TestName = "Correctly")]
		[TestCase(
			arg: "\\_Вот это\\_, не должно выделиться тегом \\<em\\>.",
			ExpectedResult = "_Вот это_, не должно выделиться тегом \\<em\\>.",
			TestName = "IgnoringEscapedUnderscopes")]
		[TestCase(
			arg: "Внутри __двойного выделения _одинарное_ тоже__ работает.",
			ExpectedResult = "Внутри <strong>двойного выделения <em>одинарное</em> тоже</strong> работает.",
			TestName = "InsideBold")]
		[TestCase(
			arg: "Подчерки, заканчивающие выделение, должны следовать за непробельным символом. Иначе эти _подчерки _не" +
			     " считаются_ окончанием выделения и остаются просто символами подчерка.",
			ExpectedResult = "Подчерки, заканчивающие выделение, должны следовать за непробельным символом. Иначе эти " +
			                 "<em>подчерки _не считаются</em> окончанием выделения и остаются просто символами подчерка.",
			TestName = "NotCloseWhenWhiteSpacesBefore")]
		public string Italic(string markDown)
		{
			return md.RenderToHtml(markDown);
		}
		
		[TestCase(
			arg: "__Двумя символами__ — должен становиться жирным с помощью тега \\<strong\\>.",
			ExpectedResult = "<strong>Двумя символами</strong> — должен становиться жирным с помощью тега \\<strong\\>.",
			TestName = "Correctly")]
		[TestCase(
			arg: "Но не наоборот — внутри _одинарного __двойное__ не работает_",
			ExpectedResult = "Но не наоборот — внутри <em>одинарного _</em>двойное<em></em> не работает_",
			TestName = "NotInsideItalic")]
		[TestCase(
			arg: "__непарные _символы не считаются выделением.",
			ExpectedResult = "__непарные _символы не считаются выделением.",
			TestName = "NotWhenUnpaired")]
		[TestCase(
			arg: "За подчерками, начинающими выделение, должен следовать непробельный символ. Иначе эти_ подчерки_ " +
			     "не считаются выделением и остаются просто символами подчерка.",
			ExpectedResult = "За подчерками, начинающими выделение, должен следовать непробельный символ. Иначе эти_ " +
			                 "подчерки_ не считаются выделением и остаются просто символами подчерка.",
			TestName = "NotOpenWhenWhiteSpacesAfter")]
		[TestCase(
			arg: "Подчерки, заканчивающие выделение, должны следовать за непробельным символом. Иначе эти __подчерки __не" +
			     " считаются__ окончанием выделения и остаются просто символами подчерка.",
			ExpectedResult = "Подчерки, заканчивающие выделение, должны следовать за непробельным символом. Иначе эти " +
			                 "<strong>подчерки <em></em>не считаются</strong> окончанием выделения и остаются просто символами подчерка.",
			TestName = "NotCloseWhenWhiteSpacesBefore")]
		[TestCase(
			arg: "___Тройная земля делаем жирным курсивом в порядке <strong><em>___",
			ExpectedResult = "<strong><em>Тройная земля делаем жирным курсивом в порядке <strong><em></em></strong>",
			TestName = "InTripleUnderscope")]
		public string Bold(string markDown)
		{
			return md.RenderToHtml(markDown);
		}
		
		[Test]
		public void _NotToI()
		{
			new Md().RenderToHtml("Текст _окруженный с двух сторон_  одинарными символами подчерка")
				.Should().NotBe("Текст <i>окруженный с двух сторон</i>  одинарными символами подчерка");
		}
	}
}