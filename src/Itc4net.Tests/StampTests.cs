using System;
using FluentAssertions;
using NUnit.Framework;

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
    }
}