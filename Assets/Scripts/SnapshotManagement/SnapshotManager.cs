using UnityEngine;
using System;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using System.Collections;

/// <summary>
/// Description:
/// Manages the capturing and sending of game snapshots to the server.
/// The SnapshotManager periodically captures a screenshot of the map, compresses the image,
/// encodes it to Base64, and sends it as a JSON message over the network.
/// Snapshot sending is only active when enabled and not during game over.
/// </summary>
public class SnapshotManager : MonoBehaviour
{
    private GameManager gameManager;

    // Coroutine instance for snapshot sending.
    private Coroutine snapshotCoroutine = null;

    // Flag to indicate whether snapshot sending is active.
    private bool isSendingSnapshots = false;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Retrieves and caches the GameManager component.
    /// </summary>
    void Awake()
    {
        gameManager = GetComponent<GameManager>();
    }

    /// <summary>
    /// Enables or disables the sending of snapshots.
    /// If enabled, starts the snapshot sending coroutine.
    /// If disabled, stops any ongoing snapshot sending.
    /// Snapshots will not be sent if the game is over.
    /// </summary>
    /// <param name="enable">True to enable snapshot sending; false to disable.</param>
    public void EnableSnapshotSending(bool enable)
    {
        if (gameManager.isGameOver)
            return; // Do not send snapshots if the game is over.

        if (enable && !isSendingSnapshots)
        {
            isSendingSnapshots = true;
            snapshotCoroutine = StartCoroutine(SendSnapshots());
        }
        else if (!enable && isSendingSnapshots)
        {
            StopSendingSnapshots();
        }
    }

    /// <summary>
    /// Stops the snapshot sending coroutine and resets the sending flag.
    /// </summary>
    public void StopSendingSnapshots()
    {
        isSendingSnapshots = false;
        if (snapshotCoroutine != null)
        {
            StopCoroutine(snapshotCoroutine);
            snapshotCoroutine = null;
        }
    }

    /// <summary>
    /// Coroutine that periodically captures and sends snapshots to the server.
    /// Initially sends one snapshot immediately after the frame ends for better user experience,
    /// then continues sending snapshots at regular intervals (approximately every 1 second).
    /// </summary>
    /// <returns>An IEnumerator for the coroutine.</returns>
    private IEnumerator SendSnapshots()
    {
        // Immediately capture and send a snapshot after the current frame for better user experience.
        yield return new WaitForEndOfFrame();
        SendSnapshotToServer();

        // Continuously send snapshots while enabled and game is not over.
        while (isSendingSnapshots && !gameManager.isGameOver)
        {
            // Wait for 1 second before sending the next snapshot.
            yield return new WaitForSeconds(1f);
            yield return new WaitForEndOfFrame();
            SendSnapshotToServer();
        }
    }

    /// <summary>
    /// Captures a snapshot of the map, compresses it, converts it to Base64, and sends it to the server.
    /// Constructs a JSON message containing the compressed snapshot data.
    /// </summary>
    private void SendSnapshotToServer()
    {
        Texture2D snapshot = CaptureSnapshot();
        byte[] jpgBytes = snapshot.EncodeToJPG(50); // Use quality parameter of 50.

        // Compress the JPG byte array.
        byte[] compressedBytes = CompressData(jpgBytes);
        string imageData = Convert.ToBase64String(compressedBytes);

        // Construct the snapshot message.
        var snapshotMessage = new GameSnapshotMessage
        {
            Type = "GameSnapshot",
            Data = new GameSnapshotData { ImageData = imageData }
        };

        // Serialize the message to JSON and send it with the appropriate prefix.
        string jsonMessage = JsonConvert.SerializeObject(snapshotMessage);
        NetworkManager.Instance.messageSender.SendMessageWithLengthPrefix(jsonMessage);

        // Optionally, release memory by destroying the snapshot texture.
        // Destroy(snapshot);
    }

    /// <summary>
    /// Captures a snapshot from the map render texture.
    /// Reads the pixels from the RenderTexture associated with the map,
    /// creates a Texture2D, and returns it.
    /// </summary>
    /// <returns>A Texture2D containing the snapshot of the current map view.</returns>
    private Texture2D CaptureSnapshot()
    {
        // Backup the current RenderTexture.
        RenderTexture currentRT = RenderTexture.active;
        // Set the map's RenderTexture as active.
        RenderTexture.active = gameManager.mapRenderTexture;

        // Create a texture with dimensions matching the map render texture.
        Texture2D tex = new Texture2D(gameManager.mapRenderTexture.width, gameManager.mapRenderTexture.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, gameManager.mapRenderTexture.width, gameManager.mapRenderTexture.height), 0, 0);
        tex.Apply();

        // Restore the previous RenderTexture.
        RenderTexture.active = currentRT;

        return tex;
    }

    /// <summary>
    /// Compresses the provided byte array using GZip compression.
    /// </summary>
    /// <param name="data">The byte array to compress.</param>
    /// <returns>A compressed byte array.</returns>
    private byte[] CompressData(byte[] data)
    {
        using (var output = new MemoryStream())
        {
            using (var gzip = new GZipStream(output, System.IO.Compression.CompressionLevel.Optimal))
            {
                gzip.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }
    }
}
