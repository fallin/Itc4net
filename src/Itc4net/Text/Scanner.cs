using System;
using System.IO;
using System.Text;

namespace Itc4net.Text
{
    /// <summary>
    /// A simple ITC stamp scanner (tokenizer)
    /// </summary>
    class Scanner
    {
        const int EndOfString = -1;
        readonly StringBuilder _lexeme = new StringBuilder();
        int _char;
        TokenKind _kind = TokenKind.Error;
        readonly CharEnumerator _enumerator;
        int _currentPosition;

        public Scanner(string text)
        {
            _enumerator = text.GetEnumerator();
            NextChar();
        }

        public Token Scan()
        {
            int startPosition = _currentPosition;
            _lexeme.Clear();
            _kind = ScanToken();
            int endPosition = _currentPosition;
            return new Token(_kind, _lexeme.ToString(), startPosition, endPosition);
        }

        void NextChar()
        {
            _currentPosition++;
            if (_enumerator.MoveNext())
            {
                _char = _enumerator.Current;
            }
            else
            {
                _char = EndOfString;
            }
        }

        void Take()
        {
            _lexeme.Append((char) _char);
            NextChar();
        }

        TokenKind ScanToken()
        {
            TokenKind result;

            // ignore whitespace
            while (char.IsWhiteSpace((char) _char))
            {
                NextChar();
            }

            if (char.IsNumber((char) _char))
            {
                result = ScanNumber();
            }
            else
            {
                switch (_char)
                {
                    case '(':
                        Take();
                        result = TokenKind.LParen;
                        break;
                    case ')':
                        Take();
                        result = TokenKind.RParen;
                        break;
                    case ',':
                        Take();
                        result = TokenKind.Comma;
                        break;
                    case EndOfString:
                        Take();
                        result = TokenKind.EndOfText;
                        break;
                    default:
                        result = TokenKind.Error;
                        break;
                }
            }
            return result;
        }

        TokenKind ScanNumber()
        {
            TokenKind result = TokenKind.Error;
            if (char.IsDigit((char) _char))
            {
                TakeDecimalDigits();
                result = TokenKind.IntegerLiteral;
            }
            return result;
        }

        void TakeDecimalDigits()
        {
            while (char.IsDigit((char) _char))
            {
                Take();
            }
        }
    }
}