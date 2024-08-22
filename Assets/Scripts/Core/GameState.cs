using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public PieceViewer pieceViewer;
    public BoardViewer boardViewer;

    private MeshRenderer[, ] squareRenderers;
	private SpriteRenderer[, ] squarePieceRenderers;

    private GameManager manager;

    // Start is called before the first frame update
    void Start() {
        manager = new GameManager(new HumanPlayer(), new HumanPlayer());
        Init();
    }

    void Update() {
        manager.Next();
        UpdatePieces();
        UpdateSquares();
    }

    public void Init() {
        Shader squareShader = Shader.Find("Unlit/Color");
		squareRenderers = new MeshRenderer[8, 8];
		squarePieceRenderers = new SpriteRenderer[8, 8];

		for (byte rank = 0; rank < 8; rank++) {
			for (byte file = 0; file < 8; file++) {
				// Create square
				Transform square = GameObject.CreatePrimitive (PrimitiveType.Quad).transform;
				square.parent = transform;
				square.name = Board.SquareName(rank, file);
				square.position = ScreenPosition(rank, file);	
                Material squareMaterial = new Material(squareShader);

				squareRenderers[rank, file] = square.gameObject.GetComponent<MeshRenderer> ();
			    squareRenderers[rank, file].material = squareMaterial;

				// Create piece sprite renderer for current square
				SpriteRenderer pieceRenderer = new GameObject("Piece").AddComponent<SpriteRenderer> ();
				pieceRenderer.transform.parent = square;
				pieceRenderer.transform.position = ScreenPosition(file, rank);
				pieceRenderer.transform.localScale = Vector3.one * 250 / (2000 / 6f);
				squarePieceRenderers[rank, file] = pieceRenderer;
			}
		}

        UpdatePieces();
        UpdateSquares();
    }


    public void UpdatePieces() {
        for (byte rank = 0; rank < 8; rank++) {
			for (byte file = 0; file < 8; file++) {
                byte piece = manager.CurrentBoard().PieceAt(rank, file).Value;
                squarePieceRenderers[rank, file].sprite = pieceViewer.GetPieceSprite(piece);
				squarePieceRenderers[rank, file].transform.position = ScreenPosition(rank, file);
            }
        }
    }

    public void UpdateSquares() {
        for (byte rank = 0; rank < 8; rank++) {
			for (byte file = 0; file < 8; file++) {
                Color squareColor = (rank + file) % 2 == 0 ? boardViewer.darkSquareSprite.normal : boardViewer.lightSquareSprite.normal;
                squareRenderers[rank, file].material.color = squareColor;
            }
        }
    }

    public Vector3 ScreenPosition(byte rank, byte file) {
        return new Vector3 (-3.5f + file, -3.5f + rank, 0);
    }

    public (byte rank, byte file) GetSquareAtMouse(Vector2 mousePosition) {
        byte file = (byte) Math.Round(mousePosition.x + 3.5f);
        byte rank = (byte) Math.Round(mousePosition.y + 3.5f);
        if (file < 0 || file > 7 || rank < 0 || rank > 7) {
            return (Byte.MaxValue, Byte.MaxValue);
        }
        return (rank, file);
    }

}
