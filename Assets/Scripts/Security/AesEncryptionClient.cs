using System;
using System.Text;
using System.Security.Cryptography;

public static class AesEncryptionClient
{
    public static string EncryptAES(string plainText, byte[] key, byte[] iv)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            using (var encryptor = aes.CreateEncryptor())
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                return Convert.ToBase64String(encrypted);
            }
        }
    }

    public static string DecryptAES(string cipherBase64, byte[] key, byte[] iv)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            using (var decryptor = aes.CreateDecryptor())
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherBase64);
                byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
    }
}
