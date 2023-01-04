using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class PlayerNamePlateManager : MonoBehaviour
{
    private static readonly Subject<Player> _onCreatePlate = new();
    private static readonly Subject<Player> _onDiminishPlate = new();

    public static void CreatePlate(Player player)
    {
        _onCreatePlate.OnNext(player);
    }

    public static void DiminishPlate(Player player)
    {
        _onDiminishPlate.OnNext(player);
    }
    
    [SerializeField] private CanvasPlayerPlate prefabPlayerNamePlate;

    private readonly Dictionary<Player, CanvasPlayerPlate> _playerPlates = new();

    private void Awake()
    {
        _onCreatePlate.Subscribe(player =>
        {
            var plate = Instantiate(prefabPlayerNamePlate);
            plate.SetPlayerName(player.PlayerName.Value);
            _playerPlates.Add(player, plate);
        }).AddTo(gameObject);

        _onDiminishPlate.Subscribe(player =>
        {
            _playerPlates.Remove(player);
        }).AddTo(gameObject);

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                foreach (var pair in _playerPlates)
                {
                    pair.Value.SetPosition(pair.Key.transform.position + new Vector3(0.0f, 2.0f, 0.0f));
                }
            }).AddTo(gameObject);
    }
}
