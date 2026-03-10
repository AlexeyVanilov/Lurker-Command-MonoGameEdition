using GameEngine.Components.UI;
using GameEngine.Services;
using GameEngine.Systems;
using LurkerCommand.GameScripts.Game.MapSystem.Cells;
using LurkerCommand.GameSystem;
using LurkerCommand.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LurkerCommand.MapSystem
{
    public class Cell : Entity, IGrid
    {
        public virtual bool canCaptured => true;
        public virtual byte idleBonus => 1;
        public virtual Color defaultColor => Color.White;
        public virtual Color hiddenColor => new Color(175, 175, 175);
        public virtual CellType cellType => CellType.DefaultCell;
        public Point gridPosition { get; set; }
        public Image cellImage;
        public Unit currentUnit = null;
        public bool IsCaptured { get; private set; }
        public Team OwnerTeam { get; private set; }

        private Color _tintedColor;
        private const float tintFactor = 0.25f;
        private bool _isVisible = false;
        public bool IsEmpty { get; set; } = true;
        protected Text moveNote;

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible == value) return;
                _isVisible = value;
                cellImage.Color = value ? (IsCaptured ? _tintedColor : defaultColor) : hiddenColor;
                if (currentUnit != null) currentUnit.isVisible = value;
            }
        }

        public Cell(Texture2D texture, Vector2 position, Vector2 scale) : base(position, scale, 0f, true)
        {
            cellImage = new Image(texture, Vector2.Zero, scale, hiddenColor) { Transform = { Parent = Transform }, OrderInLayer = 0 };
            moveNote = new Text(AssetManager.GetFont("Arial"), "·", cellImage.GetSize().ToVector2() / 2, Color.LightGreen, true)
            { Transform = { Parent = Transform, LocalScale = scale }, IsActive = false };
        }

        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {
            cellImage.Draw(gameTime, sb);
            if (moveNote.IsActive) moveNote.Draw(gameTime, sb);
        }

        public void Capture(Team team)
        {
            if (!canCaptured && IsEmpty) return;
            OwnerTeam = team;
            IsCaptured = true;

            _tintedColor = Color.Lerp(Color.White, team.TeamColor, tintFactor);

            if (IsVisible) cellImage.Color = _tintedColor;
        }

        public void Uncapture()
        {
            IsCaptured = false;
            OwnerTeam = null;
            if (IsVisible) cellImage.Color = defaultColor;
        }

        public void BindUnit(Unit unit) {
            unit.giveBonus = true;
            currentUnit = unit;
            IsEmpty = false;
            if (unit != null) unit.isVisible = IsVisible;
        }

        public void Unbind() {
            if (currentUnit == null) return;
            currentUnit.giveBonus = false;
            currentUnit = null; 
            IsEmpty = true; 
        }
        public void Toggle(bool toggle) => moveNote.IsActive = toggle;
    }
}