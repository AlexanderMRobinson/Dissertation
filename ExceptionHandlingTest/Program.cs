using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CombinedSolution.Classes;
namespace ExceptionHandlingTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int count = 0;
            Int32.TryParse(args[0], out count);
            CombinedSolution.Classes.Experiment.Grid[] gArray = new CombinedSolution.Classes.Experiment.Grid[count];
            Task[] tArray = new Task[count * 5];
            Scheduler sched = Scheduler.Instance;
            String inputFile = "";
            if (args.Contains("-s"))
            {
                sched.Mechanism = SchedulingMechanism.WorkStealing;
            }
            if (args.Contains("-e"))
            {
                inputFile = "bad.txt";
            }
            else
            {
                inputFile = "good.txt";
            }
            for (int i = 0; i < count; ++i)
            {
                gArray[i] = new CombinedSolution.Classes.Experiment.Grid(inputFile, "Images/" + i + ".bmp", 10, 10, 100);
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
