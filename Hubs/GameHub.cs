using MazeGeneration;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Rex3.Dto;
using System.Drawing;

namespace Rex3.Hubs
{
    public class GameHub : Hub
    {
        private readonly GameState _state;
        private readonly Random rnd;

        public GameHub(GameState state)
        {
            _state = state;
            rnd = new Random();
        }

        private void NextLevel()
        {
            if (_state.Mazes.Count == 0)
            {
                _state.Mazes.Add(new Maze(5, 5));
                _state.CurrentLocation = new Point(0, 0);
                _state.Mazes[_state.CurrentLevelIndex].Visited[0, 0] = true;

                //_state.CurrentLevel.StairsLocation

                var x = rnd.Next(_state.CurrentMaze.Width - 1);
                var y = _state.CurrentMaze.Height - 1; //rnd.Next(_state.CurrentMaze.Height - 1);

                _state.CurrentLevel.StairsLocation = new Point(x, y);

                _state.HP = 5;
                _state.Energy = 5;
            }
            else
            {
                // generate next level depending on the previous result
            }
        }

        public async Task StartVotingForAction(string user, string action)
        {
            var ac = (Action)Convert.ToInt32(action);

            _state.Current = new Voting() { Action = ac };
            await Clients.All.SendAsync("VotingStarted", user, _state.Current.Action);

            //Task.St

            //Task.Run(async () => {
            //    await Task.Delay(TimeSpan.FromSeconds(10));
            //    if (!_state.Current.IsFinished())
            //    {
            //        await Clients.All.SendAsync("VotingInconclusive");
            //    }
            //});
        }

        public async Task VotingTimeout(string user)
        {
            if (_state.Current != null && !_state.Current.IsFinished())
            {
                _state.InconclusiveCount++;
                ArchiveVoting();
                await Clients.All.SendAsync("VotingInconclusive");

                if (_state.InconclusiveCount == 2)
                {
                    _state.HP--;
                    _state.InconclusiveCount = 0;
                    await SendUpdatedState();
                }
            }
        }

        public async Task Vote(string user, string action)
        {
            bool decision = action == "1";

            if (_state.Current != null)
            {
                switch (user)
                {
                    case "clairvoyant":
                        _state.Current.Clairvoyant = decision;
                        break;
                    case "navigator":
                        _state.Current.Navigator = decision;
                        break;
                    case "scribe":
                        _state.Current.Scribe = decision;
                        break;
                }

                await Clients.All.SendAsync("VoteReceived", user);

                // tymczasowo, głosowanie zawsze jest udane, po jednym
                //if (_state.Current.IsFinished())
                if (true)
                {
                    await Clients.All.SendAsync("VotingFinished", true);

                    if (_state.Energy > 0)
                    {
                        switch (_state.Current.Action)
                        {
                            case Action.North:
                                _state.CurrentLocation = new Point(
                                    _state.CurrentLocation.X - 1,
                                    _state.CurrentLocation.Y
                                );
                                _state.Energy--;
                                break;
                            case Action.East:
                                _state.CurrentLocation = new Point(
                                    _state.CurrentLocation.X,
                                    _state.CurrentLocation.Y + 1
                                );
                                _state.Energy--;
                                break;
                            case Action.West:
                                _state.CurrentLocation = new Point(
                                    _state.CurrentLocation.X,
                                    _state.CurrentLocation.Y - 1
                                );
                                _state.Energy--;
                                break;
                            case Action.South:
                                _state.CurrentLocation = new Point(
                                    _state.CurrentLocation.X + 1,
                                    _state.CurrentLocation.Y
                                );
                                _state.Energy--;
                                break;
                        }
                    }

                    // marks current cell as visited
                    _state.Mazes[_state.CurrentLevelIndex].Visited[
                        _state.CurrentLocation.Y,
                        _state.CurrentLocation.X
                    ] = true;

                    // archiving of the votes
                    ArchiveVoting();

                    // update energy, move enemies
                    UpdateEnergy();
                    MoveEnemies();

                    Console.WriteLine(
                        "loc: {0} {1}",
                        _state.CurrentLocation,
                        _state.CurrentLevel.StairsLocation
                    );
                    if (_state.CurrentLocation == _state.CurrentLevel.StairsLocation)
                    {
                        await SendWin();
                    }
                    else if (_state.HP == 0)
                    {
                        await SendLose();
                    }
                    else
                    {
                        await SendUpdatedState();
                    }
                }
            }
        }

        private async Task SendWin()
        {
            await Clients.All.SendAsync("Win");
        }

        private async Task SendLose()
        {
            await Clients.All.SendAsync("Lose");
        }

        private void ArchiveVoting()
        {
            _state.InconclusiveCount = 0;
            if (_state.Current != null)
                _state.VotingHistory.Add(_state.Current);
            _state.Current = null;
        }

        private void UpdateEnergy()
        {
            if (_state.Turn % _state.Levels[_state.CurrentLevelIndex].EnergyRecoveryRate == 0)
            {
                _state.Energy++;
            }
        }

        private void MoveEnemies() { }

        public async Task GameStarted()
        {
            NextLevel();

            await SendUpdatedState();
        }

        public async Task SendUpdatedState()
        {
            var ms = new MapState
            {
                Cells = _state.Mazes[_state.CurrentLevelIndex].CellStateToStringArray(),
                X = _state.CurrentLocation.X,
                Y = _state.CurrentLocation.Y,
                HP = _state.HP,
                Energy = _state.Energy,
                Turn = _state.Turn,
                VotingHistory = _state.VotingHistory.Select(x => x.CalculateResult()).ToList(),
                Level = _state.Levels[_state.CurrentLevelIndex],
                Visited = _state.Mazes[_state.CurrentLevelIndex].Visited
            };

            // serializing stairs location
            ms.Cells[
                _state.Levels[_state.CurrentLevelIndex].StairsLocation.X,
                _state.Levels[_state.CurrentLevelIndex].StairsLocation.Y
            ] += "s";

            var serializedMs = JsonConvert.SerializeObject(ms);

            await Clients.All.SendAsync("MapUpdate", serializedMs);
        }

        public async Task Debug(string user, string message)
        {
            if (message == "gamestarted")
            {
                await this.GameStarted();
            }

            if (message == "hit")
            {
                _state.HP--;
                await this.SendUpdatedState();
            }
            //await Clients.All.SendAsync("Receive", user, message);
        }
    }
}
