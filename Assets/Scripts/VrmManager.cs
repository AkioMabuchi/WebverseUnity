using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UniGLTF;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
using VRM;

[Serializable]
public abstract class PlayerVrm
{
    public Transform transform;
}

[Serializable]
public class PlayerVrmNormal: PlayerVrm
{
    public Animator animator;
}

[Serializable]
public class PlayerVrmError : PlayerVrm
{

}

public class VrmManager : MonoBehaviour
{
    private static readonly Subject<(uint playerId, string token)> _onLoadAvatar = new();
    private static readonly Subject<(uint playerId, Vector3 position)> _onSetPosition = new();
    private static readonly Subject<(uint playerId, Quaternion rotation)> _onSetRotation = new();
    private static readonly Subject<(uint playerId, string trigger)> _onSetAnimatorTrigger = new();
    public static void LoadAvatar(uint playerId, string token)
    {
        _onLoadAvatar.OnNext((playerId, token));
    }

    public static void SetPosition(uint playerId, Vector3 position)
    {
        _onSetPosition.OnNext((playerId, position));
    }

    public static void SetRotation(uint playerId, Quaternion rotation)
    {
        _onSetRotation.OnNext((playerId, rotation));
    }

    public static void SetAnimatorTrigger(uint playerId, string trigger)
    {
        _onSetAnimatorTrigger.OnNext((playerId, trigger));
    }

    [SerializeField] private RuntimeAnimatorController animatorController;
    
    private readonly Dictionary<uint, PlayerVrm> _vrms = new();
    private readonly Dictionary<uint, Coroutine> _coroutinesLoadVrm = new();
    private void Awake()
    {
        _onLoadAvatar.Subscribe(tuple =>
        {
            var (playerId, token) = tuple;
            if (_coroutinesLoadVrm.TryGetValue(playerId, out var coroutine))
            {
                StopCoroutine(coroutine);
            }
            _coroutinesLoadVrm.Add(playerId, StartCoroutine(CoroutineLoadVrm(playerId, token)));
        }).AddTo(gameObject);

        _onSetPosition.Subscribe(tuple =>
        {
            var (playerId, position) = tuple;
            if (_vrms.TryGetValue(playerId, out var vrm))
            {
                vrm.transform.position = position;
            }
        }).AddTo(gameObject);

        _onSetRotation.Subscribe(tuple =>
        {
            var (playerId, rotation) = tuple;
            if (_vrms.TryGetValue(playerId, out var vrm))
            {
                vrm.transform.rotation = rotation;
            }
        }).AddTo(gameObject);

        _onSetAnimatorTrigger.Subscribe(tuple =>
        {
            var (playerId, trigger) = tuple;
            if (_vrms.TryGetValue(playerId, out var vrm))
            {
                switch (vrm)
                {
                    case PlayerVrmNormal vrmNormal:
                    {
                        if (vrmNormal.animator != null)
                        {
                            vrmNormal.animator.SetTrigger(trigger);
                        }

                        break;
                    }
                }
            }
        }).AddTo(gameObject);
    }

    private IEnumerator CoroutineLoadVrm(uint playerId, string token)
    {
        switch (token)
        {
            case "default_female":
            {
                break;
            }
            case "default_male":
            {
                break;
            }
            default:
            {
                var request = UnityWebRequest.Get("http://localhost:3000/uploads/vrms/" + token + "/body.vrm");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success && request.responseCode == 200)
                {
                    using var gltfData = new GlbBinaryParser(request.downloadHandler.data, "").Parse();
                    var vrm = new VRMData(gltfData);
                    using var context = new VRMImporterContext(vrm);
                    var instance = context.Load();
                    instance.ShowMeshes();

                    var playerVrm = new PlayerVrmNormal
                    {
                        transform = instance.gameObject.transform
                    };

                    if (instance.gameObject.TryGetComponent(out Animator animator))
                    {
                        animator.runtimeAnimatorController = animatorController;
                        playerVrm.animator = animator;
                    }

                    _vrms.Add(playerId, playerVrm);
                }
                else
                {
                    
                }
                break;
            }
        }
    }
}
