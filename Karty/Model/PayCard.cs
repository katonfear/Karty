using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Karty.Model
{
    public class PayCard
    {
        public string? Id { get; set; }
        public string? AccountNumber { get; set; }
        public string? SerialNumber { get; set; }
        public string? Pin { get; set; }
        [JsonIgnore]
        public byte[]? PinEncrypt { get { return EncryptPin(Pin); } set { Pin = DecryptPin(value); } }

        public override string ToString()
        {
            return $"Identyfikator karty: {Id}, Numer konta: {AccountNumber}, Numer seryjny: {SerialNumber}";
        }

        [JsonIgnore]
        protected static string key = "g58vv2zi7rzfq95bi8hs1f4i7gh4ex";
        [JsonIgnore]
        protected static byte[] IV = { 0x59, 0x12, 0x71, 0x34, 0xE6, 0x17, 0x51, 0x37, 0x97, 0x84, 0x19, 0x99, 0xA3, 0x7E, 0xC9, 0x01 };
        [JsonIgnore]
        protected static byte[] salt = { 0x35, 0x85, 0xA1, 0x54, 0xB6, 0xF7, 0x1F, 0x9C, 0x4A, 0x7F, 0x21, 0xAD, 0xEA, 0xFF, 0x25, 0x5A, 0xEF, 0x3D, 0x1F, 0x99, 0x3F, 0xAA, 0xA6, 0xF1, 0xCC, 0xC7, 0x5A, 0xF2 };

        public static byte[]? EncryptPin(string? message)
        {
            try
            {
                byte[]? encrypted;

                using (Aes aes = Aes.Create())
                {
                    int myIterations = 1000;
                    Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(PayCard.key, salt, myIterations, HashAlgorithmName.SHA256);
                    aes.Key = key.GetBytes(16);
                    aes.IV = IV;
                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(message);
                            }
                            encrypted = msEncrypt.ToArray();
                        }
                    }
                }
                return encrypted;
            }
            catch
            {
                return null;
            }
        }

        public static string? DecryptPin(byte[]? encrypted)
        {
            try
            {

                string? ret = null;

                using (Aes aes = Aes.Create())
                {
                    int myIterations = 1000;
                    Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(PayCard.key, salt, myIterations, HashAlgorithmName.SHA256);
                    aes.Key = key.GetBytes(16);
                    aes.IV = IV;
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    using (MemoryStream msDecrypt = new MemoryStream())
                    {
                        if (encrypted != null)
                        {
                            msDecrypt.Write(encrypted);
                            msDecrypt.Position = 0;
                            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                            {
                                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                                {
                                    ret = srDecrypt.ReadToEnd();
                                }
                            }
                        }
                    }
                }
                return ret;
            }
            catch 
            {
                return null;
            }
        }
    }
}
