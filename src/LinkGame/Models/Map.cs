using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LinkGame.Models
{
    /// <summary>
    /// Represents the map of a challenge.
    /// </summary>
    internal sealed class Map
    {

        #region Public Fields

        /// <summary>
        /// Represents the value of a tile which is free (has nothing on it).
        /// </summary>
        public const int FreeTileValue = -1;

        #endregion Public Fields

        #region Private Fields

        private readonly int[,] _mapData;
        private readonly Random _rnd = new(DateTime.Now.Millisecond);
        private readonly Dictionary<int, List<Point>> _tileCoordinates = new();

        #endregion Private Fields

        public event EventHandler<TileChangedEventArgs> TileChanged;

        #region Public Constructors

        public Map(int width, int height, int totalTileTypes)
        {
            _mapData = new int[width, height];
            Width = width;
            Height = height;
            TotalTileTypes = totalTileTypes;
        }

        #endregion Public Constructors

        #region Public Properties

        public int Height { get; }
        public IEnumerable<KeyValuePair<int, List<Point>>> TileCoordinates => _tileCoordinates;
        public int TotalTileTypes { get; }
        public int Width { get; }

        #endregion Public Properties

        #region Public Methods

        public IEnumerable<ResolutionPath> GetPossibleResolutions()
        {
            var result = new List<ResolutionPath>();
            foreach (var kvp in _tileCoordinates)
            {
                var pointsForTile = kvp.Value;
                for (var i = 0; i < pointsForTile.Count; i++)
                    for (var j = i + 1; j < pointsForTile.Count; j++)
                    {
                        if (TryResolve(pointsForTile[i], pointsForTile[j], out var res))
                        {
                            result.Add(res);
                        }
                    }
            }

            return result;
        }

        public void SetTileValue(int x, int y, int val = FreeTileValue)
        {
            if ((val < 1 || val > TotalTileTypes) && val != FreeTileValue)
                return;

            var origValue = _mapData[x, y];
            if (origValue != val)
            {
                _mapData[x, y] = val;
                TileChanged?.Invoke(this, new TileChangedEventArgs(x, y, origValue, val));

                if (origValue != FreeTileValue)
                {
                    _tileCoordinates[origValue].Remove(new Point(x, y));
                }

                if (val != FreeTileValue)
                {
                    _tileCoordinates[val].Add(new Point(x, y));
                }
            }
        }

        public bool Resolved
        {
            get
            {
                for (var x = 0; x < Width; x++)
                    for (var y = 0; y < Height; y++)
                    {
                        if (_mapData[x, y] != FreeTileValue)
                            return false;
                    }
                return true;
            }
        }

        public int GetTileValue(int x, int y)
                    => x < 0 || x >= _mapData.GetLength(0) || y < 0 || y >= _mapData.GetLength(1)
                        ?
                        FreeTileValue
                        :
                        _mapData?[x, y] ?? throw new InvalidOperationException("Map has not been initialized or recreated.");

        public bool HasResolution()
        {
            foreach (var kvp in _tileCoordinates)
            {
                var pointsForTile = kvp.Value;
                for (var i = 0; i < pointsForTile.Count; i++)
                    for (var j = i + 1; j < pointsForTile.Count; j++)
                    {
                        if (TryResolve(pointsForTile[i], pointsForTile[j], out _))
                        {
                            return true;
                        }
                    }
            }

            return false;
        }

        public void Refresh()
        {
            _tileCoordinates.Clear();
            // Computes all the coordinates that should be re-calculated.
            var coordinatesHaveTiles = new List<Point>();
            for (var x = 0; x < Width; x++)
                for (var y = 0; y < Height; y++)
                {
                    if (GetTileValue(x, y) != FreeTileValue)
                    {
                        coordinatesHaveTiles.Add(new Point(x, y));
                    }
                }

            var totalCoordinatesHaveTiles = coordinatesHaveTiles.Count;
            var secondHalfCoordinatesArray = new Point[totalCoordinatesHaveTiles / 2];
            Array.Copy(coordinatesHaveTiles.ToArray(), totalCoordinatesHaveTiles / 2, secondHalfCoordinatesArray, 0, totalCoordinatesHaveTiles / 2);
            var secondHalfCoordinates = secondHalfCoordinatesArray.OrderBy(_ => _rnd.Next()).ToList();
            for (var i = 0; i < totalCoordinatesHaveTiles; i++)
            {
                int tileType, x, y;
                if (i < totalCoordinatesHaveTiles / 2)
                {
                    tileType = _rnd.Next(TotalTileTypes) + 1;
                    x = coordinatesHaveTiles[i].X;
                    y = coordinatesHaveTiles[i].Y;
                }
                else
                {
                    tileType = _mapData[coordinatesHaveTiles[i - totalCoordinatesHaveTiles / 2].X, coordinatesHaveTiles[i - totalCoordinatesHaveTiles / 2].Y];
                    var point = secondHalfCoordinates[i - totalCoordinatesHaveTiles / 2];
                    x = point.X;
                    y = point.Y;
                }
                _mapData[x, y] = tileType;
                if (_tileCoordinates.ContainsKey(tileType))
                {
                    _tileCoordinates[tileType].Add(new Point(x, y));
                }
                else
                {
                    _tileCoordinates.Add(tileType, new List<Point> { new Point(x, y) });
                }
            }
        }

        public bool TryResolve(Point start, Point end, out ResolutionPath resolution) =>
            TryGetDirectPath(start, end, out resolution) ||
            TryGetPathWithOneCorner(start, end, out resolution) ||
            TryGetPathWithTwoCorners(start, end, out resolution);

        #endregion Public Methods

        #region Private Methods

        private bool TryGetDirectPath(Point p1, Point p2, out ResolutionPath resolvedPath)
        {
            resolvedPath = new();
            if ((p1.X == p2.X && Math.Abs(p1.Y - p2.Y) == 1) ||
                (p1.Y == p2.Y && Math.Abs(p1.X - p2.X) == 1))
            {
                resolvedPath.Add(p1);
                resolvedPath.Add(p2);
                return true;
            }
            else
            {
                if (p1.X == p2.X)
                {
                    var stepY = p2.Y - p1.Y > 0 ? 1 : -1;
                    var y = p1.Y + stepY;
                    resolvedPath.Add(p1);
                    while (y != p2.Y)
                    {
                        if (GetTileValue(p1.X, y) != FreeTileValue)
                        {
                            resolvedPath.Clear();
                            return false;
                        }
                        resolvedPath.Add(new Point(p1.X, y));
                        y += stepY;
                    }

                    resolvedPath.Add(p2);
                    return true;
                }
                else if (p1.Y == p2.Y)
                {
                    var stepX = p2.X - p1.X > 0 ? 1 : -1;
                    var x = p1.X + stepX;
                    resolvedPath.Add(p1);
                    while (x != p2.X)
                    {
                        if (GetTileValue(x, p1.Y) != FreeTileValue)
                        {
                            resolvedPath.Clear();
                            return false;
                        }
                        resolvedPath.Add(new Point(x, p1.Y));
                        x += stepX;
                    }
                    resolvedPath.Add(p2);
                    return true;
                }

                resolvedPath.Clear();
                return false;
            }
        }

        private bool TryGetPathWithOneCorner(Point p1, Point p2, out ResolutionPath resolvedPath)
        {
            resolvedPath = new();
            if (p1.X == p2.X || p1.Y == p2.Y)
            {
                return false;
            }

            var cornerPointA = new Point(p1.X, p2.Y);
            if (GetTileValue(cornerPointA.X, cornerPointA.Y) == FreeTileValue &&
                TryGetDirectPath(p1, cornerPointA, out var p1ToCornerAPath) &&
                TryGetDirectPath(cornerPointA, p2, out var cornerAToP2Path))
            {
                resolvedPath = ResolutionPath.Combine(p1ToCornerAPath, cornerAToP2Path);
                return true;
            }

            var cornerPointB = new Point(p2.X, p1.Y);
            if (GetTileValue(cornerPointB.X, cornerPointB.Y) == FreeTileValue &&
                TryGetDirectPath(p1, cornerPointB, out var p1ToCornerBPath) &&
                TryGetDirectPath(cornerPointB, p2, out var cornerBToP2Path))
            {
                resolvedPath = ResolutionPath.Combine(p1ToCornerBPath, cornerBToP2Path);
                return true;
            }

            return false;
        }

        private bool TryGetPathWithTwoCorners(Point p1, Point p2, out ResolutionPath resolvedPath)
        {
            resolvedPath = new ResolutionPath
            {
                p1
            };

            for (var y = p1.Y - 1; y >= -1; y--)
            {
                if (GetTileValue(p1.X, y) == FreeTileValue)
                {
                    var curPoint = new Point(p1.X, y);
                    resolvedPath.Add(curPoint);
                    if (TryGetPathWithOneCorner(curPoint, p2, out var pathN))
                    {
                        resolvedPath = ResolutionPath.Combine(resolvedPath, pathN);
                        resolvedPath.Add(p2);
                        return true;
                    }
                }
                else
                {
                    break;
                }
            }

            resolvedPath.Clear();
            resolvedPath.Add(p1);
            for (var y = p1.Y + 1; y <= Height; y++)
            {
                if (GetTileValue(p1.X, y) == FreeTileValue)
                {
                    var curPoint = new Point(p1.X, y);
                    resolvedPath.Add(curPoint);
                    if (TryGetPathWithOneCorner(curPoint, p2, out var pathS))
                    {
                        resolvedPath = ResolutionPath.Combine(resolvedPath, pathS);
                        resolvedPath.Add(p2);
                        return true;
                    }
                }
                else
                {
                    break;
                }
            }

            resolvedPath.Clear();
            resolvedPath.Add(p1);
            for (var x = p1.X + 1; x <= Width; x++)
            {
                if (GetTileValue(x, p1.Y) == FreeTileValue)
                {
                    var curPoint = new Point(x, p1.Y);
                    resolvedPath.Add(curPoint);
                    if (TryGetPathWithOneCorner(curPoint, p2, out var pathE))
                    {
                        resolvedPath = ResolutionPath.Combine(resolvedPath, pathE);
                        resolvedPath.Add(p2);
                        return true;
                    }
                }
                else
                {
                    break;
                }
            }

            resolvedPath.Clear();
            resolvedPath.Add(p1);
            for (var x = p1.X - 1; x >= -1; x--)
            {
                if (GetTileValue(x, p1.Y) == FreeTileValue)
                {
                    var curPoint = new Point(x, p1.Y);
                    resolvedPath.Add(curPoint);
                    if (TryGetPathWithOneCorner(curPoint, p2, out var pathW))
                    {
                        resolvedPath = ResolutionPath.Combine(resolvedPath, pathW);
                        resolvedPath.Add(p2);
                        return true;
                    }
                }
                else
                {
                    break;
                }
            }

            resolvedPath.Clear();
            return false;
        }

        #endregion Private Methods

    }
}