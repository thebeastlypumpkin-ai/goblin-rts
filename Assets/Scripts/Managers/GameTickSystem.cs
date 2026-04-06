using System;
using UnityEngine;

public class GameTickSystem : MonoBehaviour
{
    public static GameTickSystem Instance;

    public event Action OnTick;

    [SerializeField] private float tickRate = 0.2f; // 5 times per second

    private float timer;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= tickRate)
        {
            timer = 0f;
            OnTick?.Invoke();
        }
    }
}