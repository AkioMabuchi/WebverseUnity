using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetworkLogin;
using TMPro;
using UniGLTF;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using VRM;

[Serializable]
public class ApiSessionStatus
{
    public bool success;
    public string token;
    public string name;
    public string image;
}
public class GameManager : MonoBehaviour
{
    private enum State
    {
        Start,
        LoginForm,
        Login,
        Entrance,
        Enter,
        Main,
    }

    private readonly ReactiveProperty<State> _state = new(State.Start);
    private readonly List<IDisposable> _disposables = new();
    private readonly List<Action> _actionsBeforeStateChange = new();
    
    private void Start()
    {
        _state.Subscribe(state =>
        {
            foreach (var action in _actionsBeforeStateChange)
            {
                action();
            }
            _actionsBeforeStateChange.Clear();
            
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
            _disposables.Clear();

            switch (state)
            {
                case State.Start:
                {
                    _disposables.Add(Observable.Timer(TimeSpan.FromSeconds(0.1)).Subscribe(_ =>
                    {
                        TitleScreen.Show();
                        _state.Value = State.LoginForm;
                    }).AddTo(gameObject));
                    break;
                }
                case State.LoginForm:
                {
                    _disposables.Add(TitleScreen.OnClickButtonLogin.Subscribe(_ =>
                    {
                        if (TitleScreen.PlayerId.Value == "")
                        {
                            TitleScreen.SetLoginWarningMessage("ユーザーIDを入力してください");
                        }
                        else if (TitleScreen.Password.Value == "")
                        {
                            TitleScreen.SetLoginWarningMessage("パスワードを入力してください");
                        }
                        else
                        {
                            _state.Value = State.Login;
                        }
                    }).AddTo(gameObject));
                    
                    _actionsBeforeStateChange.Add(() =>
                    {
                        TitleScreen.HideLoginForm();
                        TitleScreen.SetLoginWarningMessage("");
                        TitleScreen.SetInteractableLoginForm(false); 
                    });
                    TitleScreen.ShowLoginForm();
                    TitleScreen.SetInteractableLoginForm(true);
                    break;
                }
                case State.Login:
                {
                    _disposables.Add(NetworkManager.OnFinishLogin.Subscribe(result =>
                    {
                        switch (result)
                        {
                            case User user:
                            {
                                var avatars = new List<(string token, string name)>();
                                
                                foreach (var avatar in user.vrms)
                                {
                                    avatars.Add((avatar.token, avatar.name));
                                }

                                MyAvatarModel.SetMyAvatars(avatars);
                                _state.Value = State.Entrance;
                                break;
                            }
                            case Failed:
                            {
                                TitleScreen.SetLoginWarningMessage("ユーザー名もしくはパスワードが違います");
                                _state.Value = State.LoginForm;
                                break;
                            }
                            case Error:
                            {
                                TitleScreen.SetLoginWarningMessage("ネットワークエラー");
                                _state.Value = State.LoginForm;
                                break;
                            }
                        }
                    }).AddTo(gameObject));
                    
                    _actionsBeforeStateChange.Add(() =>
                    {
                        TitleScreen.HideLoginForm();
                    });
                    TitleScreen.ShowLoginForm();
                    
                    NetworkManager.ExternalLogin(TitleScreen.PlayerId.Value, TitleScreen.Password.Value);
                    break;
                }
                case State.Entrance:
                {
                    _disposables.Add(TitleScreen.OnClickButtonEnter.Subscribe(_ =>
                    {
                        _state.Value = State.Enter;
                    }).AddTo(gameObject));
                    
                    _actionsBeforeStateChange.Add(() =>
                    {
                        TitleScreen.HideEntranceForm();
                    });
                    
                    TitleScreen.ShowEntranceForm();
                    break;
                }
                case State.Enter:
                {
                    _disposables.Add(BasicSpawner.OnJoined.Subscribe(_ =>
                    {
                        _state.Value = State.Main;
                    }).AddTo(gameObject));
                    
                    _actionsBeforeStateChange.Add(() =>
                    {
                        TitleScreen.Hide();
                    });
                    BasicSpawner.StartGame();
                    break;
                }
                case State.Main:
                {
                    _actionsBeforeStateChange.Add(() =>
                    {
                        MainScreen.Hide();
                    });
                    
                    MainScreen.Show();
                    break;
                }
            }
        }).AddTo(gameObject);
    }
}
