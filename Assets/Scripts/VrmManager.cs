using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using NetworkLogin;
using UniGLTF;
using UniRx;
using UniRx.Triggers;
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
    public VRMBlendShapeProxy blendShapeProxy;
    public int emoteCount;
}

[Serializable]
public class PlayerVrmError : PlayerVrm
{

}

public class VrmManager : MonoBehaviour
{
    private static readonly Subject<(Player player, string token)> _onLoadAvatar = new();
    private static readonly Subject<(Player player, Vector3 position)> _onSetPosition = new();
    private static readonly Subject<(Player player, Quaternion rotation)> _onSetRotation = new();
    private static readonly Subject<(Player player, string trigger)> _onSetAnimatorTrigger = new();
    private static readonly Subject<(Player player, BlendShapePreset preset)> _onEmote = new();
    private static readonly Subject<Player> _onDiminishVrm = new();
    public static void LoadAvatar(Player player, string token)
    {
        _onLoadAvatar.OnNext((player, token));
    }

    public static void SetPosition(Player player, Vector3 position)
    {
        _onSetPosition.OnNext((player, position));
    }

    public static void SetRotation(Player player, Quaternion rotation)
    {
        _onSetRotation.OnNext((player, rotation));
    }

    public static void SetAnimatorTrigger(Player player, string trigger)
    {
        _onSetAnimatorTrigger.OnNext((player, trigger));
    }

    public static void Emote(Player player, BlendShapePreset preset)
    {
        _onEmote.OnNext((player, preset));
    }

    public static void DiminishVrm(Player player)
    {
        _onDiminishVrm.OnNext(player);
    }

    [SerializeField] private RuntimeAnimatorController animatorController;
    [SerializeField] private GameObject prefabDefaultFemale;
    [SerializeField] private GameObject prefabDefaultMale;
    
    private readonly Dictionary<Player, PlayerVrm> _vrms = new();
    private readonly Dictionary<Player, Coroutine> _coroutinesLoadVrm = new();
    private void Awake()
    {
        _onLoadAvatar.Subscribe(tuple =>
        {
            var (player, token) = tuple;
            if (_coroutinesLoadVrm.TryGetValue(player, out var coroutine))
            {
                StopCoroutine(coroutine);
            }
            _coroutinesLoadVrm.Add(player, StartCoroutine(CoroutineLoadVrm(player, token)));
        }).AddTo(gameObject);

        _onSetPosition.Subscribe(tuple =>
        {
            var (player, position) = tuple;
            if (_vrms.TryGetValue(player, out var vrm))
            {
                vrm.transform.position = position;
            }
        }).AddTo(gameObject);

        _onSetRotation.Subscribe(tuple =>
        {
            var (player, rotation) = tuple;
            if (_vrms.TryGetValue(player, out var vrm))
            {
                vrm.transform.rotation = rotation;
            }
        }).AddTo(gameObject);

        _onSetAnimatorTrigger.Subscribe(tuple =>
        {
            var (player, trigger) = tuple;
            if (_vrms.TryGetValue(player, out var vrm))
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

        _onEmote.Subscribe(tuple =>
        {
            var (player, preset) = tuple;
            if (_vrms.TryGetValue(player, out var vrm))
            {
                switch (vrm)
                {
                    case PlayerVrmNormal vrmNormal:
                    {
                        if (vrmNormal.blendShapeProxy != null)
                        {
                            vrmNormal.blendShapeProxy.SetValues(new Dictionary<BlendShapeKey, float>
                            {
                                {
                                    BlendShapeKey.CreateFromPreset(BlendShapePreset.Joy),
                                    preset == BlendShapePreset.Joy ? 1.0f : 0.0f
                                },
                                {
                                    BlendShapeKey.CreateFromPreset(BlendShapePreset.Angry),
                                    preset == BlendShapePreset.Angry ? 1.0f : 0.0f
                                },
                                {
                                    BlendShapeKey.CreateFromPreset(BlendShapePreset.Sorrow),
                                    preset == BlendShapePreset.Sorrow ? 1.0f : 0.0f
                                },
                                {
                                    BlendShapeKey.CreateFromPreset(BlendShapePreset.Fun),
                                    preset == BlendShapePreset.Fun ? 1.0f : 0.0f
                                }
                            });

                            vrmNormal.emoteCount = 250;
                        }
                        break;
                    }
                }
            }
        }).AddTo(gameObject);

        _onDiminishVrm.Subscribe(player =>
        {
            if (_vrms.TryGetValue(player, out var vrm))
            {
                Destroy(vrm.transform.gameObject);
            }

            _vrms.Remove(player);
        }).AddTo(gameObject);

        this.FixedUpdateAsObservable()
            .Subscribe(_ =>
            {
                foreach (var vrm in _vrms.Values)
                {
                    switch (vrm)
                    {
                        case PlayerVrmNormal vrmNormal:
                        {
                            if (vrmNormal.emoteCount > 0)
                            {
                                vrmNormal.emoteCount--;
                                if (vrmNormal.emoteCount == 0)
                                {
                                    if (vrmNormal.blendShapeProxy != null)
                                    {
                                        var blendShapeValues = new Dictionary<BlendShapeKey, float>();
                                        var blendShapePresets = new[]
                                        {
                                            BlendShapePreset.Joy,
                                            BlendShapePreset.Angry,
                                            BlendShapePreset.Sorrow,
                                            BlendShapePreset.Fun
                                        };
                                        foreach (var preset in blendShapePresets)
                                        {
                                            blendShapeValues.Add(BlendShapeKey.CreateFromPreset(preset), 0.0f);
                                        }

                                        vrmNormal.blendShapeProxy.SetValues(blendShapeValues);
                                    }
                                }
                            }

                            break;
                        }
                    }
                }
            }).AddTo(gameObject);
    }

    private IEnumerator CoroutineLoadVrm(Player player, string token)
    {
        GameObject vrmObject = null;
        switch (token)
        {
            case "default_female":
            {
                vrmObject = Instantiate(prefabDefaultFemale);
                break;
            }
            case "default_male":
            {
                vrmObject = Instantiate(prefabDefaultMale);
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

                    vrmObject = instance.gameObject;
                }
                break;
            }
        }

        if (vrmObject == null)
        {

        }
        else
        {
            var playerVrm = new PlayerVrmNormal
            {
                transform = vrmObject.transform
            };

            if (vrmObject.TryGetComponent(out Animator animator))
            {
                animator.runtimeAnimatorController = animatorController;
                playerVrm.animator = animator;
            }

            if (vrmObject.TryGetComponent(out VRMBlendShapeProxy blendShapeProxy))
            {
                playerVrm.blendShapeProxy = blendShapeProxy;
            }

            _vrms.Add(player, playerVrm);
        }
    }
}
