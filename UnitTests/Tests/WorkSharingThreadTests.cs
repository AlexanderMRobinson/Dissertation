using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CombinedSolution.Classes;

namespace UnitTests
{
    [TestClass]
    public class WorkSharingThreadTests
    {
        Scheduler sched = Scheduler.Instance;
        [TestMethod]
        public void StandardGetSleeping()
        {
            WorkSharingThread thread = new WorkSharingThread(sched, 0);
            bool retVal = thread.IsSleeping;
            thread.Start();
            bool retVal2 = thread.IsSleeping;
            thread.Abort();
            Assert.AreEqual(false, retVal);
            Assert.AreEqual(false, retVal2);
        }
        [TestMethod]
        public void StandardAddTask()
        {
            WorkSharingThread thread = new WorkSharingThread(sched, 0);
            Task t = new Task(() => { Console.WriteLine("HI"); });
            bool retVal = false;
            thread.Start();
            while (true)
            {
                if (thread.IsSleeping)
                {
                    retVal = thread.AddNewTask(t);
                    break;
                }
            }
            Assert.AreEqual(true, retVal);
            while (true)
            {
                if (t.State == TaskState.Aborted)
                {
                    retVal = false;
                    thread.Abort();
                    break;
                }
                else if (t.State == TaskState.Complete)
                {
                    retVal = true;
                    thread.Abort();
                    break;
                }
            }
            Assert.AreEqual(true, retVal);
        }
        [TestMethod]
        public void ExecutingAddTask()
        {
            WorkSharingThread thread = new WorkSharingThread(sched, 0);
            Task t = new Task(() => { Console.WriteLine("HI"); });
            bool retVal = false;
            thread.Start();
            while (true)
            {
                if (thread.IsSleeping)
                {
                    retVal = thread.AddNewTask(t);
                    if (retVal)
                    {
                        t = new Task(() => { Console.WriteLine("HI2"); });
                        retVal = thread.AddNewTask(t);
                        Assert.AreEqual(false, retVal);
                    }
                    else
                    {
                        Assert.Fail();
                    }
                    break;
                }
            }
            thread.Abort();
        }
        [TestMethod]
        public void AddNullTask()
        {
            WorkSharingThread thread = new WorkSharingThread(sched, 0);
            thread.Start();
            Task t = null;
            bool retVal = thread.AddNewTask(t);
            Assert.AreEqual(false, retVal);
            thread.Abort();
        }
        [TestMethod]
        public void CompletedAddTask()
        {
            WorkSharingThread thread = new WorkSharingThread(sched, 0);
            thread.Start();
            Task t = new Task(() => { Console.WriteLine("HI"); });
            t.State = TaskState.Complete;
            bool retVal = thread.AddNewTask(t);
            Assert.AreEqual(false, retVal);
            thread.Abort();
        }
        
    }
}
