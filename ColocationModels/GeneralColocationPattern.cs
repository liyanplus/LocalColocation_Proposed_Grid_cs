using System;
using System.Collections.Generic;
using System.Linq;
using HelpLib;
namespace ColocationModels
{
    public class GeneralColocationPattern : ColocationPattern
    {
        #region Constructor
        public GeneralColocationPattern()
        {
        }

        public GeneralColocationPattern(
            PointGrid m_basePointGrid,
            ColocationPattern m_aPattern, ColocationPattern m_bPattern,
            double m_piThreshold)
        {
            _patternTypes = m_aPattern.PatternTypes.Union(m_bPattern.PatternTypes).ToArray();
            Array.Sort(_patternTypes, StringComparer.InvariantCulture);

            if (TypeNumber != m_aPattern.TypeNumber + 1 || TypeNumber != m_bPattern.TypeNumber + 1)
            {
                // join is not valid.
                _patternTypes = new string[0];
                return;
            }

            string aExtraType = PatternTypes.Except(m_aPattern.PatternTypes).ElementAt(0);
            string bExtraType = PatternTypes.Except(m_bPattern.PatternTypes).ElementAt(0);

            if ((Array.IndexOf(PatternTypes, aExtraType) == 0 && Array.IndexOf(PatternTypes, bExtraType) == PatternTypes.Count() - 1) ||
                (Array.IndexOf(PatternTypes, aExtraType) == PatternTypes.Count() - 1 && Array.IndexOf(PatternTypes, bExtraType) == 0))
            {
                // initialize prefix sum matrix
                foreach (var type in PatternTypes)
                {
                    ParticipationPointPrefixSum.Add(type,
                                                    new int[m_basePointGrid.RowGridCount + 1,
                                                            m_basePointGrid.ColumnGridCount + 1]);
                }
                _patternPrefixSum = new int[m_basePointGrid.RowGridCount + 1,
                                            m_basePointGrid.ColumnGridCount + 1];

                BuildInstances(m_basePointGrid, m_aPattern, m_bPattern);

                GenerateHotspot(m_basePointGrid, m_piThreshold, m_aPattern, m_bPattern);
            }
        }
        #endregion
        #region Method
        public void BuildInstances(PointGrid m_basePointGrid, ColocationPattern m_aPattern, ColocationPattern m_bPattern)
        {
            string extraType;
            List<ColocationInstance> baseInstances;
            if (m_aPattern.Instances.Count <= m_bPattern.Instances.Count)
            {
                extraType = PatternTypes.Except(m_aPattern.PatternTypes).ElementAt(0);
                baseInstances = m_aPattern.Instances;
            }
            else
            {
                extraType = PatternTypes.Except(m_bPattern.PatternTypes).ElementAt(0);
                baseInstances = m_bPattern.Instances;
            }

            foreach (var type in PatternTypes)
            {
                _participatingPoints.Add(type, new List<int>());
            }

            foreach (var baseInstance in baseInstances)
            {
                if (!m_basePointGrid.Points[baseInstance.EventIndices[0]].NeighborPointIds.ContainsKey(extraType))
                {
                    continue;
                }

                bool doesExist = false;

                foreach (var extraPointId in m_basePointGrid.Points[baseInstance.EventIndices[0]].NeighborPointIds[extraType])
                {
                    bool extraPointIsInClique = true;

                    foreach (var basePointId in baseInstance.EventIndices)
                    {
                        if (!m_basePointGrid.Points[basePointId].NeighborPointIds.ContainsKey(extraType))
                        {
                            extraPointIsInClique = false;
                            break;
                        }
                        if (!m_basePointGrid.Points[basePointId].NeighborPointIds[extraType].Contains(extraPointId))
                        {
                            extraPointIsInClique = false;
                            break;
                        }
                    }

                    if (extraPointIsInClique)
                    {
                        doesExist = true;

                        int[] newCliqueIds = new int[baseInstance.EventIndices.Length + 1];
                        Array.Copy(baseInstance.EventIndices, newCliqueIds, baseInstance.EventIndices.Length);
                        newCliqueIds[baseInstance.EventIndices.Length] = extraPointId;

                        var instance = new ColocationInstance(Instances.Count, newCliqueIds, m_basePointGrid);
                        Instances.Add(instance);

                        PatternIndex.Add(instance.GridRowIndex, instance.GridColumnIndex, instance.Id);

                        PatternPrefixSum[instance.GridRowIndex + 1, instance.GridColumnIndex + 1]++;

                        _participatingPoints[m_basePointGrid.Points[extraPointId].TypeLabel].Add(extraPointId);
                    }
                }

                if (doesExist)
                {
                    foreach (var basePointId in baseInstance.EventIndices)
                    {
                        _participatingPoints[m_basePointGrid.Points[basePointId].TypeLabel].Add(basePointId);
                    }
                }
            }

            Helper.CalculatePrefixSum(PatternPrefixSum,
                                      m_basePointGrid.RowGridCount + 1,
                                      m_basePointGrid.ColumnGridCount + 1);

            foreach (var type in PatternTypes)
            {
                _participatingPoints[type] = _participatingPoints[type].Distinct().ToList();
                foreach (var pointId in _participatingPoints[type])
                {
                    ParticipationPointPrefixSum[type][m_basePointGrid.Points[pointId].GridRowIndex + 1,
                                                      m_basePointGrid.Points[pointId].GridColumnIndex + 1]++;
                }
                Helper.CalculatePrefixSum(ParticipationPointPrefixSum[type],
                                         m_basePointGrid.RowGridCount + 1,
                                         m_basePointGrid.ColumnGridCount + 1);
            }
        }

        public void GenerateHotspot(PointGrid m_basePointGrid, double m_piThreshold,
                                    ColocationPattern aPattern, ColocationPattern bPattern)
        {
            int checkedNum = 1;
            foreach (var aRowId in aPattern.MBRIndex.Keys)
            {
                foreach (var aColumnId in aPattern.MBRIndex[aRowId].Keys)
                {
                    var aMBRIds = aPattern.MBRIndex[aRowId][aColumnId];
                    var bRowMin = aRowId - 1 >= 0 ? aRowId - 1 : 0;
                    var bRowMax = aRowId + 1 < m_basePointGrid.RowGridCount ? aRowId + 1 : m_basePointGrid.RowGridCount - 1;
                    var bColumnMin = aColumnId - 1 >= 0 ? aColumnId - 1 : 0;
                    var bColumnMax = aColumnId + 1 < m_basePointGrid.ColumnGridCount ? aColumnId + 1 : m_basePointGrid.ColumnGridCount - 1;

                    foreach (var bRowId in bPattern.MBRIndex.Keys.Where((row) => (bRowMin <= row && row <= bRowMax)))
                    {
                        foreach (var bColumnId in bPattern.MBRIndex[bRowId].Keys.Where((column) => (bColumnMin <= column && column <= bColumnMax)))
                        {
                            var bMBRIds = bPattern.MBRIndex[bRowId][bColumnId];

                            foreach (var aMBRId in aMBRIds)
                            {
                                foreach (var bMBRId in bMBRIds)
                                {
                                    if (aPattern.MBR[aMBRId].MaxRowIndex > bPattern.MBR[bMBRId].MaxRowIndex + 1 ||
                                        aPattern.MBR[aMBRId].MaxRowIndex < bPattern.MBR[bMBRId].MaxRowIndex - 1 ||
                                        aPattern.MBR[aMBRId].MaxColumnIndex > bPattern.MBR[bMBRId].MaxColumnIndex + 1 ||
                                        aPattern.MBR[aMBRId].MaxColumnIndex < bPattern.MBR[bMBRId].MaxColumnIndex - 1)
                                    {
                                        continue;
                                    }

                                    int minZoneRowIdx = Math.Min(aRowId, bRowId);
                                    int minZoneColumnIdx = Math.Min(aColumnId, bColumnId);
                                    int maxZoneRowIdx = Math.Max(aPattern.MBR[aMBRId].MaxRowIndex, bPattern.MBR[bMBRId].MaxRowIndex);
                                    int maxZoneColumnIdx = Math.Max(aPattern.MBR[aMBRId].MaxColumnIndex, bPattern.MBR[bMBRId].MaxColumnIndex);

                                    List<ActiveCell> activeCellList = new List<ActiveCell>();
                                    foreach (var cellRowIdx in PatternIndex.Keys)
                                    {
                                        foreach (var cellColumnIdx in PatternIndex[cellRowIdx].Keys)
                                        {
                                            if (cellRowIdx >= minZoneRowIdx && cellRowIdx <= maxZoneRowIdx &&
                                                cellColumnIdx >= minZoneColumnIdx && cellColumnIdx <= maxZoneColumnIdx)
                                            {
                                                activeCellList.Add(new ActiveCell(activeCellList.Count, cellRowIdx, cellColumnIdx));
                                            }
                                        }
                                    }

                                    _SearchPossibleMBR(activeCellList, m_basePointGrid, m_piThreshold, checkedNum);
                                    checkedNum++;
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
