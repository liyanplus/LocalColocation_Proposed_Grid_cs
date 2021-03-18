using System;
namespace ColocationModels
{
    public class ActiveCell
    {
        #region Property
        protected int _id;
        public int Id
        {
            get { return _id; }
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
        #region Constructor
        public ActiveCell()
        {
        }
        public ActiveCell(int m_id, int m_rowIdx, int m_columnIdx)
        {
            _id = m_id;
            _gridRowIndex = m_rowIdx;
            _gridColumnIndex = m_columnIdx;
        }
        #endregion

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(ActiveCell))
            {
                return false;
            }
            else
            {
                return ((ActiveCell)obj).Id == this.Id ? true : false;
            }
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
