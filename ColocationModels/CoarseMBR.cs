using System;
using System.Collections.Generic;
using HelpLib;
using System.Linq;

namespace ColocationModels
{
    public class CoarseMBR
    {
        #region Property
        private int _id;
        public int Id
        {
            get { return _id; }
        }

        private string[] _patternTypes;

        private int _minRowIndex;
        public int MinRowIndex
        {
            get { return _minRowIndex; }
        }

        private int _minColumnIndex;
        public int MinColumnIndex
        {
            get { return _minColumnIndex; }
        }

        private int _maxRowIndex;
        public int MaxRowIndex
        {
            get { return _maxRowIndex; }
        }

        private int _maxColumnIndex;
        public int MaxColumnIndex
        {
            get { return _maxColumnIndex; }
        }

        public int checkedMBR
        {
            get;
            set;
        }

        private Dictionary<string, int> _minTotalPointNumberIn = new Dictionary<string, int>();
        private Dictionary<string, int> _minTruePointNumberIn = new Dictionary<string, int>();
        private Dictionary<string, int> _maxTotalPointNumberIn = new Dictionary<string, int>();
        private Dictionary<string, int> _maxTruePointNumberIn = new Dictionary<string, int>();

        private List<PreciseMBR> _preciseMBRList = new List<PreciseMBR>();
        public List<PreciseMBR> PreciseMBRList
        {
            get
            {
                return _preciseMBRList;
            }
        }

        private double _maxPI = double.MaxValue;
        public double MaxParticipationIndex
        {
            get
            {
                return _maxPI;
            }
        }
        #endregion
        #region Constructor
        public CoarseMBR()
        {
        }
        public CoarseMBR(int m_id,
                         int m_minRowIndex, int m_minColumnIndex, int m_maxRowIndex, int m_maxColumnIndex,
                         string[] m_patternTypes,
                         PointGrid m_basePointGrid,
                         Dictionary<string, int[,]> m_participationPointPrefixSum)
        {
            _id = m_id;
            _minRowIndex = m_minRowIndex;
            _minColumnIndex = m_minColumnIndex;
            _maxRowIndex = m_maxRowIndex;
            _maxColumnIndex = m_maxColumnIndex;
            _patternTypes = m_patternTypes;

            //if (MinRowIndex == 4 && MinColumnIndex == 1 && MaxRowIndex == 4 && MaxColumnIndex == 3)
            //{
            //    int aaaaaaa = 2;
            //}


            int maxTotalPointCount, minTotalPointCount, maxTruePointCount, minTruePointCount;
            foreach (var evtType in m_patternTypes)
            {
                maxTotalPointCount = Helper.GetCountInRangeFromPrefixSum(m_basePointGrid.PrefixCountMatrices[evtType],
                                                                       m_minRowIndex,
                                                                       m_minColumnIndex,
                                                                       m_maxRowIndex,
                                                                       m_maxColumnIndex);

                maxTruePointCount = Helper.GetCountInRangeFromPrefixSum(m_participationPointPrefixSum[evtType],
                                                                           m_minRowIndex,
                                                                           m_minColumnIndex,
                                                                           m_maxRowIndex,
                                                                           m_maxColumnIndex);

                minTotalPointCount = Helper.GetCountInRangeFromPrefixSum(m_basePointGrid.PrefixCountMatrices[evtType],
                                                                       m_minRowIndex + 1,
                                                                       m_minColumnIndex + 1,
                                                                       m_maxRowIndex - 1,
                                                                       m_maxColumnIndex - 1);

                minTruePointCount = Helper.GetCountInRangeFromPrefixSum(m_participationPointPrefixSum[evtType],
                                                                       m_minRowIndex + 1,
                                                                       m_minColumnIndex + 1,
                                                                       m_maxRowIndex - 1,
                                                                       m_maxColumnIndex - 1);

                _minTotalPointNumberIn.Add(evtType, minTotalPointCount);
                _minTruePointNumberIn.Add(evtType, minTruePointCount);
                _maxTotalPointNumberIn.Add(evtType, maxTotalPointCount);
                _maxTruePointNumberIn.Add(evtType, maxTruePointCount);

                if (maxTruePointCount < 3)
                {
                    _maxPI = 0;
                    break;
                }

                double tmpPI = Convert.ToDouble(maxTruePointCount) / Convert.ToDouble(minTotalPointCount - minTruePointCount + maxTruePointCount);
                if (MaxParticipationIndex > tmpPI)
                {
                    _maxPI = tmpPI;
                }
            }
        }
        #endregion
        #region Method
        public void PreciseSearch(TwoDimensionalDictionary<int> m_patternIndex, List<ColocationInstance> m_colocationInstances,
                                  PointGrid m_basePointGrid, double m_piThreshold,
                                  int[] m_activeCellIdxes, List<ActiveCell> m_activeCellList, Dictionary<string, List<int>> m_participatingPoints)
        {
            // if this the maxPI of this MBR is greater than the threshold, search precisely.
            double minX, minY, maxX, maxY;

            if (m_activeCellIdxes.Count() == 1)
            {
                var activeCell0PatternIdList = m_patternIndex[m_activeCellList[m_activeCellIdxes[0]].GridRowIndex][m_activeCellList[m_activeCellIdxes[0]].GridColumnIndex];

                for (int activeInstance0IdIdx = 0; activeInstance0IdIdx < activeCell0PatternIdList.Count; activeInstance0IdIdx++)
                {
                    var activeInstance0Id = activeCell0PatternIdList[activeInstance0IdIdx];

                    for (int activeInstance1IdIdx = activeInstance0IdIdx + 1; activeInstance1IdIdx < activeCell0PatternIdList.Count; activeInstance1IdIdx++)
                    {
                        var activeInstance1Id = activeCell0PatternIdList[activeInstance1IdIdx];

                        _GetPreciseMBR(new int[] { activeInstance0Id, activeInstance1Id },
                                       m_colocationInstances,
                                       m_patternIndex,
                                       m_basePointGrid,
                                       m_piThreshold, m_participatingPoints);

                        for (int activeInstance2IdIdx = activeInstance1IdIdx + 1; activeInstance2IdIdx < activeCell0PatternIdList.Count; activeInstance2IdIdx++)
                        {
                            var activeInstance2Id = activeCell0PatternIdList[activeInstance2IdIdx];

                            if (!_GetPreciseMBR(new int[] { activeInstance0Id, activeInstance1Id, activeInstance2Id },
                                                m_colocationInstances,
                                                m_patternIndex,
                                                m_basePointGrid,
                                                m_piThreshold, m_participatingPoints))
                            {
                                continue;
                            }

                            for (int activeInstance3IdIdx = activeInstance2IdIdx + 1; activeInstance3IdIdx < activeCell0PatternIdList.Count; activeInstance3IdIdx++)
                            {
                                var activeInstance3Id = activeCell0PatternIdList[activeInstance3IdIdx];

                                _GetPreciseMBR(new int[] { activeInstance0Id, activeInstance1Id, activeInstance2Id, activeInstance3Id },
                                               m_colocationInstances,
                                               m_patternIndex,
                                               m_basePointGrid,
                                               m_piThreshold, m_participatingPoints);
                            }
                        }
                    }
                }
            }
            else if (m_activeCellIdxes.Count() == 2)
            {
                var activeCell0PatternIdList = m_patternIndex[m_activeCellList[m_activeCellIdxes[0]].GridRowIndex][m_activeCellList[m_activeCellIdxes[0]].GridColumnIndex];
                var activeCell1PatternIdList = m_patternIndex[m_activeCellList[m_activeCellIdxes[1]].GridRowIndex][m_activeCellList[m_activeCellIdxes[1]].GridColumnIndex];

                for (int activeInstance0IdIdx = 0; activeInstance0IdIdx < activeCell0PatternIdList.Count; activeInstance0IdIdx++)
                {
                    var activeInstance0Id = activeCell0PatternIdList[activeInstance0IdIdx];
                    for (int activeInstance1IdIdx = 0; activeInstance1IdIdx < activeCell1PatternIdList.Count; activeInstance1IdIdx++)
                    {
                        var activeInstance1Id = activeCell1PatternIdList[activeInstance1IdIdx];

                        _GetPreciseMBR(new int[] { activeInstance0Id, activeInstance1Id },
                                              m_colocationInstances,
                                              m_patternIndex,
                                              m_basePointGrid,
                                              m_piThreshold, m_participatingPoints);

                        for (int activeInstance2IdIdx = activeInstance0IdIdx + 1; activeInstance2IdIdx < activeCell0PatternIdList.Count; activeInstance2IdIdx++)
                        {
                            var activeInstance2Id = activeCell0PatternIdList[activeInstance2IdIdx];

                            if (!_GetPreciseMBR(new int[] { activeInstance0Id, activeInstance1Id, activeInstance2Id },
                                               m_colocationInstances,
                                               m_patternIndex,
                                               m_basePointGrid,
                                               m_piThreshold, m_participatingPoints))
                            {
                                continue;
                            }

                            for (int activeInstance3IdIdx = activeInstance2IdIdx + 1; activeInstance3IdIdx < activeCell0PatternIdList.Count; activeInstance3IdIdx++)
                            {
                                var activeInstance3Id = activeCell0PatternIdList[activeInstance3IdIdx];

                                _GetPreciseMBR(new int[] { activeInstance0Id, activeInstance1Id, activeInstance2Id, activeInstance3Id },
                                              m_colocationInstances,
                                              m_patternIndex,
                                              m_basePointGrid,
                                              m_piThreshold, m_participatingPoints);
                            }

                            for (int activeInstance3IdIdx = activeInstance1IdIdx + 1; activeInstance3IdIdx < activeCell1PatternIdList.Count; activeInstance3IdIdx++)
                            {
                                var activeInstance3Id = activeCell1PatternIdList[activeInstance3IdIdx];

                                _GetPreciseMBR(new int[] { activeInstance0Id, activeInstance1Id, activeInstance2Id, activeInstance3Id },
                                              m_colocationInstances,
                                              m_patternIndex,
                                              m_basePointGrid,
                                              m_piThreshold, m_participatingPoints);
                            }

                        }

                        for (int activeInstance2IdIdx = activeInstance1IdIdx + 1; activeInstance2IdIdx < activeCell1PatternIdList.Count; activeInstance2IdIdx++)
                        {
                            var activeInstance2Id = activeCell1PatternIdList[activeInstance2IdIdx];

                            if (!_GetPreciseMBR(new int[] { activeInstance0Id, activeInstance1Id, activeInstance2Id },
                                               m_colocationInstances,
                                               m_patternIndex,
                                               m_basePointGrid,
                                               m_piThreshold, m_participatingPoints))
                            {
                                continue;
                            }

                            for (int activeInstance3IdIdx = activeInstance2IdIdx + 1; activeInstance3IdIdx < activeCell1PatternIdList.Count; activeInstance3IdIdx++)
                            {
                                var activeInstance3Id = activeCell1PatternIdList[activeInstance3IdIdx];

                                _GetPreciseMBR(new int[] { activeInstance0Id, activeInstance1Id, activeInstance2Id, activeInstance3Id },
                                              m_colocationInstances,
                                              m_patternIndex,
                                              m_basePointGrid,
                                              m_piThreshold, m_participatingPoints);
                            }
                        }
                    }
                }
            }
            else if (m_activeCellIdxes.Count() == 3)
            {
                var activeCell0PatternIdList = m_patternIndex[m_activeCellList[m_activeCellIdxes[0]].GridRowIndex][m_activeCellList[m_activeCellIdxes[0]].GridColumnIndex];
                var activeCell1PatternIdList = m_patternIndex[m_activeCellList[m_activeCellIdxes[1]].GridRowIndex][m_activeCellList[m_activeCellIdxes[1]].GridColumnIndex];
                var activeCell2PatternIdList = m_patternIndex[m_activeCellList[m_activeCellIdxes[2]].GridRowIndex][m_activeCellList[m_activeCellIdxes[2]].GridColumnIndex];

                for (int activeInstance0IdIdx = 0; activeInstance0IdIdx < activeCell0PatternIdList.Count; activeInstance0IdIdx++)
                {
                    var activeInstance0Id = activeCell0PatternIdList[activeInstance0IdIdx];
                    for (int activeInstance1IdIdx = 0; activeInstance1IdIdx < activeCell1PatternIdList.Count; activeInstance1IdIdx++)
                    {
                        var activeInstance1Id = activeCell1PatternIdList[activeInstance1IdIdx];
                        for (int activeInstance2IdIdx = 0; activeInstance2IdIdx < activeCell2PatternIdList.Count; activeInstance2IdIdx++)
                        {
                            var activeInstance2Id = activeCell2PatternIdList[activeInstance2IdIdx];
                            if (!_GetPreciseMBR(new int[] { activeInstance0Id, activeInstance1Id, activeInstance2Id },
                                               m_colocationInstances,
                                               m_patternIndex,
                                               m_basePointGrid,
                                               m_piThreshold, m_participatingPoints))
                            {
                                continue;
                            }

                            for (int activeInstance3IdIdx = activeInstance0IdIdx + 1; activeInstance3IdIdx < activeCell0PatternIdList.Count; activeInstance3IdIdx++)
                            {
                                var activeInstance3Id = activeCell0PatternIdList[activeInstance3IdIdx];

                                _GetPreciseMBR(new int[] { activeInstance0Id, activeInstance1Id, activeInstance2Id, activeInstance3Id },
                                              m_colocationInstances,
                                              m_patternIndex,
                                              m_basePointGrid,
                                              m_piThreshold, m_participatingPoints);
                            }

                            for (int activeInstance3IdIdx = activeInstance1IdIdx + 1; activeInstance3IdIdx < activeCell1PatternIdList.Count; activeInstance3IdIdx++)
                            {
                                var activeInstance3Id = activeCell1PatternIdList[activeInstance3IdIdx];

                                _GetPreciseMBR(new int[] { activeInstance0Id, activeInstance1Id, activeInstance2Id, activeInstance3Id },
                                              m_colocationInstances,
                                              m_patternIndex,
                                              m_basePointGrid,
                                              m_piThreshold, m_participatingPoints);
                            }

                            for (int activeInstance3IdIdx = activeInstance2IdIdx + 1; activeInstance3IdIdx < activeCell2PatternIdList.Count; activeInstance3IdIdx++)
                            {
                                var activeInstance3Id = activeCell2PatternIdList[activeInstance3IdIdx];

                                _GetPreciseMBR(new int[] { activeInstance0Id, activeInstance1Id, activeInstance2Id, activeInstance3Id },
                                              m_colocationInstances,
                                              m_patternIndex,
                                              m_basePointGrid,
                                              m_piThreshold, m_participatingPoints);
                            }
                        }
                    }
                }
            }
            else if (m_activeCellIdxes.Count() == 4)
            {
                var activeCell0PatternIdList = m_patternIndex[m_activeCellList[m_activeCellIdxes[0]].GridRowIndex][m_activeCellList[m_activeCellIdxes[0]].GridColumnIndex];
                var activeCell1PatternIdList = m_patternIndex[m_activeCellList[m_activeCellIdxes[1]].GridRowIndex][m_activeCellList[m_activeCellIdxes[1]].GridColumnIndex];
                var activeCell2PatternIdList = m_patternIndex[m_activeCellList[m_activeCellIdxes[2]].GridRowIndex][m_activeCellList[m_activeCellIdxes[2]].GridColumnIndex];
                var activeCell3PatternIdList = m_patternIndex[m_activeCellList[m_activeCellIdxes[3]].GridRowIndex][m_activeCellList[m_activeCellIdxes[3]].GridColumnIndex];

                for (int activeInstance0IdIdx = 0; activeInstance0IdIdx < activeCell0PatternIdList.Count; activeInstance0IdIdx++)
                {
                    var activeInstance0Id = activeCell0PatternIdList[activeInstance0IdIdx];
                    for (int activeInstance1IdIdx = 0; activeInstance1IdIdx < activeCell1PatternIdList.Count; activeInstance1IdIdx++)
                    {
                        var activeInstance1Id = activeCell1PatternIdList[activeInstance1IdIdx];
                        for (int activeInstance2IdIdx = 0; activeInstance2IdIdx < activeCell2PatternIdList.Count; activeInstance2IdIdx++)
                        {
                            var activeInstance2Id = activeCell2PatternIdList[activeInstance2IdIdx];

                            if (!_GetBoundaryIndex(new int[] { activeInstance0Id, activeInstance1Id, activeInstance2Id }, m_colocationInstances,
                                                  out minX, out minY, out maxX, out maxY))
                            {
                                continue;
                            }

                            for (int activeInstance3IdIdx = 0; activeInstance3IdIdx < activeCell3PatternIdList.Count; activeInstance3IdIdx++)
                            {
                                var activeInstance3Id = activeCell3PatternIdList[activeInstance3IdIdx];

                                _GetPreciseMBR(new int[] { activeInstance0Id, activeInstance1Id, activeInstance2Id, activeInstance3Id },
                                              m_colocationInstances,
                                              m_patternIndex,
                                              m_basePointGrid,
                                              m_piThreshold, m_participatingPoints);
                            }
                        }
                    }
                }
            }
        }

        private bool _GetBoundaryIndex(int[] m_activeInstanceIdxes, List<ColocationInstance> m_colocationInstances,
                                       out double m_minX, out double m_minY, out double m_maxX, out double m_maxY)
        {
            m_minX = m_colocationInstances[m_activeInstanceIdxes[0]].XCoordinate;
            m_maxX = m_colocationInstances[m_activeInstanceIdxes[0]].XCoordinate;
            m_minY = m_colocationInstances[m_activeInstanceIdxes[0]].YCoordinate;
            m_maxY = m_colocationInstances[m_activeInstanceIdxes[0]].YCoordinate;

            foreach (var instanceId in m_activeInstanceIdxes)
            {
                if (m_minX > m_colocationInstances[instanceId].XCoordinate)
                {
                    m_minX = m_colocationInstances[instanceId].XCoordinate;
                }
                else if (m_maxX < m_colocationInstances[instanceId].XCoordinate)
                {
                    m_maxX = m_colocationInstances[instanceId].XCoordinate;
                }

                if (m_minY > m_colocationInstances[instanceId].YCoordinate)
                {
                    m_minY = m_colocationInstances[instanceId].YCoordinate;
                }
                else if (m_maxY < m_colocationInstances[instanceId].YCoordinate)
                {
                    m_maxY = m_colocationInstances[instanceId].YCoordinate;
                }
            }

            if (m_activeInstanceIdxes.Count() > 2)
            {
                Dictionary<int, List<int>> cellEffectiveBoundary = new Dictionary<int, List<int>>();

                foreach (var instanceIdx in m_activeInstanceIdxes)
                {
                    cellEffectiveBoundary.Add(instanceIdx, new List<int>());
                    if (Math.Abs(m_minX - m_colocationInstances[instanceIdx].XCoordinate) < 1e-2)
                    {
                        cellEffectiveBoundary[instanceIdx].Add(1);
                    }
                    if (Math.Abs(m_maxX - m_colocationInstances[instanceIdx].XCoordinate) < 1e-2)
                    {
                        cellEffectiveBoundary[instanceIdx].Add(2);
                    }
                    if (Math.Abs(m_minY - m_colocationInstances[instanceIdx].YCoordinate) < 1e-2)
                    {
                        cellEffectiveBoundary[instanceIdx].Add(3);
                    }
                    if (Math.Abs(m_maxY - m_colocationInstances[instanceIdx].YCoordinate) < 1e-2)
                    {
                        cellEffectiveBoundary[instanceIdx].Add(4);
                    }
                }

                if (cellEffectiveBoundary.Where((kv) => kv.Value.Count == 0).Count() > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return true;
        }

        private bool _GetPreciseMBR(int[] activeInstanceIds, List<ColocationInstance> m_colocationInstances, TwoDimensionalDictionary<int> m_patternIndex,
                                    PointGrid m_basePointGrid, double m_piThreshold, Dictionary<string, List<int>> m_participatingPoints)
        {
            double minX, minY, maxX, maxY;

            if (!_GetBoundaryIndex(activeInstanceIds, m_colocationInstances, out minX, out minY, out maxX, out maxY))
            {
                return false;
            }

            //if (minX == 1448.565340265 && maxX == 3259.915115965 && minY == 4038.955293295 && maxY == 4629.322922305)
            //{
            //    int aaaaaa = 2;
            //}

            foreach (var oldMBR in PreciseMBRList)
            {
                // if this new mbr is smaller than any old mbr, discard
                if (oldMBR.MinX <= minX && oldMBR.MinY <= minY && oldMBR.MaxX >= maxX && oldMBR.MaxY >= maxY)
                {
                    return true;
                }
            }

            PreciseMBR mbrInstance = new PreciseMBR(minX, minY, maxX, maxY);

            for (int columnIdx = MinColumnIndex; columnIdx <= MaxColumnIndex; columnIdx++)
            {
                _GetPointsCount(m_participatingPoints, m_basePointGrid, mbrInstance, MinRowIndex, columnIdx);

                if (MaxRowIndex != MinRowIndex)
                {
                    _GetPointsCount(m_participatingPoints, m_basePointGrid, mbrInstance, MaxRowIndex, columnIdx);
                }
            }

            for (int rowIdx = MinRowIndex + 1; rowIdx <= MaxRowIndex - 1; rowIdx++)
            {
                _GetPointsCount(m_participatingPoints, m_basePointGrid, mbrInstance, rowIdx, MinColumnIndex);

                if (MaxColumnIndex != MinColumnIndex)
                {
                    _GetPointsCount(m_participatingPoints, m_basePointGrid, mbrInstance, rowIdx, MaxColumnIndex);
                }
            }


            foreach (var evtType in _patternTypes)
            {
                if (!mbrInstance.TotalPointNumberIn.ContainsKey(evtType))
                {
                    mbrInstance.TotalPointNumberIn.Add(evtType, _minTotalPointNumberIn[evtType]);
                }
                else
                {
                    mbrInstance.TotalPointNumberIn[evtType] += _minTotalPointNumberIn[evtType];
                }

                if (!mbrInstance.TruePointNumberIn.ContainsKey(evtType))
                {
                    mbrInstance.TruePointNumberIn.Add(evtType, _minTruePointNumberIn[evtType]);
                }
                else
                {
                    mbrInstance.TruePointNumberIn[evtType] += _minTruePointNumberIn[evtType];
                }
            }

            mbrInstance.CalculateParticipationIndex();

            if (mbrInstance.ParticipationIndex >= m_piThreshold)
            {
                for (int mbrIdx = PreciseMBRList.Count - 1; mbrIdx >= 0; mbrIdx--)
                {
                    // if this new mbr is larger than any old mbr, remove old one
                    var oldMBR = PreciseMBRList[mbrIdx];
                    if (oldMBR.MinX >= minX && oldMBR.MinY >= minY && oldMBR.MaxX <= maxX && oldMBR.MaxY <= maxY)
                    {
                        PreciseMBRList.RemoveAt(mbrIdx);
                    }
                }
                PreciseMBRList.Add(mbrInstance);
            }

            return true;
        }

        private void _GetPointsCount(Dictionary<string, List<int>> m_participatingPoints,
                                     PointGrid m_basePointGrid, PreciseMBR m_mbrInstance,
                                     int m_rowIdx, int m_columnIdx)
        {
            // Get the number of points in a grid cell which are in a precisembr
            // including total points and true points participating in colocation instances

            foreach (var evtType in _patternTypes)
            {
                if (!m_mbrInstance.TotalPointNumberIn.ContainsKey(evtType))
                {
                    m_mbrInstance.TotalPointNumberIn.Add(evtType, 0);
                    m_mbrInstance.TruePointNumberIn.Add(evtType, 0);
                }

                foreach (var truePtId in m_participatingPoints[evtType])
                {
                    if (m_basePointGrid.Points[truePtId].GridRowIndex == m_rowIdx && m_basePointGrid.Points[truePtId].GridColumnIndex == m_columnIdx &&
                        m_basePointGrid.Points[truePtId].XCoordinate >= m_mbrInstance.MinX && m_basePointGrid.Points[truePtId].XCoordinate <= m_mbrInstance.MaxX &&
                        m_basePointGrid.Points[truePtId].YCoordinate >= m_mbrInstance.MinY && m_basePointGrid.Points[truePtId].YCoordinate <= m_mbrInstance.MaxY)
                    {
                        m_mbrInstance.TruePointNumberIn[evtType]++;
                    }
                }

                if (m_basePointGrid.PointIndex[evtType].ContainsKey(m_rowIdx, m_columnIdx))
                {
                    foreach (var evtId in m_basePointGrid.PointIndex[evtType][m_rowIdx][m_columnIdx])
                    {
                        if (m_basePointGrid.Points[evtId].XCoordinate >= m_mbrInstance.MinX && m_basePointGrid.Points[evtId].XCoordinate <= m_mbrInstance.MaxX &&
                            m_basePointGrid.Points[evtId].YCoordinate >= m_mbrInstance.MinY && m_basePointGrid.Points[evtId].YCoordinate <= m_mbrInstance.MaxY)
                        {
                            m_mbrInstance.TotalPointNumberIn[evtType]++;
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            string resultStr = "Precise MBR: \n";
            foreach (var mbr in PreciseMBRList)
            {
                resultStr += string.Format("{0} \n", mbr.ToString());
            }
            return string.Format("[CoarseMBR: MinRowIndex={0}, MinColumnIndex={1}, MaxRowIndex={2}, MaxColumnIndex={3}] \n {4} \n", MinRowIndex, MinColumnIndex, MaxRowIndex, MaxColumnIndex, resultStr);
        }
        #endregion

    }
}
