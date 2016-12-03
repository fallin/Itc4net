using System;

namespace Itc4net.Text
{
    class Token
	{
	    public TokenKind Kind { get; }
	    public string Lexeme { get; }
	    public int StartPosition { get; }
        public int EndPosition { get; }

        public Token(TokenKind kind, string lexeme, int startPosition, int endPosition)
		{
			Kind = kind;
			Lexeme = lexeme;
		    StartPosition = startPosition;
		    EndPosition = endPosition;
		}
	}
}
