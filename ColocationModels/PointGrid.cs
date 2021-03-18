using System.Collections.Generic;
using HelpLib;
namespace ColocationModels
{
    public class PointGrid
    {
        #region Property
        private double _gridEdgeLength;
        public double GridEdgeLength
        {
            get { return _gridEdgeLength; }
        }

        public double XMin
        {
            get;
            set;
        } = double.MaxValue;
        public double XMax
        {
            get;
            set;
        } = double.MinValue;

        public double YMin
        {
            get;
            set;
        } = double.MaxValue;
        public double YMax
        {
            get;
            set;
        } = double.MinValue;

        public int RowGridCount
        {
            get;
            set;
        }
        public int ColumnGridCount
        {
            get;
            set;
        }

        private List<PointEvent> _points = new List<PointEvent>();
        public List<PointEvent> Points
        {
            get { return _points; }
        }

        private Dictionary<string, TwoDimensionalDictionary<int>> _pointIndex =
            new Dictionary<string, TwoDimensionalDictionary<int>>();
        public Dictionary<string, TwoDimensionalDictionary<int>> PointIndex
        {
            get { return _pointIndex; }
        }

        private Dictionary<string, int[,]> _prefixCountMatrices = new Dictionary<string, int[,]>();
        public Dictionary<string, int[,]> PrefixCountMatrices
        {
            get { return _prefixCountMatrices; }
        }
        #endregion
        #region Constructor
        public PointGrid()
        {
        }

        public PointGrid(double m_distance)
        {
            _gridEdgeLength = m_distance;
        }
        #endregion
        #region Method
        int QueryPointNumberofType(string m_type)
        {
            return PrefixCountMatrices[m_type][RowGridCount, ColumnGridCount];
        }
        #endregion

    }
}
