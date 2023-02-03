using MazeGeneration;
using Microsoft.AspNetCore.SignalR;

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
            _state.Current = new Voting() { Action = Action.North };
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
                    //case "scribe":
                    //    _state.Current.Scribe = decision; break;
                }

                await Clients.All.SendAsync("VoteReceived", user);

                if (_state.Current.IsFinished())
                {
                    await Clients.All.SendAsync("VotingFinished", _state.Current.CalculateResult());
                }
            }
        }

        public async Task GameStarted()
        {
            if (_state.Mazes.Count == 0)
                _state.Mazes.Add(new Maze(5, 5));

            await Clients.All.SendAsync("MapUpdate", _state.Mazes.First().CellStateToJson());
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
