using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ham2.Security
{
    /// <summary>
    /// Implements the Security class.
    /// </summary>
    public static class Security
    {
        private static HttpClient _client = new HttpClient();
        private static DateTime _unixTimeBase = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Event handler called when the Ip Address is acquired.
        /// </summary>
        public static event EventHandler IpAddressAquired;

        /// <summary>
        /// Raises Ip Address is aquired event.
        /// </summary>
        static void OnIpAddressAquired()
        {
            EventHandler t = IpAddressAquired;
            t?.Invoke(null, new EventArgs());
        }

        /// <summary>
        /// Returns the Md5 hash of the input string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The Md5 hash of the input string.</returns>
        static string ToMd5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                var s = new StringBuilder();
                foreach (var item in data)
                {
                    s.Append($"{item:X2}");
                }

                return s.ToString();
            }
        }

        /// <summary>
        /// Gets this instance's Md5 hash given the password supplied.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns>This instance's Md5 hash.</returns>
        public static string GetMd5Hash(string password)
        {
            if (string.IsNullOrEmpty(User) || string.IsNullOrEmpty(App) ||
                string.IsNullOrEmpty(IpAddress) || string.IsNullOrEmpty(Name) ||
                string.IsNullOrEmpty(Secret))
            {
                throw new InvalidOperationException();
            }

            return ToMd5Hash($"{User}{Expiration}{App}{IpAddress}{Name}{Secret}{password}");
        }

        /// <summary>
        /// Determines and sets the current public IP address of the computr given the address service.
        /// </summary>
        /// <param name="addressService">The address service.</param>
        /// <returns>The void Task.</returns>
        public static async Task SetPublicIpAddress(string addressService)
        {
            try
            {
                var responce = await _client.GetAsync(addressService);
                var content = await responce.Content.ReadAsStringAsync();
                IpAddress = content;
                OnIpAddressAquired();
            }
            catch
            {
                IpAddress = "Invalid";
            }
        }

        /// <summary>
        /// Converts the given DateTime to Unix time.
        /// </summary>
        /// <param name="dt">The DateTime to convert.</param>
        /// <returns>The Unix time.</returns>
        public static long ToUnixTime(DateTime dt)
        {
            try
            {
                return (long)(TimeZoneInfo.ConvertTimeToUtc(dt) - _unixTimeBase).TotalSeconds;
            }
            catch
            {
                throw new ArgumentOutOfRangeException(nameof(dt));
            }
        }

        /// <summary>
        /// Gets or sets the App.
        /// </summary>
        public static string App { get; set; }

        /// <summary>
        /// Gets or sets the Expiration.
        /// </summary>
        public static long Expiration
        {
            get; set;
        }

        /// <summary>
        /// Gets the Ip Address.
        /// </summary>
        public static string IpAddress
        {
            get; private set;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public static string Name { get; set; }

        /// <summary>
        /// Gets or sets the secret.
        /// </summary>
        public static string Secret { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public static string User { get; set; }
    }
}
