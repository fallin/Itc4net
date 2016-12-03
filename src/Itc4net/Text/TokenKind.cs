using System;

namespace Itc4net.Text
{
    public enum TokenKind : byte
    {
        LParen,
        RParen,
        Comma,
        IntegerLiteral,
        EndOfText,
        Error
    };
}