using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using System.Collections.ObjectModel;


public class Board {

    private const string rankNames = "12345678";
    private const string fileNames = "abcdefgh";
    private const string BASE_FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    private const string FEN_PATTERN = @"^([1-8pbnrqkPBNRQK]+(?:/[1-8pbnrqkPBNRQK]+){7})\s+([wb])\s+(-|K?Q?k?q?)\s+(-|[a-e][1-8])\s+([1-9]*[0-9])\s+([1-9]*[0-9])$";
    private const byte NUM_SQUARES = 64;

    
    private byte[] squares;
    private bool whiteKingSideCastle;
    private bool whiteQueenSideCastle;
    private bool blackKingSideCastle;
    private bool blackQueenSideCastle;
    private bool whiteToMove;
    private bool enPassantAvailable;
    private int enPassantSquare;
    private int halfMoveCount;
    private int moveCount;


    private Board() {
        // handle the instance variables
        squares = new byte[NUM_SQUARES];
        Array.Clear(squares, Piece.Empty, squares.Length);

        whiteKingSideCastle = true;
        whiteQueenSideCastle = true;
        blackKingSideCastle = true;
        blackQueenSideCastle = true;
        whiteToMove = true;
        enPassantAvailable = false;

        enPassantSquare = -1;
        halfMoveCount = -1;
        moveCount = -1;
    }

    public Board Clone() {
        Board copy = new Board();
        Array.Copy(this.squares, copy.squares, this.squares.Length);

        copy.whiteKingSideCastle  = this.whiteKingSideCastle;
        copy.whiteQueenSideCastle = this.whiteQueenSideCastle;
        copy.blackKingSideCastle  = this.blackKingSideCastle;
        copy.blackQueenSideCastle = this.blackQueenSideCastle;
        copy.whiteToMove          = this.whiteToMove;
        copy.enPassantAvailable   = this.enPassantAvailable;

        copy.enPassantSquare = this.enPassantSquare;
        copy.halfMoveCount   = this.halfMoveCount;
        copy.moveCount       = this.moveCount;

        return copy;
    }  

    public static Board FromFen(string fen = BASE_FEN) {

        Board board = new Board();

        Regex fenRegex = new Regex(FEN_PATTERN);
        Match m = fenRegex.Match(fen);

        if (!m.Success) {
            Debug.Log("Failure");
            // on failure just use the default setup
            m = fenRegex.Match(BASE_FEN);
        }

        Debug.Log(m.Groups[1].Value);
        Debug.Log(m.Groups[2].Value);
        Debug.Log(m.Groups[3].Value);
        Debug.Log(m.Groups[4].Value);
        Debug.Log(m.Groups[5].Value);
        Debug.Log(m.Groups[6].Value);


        // handle the first segment defining the piece placement
        string boardString = m.Groups[1].Value;
        string[] ranksLastToFirst = boardString.Split('/');

        // parse through each rank and character
        int rank = 7;
        foreach (string rankString in ranksLastToFirst) {
            int file = 0;
            char[] chars = rankString.ToCharArray();
            foreach (char item in chars) {

                if (file >= 8) {
                    //ignore the rest of the case since the string is invalid
                    break;
                }

                switch (item) {
                    case 'P': board.SetSquare(rank, file++, Piece.White | Piece.Pawn);   break;
                    case 'N': board.SetSquare(rank, file++, Piece.White | Piece.Knight); break;
                    case 'B': board.SetSquare(rank, file++, Piece.White | Piece.Bishop); break;
                    case 'R': board.SetSquare(rank, file++, Piece.White | Piece.Rook);   break;
                    case 'Q': board.SetSquare(rank, file++, Piece.White | Piece.Queen);  break;
                    case 'K': board.SetSquare(rank, file++, Piece.White | Piece.King);   break;
                    case 'p': board.SetSquare(rank, file++, Piece.Black | Piece.Pawn);   break;
                    case 'n': board.SetSquare(rank, file++, Piece.Black | Piece.Knight); break;
                    case 'b': board.SetSquare(rank, file++, Piece.Black | Piece.Bishop); break;
                    case 'r': board.SetSquare(rank, file++, Piece.Black | Piece.Rook);   break;
                    case 'q': board.SetSquare(rank, file++, Piece.Black | Piece.Queen);  break;
                    case 'k': board.SetSquare(rank, file++, Piece.Black | Piece.King);   break;
                    default:  file += (byte) (item - '0');                               break;
                }
            }
            rank--;
        }

        // handle the color to move
        string colorString = m.Groups[2].Value;
        byte color = colorString.First() == 'w' ? Piece.White : Piece.Black;
        board.UpdateColor(color);

        //TODO

        // handle castle rights
        string castleString = m.Groups[3].Value;

        board.ClearCastleRights(Piece.White, true, true);
        board.ClearCastleRights(Piece.Black, true, true);
        if (castleString.Contains('K')) board.SetCastleRights(Piece.White, true,  false);
        if (castleString.Contains('Q')) board.SetCastleRights(Piece.White, false, true);
        if (castleString.Contains('k')) board.SetCastleRights(Piece.Black, true,  false);
        if (castleString.Contains('q')) board.SetCastleRights(Piece.Black, false, true);

        // handle en passant
        string enPassantString = m.Groups[4].Value;
        
        board.enPassantSquare = -1;
        if (!enPassantString.Equals("-")) {
            int enPassantRank = fileNames.IndexOf(enPassantString.Last());
            int enPassantFile = rankNames.IndexOf(enPassantString.First());
            board.enPassantSquare = CoordToIndex(enPassantRank, enPassantFile).Value;
        }

        // handle half move clock
        string halfMoveString = m.Groups[5].Value;
        board.halfMoveCount = Int32.Parse(halfMoveString);

        
        // handle full move counter
        string moveString = m.Groups[6].Value;
        board.halfMoveCount = Int32.Parse(moveString);

        return board;
    }


    public Board MakeMove(Move move) {

        // create a clone of the current state
        Board copy = this.Clone();
        
        int start = move.Start();
        int dest = move.Dest();
        
        // do the move
        byte piece = copy.PieceAt(start).Value;
        copy.SetSquare(start, Piece.Empty);
        copy.SetSquare(dest, piece);

        // handle side effects
        switch (move.Flag()) {
            case Move.MoveFlag.Normal:
                if ((piece & Piece.King) != 0) {
                    copy.ClearCastleRights(Piece.Color(piece), true, true);
                }
                if (File(start) == 0 && (piece & Piece.Rook) != 0) {
                    copy.ClearCastleRights(Piece.Color(piece), false, true);
                }
                if (File(start) == 7 && (piece & Piece.Rook) != 0) {
                    copy.ClearCastleRights(Piece.Color(piece), true, false);
                }
                copy.DisableEnPassant();
                break;
            case Move.MoveFlag.KingSideCastle:
                copy.ClearCastleRights(Piece.Color(piece), true, true);
                copy.SetSquare(Rank(start), 7, Piece.Empty);
                copy.SetSquare(Rank(start), 5, (byte) (Piece.Color(piece) | Piece.Rook));
                copy.DisableEnPassant();
                break;
            case Move.MoveFlag.QueenSideCastle:
                copy.ClearCastleRights(Piece.Color(piece), true, true);
                copy.SetSquare(Rank(start), 0, Piece.Empty);
                copy.SetSquare(Rank(start), 3, (byte) (Piece.Color(piece) | Piece.Rook));
                copy.DisableEnPassant();
                break;
            case Move.MoveFlag.PawnTwoForward:
                if (Piece.Color(piece) == Piece.White) {
                    copy.EnableEnPassant(dest - LookupTable.N);
                } else {
                    copy.EnableEnPassant(dest - LookupTable.S);
                }
                break;
            case Move.MoveFlag.EnPassant:
                if (Piece.Color(piece) == Piece.White) {
                    copy.SetSquare(dest - LookupTable.N, Piece.Empty);
                } else {
                    copy.SetSquare(dest - LookupTable.S, Piece.Empty);
                }
                copy.DisableEnPassant();
                break; 
            case Move.MoveFlag.PromoteToKnight:
                copy.SetSquare(dest, (byte) (Piece.Color(piece) | Piece.Knight));
                copy.DisableEnPassant();
                break;
            case Move.MoveFlag.PromoteToBishop:
                copy.SetSquare(dest, (byte) (Piece.Color(piece) | Piece.Bishop));
                copy.DisableEnPassant();
                break;
            case Move.MoveFlag.PromoteToRook:
                copy.SetSquare(dest, (byte) (Piece.Color(piece) | Piece.Rook));
                copy.DisableEnPassant();
                break;
            case Move.MoveFlag.PromoteToQueen:
                copy.SetSquare(dest, (byte) (Piece.Color(piece) | Piece.Queen));
                copy.DisableEnPassant();
                break;
            default:
                break;
        }
        copy.FlipColor();
        return copy;
    }


    //////////////////////////////////////////////////////////////////////
    /// Square Getters/Setters
    //////////////////////////////////////////////////////////////////////
    
    public byte[] Squares() {
        return squares;
    }

    /// <summary>
    /// Get the piece at a certain (rank, file).
    /// </summary>
    /// <param name="rank"></param>
    /// <param name="file"></param>
    /// <returns>The piece at board[rank * 8 + file] or null if out-of-bounds.</returns>
    public byte? PieceAt(int rank, int file) {
        int? index = CoordToIndex(rank, file);
        if (index.HasValue) {
            return squares[index.Value];
        } else {
            return null;
        }
    }

    /// <summary>
    /// Get the piece at a certain index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns>The piece at board[index] or null if out-of-bounds.</returns>
    public byte? PieceAt(int index) {
        if (index >= 0 && index <= 63) {
            return squares[index];
        } else {
            return null;
        }
    }

    /// <summary>
    /// Set the square at a certain (rank, file) to be piece.
    /// Does nothing if (rank, file) is out-of-bounds.
    /// </summary>
    /// <param name="rank"></param>
    /// <param name="file"></param>
    /// <param name="piece"></param>
    public void SetSquare(int rank, int file, byte piece) {
        int? index = CoordToIndex(rank, file);
        if (index.HasValue) {
            squares[index.Value] = piece;
        }
    }

    /// <summary>
    /// Set the square at a certain index to be piece.
    /// Does nothing if index is out-of-bounds.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="piece"></param>
    public void SetSquare(int index, byte piece) {
        if (index >= 0 && index <= 63) {
            squares[index] = piece;
        }
    }

    public static int? CoordToIndex(int rank, int file) {
        if (rank < 0 || rank > 7 || file < 0 || file > 7) {
            return null;
        } else {
            return (byte) (rank * 8 + file);
        }
    }

    public static Tuple<int, int> IndexToCoord(byte index) {
        if (index < 0 || index > NUM_SQUARES - 1) {
            return null;
        } else {
            return new Tuple<int, int>((index & 0xFFF8) >> 3, index & 7);
        }
    }

    /// <summary>
    /// Get the file component of an index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns>An int corresponding to the file of the index</returns>
    public static int File(int index) {
        return index & 0x0007;
    }

    /// <summary>
    /// Get the rank component of an index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns>An int corresponding to the rank of the index</returns>
    public static int Rank(int index) {
        return (index & 0xFFF8) >> 3;
    }


    //////////////////////////////////////////////////////////////////////
    /// En Passant Methods
    //////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Enable En Passant Capture for any pawn attacking the square index
    /// </summary>
    /// <param name="index"></param>
    public void EnableEnPassant(int index) {
        enPassantAvailable = true;
    }

    /// <summary>
    /// Disable En Passant Capture and clear the enPassantSquare
    /// </summary>
    public void DisableEnPassant() {
        enPassantAvailable = false;
    }

    /// <summary>
    ///  
    /// </summary>
    /// <returns>true/false if En Passant capture is available at a square.</returns>
    public bool EnPassantAvailable() {
        return enPassantAvailable;
    }

    /// <summary>
    ///  
    /// </summary>
    /// <returns>An int corresponding to the index of the square En Passant Capture can be done.</returns>
    public int EnPassantLocation() {
        return enPassantSquare;
    }


    //////////////////////////////////////////////////////////////////////
    /// Castle-Rights Getter/Setter
    //////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Get the king a queenside castle rights for a certain color.
    /// </summary>
    /// <param name="color"></param>
    /// <returns>
    /// A tuple of booleans with [0] corresponding to Kingside Castle Rights
    /// and [1] corresponding to Queenside Castle Rights
    /// </returns>
    public Tuple<bool, bool> KingQueenCastleRights(byte color) {
        if (color == Piece.White) {
            return new Tuple<bool, bool>(whiteKingSideCastle, whiteQueenSideCastle);
        } else {
            return new Tuple<bool, bool>(blackKingSideCastle, blackQueenSideCastle);
        }
    }

    /// <summary>
    /// Clears the Castle Rights for a color given two booleans.
    /// </summary>
    /// <param name="color"></param>
    /// <param name="king">true/false for whether kingside rights should be cleared</param>
    /// <param name="queen">true/false for whether queenside rights should be cleared</param>
    public void ClearCastleRights(byte color, bool king, bool queen) {
        if (color == Piece.White) {
            whiteKingSideCastle &= !king;
            whiteQueenSideCastle &= !queen;
        } else {
            blackKingSideCastle &= !king;
            blackQueenSideCastle &= !queen;
        }
    }


    public void SetCastleRights(byte color, bool king, bool queen) {
        if (color == Piece.White) {
            whiteKingSideCastle |= king;
            whiteQueenSideCastle |= queen;
        } else {
            blackKingSideCastle |= king;
            blackQueenSideCastle |= queen;
        }
    }



    public byte ColorToMove() {
        return whiteToMove ? Piece.White : Piece.Black;
    }

    public void FlipColor() {
        whiteToMove ^= true;
    }

    public void UpdateColor(byte color) {
        whiteToMove = color == Piece.White;
    }

    public static string SquareName(int rank, int file) {
        return fileNames[file] + "" + rankNames[rank];
    }

    public List<Tuple<int, byte>> AllIndexPieceOfColor(bool white) {
        List<Tuple<int, byte>> pieces = new();
        byte color_mask = white ? Piece.White : Piece.Black;

        for (byte index = 0; index < NUM_SQUARES; index++) {
            if ((squares[index] & color_mask) != 0) {
                pieces.Add(new Tuple<int, byte>(index, squares[index]));
            }
        }
        return pieces;
    }



    public string BoardToString() {
        StringBuilder sb = new StringBuilder();
        byte i = 0;
        foreach (byte piece in squares) {
            i++;
            switch (piece) {
                case Piece.White | Piece.Pawn:   sb.Append('P'); break;
                case Piece.White | Piece.Knight: sb.Append('N'); break;
                case Piece.White | Piece.Bishop: sb.Append('B'); break;
                case Piece.White | Piece.Rook:   sb.Append('R'); break;
                case Piece.White | Piece.Queen:  sb.Append('K'); break;
                case Piece.White | Piece.King:   sb.Append('Q'); break;
                case Piece.Black | Piece.Pawn:   sb.Append('p'); break;
                case Piece.Black | Piece.Knight: sb.Append('n'); break;
                case Piece.Black | Piece.Bishop: sb.Append('b'); break;
                case Piece.Black | Piece.Rook:   sb.Append('r'); break;
                case Piece.Black | Piece.Queen:  sb.Append('k'); break;
                case Piece.Black | Piece.King:   sb.Append('q'); break;
                default:                         sb.Append(' '); break;
            }
            if (i % 8 == 0) sb.Append('\n');
        }
        return sb.ToString();
    }


    


}
