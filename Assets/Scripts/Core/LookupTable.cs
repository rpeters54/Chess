using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

public class LookupTable {

    public const int SE = -7;
    public const int S = -8;
    public const int SW = -9;
    public const int W = -1;
    public const int E = 1;
    public const int NW = 7;
    public const int N = 8;
    public const int NE = 9;


    public static readonly int[] knightOffsets = {-17, -15, -10, -6, 6, 10, 15, 17};                                
    public static readonly int[] directionOffsets = {SW, S, SE, W, E, NW, N, NE};
    public static readonly int[] straightDirections = {N, S, E, W};
    public static readonly int[] diagonalDirections = {NW, NE, SW, SE};





    public readonly List<int>[] knightMoves;
    public readonly List<int>[] kingMoves;
    public readonly List<int>[] attackingWhitePawns;
    public readonly List<int>[] attackingBlackPawns;
    public readonly int[][] diagonalDistanceToEdge;
    public readonly int[][] straightDistanceToEdge;

    public readonly int[] pawnCaptureOffsets = {-1, 1};

    public LookupTable() {
        knightMoves = PrecomputeKnightMoves();
        kingMoves = PrecomputeKingMoves();
        diagonalDistanceToEdge = PrecomputeDiagonalDistanceToEdge();
        straightDistanceToEdge = PrecomputeStraightDistanceToEdge();
        attackingWhitePawns = PreomputeAttackingPawnLocations(Piece.White);
        attackingBlackPawns = PreomputeAttackingPawnLocations(Piece.Black);

    }

    private List<int>[] PrecomputeKnightMoves() {
        List<int>[] movesets = new List<int>[64];

        for (int square = 0; square < 64; square++) {
            List<int> currentMoveset = new();
            foreach (int offset in knightOffsets) {
                int result = square + offset;
                if (result >= 0 && result <= 63) {
                    currentMoveset.Add(result);
                }
            }
            movesets[square] = currentMoveset;
        }

        return movesets;
    }

    private List<int>[] PrecomputeKingMoves() {
        List<int>[] movesets = new List<int>[64];

        for (int square = 0; square < 64; square++) {
            List<int> currentMoveset = new();
            foreach (int offset in directionOffsets) {
                int result = square + offset;
                if (result >= 0 && result <= 63) {
                    currentMoveset.Add(result);
                }
            }
            movesets[square] = currentMoveset;
        }

        return movesets;
    }

    private int[][] PrecomputeStraightDistanceToEdge() {
        int[][] distances =  new int[64][];
        Debug.Log("Straight");
        for (int square = 0; square < 64; square++) {
            distances[square] = new int[4];
            distances[square][0] = 7 - Board.Rank(square);  //N
            distances[square][1] = Board.Rank(square);      //S
            distances[square][2] = 7 - Board.File(square);  //E
            distances[square][3] = Board.File(square);      //W
        }
        return distances;
    }

    private int[][] PrecomputeDiagonalDistanceToEdge() {
        int[][] distances =  new int[64][];
        Debug.Log("Diagonal");
        for (int square = 0; square < 64; square++) {
            distances[square] = new int[4];
            distances[square][0] = Math.Min(7 - Board.Rank(square), Board.File(square));        //NW
            distances[square][1] = Math.Min(7 - Board.Rank(square), 7 - Board.File(square));    //NE
            distances[square][2] = Math.Min(Board.Rank(square),     Board.File(square));        //SW
            distances[square][3] = Math.Min(Board.Rank(square),     7 - Board.File(square));    //SE
        }
        return distances;
    }


    private int[] blackPawnOffsets = {NE, NW};
    private int[] whitePawnOffsets = {SE, SW};
    private List<int>[] PreomputeAttackingPawnLocations(byte color) {
        List<int>[] allLocations = new List<int>[64];
        int[] offsets = color == Piece.White ? whitePawnOffsets : blackPawnOffsets;
        for (int square = 0; square < 64; square++) {
            List<int> locations = new();
            foreach(int pawnOffset in offsets) {
                int pawnLocation = pawnOffset + square;
                if (pawnLocation >= 0 && pawnLocation <= 63) {
                    locations.Add(pawnLocation);
                }
            }   
            allLocations[square] = locations;
        }
        
        return allLocations;
    }


    




}