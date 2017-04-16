using GalaSoft.MvvmLight;
using Ham2.Security;
using System;
using System.Linq;

namespace Ham2.viewmodel
{
    public class SecurityViewModel : ViewModelBase, ISettings
    {
        string _addressService;
        long _expiration;
        string _hash;
        SecureData _secureData;
        string _sharedSecret;
        string _username;

        public SecurityViewModel() => ResetModel();

        public void GenerateHash(string app, string name)
        {
            this.Expiration = Security.Security.ToUnixTime(DateTime.Now) + 3600;

            Security.Security.Expiration = this.Expiration;
            Security.Security.App = app;
            Security.Security.Name = name;
            Security.Security.Secret = this.SharedSecret;
            Security.Security.User = this.Username;

            this.Hash = Security.Security.GetMd5Hash(this.SecureData.GetUnsecuredText());
        }

        public void ResetModel()
        {
            this.SharedSecret = Properties.Security.Default.Secret;
            this.Username = Properties.Security.Default.Username;
            this.SecureData = Properties.Security.Default.SecureData;
            this.AddressService = Properties.Security.Default.IpAddressService;
        }

        public void UpdateSettings()
        {
            Properties.Security.Default.Secret = this.SharedSecret;
            Properties.Security.Default.Username = this.Username;
            Properties.Security.Default.SecureData = this.SecureData;
            Properties.Security.Default.IpAddressService = this.AddressService;
            Properties.Security.Default.Save();
        }

        public string AddressService
        {
            get => this._addressService;
            set
            {
                if (this._addressService == value)
                {
                    return;
                }

                this._addressService = value;
                RaisePropertyChanged();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Security.Security.SetPublicIpAddress(value);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        public long Expiration
        {
            get => this._expiration;
            set
            {
                if (this._expiration == value)
                {
                    return;
                }

                this._expiration = value;
                RaisePropertyChanged();
            }
        }

        public string Hash
        {
            get => this._hash;
            set
            {
                if (this._hash == value)
                {
                    return;
                }

                this._hash = value;
                RaisePropertyChanged();
            }
        }

        public SecureData SecureData
        {
            get => this._secureData;
            set
            {
                if (this._secureData == value)
                {
                    return;
                }

                this._secureData = value;
                RaisePropertyChanged();
            }
        }

        public string SharedSecret
        {
            get => this._sharedSecret;
            set
            {
                if (this._sharedSecret == value)
                {
                    return;
                }

                this._sharedSecret = value;
                RaisePropertyChanged(nameof(SharedSecret));
            }
        }

        public string Username
        {
            get => this._username;
            set
            {
                if (this._username == value)
                {
                    return;
                }

                this._username = value;
                RaisePropertyChanged();
            }
        }
    }
}
