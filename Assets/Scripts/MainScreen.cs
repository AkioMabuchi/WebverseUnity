using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VRM;

public class MainScreen : MonoBehaviour
{
    private const int EmoteNum = 4;
    private static readonly Subject<BlendShapePreset> _onClickButtonEmote = new();
    public static IObservable<BlendShapePreset> OnClickButtonEmote => _onClickButtonEmote;

    private static readonly ReactiveProperty<bool> _isActive = new(false);

    public static void Show()
    {
        _isActive.Value = true;
    }

    public static void Hide()
    {
        _isActive.Value = false;
    }
    
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button[] buttonsEmote = new Button[EmoteNum];

    private void Awake()
    {
        _isActive.Subscribe(isActive =>
        {
            canvasGroup.gameObject.SetActive(isActive);
        }).AddTo(gameObject);
        
        for (var i = 0; i < EmoteNum; i++)
        {
            var ii = i;
            buttonsEmote[i].OnClickAsObservable().Subscribe(_ =>
            {
                var presets = new[]
                {
                    BlendShapePreset.Joy,
                    BlendShapePreset.Angry,
                    BlendShapePreset.Sorrow,
                    BlendShapePreset.Fun
                };
                _onClickButtonEmote.OnNext(presets[ii]);
            }).AddTo(gameObject);
        }
    }
}
