using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinkGame.Models
{
    internal sealed class GamePreset
    {
        public GamePreset(string name, int width, int height, int tileTypes, Keys shortcut = Keys.None)
        {
            Name = name;
            Shortcut = shortcut;
            Width = width;
            Height = height;
            TileTypes = tileTypes;
        }

        public string Name { get; }

        public Keys Shortcut { get; }

        public int Width { get; }

        public int Height { get; }

        public int TileTypes { get; }
    }
}
