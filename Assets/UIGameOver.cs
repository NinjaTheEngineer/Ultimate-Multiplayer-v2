using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGameOver : MonoBehaviour
{
    private Canvas _gameOverCanvas;

    private void Start() {
        _gameOverCanvas = GetComponent<Canvas>();
    }

    private void OnEnable() {
        PlayerController.GameOverEvent += GameOver;
    }
    private void OnDisable() {
        PlayerController.GameOverEvent -= GameOver;
    }

    private void GameOver() {
        if (_gameOverCanvas != null) {
            _gameOverCanvas.enabled = true;
        }
    }
}
