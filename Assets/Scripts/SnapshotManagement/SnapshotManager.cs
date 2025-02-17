using UnityEngine;
using System;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using System.Collections;

public class SnapshotManager : MonoBehaviour
{
    private GameManager gameManager;

    private Coroutine snapshotCoroutine = null;
    private bool isSendingSnapshots = false;

    void Awake()
    {
        gameManager = GetComponent<GameManager>();
    }

    // Called by NetworkManager when we receive "ShowSnapshots" or "HideSnapshots"
    public void EnableSnapshotSending(bool enable)
    {
        if (gameManager.isGameOver) return; // No snapshots if the game is over

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

    public void StopSendingSnapshots()
    {
        isSendingSnapshots = false;
        if (snapshotCoroutine != null)
        {
            StopCoroutine(snapshotCoroutine);
            snapshotCoroutine = null;
        }
    }

    private IEnumerator SendSnapshots()
    {
        // Immediately send one snapshot for better user experience
        yield return new WaitForEndOfFrame();
        SendSnapshotToServer();

        while (isSendingSnapshots && !gameManager.isGameOver)
        {
            yield return new WaitForSeconds(1f); // Every 5 seconds
            yield return new WaitForEndOfFrame(); //Check if this line is needed here.
            SendSnapshotToServer();
        }
    }

    private void SendSnapshotToServer()
    {
        Texture2D snapshot = CaptureSnapshot();
        byte[] jpgBytes = snapshot.EncodeToJPG(50);

        byte[] compressedBytes = CompressData(jpgBytes);
        string imageData = Convert.ToBase64String(compressedBytes);

        //Debug.Log("Length comparison: ---------> " + jpgBytes.Length + ", " + compressedBytes.Length);
        //Debug.Log("Image Data initialy is: " + imageData);

        var snapshotMessage = new GameSnapshotMessage
        {
            Type = "GameSnapshot",
            Data = new GameSnapshotData { ImageData = imageData }
        };

        string jsonMessage = JsonConvert.SerializeObject(snapshotMessage);
        NetworkManager.Instance.messageSender.SendMessageWithLengthPrefix(jsonMessage);

        //Works without it, but maybe add this to free memory:
        //Destroy(snapshot);
    }

    private Texture2D CaptureSnapshot()
    {
        // Set the RenderTexture as active
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = gameManager.mapRenderTexture;

        // Create a Texture2D with the same size as mapRenderTexture
        Texture2D tex = new Texture2D(
            gameManager.mapRenderTexture.width, gameManager.mapRenderTexture.height,
            TextureFormat.RGB24, false
        );

        tex.ReadPixels(new Rect(0, 0, gameManager.mapRenderTexture.width, gameManager.mapRenderTexture.height), 0, 0);
        tex.Apply();

        // Restore the active RenderTexture
        RenderTexture.active = currentRT;

        return tex;
    }

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
