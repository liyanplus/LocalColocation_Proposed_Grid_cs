using System;
using System.Collections.Generic;

namespace ColocationModels
{
	public class ColocationInstance : PointBase
	{
		#region Property
		private int[] _eventIndices;
		public int[] EventIndices
		{
			get { return _eventIndices; }
		}
		#endregion
		#region Constructor
		public ColocationInstance()
		{
		}
		public ColocationInstance(int m_id, int[] m_eventIndices, PointGrid m_basePointGrid)
		{
			_id = m_id;

			_eventIndices = m_eventIndices;
			_xCoordinate = 0;
			_yCoorindate = 0;
			foreach (var pointIndex in m_eventIndices)
			{
				_xCoordinate += m_basePointGrid.Points[pointIndex].XCoordinate;
				_yCoorindate += m_basePointGrid.Points[pointIndex].YCoordinate;
			}
			_xCoordinate /= m_eventIndices.Length;
			_yCoorindate /= m_eventIndices.Length;

			GenerateGridIndex(m_basePointGrid.GridEdgeLength, m_basePointGrid.XMin, m_basePointGrid.YMin);
		}
		#endregion
	}
}
