using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace Ham2.Security
{
    public class SecureDataConverter : TypeConverter
    {
        private string BytesToString(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var item in bytes)
            {
                sb.Append($"{item:X2}");
            }

            return sb.ToString();
        }

        private byte[] StringToBytes(string s)
        {
            var bytes = new byte[s.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                var h1 = s[i * 2];
                var h2 = s[i * 2 + 1];
                bytes[i] = (byte)("0123456789ABCDEF".IndexOf(h2) + "0123456789ABCDEF".IndexOf(h1) * 16);
            }
            return bytes;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => sourceType == typeof(string);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!(value is string))
            {
                return base.ConvertFrom(context, culture, value);
            }

            var props = ((string)value).Split(':');
            if (props.Length != 2)
            {
                return null;
            }

            var entropy = StringToBytes(props[0]);
            var cypher = StringToBytes(props[1]);

            return new SecureData(cypher, entropy);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(string))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }

            var data = value as SecureData;
            var entropy = BytesToString(data.Entropy);
            var cypher = BytesToString(data.Cyphertext);
            return $"{entropy}:{cypher}";
        }
    }
}
