using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using VRM;

public class Player : NetworkBehaviour
{
    [SerializeField] private NetworkCharacterControllerPrototype characterController;
    [SerializeField] private NetworkObject networkObject;

    [Networked] public NetworkString<_128> VrmToken { get; set; }
    [Networked] public NetworkString<_64> PlayerName { get; set; }

    private readonly ReactiveProperty<string> _animatorTrigger = new("");

    private void Awake()
    {
        MainScreen.OnClickButtonEmote
            .Where(_ => HasInputAuthority)
            .Subscribe(preset =>
            {
                RPC_Emote(preset);
            }).AddTo(gameObject);

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                VrmManager.SetPosition(this, transform.position);
                VrmManager.SetRotation(this, transform.rotation);
            }).AddTo(gameObject);

        this.FixedUpdateAsObservable()
            .Where(_ => HasInputAuthority)
            .Subscribe(_ =>
            {
                
            }).AddTo(gameObject);

        _animatorTrigger.Subscribe(animatorTrigger =>
        {
            if (HasInputAuthority)
            {
                RPC_SetAnimatorTrigger(animatorTrigger);
            }
        }).AddTo(gameObject);
    }

    private void Start()
    {
        PlayerNamePlateManager.CreatePlate(this);
        VrmManager.LoadAvatar(this, VrmToken.Value);
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            var rotation = transform.rotation.eulerAngles;
            rotation += Vector3.up * data.direction.x * 3.0f;
            transform.rotation = Quaternion.Euler(rotation);

            characterController.Move(transform.rotation * Vector3.forward * data.direction.z * Runner.DeltaTime * 5.0f);

            if (HasInputAuthority)
            {
                if (data.direction.z > 0.2f)
                {
                    _animatorTrigger.Value = "Walk";
                }
                else
                {
                    _animatorTrigger.Value = "Wait";
                }
                
                CameraManager.SendMyPositionAndRotation(transform.position, transform.rotation);
            }
        }
    }

    private void OnDestroy()
    {
        PlayerNamePlateManager.DiminishPlate(this);
        VrmManager.DiminishVrm(this);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_LoadVrm()
    {
        VrmManager.LoadAvatar(this, VrmToken.Value);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_SetAnimatorTrigger(string trigger)
    {
        VrmManager.SetAnimatorTrigger(this, trigger);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_Emote(BlendShapePreset preset)
    {
        VrmManager.Emote(this, preset);
    }
}
