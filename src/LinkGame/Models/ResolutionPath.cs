using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LinkGame.Models
{
    /// <summary>
    /// 表示一条连接相同图片的路径。
    /// </summary>
    internal sealed class ResolutionPath : ICollection<Point>
    {
        #region Private Fields

        private readonly List<Point> _points = new();

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// 获取路径中点的个数。
        /// </summary>
        public int Count => _points.Count;

        /// <summary>
        /// 获取一个<see cref="bool"/>值，该值表示整个点的集合是否是只读的。
        /// </summary>
        public bool IsReadOnly => false;

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// 将多个路径组合起来，形成一条完整的路径。
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
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