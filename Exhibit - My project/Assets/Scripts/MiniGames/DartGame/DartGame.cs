using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class DartGame : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    public void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag($"DartBullet"))
        {
            scoreText.text = "Score: " + Random.Range(0, 100);
        }
    }
}