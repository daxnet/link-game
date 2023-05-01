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

        private readonly Dictionary<Point, Panel> _panelMap = new();
        private Map _map;
        private Panel _selectedPanel = null;
        private int _selectedTileType = -1;
        private int _selectedX = -1;
        private int _selectedY = -1;

        #endregion Private Fields

        #region Public Constructors

        public FrmMain()
        {
            InitializeComponent();
        }

        #endregion Public Constructors

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
                var rect = new Rectangle(point.X * 64, point.Y * 64, 64, 64);
                g.FillRectangle(Brushes.Orange, rect);
            }
            Thread.Sleep(50);
            foreach (var point in path)
            {
                var rect = new Rectangle(point.X * 64, point.Y * 64, 64, 64);
                g.FillRectangle(SystemBrushes.Control, rect);
            }
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
        }

        private void InitializeTiles()
        {
            pnlMain.Width = _map.Width * 64;
            pnlMain.Height = _map.Height * 64;

            pnlMain.Left = (Width - pnlMain.Width) / 2;
            pnlMain.Top = (Height - pnlMain.Height) / 2;
            pnlMain.Controls.Clear();
            _panelMap.Clear();
            for (var x = 0; x < _map.Width; x++)
                for (var y = 0; y < _map.Height; y++)
                {
                    var pnl = new Panel
                    {
                        Top = y * 64,
                        Left = x * 64,
                        Width = 64,
                        Height = 64,
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
            // RenderMap();
            ConfigureTilePanel(e.X, e.Y, e.NewTileType);
        }
        private void mnuEasyLevel_Click(object sender, EventArgs e)
        {
            InitializeGame(3, 2, 20, IconSet);
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
                        _map.SetTileValue(clickedX, clickedY);
                        _map.SetTileValue(_selectedX, _selectedY);
                        DrawResolutionPath(resolution);
                        if (_map.Resolved)
                        {
                            InitializeGame(3, 2, 20, IconSet);
                            return;
                        }

                        var hasResolution = _map.HasResolution();
                        while (!hasResolution)
                        {
                            _map.Refresh();
                            RenderMap();
                            hasResolution = _map.HasResolution();
                        }
                    }
                }

                _selectedPanel.BackColor = SystemColors.Control;
                _selectedPanel = null;
                _selectedX = -1;
                _selectedY = -1;
                _selectedTileType = -1;
                pnl.BackColor = SystemColors.Control;
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
                _selectedPanel.BackColor = SystemColors.Control;
                _selectedPanel = null;
                _selectedX = -1;
                _selectedY = -1;
                _selectedTileType = -1;
                pnl.BackColor = SystemColors.Control;
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

        protected override void OnClosed(EventArgs e)
        {
            pnlMain.Controls.Clear();
            base.OnClosed(e);
        }

        #endregion Private Methods
    }
}
