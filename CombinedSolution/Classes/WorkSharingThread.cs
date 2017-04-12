using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CombinedSolution.Classes
{
    public class WorkSharingThread : AThread
    {
        private bool _sleep;
        private ATask _currentTask;
        private const int SLEEP_LEN = 1000;

        public WorkSharingThread(Scheduler parent, int location)
            : base(parent, location)
        {
            _measure = new Measurement.ThreadMeasure(false);
            _sleep = false;
            _currentTask = null;
            IsReady = true;
        }

        #region Public Methods
        /// <summary>
        /// Property that allows thread safe access to the _sleep boolean variable.
        /// </summary>
        public bool IsSleeping
        {
            get
            {
                bool retVal = false;
                lock (_locker)
                {
                    retVal = _sleep;
                }
                return retVal;
            }
            private set
            {
                lock (_locker)
                {
                    _sleep = value;
                }
            }
        }
        #endregion

        #region Overridden Methods
        /// <summary>
        /// Overridden method to add a new ATask instance to the thread and subsequently wake the sleeping Thread
        /// to allow it to be executed.
        /// </summary>
        /// <param name="task">ATask to be executed.</param>
        /// <returns>Boolean representing success</returns>
        public override bool AddNewTask(ATask task)
        {
            bool retVal = false;
            if (task != null)
            {
                if (IsSleeping)
                {
                    if (task.Delegate != null && task.State == TaskState.ToBeExecuted)
                    {
                        lock (_locker)
                        {
                            _currentTask = task;
                            _thread.Interrupt(); //Wakes the Thread from sleeping state.
                            IsSleeping = false;
                            retVal = true;
                        }
                    }
                }
            }
            return retVal;
        }
        /// <summary>
        /// Thread loop that executes the ATask delegate available, then sleeping which allows the scheduler to provide more work.
        /// </summary>
        protected override void threadLoop()
        {
            TaskDelegate localDelegate = null;
            while (!IsAborted)
            {
                if(IsClosing)
                {
                    break;
                }
                lock (_locker)
                {
                    if (_currentTask != null)
                    {
                        if (_currentTask.State == TaskState.ToBeExecuted)
                        {
                            localDelegate = _currentTask.Delegate;
                            if (localDelegate != null)
                            {
                                try
                                {
                                    _currentTask.State = TaskState.Executing;
                                    _measure.StartedTask();
                                    localDelegate.Invoke();
                                    _measure.FinishedTask();
                                    _measure.IncrementThroughput();
                                    _currentTask.State = TaskState.Complete;
                                }
                                catch (Exception e)
                                {
                                    _measure.FinishedTask();
                                    _currentTask.State = TaskState.Aborted;
                                }
                            }
                            else
                            {
                                _currentTask.State = TaskState.Complete;
                            }
                            localDelegate = null;
                            _currentTask = null;
                        }
                    }
                }

                if (IsClosing) //Closes the Thread as the system is closing.
                {
                    break;
                }
                try
                {
                    IsSleeping = true;
                    Thread.Sleep(SLEEP_LEN); //Send Thread to sleep for SLEEP_LEN microseconds.
                }
                catch (ThreadInterruptedException) //Ensures the system correctly handles the thread being interupted.
                {
                }
            }
            _measure.Complete();
            GetScheduler.threadClosed(ID,ParentLocation);
            cleanUp();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Method to clean up Thread object before GC.
        /// </summary>
        private void cleanUp()
        {
            _currentTask = null;
            abstractCleanUp();
        }
        #endregion
        
    }
}
