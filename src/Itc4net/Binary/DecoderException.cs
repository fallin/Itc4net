using System;
using System.Runtime.Serialization;

namespace Itc4net.Binary
{
    [Serializable]
    public class DecoderException : Exception
    {
        public int? Expecting { get; }
        public int Found { get; }
        public int Position { get; }

        public DecoderException(string message) : base(message)
        {
        }

        public DecoderException(string message, int? expecting, int found, int position) : base(message)
        {
            Expecting = expecting;
            Found = found;
            Position = position;
        }

        protected DecoderException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}