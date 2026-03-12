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

        public static Team CurrentTeam {
            get => Teams[_currentIndex];
        }

        public static void Init()
        {
            Teams[0] = new Team(Color.Red, true) { Name = "Red" };
            Teams[1] = new Team(Color.Blue, false) { Name = "Blue" };
            bot = new Bot(Teams[1]);
            _currentIndex = 1;
        }

        public static void Update(GameTime gameTime)
        {
            CurrentTeam.TimeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            GameUI.UpdateTime(CurrentTeam.Name, (int)MathF.Ceiling(CurrentTeam.TimeLeft));

            if (CurrentTeam.TimeLeft <= 0)
            {
                NextTurn();
            }
            bot.Update(gameTime);
        }

        public static void NextTurn()
        {
            CurrentTeam.SkipMove();
            _currentIndex = (_currentIndex + 1) % Teams.Length;
            CurrentTeam.RefreshTurn();
        }

        public static void AddUnitToTeam(int teamIndex, Unit unit)
        {
            if ((uint)teamIndex < (uint)Teams.Length) Teams[teamIndex].AddUnit(unit);
        }
        public static Team GetTeamByIndex(int teamIndex) => Teams[teamIndex];
    }
}