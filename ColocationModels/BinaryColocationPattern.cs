using System;
using System.Collections.Generic;
using System.Linq;
using HelpLib;

namespace ColocationModels
{
    public class BinaryColocationPattern : ColocationPattern
    {
        #region Property
        #endregion
        #region Constructor
        public BinaryColocationPattern() { }
        public BinaryColocationPattern(string[] m_patternTypes, PointGrid m_basePointGrid,
                                       double m_piThreshold)
        {
            _patternTypes = m_patternTypes;
            Array.Sort(_patternTypes, StringComparer.InvariantCulture);

            // initialize prefix sum matrix
            foreach (var type in PatternTypes)
            {
                ParticipationPointPrefixSum.Add(type,
                                                new int[m_basePointGrid.RowGridCount + 1,
                                                        m_basePointGrid.ColumnGridCount + 1]);
            }
            _patternPrefixSum = new int[m_basePointGrid.RowGridCount + 1,
                                        m_basePointGrid.ColumnGridCount + 1];

            BuildInstances(m_basePointGrid);

            GenerateHotspot(m_basePointGrid, m_piThreshold);
        }
        #endregion
        #region Method
        public void BuildInstances(PointGrid m_basePointGrid)
        {
            // build binary colocation instances based on neighbor graph
            foreach (var type in PatternTypes)
            {
                _participatingPoints.Add(type, new List<int>());
            }

            foreach (var aPoint in m_basePointGrid.Points.Where((pt) => string.Equals(pt.TypeLabel, PatternTypes[0])))
            {
                bool doesExist = false;
                if (!aPoint.NeighborPointIds.ContainsKey(PatternTypes[1]))
                    continue;

                foreach (var bPointId in aPoint.NeighborPointIds[PatternTypes[1]])
                {
                    doesExist = true;
                    var instance = new ColocationInstance(Instances.Count, new int[] { aPoint.Id, bPointId }, m_basePointGrid);
                    Instances.Add(instance);

                    PatternIndex.Add(instance.GridRowIndex, instance.GridColumnIndex, instance.Id);

                    PatternPrefixSum[instance.GridRowIndex + 1, instance.GridColumnIndex + 1]++;

                    _participatingPoints[m_basePointGrid.Points[bPointId].TypeLabel].Add(bPointId);
                }
                if (doesExist)
                {
                    _participatingPoints[aPoint.TypeLabel].Add(aPoint.Id);
                }
            }

            Helper.CalculatePrefixSum(PatternPrefixSum,
                                      m_basePointGrid.RowGridCount + 1,
                                      m_basePointGrid.ColumnGridCount + 1);

            foreach (var evtType in PatternTypes)
            {
                _participatingPoints[evtType] = _participatingPoints[evtType].Distinct().ToList();
                foreach (var pointId in _participatingPoints[evtType])
                {
                    ParticipationPointPrefixSum[evtType][m_basePointGrid.Points[pointId].GridRowIndex + 1,
                                                      m_basePointGrid.Points[pointId].GridColumnIndex + 1]++;
                }
                Helper.CalculatePrefixSum(ParticipationPointPrefixSum[evtType],
                                          m_basePointGrid.RowGridCount + 1,
                                          m_basePointGrid.ColumnGridCount + 1);
            }
        }

        public void GenerateHotspot(PointGrid m_basePointGrid, double m_piThreshold)
        {
            // this function is only useful for binary colocation pattern
            List<ActiveCell> activeCellList = new List<ActiveCell>();
            foreach (var cellRowIdx in PatternIndex.Keys)
            {
                foreach (var cellColumnIdx in PatternIndex[cellRowIdx].Keys)
                {
                    activeCellList.Add(new ActiveCell(activeCellList.Count, cellRowIdx, cellColumnIdx));
                }
            }

            _SearchPossibleMBR(activeCellList, m_basePointGrid, m_piThreshold, 0);
        }


        #endregion
    }
}
