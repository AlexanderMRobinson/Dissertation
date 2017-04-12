using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CombinedSolution.Classes;

namespace UnitTests
{
    [TestClass]
    public class DequeTests
    {
        [TestMethod]
        public void StandardInj()
        {
            int val = 12;
            int temp = 0;
            Deque<int> sBuild = new Deque<int>();
            sBuild.Inject(val);
            temp = sBuild.Eject();
            CollectionAssert.Equals(val, temp);
        }
        [TestMethod]
        public void ExceedInj()
        {
            int val = 12;
            int temp = 0;
            Deque<int> sBuild = new Deque<int>();
            for (int i = 0; i < 8; ++i)
            {
                sBuild.Inject(i);
            }
            sBuild.Inject(val);
            temp = sBuild.Eject();
            CollectionAssert.Equals(val, temp);
        }
        [TestMethod]
        public void NullInj()
        {
            Deque<string> sBuild = new Deque<string>();
            bool retVal = sBuild.Inject(null);
            CollectionAssert.Equals(false, retVal);
        }
        [TestMethod]
        public void StandardPush()
        {
            int val = 12;
            int temp = 0;
            Deque<int> sBuild = new Deque<int>();
            sBuild.Push(val);
            temp = sBuild.Pop();
            CollectionAssert.Equals(val, temp);
        }
        [TestMethod]
        public void ExceedPush()
        {
            int val = 12;
            int temp = 0;
            Deque<int> sBuild = new Deque<int>();
            for (int i = 0; i < 8; ++i)
            {
                sBuild.Push(i);
            }
            sBuild.Push(val);
            temp = sBuild.Pop();
            CollectionAssert.Equals(val, temp);
        }
        [TestMethod]
        public void NullPush()
        {
            Deque<string> sBuild = new Deque<string>();
            bool retVal = sBuild.Push(null);
            CollectionAssert.Equals(false, retVal);
        }
        [TestMethod]
        public void EmptyEject()
        {
            Deque<int> sBuild = new Deque<int>();
            int val = sBuild.Eject();
            CollectionAssert.Equals(0, val);
        }
        [TestMethod]
        public void StandardEject()
        {
            int val = 7;
            int temp = 0;
            Deque<int> sBuild = new Deque<int>();
            for (int i = 0; i < 8; ++i)
            {
                sBuild.Inject(i);
            }
            temp = sBuild.Eject();
            CollectionAssert.Equals(val, temp);
        }
        [TestMethod]
        public void EmptyPop()
        {
            Deque<int> sBuild = new Deque<int>();
            int val = sBuild.Pop();
            CollectionAssert.Equals(0, val);
        }
        [TestMethod]
        public void StandardPop()
        {
            int val = 7;
            int temp = 0;
            Deque<int> sBuild = new Deque<int>();
            for (int i = 0; i < 8; ++i)
            {
                sBuild.Push(i);
            }
            temp = sBuild.Pop();
            CollectionAssert.Equals(val, temp);
        }
        [TestMethod]
        public void EmptyCount()
        {
            Deque<int> sBuild = new Deque<int>();
            int val = sBuild.Count;
            CollectionAssert.Equals(0, val);
        }
        [TestMethod]
        public void StandardCount()
        {
            Deque<int> sBuild = new Deque<int>();
            for (int i = 0; i < 8; ++i)
            {
                sBuild.Push(i);
            }
            int val = sBuild.Count;
            CollectionAssert.Equals(8, val);
        }

    }
}
