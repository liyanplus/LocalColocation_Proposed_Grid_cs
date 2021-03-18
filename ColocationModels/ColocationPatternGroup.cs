using System;
using System.Collections.Generic;
namespace ColocationModels
{
    public class ColocationPatternGroup
    {
        #region Property
        private Dictionary<string, ColocationPattern> _patterns = new Dictionary<string, ColocationPattern>();
        public Dictionary<string, ColocationPattern> Patterns
        {
            get { return _patterns; }
        }

        #endregion
        #region Constructor
        public ColocationPatternGroup()
        {
        }
        public ColocationPatternGroup(PointGrid m_basePointGrid,
                                      double m_piThreshold)
        {
            var eventTypes = new List<string>(m_basePointGrid.PointIndex.Keys);
            for (int i = 0; i < eventTypes.Count - 1; i++)
            {
                for (int j = i + 1; j < eventTypes.Count; j++)
                {
                    var colocationPattern = new BinaryColocationPattern(new string[] { eventTypes[i], eventTypes[j] },
                                                                        m_basePointGrid,
                                                                        m_piThreshold);
                    if (colocationPattern.MBR.Count > 0)
                    {
                        Patterns.Add(colocationPattern.TypeLabel, colocationPattern);
                    }
                }
            }
        }

        public ColocationPatternGroup(PointGrid m_basePointGrid,
                                      double m_piThreshold,
                                      ColocationPatternGroup m_basePatternGroup
                                     )
        {
            var patternLabels = new List<string>(m_basePatternGroup.Patterns.Keys);

            for (int i = 0; i < patternLabels.Count - 1; i++)
            {
                for (int j = i + 1; j < patternLabels.Count; j++)
                {
                    var colocationPattern = new GeneralColocationPattern(m_basePointGrid,
                                                                         m_basePatternGroup.Patterns[patternLabels[i]],
                                                                         m_basePatternGroup.Patterns[patternLabels[j]],
                                                                         m_piThreshold);
                    if (colocationPattern.TypeNumber == 0)
                    {
                        continue;
                    }

                    if (colocationPattern.MBR.Count > 0)
                    {
                        Patterns.Add(colocationPattern.TypeLabel, colocationPattern);
                    }
                }
            }
        }
        #endregion
    }
}
