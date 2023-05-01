using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LinkGame.Models
{
    internal sealed class ResolutionPath : ICollection<Point>
    {
        #region Private Fields

        private readonly List<Point> _points = new();

        #endregion Private Fields

        #region Public Properties

        public int Count => _points.Count;

        public bool IsReadOnly => false;

        #endregion Public Properties

        #region Public Methods

        public static ResolutionPath Combine(params ResolutionPath[] paths)
        {
            var result = new ResolutionPath();
            foreach (var path in paths)
                foreach (var point in path)
                {
                    result.Add(point);
                }
            return result;
        }

        public void Add(Point item)
        {
            if (_points.Contains(item)) return;
            _points.Add(item);
        }

        public void Clear() => _points.Clear();

        public bool Contains(Point item) => _points.Contains(item);

        public void CopyTo(Point[] array, int arrayIndex) => _points.CopyTo(array, arrayIndex);

        public IEnumerator<Point> GetEnumerator() => _points.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _points.GetEnumerator();

        public bool Remove(Point item) => _points.Remove(item);

        public override string ToString() => string.Join("->", _points.Select(p => $"({p.X},{p.Y})"));

        #endregion Public Methods
    }
}