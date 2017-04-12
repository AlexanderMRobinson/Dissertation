using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CombinedSolution.Classes;

namespace ExecutionTimeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int count = 0;
            Int32.TryParse(args[0], out count);
            Scheduler sched = Scheduler.Instance;
            int xy = 10;
            int hw = 100;
            string name = "smallGrid.txt";
            if (args.Contains("-b"))
            {
                name = "bigGrid.txt";
                xy = 100;
                hw = 10;
            }

            if (args.Contains("-s"))
            {
                sched.Mechanism = SchedulingMechanism.WorkStealing;
            }

            CombinedSolution.Classes.Experiment.Grid[] gArray = new CombinedSolution.Classes.Experiment.Grid[count];
            Task[] tArray = new Task[count * 5];

            for (int i = 0; i < count; ++i)
            {
                gArray[i] = new CombinedSolution.Classes.Experiment.Grid(name, "Images/" + i + ".bmp", xy, xy, hw);
                tArray[5 * i] = new Task(gArray[i].createGrid);
                tArray[5 * i + 1] = new Task(gArray[i].LoadGrid);
                tArray[5 * i + 1].AddDependency(tArray[5 * i].ID);
                tArray[5 * i + 2] = new Task(gArray[i].AStar);
                tArray[5 * i + 2].AddDependency(tArray[5 * i + 1].ID);
                tArray[5 * i + 3] = new Task(gArray[i].Dijkstra);
                tArray[5 * i + 3].AddDependency(tArray[5 * i + 1].ID);
                tArray[5 * i + 4] = new Task(gArray[i].PrintToFile);
                tArray[5 * i + 4].AddDependency(tArray[5 * i + 3].ID);
                tArray[5 * i + 4].AddDependency(tArray[5 * i + 2].ID);
            }
            Scheduler.Instance.AddNewTasks(tArray);
            Scheduler.Instance.Start();
            Scheduler.Instance.Close();
        }
    }
}
