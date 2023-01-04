using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class CanvasPlayerPlate : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshProUGUI textMeshProPlayerName;

    private void Awake()
    {
        if (Camera.main != null)
        {
            canvas.worldCamera = Camera.main;
        }

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                if (Camera.main != null)
                {
                    rectTransform.rotation = Camera.main.transform.rotation;
                }
            }).AddTo(gameObject);
    }

    public void SetPlayerName(string playerName)
    {
        textMeshProPlayerName.text = playerName;
    }

    public void SetPosition(Vector3 position)
    {
        rectTransform.position = position;
    }
}
