using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Itc4net.Binary;
using NSubstitute;
using NUnit.Framework;

namespace Itc4net.Tests
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
        public void WriteBitsShouldBeAbleToWrite5Bits()
        {
            byte[] bytes = new byte[2];
            using (var writer = new BitWriter(new MemoryStream(bytes)))
            {
                writer.WriteBits(0xFF, 5); // 11111111
                //                               ^^^^^
            }

            bytes[0].Should().Be(0xF8);
            bytes[1].Should().Be(0x00);
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
        public void WriteBitsShouldBeAbleToWrite5BitsThen5Bits()
        {
            byte[] bytes = new byte[2];
            using (var writer = new BitWriter(new MemoryStream(bytes)))
            {
                writer.WriteBits(0xAA, 5); // 10101010
                //                               ^^^^^
                writer.WriteBits(0xAA, 5); // 10101010
                //                               ^^^^^
            }

            // 01010010 10xxxxxx = 0x52 0x80
            // ^^^^^             <- 5 bits
            //      ^^^ ^^       <- 5 bits

            bytes[0].Should().Be(0x52);
            bytes[1].Should().Be(0x80);
        }

        [Test]
        public void WriteBitsShouldBeAbleToWrite5BitsThen4BitsThen2Bits()
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
            // ^^^^^             <- 5 bits
            //      ^^^ ^        <- 4 bits
            //           ^^      <- 2 bits

            bytes[0].Should().Be(0x55);
            bytes[1].Should().Be(0x40);
        }
    }
}
