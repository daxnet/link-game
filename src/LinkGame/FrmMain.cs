using LinkGame.Models;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinkGame
{
    public partial class FrmMain : Form
    {

        #region Private Fields

        private const string IconSet = @"Icons\circus.zip";
        private const int TileSize = 64;
        private readonly BackgroundMusic _bgm = new(new[]
        {
            "Music\\1.mp3",
            "Music\\2.mp3",
            "Music\\3.mp3",
            "Music\\4.mp3" })
        {
            Volume = 0.1f
        };

        private readonly FrmDiag _diagForm = new FrmDiag();
        private readonly List<GamePreset> _gamePresets = new()
        {
            new GamePreset("简单", 8, 5, 10, Keys.F2),
            new GamePreset("较难", 10, 6, 12, Keys.F3),
            new GamePreset("困难", 12, 8, 15, Keys.F4),
            new GamePreset("挑战", 15, 8, 20, Keys.F5),
        };
        private readonly Dictionary<Point, Panel> _panelMap = new();
        private readonly SoundPlayer _soundPlayer = new(@"Music\click.wav");
        private Map _map;
        private Panel _selectedPanel = null;
        private GamePreset _selectedPreset;
        private int _selectedTileType = -1;
        private int _selectedX = -1;
        private int _selectedY = -1;

        #endregion Private Fields

        #region Public Constructors

        public FrmMain()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }

        #endregion Public Constructors

        #region Protected Methods

        protected override void OnClosed(EventArgs e)
        {
            pnlMain.Controls.Clear();
            foreach (ToolStripMenuItem item in mnuNewGame.DropDownItems)
            {
                item.Click -= Tmnu_Click;
            }

            _bgm.Stop();
            _bgm.Dispose();
            _soundPlayer.Dispose();

            base.OnClosed(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            mnuNewGame.DropDownItems.Clear();
            foreach (var preset in _gamePresets)
            {
                var tmnu = new ToolStripMenuItem(preset.Name)
                {
                    ShortcutKeys = preset.Shortcut,
                    Tag = preset,
                };
                tmnu.Click += Tmnu_Click;
                mnuNewGame.DropDownItems.Add(tmnu);
            }

            mnuStopGame.Enabled = false;
            mnuShowDiagWindow.Enabled = false;
        }

        #endregion Protected Methods

        #region Private Methods

        private void ConfigureTilePanel(int x, int y, int tileType)
        {
            var pnl = _panelMap[new Point(x, y)];
            if (tileType != Map.FreeTileValue)
            {
                pnl.Name = $"tile_{x}_{y}";
                pnl.BackgroundImage = imageList.Images[$"{tileType}"];
                pnl.BackgroundImageLayout = ImageLayout.Zoom;
                pnl.Tag = $"Tile_{x}_{y}_{tileType}";
                pnl.Visible = true;
            }
            else
            {
                pnl.BackgroundImage = null;
                pnl.BackgroundImageLayout = ImageLayout.None;
                pnl.Tag = $"Tile_{x}_{y}_{-1}";
                pnl.Visible = false;
            }
        }

        private void DrawResolutionPath(ResolutionPath path)
        {
            using var g = pnlMain.CreateGraphics();
            foreach (var point in path)
            {
                var rect = new Rectangle(point.X * TileSize, point.Y * TileSize, TileSize, TileSize);
                g.FillRectangle(Brushes.LightBlue, rect);
            }
            Thread.Sleep(50);
            foreach (var point in path)
            {
                var rect = new Rectangle(point.X * TileSize, point.Y * TileSize, TileSize, TileSize);
                g.FillRectangle(SystemBrushes.Control, rect);
            }
        }

        private void FrmMain_DpiChanged(object sender, DpiChangedEventArgs e)
        {

        }

        private void FrmMain_Resize(object sender, EventArgs e)
        {
            pnlMain.Left = (Width - pnlMain.Width) / 2;
            pnlMain.Top = (Height - pnlMain.Height) / 2;
        }

        private void InitializeGame(int width, int height, int totalTileTypes, string iconSet)
        {
            _selectedPanel = null;
            _selectedTileType = -1;
            _selectedX = -1;
            _selectedY = -1;

            LoadIconSet(iconSet);

            if (_map != null)
            {
                _map.TileChanged -= Map_TileChanged;
            }

            _map = new Map(width, height, totalTileTypes);
            _map.TileChanged += Map_TileChanged;

            var hasResolution = false;
            while (!hasResolution)
            {
                _map.Refresh();
                hasResolution = _map.HasResolution();
            }

            InitializeTiles();
            RenderMap();

            _diagForm.Map = _map;
            _diagForm.ShowDiagnosticInformation();

            _bgm.Play();
        }

        private void InitializeTiles()
        {
            pnlMain.Visible = true;
            pnlMain.Width = _map.Width * TileSize;
            pnlMain.Height = _map.Height * TileSize;

            pnlMain.Left = (Width - pnlMain.Width) / 2;
            pnlMain.Top = (Height - pnlMain.Height) / 2;
            pnlMain.Controls.Clear();
            _panelMap.Clear();
            for (var x = 0; x < _map.Width; x++)
                for (var y = 0; y < _map.Height; y++)
                {
                    var pnl = new DoubleBufferedPanel
                    {
                        Top = y * TileSize,
                        Left = x * TileSize,
                        Width = TileSize,
                        Height = TileSize,
                    };
                    pnl.Click += pnl_Clicked;
                    _panelMap.Add(new Point(x, y), pnl);
                    pnlMain.Controls.Add(pnl);
                }
            pnlMain.Invalidate();
        }

        private void LoadIconSet(string iconSet)
        {
            imageList.Images.Clear();
            using var zipFile = ZipFile.OpenRead(iconSet);
            foreach (var entry in zipFile.Entries)
            {
                var bmp = new Bitmap(entry.Open());
                imageList.Images.Add(Path.GetFileNameWithoutExtension(entry.Name), bmp);
            }
        }
        private void Map_TileChanged(object sender, TileChangedEventArgs e)
        {
            ConfigureTilePanel(e.X, e.Y, e.NewTileType);
        }

        private void mnuShowDiagWindow_Click(object sender, EventArgs e)
        {
            _diagForm.Show();
        }

        private void mnuStopGame_Click(object sender, EventArgs e)
        {
            pnlMain.Controls.Clear();
            pnlMain.Visible = false;
            _bgm.Stop();
            mnuNewGame.Enabled = true;
            mnuStopGame.Enabled = false;
            mnuShowDiagWindow.Enabled = false;
            _diagForm.Close();
        }

        private void pnl_Clicked(object sender, EventArgs e)
        {
            var pnl = (Panel)sender;
            var tag = pnl.Tag.ToString();
            var splittedTags = tag.Split('_');
            var clickedX = Convert.ToInt32(splittedTags[1]);
            var clickedY = Convert.ToInt32(splittedTags[2]);
            var clickedTileType = Convert.ToInt32(splittedTags[3]);
            if (clickedTileType == _selectedTileType)
            {
                if (clickedX != _selectedX || clickedY != _selectedY)
                {
                    if (_map.TryResolve(new Point(clickedX, clickedY), new Point(_selectedX, _selectedY), out var resolution))
                    {
                        _soundPlayer.Play();
                        _map.SetTileValue(clickedX, clickedY);
                        _map.SetTileValue(_selectedX, _selectedY);
                        DrawResolutionPath(resolution);
                        if (_map.Resolved)
                        {
                            InitializeGame(_selectedPreset.Width, _selectedPreset.Height, _selectedPreset.TileTypes, IconSet);
                            return;
                        }

                        var hasResolution = _map.HasResolution();
                        while (!hasResolution)
                        {
                            _map.Refresh();
                            RenderMap();
                            hasResolution = _map.HasResolution();
                        }

                        _diagForm.ShowDiagnosticInformation();
                    }
                }

                _selectedPanel.BackColor = Color.Transparent;
                _selectedPanel = null;
                _selectedX = -1;
                _selectedY = -1;
                _selectedTileType = -1;
                pnl.BackColor = Color.Transparent;
            }
            else if (_selectedTileType == -1)
            {
                _selectedPanel = pnl;
                _selectedTileType = clickedTileType;
                _selectedX = clickedX;
                _selectedY = clickedY;
                pnl.BackColor = SystemColors.Highlight;
            }
            else
            {
                _selectedPanel.BackColor = Color.Transparent;
                _selectedPanel = null;
                _selectedX = -1;
                _selectedY = -1;
                _selectedTileType = -1;
                pnl.BackColor = Color.Transparent;
            }
        }

        private void pnlMain_ControlRemoved(object sender, ControlEventArgs e)
        {
            if (e.Control is Panel pnl && pnl.Tag.ToString().StartsWith("Tile"))
            {
                pnl.Click -= pnl_Clicked;
                pnl.Dispose();
            }
        }

        private void RenderMap()
        {
            for (var x = 0; x < _map.Width; x++)
                for (var y = 0; y < _map.Height; y++)
                {
                    ConfigureTilePanel(x, y, _map.GetTileValue(x, y));
                }
        }
        private void Tmnu_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem tmnu)
            {
                _selectedPreset = (GamePreset)tmnu.Tag;
                InitializeGame(_selectedPreset.Width, _selectedPreset.Height, _selectedPreset.TileTypes, IconSet);
                mnuStopGame.Enabled = true;
                mnuNewGame.Enabled = false;
                mnuShowDiagWindow.Enabled = true;
            }
        }

        #endregion Private Methods

    }
}
