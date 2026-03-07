namespace LurkerCommand.GameSystem {
    public struct UnitStats {
        public sbyte value;
        public sbyte moves;

        public const sbyte maxMoves = 25;
        public const sbyte maxValue = 9;
        public const byte defaultValue = 1;
        public const byte defaultMoves = 1;

        public UnitStats(sbyte value, sbyte moves) {
            this.value = value;
            this.moves = moves;
        }
    }
}