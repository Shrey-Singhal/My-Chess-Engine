namespace ChessEngine
{
    public class Gameboard
    {
        // represents chess board squares. each index shows what piece is on that square.
        public int[] pieces = new int[Defs.BRD_SQ_NUM];        
        public Defs.Colours side; //which side to move next
        public int fiftyMove; // Number of half-moves since the last pawn move or capture.
        public int hisPly; // half moves played in the entire game

        // ply is just engine calculating a certain number of moves ahead at a particular 
        // position to find the best move
        public int ply; 
        public Defs.CASTLEBIT castlePerm; // track which side can castle legally

        public Gameboard()
        {
            side = Defs.Colours.WHITE;
            fiftyMove = 0;
            hisPly = 0;
            ply = 0;
            castlePerm = 0;

        }
    }

    
}