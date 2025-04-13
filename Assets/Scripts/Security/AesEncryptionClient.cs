using System;
using System.Text;
using System.Security.Cryptography;

/// <summary>
/// Provides static methods to perform AES encryption and decryption.
/// EncryptAES converts plain text into a Base64-encoded encrypted string using the specified key and IV.
/// DecryptAES converts a Base64-encoded encrypted string back into plain text using the specified key and IV.
/// </summary>
public static class AesEncryptionClient
{
    /// <summary>
    /// Encrypts a plain text string using AES encryption with the given key and initialization vector (IV).
    /// The output is a Base64-encoded string representing the encrypted data.
    /// </summary>
    /// <param name="plainText">The plain text string to encrypt.</param>
    /// <param name="key">A byte array representing the AES encryption key.</param>
    /// <param name="iv">A byte array representing the AES initialization vector (IV).</param>
    /// <returns>
    /// A Base64-encoded string containing the encrypted data.
    /// </returns>
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

    /// <summary>
    /// Decrypts a Base64-encoded string containing AES encrypted data using the given key and initialization vector (IV).
    /// The output is the decrypted plain text.
    /// </summary>
    /// <param name="cipherBase64">The Base64-encoded string representing the encrypted data.</param>
    /// <param name="key">A byte array representing the AES decryption key.</param>
    /// <param name="iv">A byte array representing the AES initialization vector (IV).</param>
    /// <returns>
    /// The decrypted plain text string.
    /// </returns>
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
