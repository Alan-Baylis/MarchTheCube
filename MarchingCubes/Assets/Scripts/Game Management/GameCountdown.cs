﻿using UnityEngine;
using System.Collections;

public class GameCountdown : MonoBehaviour {

    PlayerUI playerUI;

    [SerializeField]
    int timeToCountdown;
    float countdownTimer;
    int nextCounterChange;

    static bool timerComplete = false;
    bool timerDepleting;

    GameManager gameManager;

	// Use this for initialization
	void Start () {
        ResetCountdown();
        gameManager = GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update() {            
        if (GetComponent<ObjectManager>().GetCurrentPlayer() != null) {
            playerUI = GetComponent<ObjectManager>().GetCurrentPlayer().GetComponent<PlayerUI>();
            StartCountdown();
        }

        if (timerDepleting)
            countdownTimer -= Time.deltaTime;

        if (countdownTimer <= -1) {
            StopCountdown();

        } else if (countdownTimer <= 0) {
            timerComplete = true;
            playerUI.SetCountdownTimer("Go!!");
            gameManager.StartGame();

        } else if (playerUI != null) {
            playerUI.SetCountdownTimer((Mathf.CeilToInt(countdownTimer)).ToString());
        }
	}

    public void StartCountdown () {
        timerDepleting = true;
    }

    public void StopCountdown () {
        timerDepleting = false;
        playerUI.SetCountdownTimer("");
    }

    public void ResetCountdown() {
        timerComplete = false;
        countdownTimer = timeToCountdown;
        nextCounterChange = timeToCountdown - 1;
    }

    public static bool CountdownComplete () {
        return timerComplete;
    }
}
