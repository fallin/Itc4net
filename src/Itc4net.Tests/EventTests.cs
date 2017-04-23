using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Itc4net.Tests
{
    [TestFixture]
    public class EventTests
    {
        [Test]
        public void ToStringShouldCorrectlyRepresentLeafWith0()
        {
            new Event.Leaf(0).ToString().Should().Be("0");
        }

        [Test]
        public void ToStringShouldCorrectlyRepresentNode()
        {
            string s = new Event.Node(0, 3, new Event.Node(1, 2, 3)).ToString();
            s.Should().Be("(0,3,(1,2,3))");
        }

        [Test]
        public void LeafCtorShouldAcceptPositiveN()
        {
            new Event.Leaf(0);
            new Event.Leaf(1);
            new Event.Leaf(5);
        }

        [Test]
        public void LeafCtorShouldThrowWhenNegativeN()
        {
            Action act = () => new Event.Leaf(-1);
            act.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Test]
        public void NodeCtorShouldAcceptPositiveN()
        {
            new Event.Node(0, 0, 0);
            new Event.Node(1, 0, 0);
            new Event.Node(5, 0, 0);
        }

        [Test]
        public void NodeCtorShouldThrowWhenNegativeN()
        {
            Action act = () => new Event.Node(-1, 0, 0);
            act.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Test]
        public void NodeCtorShouldThrowWhenLeftEventNull()
        {
            Action act = () => new Event.Node(0, null, 0);
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void NodeCtorShouldThrowWhenRightEventNull()
        {
            Action act = () => new Event.Node(0, 0, null);
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void MatchShouldInvokeLeafFuncWhenNodeRepresentsLeaf()
        {
            // Arrange
            Event e = new Event.Leaf(0);

            Func<int, object> leafFn = Substitute.For<Func<int, object>>();
            Func<int, Event, Event, object> nodeFn = Substitute.For<Func<int, Event, Event, object>>();

            // Act
            e.Match(leafFn, nodeFn);

            // Assert
            leafFn.Received().Invoke(0);
            nodeFn.DidNotReceiveWithAnyArgs();
        }

        [Test]
        public void MatchShouldInvokeNodeFuncWhenEventRepresentsNode()
        {
            // Arrange
            Event e = new Event.Node(0, 1, 2);

            Func<int, object> leafFn = Substitute.For<Func<int, object>>();
            Func<int, Event, Event, object> nodeFn = Substitute.For<Func<int, Event, Event, object>>();

            // Act
            e.Match(leafFn, nodeFn);

            // Assert
            leafFn.DidNotReceiveWithAnyArgs();
            nodeFn.Received().Invoke(0, 1, 2);
        }

        [Test]
        public void MatchShouldInvokeLeafActionWhenEventRepresentsLeaf()
        {
            // Arrange
            Event e = new Event.Leaf(0);

            Action<int> leafAction = Substitute.For<Action<int>>();
            Action<int, Event, Event> nodeAction = Substitute.For<Action<int, Event, Event>>();

            // Act
            e.Match(leafAction, nodeAction);

            // Assert
            leafAction.Received().Invoke(0);
            nodeAction.DidNotReceiveWithAnyArgs();
        }

        [Test]
        public void MatchShouldInvokeNodeActionWhenEventRepresentsNode()
        {
            // Arrange
            Event e = new Event.Node(0, 1, 2);

            Action<int> leafAction = Substitute.For<Action<int>>();
            Action<int, Event, Event> nodeAction = Substitute.For<Action<int, Event, Event>>();

            // Act
            e.Match(leafAction, nodeAction);

            // Assert
            leafAction.DidNotReceiveWithAnyArgs();
            nodeAction.Received().Invoke(0, 1, 2);
        }

        [Test]
        public void EqualsShouldReturnTrueWhenComparingLeafsWithSameValues()
        {
            // Arrange
            Event e1 = new Event.Leaf(0);
            Event e2 = new Event.Leaf(0);

            // Act & Assert
            e1.Equals(e2).Should().BeTrue();
        }

        [Test]
        public void EqualsShouldReturnFalseWhenComparingLeafsWithDifferentValues()
        {
            // Arrange
            Event e1 = new Event.Leaf(0);
            Event e2 = new Event.Leaf(1);

            // Act & Assert
            e1.Equals(e2).Should().BeFalse();
        }

        [Test]
        public void EqualsShouldReturnTrueWhenComparingNodesWithSameValues()
        {
            // Arrange
            Event e1 = new Event.Node(0, 1, 2);
            Event e2 = new Event.Node(0, 1, 2);

            // Act & Assert
            e1.Equals(e2).Should().BeTrue();
        }

        [Test]
        public void EqualsShouldReturnFalseWhenComparingNodesWithDifferentN()
        {
            // Arrange
            Event e1 = new Event.Node(0, 1, 2);
            Event e2 = new Event.Node(9, 1, 2);

            // Act & Assert
            e1.Equals(e2).Should().BeFalse();
        }

        [Test]
        public void EqualsShouldReturnFalseWhenComparingNodesWithDifferentL()
        {
            // Arrange
            Event e1 = new Event.Node(0, 1, 2);
            Event e2 = new Event.Node(0, 9, 2);

            // Act & Assert
            e1.Equals(e2).Should().BeFalse();
        }

        [Test]
        public void EqualsShouldReturnFalseWhenComparingNodesWithDifferentR()
        {
            // Arrange
            Event e1 = new Event.Node(0, 1, 2);
            Event e2 = new Event.Node(0, 1, 9);

            // Act & Assert
            e1.Equals(e2).Should().BeFalse();
        }

        [Test]
        public void EqualsShouldReturnTrueWhenComparingComplexNodesWithSameValues()
        {
            // Arrange
            Event e1 = new Event.Node(0, new Event.Node(3, 4, 5), new Event.Node(6, 7, 8));
            Event e2 = new Event.Node(0, new Event.Node(3, 4, 5), new Event.Node(6, 7, 8));

            // Act & Assert
            e1.Equals(e2).Should().BeTrue();
        }

        [Test]
        public void EqualsShouldReturnTrueWhenComparingComplexNodesWithDifferentValues()
        {
            // Arrange
            Event e1 = new Event.Node(0, new Event.Node(3, 4, 5), new Event.Node(6, 7, 8));
            Event e2 = new Event.Node(0, new Event.Node(3, 5, 4), new Event.Node(6, 7, 8));

            // Act & Assert
            e1.Equals(e2).Should().BeFalse();
        }

        [Test]
        public void GetHashCodeShouldMatchWhenLeafsHaveSameValues()
        {
            // Arrange
            Event e1 = new Event.Leaf(0);
            Event e2 = new Event.Leaf(0);

            // Act
            int hash1 = e1.GetHashCode();
            int hash2 = e2.GetHashCode();

            e1.Equals(e2).Should().BeTrue();
        }

        [Test]
        public void GetHashCodeShouldNotMatchWhenLeafsHaveDifferentValues()
        {
            // Arrange
            Event e1 = new Event.Leaf(0);
            Event e2 = new Event.Leaf(4);

            // Act
            int hash1 = e1.GetHashCode();
            int hash2 = e2.GetHashCode();

            e1.Equals(e2).Should().BeFalse();
        }

        [Test]
        public void GetHashCodeShouldMatchWhenNodesHaveSameValues()
        {
            // Arrange
            Event e1 = new Event.Node(0, 1, 2);
            Event e2 = new Event.Node(0, 1, 2);

            // Act
            int hash1 = e1.GetHashCode();
            int hash2 = e2.GetHashCode();

            e1.Equals(e2).Should().BeTrue();
        }

        [Test]
        public void GetHashCodeShouldNotMatchWhenNodesHaveDifferentValues()
        {
            // Arrange
            Event e1 = new Event.Node(0, 1, 2);
            Event e2 = new Event.Node(0, 2, 2);

            // Act
            int hash1 = e1.GetHashCode();
            int hash2 = e2.GetHashCode();

            e1.Equals(e2).Should().BeFalse();
        }

        [Test]
        public void GetHashCodeShouldMatchWhenComplexNodesHaveSameValues()
        {
            // Arrange
            Event e1 = new Event.Node(0, new Event.Node(3, 4, 5), new Event.Node(6, 7, 8));
            Event e2 = new Event.Node(0, new Event.Node(3, 4, 5), new Event.Node(6, 7, 8));

            // Act
            int hash1 = e1.GetHashCode();
            int hash2 = e2.GetHashCode();

            e1.Equals(e2).Should().BeTrue();
        }

        [Test]
        public void GetHashCodeShouldMatchWhenComplexNodesHaveDifferentValues()
        {
            // Arrange
            Event e1 = new Event.Node(0, new Event.Node(3, 4, 5), new Event.Node(6, 7, 8));
            Event e2 = new Event.Node(0, new Event.Node(3, 4, 6), new Event.Node(6, 7, 8));

            // Act
            int hash1 = e1.GetHashCode();
            int hash2 = e2.GetHashCode();

            e1.Equals(e2).Should().BeFalse();
        }

        [Test(Description = "lift := n↑m = n+m")]
        public void LiftShouldReturnDefinedValueForLeaf()
        {
            // Arrange
            int n = 2;
            int m = 1;
            Event e = new Event.Leaf(n);

            // Act
            Event result = e.Lift(m);

            // Assert
            result.Should().Be(new Event.Leaf(n+m));
        }

        [Test(Description = "lift := (n,e1,e2)↑m = (n+m,e1,e2)")]
        public void LiftShouldReturnDefinedValueForNode()
        {
            // Arrange
            int n = 2;
            int m = 1;
            Event e = new Event.Node(n, 8, 9);

            // Act
            Event result = e.Lift(m);

            // Assert
            result.Should().Be(new Event.Node(n + m, 8, 9));
        }

        [Test(Description = "sink := n↓m = n-m")]
        public void SinkShouldReturnDefinedValueForLeaf()
        {
            // Arrange
            int n = 2;
            int m = 1;
            Event e = new Event.Leaf(n);

            // Act
            Event result = e.Sink(m);

            // Assert
            result.Should().Be(new Event.Leaf(n - m));
        }

        [Test(Description = "sink := (n,e1,e2)↓m = (n-m,e1,e2)")]
        public void SinkShouldReturnDefinedValueForNode()
        {
            // Arrange
            int n = 2;
            int m = 1;
            Event e = new Event.Node(n, 8, 9);

            // Act
            Event result = e.Sink(m);

            // Assert
            result.Should().Be(new Event.Node(n - m, 8, 9));
        }

        [Test]
        public void SinkShouldCascadeToSubTreeWhenSinkingGreaterThanN()
        {
            // Arrange
            Event e = new Event.Node(2, 3, 4);

            // Act
            Event result = e.Sink(3);

            // Assert
            result.Should().Be(new Event.Node(0, 2, 3));
        }

        [Test(Description = "norm(n) = n")]
        public void NormalizeShouldReturnNWhenLeafIsN()
        {
            // Arrange
            Event e = new Event.Leaf(2);

            // Act
            Event result = e.Normalize();

            // Assert
            result.Should().Be(new Event.Leaf(2));
        }

        [Test(Description = "norm((n,m,m)) = n+m")]
        public void NormalizeShouldReturnLeafWithNPlusMWhenNodeHasSameLeftAndRightEvents()
        {
            // Arrange
            Event e = new Event.Node(2, 3, 3);

            // Act
            Event result = e.Normalize();

            // Assert
            result.Should().Be(new Event.Leaf(5));
        }

        [Test(Description = "norm((n,e1,e2)) = (n+m,e1↓m,e2↓m), where m=min(min(e1),min(e2)) for leaf")]
        public void NormalizeShouldReturnNormalizedTreeForNodeWithLeftAndRightLeafs()
        {
            // Arrange
            Event e = new Event.Node(1, 2, 3);

            // Act
            Event result = e.Normalize();

            // Assert
            result.Should().Be(new Event.Node(3, 0, 1));
        }

        [Test(Description = "norm((n,e1,e2)) = (n+m,e1↓m,e2↓m), where m=min(min(e1),min(e2)) for node")]
        public void NormalizeShouldReturnNormalizedTreeForNodeWithLeftNodeAndRightLeaf()
        {
            // This is an example from the ITC paper which assumes events are always
            // normalized (or start as normalized). In this, the tree is not normalized, but
            // the sub-trees are.

            // Arrange
            Event e = new Event.Node(2, new Event.Node(2, 1, 0), 3);

            // Act
            Event result = e.Normalize();

            // Assert
            result.Should().Be(new Event.Node(4, new Event.Node(0,1,0), 1));
        }

        [Test]
        public void NormalizeShouldReturnRecursivelyNormalizedEventTree()
        {
            // This event tree, and sub-trees, are not normalized which recursively normalized.

            // Arrange
            Event e = new Event.Node(1, new Event.Node(2, 3, 4), new Event.Node(1, 2, 4));

            // Act
            Event result = e.Normalize();

            // Assert
            result.Should().Be(new Event.Node(4, new Event.Node(2, 0, 1), new Event.Node(0, 0, 2)));
        }

        [Test(Description = "min(n) = n")]
        public void MinShouldReturnNWhenLeafN()
        {
            // Arrange
            Event e = new Event.Leaf(3);

            // Act & Assert
            e.Min().Should().Be(3);
        }

        [Test(Description = "min((n,e1,e2)) = n+min(min(e1),min(e2))")]
        public void MinShouldReturnNPlusMinOfLeftAndRightEvents()
        {
            // Arrange
            Event e = new Event.Node(1, 2, 3);

            // Act & Assert
            e.Min().Should().Be(1 + 2);
        }

        [Test(Description = "max(n) = n")]
        public void MaxShouldReturnNWhenLeafN()
        {
            // Arrange
            Event e = new Event.Leaf(3);

            // Act & Assert
            e.Max().Should().Be(3);
        }

        [Test(Description = "max((n,e1,e2)) = n+max(max(e1),max(e2))")]
        public void MaxShouldReturnNPlusMaxOfLeftAndRightEvents()
        {
            // Arrange
            Event e = new Event.Node(1, 2, 3);

            // Act & Assert
            e.Max().Should().Be(1 + 3);
        }

        [Test(Description = "leq(n1,n2) = n1<=n2, case n1 < n2")]
        public void LeqShouldReturnTrueWhenN1IsLessThenN2()
        {
            // Arrange
            Event e1 = new Event.Leaf(1);
            Event e2 = new Event.Leaf(2);

            // Act & Assert
            e1.Leq(e2).Should().BeTrue();
        }

        [Test(Description = "leq(n1,n2) = n1<=n2, case n1 == n2")]
        public void LeqShouldReturnTrueWhenN1EqualsN2()
        {
            // Arrange
            Event e1 = new Event.Leaf(1);
            Event e2 = new Event.Leaf(1);

            // Act & Assert
            e1.Leq(e2).Should().BeTrue();
        }

        [Test(Description = "leq(n1,n2) = n1<=n2, case n1 > n2")]
        public void LeqShouldReturnFalseWhenN1IsGreaterThanN2()
        {
            // Arrange
            Event e1 = new Event.Leaf(2);
            Event e2 = new Event.Leaf(1);

            // Act & Assert
            e1.Leq(e2).Should().BeFalse();
        }

        [Test(Description = "leq(n1,(n2,l2,r2)) = n1<=n2")]
        public void LeqShouldReturnTrueWhenLeafN1IsLessThanNodeN2()
        {
            // Arrange
            Event e1 = 1;
            Event e2 = new Event.Node(2, 0, 0);

            // Act & Assert
            e1.Leq(e2).Should().BeTrue();
        }

        [Test(Description = "leq((n1,l1,r2),n2) = n1<=n2 Λ leq(l1↑n1,n2) Λ leq(r1↑n1,n2), case n1 < n2")]
        public void LeqShouldReturnTrueWhenNodeN1IsLessThanLeafN2()
        {
            // Arrange
            Event e1 = new Event.Node(2, 0, 0);
            Event e2 = new Event.Leaf(3);

            // Act & Assert
            e1.Leq(e2).Should().BeTrue();
        }

        [Test(Description = "leq((n1,l1,r2),n2) = n1<=n2 Λ leq(l1↑n1,n2) Λ leq(r1↑n1,n2), case n1+l1<n2")]
        public void LeqShouldReturnTrueWhenNodeN1PlusL1IsLessThanOrEqualToLeafN2()
        {
            // Arrange
            Event e1 = new Event.Node(2, 1, 0);
            Event e2 = new Event.Leaf(3);

            // Act & Assert
            e1.Leq(e2).Should().BeTrue();
        }

        [Test(Description = "leq((n1,l1,r2),n2) = n1<=n2 Λ leq(l1↑n1,n2) Λ leq(r1↑n1,n2), case n1+r1<n2")]
        public void LeqShouldReturnTrueWhenNodeN1PlusR1IsLessThanOrEqualToLeafN2()
        {
            // Arrange
            Event e1 = new Event.Node(2, 0, 1);
            Event e2 = new Event.Leaf(3);

            // Act & Assert
            e1.Leq(e2).Should().BeTrue();
        }

        [Test(Description = "leq((n1,l1,r2),n2) = n1<=n2 Λ leq(l1↑n1,n2) Λ leq(r1↑n1,n2), case n1+l1>n2")]
        public void LeqShouldReturnFalseWhenNodeN1PlusL1IsGreaterThanLeafN2()
        {
            // Arrange
            Event e1 = new Event.Node(2, 2, 0);
            Event e2 = new Event.Leaf(3);

            // Act & Assert
            e1.Leq(e2).Should().BeFalse();
        }

        [Test(Description = "leq((n1,l1,r2),n2) = n1<=n2 Λ leq(l1↑n1,n2) Λ leq(r1↑n1,n2), case n1+r1>n2")]
        public void LeqShouldReturnFalseWhenNodeN1PlusR1IsGreaterThanLeafLeafN2()
        {
            // Arrange
            Event e1 = new Event.Node(2, 0, 2);
            Event e2 = new Event.Leaf(3);

            // Act & Assert
            e1.Leq(e2).Should().BeFalse();
        }

        [Test(Description = "leq((n1,l1,r2),(n2,l2,r2)) = n1<=n2 Λ leq(l1↑n1,l2↑n2) Λ leq(r1↑n1,r2↑n2), case n1=n2, l1<l2")]
        public void LeqShouldReturnTrueWhenNodeN1EqualsNodeN2()
        {
            // Arrange
            Event e1 = new Event.Node(2, 0, 0);
            Event e2 = new Event.Node(2, 0, 0);

            // Act & Assert
            e1.Leq(e2).Should().BeTrue();
        }

        [Test(Description = "leq((n1,l1,r2),(n2,l2,r2)) = n1<=n2 Λ leq(l1↑n1,l2↑n2) Λ leq(r1↑n1,r2↑n2), case n1=n2, l1<l2")]
        public void LeqShouldReturnTrueWhenNodeN1EqualsNodeN2AndL1LessThanL2()
        {
            // Arrange
            Event e1 = new Event.Node(2, 2, 0);
            Event e2 = new Event.Node(2, 2, 0);

            // Act & Assert
            e1.Leq(e2).Should().BeTrue();
        }

        [Test(Description = "leq((n1,l1,r2),(n2,l2,r2)) = n1<=n2 Λ leq(l1↑n1,l2↑n2) Λ leq(r1↑n1,r2↑n2), case n1=n2, l1==l2")]
        public void LeqShouldReturnTrueWhenNodeN1EqualsNodeN2AndL1EqualsL2()
        {
            // Arrange
            Event e1 = new Event.Node(2, 2, 0);
            Event e2 = new Event.Node(2, 2, 0);

            // Act & Assert
            e1.Leq(e2).Should().BeTrue();
        }

        [Test(Description = "leq((n1,l1,r2),(n2,l2,r2)) = n1<=n2 Λ leq(l1↑n1,l2↑n2) Λ leq(r1↑n1,r2↑n2), case n1=n2, l1>l2")]
        public void LeqShouldReturnFalseWhenNodeN1EqualsNodeN2AndL1GreaterThanL2()
        {
            // Arrange
            Event e1 = new Event.Node(2, 3, 0);
            Event e2 = new Event.Node(2, 2, 0);

            // Act & Assert
            e1.Leq(e2).Should().BeFalse();
        }

        [Test(Description = "join(n1,n2) = max(n1,n2)")]
        public void JoinShouldReturnMaxNWhenBothLeafs()
        {
            // Arrange
            Event e1 = new Event.Leaf(1);
            Event e2 = new Event.Leaf(2);

            // Act
            Event result = e1.Join(e2);

            // Assert
            result.Should().Be(new Event.Leaf(2));
        }

        [Test(Description = "join(n1,(n2,l2,r2)) = join((n1,0,0),(n2,l2,r2))")]
        public void JoinShouldReturnDefinedValueWhenLeafN1AndNodeN2()
        {
            // Arrange
            Event e1 = new Event.Leaf(1);
            Event e2 = new Event.Node(2, 0, 1);

            // Act
            Event result = e1.Join(e2);

            // Assert
            result.Should().Be(new Event.Node(2,0,1));
        }

        [Test(Description = "join((n1,l1,r1),n2) = join((n1,l1,r1),(n2,0,0))")]
        public void JoinShouldReturnDefinedValueWhenNodeN1AndLeafN2()
        {
            // Arrange
            Event e1 = new Event.Node(2, 0, 1);
            Event e2 = new Event.Leaf(1);

            // Act
            Event result = e1.Join(e2);

            // Assert
            result.Should().Be(new Event.Node(2, 0, 1));
        }

        [Test(Description = "join((n1,l1,r1),(n2,l2,r2)) = join((n2,l2,r2),(n1,l1,r1)), if n1>n2")]
        public void JoinShouldReturnDefinedValueWhenNodeN1AndNodeN2WithN1GreaterThanN2()
        {
            // Arrange
            Event e1 = new Event.Node(2, 0, 1);
            Event e2 = new Event.Node(1, 1, 0);

            // Act
            Event result = e1.Join(e2);

            // Assert
            result.Should().Be(new Event.Node(2, 0, 1));
        }

        [Test(Description = "join((n1,l1,r1),(n2,l2,r2)) = norm((n1,join(l1,l2↑n2-n1),join(r1,r2↑n2-n1)))")]
        public void JoinShouldReturnDefinedValueWhenNodeN1AndNodeN2WithN1LessThanN2()
        {
            // Arrange
            Event e1 = new Event.Node(1, 2, 0);
            Event e2 = new Event.Node(3, 1, 3);

            // Act
            Event result = e1.Join(e2);

            // Assert
            result.Should().Be(new Event.Node(4, 0, 2));
        }

        [Test]
        public void ImplicitConversionOperatorShouldReturnLeafWhenInteger()
        {
            Event e1 = new Event.Leaf(4);
            Event e2 = 4;

            e1.Equals(e2).Should().BeTrue();
        }

        [Test]
        public void ImplicitConversionOperatorShouldReturnNodeWhenTuple()
        {
            Event e1 = new Event.Node(0, new Event.Node(1, 1, 0), 0);
            Event e2 = (0, (1, 1, 0), 0); // combo of C#7 tuples and implicit conversion operator is wow!

            e1.Equals(e2).Should().BeTrue();
        }
    }
}