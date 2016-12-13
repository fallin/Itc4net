using System;
using FluentAssertions;
using Itc4net.Binary;
using NUnit.Framework;

namespace Itc4net.Tests.Binary
{
    [TestFixture]
    public class DecoderTests
    {
        Decoder _decoder;

        [SetUp]
        public void SetUp()
        {
            _decoder = new Decoder();
        }

        [TearDown]
        public void TearDown()
        {
            _decoder.Dispose();
        }

        [Test]
        public void DecodeShouldReturnStampForEncodedSeedStamp()
        {
            // Arrange
            byte[] bytes = { 0x30 };

            // Act
            Stamp s = _decoder.Decode(bytes);

            // Assert
            s.Should().Be(new Stamp());
        }

        [Test]
        public void DecodeShouldReturnStampForEncodedAnonymousStampWithZeroEventLeaf()
        {
            // Arrange
            byte[] bytes = { 0x10 };

            // Act
            Stamp s = _decoder.Decode(bytes);

            // Assert
            s.Should().Be(new Stamp(0, 0));
        }

        [Test]
        public void DecodeShouldReturnStampForEncodedIdNode0I()
        {
            // Arrange
            byte[] bytes = { 0x4C, 0x00 };

            // Act
            Stamp s = _decoder.Decode(bytes);

            // Assert
            s.Should().Be(new Stamp(new Id.Node(0, 1), 0));
        }

        [Test]
        public void DecodeShouldReturnStampForEncodedIdNodeI0()
        {
            // Arrange
            byte[] bytes = { 0x8C, 0x00 };

            // Act
            Stamp s = _decoder.Decode(bytes);

            // Assert
            s.Should().Be(new Stamp(new Id.Node(1, 0), 0));
        }

        [Test]
        public void DecodeShouldReturnStampForEncodedIdNode1001()
        {
            // Arrange
            byte[] bytes = { 0xE2, 0x98 };

            // Act
            Stamp s = _decoder.Decode(bytes);

            // Assert
            s.Should().Be(new Stamp(new Id.Node(new Id.Node(1, 0), new Id.Node(0, 1)), 0));
        }

        [Test]
        public void DecodeShouldReturnStampForEncodedEventNode00E()
        {
            // Arrange
            byte[] bytes = { 0x02, 0x40 };

            // Act
            Stamp s = _decoder.Decode(bytes);

            // Assert
            s.Should().Be(new Stamp(0, new Event.Node(0, 0, 1)));
        }

        [Test]
        public void DecodeShouldReturnStampForEncodedEventNode0E0()
        {
            // Arrange
            byte[] bytes = { 0x06, 0x40 };

            // Act
            Stamp s = _decoder.Decode(bytes);

            // Assert
            s.Should().Be(new Stamp(0, new Event.Node(0, 1, 0)));
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void DecodeShouldReturnStampForEncodedEventNode0EEWithNestedEventLeaf()
        {
            // Arrange
            byte[] bytes = { 0x0A, 0x64 };

            // Act
            Stamp s = _decoder.Decode(bytes);

            // Assert
            s.Should().Be(new Stamp(0, new Event.Node(0, 1, 1)));
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void DecodeShouldReturnStampForEncodedEventNode0EEWithNestedEventNode()
        {
            // Arrange
            byte[] bytes = { 0x08, 0xCC, 0x80 };

            // Act
            Stamp s = _decoder.Decode(bytes);

            // Assert
            s.Should().Be(new Stamp(0, new Event.Node(0, new Event.Node(0, 1, 0), 1)));
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void DecodeShouldReturnStampForEncodedEventNodeN0E()
        {
            // Arrange
            byte[] bytes = { 0x0C, 0x99 };

            // Act
            Stamp s = _decoder.Decode(bytes);

            // Assert
            s.Should().Be(new Stamp(0, new Event.Node(1, 0, 1)));
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void DecodeShouldReturnStampForEncodedEventNodeNE0()
        {
            // Arrange
            byte[] bytes = { 0x0D, 0x99 };

            // Act
            Stamp s = _decoder.Decode(bytes);

            // Assert
            s.Should().Be(new Stamp(0, new Event.Node(1, 1, 0)));
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void DecodeShouldReturnStampForEncodedEventNodeNEE()
        {
            // Arrange
            byte[] bytes = { 0x0F, 0x32, 0xDB, 0x90 };

            // Act
            Stamp s = _decoder.Decode(bytes);

            // Assert
            s.Should().Be(new Stamp(0, new Event.Node(1, 1, new Event.Node(3, 1, 0))));
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void DecodeShouldReturnStampForEncodedEventNodeWithLargeNCase17()
        {
            // Arrange
            byte[] bytes = { 0x1C, 0xA0 };

            // Act
            Stamp s = _decoder.Decode(bytes);

            // Assert
            s.Should().Be(new Stamp(0, 17));
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void DecodeShouldReturnStampForEncodedEventNodeWithLargeNCase258()
        {
            // Arrange
            byte[] bytes = { 0x1F, 0xC0, 0xC0 };

            // Act
            Stamp s = _decoder.Decode(bytes);

            // Assert
            s.Should().Be(new Stamp(0, 258));
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void DecodeShouldReturnStampForEncodedEventNodeWithLargeNCase513()
        {

            // Arrange
            byte[] bytes = { 0x1F, 0xE0, 0x28 };

            // Act
            Stamp s = _decoder.Decode(bytes);

            // Assert
            // Arrange
            s.Should().Be(new Stamp(0, 513));
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void DecodeShouldReturnStampForEncodedEventNodeWithLargeNCase21474836()
        {

            // Arrange
            byte[] bytes = { 0x1F, 0xFF, 0xFF, 0xC8, 0xF5, 0xC3, 0x00 };

            // Act
            Stamp s = _decoder.Decode(bytes);

            // Assert
            // Arrange
            s.Should().Be(new Stamp(0, 21474836));
        }

        [Test]
        public void DecodeShouldReturnStampForEncodedStampWithIdNode10AndEventNode110()
        {
            // Arrange
            byte[] bytes = { 0x8B, 0x66, 0x40 };

            // Act
            Stamp s = _decoder.Decode(bytes);

            // Assert
            s.Should().Be(new Stamp(new Id.Node(1, 0), new Event.Node(1, 1, 0)));
        }

        [Test]
        public void DecodeShouldReturnStampForEncodedStampWithIdNode01AndEventNode110()
        {
            // Arrange
            byte[] bytes = { 0x4B, 0x66, 0x40 };

            // Act
            Stamp s = _decoder.Decode(bytes);

            // Assert
            s.Should().Be(new Stamp(new Id.Node(0, 1), new Event.Node(1, 1, 0)));
        }

        [Test]
        public void DecodeShouldThrowWhenNull()
        {
            Action act = () => _decoder.Decode(null);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void DecodeShouldThrowWhenEmpty()
        {
            Action act = () => _decoder.Decode(new byte[0]);

            act.ShouldThrow<DecoderException>().Where(
                e => e.Position == 0 // 0-based index for decoder
                && e.Expecting == null
                && e.Found == BitProcessor.EndOfStream);
        }

        [Test]
        public void DecodeShouldThrowWhenUnexpectedlyReachingEndOfStream()
        {
            // 00000000
            // ^^^      ~~> i 0
            //    ^^^   ~~> e with structure (0,0,er)
            //
            Action act = () => _decoder.Decode(new byte[] { 0x00 });

            act.ShouldThrow<DecoderException>().Where(
                e => e.Position == 8
                && e.Expecting == null
                && e.Found == BitProcessor.EndOfStream);
        }
    }
}
