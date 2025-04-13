using UnityEngine;
using System;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

/// <summary>
/// Description:
/// Handles the RSA handshake with the server and manages AES encryption/decryption for secure communication.
/// This class processes the handshake by receiving the server's RSA public key, generating an AES key/IV pair,
/// encrypting these with RSA, and then exchanging them with the server. It also provides methods to encrypt and decrypt messages
/// using AES after the handshake has been completed.
/// </summary>
public class NetworkEncryptionManager : MonoBehaviour
{
    private NetworkManager net;

    // RSA server public key components.
    private byte[] serverModulus;
    private byte[] serverExponent;

    // AES key and initialization vector (IV) used for encryption and decryption.
    private byte[] aesKey;
    private byte[] aesIV;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Retrieves a reference to the NetworkManager component on the same GameObject.
    /// </summary>
    void Awake()
    {
        net = GetComponent<NetworkManager>();
    }

    /// <summary>
    /// Handles handshake messages received from the server before the AES encryption is established.
    /// Expects a JSON message of type "ServerPublicKey" containing the RSA modulus and exponent.
    /// Upon receiving the server's public key, initiates the generation of a new AES key/IV and sends them
    /// to the server in an "AESKeyExchange" message.
    /// </summary>
    /// <param name="data">The handshake message in JSON format.</param>
    public void HandleHandshakeMessage(string data)
    {
        Debug.Log("[CLIENT] Handshake msg: " + data);
        JObject jo = JObject.Parse(data);
        string msgType = jo["Type"].ToString();

        if (msgType == "ServerPublicKey")
        {
            // Retrieve and decode RSA public key components.
            serverModulus = Convert.FromBase64String(jo["Modulus"].ToString());
            serverExponent = Convert.FromBase64String(jo["Exponent"].ToString());

            // Generate AES key/IV and send an AESKeyExchange message.
            Debug.Log("C");
            GenerateAESKeyAndSend();
            Debug.Log("D");
        }
        else
        {
            Debug.LogWarning("[CLIENT] Unexpected handshake message type: " + msgType);
        }
    }

    /// <summary>
    /// Generates a random AES key and IV, encrypts them with the server's RSA public key, and sends them 
    /// to the server in an "AESKeyExchange" message.
    /// Once this is sent, the handshake is considered complete.
    /// </summary>
    private void GenerateAESKeyAndSend()
    {
        // 1) Generate random AES key and IV.
        using (Aes aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.GenerateKey();
            aes.GenerateIV();
            aesKey = aes.Key;
            aesIV = aes.IV;
        }

        // 2) Encrypt the AES key and IV using the server's RSA public key.
        using (RSA rsa = RSA.Create())
        {
            rsa.ImportParameters(new RSAParameters
            {
                Modulus = serverModulus,
                Exponent = serverExponent
            });

            // Encrypt the AES key and IV with RSA using OAEP SHA-1 padding.
            byte[] encKey = rsa.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA1);
            byte[] encIV = rsa.Encrypt(aesIV, RSAEncryptionPadding.OaepSHA1);

            // 3) Build a JSON object containing the encrypted AES key and IV.
            JObject exchange = new JObject();
            exchange["Type"] = "AESKeyExchange";
            exchange["EncryptedKey"] = Convert.ToBase64String(encKey);
            exchange["EncryptedIV"] = Convert.ToBase64String(encIV);

            // 4) Send the JSON message unencrypted.
            net.SendRaw(exchange.ToString());
        }

        // 5) Mark the handshake as completed so subsequent communications use AES encryption.
        net.isHandshakeCompleted = true;

        Debug.Log("[CLIENT] AESKeyExchange sent. Handshake completed on client side.");

        // Debug: Log the raw AES key and IV values.
        Debug.Log("client aesKey:");
        string key = "";
        for (int i = 0; i < aesKey.Length; i++)
        {
            key += aesKey[i];
        }
        Debug.Log(key);
        Debug.Log("client aesIV:");
        string iv = "";
        for (int i = 0; i < aesIV.Length; i++)
        {
            iv += aesIV[i];
        }
        Debug.Log(iv);
    }

    /// <summary>
    /// Decrypts a base64-encoded ciphertext using AES with the established key and IV.
    /// </summary>
    /// <param name="base64Cipher">A base64 string representing the ciphertext to decrypt.</param>
    /// <returns>
    /// The decrypted plain text string, or an empty string if decryption fails or if the network is not connected or handshake is incomplete.
    /// </returns>
    public string Decrypt(string base64Cipher)
    {
        try
        {
            if (!net.isConnected || !net.isHandshakeCompleted)
                return string.Empty;

            return AesEncryptionClient.DecryptAES(base64Cipher, aesKey, aesIV);
        }
        catch (Exception ex)
        {
            Debug.LogError("[EncryptionManager] Decrypt error: " + ex.Message);
            return string.Empty;
        }
    }

    /// <summary>
    /// Encrypts a plain JSON string using AES with the established key and IV, returning a base64-encoded ciphertext.
    /// Intended for use after the handshake has been completed.
    /// </summary>
    /// <param name="plainJson">The plain JSON string to be encrypted.</param>
    /// <returns>
    /// A base64 string representing the encrypted ciphertext, or an empty string if encryption fails or if the network is not connected or handshake is incomplete.
    /// </returns>
    public string Encrypt(string plainJson)
    {
        try
        {
            if (!net.isConnected || !net.isHandshakeCompleted)
                return string.Empty;
            return AesEncryptionClient.EncryptAES(plainJson, aesKey, aesIV);
        }
        catch (Exception ex)
        {
            Debug.LogError("[EncryptionManager] Encrypt error: " + ex.Message);
            return string.Empty;
        }
    }
}
