

using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : Player {

    private MouseHandler handler;

    public HumanPlayer() {
        handler = new MouseHandler();
    }

    public override Move SelectMove(Board board, List<Move> legalMoves, bool whiteToMove) {
        
        if (legalMoves.Count == 0) {
            return null;
        }

        (int, int)? moveTuple = handler.Poll();

        if (moveTuple == null) {
            return null;
        }

        (int start, int dest) = moveTuple.Value;

        foreach (Move move in legalMoves) {
            if (move.Matches(start, dest)) {
                return move;
            }
        }

        return null;
    }
}