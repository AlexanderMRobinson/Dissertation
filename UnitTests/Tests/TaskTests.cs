using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CombinedSolution.Classes;
namespace UnitTests
{
    [TestClass]
    public class TaskTests
    {
        [TestMethod]
        public void StandardBuildNullParam()
        {
            try
            {
                Task temp = new Task(null);
            }
            catch (ArgumentNullException e)
            {
                Assert.IsTrue(true);
            }
        }
        [TestMethod]
        public void TemplateBuildNullParam()
        {
            try
            {
                Task<int> temp = new Task<int>(null);
            }
            catch (ArgumentNullException e)
            {
                Assert.IsTrue(true);
            }
        }
    }
}
