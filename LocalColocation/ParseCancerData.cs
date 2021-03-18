using System;
using System.IO;
using CsvHelper;
using ColocationModels;
using HelpLib;
using System.Linq;

namespace LocalColocation
{
    public static class ParseCancerData
    {
        #region Method
        public static void Parse(string m_cancerFile, string m_screenCenterFile, PointGrid m_pointGrid)
        {
            _ParseInputCancerFile(m_cancerFile, m_pointGrid);
            _ParseInputScreenCenterFile(m_screenCenterFile, m_pointGrid);
            _AssignPointToGrid(m_pointGrid);
            _BuildNeighborGraph(m_pointGrid);
        }
        private static void _ParseInputCancerFile(string m_fileName, PointGrid m_pointGrid)
        {
            using (TextReader reader = File.OpenText(m_fileName))
            {
                var csv = new CsvReader(reader);
                int id = m_pointGrid.Points.Count;
                while (csv.Read())
                {
                    double xCoordinate = csv.GetField<double>(0);
                    if (xCoordinate > m_pointGrid.XMax)
                    {
                        m_pointGrid.XMax = xCoordinate;
                    }
                    if (xCoordinate < m_pointGrid.XMin)
                    {
                        m_pointGrid.XMin = xCoordinate;
                    }

                    double yCoordinate = csv.GetField<double>(1);
                    if (yCoordinate > m_pointGrid.YMax)
                    {
                        m_pointGrid.YMax = yCoordinate;
                    }
                    if (yCoordinate < m_pointGrid.YMin)
                    {
                        m_pointGrid.YMin = yCoordinate;
                    }

                    string type = csv.GetField<string>(2);
                    if (!m_pointGrid.PointIndex.ContainsKey(type))
                    {
                        m_pointGrid.PointIndex.Add(type, new TwoDimensionalDictionary<int>());
                    }
                    m_pointGrid.Points.Add(new PointEvent(id, xCoordinate, yCoordinate, type));
                    id++;
                }
            }
        }

        private static void _ParseInputScreenCenterFile(string m_fileName, PointGrid m_pointGrid)
        {
            using (TextReader reader = File.OpenText(m_fileName))
            {
                var csv = new CsvReader(reader);
                int id = m_pointGrid.Points.Count;
                m_pointGrid.PointIndex.Add("ScreenCenter", new TwoDimensionalDictionary<int>());
                while (csv.Read())
                {
                    double xCoordinate = csv.GetField<double>(0);
                    if (xCoordinate > m_pointGrid.XMax)
                    {
                        m_pointGrid.XMax = xCoordinate;
                    }
                    if (xCoordinate < m_pointGrid.XMin)
                    {
                        m_pointGrid.XMin = xCoordinate;
                    }

                    double yCoordinate = csv.GetField<double>(1);
                    if (yCoordinate > m_pointGrid.YMax)
                    {
                        m_pointGrid.YMax = yCoordinate;
                    }
                    if (yCoordinate < m_pointGrid.YMin)
                    {
                        m_pointGrid.YMin = yCoordinate;
                    }

                    m_pointGrid.Points.Add(new PointEvent(id, xCoordinate, yCoordinate, "ScreenCenter"));
                    id++;
                }
            }
        }

        private static void _AssignPointToGrid(PointGrid m_pointGrid)
        {
            m_pointGrid.RowGridCount = Convert.ToInt32(Math.Ceiling((m_pointGrid.YMax - m_pointGrid.YMin) / m_pointGrid.GridEdgeLength));
            m_pointGrid.ColumnGridCount = Convert.ToInt32(Math.Ceiling((m_pointGrid.XMax - m_pointGrid.XMin) / m_pointGrid.GridEdgeLength));

            foreach (var type in m_pointGrid.PointIndex.Keys)
            {
                m_pointGrid.PrefixCountMatrices.Add(type, new int[m_pointGrid.RowGridCount + 1, m_pointGrid.ColumnGridCount + 1]);
                //Helper.InitializeMatrix(m_pointGrid.PrefixCountMatrices[type], m_pointGrid.RowGridCount + 1, m_pointGrid.ColumnGridCount + 1);
            }

            foreach (var pt in m_pointGrid.Points)
            {
                pt.GenerateGridIndex(m_pointGrid.GridEdgeLength, m_pointGrid.XMin, m_pointGrid.YMin);
                m_pointGrid.PointIndex[pt.TypeLabel].Add(pt.GridRowIndex, pt.GridColumnIndex, pt.Id);

                m_pointGrid.PrefixCountMatrices[pt.TypeLabel][pt.GridRowIndex + 1, pt.GridColumnIndex + 1]++;
            }

            foreach (var type in m_pointGrid.PointIndex.Keys)
            {
                Helper.CalculatePrefixSum(m_pointGrid.PrefixCountMatrices[type], m_pointGrid.RowGridCount + 1, m_pointGrid.ColumnGridCount + 1);
            }
        }

        private static void _BuildNeighborGraph(PointGrid m_pointGrid)
        {
            foreach (var pt in m_pointGrid.Points)
            {
                int neighborRowMin = pt.GridRowIndex - 1 >= 0 ? pt.GridRowIndex - 1 : 0;
                int neighborRowMax = pt.GridRowIndex + 1 < m_pointGrid.RowGridCount ? pt.GridRowIndex + 1 : m_pointGrid.RowGridCount - 1;
                int neighborColumnMin = pt.GridColumnIndex - 1 >= 0 ? pt.GridColumnIndex - 1 : 0;
                int neighborColumnMax = pt.GridColumnIndex + 1 < m_pointGrid.ColumnGridCount ? pt.GridColumnIndex + 1 : m_pointGrid.ColumnGridCount - 1;

                var otherTypes = m_pointGrid.PointIndex.Keys.Where((eventType) => !string.Equals(eventType, pt.TypeLabel));
                foreach (var gridType in otherTypes)
                {
                    var grid = m_pointGrid.PointIndex[gridType];
                    var neighborRows = grid.Keys.Where(
                        (row) => row >= neighborRowMin && row < neighborRowMax
                    );
                    foreach (var gridRow in neighborRows)
                    {
                        var neighborColumns = grid[gridRow].Keys.Where(
                            (column) => column >= neighborColumnMin && column < neighborColumnMax
                        );
                        foreach (var gridColumn in neighborColumns)
                        {
                            foreach (var anotherPointId in grid[gridRow][gridColumn])
                            {
                                if (pt.Id < anotherPointId && pt.DistanceTo(m_pointGrid.Points[anotherPointId]) < m_pointGrid.GridEdgeLength)
                                {
                                    pt.AddNeighbor(m_pointGrid.Points[anotherPointId]);
                                    m_pointGrid.Points[anotherPointId].AddNeighbor(pt);
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
