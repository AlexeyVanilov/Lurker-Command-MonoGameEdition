namespace LurkerCommand.GameSystem {
    public struct UnitStats {
        public int value;
        public int moves;

        public const int maxMoves = 25;
        public const int maxValue = 9;

        public UnitStats(int value, int moves) {
            this.value = value;
            this.moves = moves;
        }
    }
}