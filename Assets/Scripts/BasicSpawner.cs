using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using NetworkLogin;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    private static readonly Subject<Unit> _onConnectedServer = new();
    public static IObservable<Unit> OnConnectedServer => _onConnectedServer;

    private static readonly Subject<Unit> _onStartGame = new();
    private static readonly Subject<Unit> _onDisconnect = new();

    public static void StartGame()
    {
        _onStartGame.OnNext(Unit.Default);
    }

    public static void Disconnect()
    {
        _onDisconnect.OnNext(Unit.Default);
    }
    
    [SerializeField] private NetworkRunner networkRunner;
    [SerializeField] private NetworkSceneManagerDefault networkSceneManagerDefault;
    [SerializeField] private NetworkPrefabRef playerPrefab;

    private readonly Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new();

    private void Awake()
    {
        _onStartGame.Subscribe(_ =>
        {
            StartCoroutine(CoroutineStartGame());
        }).AddTo(gameObject);

        _onDisconnect.Subscribe(_ =>
        {
            networkRunner.Disconnect(networkRunner.LocalPlayer);
        }).AddTo(gameObject);
    }

    private IEnumerator CoroutineStartGame()
    {
        networkRunner.ProvideInput = true;

        yield return networkRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = "Webverse",
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneManager = networkSceneManagerDefault
        });
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            var positionX = UnityEngine.Random.Range(-10.0f, 10.0f);
            var positionZ = UnityEngine.Random.Range(-10.0f, 10.0f);
            var spawnPosition = new Vector3(positionX, 0.0f, positionZ);
            var networkPlayerObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);

            if (networkPlayerObject.HasStateAuthority)
            {
                if (networkPlayerObject.TryGetComponent(out Player component))
                {
                    component.VrmToken = MyAvatarModel.Avatars.Value[MyAvatarModel.AvatarIndex.Value].token;
                }
            }
            _spawnedCharacters.Add(player, networkPlayerObject);
        }
        
        Debug.Log(runner.LocalPlayer);
        Debug.Log(player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out var networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();
        
        if (Input.GetKey(KeyCode.UpArrow))
        {
            data.direction += Vector3.forward;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            data.direction += Vector3.back;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            data.direction += Vector3.left;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            data.direction += Vector3.right;
        }
        
        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {

    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("こっちかな");
    }

    public void OnConnectedToServer()
    {
        Debug.Log("あれれ");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {

    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {

    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log("Connect Failed");
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {

    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {

    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {

    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {

    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {

    }
}
