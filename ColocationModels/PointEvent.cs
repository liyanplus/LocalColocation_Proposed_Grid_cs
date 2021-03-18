using System;
using System.Collections.Generic;
namespace ColocationModels
{
	public class PointEvent : PointBase
	{
		#region
		private string _typeLabel;
		public string TypeLabel
		{
			get { return _typeLabel; }
		}

		private Dictionary<string, List<int>> _neighborPointIds = new Dictionary<string, List<int>>();
		public Dictionary<string, List<int>> NeighborPointIds
		{
			get { return _neighborPointIds; }
		}

		#endregion
		#region Constructor
		public PointEvent()
		{
		}

		public PointEvent(int m_id, double m_x, double m_y, string m_type)
		{
			_id = m_id;
			_xCoordinate = m_x;
			_yCoorindate = m_y;
			_typeLabel = m_type;
		}
		#endregion

		#region Method
		public void AddNeighbor(PointEvent new_evt)
		{
			if (!NeighborPointIds.ContainsKey(new_evt.TypeLabel))
			{
				NeighborPointIds.Add(new_evt.TypeLabel, new List<int>());
			}
			NeighborPointIds[new_evt.TypeLabel].Add(new_evt.Id);
		}
		#endregion
	}
}
