using System;
using System.IO;
using FluentAssertions;
using Itc4net.Binary;
using NUnit.Framework;

namespace Itc4net.Tests.Binary
{
    [TestFixture]
    public class BitWriterTests
    {
        [Test]
        public void WriteBitsShouldBeAbleToWrite8Bits()
        {
            byte[] bytes = new byte[2];
            using (var writer = new BitWriter(new MemoryStream(bytes)))
            {
                writer.WriteBits(0xFF, 8); // 11111111
                //                            ^^^^^^^^
            }

            bytes[0].Should().Be(0xFF);
            bytes[1].Should().Be(0x00);
        }

        [Test]
        public void WriteBitsShouldBeAbleToWriteSuccessive8Bits()
        {
            byte[] bytes = new byte[2];
            using (var writer = new BitWriter(new MemoryStream(bytes)))
            {
                writer.WriteBits(0xFF, 8); // 11111111
                //                            ^^^^^^^^

                writer.WriteBits(0xAA, 8); // 10101010
                //                            ^^^^^^^^
            }

            bytes[0].Should().Be(0xFF);
            bytes[1].Should().Be(0xAA);
        }

        [Test]
        public void WriteBitsShouldBeAbleToWrite5Bits()
        {
            byte[] bytes = new byte[2];
            using (var writer = new BitWriter(new MemoryStream(bytes)))
            {
                writer.WriteBits(0xFF, 5); // 11111111
                //                               ^^^^^
            }

            // 11111000 = 0xF8
            // ^^^^^    = 1st 5 bits

            bytes[0].Should().Be(0xF8);
            bytes[1].Should().Be(0x00);
        }

        [Test]
        public void WriteBitsShouldBeAbleToWriteSuccessive5Bits()
        {
            byte[] bytes = new byte[2];
            using (var writer = new BitWriter(new MemoryStream(bytes)))
            {
                writer.WriteBits(0xFF, 5); // 11111111
                //                               ^^^^^

                writer.WriteBits(0xAA, 5); // 10101010
                //                               ^^^^^
            }

            // 11111010 10000000 = 0xFA 0x80
            // ^^^^^             = 1st 5 bits
            //      ^^^ ^^       = 2nd 5 bits

            bytes[0].Should().Be(0xFA);
            bytes[1].Should().Be(0x80);
        }

        [Test]
        public void WriteBitsShouldBeAbleToWrite3Bits()
        {
            byte[] bytes = new byte[2];
            using (var writer = new BitWriter(new MemoryStream(bytes)))
            {
                writer.WriteBits(0xFF, 3);  // 11111111
                //                                  ^^^
            }

            bytes[0].Should().Be(0xE0);
            bytes[1].Should().Be(0x00);
        }

        [Test]
        public void WriteBitsShouldBeAbleToWriteSuccessive3Bits()
        {
            byte[] bytes = new byte[2];
            using (var writer = new BitWriter(new MemoryStream(bytes)))
            {
                writer.WriteBits(0xFF, 3);  // 11111111
                //                                  ^^^

                writer.WriteBits(0xAA, 3);  // 10101010
                //                                  ^^^
            }

            // 11101000 = 0xE8
            // ^^^      = 1st 3 bits
            //    ^^^   = 2nd 3 bits

            bytes[0].Should().Be(0xE8);
            bytes[1].Should().Be(0x00);
        }

        [Test]
        public void WriteBitsShouldBeAbleToWrite0Bits()
        {
            byte[] bytes = new byte[2];
            using (var writer = new BitWriter(new MemoryStream(bytes)))
            {
                writer.WriteBits(0xFF, 0); // 11111111
            }

            bytes[0].Should().Be(0x00);
            bytes[1].Should().Be(0x00);
        }

        [Test]
        public void WriteBitsShouldBeAbleToWriteSuccesive5BitsThen4BitsThen2Bits()
        {
            byte[] bytes = new byte[2];
            using (var writer = new BitWriter(new MemoryStream(bytes)))
            {
                writer.WriteBits(0xAA, 5); // 10101010
                //                               ^^^^^
                writer.WriteBits(0xAA, 4); // 10101010
                //                                ^^^^
                writer.WriteBits(0xAA, 2); // 10101010
                //                                  ^^
            }

            // 01010101 010xxxxx = 0x55 0x40
            // ^^^^^             = 1st 5 bits
            //      ^^^ ^        = 2nd 4 bits
            //           ^^      = 3rd 2 bits

            bytes[0].Should().Be(0x55);
            bytes[1].Should().Be(0x40);
        }
    }
}
