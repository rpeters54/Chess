
using System;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : Player {

    private Evaluator eval;
    
    public AIPlayer() {
        eval = new Evaluator();
    }

    private const int depth = 3;

    public override Move SelectMove(Board board, List<Move> legalMoves, bool whiteToMove) {
        Move bestMove = null;
        int bestEval = whiteToMove ? Int32.MinValue : Int32.MaxValue;
        foreach (Move move in legalMoves) {
            int eval = search(board.MakeMove(move), depth, whiteToMove);
            if (whiteToMove) {
                if (eval >= bestEval) {
                    bestEval = eval;
                    bestMove = move;
                }
            } else {
                if (eval <= bestEval) {
                    bestEval = eval;
                    bestMove = move;
                }
            }
        }
        return bestMove;
    }

    public int search(Board board, int depth, bool whiteToMove) {
        return search(board, depth, whiteToMove, Int32.MinValue, Int32.MaxValue);
    }

    public int search(Board board, int depth, bool whiteToMove, int alpha, int beta) {
        if (depth <= 0) {
            return eval.EvaluatePosition(board);
        }
        
        List<Move> legalMoves = MoveGenerator.GenerateMoves(board);
        if (whiteToMove) {
            int best = Int32.MinValue;
            foreach (Move move in legalMoves) {
                int value = search(board.MakeMove(move), depth-1, false, alpha, beta);
                best = Math.Max(best, value);
                alpha = Math.Max(alpha, best);
                if (beta <= alpha) {
                    break;
                }
            }
            return best;
        } else {
            int best = Int32.MaxValue;
            foreach (Move move in legalMoves) {
                int value = search(board.MakeMove(move), depth-1, false, alpha, beta);
                best = Math.Min(best, value);
                beta = Math.Min(beta, best);
                if (beta <= alpha) {
                    break;
                }
            }
            return best;
        }
    }

}