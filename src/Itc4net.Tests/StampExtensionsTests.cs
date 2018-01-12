using System;
using FluentAssertions;
using NUnit.Framework;

namespace Itc4net.Tests
{
    [TestFixture]
    public class StampExtensionsTests
    {
        [Test]
        public void SendShouldReturnInflatedStampAsReturnValue()
        {
            // Arrange
            Stamp s = new Stamp();

            // Act
            Stamp _;
            Stamp returnValue = s.Send(out _);

            // Assert
            returnValue.Should().Be(s.Event());
        }

        [Test]
        public void SendShouldReturnInflatedAnonymousStampAsOutParameter()
        {
            // Arrange
            Stamp s = new Stamp();

            // Act
            Stamp outParameter;
            s.Send(out outParameter);

            // Assert
            outParameter.IsAnonymous.Should().BeTrue();
        }

        [Test]
        public void EquivalentShouldReturnTrueWhenStampsHaveEqualEvents()
        {
            Stamp a = ((1, 0), (0, 1, 0));
            Stamp b = ((0, 1), (0, 1, 0));

            a.Equivalent(b).Should().BeTrue();
        }

        [Test]
        public void EquivalentShouldReturnFalseWhenStampsHaveConcurrentEvents()
        {
            Stamp a = ((1, 0), (0, 1, 0));
            Stamp b = ((0, 1), (0, 0, 1));

            a.Equivalent(b).Should().BeFalse();
        }

        [Test]
        public void EquivalentShouldReturnFalseWhenOneStampDominatesTheOther()
        {
            Stamp a = ((1, 0), (0, 1, 0));
            Stamp b = a.Event();

            a.Equivalent(b).Should().BeFalse();
        }
    }
}
