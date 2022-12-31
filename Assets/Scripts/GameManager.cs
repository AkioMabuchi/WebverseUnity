using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    [SerializeField] private TextMeshProUGUI textMeshPro;
    [SerializeField] private RawImage rawImage;
    
    private readonly List<Coroutine> _coroutines = new();
    private void Awake()
    {

    }



    private void HaltAllCoroutines()
    {
        foreach (var coroutine in _coroutines)
        {
            StopCoroutine(coroutine);
        }
        _coroutines.Clear();
    }
}
