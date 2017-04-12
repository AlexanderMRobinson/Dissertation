using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CombinedSolution.Classes;
namespace TBPVsImp
{
    class Program
    {
        static void Main(string[] args)
        {
            int count = 0;
            Int32.TryParse(args[0], out count);
            int x, y, hw;
            string txt = "";
            
            if (args.Contains("-10"))
            {
                x = 10;
                y = 10;
                hw = 100;
                txt = "10x10.txt";
            }
            else if (args.Contains("-20"))
            {
                x = 20;
                y = 20;
                hw = 50;
                txt = "20x20.txt";
            }
            else if (args.Contains("-30"))
            {
                x = 30;
                y = 30;
                hw = 33;
                txt = "30x30.txt";
            }
            else if (args.Contains("-40"))
            {
                x = 40;
                y = 40;
                hw = 25;
                txt = "40x40.txt";
            }
            else if (args.Contains("-50"))
            {
                x = 50;
                y = 50;
                hw = 20;
                txt = "50x50.txt";
            }
            else if (args.Contains("-60"))
            {
                x = 60;
                y = 60;
                hw = 16;
                txt = "60x60.txt";
            }
            else if (args.Contains("-70"))
            {
                x = 70;
                y = 70;
                hw = 14;
                txt = "70x70.txt";
            }
            else if (args.Contains("-80"))
            {
                x = 80;
                y = 80;
                hw = 12;
                txt = "80x80.txt";
            }
            else if (args.Contains("-90"))
            {
                x = 90;
                y = 90;
                hw = 11;
                txt = "90x90.txt";
            }
            else
            {
                x = 100;
                y = 100;
                hw = 10;
                txt = "100x100.txt";
            }

            CombinedSolution.Classes.Experiment.Grid[] gArray = new CombinedSolution.Classes.Experiment.Grid[count];
            for (int i = 0; i < count; ++i)
            {
                gArray[i] = new CombinedSolution.Classes.Experiment.Grid(txt, "Images/" + i + ".bmp", x, y, hw);
            }
            if (args.Contains("-i"))
            {
                DateTime t = DateTime.Now;
                foreach (CombinedSolution.Classes.Experiment.Grid g in gArray)
                {
                    g.createGrid();
                    g.LoadGrid();
                    g.DjikSub();
                    g.AStar();
                    g.PrintToFile();
                }
                DateTime ts = DateTime.Now;
                Console.WriteLine(ts.Subtract(t).TotalMilliseconds.ToString());
            }
            else
            {
                if (args.Contains("-s"))
                {
                    Scheduler.Instance.Mechanism = SchedulingMechanism.WorkStealing;
                }
                Task[] tArray = new Task[count * 5];

                for (int i = 0; i < count; ++i)
                {
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
}
