using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Itc4net.Text;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Itc4net.Tests
{
    [TestFixture]
    public class StampTests
    {
        [Test]
        public void DefaultCtorShouldCreateSeed()
        {
            var seed = new Stamp();

            seed.Should().Be(new Stamp(1, 0));
            seed.ToString().Should().Be("(1,0)");
        }

        //[Test]
        //public void ForkShould()
        //{
        //    var seed = new Stamp();

        //    Tuple<Stamp, Stamp> forks = seed.Fork();
        //    forks.Item1.ToString().Should().Be("((1,0),0)");
        //    forks.Item2.ToString().Should().Be("((0,1),0)");
        //}

        [Test(Description = "fill(0,e) = e")]
        public void FillShouldReturnUnchangedEventWhenAnonymousId()
        {
            // Arrange
            Stamp s = new Stamp(0, new Event.Node(1, 2, 3));

            // Act
            Event e = s.Fill();

            // Assert
            e.Should().Be(new Event.Node(1, 2, 3));
        }

        [Test(Description = "fill(1,e) = max(e), test e is leaf")]
        public void FillShouldReturnMaxEventWhenId1WithLeaf()
        {
            // Arrange
            Stamp s = new Stamp(1, new Event.Leaf(3));

            // Act
            Event e = s.Fill();

            // Assert
            e.Should().Be(new Event.Leaf(3));
        }

        [Test(Description = "fill(1,e) = max(e), test e is node")]
        public void FillShouldReturnMaxEventWhenId1WithNode()
        {
            // Arrange
            Stamp s = new Stamp(1, new Event.Node(3, 1, 0));

            // Act
            Event e = s.Fill();

            // Assert
            e.Should().Be(new Event.Leaf(4));
        }

        [Test(Description = "fill(i,n) = n, test id leaf:0")]
        public void FillShouldReturnLeafEventForId0WhenLeafEvent()
        {
            // Arrange
            Stamp s = new Stamp(new Id.Leaf(0), new Event.Leaf(3));

            // Act
            Event e = s.Fill();

            // Assert
            e.Should().Be(new Event.Leaf(3));
        }

        [Test(Description = "fill(i,n) = n, test id leafL1")]
        public void FillShouldReturnLeafEventForId1WhenLeafEvent()
        {
            // Arrange
            Stamp s = new Stamp(new Id.Leaf(1), new Event.Leaf(3));

            // Act
            Event e = s.Fill();

            // Assert
            e.Should().Be(new Event.Leaf(3));
        }

        [Test(Description = "fill(i,n) = n, test id node")]
        public void FillShouldReturnLeafEventForAnyIdWhenLeafEvent()
        {
            // Arrange
            Stamp s = new Stamp(new Id.Node(new Id.Node(1, 0), 0), new Event.Leaf(3));

            // Act
            Event e = s.Fill();

            // Assert
            e.Should().Be(new Event.Leaf(3));
        }

        [Test(Description = "fill((1,ir)) = norm((n,max(max(el),min(eʹr)),eʹr)), where eʹr=fill(ir,er)")]
        public void FillShouldReturnDefinedStructureWhenIdNodeLeft1()
        {
            // Arrange
            Stamp s = new Stamp(new Id.Node(1, 0), new Event.Node(1, new Event.Node(0, 1, 0), 1));

            // Act
            Event e = s.Fill();

            // Assert
            e.Should().Be(new Event.Leaf(2));
        }

        [Test(Description = "fill((il,1)) = norm((n,eʹl,max(max(er),min(eʹl)))), where eʹl=fill(il,el)")]
        public void FillShouldReturnDefinedStructureWhenIdNodeRight1()
        {
            // Arrange
            Stamp s = new Stamp(new Id.Node(0, 1), new Event.Node(1, 1, new Event.Node(0, 1, 0)));

            // Act
            Event e = s.Fill();

            // Assert
            e.Should().Be(new Event.Leaf(2));
        }

        [Test(Description = "fill((il,ir)) = norm(n,fill(il,el),fill(ir,er))")]
        public void FillShouldReturnDefinedStructureWhenIdNode()
        {
            // Arrange
            Stamp s = new Stamp(
                new Id.Node(new Id.Node(1, 0), new Id.Node(0, 1)),
                new Event.Node(1, new Event.Node(0, 1, 2), new Event.Node(0, 1, 2))
                );

            // Act
            Event e = s.Fill();

            // Assert
            e.Should().Be(new Event.Node(2, 1, new Event.Node(0, 0, 1)));
        }

        [Test(Description = "grow(1,n) = (n+1,0)")]
        public void GrowShouldInflateEventLeafWhenId1AndLeafEvent()
        {
            // Arrange
            Stamp s = new Stamp(1, 2);

            // Act
            var result = s.Grow();
            Event e = result.Item1;
            int cost = result.Item2;

            // Assert
            e.Should().Be(new Event.Leaf(3));
            cost.Should().Be(0);
        }

        [Test(Description = "grow(i,n), where (eʹ,c) = grow(i,(n,0,0)), test id:(0,1)")]
        public void GrowShouldInflateRightEventLeafWhenEventLeafAndLeftId0()
        {
            // Arrange
            Stamp s = new Stamp(new Id.Node(0, 1), 1);

            // Act
            var result = s.Grow();
            Event e = result.Item1;
            int cost = result.Item2;

            // Assert
            e.Should().Be(new Event.Node(1, 0, 1));
            cost.Should().Be(1001);
        }

        [Test(Description = "grow(i,n), where (eʹ,c) = grow(i,(n,0,0)), test id:(1,0)")]
        public void GrowShouldInflateLeftEventLeafWhenEventLeafAndRightId1()
        {
            // Arrange
            Stamp s = new Stamp(new Id.Node(1, 0), 1);

            // Act
            var result = s.Grow();
            Event e = result.Item1;
            int cost = result.Item2;

            // Assert
            e.Should().Be(new Event.Node(1, 1, 0));
            cost.Should().Be(1001);
        }

       [Test(Description = "grow((0,ir),(n,el,er)) = ((n,el,eʹr),cr+1), where (eʹr,cr)=grow(ir,er)")]
        public void GrowShouldInflateRightEventWhenLeftId0()
        {
            // Arrange
            Stamp s = new Stamp(new Id.Node(0, 1), new Event.Node(1, 1, 0));

            // Act
            var result = s.Grow();
            Event e = result.Item1;
            int cost = result.Item2;

            // Assert
            e.Should().Be(new Event.Node(1, 1, 1)); // TODO: shouldn't grow normalize to (2,0,0)?
            cost.Should().Be(1);
        }

        [Test(Description = "grow((il,0),(n,el,er)) = ((n,eʹl,er),cl+1), where (eʹl,cl)=grow(ilr,el)")]
        public void GrowShouldInflateLeftEventWhenRightId0()
        {
            // Arrange
            Stamp s = new Stamp(new Id.Node(1, 0), new Event.Node(1, 1, 0));

            // Act
            var result = s.Grow();
            Event e = result.Item1;
            int cost = result.Item2;

            // Assert
            e.Should().Be(new Event.Node(1, 2, 0));
            cost.Should().Be(1);
        }

        [Test]
        public void LeqShouldReturnTrueWhenComparingOriginalStampAndInflatedStamp()
        {
            // Arrange
            Stamp s1 = new Stamp();

            // Act
            Stamp s2 = s1.Event();

            // Assert
            s1.Leq(s2).Should().BeTrue();
        }

        [Test]
        public void LeqShouldReturnFalseWhenComparingInflatedStampAndOriginalStamp()
        {
            // Arrange
            Stamp s1 = new Stamp();

            // Act
            Stamp s2 = s1.Event();

            // Assert
            s2.Leq(s1).Should().BeFalse();
        }

        [Test]
        public void LeqShouldReturnTrueWhenComparingWithItself()
        {
            // Arrange
            Stamp s = new Stamp().Event().Event();

            // Act & Assert
            s.Leq(s).Should().BeTrue();
        }

        [Test]
        public void ToBinaryShouldReturnEncodedBytesForSeedStamp()
        {
            // Arrange
            Stamp s = new Stamp(); // (1,0)

            // Act
            byte[] bytes = s.ToBinary();

            // Assert
            bytes.ShouldBeEquivalentTo(new byte[] { 0x30 });
            // 00110000 = 0x30
            // ^^^      id
            //    ^^^^  event
        }

        [Test]
        public void ToBinaryShouldReturnEncodedBytesForAnonymousStampWithZeroEventLeaf()
        {
            // Arrange
            Stamp s = new Stamp(0, 0); // (0,0)

            // Act
            byte[] bytes = s.ToBinary();

            // Assert
            bytes.ShouldBeEquivalentTo(new byte[] { 0x10 });
            // 00010000 = 0x10
            // ^^^      id
            //    ^^^^  event
        }

        [Test]
        public void ToBinaryShouldReturnEncodedBytesForIdNode0I()
        {
            // Arrange
            Stamp s = new Stamp(new Id.Node(0, 1), 0);

            // Act
            byte[] bytes = s.ToBinary();

            // Assert
            bytes.ShouldBeEquivalentTo(new byte[] { 0x4C, 0x00 });
            // 01001100 00000000 = 0x4C 0x00
            // ^^^^^             id
            //      ^^^ ^        event
        }

        [Test]
        public void ToBinaryShouldReturnEncodedBytesForIdNodeI0()
        {
            // Arrange
            Stamp s = new Stamp(new Id.Node(1, 0), 0);

            // Act
            byte[] bytes = s.ToBinary();

            // Assert
            bytes.ShouldBeEquivalentTo(new byte[] { 0x8C, 0x00 });
            // 10001100 00000000 = 0x8C 0x00
            // ^^^^^             id
            //      ^^^ ^        event
        }

        [Test]
        public void ToBinaryShouldReturnEncodedBytesForIdNode1001()
        {
            // Arrange
            Stamp s = new Stamp(new Id.Node(new Id.Node(1, 0), new Id.Node(0, 1)), 0);

            // Act
            byte[] bytes = s.ToBinary();

            // Assert
            bytes.ShouldBeEquivalentTo(new byte[] { 0xE2, 0x98 });
            // 11100010 10011000 = 0xE2 0x98
            // ^^^^^^^^ ^^^^     id
            //              ^^^^ event
        }

        [Test]
        public void ToBinaryShouldReturnEncodedBytesForEventNode00E()
        {
            // Arrange
            Stamp s = new Stamp(0, new Event.Node(0, 0, 1));

            // Act
            byte[] bytes = s.ToBinary();

            // Assert
            bytes.ShouldBeEquivalentTo(new byte[] { 0x02, 0x40 });
            // 00000010 01000000 = 0x02 0x40
            // ^^^               id
            //    ^^^^^ ^^       event
        }

        [Test]
        public void ToBinaryShouldReturnEncodedBytesForEventNode0E0()
        {
            // Arrange
            Stamp s = new Stamp(0, new Event.Node(0, 1, 0));

            // Act
            byte[] bytes = s.ToBinary();

            // Assert
            bytes.ShouldBeEquivalentTo(new byte[] { 0x06, 0x40 });
            // 00000110 01000000 = 0x06 0x40
            // ^^^               id
            //    ^^^^^ ^^       event
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void ToBinaryShouldReturnEncodedBytesForEventNode0EEWithNestedEventLeaf()
        {
            // Arrange
            Stamp s = new Stamp(0, new Event.Node(0, 1, 1));

            // Act
            byte[] bytes = s.ToBinary();

            // Assert
            bytes.ShouldBeEquivalentTo(new byte[] { 0x0A, 0x64 });
            // 00001010 01100100 = 0x0A 0x64
            // ^^^               id
            //    ^^^^^ ^^^^^^   event
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void ToBinaryShouldReturnEncodedBytesForEventNode0EEWithNestedEventNode()
        {
            // Arrange
            Stamp s = new Stamp(0, new Event.Node(0, new Event.Node(0, 1, 0), 1));

            // Act
            byte[] bytes = s.ToBinary();

            // Assert
            bytes.ShouldBeEquivalentTo(new byte[] { 0x08, 0xCC, 0x80 });
            // 00001000 11001100 10000000 = 0x08 0xCC 0x80
            // ^^^                        id
            //    ^^^^^ ^^^^^^^^ ^        event
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void ToBinaryShouldReturnEncodedBytesForEventNodeN0E()
        {
            // Arrange
            Stamp s = new Stamp(0, new Event.Node(1, 0, 1));

            // Act
            byte[] bytes = s.ToBinary();

            // Assert
            bytes.ShouldBeEquivalentTo(new byte[] { 0x0C, 0x99 });
            // 00001100 10011001 = 0x0C 0x99
            // ^^^               id
            //    ^^^^^ ^^^^^^^^ event
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void ToBinaryShouldReturnEncodedBytesForEventNodeNE0()
        {
            // Arrange
            Stamp s = new Stamp(0, new Event.Node(1, 1, 0));

            // Act
            byte[] bytes = s.ToBinary();

            // Assert
            bytes.ShouldBeEquivalentTo(new byte[] { 0x0D, 0x99 });
            // 00001101 10011001 = 0x0D 0x99
            // ^^^               id
            //    ^^^^^ ^^^^^^^^ event
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void ToBinaryShouldReturnEncodedBytesForEventNodeNEE()
        {
            // Arrange
            Stamp s = new Stamp(0, new Event.Node(1, 1, new Event.Node(3, 1, 0)));

            // Act
            byte[] bytes = s.ToBinary();

            // Assert
            bytes.ShouldBeEquivalentTo(new byte[] { 0x0F, 0x32, 0xDB, 0x90 });
            // 00001111 00110010 11011011 10010000 = 0x0F 0x32 0xDB 0x90
            // ^^^                                 id
            //    ^^^^^ ^^^^^^^^ ^^^^^^^^ ^^^^     event
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void ToBinaryShouldReturnEncodedBytesForEventNodeWithLargeNCase17()
        {
            // Arrange
            Stamp s = new Stamp(0, 17);

            // Act
            byte[] bytes = s.ToBinary();

            // Assert
            bytes.ShouldBeEquivalentTo(new byte[] { 0x1C, 0xA0 });
            // 00011100 10100000 = 0x1C 0xA0
            // ^^^               id
            //    ^^^^^ ^^^^     event
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void ToBinaryShouldReturnEncodedBytesForEventNodeWithLargeNCase258()
        {
            // Arrange
            Stamp s = new Stamp(0, 258);

            // Act
            byte[] bytes = s.ToBinary();

            // Assert
            bytes.ShouldBeEquivalentTo(new byte[] { 0x1F, 0xC0, 0xC0 });
            // 00011111 11000000 11000000 = 0x1F 0xC0 0xC0
            // ^^^                        id
            //    ^^^^^ ^^^^^^^^ ^^^      event
        }

        [Test]
        public void ToBinaryShouldReturnEncodedBytesForStampWithIdNode10AndEventNode110()
        {
            // Arrange
            Stamp s = new Stamp(new Id.Node(1, 0), new Event.Node(1, 1, 0));

            // Act
            byte[] bytes = s.ToBinary();

            // Assert
            bytes.ShouldBeEquivalentTo(new byte[] { 0x8B, 0x66, 0x40 });
            // 10001011 01100110 01000000 = 0x8B 66 40
            // ^^^^^                      id
            //      ^^^ ^^^^^^^^ ^^       event
        }

        [Test]
        public void ToBinaryShouldReturnEncodedBytesForStampWithIdNode01AndEventNode110()
        {
            // Arrange
            Stamp s = new Stamp(new Id.Node(0, 1), new Event.Node(1, 1, 0));

            // Act
            byte[] bytes = s.ToBinary();

            // Assert
            bytes.ShouldBeEquivalentTo(new byte[] { 0x4B, 0x66, 0x40 });
            // 01001011 01100110 01000000 = 0x4B 66 40
            // ^^^^^                      id
            //      ^^^ ^^^^^^^^ ^^       event
        }

        [Test]
        public void FromBinaryShouldReturnStampForEncodedSeedStamp()
        {
            // Arrange
            byte[] bytes = { 0x30 };

            // Act
            Stamp s = Stamp.FromBinary(bytes);

            // Assert
            s.Should().Be(new Stamp());
        }

        [Test]
        public void FromBinaryShouldReturnStampForEncodedAnonymousStampWithZeroEventLeaf()
        {
            // Arrange
            byte[] bytes = { 0x10 };

            // Act
            Stamp s = Stamp.FromBinary(bytes);

            // Assert
            s.Should().Be(new Stamp(0, 0));
        }

        [Test]
        public void FromBinaryShouldReturnStampForEncodedIdNode0I()
        {
            // Arrange
            byte[] bytes = { 0x4C, 0x00 };

            // Act
            Stamp s = Stamp.FromBinary(bytes);

            // Assert
            s.Should().Be(new Stamp(new Id.Node(0, 1), 0));
        }

        [Test]
        public void FromBinaryShouldReturnStampForEncodedIdNodeI0()
        {
            // Arrange
            byte[] bytes = { 0x8C, 0x00 };

            // Act
            Stamp s = Stamp.FromBinary(bytes);

            // Assert
            s.Should().Be(new Stamp(new Id.Node(1, 0), 0));
        }

        [Test]
        public void FromBinaryShouldReturnStampForEncodedIdNode1001()
        {
            // Arrange
            byte[] bytes = { 0xE2, 0x98 };

            // Act
            Stamp s = Stamp.FromBinary(bytes);

            // Assert
            s.Should().Be(new Stamp(new Id.Node(new Id.Node(1, 0), new Id.Node(0, 1)), 0));
        }

        [Test]
        public void FromBinaryShouldReturnStampForEncodedEventNode00E()
        {
            // Arrange
            byte[] bytes = { 0x02, 0x40 };

            // Act
            Stamp s = Stamp.FromBinary(bytes);

            // Assert
            s.Should().Be(new Stamp(0, new Event.Node(0, 0, 1)));
        }

        [Test]
        public void FromBinaryShouldReturnStampForEncodedEventNode0E0()
        {
            // Arrange
            byte[] bytes = { 0x06, 0x40 };

            // Act
            Stamp s = Stamp.FromBinary(bytes);

            // Assert
            s.Should().Be(new Stamp(0, new Event.Node(0, 1, 0)));
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void FromBinaryShouldReturnStampForEncodedEventNode0EEWithNestedEventLeaf()
        {
            // Arrange
            byte[] bytes = { 0x0A, 0x64 };

            // Act
            Stamp s = Stamp.FromBinary(bytes);

            // Assert
            s.Should().Be(new Stamp(0, new Event.Node(0, 1, 1)));
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void FromBinaryShouldReturnStampForEncodedEventNode0EEWithNestedEventNode()
        {
            // Arrange
            byte[] bytes = { 0x08, 0xCC, 0x80 };

            // Act
            Stamp s = Stamp.FromBinary(bytes);

            // Assert
            s.Should().Be(new Stamp(0, new Event.Node(0, new Event.Node(0, 1, 0), 1)));
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void FromBinaryShouldReturnStampForEncodedEventNodeN0E()
        {
            // Arrange
            byte[] bytes = { 0x0C, 0x99 };

            // Act
            Stamp s = Stamp.FromBinary(bytes);

            // Assert
            s.Should().Be(new Stamp(0, new Event.Node(1, 0, 1)));
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void FromBinaryShouldReturnStampForEncodedEventNodeNE0()
        {
            // Arrange
            byte[] bytes = { 0x0D, 0x99 };

            // Act
            Stamp s = Stamp.FromBinary(bytes);

            // Assert
            s.Should().Be(new Stamp(0, new Event.Node(1, 1, 0)));
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void FromBinaryShouldReturnStampForEncodedEventNodeNEE()
        {
            // Arrange
            byte[] bytes = { 0x0F, 0x32, 0xDB, 0x90 };

            // Act
            Stamp s = Stamp.FromBinary(bytes);

            // Assert
            s.Should().Be(new Stamp(0, new Event.Node(1, 1, new Event.Node(3, 1, 0))));
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void FromBinaryShouldReturnStampForEncodedEventNodeWithLargeNCase17()
        {
            // Arrange
            byte[] bytes = { 0x1C, 0xA0 };

            // Act
            Stamp s = Stamp.FromBinary(bytes);

            // Assert
            s.Should().Be(new Stamp(0, 17));
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void FromBinaryShouldReturnStampForEncodedEventNodeWithLargeNCase258()
        {
            // Arrange
            byte[] bytes = { 0x1F, 0xC0, 0xC0 };

            // Act
            Stamp s = Stamp.FromBinary(bytes);

            // Assert
            s.Should().Be(new Stamp(0, 258));
        }

        [Test]
        public void FromBinaryShouldReturnStampForEncodedStampWithIdNode10AndEventNode110()
        {
            // Arrange
            byte[] bytes = { 0x8B, 0x66, 0x40 };

            // Act
            Stamp s = Stamp.FromBinary(bytes);

            // Assert
            s.Should().Be(new Stamp(new Id.Node(1, 0), new Event.Node(1, 1, 0)));
        }

        [Test]
        public void FromBinaryShouldReturnStampForEncodedStampWithIdNode01AndEventNode110()
        {
            // Arrange
            byte[] bytes = { 0x4B, 0x66, 0x40 };

            // Act
            Stamp s = Stamp.FromBinary(bytes);

            // Assert
            s.Should().Be(new Stamp(new Id.Node(0, 1), new Event.Node(1, 1, 0)));
        }
    }
}