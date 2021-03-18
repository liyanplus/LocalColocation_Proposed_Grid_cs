using System;
namespace HelpLib
{
    public static class Helper
    {
        #region Method
        public static void CalculatePrefixSum(int[,] m_matrix, int m_rowNumber, int m_columnNumber)
        {
            for (int i = 1; i < m_rowNumber; i++)
            {
                for (int j = 0; j < m_columnNumber; j++)
                {
                    m_matrix[i, j] += m_matrix[i - 1, j];
                }
            }

            for (int i = 1; i < m_columnNumber; i++)
            {
                for (int j = 0; j < m_rowNumber; j++)
                {
                    m_matrix[j, i] += m_matrix[j, i - 1];
                }
            }
        }

        public static int GetCountInRangeFromPrefixSum(int[,] m_matrix,
                                                       int m_minRowIndex,
                                                       int m_minColumnIndex,
                                                       int m_maxRowIndex,
                                                       int m_maxColumnIndex)
        {
            if (m_minRowIndex > m_maxRowIndex || m_minColumnIndex > m_maxColumnIndex)
            {
                return 0;
            }
            return m_matrix[m_maxRowIndex + 1, m_maxColumnIndex + 1] - m_matrix[m_maxRowIndex + 1, m_minColumnIndex]
                - m_matrix[m_minRowIndex, m_maxColumnIndex + 1] + m_matrix[m_minRowIndex, m_minColumnIndex];
        }
        #endregion
    }
}
