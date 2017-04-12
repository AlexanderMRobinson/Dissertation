using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CombinedSolution.Classes;
using System.Collections.Generic;
namespace UnitTests
{
    [TestClass]
    public class WorkStealingThreadTests
    {
        Scheduler sched = Scheduler.Instance;
        [TestMethod]
        public void StandardAddThreads()
        {
            WorkStealingThread thread = new WorkStealingThread(sched, 0);
            WorkStealingThread[] tArray = new WorkStealingThread[1];
            tArray[0] = new WorkStealingThread(sched, 1);
            bool retVal = thread.AddOtherThreads(tArray);
            Assert.IsTrue(retVal);
        }
        [TestMethod]
        public void NullAddThreads()
        {
            WorkStealingThread thread = new WorkStealingThread(sched, 0);
            WorkStealingThread[] tArray = null;
            bool retVal = thread.AddOtherThreads(tArray);
            Assert.IsFalse(retVal);
        }
        [TestMethod]
        public void StandardSteal()
        {
            WorkStealingThread thread = new WorkStealingThread(sched, 0);
            Task t = new Task(() => { Console.WriteLine("HI"); });
            bool retVal = thread.AddNewTask(t);
            if (retVal)
            {
                ATask rVal = thread.Steal();
                Assert.AreSame(t, rVal);
            }
            else
            {
                Assert.Fail();
            }
        }
        [TestMethod]
        public void EmptySteal()
        {
            WorkStealingThread thread = new WorkStealingThread(sched, 0);
            ATask rVal = thread.Steal();
            Assert.IsNull(rVal);
        }
        [TestMethod]
        public void StandardRetrieve()
        {
            WorkStealingThread thread = new WorkStealingThread(sched, 0);
            Task[] tArray = getTasks(20, () => { Console.WriteLine("HELLO"); });
            bool rVal = false;
            for (int i = 0; i < 20; ++i)
            {
                rVal = thread.AddNewTask(tArray[i]);
                if (rVal == false)
                {
                    break;
                }
            }
            if (rVal)
            {
                thread.IsReady = true;
                thread.Start();
                List<Guid> rList = null;
                while (true)
                {
                    rList = thread.RetrieveCompletedTasks();
                    if (rList.Count > 0)
                    {
                        Assert.IsTrue(true);
                        break;
                    }
                }
            }
            else
            {
                Assert.Fail();
            }
        }
        private Task[] getTasks(int numTasks, TaskDelegate td)
        {
            Task[] tArray = new Task[numTasks];
            for (int i = 0; i < numTasks; ++i)
            {
                tArray[i] = new Task(td);
            }
            return tArray;
        }
        [TestMethod]
        public void StandardAddTask()
        {
            WorkStealingThread thread = new WorkStealingThread(sched, 0);
            Task t = new Task(() => { Console.WriteLine("HELLO"); });
            bool retVal = thread.AddNewTask(t);
            Assert.IsTrue(retVal);
        }
        [TestMethod]
        public void NullAddTask()
        {
            WorkStealingThread thread = new WorkStealingThread(sched, 0);
            Task t = null;
            bool retVal = thread.AddNewTask(t);
            Assert.IsFalse(retVal);
        }
        [TestMethod]
        public void InvalidStateAdd()
        {
            WorkStealingThread thread = new WorkStealingThread(sched, 0);
            Task t = new Task(() => { Console.WriteLine("HELLO"); });
            t.State = TaskState.Complete;
            bool retVal = thread.AddNewTask(t);
            Assert.IsFalse(retVal);
        }
    }
}
