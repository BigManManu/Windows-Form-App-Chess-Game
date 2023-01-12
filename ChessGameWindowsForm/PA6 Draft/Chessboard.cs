using System;
using System.IO;
using System.Media;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PA6_Draft
{
    public partial class Chessboard : Form
    {
        private Brush LightColor;
        private Brush DarkColor;
        private Brush Highlighted;
        private ChessGame Game;
        private Square Picked;
        private Square Dropped;
        private Point PickedLocation;
        private SoundPlayer player;
        private BindingSource listOfMoves;
        private Dictionary<Piece,Bitmap> PieceImages;//BlackPawn,WhitePawn,BlackRook,WhiteRook,BlackKnight,WhiteKnight,BlackBishop,WhiteBishop
                                                     //,BlackKing, WhiteKing, BlackQueen, WhiteQueen;
        
     
        internal Chessboard(Color Light, Color Dark, ChessGame Game)
        {
            InitializeComponent();

            player = new SoundPlayer();
            listOfMoves = new BindingSource();
            PieceImages = new Dictionary<Piece, Bitmap>();
            PieceImages.Add(Piece.BPAWN, new Bitmap(new Bitmap(@"bp.png"), new Size(64, 64)));
            PieceImages.Add(Piece.WPAWN, new Bitmap(new Bitmap(@"wp.png"), new Size(64, 64)));
            PieceImages.Add(Piece.BROOK, new Bitmap(new Bitmap(@"br.png"), new Size(64, 64)));
            PieceImages.Add(Piece.WROOK, new Bitmap(new Bitmap(@"wr.png"), new Size(64, 64)));
            PieceImages.Add(Piece.BKNIGHT, new Bitmap(new Bitmap(@"bkn.png"), new Size(64, 64)));
            PieceImages.Add(Piece.WKNIGHT, new Bitmap(new Bitmap(@"wkn.png"), new Size(64, 64)));
            PieceImages.Add(Piece.BBISHOP, new Bitmap(new Bitmap(@"bb.png"), new Size(64, 64)));
            PieceImages.Add(Piece.WBISHOP, new Bitmap(new Bitmap(@"wb.png"), new Size(64, 64)));
            PieceImages.Add(Piece.BKING, new Bitmap(new Bitmap(@"bk.png"), new Size(64, 64)));
            PieceImages.Add(Piece.WKING, new Bitmap(new Bitmap(@"wk.png"), new Size(64, 64)));
            PieceImages.Add(Piece.BQUEEN, new Bitmap(new Bitmap(@"bq.png"), new Size(64, 64)));
            PieceImages.Add(Piece.WQUEEN, new Bitmap(new Bitmap(@"wq.png"), new Size(64, 64)));
            LightColor = new SolidBrush(Light);
            DarkColor = new SolidBrush(Dark);
            Highlighted = new SolidBrush(Color.FromArgb(100, Color.FromName("yellow")));
            this.Game = Game;
            Player1.Text = Game.Player1Name;
            Player2.Text = Game.Player2Name;
            Game.Promote += Game_Promote;
            Game.CheckMate += Game_CheckMate;
            Game.StaleMate += Game_StaleMate;
            Game.MoveFX += Game_MoveFX;
            Game.CaptureFX += Game_CaptureFX;
            Game.CheckFX += Game_CheckFX;
            Game.CheckmateFX += Game_CheckmateFX;
            Game.StalemateFX += Game_StalemateFX;
            listOfMoves.DataSource = Game.BindingMoves;
            listBox1.DataSource = listOfMoves;
            Picked = new Square(0,'z');
            Dropped = new Square(0, 'z');
            Board.Image = new Bitmap(512,512);
            Board_Paint(null,null);
        }
        private object Game_Promote(Move move)
        {
            // make changes...
            PromotionForm p = new PromotionForm();
            if (p.ShowDialog() == DialogResult.OK)
                return ((int)move.MovedPiece % 2 == 0) ? p.BlackPromote() : p.WhitePromote();
            return ((int)move.MovedPiece % 2 == 0) ? Promotion.BQUEEN : Promotion.WQUEEN;
        }

        private object Game_CheckMate(Move move)
        {
            if((int)move.MovedPiece % 2 == 0)
            {
                MainTimer.Stop();
                MessageBox.Show(Game.Player1Name + " lost by checkmate!");
            }
            else
            {
                MainTimer.Stop();
                MessageBox.Show(Game.Player2Name + " lost by checkmate!");
            }
            return null;
        }

        private object Game_StaleMate(Move move)
        {
            MainTimer.Stop();
            MessageBox.Show("Stalemate!");
            return null;
        }

        private void Game_MoveFX()
        {
            string path = Path.GetFullPath("Move.wav");
            path = path.Replace(@"bin\Debug", @"Sounds");
            player.SoundLocation = path;
            player.Load();
            player.Play();
        }

        private void Game_CaptureFX()
        {
            string path = Path.GetFullPath("Capture.wav");
            path = path.Replace(@"bin\Debug", @"Sounds");
            player.SoundLocation = path;
            player.Load();
            player.Play();
        }

        private void Game_CheckFX()
        {
            string path = Path.GetFullPath("Check.wav");
            path = path.Replace(@"bin\Debug", @"Sounds");
            player.SoundLocation = path;
            player.Load();
            player.Play();
        }

        private void Game_CheckmateFX()
        {
            string path = Path.GetFullPath("Checkmate.wav");
            path = path.Replace(@"bin\Debug", @"Sounds");
            player.SoundLocation = path;
            player.Load();
            player.Play();
            MainTimer.Stop();
        }

        private void Game_StalemateFX()
        {
            string path = Path.GetFullPath("Stalemate.wav");
            path = path.Replace(@"bin\Debug", @"Sounds");
            player.SoundLocation = path;
            player.Load();
            player.Play();
            MainTimer.Stop();
        }

        private void TimeOutFX()
        {
            string path = Path.GetFullPath("TimeOut.wav");
            path = path.Replace(@"bin\Debug", @"Sounds");
            player.SoundLocation = path;
            player.Load();
            player.Play();
        }

        private void TenSecLeft()
        {
            string path = Path.GetFullPath("TenSecLeft.wav");
            path = path.Replace(@"bin\Debug", @"Sounds");
            player.SoundLocation = path;
            player.Load();
            player.Play();
        }

        private void Board_MouseDown(object sender, MouseEventArgs e)
        {
            int sizeUnit = (int)Math.Round(Board.Image.Width / 16.0);
            int X = e.X / (2*sizeUnit);
            int Y = e.Y / (2 * sizeUnit);
            if (Game.Board[X][Y].Occupant == Piece.NONE)
                return;
            Picked = new Square(Game.Board[X][Y].Rank,
                                Game.Board[X][Y].File,
                                Game.Board[X][Y].Occupant);
            PickedLocation = new Point(e.Location.X - sizeUnit, e.Location.Y - sizeUnit);
            Board.Refresh();
        }

        private void Board_MouseUp(object sender, MouseEventArgs e)
        {
            int sizeUnit = (int)Math.Round(Board.Image.Width / 16.0);
            if (Picked.Occupant == Piece.NONE)
                return;
            if (e.X >= Board.Width || e.Y >= Board.Height || e.X < 0 || e.Y < 0)
            {
                Picked = new Square(0, 'z');
                Board.Invalidate();
                return;
            }
            int X = e.X / (2 * sizeUnit);
            int Y = e.Y / (2 * sizeUnit);
            Move move = new Move(Picked.File - 'a', 8 - Picked.Rank, X, Y);
            bool Success = Game.Move(move);
            if (Success)
                Game.BindingMoves.Add(move);
            if (Success)
                Dropped = new Square(Game.Board[X][Y].Rank,
                                    Game.Board[X][Y].File,
                                    Game.Board[X][Y].Occupant);
            Picked.Occupant = Piece.NONE ;
            if(Success)
                MainTimer.Enabled = true;
            Board.Invalidate();
        }

        private void Board_MouseMove(object sender, MouseEventArgs e)
        {
            int sizeUnit = (int)Math.Round(Board.Image.Width / 16.0);
            if (Picked.Occupant != Piece.NONE)
            {
                PickedLocation = new Point(e.Location.X - sizeUnit, e.Location.Y - sizeUnit);
                if (e.X >= Board.Width)
                    PickedLocation.X = Board.Width - sizeUnit;
                if (e.X < 0)
                    PickedLocation.X = -sizeUnit;
                if (e.Y >= Board.Height)
                    PickedLocation.Y = Board.Height - sizeUnit;
                if (e.Y < 0)
                    PickedLocation.Y = -sizeUnit;
            }
            Board.Invalidate();

        }
        private void Board_Paint(object sender, PaintEventArgs e)
        {
            int squareWidth = (int)Math.Round(Board.Image.Width / 8.0);
            using (Graphics g = Graphics.FromImage(Board.Image))
            {
                for (int i = 0; i < 8; i++)
                    for (int j = 0; j < 8; j++)
                        if ((i + j) % 2 == 0)
                            g.FillRectangle(LightColor, new Rectangle(squareWidth * i, squareWidth * j, squareWidth, squareWidth));
                        else
                            g.FillRectangle(DarkColor, new Rectangle(squareWidth * i, squareWidth * j, squareWidth, squareWidth));
                for (int i = 0; i < 8; i++)
                {
                    g.DrawString("" + (8 - i), new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold),
                        (i % 2 == 0) ? DarkColor : LightColor, new Point(0, 3 * squareWidth / 64 + squareWidth * i));
                    g.DrawString("" + (char)('a' + i), new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold),
                        (i % 2 == 1) ? DarkColor : LightColor, new Point(54 * squareWidth/64 + squareWidth * i, 498));
                }
                if(Dropped.Occupant != Piece.NONE)
                    g.FillRectangle(Highlighted, new Rectangle(squareWidth * (Dropped.File - 'a'), squareWidth * (8 - Dropped.Rank), squareWidth, squareWidth));
                for (int i = 0; i < 8; i++)
                    for (int j = 0; j < 8; j++)
                    {
                        if (Game.Board[i][j].Occupant == Piece.NONE)//empty square
                            continue;
                        if (Picked.Occupant != Piece.NONE)
                            if (Game.Board[i][j].Rank == Picked.Rank && Game.Board[i][j].File == Picked.File)
                                continue;
                        g.DrawImage(PieceImages[Game.Board[i][j].Occupant], new Point(squareWidth * i, squareWidth * j));
                    }
                if (Picked.Occupant == Piece.NONE)
                    return;
                g.FillRectangle(Highlighted,
                    new Rectangle(squareWidth * (Picked.File - 'a'), squareWidth * (8 - Picked.Rank), squareWidth, squareWidth));
                g.DrawImage(PieceImages[Picked.Occupant], PickedLocation);
            }
        }

        private void ChessBoard_MouseMove(object sender, MouseEventArgs e)
        {
            Board.Invalidate();
        }

        private void Board_MouseLeave(object sender, EventArgs e)
        {
            Board.Invalidate();
        }

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            if (Game.WhiteTurn)
            {
                if (Game.WLimit <= MainTimer.Interval)
                {
                    Game.WhiteTimeLimit = "0.00";
                    Game.WLimit = 0;
                    TimeOutFX();
                    MainTimer.Stop();
                    MessageBox.Show(Game.Player1Name + " lost by timeout");
                }
                else
                {
                    Game.WhiteTimeLimit = Game.TimeToString(Game.WLimit -= MainTimer.Interval);
                    if (String.Equals(Game.WhiteTimeLimit, "0:10"))
                        TenSecLeft();
                    Player1Time.Text = Game.WhiteTimeLimit;
                }
            }
            else
            {
                if (Game.BLimit <= MainTimer.Interval)
                {
                    Game.BlackTimeLimit = "0.00";
                    Game.BLimit = 0;
                    TimeOutFX();
                    MainTimer.Stop();
                    MessageBox.Show(Game.Player2Name + " lost by timeout");
                }
                else
                {
                    Game.BlackTimeLimit = Game.TimeToString(Game.BLimit -= MainTimer.Interval);
                    if (String.Equals(Game.BlackTimeLimit, "0:10"))
                        TenSecLeft();
                    Player2Time.Text = Game.BlackTimeLimit;
                }
            }
        }

        private void Chessboard_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainTimer.Stop();
        }
    }
}
