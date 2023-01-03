using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private NetworkCharacterControllerPrototype characterController;
    [SerializeField] private NetworkObject networkObject;

    [Networked] public NetworkString<_128> VrmToken { get; set; }

    private void Awake()
    {
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                VrmManager.SetPosition(networkObject.Id.Raw, transform.position);
                VrmManager.SetRotation(networkObject.Id.Raw, transform.rotation);
            }).AddTo(gameObject);
    }

    private void Start()
    {
        VrmManager.LoadAvatar(networkObject.Id.Raw, VrmToken.Value);
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();
            characterController.Move(data.direction * Runner.DeltaTime * 5.0f);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_LoadVrm()
    {
        VrmManager.LoadAvatar(networkObject.Id.Raw, VrmToken.Value);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_SetTriggerAnimator()
    {
        
    }
}
