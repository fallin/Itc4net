using System;
using System.ComponentModel;
using FluentAssertions;
using NUnit.Framework;

namespace Itc4net.Tests
{
    [TestFixture]
    public class StampConverterTests
    {
        [Test]
        public void StampShouldDeclareTypeConverter()
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(Stamp));
            converter.Should().BeOfType<StampConverter>();
        }

        [Test]
        public void CanConvertToShouldReturnTrueWhenDestinationTypeIsString()
        {
            // Note, this is default TypeConverter behavior
            var converter = new StampConverter();
            converter.CanConvertTo(typeof(string)).Should().BeTrue();
        }

        [Test]
        public void CanConvertToShouldReturnFalseWhenDestinationTypeIsUnsupported()
        {
            // Note, this is default TypeConverter behavior
            var converter = new StampConverter();
            converter.CanConvertTo(typeof(int)).Should().BeFalse();
        }

        [Test]
        public void ConvertToShouldEqualStampStringRepresentationWhenDestinationTypeIsString()
        {
            // Note, this is default TypeConverter behavior (will call value.ToString method)
            Stamp stamp = new Stamp(((1, 0), 0), (0, (1, 1, 0), 0));
            var converter = new StampConverter();

            string result = (string) converter.ConvertTo(stamp, typeof(string));

            result.Should().Be(stamp.ToString());
            result.Should().Be("(((1,0),0),(0,(1,1,0),0))");
        }

        [Test]
        public void ConvertToShouldThrowWhenDestinationTypeIsUnsupported()
        {
            // Note, this is default TypeConverter behavior
            Stamp stamp = new Stamp(((1, 0), 0), (0, (1, 1, 0), 0));
            var converter = new StampConverter();

            Action act = () => converter.ConvertTo(stamp, typeof(int));
            act.ShouldThrow<NotSupportedException>();
        }

        [Test]
        public void CanConvertFromShouldReturnTrueWhenSourceTypeIsString()
        {
            var converter = new StampConverter();
            converter.CanConvertFrom(typeof(string)).Should().BeTrue();
        }

        [Test]
        public void CanConvertFromShouldReturnFalseWhenSourceTypeIsUnsupported()
        {
            // Note, this is default TypeConverter behavior
            var converter = new StampConverter();
            converter.CanConvertFrom(typeof(int)).Should().BeFalse();
        }

        [Test]
        public void ConvertFromShouldParseString()
        {
            var converter = new StampConverter();
            Stamp result = (Stamp) converter.ConvertFrom("(((1,0),0),(0,(1,1,0),0))");

            result.Should().Be(new Stamp(((1, 0), 0), (0, (1, 1, 0), 0)));
        }

        [Test]
        public void IsValidShouldReturnTrueForLegitimateStampString()
        {
            // Note, this is default TypeConverter behavior
            var converter = new StampConverter();
            bool result = converter.IsValid("(((1,0),0),(0,(1,1,0),0))");

            result.Should().BeTrue();
        }

        [Test]
        public void IsValidShouldReturnFalseForMalformedStampString()
        {
            // Note, this is default TypeConverter behavior
            var converter = new StampConverter();
            bool result = converter.IsValid("(((1,0),0),(0,(1,1,0);0))");
            //                                                    ^--- invalid char

            result.Should().BeFalse();
        }
    }
}
