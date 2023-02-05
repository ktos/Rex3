using MazeGeneration;
using Rex3.Models;
using System.Drawing;

namespace Rex3
{
    public class GameState
    {
        public List<Voting> VotingHistory { get; set; }
        public Voting? Current { get; set; } = null;
        public int BadVotesCount { get; set; } = 0;
        public List<Maze> Mazes { get; set; }
        public List<Level> Levels { get; set; }
        public int CurrentLevelIndex { get; set; }

        public int SelectedRolesCount { get; set; }

        public Level CurrentLevel => Levels[CurrentLevelIndex];
        public Maze CurrentMaze => Mazes[CurrentLevelIndex];

        public Point CurrentLocation { get; set; }

        public int HP { get; set; }
        public int Energy { get; set; }
        public int Turn => VotingHistory.Count;

        public GameState()
        {
            VotingHistory = new List<Voting>();
            Mazes = new List<Maze>();
            Levels = new List<Level>();
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
            return (Clairvoyant.HasValue && Navigator.HasValue && Scribe.HasValue);
        }

        public bool? CalculateResult()
        {
            //return true; // debug only

            // depending on the action all of them must agree or only two
            // pre-final: everyone must agree
            //if (Action == Action.ChangeEnemiesHp || Action == Action.Sacrifice || Action == Action.Teleport)
            //{
            if (!Clairvoyant.HasValue || !Navigator.HasValue || !Scribe.HasValue)
                return null;

            if (Clairvoyant.Value && Navigator.Value && Scribe.Value)
                return true;

            return false;
            /*}
            else
            {
                if (!Clairvoyant.HasValue || !Navigator.HasValue || !Scribe.HasValue)
                    return null;

                if (
                    Clairvoyant.Value && Navigator.Value
                    || Clairvoyant.Value
                    || Navigator.Value && Scribe.Value
                    || Scribe.Value && Navigator.Value
                )
                    return true;

                return false;
            }*/
        }
    }

    public enum Action
    {
        North,
        East,
        West,
        South,
        Rest,
        ChangeEnemiesHp,
        Teleport,
        Sacrifice
    }
}
