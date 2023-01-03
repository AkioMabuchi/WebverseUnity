using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    private static readonly Subject<Unit> _onClickButtonLogin = new();
    public static IObservable<Unit> OnClickButtonLogin => _onClickButtonLogin;
    private static readonly Subject<Unit> _onClickButtonEnter = new();
    public static IObservable<Unit> OnClickButtonEnter => _onClickButtonEnter;

    private static readonly ReactiveProperty<string> _playerId = new("");
    public static IReadOnlyReactiveProperty<string> PlayerId => _playerId;
    private static readonly ReactiveProperty<string> _password = new("");
    public static IReadOnlyReactiveProperty<string> Password => _password;

    private static readonly ReactiveProperty<bool> _isActive = new(false);
    private static readonly ReactiveProperty<bool> _isActiveLoginForm = new(false);
    private static readonly ReactiveProperty<bool> _isActiveEntranceForm = new(false);
    
    private static readonly ReactiveProperty<bool> _isInteractableLoginForm = new(false);
    private static readonly ReactiveProperty<string> _loginWarningMessage = new("");

    public static void Show()
    {
        _isActive.Value = true;
    }

    public static void Hide()
    {
        _isActive.Value = false;
    }
    public static void ShowLoginForm()
    {
        _isActiveLoginForm.Value = true;
    }

    public static void HideLoginForm()
    {
        _isActiveLoginForm.Value = false;
    }

    public static void ShowEntranceForm()
    {
        _isActiveEntranceForm.Value = true;
    }

    public static void HideEntranceForm()
    {
        _isActiveEntranceForm.Value = false;
    }

    public static void SetInteractableLoginForm(bool interactable)
    {
        _isInteractableLoginForm.Value = interactable;
    }

    public static void SetLoginWarningMessage(string message)
    {
        _loginWarningMessage.Value = message;
    }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image imageBackground;
    [SerializeField] private CanvasGroup canvasGroupLoginForm;
    [SerializeField] private TMP_InputField inputFieldPlayerId;
    [SerializeField] private TMP_InputField inputFieldPassword;
    [SerializeField] private TextMeshProUGUI textMeshProLoginWarningMessage;
    [SerializeField] private Button buttonLogin;

    [SerializeField] private CanvasGroup canvasGroupEntranceForm;
    [SerializeField] private Button[] buttonsAvatar = new Button[8];
    [SerializeField] private TextMeshProUGUI[] textMeshProsAvatarName = new TextMeshProUGUI[8];
    [SerializeField] private Button buttonEnter;

    private void Awake()
    {
        _isActive.Subscribe(isActive =>
        {
            canvasGroup.gameObject.SetActive(isActive);
        }).AddTo(gameObject);
        
        _isActiveLoginForm.Subscribe(isActive =>
        {
            canvasGroupLoginForm.gameObject.SetActive(isActive);
        }).AddTo(gameObject);

        _isActiveEntranceForm.Subscribe(isActive =>
        {
            canvasGroupEntranceForm.gameObject.SetActive(isActive);
        }).AddTo(gameObject);
        
        _isInteractableLoginForm.Subscribe(interactable =>
        {
            canvasGroupLoginForm.interactable = interactable;
        }).AddTo(gameObject);

        _loginWarningMessage.Subscribe(message =>
        {
            textMeshProLoginWarningMessage.text = message;
        }).AddTo(gameObject);

        for (var i = 0; i < 8; i++)
        {
            var ii = i;
            buttonsAvatar[i].OnClickAsObservable().Subscribe(_ =>
            {
                MyAvatarModel.SetMyAvatarIndex(ii);
            }).AddTo(gameObject);
        }

        buttonLogin.OnClickAsObservable().Subscribe(_ =>
        {
            _playerId.Value = inputFieldPlayerId.text;
            _password.Value = inputFieldPassword.text;
            _onClickButtonLogin.OnNext(Unit.Default);
        }).AddTo(gameObject);

        buttonEnter.OnClickAsObservable().Subscribe(_ =>
        {
            _onClickButtonEnter.OnNext(Unit.Default);
        }).AddTo(gameObject);
        
        
        MyAvatarModel.Avatars.Subscribe(avatars =>
        {
            for (var i = 0; i < 8; i++)
            {
                if (i < avatars.Count)
                {
                    buttonsAvatar[i].interactable = true;
                    if (i == MyAvatarModel.AvatarIndex.Value)
                    {
                        buttonsAvatar[i].image.color = new Color(0.875f, 1.0f, 0.0f);
                    }
                    else
                    {
                        buttonsAvatar[i].image.color = new Color(0.8f, 0.8f, 0.8f);
                    }

                    textMeshProsAvatarName[i].text = avatars[i].name;
                }
                else
                {
                    buttonsAvatar[i].image.color = new Color(0.5f, 0.5f, 0.5f);
                    buttonsAvatar[i].interactable = false;
                    textMeshProsAvatarName[i].text = "";
                }
            }
        }).AddTo(gameObject);

        MyAvatarModel.AvatarIndex.Subscribe(index =>
        {
            for (var i = 0; i < 8; i++)
            {
                if (i < MyAvatarModel.Avatars.Value.Count)
                {
                    if (i == index)
                    {
                        buttonsAvatar[i].image.color = new Color(0.875f, 1.0f, 0.0f);
                    }
                    else
                    {
                        buttonsAvatar[i].image.color = new Color(0.8f, 0.8f, 0.8f);
                    }
                }
                else
                {
                    buttonsAvatar[i].image.color = new Color(0.5f, 0.5f, 0.5f);
                }
            }
        }).AddTo(gameObject);
    }
}
