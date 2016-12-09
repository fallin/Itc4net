using System;
using System.IO;
using FluentAssertions;
using Itc4net.Binary;
using NUnit.Framework;

namespace Itc4net.Tests.Binary
{
    [TestFixture]
    public class BitReaderTests
    {
        [Test]
        public void ReadBitsShouldBeAbleToReadSuccessive8Bits()
        {
            byte[] bytes = { 0x55, 0xAA };
            using (var reader = new BitReader(new MemoryStream(bytes)))
            {
                byte b1;
                reader.ReadBits(8, out b1); // 01010101 10101010
                                            // ^^^^^^^^          ~~> 01010101 = 0x55
                b1.Should().Be(0x55);

                byte b2;
                reader.ReadBits(8, out b2); // 01010101 10101010
                                            //          ^^^^^^^^ ~~> 10101010 = 0xAA
                b2.Should().Be(0xAA);
            }
        }

        [Test]
        public void ReadBitsShouldBeAbleToReadSuccessive5Bits()
        {
            byte[] bytes = { 0x55, 0xAA };
            using (var reader = new BitReader(new MemoryStream(bytes)))
            {
                byte b1;
                reader.ReadBits(5, out b1); // 01010101 10101010
                                            // ^^^^^             ~~> 00001010 = 0x0A
                b1.Should().Be(0x0A);

                byte b2;
                reader.ReadBits(5, out b2); // 01010101 10101010
                                            //      ^^^ ^^       ~~> 00010110 = 0x16
                b2.Should().Be(0x16);
            }
        }

        [Test]
        public void ReadBitsShouldBeAbleToReadSuccessive5BitsThen4BitsThen2Bits()
        {
            byte[] bytes = { 0x55, 0xAA };
            using (var reader = new BitReader(new MemoryStream(bytes)))
            {
                byte b1;
                reader.ReadBits(5, out b1); // 01010101 10101010
                                            // ^^^^^             ~~> 00001010 = 0x0A
                b1.Should().Be(0x0A);

                byte b2;
                reader.ReadBits(4, out b2); // 01010101 10101010
                                            //      ^^^ ^        ~~> 00001011 = 0x0B
                b2.Should().Be(0x0B);

                byte b3;
                reader.ReadBits(2, out b3); // 01010101 10101010
                                            //           ^^      ~~> 00000001 = 0x01
                b3.Should().Be(0x01);
            }
        }

        [Test]
        public void ReadBitsShouldReturnActualBitsRead()
        {
            byte[] bytes = { 0x55, 0xAA };
            using (var reader = new BitReader(new MemoryStream(bytes)))
            {
                byte b1;
                int bitsRead1 = reader.ReadBits(5, out b1);
                bitsRead1.Should().Be(5);

                byte b2;
                int bitsRead2 = reader.ReadBits(4, out b2);
                bitsRead2.Should().Be(4);

                byte b3;
                int bitsRead3 = reader.ReadBits(2, out b3);
                bitsRead3.Should().Be(2);
            }
        }

        [Test]
        public void ReadBitsShouldReturnActualBitsReadWhenReadingEndOfStream()
        {
            byte[] bytes = { 0x55 };
            using (var reader = new BitReader(new MemoryStream(bytes)))
            {
                byte b1;
                int bitsRead1 = reader.ReadBits(5, out b1); // 01010101 --------
                                                            // ^^^^^    ~~> 00001010 = 0x0A
                b1.Should().Be(0x0A);
                bitsRead1.Should().Be(5);

                byte b2;
                int bitsRead2 = reader.ReadBits(5, out b2); // 01010101 --------
                                                            //      ^^^ ^^
                                                            // Possible alternatives:
                                                            // 1. 00000101 = 0x05 (right-aligned)
                                                            //         ^^^
                                                            // 2. 00010100 = 0x14 (zero padded)
                                                            //       ^^^^^
                                                            // Using option #2
                b2.Should().Be(0x14);
                bitsRead2.Should().Be(3);
            }
        }

        [Test]
        public void ReadBitsShouldReturnEndOfStreamConstReadWhenReadingBeyondEndOfStream()
        {
            byte[] bytes = { 0x55 };
            using (var reader = new BitReader(new MemoryStream(bytes)))
            {
                byte b1;
                int bitsRead1 = reader.ReadBits(5, out b1); // 01010101 --------
                                                            // ^^^^^    ~~> 00001010 = 0x0A
                b1.Should().Be(0x0A);
                bitsRead1.Should().Be(5);

                byte b2;
                int bitsRead2 = reader.ReadBits(5, out b2); // 01010101 -------- (reach end of stream)
                                                            //      ^^^ ^^ ~~> 00010100 = 0x14
                b2.Should().Be(0x14);
                bitsRead2.Should().Be(3);

                byte b3;
                int bitsRead3 = reader.ReadBits(5, out b3); // 01010101 -------- (beyond end of stream)
                                                            //            ^^^^^^ ~~> 0x00
                b3.Should().Be(0x00);
                bitsRead3.Should().Be(0);
            }
        }
    }
}
