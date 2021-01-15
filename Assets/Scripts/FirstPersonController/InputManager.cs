﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager _instance;

    public static InputManager Instance {
        get {
            return _instance;
        }
    }
    private PlayerControls playerControls;

    private void Awake() {
        if(_instance != null && _instance != this) {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
        playerControls = new PlayerControls();
        Cursor.visible = false;
    }

    private void OnEnable() {
        playerControls.Enable();
    }

    private void OnDisable() {
        playerControls.Disable();
    }

    public Vector2 GetPlayerMovement() {
        return playerControls.Walking.Movement.ReadValue<Vector2>();
    }

    public Vector2 GetLookVector() {
        return playerControls.Walking.Look.ReadValue<Vector2>();
    }

    public bool PlayerInteract() {
        return playerControls.Walking.Interact.triggered;
    }

    public bool IsRunning() {
        return playerControls.Walking.Run.triggered;
    }
}
