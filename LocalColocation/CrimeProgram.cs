using System;
using ColocationModels;

namespace LocalColocation
{
    public static class CrimeProgram
    {
        /// <summary>
        /// Program for Chicago crime data.
        /// </summary>
        /// <param name="args">Arguments from terminal.</param>
        public static void ProgramProc(string[] args)
        {
            // args[0]: input crime csv file path
            // args[1]: output result file path
            // args[2]: neighborhood relationship distance
            // args[3]: grid cell edge length
            // args[4]: threshold for Participation Index

            string inFilePath = args[0];
            string outFilePath = args[1];
            double neightborThreshold = Convert.ToDouble(args[2]);
            double gridSize = Convert.ToDouble(args[3]);
            double piThreshold = Convert.ToDouble(args[4]);

            if (gridSize < neightborThreshold)
            {
                Console.WriteLine("Grid size and neighbor threshold do not match!");
                return;
            }

            var primaryPointGrid = new PointGrid(gridSize);
            ParseCrimeData.Parse(inFilePath, primaryPointGrid, neightborThreshold);
            var startTime = DateTime.Now;
            var primaryMiner = new ColocationMiner(primaryPointGrid, piThreshold);
            var endTime = DateTime.Now;
            string timerStr = string.Format("Time: {0} \t {1}min", endTime - startTime, (endTime - startTime).TotalMinutes);
            primaryMiner.WriteToFile(outFilePath, args, timerStr);
        }
    }
}
