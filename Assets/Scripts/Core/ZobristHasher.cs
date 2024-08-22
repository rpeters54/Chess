
using System;

public class ZobristHasher {

    private uint[][] hasher;
    private uint whiteToMoveHash;

    public ZobristHasher() {
        hasher = new uint[64][];
        for (int i = 0; i < 64; i++) {
            hasher[i] = new uint[12];
        }
        PrecomputeZobristArrays();
    }

    public void PrecomputeZobristArrays() {
        Random rng = new();
        for (int square = 0; square < 64; square++) {
            for (int pieceType = 0; pieceType < 12; pieceType++) {
                hasher[square][pieceType] = (uint) rng.Next(Int32.MinValue, Int32.MaxValue);
            }
        }
        whiteToMoveHash = (uint) rng.Next(Int32.MinValue, Int32.MaxValue);
    }


}
