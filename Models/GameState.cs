using MazeGeneration;
using Rex3.Models;
using System.Drawing;

namespace Rex3
{
    public class GameState
    {
        public List<Voting> VotingHistory { get; set; }
        public Voting? Current { get; set; } = null;
        public List<Maze> Mazes { get; set; }
        public List<Level> Levels { get; set; }
        public int CurrentLevel { get; set; }

        public Point CurrentLocation { get; set; }

        public int HP { get; set; }
        public int Energy { get; set; }
        public int Turn => VotingHistory.Count;

        public GameState()
        {
            VotingHistory = new List<Voting>();
            Mazes = new List<Maze>();
            Levels = new List<Level>();

            Levels.Add(new Level { EnergyRecoveryRate = 3 });
        }

    }

    public class Voting
    {
        public Action Action { get; set; }
        public bool? Clairvoyant { get; set; }
        public bool? Navigator { get; set; }
        
        public bool? Scribe { get; set; }

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
        South,
        Rest
    }
}
