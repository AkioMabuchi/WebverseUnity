using System;
using System.Collections;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using UnityEngine.Networking;
using VRM;

public class NetworkManager : MonoBehaviour
{
    [SerializeField] private string accessKey;
    [SerializeField] private string secretAccessKey;

    private IEnumerator CoroutineExternalLogin()
    {

        const string address = "akio";
        const string password = "akiomabuchi";
        var form = new WWWForm();
        form.AddField("secret", secretAccessKey);
        form.AddField("address", address);
        form.AddField("password", password);
        var request = UnityWebRequest.Post("http://localhost:3000/" + accessKey + "/login", form);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            if (request.responseCode == 200)
            {
                Debug.Log(request.downloadHandler.text);
            }
        }
    }

    private IEnumerator CoroutineExternalSession()
    {
        var form = new WWWForm();
        form.AddField("secret", secretAccessKey);
        var request = UnityWebRequest.Post("http://localhost:3000/" + accessKey + "/session", form);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            if (request.responseCode == 200)
            {
                var imageUrl = "";
                try
                {
                    var data = JsonUtility.FromJson<ApiSessionStatus>(request.downloadHandler.text);
                    imageUrl = "http://localhost:3000" + data.image;

                }
                catch(Exception e)
                {
                    Debug.Log(e);
                }

                if (imageUrl != "")
                {
                    var imageRequest = UnityWebRequestTexture.GetTexture(imageUrl);

                    yield return imageRequest.SendWebRequest();

                    if (imageRequest.result == UnityWebRequest.Result.Success && imageRequest.responseCode == 200)
                    {
                        // rawImage.texture = ((DownloadHandlerTexture) imageRequest.downloadHandler).texture;
                    }
                    else
                    {
                        Debug.LogWarning("FAILED");
                    }
                }
            }
        }
    }

    private IEnumerator CoroutineLoadVrm(string token)
    {
        var request = UnityWebRequest.Get("http://localhost:3000/uploads/vrms/" + token + "/body.vrm");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success && request.responseCode == 200)
        {
            using var gltfData = new GlbBinaryParser(request.downloadHandler.data, "").Parse();
            var vrm = new VRMData(gltfData);
            using var context = new VRMImporterContext(vrm);
            Debug.Log("START");
            var instance = context.Load();
            Debug.Log("END");
            instance.ShowMeshes();
        }
    }
}
