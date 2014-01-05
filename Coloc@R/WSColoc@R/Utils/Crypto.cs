using System;
using System.Security.Cryptography;
using System.Text;

namespace WSColocAtR
{
    public partial class Utils
    {
        protected static SHA256Managed Sha256crypter = new SHA256Managed();

        public static string GetUniqueKey()
        {
            int size = int.Parse(WSColocAtR.Properties.Resources.TokenSize);

            string a = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            char[] chars = a.ToCharArray();
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            byte[] data = new byte[size];
            crypto.GetNonZeroBytes(data);
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            { result.Append(chars[b % (chars.Length - 1)]); }
            return result.ToString();
        }

        public static string ToSha256(string str)
        {
            string hash = String.Empty;
            byte[] crypto = Sha256crypter.ComputeHash(Encoding.UTF8.GetBytes(str), 0, Encoding.UTF8.GetByteCount(str));
            foreach (byte bit in crypto)
            {
                hash += bit.ToString("x2");
            }
            return hash;
        }

    }
}