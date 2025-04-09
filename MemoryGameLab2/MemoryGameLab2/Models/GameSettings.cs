namespace MemoryGameLab2.Models
{
    public class GameSettings
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int TimeLimit { get; set; }
        public string Category { get; set; }

        public GameSettings(int rows, int columns, int timeLimit, string category)
        {
            Rows = rows;
            Columns = columns;
            TimeLimit = timeLimit;
            Category = category;
        }
    }
} 