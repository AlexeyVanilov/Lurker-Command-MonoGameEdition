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
        private static readonly Random _rng = new();
        public const int BaseRangeX = 8;
        public const int BaseRangeY = 5;

        public static bool IsInsideBase(int x, int y, out int team, out bool isBorder)
        {
            team = -1;
            isBorder = false;
            int baseStartX = (SizeX / 2) - (BaseRangeX / 2);

            if (x >= baseStartX && x < baseStartX + BaseRangeX)
            {
                if (y < BaseRangeY)
                {
                    team = 0;
                    isBorder = x == baseStartX || x == baseStartX + BaseRangeX - 1 || y == BaseRangeY - 1;
                    return true;
                }

                int botBaseStartY = SizeY - BaseRangeY;
                if (y >= botBaseStartY)
                {
                    team = 1;
                    isBorder = x == baseStartX || x == baseStartX + BaseRangeX - 1 || y == botBaseStartY;
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
                    if (cells[x, y]?.cellType == CellType.GoldenCell && IsInsideBase(x, y, out int team, out _))
                    {
                        spawnPoints.Add((new Point(x, y), team));
                    }
                }
            }
            return spawnPoints;
        }

        public static void SetMap(Scene scene)
        {
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
                    CellType type = CellType.DefaultCell;
                    int team = -1;

                    if (IsInsideBase(x, y, out team, out bool isBorder))
                    {
                        type = isBorder ? CellType.ImmortalCell : CellType.GoldenCell;
                    }
                    Cell cell = CreateCell(pos, scaleVec, type, team);
                    cell.gridPosition = new Point(x, y);
                    cell.cellType = type;
                    cells[x, y] = cell;
                    scene.Add(cell);
                }
            }
        }

        private static Cell CreateCell(Vector2 pos, Vector2 scale, CellType type, int team)
        {
            return type switch
            {
                CellType.GoldenCell => new GoldenCell(AssetManager.GetTexture("golden_square"), pos, scale),
                CellType.ImmortalCell => new ImmortalCell(AssetManager.GetTexture("square"), pos, scale, team),
                _ => new Cell(AssetManager.GetTexture("square"), pos, scale)
            };
        }
        public static ReadOnlySpan<Cell> GetAvailableCells(Cell start, int range)
        {
            int count = 0;
            int effectiveRange = range - 1;
            Point p = start.gridPosition;

            count = ScanDirection(p, 1, 0, effectiveRange, count);
            count = ScanDirection(p, -1, 0, effectiveRange, count);
            count = ScanDirection(p, 0, 1, effectiveRange, count);
            count = ScanDirection(p, 0, -1, effectiveRange, count);

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
                if (cell == null) break;
                if(!cell.IsEmpty) {
                    ResultBuffer[count++] = cell;
                    break;
                }
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
            if (team == null || !team.isPlayer) return;
            ClearVisibility();
            var units = team.GetUnits();
            for (int i = 0; i < units.Length; i++)
            {
                UpdateUnitSight(units[i]);
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
                        if (cell != null && !cell.IsVisible)
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
            for (int i = 0; i < span.Length; i++)
            {
                span[i].IsVisible = false;
            }
            VisibleRegistry.Clear();
        }

        public static void ToggleMoveNotes(Cell cell, bool toggle, int range)
        {
            ReadOnlySpan<Cell> available = GetAvailableCells(cell, range);
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