using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CombinedSolution.Classes;
using System.Threading;
namespace UnitTests
{
    [TestClass]
    public class SchedulerTests
    {
        Scheduler sched;
        [TestMethod]
        public void SingletonTest()
        {
            sched = Scheduler.Instance;
            Scheduler sched2 = Scheduler.Instance;
            Assert.AreSame(sched, sched2);
        }
        [TestMethod]
        public void StandardGetMax()
        {
            sched = Scheduler.Instance;
            int retVal = sched.MaxThreads;
            Assert.AreEqual(8, retVal);          
        }
        [TestMethod]
        public void StandardSetMax()
        {
            sched = Scheduler.Instance;
            int value = 10;
            sched.MaxThreads = value;
            int retVal = sched.MaxThreads;
            sched.MaxThreads = 8;
            Assert.AreEqual(value, retVal);
        }
        [TestMethod]
        public void ZeroSetMax()
        {
            sched = Scheduler.Instance;
            int value = 0;
            sched.MaxThreads = value;
            int retVal = sched.MaxThreads;
            Assert.AreEqual(8, retVal);
        }
        [TestMethod]
        public void ExcessiveSetMax()
        {
            sched = Scheduler.Instance;
            int value = 100;
            sched.MaxThreads = value;
            int retVal = sched.MaxThreads;
            Assert.AreNotEqual(value, retVal);
        }
        [TestMethod]
        public void NotStartedThreadCount()
        {
            sched = Scheduler.Instance;
            int value = 0;
            int retVal = sched.ThreadCount;
            Assert.AreEqual(value, retVal);
        }
        [TestMethod]
        public void StandardThreadCount()
        {
            sched = Scheduler.Instance;
            int value = sched.MaxThreads;
            sched.AddNewTasks(getTasks(10));
            sched.Start();
            int retVal = sched.ThreadCount;
            sched.Close();
            Assert.AreEqual(value, retVal);
        }
        [TestMethod]
        public void StandardGetMech()
        {
            sched = Scheduler.Instance;
            SchedulingMechanism m = sched.Mechanism;
            Assert.AreEqual(SchedulingMechanism.WorkSharing, m);
        }
        [TestMethod]
        public void StealingGetMech()
        {
            sched = Scheduler.Instance;
            sched.Mechanism = SchedulingMechanism.WorkStealing;
            SchedulingMechanism m = sched.Mechanism;
            Assert.AreEqual(SchedulingMechanism.WorkStealing, m);
            sched.Mechanism = SchedulingMechanism.WorkSharing; //Reset
        }
        [TestMethod]
        public void StandardSetMech()
        {
            sched = Scheduler.Instance;
            sched.Mechanism = SchedulingMechanism.WorkStealing;
            SchedulingMechanism m = sched.Mechanism;
            Assert.AreEqual(SchedulingMechanism.WorkStealing, m);
            sched.Mechanism = SchedulingMechanism.WorkSharing; //Reset
        }
        [TestMethod]
        public void StandardSetMech2()
        {
            sched = Scheduler.Instance;
            sched.Mechanism = SchedulingMechanism.WorkStealing;
            sched.Mechanism = SchedulingMechanism.WorkSharing;
            SchedulingMechanism m = sched.Mechanism;
            Assert.AreEqual(SchedulingMechanism.WorkSharing, m);
        }
        [TestMethod]
        public void SameSetMech()
        {
            sched = Scheduler.Instance;
            sched.Mechanism = SchedulingMechanism.WorkSharing;
            SchedulingMechanism m = sched.Mechanism;
            Assert.AreEqual(SchedulingMechanism.WorkSharing, m);
        }
        [TestMethod]
        public void StartedSetMech()
        {
            sched = Scheduler.Instance;
            sched.Start();
            sched.Mechanism = SchedulingMechanism.WorkStealing;
            SchedulingMechanism m = sched.Mechanism;
            sched.Close();
            Assert.AreEqual(SchedulingMechanism.WorkSharing, m);
        }
        [TestMethod]
        public void NotStartedGetState()
        {
            sched = Scheduler.Instance;
            SchedulerState retVal = sched.State;
            Assert.AreEqual(SchedulerState.NotStarted, retVal);
        }
        [TestMethod]
        public void StartedGetState()
        {
            sched = Scheduler.Instance;
            sched.Start();
            SchedulerState retVal = sched.State;
            sched.Close();
            Assert.AreEqual(SchedulerState.Running, retVal);
        }
        [TestMethod]
        public void ClosingGetState()
        {
            sched = Scheduler.Instance;
            sched.Start();
            sched.Close();
            SchedulerState retVal = sched.State;
            Assert.AreEqual(SchedulerState.Closing, retVal);
        }
        [TestMethod]
        public void AbortedGetState()
        {
            sched = Scheduler.Instance;
            sched.Start();
            sched.Abort();
            SchedulerState retVal = sched.State;
            Assert.AreEqual(SchedulerState.Aborted, retVal);
        }
        [TestMethod]
        public void StandardClose()
        {
            ATask t = new Task(() => {Console.Beep();});
            sched = Scheduler.Instance;
            sched.Start();
            sched.Close();
            bool retVal = sched.AddNewTask(t);
            Assert.IsFalse(retVal);
        }
        [TestMethod]
        public void StandardClose2()
        {
            ATask t = new Task(() => { Console.Beep(); });
            sched = Scheduler.Instance;
            sched.Close();
            bool retVal = sched.AddNewTask(t);
            Assert.IsFalse(retVal);
        }
        [TestMethod]
        public void StandardAbort()
        {
            ATask t = new Task(() => { Console.Beep(); });
            sched = Scheduler.Instance;
            sched.Start();
            sched.Abort();
            bool retVal = sched.AddNewTask(t);
            if (!retVal)
            {
                Assert.AreEqual(SchedulerState.Aborted,sched.State);
            }
            else
            {
                Assert.Fail();
            }
        }
        [TestMethod]
        public void StandardAbort2()
        {
            ATask t = new Task(() => { Console.Beep(); });
            sched = Scheduler.Instance;
            sched.Mechanism = SchedulingMechanism.WorkStealing;
            sched.AddNewTasks(getTasks(10));
            sched.Start();
            sched.Abort();
            Assert.AreEqual(SchedulerState.Aborted, sched.State);
        }
        [TestMethod]
        public void NotStartedSharingAdd()
        {
            ATask t = new Task(() => { Console.Beep(); });
            sched = Scheduler.Instance;
            bool retVal = sched.AddNewTask(t);
            Assert.IsTrue(retVal);
        }
        [TestMethod]
        public void NotStartedStealingAdd()
        {
            ATask t = new Task(() => { Console.Beep(); });
            sched = Scheduler.Instance;
            sched.Mechanism = SchedulingMechanism.WorkStealing;
            bool retVal = sched.AddNewTask(t);
            Assert.IsTrue(retVal);
        }
        [TestMethod]
        public void StartedSharingAdd()
        {
            ATask t = new Task(() => { Console.Beep(); });
            sched = Scheduler.Instance;
            bool retVal = sched.AddNewTask(t);
            if (retVal)
            {
                sched.Start();
                t = new Task(() => { Console.Beep(); });
                retVal = sched.AddNewTask(t);
                Assert.IsTrue(retVal);
            }
            else
            {
                Assert.Fail();
            }
        }
        [TestMethod]
        public void StartedStealingAdd()
        {
            ATask t = new Task(() => { Console.Beep(); });
            sched = Scheduler.Instance;
            sched.Mechanism = SchedulingMechanism.WorkStealing;
            int rVal = sched.AddNewTasks(getTasks(10));
            if (rVal == -1)
            {
                sched.Start();
                t = new Task(() => { Console.Beep(); });
                bool retVal = sched.AddNewTask(t);
                Assert.IsTrue(retVal);

            }
            else
            {
                Assert.Fail();
            }
            sched.Abort();
        }
        [TestMethod]
        public void NullTaskAdd()
        {
            sched = Scheduler.Instance;
            bool retVal = sched.AddNewTask(null);
            Assert.IsFalse(retVal);
        }
        [TestMethod]
        public void InvalidTaskAdd()
        {
            ATask t = new Task(() => { Console.Beep(); });
            t.State = TaskState.Aborted;
            sched = Scheduler.Instance;
            bool retVal = sched.AddNewTask(t);
            Assert.IsFalse(retVal);
        }
        [TestMethod]
        public void StandardAdds()
        {
            sched = Scheduler.Instance;
            int rVal = sched.AddNewTasks(getTasks(10));
            Assert.AreEqual(-1, rVal);
        }
        [TestMethod]
        public void EmptyListAdds()
        {
            sched = Scheduler.Instance;
            ATask[] empty = new ATask[10];
            int rVal = sched.AddNewTasks(empty);
            Assert.AreEqual(0, rVal);
        }
        [TestMethod]
        public void InvalidTaskAdds()
        {
            sched = Scheduler.Instance;
            ATask[] empty = getTasks(10);
            empty[9].State = TaskState.Complete;
            int rVal = sched.AddNewTasks(empty);
            Assert.AreEqual(9, rVal);
        }
        private ATask[] getTasks(int count)
        {
            ATask[] taskList = new ATask[count];
            Task t;
            TaskDelegate td = () => { Thread.Sleep(1000); };
            for (int i = 0; i < count; ++i)
            {
                t = new Task(td);
                taskList[i] = t;
            }
            return taskList;
        }
    }
}
