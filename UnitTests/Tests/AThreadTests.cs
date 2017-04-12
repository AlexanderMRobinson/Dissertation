using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CombinedSolution.Classes;
namespace UnitTests
{
    [TestClass]
    public class AThreadTests
    {
        Scheduler sched = Scheduler.Instance;
        [TestMethod]
        public void StandardStart()
        {
            AThread temp = new WorkSharingThread(sched,0);
            bool retVal = temp.Start();
            temp.Abort();
            Assert.IsTrue(retVal);
        }
        [TestMethod]
        public void StartAlreadyStarted()
        {
            AThread temp = new WorkSharingThread(sched,0);
            bool retVal = temp.Start();
            if (retVal)
            {
                retVal = temp.Start();
                Assert.IsFalse(retVal);
            }
            else
            {
                Assert.Fail();
            }
        }
        [TestMethod]
        public void StartNotReady()
        {
            AThread temp = new WorkStealingThread(sched,0);
            bool retVal = temp.Start();
            Assert.IsFalse(retVal);
        }
        [TestMethod]
        public void StandardClose()
        {
            AThread temp = new WorkSharingThread(sched,0);
            bool retVal = temp.Start();
            if (retVal)
            {
                temp.Close();
                retVal = temp.IsClosing;
                Assert.IsTrue(retVal);
            }
            else
            {
                Assert.Fail();
            }
        }
        [TestMethod]
        public void StandardAbort()
        {
            AThread temp = new WorkSharingThread(sched,0);
            bool retVal = temp.Start();
            if (retVal)
            {
                temp.Abort();
                retVal = temp.IsAborted;
                Assert.IsTrue(retVal);
            }
            else
            {
                Assert.Fail();
            }
        }
        [TestMethod]
        public void IsReadyGetVal()
        {
            AThread temp = new WorkStealingThread(sched,0);
            bool retVal = temp.IsReady;
            Assert.IsFalse(retVal);
        }
        [TestMethod]
        public void IsReadySetVal()
        {
            AThread temp = new WorkStealingThread(sched,0);
            temp.IsReady = true;
            bool retVal = temp.IsReady;
            Assert.IsTrue(retVal);
        }
        [TestMethod]
        public void IsStartedGetVal()
        {
            AThread temp = new WorkStealingThread(sched,0);
            bool retVal = temp.IsStarted;
            Assert.IsFalse(retVal);
        }
        [TestMethod]
        public void IsStartedSetVal()
        {
            AThread temp = new WorkSharingThread(sched,0);
            temp.Start();
            bool retVal = temp.IsStarted;
            temp.Abort();
            Assert.IsTrue(retVal);
        }
        [TestMethod]
        public void IsClosingGetVal()
        {
            AThread temp = new WorkStealingThread(sched,0);
            bool retVal = temp.IsClosing;
            Assert.IsFalse(retVal);
        }
        [TestMethod]
        public void IsClosingSetVal()
        {
            AThread temp = new WorkSharingThread(sched,0);
            temp.Start();
            bool retVal = temp.IsStarted;
            if (retVal)
            {
                temp.Close();
                retVal = temp.IsClosing;
                Assert.IsTrue(retVal);
            }
            else
            {
                Assert.Fail();
            }
        }
        [TestMethod]
        public void IsAbortedGetVal()
        {
            AThread temp = new WorkStealingThread(sched,0);
            bool retVal = temp.IsAborted;
            Assert.IsFalse(retVal);
        }
        [TestMethod]
        public void IsAbortedSetVal()
        {
            AThread temp = new WorkSharingThread(sched,0);
            temp.Start();
            bool retVal = temp.IsStarted;
            if (retVal)
            {
                temp.Abort();
                retVal = temp.IsAborted;
                Assert.IsTrue(retVal);
            }
            else
            {
                Assert.Fail();
            }
        }
        [TestMethod]
        public void StandardGetScheduler()
        {
            AThread temp = new WorkSharingThread(sched,0);
            Scheduler s = temp.GetScheduler;
            Assert.AreEqual(sched, s);
        }
        [TestMethod]
        public void StandardGetID()
        {
            AThread temp = new WorkSharingThread(sched,0);
            Guid id = temp.ID;
            if (id != Guid.Empty)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail();
            }
        }
        [TestMethod]
        public void StandardCleanUp()
        {
            AThread temp = new WorkSharingThread(sched,0);
            temp.abstractCleanUp();
            Guid id = temp.ID;
            if (id == Guid.Empty)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}
