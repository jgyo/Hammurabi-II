using System;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace Ham2.Security
{
    [TypeConverter(typeof(SecureDataConverter))]
    [SettingsSerializeAs(SettingsSerializeAs.ProviderSpecific)]
    public class SecureData : ISerializable, IEquatable<SecureData>
    {
        readonly byte[] _cyphertext;
        readonly byte[] _entropy;

        public SecureData(string unsecuredText)
        {
            byte[] unsecuredBytes = Encoding.UTF8.GetBytes(unsecuredText);
            this._entropy = new byte[20];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(this.Entropy);
            }

            this._cyphertext = ProtectedData.Protect(unsecuredBytes, this.Entropy, DataProtectionScope.CurrentUser);
        }

        public SecureData(SerializationInfo info, StreamingContext context)
        {
            this._cyphertext = (byte[])info.GetValue(nameof(this._cyphertext), typeof(byte[]));
            this._entropy = (byte[])info.GetValue(nameof(this._entropy), typeof(byte[]));
        }

        public SecureData(byte[] cypher, byte[] entropy)
        {
            this._cyphertext = cypher;
            this._entropy = entropy;
        }

        public static bool operator !=(SecureData first, SecureData second) => !(first == second);

        public static bool operator ==(SecureData first, SecureData second)
        {
            if ((object)first == null)
            {
                return (object)second == null;
            }

            return first.Equals(second);
        }

        public override bool Equals(object obj)
        {
            if (obj is SecureData)
            {
                return Equals((SecureData)obj);
            }

            return object.ReferenceEquals(this, obj);
        }

        public bool Equals(SecureData other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (this._cyphertext.Length != other._cyphertext.Length ||
                this._entropy.Length != other._entropy.Length)
            {
                return false;
            }

            for (int i = 0; i < this._cyphertext.Length; i++)
            {
                if (other._cyphertext[i] != this._cyphertext[i])
                {
                    return false;
                }
            }

            for (int i = 0; i < this._entropy.Length; i++)
            {
                if (other._entropy[i] != this._entropy[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 47;
                if (this._cyphertext != null)
                {
                    hashCode = (hashCode * 53) ^ this._cyphertext.GetHashCode();
                }

                if (this._entropy != null)
                {
                    hashCode = (hashCode * 53) ^ this._entropy.GetHashCode();
                }

                return hashCode;
            }
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this._cyphertext), this._cyphertext);
            info.AddValue(nameof(this._entropy), this._entropy);
        }

        public string GetUnsecuredText()
        {
            try
            {
                var plaintext = ProtectedData.Unprotect(this._cyphertext, this._entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plaintext);
            }
            catch
            {
                return string.Empty;
            }
        }

        public byte[] Cyphertext { get => this._cyphertext; }

        public byte[] Entropy { get => this._entropy; }
    }
}
