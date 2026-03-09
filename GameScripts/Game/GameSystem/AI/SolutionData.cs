using System;

namespace LurkerCommand.GameSystem.AI
{
    public sealed class SolutionData
    {
        private readonly Random _rand = new Random();

        public bool IsThinking;
        public float CurrentTimer;
        public float TargetTime;

        public const float MinTime = 0.5f;
        public const float MaxTime = 1.5f;

        public void Reset()
        {
            IsThinking = true;
            CurrentTimer = 0f;
            TargetTime = (float)(_rand.NextDouble() * (MaxTime - MinTime) + MinTime);
        }

        public void Stop()
        {
            IsThinking = false;
            CurrentTimer = 0f;
        }
    }
}