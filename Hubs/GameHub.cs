﻿using MazeGeneration;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Rex3.Dto;
using Rex3.Models;
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

                _state.Levels.Add(new Level());

                var x = rnd.Next(_state.CurrentMaze.Width - 1);
                var y = _state.CurrentMaze.Height - 1; //rnd.Next(_state.CurrentMaze.Height - 1);

                _state.CurrentLevel.StairsLocation = new Point(x, y);

                _state.HP = 5;
                _state.Energy = 5;

                _state.CurrentMaze.Display();

                _state.CurrentLevel.EnergyRecoveryRate = 3;
                _state.CurrentLevel.EnergyRecoveryAmount = 3;

                _state.CurrentLevel.ClairvoyantGoal = SecretGoal.None;

                GenerateEnemies(3, 3);
                GenerateBoxes(3, 2, 2);
            }
            else
            {
                // generate next level depending on the previous result
            }
        }

        private void GenerateBoxes(int count, int hp, int energy)
        {
            _state.CurrentLevel.BoxesCount = count;
            for (int i = 0; i < _state.CurrentLevel.BoxesCount; i++)
            {
                int a = rnd.Next(1, _state.CurrentMaze.Width - 1);
                int b = rnd.Next(1, _state.CurrentMaze.Height - 1);

                while (
                    _state.CurrentLevel.Enemies.FirstOrDefault(x => x.Position == new Point(a, b))
                        != null
                    || _state.CurrentLevel.Boxes.FirstOrDefault(x => x.Position == new Point(a, b))
                        != null
                )
                {
                    a = rnd.Next(2, _state.CurrentMaze.Width - 1);
                    b = rnd.Next(1, _state.CurrentMaze.Height - 1);
                }

                _state.CurrentLevel.Boxes.Add(
                    new Box()
                    {
                        HP = hp,
                        Energy = energy,
                        Position = new Point(a, b)
                    }
                );
            }
        }

        private void GenerateEnemies(int count, int hp)
        {
            _state.CurrentLevel.EnemiesCount = count;

            for (int i = 0; i < _state.CurrentLevel.EnemiesCount; i++)
            {
                int a = rnd.Next(2, _state.CurrentMaze.Width - 1);
                int b = rnd.Next(1, _state.CurrentMaze.Height - 1);

                while (
                    _state.CurrentLevel.Enemies.FirstOrDefault(x => x.Position == new Point(a, b))
                    != null
                )
                {
                    a = rnd.Next(2, _state.CurrentMaze.Width - 1);
                    b = rnd.Next(1, _state.CurrentMaze.Height - 1);
                }

                _state.CurrentLevel.Enemies.Add(
                    new Enemy() { HP = hp, Position = new Point(a, b) }
                );
            }
        }

        public async Task StartVotingForAction(string user, string action)
        {
            var ac = (Action)Convert.ToInt32(action);

            _state.Current = new Voting() { Action = ac };
            await Clients.All.SendAsync("VotingStarted", user, _state.Current.Action);
        }

        public async Task VotingTimeout(string user)
        {
            if (_state.Current != null && !_state.Current.IsFinished())
            {
                ArchiveVoting();
                _state.BadVotesCount++;

                await Clients.All.SendAsync("VotingInconclusive");

                if (_state.BadVotesCount >= 2)
                {
                    _state.HP--;
                    _state.BadVotesCount = 0;
                }
                await SendUpdatedState();
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

                    //var votingResult = _state.Current.CalculateResult();
                    //if (votingResult.HasValue && votingResult.Value)
                    if (true)
                    {
                        if (_state.Energy > 0)
                        {
                            switch (_state.Current.Action)
                            {
                                case Action.North:
                                    _state.CurrentLocation = new Point(
                                        _state.CurrentLocation.X,
                                        _state.CurrentLocation.Y - 1
                                    );
                                    _state.Energy--;
                                    break;
                                case Action.East:
                                    _state.CurrentLocation = new Point(
                                        _state.CurrentLocation.X + 1,
                                        _state.CurrentLocation.Y
                                    );
                                    _state.Energy--;
                                    break;
                                case Action.West:
                                    _state.CurrentLocation = new Point(
                                        _state.CurrentLocation.X - 1,
                                        _state.CurrentLocation.Y
                                    );
                                    _state.Energy--;
                                    break;
                                case Action.South:
                                    _state.CurrentLocation = new Point(
                                        _state.CurrentLocation.X,
                                        _state.CurrentLocation.Y + 1
                                    );
                                    _state.Energy--;
                                    break;
                                case Action.Rest:
                                    _state.Energy--;
                                    _state.HP += 1;
                                    break;
                            }
                        }

                        // marks current cell as visited
                        _state.Mazes[_state.CurrentLevelIndex].Visited[
                            _state.CurrentLocation.X,
                            _state.CurrentLocation.Y
                        ] = true;

                        // resetting BadVotesCount if voting is successful
                        _state.BadVotesCount = 0;
                    }
                    // else
                    // {
                    //     // voting was not successful
                    //     _state.BadVotesCount++;

                    //     if (_state.BadVotesCount >= 2)
                    //     {
                    //         _state.HP--;
                    //         _state.BadVotesCount = 0;
                    //         await SendUpdatedState();
                    //     }
                    // }

                    // archiving of the votes
                    ArchiveVoting();

                    // update energy, move enemies
                    await CheckEnemies();
                    await CheckBoxes();
                    UpdateEnergy();
                    MoveEnemies();

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

        private async Task CheckEnemies()
        {
            var enemyAtCurrentPosition = _state.CurrentLevel.Enemies.FirstOrDefault(
                x => x.Position == _state.CurrentLocation
            );

            if (enemyAtCurrentPosition != null)
            {
                _state.HP -= enemyAtCurrentPosition.HP;
                _state.CurrentLevel.Enemies.Remove(enemyAtCurrentPosition);
                await Clients.All.SendAsync("Attack");
            }
        }

        private async Task CheckBoxes()
        {
            var boxAtCurrentPosition = _state.CurrentLevel.Boxes.FirstOrDefault(
                x => x.Position == _state.CurrentLocation
            );

            if (boxAtCurrentPosition != null)
            {
                _state.HP += boxAtCurrentPosition.HP;
                _state.Energy += boxAtCurrentPosition.Energy;
                _state.CurrentLevel.Boxes.Remove(boxAtCurrentPosition);

                await Clients.All.SendAsync("Box");
            }
        }

        private async Task SendWin()
        {
            var fs = new FinishState
            {
                Mystery = Mysteries.GenerateMystery(_state.CurrentLevel, _state.CurrentLevelIndex)
            };
            await Clients.All.SendAsync("Win", JsonConvert.SerializeObject(fs));
        }

        private async Task SendLose()
        {
            var fs = new FinishState
            {
                Mystery = Mysteries.GenerateLoseMystery(
                    _state.CurrentLevel,
                    _state.CurrentLevelIndex
                )
            };
            await Clients.All.SendAsync("Lose", JsonConvert.SerializeObject(fs));
        }

        private void ArchiveVoting()
        {
            if (_state.Current != null)
                _state.VotingHistory.Add(_state.Current);
            _state.Current = null;
        }

        private void UpdateEnergy()
        {
            if (_state.Turn % _state.Levels[_state.CurrentLevelIndex].EnergyRecoveryRate == 0)
            {
                _state.Energy += _state.CurrentLevel.EnergyRecoveryAmount;
            }
        }

        private void MoveEnemies() { }

        public async Task GameStarted()
        {
            NextLevel();
            await Clients.All.SendAsync("GameStarted");
            await SendUpdatedState();
        }

        public async Task SetRole(string role)
        {
            _state.SelectedRolesCount++;

            if (_state.SelectedRolesCount == 3)
            {
                await GameStarted();
            }
            else
            {
                await Clients.All.SendAsync("RoleSelected", role);
            }
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
                Visited = _state.Mazes[_state.CurrentLevelIndex].Visited,
                BadVotesCount = _state.BadVotesCount
            };

            // serializing stairs location
            ms.Cells[
                _state.Levels[_state.CurrentLevelIndex].StairsLocation.X,
                _state.Levels[_state.CurrentLevelIndex].StairsLocation.Y
            ] += "s";

            // serializing enemies
            foreach (var item in _state.CurrentLevel.Enemies)
            {
                ms.Cells[item.Position.X, item.Position.Y] += "e" + item.HP;
            }

            foreach (var item in _state.CurrentLevel.Boxes)
            {
                ms.Cells[item.Position.X, item.Position.Y] += "x";
            }

            var serializedMs = JsonConvert.SerializeObject(ms);

            await Clients.All.SendAsync("MapUpdate", serializedMs);
        }

        public async Task Debug(string user, string message)
        {
            if (message == "gamestarted")
            {
                await this.GameStarted();
            }

            if (message == "next")
            {
                NextLevel();
                await SendUpdatedState();
            }

            if (message == "win")
            {
                await this.SendWin();
            }

            if (message == "lose")
            {
                await this.SendLose();
            }
            //await Clients.All.SendAsync("Receive", user, message);
        }
    }
}
