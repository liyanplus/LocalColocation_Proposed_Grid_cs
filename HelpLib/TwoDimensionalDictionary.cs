using System;
using System.Collections.Generic;
using System.Linq;

namespace HelpLib
{
    public class TwoDimensionalDictionary<TKey> : Dictionary<TKey, Dictionary<TKey, List<int>>> where TKey : IComparable
    {
        #region Constructor
        public TwoDimensionalDictionary()
        {
        }
        #endregion
        #region Method
        public bool ContainsKey(TKey m_rowKey, TKey m_columnKey)
        {
            if (!this.ContainsKey(m_rowKey))
            {
                return false;
            }
            if (!this[m_rowKey].ContainsKey(m_columnKey))
            {
                return false;
            }
            return true;
        }

        public void Add(TKey m_rowKey, TKey m_columnKey, int m_addValue)
        {
            if (!this.ContainsKey(m_rowKey))
            {
                this.Add(m_rowKey, new Dictionary<TKey, List<int>>());
                this[m_rowKey].Add(m_columnKey, new List<int>());
            }
            else if (!this[m_rowKey].ContainsKey(m_columnKey))
            {
                this[m_rowKey].Add(m_columnKey, new List<int>());
            }

            this[m_rowKey][m_columnKey].Add(m_addValue);
        }

        public int[] RangeQuery(TKey m_minRowIndex, TKey m_minColumnIndex, TKey m_maxRowIndex, TKey m_maxColumnIndex)
        {
            var results = new List<int>();

            foreach (var row in this.Keys.Where((rowIndex) => (m_minRowIndex.CompareTo(rowIndex) <= 0 && rowIndex.CompareTo(m_maxRowIndex) <= 0)))
            {
                foreach (var column in this[row].Keys.Where((columnIndex) => (m_minColumnIndex.CompareTo(columnIndex) <= 0 && columnIndex.CompareTo(m_maxColumnIndex) <= 0)))
                {
                    results.AddRange(this[row][column]);
                }
            }
            return results.ToArray();
        }
        #endregion
    }
}
