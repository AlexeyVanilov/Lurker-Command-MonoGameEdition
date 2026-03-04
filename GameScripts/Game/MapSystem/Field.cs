using GameEngine.Services;
using GameEngine.Systems;
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
        private const int CellScale = 2;

        public static readonly Cell[,] cells = new Cell[SizeX, SizeY];

        private static readonly Cell[] ResultBuffer = new Cell[SizeX * SizeY];
        private static readonly List<Cell> VisibleRegistry = new(SizeX * SizeY);

        public static int CellWidth { get; private set; }
        public static int CellHeight { get; private set; }
        public static int MapWidth { get; private set; }
        public static int MapHeight { get; private set; }

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
                    Cell cell = new Cell(texture, pos, scaleVec);
                    cell.gridPosition = new Point(x, y);

                    cells[x, y] = cell;
                    scene.Add(cell);
                }
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
            Point p = unit.gridPosition;
            int range = unit.Value;

            for (int x = -range; x <= range; x++)
            {
                for (int y = -range; y <= range; y++)
                {
                    if (Math.Abs(x) + Math.Abs(y) > range) continue;

                    int cx = p.X + x;
                    int cy = p.Y + y;

                    if ((uint)cx < SizeX && (uint)cy < SizeY)
                    {
                        Cell cell = cells[cx, cy];
                        cell.IsVisible = true;
                        VisibleRegistry.Add(cell);
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