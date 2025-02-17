using UnityEngine;
using System;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

/// <summary>
/// Handles the RSA handshake with server and manages AES encryption/decryption.
/// </summary>
public class NetworkEncryptionManager : MonoBehaviour
{
    private NetworkManager net;

    // RSA server public key
    private byte[] serverModulus;
    private byte[] serverExponent;

    // AES key/IV
    private byte[] aesKey;
    private byte[] aesIV;

    void Awake()
    {
        net = GetComponent<NetworkManager>();
    }

    /// <summary>
    /// Handle handshake messages BEFORE AES is established.
    /// Expects "ServerPublicKey" => respond with "AESKeyExchange".
    /// </summary>
    public void HandleHandshakeMessage(string data)
    {
        Debug.Log("[CLIENT] Handshake msg: " + data);
        JObject jo = JObject.Parse(data);
        string msgType = jo["Type"].ToString();

        if (msgType == "ServerPublicKey")
        {
            // Retrieve modulus + exponent
            serverModulus = Convert.FromBase64String(jo["Modulus"].ToString());
            serverExponent = Convert.FromBase64String(jo["Exponent"].ToString());

            // Next step: generate AES and send "AESKeyExchange"
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
    /// Generate random AES key/IV, encrypt with server's RSA public key, and send them in a "AESKeyExchange" message.
    /// </summary>
    private void GenerateAESKeyAndSend()
    {
        // 1) Generate random AES key/IV
        using (Aes aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.GenerateKey();
            aes.GenerateIV();
            aesKey = aes.Key;
            aesIV = aes.IV;
        }

        // 2) RSA-encrypt them
        using (RSA rsa = RSA.Create())
        {
            rsa.ImportParameters(new RSAParameters
            {
                Modulus = serverModulus,
                Exponent = serverExponent
            });

            //byte[] encKey = rsa.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA256);
            //byte[] encIV = rsa.Encrypt(aesIV, RSAEncryptionPadding.OaepSHA256);
            byte[] encKey = rsa.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA1);
            byte[] encIV = rsa.Encrypt(aesIV, RSAEncryptionPadding.OaepSHA1);

            // 3) Build JSON
            JObject exchange = new JObject();
            exchange["Type"] = "AESKeyExchange";
            exchange["EncryptedKey"] = Convert.ToBase64String(encKey);
            exchange["EncryptedIV"] = Convert.ToBase64String(encIV);

            // 4) Send it unencrypted
            net.SendRaw(exchange.ToString());
        }

        // 5) The server should decrypt & store. 
        // We'll assume once we send that, the server is ready.
        net.isHandshakeCompleted = true;

        Debug.Log("[CLIENT] AESKeyExchange sent. Handshake completed on client side.");


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
    /// AES-decrypt the base64 string received from server after handshake.
    /// </summary>
    public string Decrypt(string base64Cipher)
    {
        try
        {
            if (!net.isConnected || !net.isHandshakeCompleted) return string.Empty;

            return AesEncryptionClient.DecryptAES(base64Cipher, aesKey, aesIV);
        }
        catch (Exception ex)
        {
            Debug.LogError("[EncryptionManager] Decrypt error: " + ex.Message);
            return string.Empty;
        }
    }

    /// <summary>
    /// AES-encrypt a plain JSON, returning base64 string. For post-handshake sending.
    /// </summary>
    public string Encrypt(string plainJson)
    {
        try
        {
            if (!net.isConnected || !net.isHandshakeCompleted) return string.Empty;
            return AesEncryptionClient.EncryptAES(plainJson, aesKey, aesIV);
        }
        catch (Exception ex)
        {
            Debug.LogError("[EncryptionManager] Encrypt error: " + ex.Message);
            return string.Empty;
        }
    }
}
