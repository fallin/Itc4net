using System;
using System.ComponentModel;
using System.Globalization;

namespace Itc4net
{
    /// <summary>
    /// The <see cref="T:Itc4net.StampConverter" /> class is used to convert from one data type to 
    /// another. Access this class through the <see cref="T:System.ComponentModel.TypeDescriptor" /> object.
    /// </summary>
    public class StampConverter : TypeConverter
    {
        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc/>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string text = value as string;
            if (text != null)
            {
                return Stamp.Parse(text);
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
