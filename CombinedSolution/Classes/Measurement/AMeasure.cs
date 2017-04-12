using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace CombinedSolution.Classes.Measurement
{
    
    
    public abstract class AMeasure
    {
        protected DateTime _creation;
        protected DateTime _completion;

        public AMeasure()
        {
            _creation = DateTime.Now;
            _completion = DateTime.MinValue;
        }

        public TimeSpan LifeTime()
        {
            TimeSpan retVal = TimeSpan.Zero;
            if (_completion != DateTime.MinValue)
            {
                retVal = _completion.Subtract(_creation);
            }
            return retVal; 
        }
    }

    public class TaskMeasure : AMeasure
    {
        private TimeSpan _timeWaiting;
        public TaskMeasure()
            : base()
        {
            _timeWaiting = TimeSpan.Zero;
        }
        public void Complete()
        {
            _completion = DateTime.Now;
        }
        public void ExecutionStarted()
        {
            _timeWaiting = DateTime.Now - _creation;
        }
        public TimeSpan GetTimeWaiting()
        {
            return _timeWaiting;
        }
        public TimeSpan GetTimeExecuting()
        {
            return LifeTime().Subtract(_timeWaiting);
        }
        public override string ToString()
        {
            string retVal = "";
            retVal = "LifeTime:- " + LifeTime().TotalMilliseconds.ToString() + "\n" +
                     "Time Waiting:- " + _timeWaiting.TotalMilliseconds.ToString() + "\n" +
                     "Time Executing:- " + GetTimeExecuting().TotalMilliseconds.ToString() + "\n";
            return retVal;
        }
    }

    public class ThreadMeasure : AMeasure
    {
        private int _throughput;
        private TimeSpan _timeNotExecuting;
        private DateTime _timeOfLastTask;
        private int _successfulSteals;
        private int _stealAttempts;
        private bool _isSteal;
        public ThreadMeasure(bool isSteal)
            : base()
        {
            if (isSteal)
            {
                _isSteal = true;
                _stealAttempts = 0;
            }
            _throughput = 0;
            _timeNotExecuting = TimeSpan.Zero;
            _timeOfLastTask = DateTime.Now;
        }
        public void Complete()
        {
            DateTime t = DateTime.Now;
            _timeNotExecuting += t.Subtract(_timeOfLastTask);
            _completion = t;
        }
        public void StealAttempt()
        {
            if (_isSteal)
            {
                ++_stealAttempts;
            }
        }
        public void SuccessfulSteal()
        {
            if (_isSteal)
            {
                ++_successfulSteals;
            }
        }
        public void IncrementThroughput()
        {
            ++_throughput;
        }
        public int GetThroughput()
        {
            return _throughput;
        }
        public void StartedTask()
        {

            TimeSpan t = DateTime.Now.Subtract(_timeOfLastTask);
            _timeNotExecuting = _timeNotExecuting.Add(t);
        }
        public void FinishedTask()
        {
            _timeOfLastTask = DateTime.Now;
        }
        public TimeSpan GetWastedTime()
        {
            return _timeNotExecuting;
        }
        public TimeSpan GetAverageExecutionTime()
        {
            TimeSpan retVal = TimeSpan.Zero;
            TimeSpan usefultime = (LifeTime().Subtract(_timeNotExecuting));
            if (_throughput > 0)
            {
                double avg = usefultime.TotalMilliseconds / _throughput;
                retVal = TimeSpan.FromMilliseconds(avg);
            }
            return retVal;
        }
        public override string ToString()
        {
            string retVal = "";
            retVal = "LifeTime:- " + LifeTime().TotalMilliseconds.ToString() + "\r\n" +
                     "Throughput:- " + GetThroughput().ToString() + "\r\n" +
                     "Time Not Executing Tasks:- " + GetWastedTime().TotalMilliseconds.ToString() + "\r\n" +
                     "Average Task Execution Time:- " + GetAverageExecutionTime().TotalMilliseconds.ToString() + "\r\n";
            if (_isSteal)
            {
                retVal += "Steal Attempts Made:- " + _stealAttempts.ToString() + "\r\n" +
                          "Successful Steals:- " + _successfulSteals.ToString() + "\r\n";
            }
            return retVal;
        }
    }


}
