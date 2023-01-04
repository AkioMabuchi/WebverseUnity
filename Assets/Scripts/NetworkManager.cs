using System;
using System.Collections;
using System.Collections.Generic;
using NetworkLogin;
using UniGLTF;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
using VRM;

namespace NetworkLogin
{
    [Serializable]
    public class NetworkResult
    {
        public bool success;
        public string userToken;
        public string userName;
        public Vrm[] userVrms;
    }
    
    [Serializable]
    public abstract class Result
    {
        
    }

    [Serializable]
    public class Error : Result
    {
        
    }
    [Serializable]
    public class Failed : Result
    {
        
    }

    [Serializable]
    public class User : Result
    {
        public string token;
        public string name;
        public Vrm[] vrms;
    }

    [Serializable]
    public class Vrm
    {
        public string token;
        public string name;
    }
}

public class NetworkManager : MonoBehaviour
{
    private static readonly Subject<Result> _onFinishLogin = new();
    public static IObservable<Result> OnFinishLogin => _onFinishLogin;
    private static readonly Subject<(string address, string password)> _onExternalLogin = new();
    
    [SerializeField] private string accessKey;
    [SerializeField] private string secretAccessKey;

    public static void ExternalLogin(string address, string password)
    {
        _onExternalLogin.OnNext((address, password));
    }

    private string _baseAddress = "https://localhost:3000/";

    private void Awake()
    {
        #if !UNITY_EDITOR && UNITY_WEBGL
        _baseAddress = "/";
        #endif
        _onExternalLogin.Subscribe(tuple =>
        {
            var (address, password) = tuple;
            StartCoroutine(CoroutineExternalLogin(address, password));
        }).AddTo(gameObject);
    }

    private IEnumerator CoroutineExternalLogin(string address, string password)
    {
        var form = new WWWForm();
        form.AddField("secret", secretAccessKey);
        form.AddField("address", address);
        form.AddField("password", password);
        var request = UnityWebRequest.Post(_baseAddress + accessKey + "/login", form);

        yield return request.SendWebRequest();

        var isSuccess = false;
        NetworkResult result = null;
        if (request.result == UnityWebRequest.Result.Success)
        {
            if (request.responseCode == 200)
            {
                try
                {
                    result = JsonUtility.FromJson<NetworkResult>(request.downloadHandler.text);
                    Debug.Log(result.userVrms);
                    isSuccess = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    isSuccess = false;
                }
            }
        }

        if (isSuccess)
        {
            if (result.success)
            {

                _onFinishLogin.OnNext(new User
                {
                    token = result.userToken,
                    name = result.userName,
                    vrms = result.userVrms
                });
            }
            else
            {
                _onFinishLogin.OnNext(new Failed());

            }

        }
        else
        {
            _onFinishLogin.OnNext(new Error());
        }
    }

    /*
    private IEnumerator CoroutineExternalSession()
    {
        var form = new WWWForm();
        form.AddField("secret", secretAccessKey);
        var request = UnityWebRequest.Post(_baseAddress + accessKey + "/session", form);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            if (request.responseCode == 200)
            {
                var imageUrl = "";
                try
                {
                    var data = JsonUtility.FromJson<ApiSessionStatus>(request.downloadHandler.text);
                    imageUrl = _baseAddress + data.image;
                }
                catch (Exception e)
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
        yield break;
    }
    
    */
}
