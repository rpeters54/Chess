
using System.Collections.Generic;

public abstract class Player {
    public abstract Move SelectMove(Board board, List<Move> legalMoves, bool whiteToMove);

}