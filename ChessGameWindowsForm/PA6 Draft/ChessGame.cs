using System;
using System.Collections.Generic;
using System.Media;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PA6_Draft
{
    internal delegate object ChessEvent(Move move);
    internal delegate void SoundEvent();

    enum Piece { 
    BPAWN,
    WPAWN,
    BKNIGHT,
    WKNIGHT,
    BBISHOP,
    WBISHOP,
    BROOK,
    WROOK,
    BQUEEN,
    WQUEEN,
    BKING,
    WKING,
    NONE
    }
    enum Castle
    {
        NONE,
        BSHORT,
        WSHORT,
        BLONG=4,
        WLONG=8
    }
    enum Promotion
    {
        BKNIGHT=2,
        WKNIGHT,
        BBISHOP,
        WBISHOP,
        BROOK,
        WROOK,
        BQUEEN,
        WQUEEN,
        NONE
    }
    class Move
    {
        internal int X1;
        internal int Y1;
        internal int X2;
        internal int Y2;
        internal bool EnPassant;
        internal bool Check;
        internal bool Checkmate;
        internal bool Stalemate;
        internal Castle Castled;
        internal Piece CapturedPiece;
        internal Piece MovedPiece;
        internal Promotion Promoted;
        internal Move(int x1,int y1,int x2,int y2)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            EnPassant = Check = Checkmate = Stalemate = false;
            Castled = Castle.NONE;
            CapturedPiece = Piece.NONE;
            MovedPiece = Piece.NONE;
            Promoted = Promotion.NONE;
        }
        public override string ToString()//we are making a algebraic notation for the
        {
            string result = ((int)MovedPiece%2==0)?"\t\t":"";
            if (Castled == Castle.BLONG || Castled == Castle.WLONG)
                return result+"O-O-O";
            if (Castled == Castle.BSHORT || Castled == Castle.WSHORT)
                return result+"O-O";
            switch (MovedPiece)
            {
                case Piece.BROOK:
                case Piece.WROOK:
                    result += "R";
                    break;
                case Piece.BKNIGHT:
                case Piece.WKNIGHT:
                    result += "N";
                    break;
                case Piece.BBISHOP:
                case Piece.WBISHOP:
                    result += "B";
                    break;
                case Piece.BQUEEN:
                case Piece.WQUEEN:
                    result += "Q";
                    break;
                case Piece.BKING:
                case Piece.WKING:
                    result += "K";
                    break;
            }
            if (CapturedPiece != Piece.NONE)
            {
                if(MovedPiece==Piece.BPAWN || MovedPiece==Piece.WPAWN)
                    result += (char)('a' + X1);
                result += "x";
            }
            result += (char)('a' + X2);
            result += (8 - Y2);
            switch (Promoted)
            {
                case Promotion.BQUEEN:
                case Promotion.WQUEEN:
                    result += "(Q)";
                    break;
                case Promotion.BROOK:
                case Promotion.WROOK:
                    result += "(R)";
                    break;
                case Promotion.BBISHOP:
                case Promotion.WBISHOP:
                    result += "(B)";
                    break;
                case Promotion.BKNIGHT:
                case Promotion.WKNIGHT:
                    result += "(N)";
                    break;
            }
            if (Check)
                result += "+";
            if (Checkmate)
                result += ("#"+(((int)MovedPiece%2==0)?"0-1":"1-0"));
            if (Stalemate)
                result += "1/2-1/2";
            return result;
        }
    }
    class Square 
    { 
        internal int Rank { get; }
        internal char File { get; }
        internal Piece Occupant { get; set; }
        internal Square(int rank, char file)
        {
            this.Rank = rank;
            this.File = file;
            Occupant = Piece.NONE;
        }
        internal Square(int rank, char file,Piece occupant)
        {
            this.Rank = rank;
            this.File = file;
            Occupant = occupant;
        }
        public override bool Equals(object obj)
        {
            return this.Rank == ((Square)obj).Rank && this.File == ((Square)obj).File;
        }
        public override int GetHashCode()
        {
            return Rank.GetHashCode()*23+File.GetHashCode();
        }
        public override string  ToString() {
            return File + "" + Rank;
        }
    }
    class ChessGame
    {
        internal event ChessEvent Promote;
        internal event ChessEvent CheckMate;
        internal event ChessEvent StaleMate;
        internal event ChessEvent Time;
        internal event SoundEvent CaptureFX;
        internal event SoundEvent MoveFX;
        internal event SoundEvent CheckFX;
        internal event SoundEvent CheckmateFX;
        internal event SoundEvent StalemateFX;
        internal Square[][] Board { get; }
        private Square EnPassant = null;
        private Castle CastlePermissions = Castle.BLONG|Castle.WLONG|Castle.BSHORT|Castle.WSHORT;
        internal bool WhiteTurn = true;
        internal long WLimit;
        internal long BLimit;
        internal string Player1Name;
        internal string Player2Name;
        internal string WhiteTimeLimit;
        internal string BlackTimeLimit;
        private long Increment { get; set; }
        internal List<Move> Moves;
        internal BindingList<Move> BindingMoves;

        internal string TimeToString(long milisec)
        {
            string result = "";
            if (milisec >= 10000)
            {
                result += (milisec / 60000);
                result += ":";
                int sec = (int)(milisec % 60000);
                sec /= 1000;
                string secString = sec + "";
                if (secString.Length == 1)
                    secString = "0" + secString;
                result += secString;
                return result;
            }
            result += (milisec / 1000);
            result += ".";
            int centisec = (int)(milisec % 1000);
            centisec /= 10;
            string centisecString = centisec + "";
            if (centisecString.Length == 1)
                centisecString = "0" + centisecString;
            result += centisecString;
            return result;
        }
        public ChessGame(int timeLimit,int increment,string player1,string player2){
            WLimit = BLimit= timeLimit * 60000;
            Increment = increment * 1000;
            Player1Name = player1;
            Player2Name = player2;
            WhiteTimeLimit = TimeToString(WLimit);
            BlackTimeLimit = TimeToString(BLimit);
            Board = new Square[8][];
            BindingMoves = new BindingList<Move>();
            for (int i = 0; i < 8; i++)
                Board[i] = new Square[8];
            for(int i = 0; i < 8;i++)
                for(int j = 0;j < 8;j++)
                {
                    Board[i][j] = new Square(8-j, (char)('a' + i));
                }
            for (int i = 0; i < 8; i++)
            {
                Board[i][1].Occupant = Piece.BPAWN;
                Board[i][6].Occupant = Piece.WPAWN;
            }
            Board[0][0].Occupant = Board[7][0].Occupant = Piece.BROOK;
            Board[0][7].Occupant = Board[7][7].Occupant = Piece.WROOK;
            Board[1][0].Occupant = Board[6][0].Occupant = Piece.BKNIGHT;
            Board[1][7].Occupant = Board[6][7].Occupant = Piece.WKNIGHT;
            Board[2][0].Occupant = Board[5][0].Occupant = Piece.BBISHOP;
            Board[2][7].Occupant = Board[5][7].Occupant = Piece.WBISHOP;
            Board[3][0].Occupant = Piece.BQUEEN;
            Board[4][0].Occupant = Piece.BKING;
            Board[3][7].Occupant = Piece.WQUEEN;
            Board[4][7].Occupant = Piece.WKING;
            Moves = new List<Move>();
        }
        private bool IsCheckmate(bool whiteKing, Move moved)
        {
            if (!IsCheck(whiteKing))
                return false;
            foreach(Move move in AllLegalMoves(whiteKing))
            {
                if (TryLegalMove(move,whiteKing))
                    return false;
            }
            CheckmateFX();
            CheckMate(moved);
            return true; 
        }
        private bool IsStalemate(bool whiteKing, Move moved)
        {
            if (!IsCheck(whiteKing))
            {
                foreach (Move move in AllLegalMoves(whiteKing))
                {
                    if (TryLegalMove(move, whiteKing))
                        return false;
                }
                StalemateFX();
                StaleMate(moved);
                return true;
            }
            return false;
        }
        private bool IsCheck(bool whiteKing)
        {
            Square kingSquare=null;
            for (int i = 0; i < 8; i++)//find the king on the board first!!!
                for (int j = 0; j < 8; j++)
                    if (Board[i][j].Occupant == (whiteKing ? Piece.WKING : Piece.BKING))
                    {   
                        kingSquare = Board[i][j];
                        i = 8;
                        break;
                    }
            if (kingSquare == null)
                return false;
            List<Move> all = AllLegalMoves(!whiteKing);
            foreach (Move move in all)//for every legal move of the opponent
            {
                if (move.X2 == kingSquare.File - 'a' && move.Y2 == 8 - kingSquare.Rank)
                { //if move threatens the king
                    CheckFX();
                    return true;
                }
            }
            return false;
        }
        private List<Move> AllLegalMoves(Square source)
        {
            List<Move> all = new List<Move>();
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    Move move = new Move(source.File - 'a', 8 - source.Rank, i, j);
                    if (LegalMove(move,true))
                        all.Add(move);
                }
                    return all;
        }
        private List<Move> AllLegalMoves(bool whiteTurn)
        {
            List<Move> all = new List<Move>();
            for(int i = 0; i < 8;i++)
                for(int j = 0; j < 8; j++)
                {
                    if (Board[i][j].Occupant == Piece.NONE)
                        continue;
                    if((int)Board[i][j].Occupant%2 == (whiteTurn ? 1 : 0))
                    {
                        List<Move> partial = AllLegalMoves(Board[i][j]);
                        foreach (Move move in partial)
                            all.Add(move);
                    }
                }
            return all;
        }
        private bool LegalMove(Move move,bool ignoreTurn)//Method has no side-effects! It checks whether move is legal. It ignores checks/checkmates/stalemates
        {
            int x1 = move.X1, y1 = move.Y1, x2 = move.X2, y2 = move.Y2;
            if (x1 == x2 && y1 == y2)//source and destination are the same!
                return false;
            if (Board[x1][y1].Occupant == Piece.NONE)//source is an empty square!
                return false;
            if (!ignoreTurn && (int)Board[x1][y1].Occupant%2==(WhiteTurn?0:1))//It's not player's turn to move!
                return false;
            if (Board[x2][y2].Occupant != Piece.NONE && 
                ((int)Board[x1][y1].Occupant + (int)Board[x2][y2].Occupant) % 2 == 0)//a piece wants to capture another piece with the same color!
                return false;
            switch (Board[x1][y1].Occupant)
            {
                case Piece.BPAWN:
                    if (y1 == 4 && y2 == 5 && Math.Abs(x1 - x2) == 1 && Board[x2][y2].Occupant == Piece.NONE)//en passant
                        if (EnPassant != null)
                            if (EnPassant.Equals(Board[x2][4]) && Board[x2][4].Occupant == Piece.WPAWN)
                                return true;
                    if (Board[x2][y2].Occupant == Piece.NONE)//pawn advancement...
                    {
                        if (x2 != x1)
                            return false;
                        if (y2 != y1 + 1 && y1 != 1)
                            return false;
                        if (y1 == 1)//first pawn advancement
                        {
                            if (y2 != 2 && y2 != 3)
                                return false;
                            else if (y2 == 3 && Board[x2][2].Occupant != Piece.NONE)
                                return false;
                        }
                    }
                    else//normal capture...
                        return Math.Abs(x2 - x1) == 1 && y2 - y1 == 1;
                    return true;
                case Piece.WPAWN:
                    if (y1 == 3 && y2 == 2 && Math.Abs(x1 - x2) == 1 && Board[x2][y2].Occupant == Piece.NONE)//en passant
                        if (EnPassant != null)
                            if (EnPassant.Equals(Board[x2][3]) && Board[x2][3].Occupant == Piece.BPAWN)
                                return true;
                    if (Board[x2][y2].Occupant == Piece.NONE)//pawn advancement...
                    {
                        if (x2 != x1)
                            return false;
                        if (y2 != y1 - 1 && y1 != 6)
                            return false;
                        if (y1 == 6)//first pawn advancement
                        {
                            if (y2 != 5 && y2 != 4)
                                return false;
                            else if (y2 == 4 && Board[x2][5].Occupant != Piece.NONE)
                                return false;
                        }
                    }
                    else//normal capture...
                        return Math.Abs(x2 - x1) == 1 && y1 - y2 == 1;
                    return true;
                case Piece.BROOK:
                case Piece.WROOK:
                    if (y1 == y2)//vertical move
                    {
                        for (int x = Math.Min(x1, x2) + 1; x < Math.Max(x1, x2); x++)//rook can't jump over other pieces!
                            if (Board[x][y1].Occupant != Piece.NONE)
                                return false;
                    }
                    else if (x1 == x2)//horizontal move
                    {
                        for (int y = Math.Min(y1, y2) + 1; y < Math.Max(y1, y2); y++)//rook can't jump over other pieces!
                            if (Board[x1][y].Occupant != Piece.NONE)
                                return false;
                    }
                    else
                        return false;
                    return true;
                case Piece.BBISHOP:
                case Piece.WBISHOP:
                    if (Math.Abs(y2 - y1) != Math.Abs(x2 - x1))//bishop can only move diagonally
                        return false;
                    bool slope = (x2 - x1) * (y2 - y1) > 0;
                    for (int x = 1; x < Math.Abs(x2 - x1); x++)//bishop can't jump over other pieces!
                        if (Board[Math.Min(x1, x2) + x][slope ? Math.Min(y1, y2) + x : Math.Max(y1, y2) - x].Occupant != Piece.NONE)
                            return false;
                    return true;
                case Piece.BKNIGHT:
                case Piece.WKNIGHT:
                    if (Math.Abs(y2 - y1) * Math.Abs(x2 - x1) != 2)//knigh moves L shape
                        return false;
                    return true;
                case Piece.BQUEEN:
                case Piece.WQUEEN:
                    if (y1 == y2)//queen can move horizontally
                    {
                        for (int x = Math.Min(x1, x2) + 1; x < Math.Max(x1, x2); x++)//queen can't jump over other pieces
                            if (Board[x][y1].Occupant != Piece.NONE)
                                return false;
                    }
                    else if (x1 == x2)//queen can move vertically
                    {
                        for (int y = Math.Min(y1, y2) + 1; y < Math.Max(y1, y2); y++)//queen can't jump over other pieces
                            if (Board[x1][y].Occupant != Piece.NONE)
                                return false;
                    }
                    else//if queen doesn't move like a rook
                    {
                        if (Math.Abs(y2 - y1) != Math.Abs(x2 - x1))//it must move diagonally
                            return false;
                        slope = (x2 - x1) * (y2 - y1) > 0;
                        for (int x = 1; x < Math.Abs(x2 - x1); x++)//queen can't jump over other pieces
                            if (Board[Math.Min(x1, x2) + x][slope ? Math.Min(y1, y2) + x : Math.Max(y1, y2) - x].Occupant != Piece.NONE)
                                return false;
                    }
                    return true;
                case Piece.BKING:
                    if ((Castle.BSHORT&CastlePermissions)!=0)//short castle is permitted
                    {
                        if (x1 == 4 && y1 == 0 && x2 == 6 && y2 == 0
                            && Board[7][0].Occupant == Piece.BROOK
                            && Board[6][0].Occupant == Piece.NONE
                            && Board[5][0].Occupant == Piece.NONE)
                            return true;
                    }
                    if ((Castle.BLONG & CastlePermissions) != 0)//long castle  is permitted
                    {
                        if (x1 == 4 && y1 == 0 && x2 == 2 && y2 == 0
                            && Board[0][0].Occupant == Piece.BROOK
                            && Board[3][0].Occupant == Piece.NONE
                            && Board[2][0].Occupant == Piece.NONE
                            && Board[1][0].Occupant == Piece.NONE)
                            return true;
                    }
                    return Math.Abs(y2 - y1) <= 1 && Math.Abs(x2 - x1) <= 1;//normal king move
                case Piece.WKING:
                    if ((Castle.WSHORT & CastlePermissions) != 0)//short castle is permitted
                    {
                        if (x1 == 4 && y1 == 7 && x2 == 6 && y2 == 7
                            && Board[7][7].Occupant == Piece.WROOK
                            && Board[6][7].Occupant == Piece.NONE
                            && Board[5][7].Occupant == Piece.NONE)
                            return true;
                    }
                    if ((Castle.WLONG & CastlePermissions) != 0)//long castle is permitted
                    {
                        if (x1 == 4 && y1 == 7 && x2 == 2 && y2 == 7
                            && Board[0][7].Occupant == Piece.WROOK
                            && Board[3][7].Occupant == Piece.NONE
                            && Board[2][7].Occupant == Piece.NONE
                            && Board[1][7].Occupant == Piece.NONE)
                            return true;
                    }
                    return Math.Abs(y2 - y1) <= 1 && Math.Abs(x2 - x1) <= 1;//normal king move
            }
            return true;
        }
        internal bool TryLegalMove(Move move,bool whiteTurn)//Mehtod has no side-effects! Assuming that the move is legal, it tries the move to see if the king will stay safe after the move.
        {
            int x1 = move.X1, y1 = move.Y1, x2 = move.X2, y2 = move.Y2;
            move.MovedPiece = Board[x1][y1].Occupant;
            switch (Board[x1][y1].Occupant)
            {
                case Piece.BPAWN:
                    if (y1 == 4 && y2 == 5 && Math.Abs(x1 - x2) == 1 && Board[x2][y2].Occupant == Piece.NONE)
                        if (EnPassant != null)
                            if (EnPassant.Equals(Board[x2][4]) && Board[x2][4].Occupant == Piece.WPAWN)
                            {
                                Board[x2][4].Occupant = Piece.NONE;
                                move.EnPassant = true;
                                move.CapturedPiece = Piece.WPAWN;
                                break;
                            }
                    break;
                case Piece.WPAWN:
                    if (y1 == 3 && y2 == 2 && Math.Abs(x1 - x2) == 1 && Board[x2][y2].Occupant == Piece.NONE)
                        if (EnPassant != null)
                            if (EnPassant.Equals(Board[x2][3]) && Board[x2][3].Occupant == Piece.BPAWN)
                            {
                                Board[x2][3].Occupant = Piece.NONE;
                                move.EnPassant = true;
                                move.CapturedPiece = Piece.BPAWN;
                                break;
                            }
                    break;
                case Piece.BKING:
                    if ((Castle.BSHORT & CastlePermissions) != 0)
                    {
                        if (x1 == 4 && y1 == 0 && x2 == 6 && y2 == 0
                            && Board[7][0].Occupant == Piece.BROOK
                            && Board[6][0].Occupant == Piece.NONE
                            && Board[5][0].Occupant == Piece.NONE)
                        {
                            move.Castled = Castle.BSHORT;
                            Board[7][0].Occupant = Piece.NONE;
                            Board[4][0].Occupant = Piece.NONE;
                            Board[6][0].Occupant = Piece.BKING;
                            Board[5][0].Occupant = Piece.BROOK;
                        }
                    }
                    if ((Castle.BLONG & CastlePermissions) != 0)
                    {
                        if (x1 == 4 && y1 == 0 && x2 == 2 && y2 == 0
                            && Board[0][0].Occupant == Piece.BROOK
                            && Board[3][0].Occupant == Piece.NONE
                            && Board[2][0].Occupant == Piece.NONE
                            && Board[1][0].Occupant == Piece.NONE)
                        {
                            move.Castled = Castle.BLONG;
                            Board[0][0].Occupant = Piece.NONE;
                            Board[4][0].Occupant = Piece.NONE;
                            Board[2][0].Occupant = Piece.BKING;
                            Board[3][0].Occupant = Piece.BROOK;
                        }
                    }
                    break;
                case Piece.WKING:
                    if ((Castle.WSHORT & CastlePermissions) != 0)
                    {
                        if (x1 == 4 && y1 == 7 && x2 == 6 && y2 == 7
                            && Board[7][7].Occupant == Piece.WROOK
                            && Board[6][7].Occupant == Piece.NONE
                            && Board[5][7].Occupant == Piece.NONE)
                        {
                            move.Castled = Castle.WSHORT;
                            Board[7][7].Occupant = Piece.NONE;
                            Board[4][7].Occupant = Piece.NONE;
                            Board[6][7].Occupant = Piece.WKING;
                            Board[5][7].Occupant = Piece.WROOK;
                        }
                    }
                    if ((Castle.WLONG & CastlePermissions) != 0)
                    {
                        if (x1 == 4 && y1 == 7 && x2 == 2 && y2 == 7
                            && Board[0][7].Occupant == Piece.WROOK
                            && Board[3][7].Occupant == Piece.NONE
                            && Board[2][7].Occupant == Piece.NONE
                            && Board[1][7].Occupant == Piece.NONE)
                        {
                            move.Castled = Castle.WLONG;
                            Board[0][7].Occupant = Piece.NONE;
                            Board[4][7].Occupant = Piece.NONE;
                            Board[2][7].Occupant = Piece.WKING;
                            Board[3][7].Occupant = Piece.WROOK;
                        }
                    }
                    break;
            }
            move.CapturedPiece = Board[x2][y2].Occupant;
            bool illegal = false;
            if (move.Castled == Castle.NONE)
            {
                Board[x2][y2].Occupant = Board[x1][y1].Occupant;
                Board[x1][y1].Occupant = Piece.NONE;
            }
            else if (move.Castled == Castle.BSHORT)
            {
                Board[5][0].Occupant = Piece.BKING;
                Board[6][0].Occupant = Piece.BROOK;
                illegal = IsCheck(whiteTurn);
                Board[5][0].Occupant = Piece.BROOK;
                Board[6][0].Occupant = Piece.BKING;
            }
            else if (move.Castled == Castle.BLONG)
            {
                Board[3][0].Occupant = Piece.BKING;
                Board[2][0].Occupant = Piece.BROOK;
                illegal = IsCheck(whiteTurn);
                Board[3][0].Occupant = Piece.BROOK;
                Board[2][0].Occupant = Piece.BKING;
            }
            else if (move.Castled == Castle.WSHORT)
            {
                Board[5][7].Occupant = Piece.WKING;
                Board[6][7].Occupant = Piece.WROOK;
                illegal = IsCheck(whiteTurn);
                Board[5][7].Occupant = Piece.WROOK;
                Board[6][7].Occupant = Piece.WKING;
            }
            else
            {
                Board[3][7].Occupant = Piece.WKING;
                Board[2][7].Occupant = Piece.WROOK;
                illegal = IsCheck(whiteTurn);
                Board[3][7].Occupant = Piece.WROOK;
                Board[2][7].Occupant = Piece.WKING;
            }
            if (illegal || IsCheck(whiteTurn))
            {
                UndoMove(move);
                return false;
            }
            UndoMove(move);
            return true;
        }
        internal bool Move(Move move)//If the move is not legal, returns false, otherwise, it makes the move and returns true...
        {
            if (!LegalMove(move,false))
                return false;
            int x1 = move.X1, y1 = move.Y1, x2 = move.X2, y2 = move.Y2;
            move.MovedPiece = Board[x1][y1].Occupant;
            bool readyForEnPassant = false;
            Castle newCastlePermissions = CastlePermissions;
            switch (Board[x1][y1].Occupant)
            {
                case Piece.BPAWN:
                    if (y1 == 4 && y2 == 5 && Math.Abs(x1 - x2) == 1 && Board[x2][y2].Occupant == Piece.NONE)
                        if (EnPassant != null)
                            if (EnPassant.Equals(Board[x2][4]) && Board[x2][4].Occupant == Piece.WPAWN)
                            {
                                Board[x2][4].Occupant = Piece.NONE;
                                move.EnPassant = true;
                                move.CapturedPiece = Piece.WPAWN;
                                break;
                            }
                    if (Board[x2][y2].Occupant == Piece.NONE)
                        if (y1 == 1)
                            readyForEnPassant = y2 == 3;
                    if (y1 == 6)//promotion
                    {
                        Promotion promoted = (Promotion)Promote(move);
                        Board[x1][y1].Occupant = (Piece)promoted;
                        move.Promoted = promoted;
                    }
                    break;
                case Piece.WPAWN:
                    if (y1 == 3 && y2 == 2 && Math.Abs(x1 - x2) == 1 && Board[x2][y2].Occupant == Piece.NONE)
                        if (EnPassant != null)
                            if (EnPassant.Equals(Board[x2][3]) && Board[x2][3].Occupant == Piece.BPAWN)
                            {
                                Board[x2][3].Occupant = Piece.NONE;
                                move.EnPassant = true;
                                move.CapturedPiece = Piece.BPAWN;
                                break;
                            }
                    if (Board[x2][y2].Occupant == Piece.NONE)
                        if (y1 == 6)
                            readyForEnPassant = y2 == 4;
                    if (y1 == 1)//promotion
                    {
                        Promotion promoted = (Promotion)Promote(move);
                        Board[x1][y1].Occupant = (Piece)promoted;
                        move.Promoted = promoted;
                    }
                    break;
                case Piece.BROOK:
                case Piece.WROOK:
                    if ((int)Board[x1][y1].Occupant % 2 == 0)
                    {
                        if (x1 == 0 && y1 == 0)
                            newCastlePermissions = CastlePermissions & ~Castle.BLONG;
                        else if (x1 == 7 && y1 == 0)
                            newCastlePermissions = CastlePermissions & ~Castle.BSHORT;
                    }
                    else
                    {
                        if (x1 == 0 && y1 == 7)
                            newCastlePermissions = CastlePermissions & ~Castle.WLONG;
                        else if (x1 == 7 && y1 == 7)
                            newCastlePermissions = CastlePermissions & ~Castle.WSHORT;
                    }
                    break;
                case Piece.BKING:
                    if ((Castle.BSHORT & CastlePermissions) != 0)
                    {
                        if (x1 == 4 && y1 == 0 && x2 == 6 && y2 == 0
                            && Board[7][0].Occupant == Piece.BROOK
                            && Board[6][0].Occupant == Piece.NONE
                            && Board[5][0].Occupant == Piece.NONE)
                        {
                            move.Castled = Castle.BSHORT;
                            Board[7][0].Occupant = Piece.NONE;
                            Board[4][0].Occupant = Piece.NONE;
                            Board[6][0].Occupant = Piece.BKING;
                            Board[5][0].Occupant = Piece.BROOK;
                        }
                    }
                    if ((Castle.BLONG & CastlePermissions) != 0)
                    {
                        if (x1 == 4 && y1 == 0 && x2 == 2 && y2 == 0
                            && Board[0][0].Occupant == Piece.BROOK
                            && Board[3][0].Occupant == Piece.NONE
                            && Board[2][0].Occupant == Piece.NONE
                            && Board[1][0].Occupant == Piece.NONE)
                        {
                            move.Castled = Castle.BLONG;
                            Board[0][0].Occupant = Piece.NONE;
                            Board[4][0].Occupant = Piece.NONE;
                            Board[2][0].Occupant = Piece.BKING;
                            Board[3][0].Occupant = Piece.BROOK;
                        }
                    }
                    newCastlePermissions = CastlePermissions & ~Castle.BSHORT;
                    newCastlePermissions &= ~Castle.BLONG;
                    break;
                case Piece.WKING:
                    if ((Castle.WSHORT & CastlePermissions) != 0)
                    {
                        if (x1 == 4 && y1 == 7 && x2 == 6 && y2 == 7
                            && Board[7][7].Occupant == Piece.WROOK
                            && Board[6][7].Occupant == Piece.NONE
                            && Board[5][7].Occupant == Piece.NONE)
                        {
                            move.Castled = Castle.WSHORT;
                            Board[7][7].Occupant = Piece.NONE;
                            Board[4][7].Occupant = Piece.NONE;
                            Board[6][7].Occupant = Piece.WKING;
                            Board[5][7].Occupant = Piece.WROOK;
                        }
                    }
                    if ((Castle.WLONG & CastlePermissions) != 0)
                    {
                        if (x1 == 4 && y1 == 7 && x2 == 2 && y2 == 7
                            && Board[0][7].Occupant == Piece.WROOK
                            && Board[3][7].Occupant == Piece.NONE
                            && Board[2][7].Occupant == Piece.NONE
                            && Board[1][7].Occupant == Piece.NONE)
                        {
                            move.Castled = Castle.WLONG;
                            Board[0][7].Occupant = Piece.NONE;
                            Board[4][7].Occupant = Piece.NONE;
                            Board[2][7].Occupant = Piece.WKING;
                            Board[3][7].Occupant = Piece.WROOK;
                        }
                    }
                    newCastlePermissions = CastlePermissions & ~Castle.WSHORT;
                    newCastlePermissions &= ~Castle.WLONG;
                    break;
            }
            if (!move.EnPassant && move.Castled == Castle.NONE)
                move.CapturedPiece = Board[x2][y2].Occupant;
            bool illegal = false;
            switch (move.Castled)//checks to see if the adjacent square to the king is threatened by the opponent when castling. If so, castling is illegal (according to FIDE)...
            {
                case Castle.BSHORT:
                    Board[5][0].Occupant = Piece.BKING;
                    Board[6][0].Occupant = Piece.BROOK;
                    illegal = IsCheck(WhiteTurn);
                    Board[5][0].Occupant = Piece.BROOK;
                    Board[6][0].Occupant = Piece.BKING;
                    break;
                case Castle.BLONG:
                    Board[3][0].Occupant = Piece.BKING;
                    Board[2][0].Occupant = Piece.BROOK;
                    illegal = IsCheck(WhiteTurn);
                    Board[3][0].Occupant = Piece.BROOK;
                    Board[2][0].Occupant = Piece.BKING;
                    break;
                case Castle.WSHORT:
                    Board[5][7].Occupant = Piece.WKING;
                    Board[6][7].Occupant = Piece.WROOK;
                    illegal = IsCheck(WhiteTurn);
                    Board[5][7].Occupant = Piece.WROOK;
                    Board[6][7].Occupant = Piece.WKING;
                    break;
                case Castle.WLONG:
                    Board[3][7].Occupant = Piece.WKING;
                    Board[2][7].Occupant = Piece.WROOK;
                    illegal = IsCheck(WhiteTurn);
                    Board[3][7].Occupant = Piece.WROOK;
                    Board[2][7].Occupant = Piece.WKING;
                    break;
                default://moves the piece from (x1,y1) to (x2,y2)
                    Board[x2][y2].Occupant = Board[x1][y1].Occupant;
                    Board[x1][y1].Occupant = Piece.NONE;
                    break;
            }
            if (illegal || IsCheck(WhiteTurn))
            {
                UndoMove(move);
                return false;
            }
            EnPassant = readyForEnPassant ? Board[x2][y2] : null;
            CastlePermissions = newCastlePermissions;
            if(WhiteTurn)
                WhiteTimeLimit = TimeToString(WLimit += Increment);
            else
                BlackTimeLimit = TimeToString(BLimit += Increment);
            move.Checkmate = IsCheckmate(!WhiteTurn, move);
            move.Stalemate = IsStalemate(!WhiteTurn, move);
            move.Check = IsCheck(!WhiteTurn) && !move.Checkmate;
            Moves.Add(move);
            WhiteTurn = !WhiteTurn;
            if (move.CapturedPiece != Piece.NONE)
                CaptureFX();
            else
                MoveFX();
            return true;
        }

        private void UndoMove(Move move)//undoes the move by restoring the board to its original state...
        {
            if (move.EnPassant)
            {
                Board[move.X1][move.Y1].Occupant = Board[move.X2][move.Y2].Occupant;
                Board[move.X2][move.Y2].Occupant = Piece.NONE;
                if(WhiteTurn)
                    Board[move.X2][move.Y2 + 1].Occupant = move.CapturedPiece;
                else
                    Board[move.X2][move.Y2 - 1].Occupant = move.CapturedPiece;
                return;
            }
            switch (move.Castled)
            {
                case Castle.NONE:
                    switch (move.Promoted)
                    {
                        case Promotion.NONE:
                            Board[move.X1][move.Y1].Occupant = Board[move.X2][move.Y2].Occupant;
                            Board[move.X2][move.Y2].Occupant = move.CapturedPiece;
                            break;
                        case Promotion.BBISHOP:
                        case Promotion.BKNIGHT:
                        case Promotion.BROOK:
                        case Promotion.BQUEEN:
                            Board[move.X1][move.Y1].Occupant = Piece.BPAWN;
                            Board[move.X2][move.Y2].Occupant = move.CapturedPiece;
                            break;
                        case Promotion.WBISHOP:
                        case Promotion.WKNIGHT:
                        case Promotion.WROOK:
                        case Promotion.WQUEEN:
                            Board[move.X1][move.Y1].Occupant = Piece.WPAWN;
                            Board[move.X2][move.Y2].Occupant = move.CapturedPiece;
                            break;
                    }
                    break;
                case Castle.BSHORT:
                    Board[7][0].Occupant = Piece.BROOK;
                    Board[4][0].Occupant = Piece.BKING;
                    Board[5][0].Occupant = Piece.NONE;
                    Board[6][0].Occupant = Piece.NONE;
                    break;
                case Castle.BLONG:
                    Board[0][0].Occupant = Piece.BROOK;
                    Board[4][0].Occupant = Piece.BKING;
                    Board[2][0].Occupant = Piece.NONE;
                    Board[3][0].Occupant = Piece.NONE;
                    break;
                case Castle.WSHORT:
                    Board[7][7].Occupant = Piece.WROOK;
                    Board[4][7].Occupant = Piece.WKING;
                    Board[5][7].Occupant = Piece.NONE;
                    Board[6][7].Occupant = Piece.NONE;
                    break;
                case Castle.WLONG:
                    Board[0][7].Occupant = Piece.WROOK;
                    Board[4][7].Occupant = Piece.WKING;
                    Board[2][7].Occupant = Piece.NONE;
                    Board[3][7].Occupant = Piece.NONE;
                    break;
            }
        }
    }
}
