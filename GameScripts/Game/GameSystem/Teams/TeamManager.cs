using LurkerCommand.GameSystem.AI;
using Microsoft.Xna.Framework;
using System;

namespace LurkerCommand.GameSystem
{
    public static class TeamManager
    {
        public const float TimeMultiplier = 1.2f;
        private static readonly Team[] Teams = new Team[2];
        private static Bot bot;
        private static int _currentIndex;

        public static Team CurrentTeam => Teams[_currentIndex];

        public static void Init()
        {
            Teams[0] = new Team(Color.Red, true) { Name = "Red" };
            Teams[1] = new Team(Color.Blue, false) { Name = "Blue" };
            bot = new Bot(Teams[1]);
            _currentIndex = 0;
            Teams[_currentIndex].RefreshTurn();
        }

        public static void Update(GameTime gameTime)
        {
            Team current = Teams[_currentIndex];
            current.TimeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            GameUI.UpdateTime(current.Name, (int)MathF.Ceiling(current.TimeLeft));

            if (current.TimeLeft <= 0)
            {
                NextTurn();
            }
            bot.Update(gameTime);
        }

        public static void NextTurn()
        {
            Teams[_currentIndex].SkipMove();
            _currentIndex = (_currentIndex + 1) % Teams.Length;
            Teams[_currentIndex].RefreshTurn();
        }

        public static void AddUnitToTeam(int teamIndex, Unit unit)
        {
            if ((uint)teamIndex < (uint)Teams.Length) Teams[teamIndex].AddUnit(unit);
        }

        public static Team GetTeamByIndex(int teamIndex) => Teams[teamIndex];
    }
}