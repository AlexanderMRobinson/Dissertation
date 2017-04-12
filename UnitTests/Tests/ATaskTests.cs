using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CombinedSolution.Classes;
namespace UnitTests
{
    [TestClass]
    public class ATaskTests
    {
        [TestMethod]
        public void StandardReplace()
        {
            TaskDelegate val = () => { int i = 0; };
            ATask task = new Task(() => { Console.WriteLine("HI"); });
            bool retval = task.ReplaceDelegate(val);
            if (retval)
            {
                TaskDelegate t = task.Delegate;
                Assert.AreSame(t, val);
            }
            else
            {
                Assert.Fail();
            }
        }
        [TestMethod]
        public void ReplaceWithNull()
        {
            ATask task = new Task(() => { Console.WriteLine("HI"); });
            bool retval = task.ReplaceDelegate(null);
            Assert.IsFalse(retval);
        }
        [TestMethod]
        public void ReplaceInScheduler()
        {
            TaskDelegate val = () => { int i = 0; };
            ATask task = new Task(() => { Console.WriteLine("HI"); });
            task.enteredScheduler();
            bool retval = task.ReplaceDelegate(val);
            Assert.IsFalse(retval);
        }
        [TestMethod]
        public void AddToEmpty()
        {
            Guid temp = Guid.NewGuid();
            bool outval = false;
            ATask task = new Task(() => { Console.WriteLine("HI"); });
            bool retVal = task.AddDependency(temp);
            if (task.DependencyCount == 1 && retVal)
            {
                outval = true;
            }
            Assert.IsTrue(outval);
        }
        [TestMethod]
        public void StandardAdd()
        {
            Guid temp = Guid.NewGuid();
            ATask task = new Task(() => { Console.WriteLine("HI"); });
            List<Guid> tlist = new List<Guid>();
            for (int i = 0; i < 10; ++i)
            {
                tlist.Add(Guid.NewGuid());
            }
            int rVal = task.AddDependencies(tlist);
            if (rVal != -1)
            {
                Assert.Fail();
            }
            else
            {
                bool outval = false;
                int currentCount = task.DependencyCount;
                bool retVal = task.AddDependency(temp);
                if (retVal)
                {
                    if (task.DependencyCount > currentCount)
                    {
                        outval = true;
                    }
                }
                Assert.IsTrue(outval);
            }
        }
        [TestMethod]
        public void AddEmptyID()
        {
            Guid temp = Guid.Empty;
            ATask task = new Task(() => { Console.WriteLine("HI"); });
            bool retVal = task.AddDependency(temp);
            Assert.IsFalse(retVal);
        }
        [TestMethod]
        public void AddInScheduler()
        {
            Guid temp = Guid.NewGuid();
            ATask task = new Task(() => { Console.WriteLine("HI"); });
            task.enteredScheduler();
            bool retval = task.AddDependency(temp);
            Assert.IsFalse(retval);
        }
        [TestMethod]
        public void StandardAddDependencies()
        {
            ATask task = new Task(() => { Console.WriteLine("HI"); });
            List<Guid> tlist = new List<Guid>();
            for (int i = 0; i < 10; ++i)
            {
                tlist.Add(Guid.NewGuid());
            }
            int rVal = task.AddDependencies(tlist);
            Assert.AreEqual(-1, rVal);
        }
        [TestMethod]
        public void AddInvalid()
        {
            ATask task = new Task(() => { Console.WriteLine("HI"); });
            List<Guid> tlist = new List<Guid>();
            for (int i = 0; i < 10; ++i)
            {
                if (i == 9)
                {
                    tlist.Add(Guid.Empty);
                }
                else
                {
                    tlist.Add(Guid.NewGuid());
                }
            }
            int rVal = task.AddDependencies(tlist);
            Assert.AreEqual(9, rVal);
        }
        [TestMethod]
        public void AddEmptyList()
        {
            ATask task = new Task(() => { Console.WriteLine("HI"); });
            List<Guid> tlist = new List<Guid>();
            int rVal = task.AddDependencies(tlist);
            Assert.AreEqual(0, rVal);
        }
        [TestMethod]
        public void AddNullList()
        {
            ATask task = new Task(() => { Console.WriteLine("HI"); });
            int rVal = task.AddDependencies(null);
            Assert.AreEqual(0, rVal);
        }
        [TestMethod]
        public void StandardRemove()
        {
            Guid tVal = Guid.NewGuid();
            ATask task = new Task(() => { Console.WriteLine("HI"); });
            List<Guid> tlist = new List<Guid>();
            for (int i = 0; i < 10; ++i)
            {
                if (i == 9)
                {
                    tlist.Add(tVal);
                }
                else
                {
                    tlist.Add(Guid.NewGuid());
                }
            }
            int rVal = task.AddDependencies(tlist);
            if (rVal != -1)
            {
                Assert.Fail();
            }
            else
            {
                task.RemoveDependancy(tVal);
                tlist = task.Dependencies;
                CollectionAssert.DoesNotContain(tlist, tVal);
            }
        }
        [TestMethod]
        public void RemoveEmpty()
        {
            ATask task = new Task(() => { Console.WriteLine("HI"); });
            List<Guid> tlist = new List<Guid>();
            for (int i = 0; i < 10; ++i)
            {
                tlist.Add(Guid.NewGuid());
            }
            int rVal = task.AddDependencies(tlist);
            if (rVal != -1)
            {
                Assert.Fail();
            }
            else
            {
                List<Guid> temp = task.Dependencies;
                task.RemoveDependancy(Guid.Empty);
                tlist = task.Dependencies;
                CollectionAssert.AreEquivalent(temp, tlist);
            }
        }
        [TestMethod]
        public void RemoveFromEmpty()
        {
            ATask task = new Task(() => { Console.WriteLine("HI"); });
            List<Guid> tlist = task.Dependencies;
            task.RemoveDependancy(Guid.NewGuid());
            List<Guid> temp = task.Dependencies;
            CollectionAssert.AreEquivalent(temp, tlist);
        }
        [TestMethod]
        public void InvalidRemove()
        {
            ATask task = new Task(() => { Console.WriteLine("HI"); });
            List<Guid> tlist = new List<Guid>();
            for (int i = 0; i < 10; ++i)
            {
                tlist.Add(Guid.NewGuid());
            }
            int rVal = task.AddDependencies(tlist);
            if (rVal != -1)
            {
                Assert.Fail();
            }
            else
            {
                List<Guid> temp = task.Dependencies;
                task.RemoveDependancy(Guid.NewGuid());
                tlist = task.Dependencies;
                CollectionAssert.AreEquivalent(temp, tlist);
            }
        }
        [TestMethod]
        public void StandardEnter()
        {
            TaskDelegate val = () => { int i = 0; };
            ATask task = new Task(() => { Console.WriteLine("HI"); });
            task.enteredScheduler();
            bool retval = task.ReplaceDelegate(val);
            Assert.IsFalse(retval);
        }
        [TestMethod]
        public void StandardGetID()
        {
            ATask task = new Task(() => { Console.WriteLine("HI"); });
            Guid id = task.ID;
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
        public void StandardGetCount()
        {
            int count = 10;
            ATask task = new Task(() => { Console.WriteLine("HI"); });
            List<Guid> tlist = new List<Guid>();
            for (int i = 0; i < count; ++i)
            {
                tlist.Add(Guid.NewGuid());
            }
            int rVal = task.AddDependencies(tlist);
            if (rVal != -1)
            {
                Assert.Fail();
            }
            else
            {
                Assert.AreEqual(count, task.DependencyCount);
            }
        }
        [TestMethod]
        public void EmptyGetCount()
        {
            ATask task = new Task(() => { Console.WriteLine("HI"); });
            Assert.AreEqual(0, task.DependencyCount);
        }
        [TestMethod]
        public void StandardGetList()
        {
            ATask task = new Task(() => { Console.WriteLine("HI"); });
            List<Guid> tlist = new List<Guid>();
            for (int i = 0; i < 10; ++i)
            {
                tlist.Add(Guid.NewGuid());
            }
            int rVal = task.AddDependencies(tlist);
            if (rVal != -1)
            {
                Assert.Fail();
            }
            else
            {
                List<Guid> temp = task.Dependencies;
                CollectionAssert.AreEquivalent(tlist, temp);
            }
        }
        [TestMethod]
        public void EmptyGetList()
        {
            ATask task = new Task(() => { Console.WriteLine("HI"); });
            List<Guid> temp = task.Dependencies;
            Assert.IsNull(temp);
        }
        [TestMethod]
        public void StandardGetDelegate()
        {
            TaskDelegate temp = () => { Console.WriteLine("HI"); };
            ATask task = new Task(temp);
            TaskDelegate retval = task.Delegate;
            Assert.AreSame(temp, retval);
        }
        [TestMethod]
        public void StandardGetState()
        {
            TaskDelegate temp = () => { Console.WriteLine("HI"); };
            ATask task = new Task(temp);
            TaskState rVal = task.State;
            Assert.AreEqual(TaskState.ToBeExecuted, rVal);
        }
        [TestMethod]
        public void SetStateValid()
        {
            TaskDelegate temp = () => { Console.WriteLine("HI"); };
            ATask task = new Task(temp);
            task.State = TaskState.Executing;
            TaskState rVal = task.State;
            Assert.AreEqual(TaskState.Executing, rVal);
        }
        [TestMethod]
        public void SetStateInvalid()
        {
            TaskDelegate temp = () => { Console.WriteLine("HI"); };
            ATask task = new Task(temp);
            task.State = TaskState.Executing;
            task.State = TaskState.ToBeExecuted;
            TaskState rVal = task.State;
            Assert.AreEqual(TaskState.Executing, rVal);
        }
    }

}
