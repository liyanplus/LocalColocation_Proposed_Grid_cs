using System;
using System.Collections.Generic;

namespace ColocationModels
{
    public class PreciseMBR
    {
        #region Property
        private double _minX;
        public double MinX
        {
            get
            {
                return _minX;
            }
        }

        private double _maxX;
        public double MaxX
        {
            get
            {
                return _maxX;
            }
        }

        private double _minY;
        public double MinY
        {
            get
            {
                return _minY;
            }
        }

        private double _maxY;
        public double MaxY
        {
            get
            {
                return _maxY;
            }
        }

        private Dictionary<string, int> _totalPointNumberIn = new Dictionary<string, int>();
        public Dictionary<string, int> TotalPointNumberIn
        {
            get
            {
                return _totalPointNumberIn;
            }
        }
        private Dictionary<string, int> _truePointNumberIn = new Dictionary<string, int>();
        public Dictionary<string, int> TruePointNumberIn
        {
            get
            {
                return _truePointNumberIn;
            }
        }

        private double _participationIndex;
        public double ParticipationIndex
        {
            get
            {
                return _participationIndex;
            }
        }

        #endregion
        #region Constructor
        public PreciseMBR()
        {
        }
        public PreciseMBR(double m_minX, double m_minY, double m_maxX, double m_maxY)
        {
            _minX = m_minX;
            _minY = m_minY;
            _maxX = m_maxX;
            _maxY = m_maxY;
        }
        #endregion
        #region Method
        public void CalculateParticipationIndex()
        {
            _participationIndex = double.MaxValue;
            if (TotalPointNumberIn.Keys.Count == 0)
            {
                _participationIndex = 0;
                return;
            }
            foreach (var evtType in TotalPointNumberIn.Keys)
            {
                if (TruePointNumberIn[evtType] < 2 || TotalPointNumberIn[evtType] == 0)
                {
                    _participationIndex = 0;
                    break;
                }
                double tmp = Convert.ToDouble(TruePointNumberIn[evtType]) / Convert.ToDouble(TotalPointNumberIn[evtType]);
                if (tmp < _participationIndex)
                {
                    _participationIndex = tmp;
                }
            }
        }
        public override string ToString()
        {
			string totalPointStr = "";
			string truePointStr = "";
			foreach (var evtType in TotalPointNumberIn.Keys)
			{
				totalPointStr += string.Format(" {0}: {1},", evtType, TotalPointNumberIn[evtType]);
				truePointStr += string.Format(" {0}: {1},", evtType, TruePointNumberIn[evtType]);
			}
			return string.Format("[PreciseMBR: MinX={0}, MaxX={1}, MinY={2}, MaxY={3}, \nTotalPointNumberIn= {4}, \nTruePointNumberIn= {5}, \nParticipationIndex={6}]", MinX, MaxX, MinY, MaxY, totalPointStr, truePointStr, ParticipationIndex);
        }
        #endregion
    }
}
