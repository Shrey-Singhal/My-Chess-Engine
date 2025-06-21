namespace ChessEngine
{
    public static class Board
    {
        // these 2 lists contain file index and rank index for each square
        public static int[] FilesBrd = new int[Defs.BRD_SQ_NUM];
        public static int[] RanksBrd = new int[Defs.BRD_SQ_NUM];

        public static void InitFilesRanksBoard()
        {

            for (int i = 0; i < Defs.BRD_SQ_NUM; ++i)
            {
                FilesBrd[i] = Defs.Squares.OFFBOARD;
                RanksBrd[i] = Defs.Squares.OFFBOARD;
            }

            for (int rank = 0; rank < 8; ++rank)
            {
                for (int file = 0; file < 8; ++file)
                {
                    int sq = Defs.getSquareIndex(file, rank);
                    FilesBrd[sq] = file;
                    RanksBrd[sq] = rank;

                    //Console.WriteLine($"Index {sq} : file = {Board.FilesBrd[sq]} and rank = {Board.RanksBrd[sq]}");
                }

            }
            //Console.WriteLine($"E8: File = {FilesBrd[Defs.Squares.E8]} and rank = {RanksBrd[Defs.Squares.E8]}");

        }
    }
}