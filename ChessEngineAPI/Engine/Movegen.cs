namespace ChessEngineAPI.Engine
{
    public class Movegen
    {
        public static int Move(int from, int to, int captured, int promoted, int flag)
        {
            return from | (to << 7) | (captured << 14) | (promoted << 20) | flag;
        }

        public void GenerateMoves()
        {
            
        }
    }
}