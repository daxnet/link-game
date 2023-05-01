using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkGame.Models
{
    internal sealed class TileChangedEventArgs : EventArgs
    {
        public TileChangedEventArgs(int x, int y, int oldTileType, int newTileType)
            => (X, Y, OldTileType, NewTileType) = (x, y, oldTileType, newTileType);

        public int X { get; }

        public int Y { get; }

        public int OldTileType { get; }

        public int NewTileType { get; }
    }
}
