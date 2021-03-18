using System;
using System.Collections.Generic;
using System.Linq;
using HelpLib;
namespace ColocationModels
{
    public class ColocationPattern
    {
        #region Property
        protected string[] _patternTypes;
        public string[] PatternTypes
        {
            get { return _patternTypes; }
        }
        public string TypeLabel
        {
            get { return string.Join("!@#", _patternTypes); }
        }
        public int TypeNumber
        {
            get { return _patternTypes.Length; }
        }

        private List<ColocationInstance> _instances = new List<ColocationInstance>();
        public List<ColocationInstance> Instances
        {
            get { return _instances; }
        }

        private TwoDimensionalDictionary<int> _patternIndex = new TwoDimensionalDictionary<int>();
        public TwoDimensionalDictionary<int> PatternIndex
        {
            get { return _patternIndex; }
        }

        protected int[,] _patternPrefixSum;
        public int[,] PatternPrefixSum
        {
            get { return _patternPrefixSum; }
        }

        protected Dictionary<string, List<int>> _participatingPoints = new Dictionary<string, List<int>>();

        private Dictionary<string, int[,]> _participationPointPrefixSum = new Dictionary<string, int[,]>();
        public Dictionary<string, int[,]> ParticipationPointPrefixSum
        {
            get { return _participationPointPrefixSum; }
        }

        private Dictionary<int, CoarseMBR> _mbr = new Dictionary<int, CoarseMBR>();
        public Dictionary<int, CoarseMBR> MBR
        {
            get { return _mbr; }
        }

        private int _mbrNum = 0;

        private TwoDimensionalDictionary<int> _mbrIndex = new TwoDimensionalDictionary<int>();
        public TwoDimensionalDictionary<int> MBRIndex
        {
            get { return _mbrIndex; }
        }

        #endregion
        #region Constructor
        public ColocationPattern()
        {
        }
        #endregion
        #region Method
        protected void _AddHotspot(PointGrid m_basePointGrid,
                                   int m_minRowIndex, int m_minColumnIndex,
                                   int m_maxRowIndex, int m_maxColumnIndex,
                                   double m_piThreshold,
                                   int[] m_activeCellIdxes, List<ActiveCell> m_activeCellList,
                                   int m_checkedNum)
        {
            CoarseMBR mbrInstance = null;
            if (MBRIndex.ContainsKey(m_minRowIndex, m_minColumnIndex))
            {
                foreach (var mbrId in MBRIndex[m_minRowIndex][m_minColumnIndex])
                {
                    if (MBR[mbrId].MaxRowIndex == m_maxRowIndex && MBR[mbrId].MaxColumnIndex == m_maxColumnIndex)
                    {
                        if (MBR[mbrId].checkedMBR == m_checkedNum && MBR[mbrId].PreciseMBRList.Count != 0)
                        {
                            mbrInstance = MBR[mbrId];
                            mbrInstance.PreciseSearch(PatternIndex, Instances, m_basePointGrid, m_piThreshold,
                                                      m_activeCellIdxes, m_activeCellList, _participatingPoints);
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }

            if (mbrInstance == null)
            {
                //if (m_minRowIndex == 4 && m_minColumnIndex == 5 && m_maxRowIndex == 6 && m_maxColumnIndex == 5)
                //{
                //    int aaaaaaa = 2;
                //}

                //if (m_minRowIndex == 4 && m_minColumnIndex == 5 && m_maxRowIndex == 6 && m_maxColumnIndex == 5)
                //{
                //	int aaaaaaa = 2;
                //}

                //if (m_minRowIndex == 4 && m_minColumnIndex == 1 && m_maxRowIndex == 4 && m_maxColumnIndex == 3)
                //{
                //	int aaaaaaa = 2;
                //}

                mbrInstance = new CoarseMBR(_mbrNum,
                                            m_minRowIndex, m_minColumnIndex,
                                            m_maxRowIndex, m_maxColumnIndex,
                                            PatternTypes,
                                            m_basePointGrid,
                                            ParticipationPointPrefixSum);

                if (mbrInstance.MaxParticipationIndex >= m_piThreshold)
                {
                    mbrInstance.PreciseSearch(PatternIndex, Instances, m_basePointGrid, m_piThreshold,
                                          m_activeCellIdxes, m_activeCellList, _participatingPoints);

                    if (mbrInstance.PreciseMBRList.Count > 0)
                    {
                        // clear precise mbr of the current coarse mbrs smaller than the previous ones.
                        foreach (var otherMinRow in MBRIndex.Keys.Where((k) => k <= m_minRowIndex))
                        {
                            foreach (var otherMinColumn in MBRIndex[otherMinRow].Keys.Where((k) => k <= m_minColumnIndex))
                            {
                                foreach (var mbrId in MBRIndex[otherMinRow][otherMinColumn])
                                {
                                    if (MBR[mbrId].MaxRowIndex >= m_maxRowIndex && MBR[mbrId].MaxColumnIndex >= m_maxColumnIndex)
                                    {
                                        mbrInstance.checkedMBR = m_checkedNum;
                                        mbrInstance.PreciseMBRList.Clear();
                                        MBR.Add(_mbrNum, mbrInstance);
                                        MBRIndex.Add(mbrInstance.MinRowIndex, mbrInstance.MinColumnIndex, mbrInstance.Id);
                                        _mbrNum++;
                                        return;
                                    }
                                }
                            }
                        }

                        // clear the precise mbr of the previous coarse mbrs larger than the current ones.
                        foreach (var otherMinRow in MBRIndex.Keys.Where((k) => k >= m_minRowIndex).ToList())
                        {
                            foreach (var otherMinColumn in MBRIndex[otherMinRow].Keys.Where((k) => k >= m_minColumnIndex).ToList())
                            {
                                foreach (var otherMBRInstanceId in MBRIndex[otherMinRow][otherMinColumn])
                                {
                                    if (MBR[otherMBRInstanceId].MaxRowIndex <= m_maxRowIndex && MBR[otherMBRInstanceId].MaxColumnIndex <= m_maxColumnIndex)
                                    {
                                        MBR[otherMBRInstanceId].PreciseMBRList.Clear();
                                    }
                                }
                            }
                        }

                        mbrInstance.checkedMBR = m_checkedNum;
                        MBR.Add(_mbrNum, mbrInstance);
                        MBRIndex.Add(mbrInstance.MinRowIndex, mbrInstance.MinColumnIndex, mbrInstance.Id);
                        _mbrNum++;
                    }
                }
            }
        }

        protected void _SearchPossibleMBR(List<ActiveCell> m_activeCellList,
                                          PointGrid m_basePointGrid, double m_piThreshold,
                                          int m_checked)
        {
            int minRowIdx, minColumnIdx, maxRowIdx, maxColumnIdx;
            for (int cell1Idx = 0; cell1Idx < m_activeCellList.Count; cell1Idx++)
            {
                _GetBoundaryIndex(new int[] { cell1Idx }, m_activeCellList,
                                 out minRowIdx, out minColumnIdx, out maxRowIdx, out maxColumnIdx);
                _AddHotspot(m_basePointGrid, minRowIdx, minColumnIdx, maxRowIdx, maxColumnIdx, m_piThreshold,
                            new int[] { cell1Idx }, m_activeCellList, m_checked);

                for (int cell2Idx = cell1Idx + 1; cell2Idx < m_activeCellList.Count; cell2Idx++)
                {
                    _GetBoundaryIndex(new int[] { cell1Idx, cell2Idx }, m_activeCellList,
                                      out minRowIdx, out minColumnIdx, out maxRowIdx, out maxColumnIdx);
                    _AddHotspot(m_basePointGrid, minRowIdx, minColumnIdx, maxRowIdx, maxColumnIdx, m_piThreshold,
                                new int[] { cell1Idx, cell2Idx }, m_activeCellList, m_checked);

                    for (int cell3Idx = cell2Idx + 1; cell3Idx < m_activeCellList.Count; cell3Idx++)
                    {
                        if (!_GetBoundaryIndex(new int[] { cell1Idx, cell2Idx, cell3Idx }, m_activeCellList,
                                               out minRowIdx, out minColumnIdx, out maxRowIdx, out maxColumnIdx))
                        {
                            continue;
                        }

                        _AddHotspot(m_basePointGrid, minRowIdx, minColumnIdx, maxRowIdx, maxColumnIdx, m_piThreshold,
                                    new int[] { cell1Idx, cell2Idx, cell3Idx }, m_activeCellList, m_checked);

                        for (int cell4Idx = cell3Idx + 1; cell4Idx < m_activeCellList.Count; cell4Idx++)
                        {
                            if (!_GetBoundaryIndex(new int[] { cell1Idx, cell2Idx, cell3Idx, cell4Idx }, m_activeCellList,
                                                   out minRowIdx, out minColumnIdx, out maxRowIdx, out maxColumnIdx))
                            {
                                continue;
                            }

                            _AddHotspot(m_basePointGrid, minRowIdx, minColumnIdx, maxRowIdx, maxColumnIdx, m_piThreshold,
                                        new int[] { cell1Idx, cell2Idx, cell3Idx, cell4Idx }, m_activeCellList, m_checked);
                        }
                    }
                }
            }
        }

        protected bool _GetBoundaryIndex(int[] m_activeCellIdxes, List<ActiveCell> m_activeCellList,
                                       out int m_minRowIdx, out int m_minColumnIdx, out int m_maxRowIdx, out int m_maxColumnIdx)
        {
            m_minRowIdx = m_activeCellList[m_activeCellIdxes[0]].GridRowIndex;
            m_minColumnIdx = m_activeCellList[m_activeCellIdxes[0]].GridColumnIndex;
            m_maxRowIdx = m_activeCellList[m_activeCellIdxes[0]].GridRowIndex;
            m_maxColumnIdx = m_activeCellList[m_activeCellIdxes[0]].GridColumnIndex;

            foreach (var cellIdx in m_activeCellIdxes)
            {
                if (m_minRowIdx > m_activeCellList[cellIdx].GridRowIndex)
                {
                    m_minRowIdx = m_activeCellList[cellIdx].GridRowIndex;
                }
                else if (m_maxRowIdx < m_activeCellList[cellIdx].GridRowIndex)
                {
                    m_maxRowIdx = m_activeCellList[cellIdx].GridRowIndex;
                }

                if (m_minColumnIdx > m_activeCellList[cellIdx].GridColumnIndex)
                {
                    m_minColumnIdx = m_activeCellList[cellIdx].GridColumnIndex;
                }
                else if (m_maxColumnIdx < m_activeCellList[cellIdx].GridColumnIndex)
                {
                    m_maxColumnIdx = m_activeCellList[cellIdx].GridColumnIndex;
                }
            }

            if (m_activeCellIdxes.Count() > 2)
            {
                Dictionary<int, List<int>> cellEffectiveBoundary = new Dictionary<int, List<int>>();

                foreach (var cellIdx in m_activeCellIdxes)
                {
                    cellEffectiveBoundary.Add(cellIdx, new List<int>());
                    if (m_minRowIdx == m_activeCellList[cellIdx].GridRowIndex)
                    {
                        cellEffectiveBoundary[cellIdx].Add(1);
                    }

                    if (m_maxRowIdx == m_activeCellList[cellIdx].GridRowIndex)
                    {
                        cellEffectiveBoundary[cellIdx].Add(2);
                    }

                    if (m_minColumnIdx == m_activeCellList[cellIdx].GridColumnIndex)
                    {
                        cellEffectiveBoundary[cellIdx].Add(3);
                    }

                    if (m_maxColumnIdx == m_activeCellList[cellIdx].GridColumnIndex)
                    {
                        cellEffectiveBoundary[cellIdx].Add(4);
                    }
                }

                if (cellEffectiveBoundary.Where((kv) => kv.Value.Count == 0).Count() > 0)
                {
                    return false;
                }

                return true;
            }
            else
            {
                return true;
            }
        }

        public override string ToString()
        {
            string mbrString = "";
            foreach (var mbrInstance in MBR.Values)
            {
                if (mbrInstance.PreciseMBRList.Count != 0)
                {
                    mbrString += mbrInstance.ToString();
                    mbrString += "\n";
                }
            }
            return string.Format("ColocationPattern: PatternTypes={0}, Instance Count: {1} \n MBR: \n{2} \n\n\n", string.Join("/ ", PatternTypes), Instances.Count, mbrString);
        }
        #endregion
    }
}
