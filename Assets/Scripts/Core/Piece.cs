using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class Piece {

    // bitboard values for the pieces
    public const byte Empty = 0x00;
    public const byte Pawn = 0x01;
    public const byte Knight = 0x02;
    public const byte Bishop = 0x04;
    public const byte Rook = 0x08;
    public const byte Queen = 0x10;
    public const byte King = 0x20;
    public const byte White = 0x40;
    public const byte Black = 0x80;

    // bitmasks to sieve the piece and color values
    public const byte PieceMask = 0x3F;
    public const byte ColorMask = 0xC0;

    public const byte StraightPiece = Rook | Queen;
    public const byte DiagonalPiece = Bishop | Queen;

    public static bool IsFriendly(byte color, byte possibleEnemy) {
        return (color & possibleEnemy) != 0 && possibleEnemy != Piece.Empty;
    }

    public static bool IsEnemy(byte color, byte possibleEnemy) {
        return (color & possibleEnemy) == 0 && possibleEnemy != Piece.Empty;
    }

    public static bool IsStraightMovingEnemy(byte color, byte possibleEnemy) {
        return (color & possibleEnemy) == 0 
        && (possibleEnemy & Piece.StraightPiece) != 0;
    }

    public static bool IsDiagonalMovingEnemy(byte color, byte possibleEnemy) {
        return (color & possibleEnemy) == 0 
        && (possibleEnemy & Piece.DiagonalPiece) != 0;
    }

    public static bool IsEnemyKing(byte color, byte possibleEnemy) {
        return (color & possibleEnemy) == 0 
        && (possibleEnemy & Piece.King) != 0;
    }

    public static bool IsEnemyPawn(byte color, byte possibleEnemy) {
        return (color & possibleEnemy) == 0 
        && (possibleEnemy & Piece.Pawn) != 0;
    }

    public static bool IsEnemyKnight(byte color, byte possibleEnemy) {
        return (color & possibleEnemy) == 0 
        && (possibleEnemy & Piece.Knight) != 0;
    }

    public static byte Color(byte piece) {
        return (piece & Piece.White) != 0 ? Piece.White : Piece.Black;
    }

    public static string ToString(byte piece) {
        string str = "";
        if ((piece & Piece.White) != 0)  str += "White";
        if ((piece & Piece.Black) != 0)  str += "Black";
        if ((piece & Piece.Pawn) != 0)   str += "Pawn";
        if ((piece & Piece.Knight) != 0) str += "Knight";
        if ((piece & Piece.Bishop) != 0) str += "Bishop";
        if ((piece & Piece.Rook) != 0)   str += "Rook";
        if ((piece & Piece.Queen) != 0)  str += "Queen";
        if ((piece & Piece.King) != 0)   str += "King";
        return str;
    }

   


}