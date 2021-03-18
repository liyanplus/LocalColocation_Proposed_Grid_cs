using System;
using ColocationModels;

namespace LocalColocation
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            if (args.Length != 5)
            {
                Console.WriteLine("Arguments are not correct!");
                return;
            }

            CrimeProgram.ProgramProc(args);
        }
    }
}
