
using System;
using System.Collections.Generic;
using UnityEngine;

public class Move {

    public enum MoveFlag {
        Normal, EnPassant, PawnTwoForward, KingSideCastle, QueenSideCastle, PromoteToBishop, PromoteToKnight, PromoteToQueen, PromoteToRook
    }
  

    private readonly MoveFlag flag;
    private readonly int start;
    private readonly int dest;
    private readonly byte victim;

    public Move(MoveFlag flag, int start, int dest, byte victim) {
        this.flag = flag;
        this.start = start;
        this.dest = dest;
        this.victim = victim;
    }

    public MoveFlag Flag() {
        return flag;
    }

    public int Start() {
        return start;
    }

    public int Dest() {
        return dest;
    }

    public byte Victim() {
        return victim;
    }

    public bool Matches(int start, int dest) {
        return this.start == start
            && this.dest == dest;
    }

    public override string ToString() {
        return String.Format("Move from (Rank: {0}, File: {1}) to (Rank: {2}, File: {3})", Board.Rank(start), Board.File(start), Board.Rank(dest), Board.File(dest));
    }

    
}

public static class MoveGenerator {

    private static LookupTable table = new LookupTable();

    public static List<Move> GenerateMoves(Board board) {
        // initialize the move list
        List<Move> moveList = new();
        
        // get all pieces with the proper color to move
        byte movingColor = board.ColorToMove();
        List<Tuple<int, byte>> indexPieceList = board.AllIndexPieceOfColor(movingColor == Piece.White);
        
        foreach (Tuple<int, byte> indexPiece in indexPieceList) {
            (int start, byte piece) = indexPiece;

            if ((piece & Piece.Pawn) != 0) {
                moveList.AddRange(GeneratePawnMoves(board, movingColor, start));
            }

            if ((piece & Piece.King) != 0) {
                moveList.AddRange(GenerateKingMoves(board, movingColor, start));
            }

            if ((piece & Piece.Knight) != 0) {
                moveList.AddRange(GenerateKnightMoves(board, movingColor, start));
            }

            if ((piece & Piece.StraightPiece) != 0) {
                moveList.AddRange(GenerateSlidingMoves(board, movingColor, start, table.straightDistanceToEdge[start], LookupTable.straightDirections));
            }

            if ((piece & Piece.DiagonalPiece) != 0) {
                moveList.AddRange(GenerateSlidingMoves(board, movingColor, start, table.diagonalDistanceToEdge[start], LookupTable.diagonalDirections));
            }
        }

        return FilterIllegalMoves(board, moveList, movingColor);
    }


    
    public static List<Move> GeneratePawnMoves(Board board, byte movingColor, int start) {
        List<Move> pawnMoves = new();

        int startingRank  = (movingColor == Piece.White) ? 1 :  6;
        int promotionRank = (movingColor == Piece.White) ? 6 :  1;
        int forwardOffset = (movingColor == Piece.White) ? LookupTable.N : LookupTable.S;

        byte? inFront   = board.PieceAt(start + forwardOffset);
        byte? twoSpaces = board.PieceAt(start + 2 * forwardOffset);

        bool clearInFront   = inFront.HasValue && inFront.Value == Piece.Empty;
        bool clearTwoSpaces = twoSpaces.HasValue && twoSpaces.Value == Piece.Empty;

        // generate single space move
        if (clearInFront) {
            if (Board.Rank(start + forwardOffset) == promotionRank) {
                pawnMoves.Add(new Move(Move.MoveFlag.PromoteToBishop, start, start + forwardOffset, Piece.Empty));
                pawnMoves.Add(new Move(Move.MoveFlag.PromoteToKnight, start, start + forwardOffset, Piece.Empty));
                pawnMoves.Add(new Move(Move.MoveFlag.PromoteToRook,   start, start + forwardOffset, Piece.Empty));
                pawnMoves.Add(new Move(Move.MoveFlag.PromoteToQueen,  start, start + forwardOffset, Piece.Empty));
            } else {
                pawnMoves.Add(new Move(Move.MoveFlag.Normal, start, start + forwardOffset, Piece.Empty));
            }
        }

        // generate jump move
        if (clearInFront && clearTwoSpaces && (Board.Rank(start) == startingRank)) {
            pawnMoves.Add(new Move(Move.MoveFlag.PawnTwoForward, start, start + 2 * forwardOffset, twoSpaces.Value));
        }

        // generate normal and en passant captures 
        foreach (int fileOffset in table.pawnCaptureOffsets) {
            byte? dest = board.PieceAt(start + forwardOffset + fileOffset);
            if (!dest.HasValue) {continue;}

            byte destPiece = dest.Value;
            if (Piece.IsEnemy(movingColor, destPiece)) {
                pawnMoves.Add(new Move(Move.MoveFlag.Normal, start, start + forwardOffset + fileOffset, destPiece));
            }
            int enPassantSquare = board.EnPassantLocation();
            if (board.EnPassantAvailable() && (start + forwardOffset + fileOffset) == enPassantSquare) {
                pawnMoves.Add(new Move(Move.MoveFlag.EnPassant, start, start + forwardOffset + fileOffset, destPiece));
            }
        }
        
        return pawnMoves;
    }

    public static List<Move> GenerateKingMoves(Board board, byte movingColor, int start) {
        List<Move> kingMoves = new();
        (bool kingSideCastleRights, bool queenSideCastleRights) = board.KingQueenCastleRights(movingColor);

        // handle all basic radial moves
        foreach (int dest in table.kingMoves[start]) {
            byte destPiece = board.PieceAt(dest).Value;

            if (destPiece == Piece.Empty || Piece.IsEnemy(movingColor, destPiece)) {
                kingMoves.Add(new Move(Move.MoveFlag.Normal, start, dest, destPiece));
            }
        }

        // handle castling
        if (kingSideCastleRights) {
            byte destPiece      = board.PieceAt(start + 2).Value;
            byte inbetweenPiece = board.PieceAt(start + 1).Value;
            byte kingPiece      = board.PieceAt(start).Value;

            bool attacked = IsAttacked(board, Piece.Color(kingPiece), start)
                        ||  IsAttacked(board, Piece.Color(kingPiece), start+1)
                        ||  IsAttacked(board, Piece.Color(kingPiece), start+2);

            if (!attacked && destPiece == Piece.Empty && inbetweenPiece == Piece.Empty) {
                kingMoves.Add(new Move(Move.MoveFlag.KingSideCastle, start, start+2, Piece.Empty));
            }
        }

        if (queenSideCastleRights) {
            byte afterPiece     = board.PieceAt(start - 3).Value;
            byte destPiece      = board.PieceAt(start - 2).Value;
            byte inbetweenPiece = board.PieceAt(start - 1).Value;
            byte kingPiece      = board.PieceAt(start).Value;

            bool attacked = IsAttacked(board, Piece.Color(kingPiece), start)
                        ||  IsAttacked(board, Piece.Color(kingPiece), start-1)
                        ||  IsAttacked(board, Piece.Color(kingPiece), start-2);
            
            if (!attacked && afterPiece == Piece.Empty && destPiece == Piece.Empty && inbetweenPiece == Piece.Empty) {
                kingMoves.Add(new Move(Move.MoveFlag.QueenSideCastle, start, start-2, Piece.Empty));
            }
        }

        return kingMoves;
    }

    public static List<Move> GenerateKnightMoves(Board board, byte movingColor, int start) {
        List<Move> knightMoves = new();

        foreach (int dest in table.knightMoves[start]) {
            byte destPiece = board.PieceAt(dest).Value;
            if (destPiece == Piece.Empty || Piece.IsEnemy(movingColor, destPiece)) {
                knightMoves.Add(new Move(Move.MoveFlag.Normal, start, dest, destPiece));
            }
        }

        return knightMoves;
    }

    public static List<Move> GenerateSlidingMoves(Board board, byte movingColor, int start, int[] maxStepsByDirection, int[] directions) {
        List<Move> slidingMoves = new();

        for (int index = 0; index < maxStepsByDirection.Length; index++) {
            int maxSteps = maxStepsByDirection[index];
            int direction = directions[index];
            int stop = start;
            for (int step = 0; step < maxSteps; step++) {
                stop += direction;
                byte destPiece = board.PieceAt(stop).Value;
                if (destPiece == Piece.Empty || Piece.IsEnemy(movingColor, destPiece)) {
                    slidingMoves.Add(new Move(Move.MoveFlag.Normal, start, stop, destPiece));
                }
                if (destPiece != Piece.Empty) {
                    break;
                }
            }
        }
        return slidingMoves;
    }

    public static bool IsAttacked(Board board, byte color, int location) {
        int[] maxStraightSteps = table.straightDistanceToEdge[location];
        int[] maxDiagonalSteps = table.diagonalDistanceToEdge[location];
        int[] straightDirections = LookupTable.straightDirections;
        int[] diagonalDirections = LookupTable.diagonalDirections;

        for (int index = 0; index < maxStraightSteps.Length; index++) {
            int diagonalStep = location;
            int straightStep = location;
            for (int step = 0; step < maxStraightSteps[index]; step++) {
                straightStep += straightDirections[index];
                byte possibleStraightAttacker = board.PieceAt(straightStep).Value;
                if (Piece.IsFriendly(color, possibleStraightAttacker) 
                || (Piece.IsEnemy(color, possibleStraightAttacker) && !Piece.IsStraightMovingEnemy(color, possibleStraightAttacker))) {
                    break;
                }
                if (Piece.IsStraightMovingEnemy(color, possibleStraightAttacker)) {
                    UnityEngine.Debug.Log("Attacked By Straight (rank: " + Board.Rank(straightStep) 
                    + ", file: " + Board.File(straightStep) + ")");
                    return true;
                }
            }
            for (int step = 0; step < maxDiagonalSteps[index]; step++) {
                diagonalStep += diagonalDirections[index];
                byte possibleDiagonalAttacker = board.PieceAt(diagonalStep).Value;
                if (Piece.IsFriendly(color, possibleDiagonalAttacker)
                || (Piece.IsEnemy(color, possibleDiagonalAttacker) && !Piece.IsDiagonalMovingEnemy(color, possibleDiagonalAttacker))) {
                    break;
                }
                if (Piece.IsDiagonalMovingEnemy(color, possibleDiagonalAttacker)) {
                    UnityEngine.Debug.Log("Attacked By Diagonal (rank: " + Board.Rank(diagonalStep) 
                    + ", file: " + Board.File(diagonalStep) + ")");
                    return true;
                }
            } 
        }

        List<int> pawnLocations = color == Piece.White ? table.attackingBlackPawns[location] : table.attackingWhitePawns[location];
        foreach (int pawnLocation in pawnLocations) {
            byte possiblePawn = board.PieceAt(pawnLocation).Value;
            if (Piece.IsEnemyKing(color, possiblePawn)) {
                UnityEngine.Debug.Log("Attacked By Pawn (rank: " + Board.Rank(pawnLocation) 
                + ", file: " + Board.File(pawnLocation) + ")");
                return true;
            }
        }

        foreach (int kingLocation in table.kingMoves[location]) {
            byte possibleKing = board.PieceAt(kingLocation).Value;
            if (Piece.IsEnemyKing(color, possibleKing)) {
                Debug.Log("Attacked By King (rank: " + Board.Rank(kingLocation) 
                + ", file: " + Board.File(kingLocation) + ")");
                return true;
            }
        }

        foreach (int knightLocation in table.knightMoves[location]) {
            byte possibleKnight = board.PieceAt(knightLocation).Value;
            if (Piece.IsEnemyKnight(color, possibleKnight)) {
                Debug.Log("Attacked By Knight (rank: " + Board.Rank(knightLocation) 
                + ", file: " + Board.File(knightLocation) + ")");
                return true;
            }
        }
        
        return false;
    }

    public static List<Move> FilterIllegalMoves(Board board, List<Move> pseudoLegalMoves, byte color) {
        List<Move> legalMoves = new();

        Debug.Log(String.Format("Checking for {0} King Safety", color == Piece.White ? "White" : "Black"));
        foreach (Move pseudoLegalMove in pseudoLegalMoves) {
            
            Board copy = board.MakeMove(pseudoLegalMove);
            if (!InCheck(copy, color)) {
                legalMoves.Add(pseudoLegalMove);
            }
        }

        return legalMoves;
    }

    public static bool InCheck(Board board, byte color) {
        int kingLocation = -1;
        for (int index = 0; index < 64; index++) {
            if (board.PieceAt(index) == (color | Piece.King)) {
                kingLocation = index;
            }
        }

        if (kingLocation == -1) {
            Debug.Log("King Missing");
            return true;
        }

        return IsAttacked(board, color, kingLocation);
    }


}