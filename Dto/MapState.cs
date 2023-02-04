using Rex3.Models;

namespace Rex3.Dto
{
    public class MapState
    {
        public string[,]? Cells { get; set; }
        public bool[,]? Visited { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int HP { get; set; }
        public int Energy { get; set; }

        public List<bool?>? VotingHistory { get; set; }
        public int Turn { get; set; }
        public Level? Level { get; set; }
    }
}
