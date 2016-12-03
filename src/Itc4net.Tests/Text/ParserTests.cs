using System;
using FluentAssertions;
using Itc4net.Text;
using NUnit.Framework;

namespace Itc4net.Tests.Text
{
    [TestFixture]
    public class ParserTests
    {
        [Test]
        public void ParseStampShouldReturnStampWhenLeafIdAndLeafEvent()
        {
            Stamp s = Stamp.Parse("(1,0)");

            s.Should().Be(new Stamp(1, 0));
        }

        [Test]
        public void ParseStampShouldReturnStampWhenNodeIdAndLeafEvent()
        {
            Stamp s = Stamp.Parse("((1,0),0)");

            s.Should().Be(new Stamp(new Id.Node(1, 0), 0));
        }

        [Test]
        public void ParseStampShouldReturnStampWhenNestedNodeIdAndLeafEvent()
        {
            Stamp s = Stamp.Parse("((1,(1,0)),0)");

            s.Should().Be(new Stamp(new Id.Node(1, new Id.Node(1, 0)), 0));
        }

        [Test]
        public void ParseStampShouldReturnStampWhenLeafIdAndNodeEvent()
        {
            Stamp s = Stamp.Parse("(1,(1,0,0))");

            s.Should().Be(new Stamp(1, new Event.Node(1, 0, 0)));
        }

        [Test]
        public void ParseStampShouldReturnStampWhenLeafIdAndNestedNodeEvent()
        {
            Stamp s = Stamp.Parse("(1,(1,0,(0,2,0)))");

            s.Should().Be(new Stamp(1, new Event.Node(1, 0, new Event.Node(0, 2, 0))));
        }

        [Test]
        public void ParseStampShouldThrowWhenTextNull()
        {
            Action act = () => Stamp.Parse(null);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void ParseStampShouldThrowWhenTextEmpty()
        {
            Action act = () => Stamp.Parse(string.Empty);

            act.ShouldThrow<ParserException>().Where(
                e => e.Position == 1
                && e.Expecting == TokenKind.LParen
                && e.Found == TokenKind.EndOfText);
        }

        [Test]
        public void ParseStampShouldThrowWhenTextMissingFirstParenthesis()
        {
            //                              00000000011111111112
            //                              12345678901234567890
            //                              ↓
            Action act = () => Stamp.Parse("1,(1,0,(0,2,0)))");

            act.ShouldThrow<ParserException>().Where(
                e => e.Position == 1
                && e.Expecting == TokenKind.LParen
                && e.Found == TokenKind.IntegerLiteral);
        }

        [Test]
        public void ParseStampShouldThrowWhenTextMissingLastParenthesis()
        {
            //                              00000000011111111112
            //                              12345678901234567890
            //                                  ↓
            Action act = () => Stamp.Parse("(1,0");

            act.ShouldThrow<ParserException>().Where(
                e => e.Position == 5
                && e.Expecting == TokenKind.RParen
                && e.Found == TokenKind.EndOfText);
        }

        [Test]
        public void ParseStampShouldThrowWhenTextMissingFirstComma()
        {
            //                              00000000011111111112
            //                              12345678901234567890
            //                                ↓
            Action act = () => Stamp.Parse("(1(1,0,(0,2,0)))");

            act.ShouldThrow<ParserException>().Where(
                e => e.Position == 3
                && e.Expecting == TokenKind.Comma
                && e.Found == TokenKind.LParen);
        }

        [Test]
        public void ParseStampShouldThrowWhenTextMissingFirstParenOfEventNode()
        {
            //                              00000000011111111112
            //                              12345678901234567890
            //                                  ↓
            Action act = () => Stamp.Parse("(1,1,0,1))");
            //                                 ↑
            //                                 └ the parser thinks this is an event leaf (int_literal)
            //                                   because of the missing '(', so it expects an RParen to
            //                                   end the stamp

            act.ShouldThrow<ParserException>().Where(
                e => e.Position == 5
                && e.Expecting == TokenKind.RParen
                && e.Found == TokenKind.Comma);

            // Future enhancement?
            //
            // You could argue a more sophisticated parser could detect the event
            // has the structure of an event node (i.e., (int,event,event)), not an
            // event leaf and could therefore determine the LParen is missing.
            //
            //            ↓ missing parethesis
            // As in: "(1,(1,0,1))"
            //
            // That would be pretty cool, and possibly a nice future enhancement,
            // but the current behavior is reasonable for a simple parser.

            /*
            //                              00000000011111111112
            //                              12345678901234567890
            //                                 ↓
            Action act = () => Stamp.Parse("(1,1,0,1))");
            act.ShouldThrow<ParserException>().Where(
                e => e.Position == 4
                && e.Expecting == TokenKind.LParen
                && e.Found == TokenKind.IntegerLiteral);
            */
        }
    }
}
