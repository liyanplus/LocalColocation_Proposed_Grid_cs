using System;
namespace ColocationModels
{
	public abstract class PointBase
	{
		#region Property
		protected int _id;
		public int Id
		{
			get { return _id; }
		}

		protected double _xCoordinate;
		public double XCoordinate
		{
			get { return _xCoordinate; }
		}

		protected double _yCoorindate;
		public double YCoordinate
		{
			get { return _yCoorindate; }
		}

		protected int _gridColumnIndex;
		public int GridColumnIndex
		{
			get { return _gridColumnIndex; }
		}

		protected int _gridRowIndex;
		public int GridRowIndex
		{
			get { return _gridRowIndex; }
		}
		#endregion

		#region Method
		public void GenerateGridIndex(double m_gridSize, double m_gridXMin, double m_gridYMin)
		{
			_gridColumnIndex = Convert.ToInt32(Math.Floor((XCoordinate - m_gridXMin) / m_gridSize));
			_gridRowIndex = Convert.ToInt32(Math.Floor((YCoordinate - m_gridYMin) / m_gridSize));
		}

		public double DistanceTo(PointBase m_another)
		{
			return Math.Sqrt((XCoordinate - m_another.XCoordinate) * (XCoordinate - m_another.XCoordinate) +
							 (YCoordinate - m_another.YCoordinate) * (YCoordinate - m_another.YCoordinate));
		}

		static public double DistanceBetween(PointBase m_pointA, PointBase m_pointB)
		{
			return m_pointA.DistanceTo(m_pointB);
		}
		#endregion
	}
}
