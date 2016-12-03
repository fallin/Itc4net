using System;

namespace Itc4net.Text
{
    /// <summary>
    /// A simple recursive-descent ITC stamp parser
    /// </summary>
    class Parser
    {
        string _text;
	    Token _currentToken;
	    Scanner _scanner;

		public Parser()
		{
			_currentToken = null;
			_scanner = null;
		}

	    public Stamp ParseStamp(string text)
	    {
	        if (text == null) throw new ArgumentNullException(nameof(text));

	        _text = text;
	        _scanner = new Scanner(text);
	        _currentToken = _scanner.Scan();

	        return ParseStamp();
	    }

        void ThrowUnexpectedToken(TokenKind unexpected)
        {
            int errorPosition = _currentToken.StartPosition;
            string error = $"Error parsing \"{_text}\". Unexpected token {unexpected} at index {errorPosition}.";

            throw new ParserException(error, null, _currentToken.Kind, errorPosition);
        }

        void ThrowExpectedToken(TokenKind expecting)
        {
            int errorPosition = _currentToken.StartPosition;
            string error = $"Error parsing \"{_text}\". Expecting token {expecting}"
                + $", yet found {_currentToken.Kind} @ index {errorPosition}";

            throw new ParserException(error, expecting, _currentToken.Kind, errorPosition);
        }

        void Accept()
		{
            _currentToken = _scanner.Scan();
        }

	    void Accept(TokenKind expected)
		{
			if (_currentToken.Kind == expected)
			{
				Accept();
			}
			else
			{
                ThrowExpectedToken(expected);
			}
		}

        /// <summary>
        /// Stamp := '(' Id ',' Event ')'
        /// </summary>
	    Stamp ParseStamp()
		{
			Accept(TokenKind.LParen);
		    Id i = ParseId();
            Accept(TokenKind.Comma);
		    Event e = ParseEvent();
            Accept(TokenKind.RParen);

            Accept(TokenKind.EndOfText);

            return new Stamp(i, e);
		}

        /// <summary>
        /// Id := IdLeaf | IdNode
        /// </summary>
	    Id ParseId()
	    {
	        Id i = null;
			switch (_currentToken.Kind)
			{
				case TokenKind.IntegerLiteral:
			        i = ParseIdLeaf();
					break;
				case TokenKind.LParen:
			        i = ParseIdNode();
			        break;
			    default:
			        ThrowUnexpectedToken(_currentToken.Kind);
			        break;
			}

	        return i;
	    }

        /// <summary>
        /// IdLeaf := INT_LITERAL
        /// </summary>
	    Id ParseIdLeaf()
	    {
            int n = int.Parse(_currentToken.Lexeme);
            Accept(TokenKind.IntegerLiteral);

            return new Id.Leaf(n);
        }

        /// <summary>
        /// IdNode := '(' Id ',' Id ')'
        /// </summary>
	    Id ParseIdNode()
	    {
	        Accept(TokenKind.LParen);

	        Id l = ParseId();

            Accept(TokenKind.Comma);

	        Id r = ParseId();

            Accept(TokenKind.RParen);

	        return new Id.Node(l, r);
	    }

        /// <summary>
        /// Event := EventLeaf | EventNode
        /// </summary>
	    Event ParseEvent()
	    {
	        Event e = null;
            switch (_currentToken.Kind)
            {
                case TokenKind.IntegerLiteral:
                    e = ParseEventLeaf();
                    break;
                case TokenKind.LParen:
                    e = ParseEventNode();
                    break;
                default:
                    ThrowUnexpectedToken(_currentToken.Kind);
                    break;
            }

	        return e;
	    }

        /// <summary>
        /// EventLeaf := INT_LITERAL
        /// </summary>
        Event ParseEventLeaf()
	    {
	        int n = int.Parse(_currentToken.Lexeme);
            Accept(TokenKind.IntegerLiteral);

            return new Event.Leaf(n);
	    }

        /// <summary>
        /// EventNode := '(' INT_LITERAL ',' Event ',' Event ')'
        /// </summary>
	    Event ParseEventNode()
	    {
	        Accept(TokenKind.LParen);

	        int n = int.Parse(_currentToken.Lexeme);
            Accept(TokenKind.IntegerLiteral);

            Accept(TokenKind.Comma);

	        Event l = ParseEvent();

            Accept(TokenKind.Comma);

	        Event r = ParseEvent();

            Accept(TokenKind.RParen);

            return new Event.Node(n, l, r);
	    }
	}
}
