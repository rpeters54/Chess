using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PieceViewer", menuName = "PieceViewer", order = 0)]
public class PieceViewer : ScriptableObject {

    public PieceSprite whiteSprites;
    public PieceSprite blackSprites;

    public Sprite GetPieceSprite(byte piece) {
        switch (piece) {
            case Piece.White | Piece.Pawn:   return whiteSprites.pawn;
            case Piece.White | Piece.Knight: return whiteSprites.knight;
            case Piece.White | Piece.Bishop: return whiteSprites.bishop;
            case Piece.White | Piece.Rook:   return whiteSprites.rook;
            case Piece.White | Piece.Queen:  return whiteSprites.queen;
            case Piece.White | Piece.King:   return whiteSprites.king;
            case Piece.Black | Piece.Pawn:   return blackSprites.pawn;
            case Piece.Black | Piece.Knight: return blackSprites.knight;
            case Piece.Black | Piece.Bishop: return blackSprites.bishop;
            case Piece.Black | Piece.Rook:   return blackSprites.rook;
            case Piece.Black | Piece.Queen:  return blackSprites.queen;
            case Piece.Black | Piece.King:   return blackSprites.king;
            default:                         return null;
        }
    }
    

    [System.Serializable]
    public class PieceSprite {
        public Sprite pawn, knight, bishop, rook, queen, king;
    }
}
