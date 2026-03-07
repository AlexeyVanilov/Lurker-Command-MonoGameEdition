using LurkerCommand.GameScripts.Game.MapSystem.Cells;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace LurkerCommand.MapSystem
{
    public sealed class ImmortalCell : Cell
    {
        private Color _hiddenColor;
        public override bool canCaptured { get; set; } = false;
        public override CellType cellType { get; set; } = CellType.ImmortalCell;
        public override Color defaultColor { get; set; }
        public override Color hiddenColor {
            get => _hiddenColor;
            set {
                UpdateColor(value.R, value.R, value.B);
                _hiddenColor = value;
            }
        }
        public ImmortalCell(Texture2D texture, Vector2 position, Vector2 scale) : base(texture, position, scale) {
            UpdateColor(defaultColor.R, defaultColor.G, defaultColor.B);
        }

        public void UpdateColor(float valueX, float valueY, float valueZ) => _hiddenColor = new Color(MathF.Abs(colorHiddenEffect - defaultColor.R), MathF.Abs(colorHiddenEffect - defaultColor.G), MathF.Abs(colorHiddenEffect - defaultColor.B));
    }
}