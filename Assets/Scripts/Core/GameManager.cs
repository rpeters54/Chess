using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager {
    private const string BASE_FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    
    private Board board;

    private Player whitePlayer;
    private Player blackPlayer;


    private static Evaluator evaluator;
    private static AIPlayer ai;

    private List<Move> legalMoves;

    private readonly Stack<Board> pastMoves;

    private static Move bestMove = null;


    public GameManager(Player white, Player black, String arrangement = BASE_FEN) {
        board = Board.FromFen(arrangement);
        whitePlayer = white;
        blackPlayer = black;
        legalMoves = MoveGenerator.GenerateMoves(board);
        pastMoves = new Stack<Board>();

        evaluator = new Evaluator();
        ai = new AIPlayer();
    }

    public void Next() {

        if (legalMoves.Count == 0) {
            byte color = board.ColorToMove();
            if (MoveGenerator.InCheck(board, color)) {
                Debug.Log(String.Format("{0} Wins By Checkmate", color == Piece.White ? "Black" : "White"));
            } else {
                Debug.Log(String.Format("Draw By Stalemate"));
            }
            return;
        }

        Debug.Log(String.Format("Current Eval {0}", evaluator.EvaluatePosition(board)));

        Move next = null;
        bestMove ??= ai.SelectMove(board, legalMoves, board.ColorToMove() == Piece.White);
        Debug.Log(String.Format("AI Recommends {0}", bestMove.ToString()));
        switch(board.ColorToMove()) {
            case Piece.White:
                next = whitePlayer.SelectMove(board, legalMoves, true);
                break;
            case Piece.Black:
                next = blackPlayer.SelectMove(board, legalMoves, false);
                break;
            default:
                // error state
                Debug.Log("Corrupted GameBoard Caused by Invalid Color at GameManager::Play");
                board = Board.FromFen(BASE_FEN);
                pastMoves.Clear();
                break;
        }

        if (next != null) {
            Board nextPosition = board.MakeMove(next);
            pastMoves.Push(board);
            board = nextPosition;
            legalMoves = MoveGenerator.GenerateMoves(board);
            bestMove = null;
        }
    }

    public Board CurrentBoard() {
        return board;
    }

}