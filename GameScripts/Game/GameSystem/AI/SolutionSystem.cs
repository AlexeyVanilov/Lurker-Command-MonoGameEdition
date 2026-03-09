using LurkerCommand.MapSystem;
using System;
using System.Collections.Generic;

namespace LurkerCommand.GameSystem.AI
{
    public sealed class SolutionSystem
    {
        private readonly SolutionData _data = new SolutionData();
        private readonly CellFinder _finder = new CellFinder();
        private readonly Queue<Unit> _unitQueue = new Queue<Unit>();
        private Team _team;

        public void Initialize(Team team) => _team = team;

        public void StartTurn()
        {
            _unitQueue.Clear();
            var units = _team.GetUnits();
            for (int i = 0; i < units.Length; i++)
            {
                if (units[i].Value > 1) _unitQueue.Enqueue(units[i]);
            }
            PrepareNextAction();
        }

        private void PrepareNextAction()
        {
            if (_unitQueue.Count > 0 && _team.Moves > 0) _data.Reset();
            else
            {
                _data.Stop();
                TeamManager.NextTurn();
            }
        }

        public void Update(float deltaTime)
        {
            if (!_data.IsThinking || !_team.isTurn) return;

            _data.CurrentTimer += deltaTime;

            if (_data.CurrentTimer >= _data.TargetTime)
            {
                PerformAction();
            }
        }

        private void PerformAction()
        {
            if (_unitQueue.Count == 0 || _team.Moves <= 0)
            {
                PrepareNextAction();
                return;
            }

            Unit unit = _unitQueue.Dequeue();
            Cell target = _finder.FindBestCell(unit);

            if (target != null)
            {
                sbyte dist = (sbyte)(Math.Abs(target.gridPosition.X - unit.gridPosition.X) +
                           Math.Abs(target.gridPosition.Y - unit.gridPosition.Y));

                unit.MoveUnit(target, dist);
                _team.ConsumeMove();
            }

            PrepareNextAction();
        }
    }
}