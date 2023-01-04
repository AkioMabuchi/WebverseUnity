using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private static readonly Subject<(Vector3 position, Quaternion rotation)> _onSendMyPositionAndRotation = new();

    public static void SendMyPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        _onSendMyPositionAndRotation.OnNext((position, rotation));
    }
    
    [SerializeField] private Camera mainCamera;

    private void Awake()
    {
        _onSendMyPositionAndRotation.Subscribe(tuple =>
        {
            var (position, rotation) = tuple;
            var cameraPosition = position;
            var cameraRotation = rotation;

            var tmpCameraRotation = cameraRotation.eulerAngles;
            tmpCameraRotation.x = 10.0f;
            cameraRotation = Quaternion.Euler(tmpCameraRotation);

            var radY = rotation.eulerAngles.y * Mathf.Deg2Rad;

            cameraPosition.x -= Mathf.Sin(radY) * 2.0f;
            cameraPosition.y += 2.0f;
            cameraPosition.z -= Mathf.Cos(radY) * 2.0f;
            
            mainCamera.transform.position = cameraPosition;
            mainCamera.transform.rotation = cameraRotation;
        }).AddTo(gameObject);
    }
}
