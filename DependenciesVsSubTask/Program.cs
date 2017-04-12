using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CombinedSolution.Classes;
using System.IO;
namespace DependenciesVsSubTask
{
    class Program
    {
        static void Main(string[] args)
        {
            int count = 0;
            Int32.TryParse(args[0], out count);
            CombinedSolution.Classes.Experiment.Grid[] gArray = new CombinedSolution.Classes.Experiment.Grid[count];
            Task[] tArray = null;
            Scheduler sched = Scheduler.Instance;
            if (args.Contains("-s"))
            {
                sched.Mechanism = SchedulingMechanism.WorkStealing;
            }
            if (args.Contains("-d"))
            {
                tArray = new Task[count * 5];
                for (int i = 0; i < count; ++i)
                {
                    gArray[i] = new CombinedSolution.Classes.Experiment.Grid("input.txt", "Images/" + i + ".bmp", 20, 20, 50);
                    tArray[5 * i] = new Task(gArray[i].createGrid);
                    tArray[5 * i + 1] = new Task(gArray[i].LoadGrid);
                    tArray[5 * i + 1].AddDependency(tArray[5 * i].ID);

                    tArray[5 * i + 2] = new Task(gArray[i].Dijkstra);
                    tArray[5 * i + 2].AddDependency(tArray[5 * i + 1].ID);

                    tArray[5 * i + 3] = new Task(gArray[i].AStar);
                    tArray[5 * i + 3].AddDependency(tArray[5 * i + 2].ID);

                    tArray[5 * i + 4] = new Task(gArray[i].PrintToFile);
                    tArray[5 * i + 4].AddDependency(tArray[5 * i + 3].ID);
                }
                Scheduler.Instance.AddNewTasks(tArray);
                Scheduler.Instance.Start();
                Scheduler.Instance.Close();
            }
            else
            {
                tArray = new Task[count];
                for (int i = 0; i < count; ++i)
                {
                    gArray[i] = new CombinedSolution.Classes.Experiment.Grid("input.txt", "Images/" + i + ".bmp", 20, 20, 50);
                    tArray[i] = new Task(gArray[i].CreateSub);
                }
                Scheduler.Instance.AddNewTasks(tArray);
                Scheduler.Instance.Start();
                int cnt = 0;
                /*
                while (true)
                {
                    cnt = Directory.GetFiles("Images/").Count();
                    if (cnt > 8)
                    {
                        Scheduler.Instance.Close();
                    }
                }
                 * */
            }

        }
    }
}
