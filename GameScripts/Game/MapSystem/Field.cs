using GameEngine.Services;
using GameEngine.Systems;
using LurkerCommand.GameScripts.Game.MapSystem.Cells;
using LurkerCommand.GameSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LurkerCommand.MapSystem
{
    public static class Field
    {
        private const int SizeX = 32;
        private const int SizeY = 32;
        private const int CellScale = 1;

        public static readonly Cell[,] cells = new Cell[SizeX, SizeY];

        private static readonly Cell[] ResultBuffer = new Cell[SizeX * SizeY];
        private static readonly List<Cell> VisibleRegistry = new(SizeX * SizeY);

        public static int CellWidth { get; private set; }
        public static int CellHeight { get; private set; }
        public static int MapWidth { get; private set; }
        public static int MapHeight { get; private set; }
        private struct CellChance
        {
            public CellType Type;
            public float Chance;
            public CellChance(CellType type, float chance) {
                Type = type;
                Chance = chance;
            }
        }

        private static readonly (CellType Type, float Weight)[] RawChances = {
    (CellType.DefaultCell, 0.95f),
    (CellType.GoldenCell, 0.05f),
};

        private struct CellWeight
        {
            public CellType Type;
            public float Threshold;
        }

        private static CellWeight[] _chanceTable;
        private static float _totalWeight;
        private static readonly Random _rng = new();
        public const int BaseRangeX = 8;
        public const int BaseRangeY = 5;

        public static bool IsInsideBase(int x, int y, out int team)
        {
            team = -1;
            int baseStartX = (SizeX / 2) - (BaseRangeX / 2);

            if (x >= baseStartX && x < baseStartX + BaseRangeX && y < BaseRangeY)
            {
                bool isBorder = x == baseStartX || x == baseStartX + BaseRangeX - 1 || y == BaseRangeY - 1;
                if (!isBorder)
                {
                    team = 0;
                    return true;
                }
            }

            int botBaseStartY = SizeY - BaseRangeY;
            if (x >= baseStartX && x < baseStartX + BaseRangeX && y >= botBaseStartY)
            {
                bool isBorder = x == baseStartX || x == baseStartX + BaseRangeX - 1 || y == botBaseStartY;
                if (!isBorder)
                {
                    team = 1;
                    return true;
                }
            }

            return false;
        }

        public static List<(Point position, int team)> GetBaseSpawnPoints()
        {
            var spawnPoints = new List<(Point, int)>();
            for (int y = 0; y < SizeY; y++)
            {
                for (int x = 0; x < SizeX; x++)
                {
                    if (cells[x, y].cellType == CellType.GoldenCell && IsInsideBase(x, y, out int team))
                    {
                        spawnPoints.Add((new Point(x, y), team));
                    }
                }
            }
            return spawnPoints;
        }

        public static void InitializeChances()
        {
            _totalWeight = 0;
            _chanceTable = new CellWeight[RawChances.Length];

            for (int i = 0; i < RawChances.Length; i++)
            {
                _totalWeight += RawChances[i].Weight;
                _chanceTable[i] = new CellWeight
                {
                    Type = RawChances[i].Type,
                    Threshold = _totalWeight
                };
            }
        }

        private static CellType GetRandomCellType()
        {
            float roll = (float)(_rng.NextDouble() * _totalWeight);

            for (int i = 0; i < _chanceTable.Length; i++)
            {
                if (roll < _chanceTable[i].Threshold)
                    return _chanceTable[i].Type;
            }

            return _chanceTable[0].Type;
        }
        public static void SetMap(Scene scene)
        {
            InitializeChances();
            Texture2D texture = AssetManager.GetTexture("square");

            CellWidth = texture.Width * CellScale;
            CellHeight = texture.Height * CellScale;
            MapWidth = SizeX * CellWidth;
            MapHeight = SizeY * CellHeight;

            Vector2 scaleVec = new Vector2(CellScale);

            for (int y = 0; y < SizeY; y++)
            {
                for (int x = 0; x < SizeX; x++)
                {
                    Vector2 pos = new Vector2(x * CellWidth, y * CellHeight);

                    CellType finalType = GetCellTypeForPosition(x, y, BaseRangeX, BaseRangeY);

                    Cell cell = GetCellType(pos, scaleVec, finalType);
                    cell.gridPosition = new Point(x, y);
                    cell.cellType = finalType;

                    cells[x, y] = cell;
                    scene.Add(cell);
                }
            }
        }

        private static CellType GetCellTypeForPosition(int x, int y, int rx, int ry)
        {
            int baseStartX = (SizeX / 2) - (rx / 2);

            if (x >= baseStartX && x < baseStartX + rx && y < ry)
            {
                bool isBorder = x == baseStartX || x == baseStartX + rx - 1 || y == ry - 1;
                return isBorder ? CellType.ImmortalCell : CellType.GoldenCell;
            }

            int botBaseStartY = SizeY - ry;
            if (x >= baseStartX && x < baseStartX + rx && y >= botBaseStartY)
            {
                bool isBorder = x == baseStartX || x == baseStartX + rx - 1 || y == botBaseStartY;
                return isBorder ? CellType.ImmortalCell : CellType.GoldenCell;
            }

            return GetRandomCellType();
        }

        private static Cell GetCellType(Vector2 position, Vector2 scale, CellType type) {
            switch (type)
            {
                case CellType.DefaultCell:
                    return new Cell(AssetManager.GetTexture("square"), position, scale);
                case CellType.GoldenCell:
                    return new GoldenCell(AssetManager.GetTexture("golden_square"), position, scale);
                case CellType.ImmortalCell:
                    return new ImmortalCell(AssetManager.GetTexture("square"), position, scale);
                default:
                    return null;
            }
        }
        public static ReadOnlySpan<Cell> GetAvailableCells(Cell start, int range)
            => GetStraightLines(start, range);

        public static ReadOnlySpan<Cell> GetStraightLines(Cell start, int range)
        {
            int count = 0;
            range -= 1;
            Point p = start.gridPosition;

            count = ScanDirection(p, 1, 0, range, count);
            count = ScanDirection(p, -1, 0, range, count);
            count = ScanDirection(p, 0, 1, range, count);
            count = ScanDirection(p, 0, -1, range, count);

            return new ReadOnlySpan<Cell>(ResultBuffer, 0, count);
        }

        private static int ScanDirection(Point start, int dx, int dy, int range, int count)
        {
            for (int i = 1; i <= range; i++)
            {
                int nx = start.X + (dx * i);
                int ny = start.Y + (dy * i);

                if ((uint)nx >= SizeX || (uint)ny >= SizeY) break;

                Cell cell = cells[nx, ny];
                if (!cell.IsEmpty) break;
                ResultBuffer[count++] = cell;
            }
            return count;
        }

        public static void UpdateVisibility(Unit unit)
        {
            if (unit == null) return;

            ClearVisibility();

            UpdateUnitSight(unit);
        }

        public static void UpdateTeamVisibility(Team team)
        {
            if (!team.isPlayer) return;
            ClearVisibility();
            foreach (var unit in team.GetUnits())
            {
                UpdateUnitSight(unit);
            }
        }

        private static void UpdateUnitSight(Unit unit)
        {
            Point p = unit.gridPosition;
            int range = unit.Value;

            int minX = Math.Max(0, p.X - range);
            int maxX = Math.Min(SizeX - 1, p.X + range);
            int minY = Math.Max(0, p.Y - range);
            int maxY = Math.Min(SizeY - 1, p.Y + range);

            for (int y = minY; y <= maxY; y++)
            {
                int dy = Math.Abs(y - p.Y);
                for (int x = minX; x <= maxX; x++)
                {
                    int dx = Math.Abs(x - p.X);

                    if (dx + dy <= range)
                    {
                        Cell cell = cells[x, y];
                        if (!cell.IsVisible)
                        {
                            cell.IsVisible = true;
                            VisibleRegistry.Add(cell);
                        }
                    }
                }
            }
        }

        public static void ClearVisibility()
        {
            var span = CollectionsMarshal.AsSpan(VisibleRegistry);
            for (ushort i = 0; i < span.Length; i++)
            {
                span[i].IsVisible = false;
            }
            VisibleRegistry.Clear();
        }

        public static void ToggleMoveNotes(Cell cell, bool toggle, int range)
        {
            ReadOnlySpan<Cell> available = GetStraightLines(cell, range);
            for (int i = 0; i < available.Length; i++)
            {
                available[i].Toggle(toggle);
            }
        }

        public static Cell GetCellByWorldPos(Vector2 worldPos)
        {
            int x = (int)(worldPos.X / CellWidth);
            int y = (int)(worldPos.Y / CellHeight);
            return GetCell(x, y);
        }

        public static Cell GetCell(Point p) => GetCell(p.X, p.Y);

        public static Cell GetCell(int x, int y) => ((uint)x < SizeX && (uint)y < SizeY) ? cells[x, y] : null;

        public static bool CellInField(int x, int y) => (uint)x < SizeX && (uint)y < SizeY;
        public static bool CellInField(Cell cell) => cell != null && CellInField(cell.gridPosition.X, cell.gridPosition.Y);
    }
}