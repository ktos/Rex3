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

        public GameHub(GameState state)
        {
            _state = state;
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
                _state.Inconclusive++;
                ArchiveVoting();
                await Clients.All.SendAsync("VotingInconclusive");

                if (_state.Inconclusive == 2)
                {
                    _state.HP--;
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
                        _state.Current.Scribe = decision; break;
                }

                await Clients.All.SendAsync("VoteReceived", user);

                // tymczasowo, głosowanie zawsze jest udane, po jednym
                //if (_state.Current.IsFinished())
                if (true)
                {

                    await Clients.All.SendAsync("VotingFinished", true);

                    switch (_state.Current.Action)
                    {
                        case Action.North:
                            _state.CurrentLocation = new Point(
                                _state.CurrentLocation.X - 1,
                                _state.CurrentLocation.Y
                            );
                            break;
                        case Action.East:
                            _state.CurrentLocation = new Point(
                                _state.CurrentLocation.X,
                                _state.CurrentLocation.Y + 1
                            );
                            break;
                        case Action.West:
                            _state.CurrentLocation = new Point(
                                _state.CurrentLocation.X,
                                _state.CurrentLocation.Y - 1
                            );
                            break;
                        case Action.South:
                            _state.CurrentLocation = new Point(
                                _state.CurrentLocation.X + 1,
                                _state.CurrentLocation.Y
                            );
                            break;
                    }

                    // archiving of the votes
                    ArchiveVoting();

                    // update energy, move enemies
                    await UpdateEnergy();
                    await MoveEnemies();

                    await SendUpdatedState();
                }


            }
        }

        private void ArchiveVoting()
        {
            _state.VotingHistory.Add(_state.Current);
            _state.Current = null;
        }

        private async Task UpdateEnergy()
        {
            if (_state.Turn % _state.Levels[_state.CurrentLevel].EnergyRecoveryRate == 0)
            {
                _state.Energy++;
            }
        }

        private async Task MoveEnemies()
        {
            
        }

        public async Task GameStarted()
        {
            if (_state.Mazes.Count == 0)
            {
                _state.Mazes.Add(new Maze(5, 5));
                _state.CurrentLocation = new Point(0, 0);
            }

            await SendUpdatedState();
        }

        public async Task SendUpdatedState()
        {
            var ms = new MapState
            {
                Cells = _state.Mazes.First().CellStateToStringArray(),
                X = _state.CurrentLocation.X,
                Y = _state.CurrentLocation.Y,
                HP = _state.HP,
                Energy = _state.Energy,
                Turn = _state.Turn,
                VotingHistory = _state.VotingHistory.Select(x => x.CalculateResult()).ToList(),
                Level = _state.Levels[_state.CurrentLevel]
            };
            var serializedMs = JsonConvert.SerializeObject(ms);

            await Clients.All.SendAsync("MapUpdate", serializedMs);
        }

        public async Task Debug(string user, string message)
        {
            if (message == "gamestarted")
            {
                await this.GameStarted();
            }
            //await Clients.All.SendAsync("Receive", user, message);
        }
    }
}
