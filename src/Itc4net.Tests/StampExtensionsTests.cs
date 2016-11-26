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
    }
}
