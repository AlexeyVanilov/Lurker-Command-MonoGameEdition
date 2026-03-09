using Microsoft.Xna.Framework;

namespace LurkerCommand.GameSystem.AI
{
    public sealed class Bot
    {
        private readonly Team _team;
        private readonly SolutionSystem _solutionSystem = new SolutionSystem();
        private bool _wasMyTurn;

        public Bot(Team team)
        {
            _team = team;
            _solutionSystem.Initialize(team);
        }

        public void Update(GameTime gameTime)
        {
            if (_team.isTurn && !_wasMyTurn)
            {
                _solutionSystem.StartTurn();
            }

            _wasMyTurn = _team.isTurn;

            if (_team.isTurn)
            {
                _solutionSystem.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
        }
    }
}