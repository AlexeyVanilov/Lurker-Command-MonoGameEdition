using Microsoft.Xna.Framework;

namespace LurkerCommand.GameSystem
{
    public static class TeamManager
    {
        public const float TimeMultiplier = 1.2f;

        private static readonly Team[] Teams = new Team[2];
        private static int _currentIndex;

        public static Team CurrentTeam => Teams[_currentIndex];

        public static void Init()
        {
            Teams[0] = new Team(Color.Red, true);
            Teams[1] = new Team(Color.Blue, false);
            _currentIndex = 0;
        }

        public static void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Team current = Teams[_currentIndex];

            current.TimeLeft -= dt;

            if (current.TimeLeft <= 0 || current.Moves <= 0)
            {
                NextTurn();
            }
        }

        public static void NextTurn()
        {
            _currentIndex = (_currentIndex + 1) % Teams.Length;
            Teams[_currentIndex].RefreshTurn();
        }

        public static void AddUnitToTeam(int teamIndex, Unit unit)
        {
            if ((uint)teamIndex < (uint)Teams.Length)
            {
                Teams[teamIndex].AddUnit(unit);
            }
        }

        public static Team GetTeamByIndex(int teamIndex) => Teams[teamIndex];
        public static Team GetCurrentTeam() => Teams[_currentIndex];
    }
}