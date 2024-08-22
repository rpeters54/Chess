using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class MouseHandler {
    
    public enum PlayerState { WAITING, CLICKED, DRAGGING };
    
    private PlayerState playerState;

    private GameState gameState;

    private int? startIndex;
    private int? destIndex;
    private Camera camera;

    public MouseHandler() {
        this.playerState = PlayerState.WAITING;
        this.gameState = GameObject.FindObjectOfType<GameState>();
        this.camera = Camera.main;
        this.startIndex = null;
        this.destIndex = null;
    }

    public (int start, int dest)? Poll() {
        Vector2 mousePosition = camera.ScreenToWorldPoint(Input.mousePosition);
        
        switch (playerState) {
            case PlayerState.WAITING:
                AwaitSelection(mousePosition);
                break;
            case PlayerState.DRAGGING:
                Debug.Log("Dragging My Balls");
                WhileDragging(mousePosition);
                break;
            case PlayerState.CLICKED:
                AwaitPlacement(mousePosition);
                break;
            default:
                Debug.Log("Invalid State");
                playerState = PlayerState.WAITING;
                break;
        }

        if (startIndex != null && destIndex != null) {
            (int, int) retval = (startIndex.Value, destIndex.Value);
            startIndex = null;
            destIndex = null;
            return retval;
        } else {
            return null;
        }
    }

    public void AwaitSelection(Vector2 mousePosition) {
        if (Input.GetMouseButtonDown(0)) {
            (byte rank, byte file) = gameState.GetSquareAtMouse(mousePosition);
            int? index = Board.CoordToIndex(rank, file);

            if (index != null) {
                playerState = PlayerState.DRAGGING;
                startIndex = index.Value;
            } 
        }
    }
    
    public void WhileDragging(Vector2 mousePosition) {
        if (Input.GetMouseButtonUp(0)) {
            (byte rank, byte file) = gameState.GetSquareAtMouse(mousePosition);
            int? index = Board.CoordToIndex(rank, file);

            if (index == null || index.Value == startIndex.Value) {
                playerState = PlayerState.CLICKED;
            } else {
                playerState = PlayerState.WAITING;  
                destIndex = index.Value;
            }    
        }
    }

    public void AwaitPlacement(Vector2 mousePosition) {
        if (Input.GetMouseButtonDown(0)) {
            (byte rank, byte file) = gameState.GetSquareAtMouse(mousePosition);
            int? index = Board.CoordToIndex(rank, file);

            playerState = PlayerState.WAITING; 
            if (index == null || index.Value == startIndex.Value) {
                startIndex = null;
            } else {
                destIndex = index.Value;
            }
        }
    }

}
