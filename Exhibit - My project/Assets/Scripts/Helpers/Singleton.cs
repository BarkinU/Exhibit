using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton : MonoBehaviour
{
    public static Singleton Main;

    #region Singletons

    [HideInInspector] public GameManager gameManager;
    [HideInInspector] public UIManager uiManager;
    [HideInInspector] public NetworkManager NetworkManager;
    [HideInInspector] public Utils Utils;
    [HideInInspector] public ObjectPool objectPool;

    #endregion

    private void Awake()
    {
        if (Main == null)
        {
            Main = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        gameManager = GetComponentInChildren<GameManager>();
        uiManager = GetComponentInChildren<UIManager>();
        objectPool = GetComponentInChildren<ObjectPool>();

        //Sinan
        NetworkManager = GetComponentInChildren<NetworkManager>();
        Utils = GetComponentInChildren<Utils>();
    }
}