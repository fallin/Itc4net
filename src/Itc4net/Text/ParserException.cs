using System;
using System.Runtime.Serialization;

namespace Itc4net.Text
{
    [Serializable]
    public class ParserException : Exception
    {
        public TokenKind? Expecting { get; }
        public TokenKind Found { get; }
        public int Position { get; }

        public ParserException(string message) : base(message) {}

        public ParserException(string message, TokenKind? expecting, TokenKind found, int position) : base(message)
        {
            Expecting = expecting;
            Found = found;
            Position = position;
        }

        protected ParserException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}