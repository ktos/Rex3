using MazeGeneration;
using System.Drawing;

namespace Rex3
{
    public class GameState
    {
        public List<Voting> VotingHistory { get; set; } = new List<Voting>();
        public Voting? Current { get; set; } = null;
        public List<Maze> Mazes { get; set; } = new List<Maze>();

        public Point CurrentLocation { get; set; }
    }

    public class Voting
    {
        public Action Action { get; set; }
        public bool? Clairvoyant { get; set; }
        public bool? Navigator { get; set; }
        //public bool? Scribe { get; set; }

        public bool IsFinished()
        {
            return (Clairvoyant.HasValue && Navigator.HasValue);// && Scribe.HasValue);
        }

        public bool? CalculateResult()
        {
            // depending on the action all of them must agree or only two

            if (!Clairvoyant.HasValue || !Navigator.HasValue)// || !Scribe.HasValue)
                return null;

            if (Clairvoyant.Value && Navigator.Value || Clairvoyant.Value || Navigator.Value)// && Scribe.Value || Scribe.Value && Navigator.Value)
                return true;

            return false;
        }
    }

    public enum Action
    {
        North,
        East,
        West,
        Down,
        Special1
    }
}
