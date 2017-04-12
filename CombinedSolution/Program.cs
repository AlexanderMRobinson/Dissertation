using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CombinedSolution.Classes
{
    class Program
    {
        static void Main(string[] args)
        {
            Measurement.ThreadMeasure t = new Measurement.ThreadMeasure(false);
            t.StartedTask();
            Thread.Sleep(1000);
            t.FinishedTask();
            Thread.Sleep(1000);
            t.StartedTask();
            Thread.Sleep(1000);
            t.FinishedTask();
            Thread.Sleep(1000);
            t.Complete();
            Console.WriteLine(t.ToString());
            
            Console.ReadLine();
            //DependenciesWorkSharing(100);
            //DependenciesWorkStealing(100);
            //ComplexWorkStealing(100);
            //ComplexWorkSharing(100);
            //SimpleWorkSharing(100, () => { Console.WriteLine("Hello"); });
            //SimpleWorkStealing(100, () => { Console.WriteLine("HELLO"); });
            //SubTaskWorkStealing(100);
            //SubTaskWorkSharing(100);
            //Console.ReadLine();
            //Scheduler.Instance.Close();
        }
        public static void SimpleWorkSharing(int taskCount,TaskDelegate del)
        {
            Scheduler scheduler = Scheduler.Instance;
            Task[] taskArray = getTaskArray(taskCount, del);
            int retVal = scheduler.AddNewTasks(taskArray);
            if (retVal == -1)
            {
                scheduler.Start();
            }
            else
            {
                Console.WriteLine("Failed to add tasks.");
            }
        }
        public static void SimpleWorkStealing(int taskCount, TaskDelegate del)
        {
            Scheduler scheduler = Scheduler.Instance;
            scheduler.Mechanism = SchedulingMechanism.WorkStealing;
            Task[] taskArray = getTaskArray(taskCount, del);
            int retVal = scheduler.AddNewTasks(taskArray);
            if (retVal == -1)
            { 
                scheduler.Start();
            }
            else
            {
                Console.WriteLine("Failed to add tasks.");
            }
        }
        public static void ComplexWorkStealing(int count)
        {
            Scheduler scheduler = Scheduler.Instance;
            scheduler.Mechanism = SchedulingMechanism.WorkStealing;
            Message[] mArray = createMessages("Hello", count);
            Task[] tArray = new Task[count];
            for (int i = 0; i < count; ++i)
            {
                tArray[i] = new Task(mArray[i].Decrypt);
            }
            int retVal = scheduler.AddNewTasks(tArray);
            if (retVal == -1)
            {
                scheduler.Start();
            }
            else
            {
                Console.WriteLine("Failed to add tasks.");
            }
        }
        public static void ComplexWorkSharing(int count)
        {
            Scheduler scheduler = Scheduler.Instance;
            Message[] mArray = createMessages("Hello", count);
            Task[] tArray = new Task[count];
            for (int i = 0; i < count; ++i)
            {
                tArray[i] = new Task(mArray[i].Decrypt);
            }
            int retVal = scheduler.AddNewTasks(tArray);
            if (retVal == -1)
            {
                scheduler.Start();
            }
            else
            {
                Console.WriteLine("Failed to add tasks.");
            }
        }
        public static void DependenciesWorkStealing(int count)
        {
            Scheduler scheduler = Scheduler.Instance;
            scheduler.Mechanism = SchedulingMechanism.WorkStealing;
            Message[] mArray = new Message[count];
            Random key = new Random();
            Task[] tArray = new Task[count];
            for (int i = 0; i < count; ++i)
            {
                mArray[i] = new Message("Hello" + i, key.Next(5));
                tArray[i] = new Task(mArray[i].Decrypt);
                if (i % 10 == 0 && i != 0)
                {
                    tArray[i].AddDependency(tArray[i - 1].ID);
                }
            }
            int retVal = scheduler.AddNewTasks(tArray);
            if (retVal == -1)
            {
                scheduler.Start();
            }
            else
            {
                Console.WriteLine("Failed to add tasks.");
            }
        }
        public static void DependenciesWorkSharing(int count)
        {
            Scheduler scheduler = Scheduler.Instance;
            Message[] mArray = new Message[count];
            Random key = new Random();
            Task[] tArray = new Task[count];
            for (int i = 0; i < count; ++i)
            {
                mArray[i] = new Message("Hello" + i, key.Next(5));
                tArray[i] = new Task(mArray[i].Decrypt);
                if (i % 10 == 0 && i != 0)
                {
                    tArray[i].AddDependency(tArray[i - 1].ID);
                }
            }
            int retVal = scheduler.AddNewTasks(tArray);
            if (retVal == -1)
            {
                scheduler.Start();
            }
            else
            {
                Console.WriteLine("Failed to add tasks.");
            }
        }
        public static void SubTaskWorkSharing(int count)
        {
            Scheduler scheduler = Scheduler.Instance;
            Task[] tArray = new Task[count];
            for (int i = 0; i < count; ++i)
            {
                tArray[i] = new Task(T1);
            }
            int retVal = scheduler.AddNewTasks(tArray);
            if (retVal == -1)
            {
                scheduler.Start();
            }
            else
            {
                Console.WriteLine("Failed to add tasks.");
            }
        }
        public static void SubTaskWorkStealing(int count)
        {
            Scheduler scheduler = Scheduler.Instance;
            scheduler.Mechanism = SchedulingMechanism.WorkStealing;
            Task[] tArray = new Task[count];
            for (int i = 0; i < count; ++i)
            {
                tArray[i] = new Task(T1);
            }
            int retVal = scheduler.AddNewTasks(tArray);
            if (retVal == -1)
            {
                scheduler.Start();
            }
            else
            {
                Console.WriteLine("Failed to add tasks.");
            }
        }
        private static void T1()
        {
            Console.WriteLine("THIS IS A");
            Scheduler.Instance.AddNewTask(new Task(T2));
        }
        private static void T2()
        {
            Console.WriteLine("A TEST");
        }
        private static void simpleTask()
        {
            Console.WriteLine("Hello");
        }
        private static Task[] getTaskArray(int count, TaskDelegate del)
        {
            Task[] taskArray = new Task[count];
            for (int i = 0; i < count; ++i)
            {
                taskArray[i] = new Task(del);
            }
            return taskArray;
        }
        private static Message[] createMessages(string inp,int count)
        {
            Random rand = new Random();
            Message[] mArray = new Message[count];
            int key = 0;
            for (int i = 0; i < count; ++i)
            { 
                key = rand.Next(5);
                mArray[i] = new Message(encrypt(inp, key), key);
            }
            return mArray;
        }
        private static Message createMessage(string inp, int key)
        {
            return new Message(encrypt(inp, key), key);
        }
        private static string encrypt(string inp, int key)
        {
            string output = "";
            int len = inp.Length;
            for (int i = 0; i < len; ++i)
            {
                output = output + nextChar(inp[i], key); 
            }
            return output;
        }
        private static char nextChar(char inp, int key)
        {
            int inpVal = (int)inp;
            int diff = inp + key;
            if (diff > 126)
            {
                inpVal = (inpVal - 126) + 32;
            }
            else
            {
                inpVal = diff;
            }
            return (char)inpVal;
        }
    }
}
