using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CombinedSolution.Classes
{
    public class WorkStealingThread : AThread
    {
        #region Static Methods & Vars
        private static Random sRand = new Random(); //Static random used to ensure output is true random.
        private static readonly object sLock = new object(); 
        /// <summary>
        /// Method to retrieve a random value between 0 and max.
        /// </summary>
        /// <param name="max">Integer value representing the highest value to return.</param>
        /// <returns>Random integer value between 0 and max.</returns>
        private static int GetRandom(int max)
        {
            int retVal = 0;
            lock (sLock)
            {
                retVal = sRand.Next(0, max); //Gets a Random value between 0 and Max
            }
            return retVal;
        }
        #endregion

        private List<Guid> _completedTasks;
        private DequeLinkedList<ATask> _deque;
        private WorkStealingThread[] _otherThreads;


        public WorkStealingThread(Scheduler parent, int location)
            : base(parent, location)
        {
            _measure = new Measurement.ThreadMeasure(true);
            _completedTasks = new List<Guid>();
            _deque = new DequeLinkedList<ATask>();
        }

        #region Publicly Visible Methods
        /// <summary>
        /// Method to add references to the other threads associated with the Scheduler
        /// to a local data store, for use within the stealing mechanism.
        /// </summary>
        /// <param name="otherThreads">Array of other WorkStealingThread objects</param>
        /// <returns></returns>
        public bool AddOtherThreads(WorkStealingThread[] otherThreads)
        {
            bool retVal = false;
            int len = 0;
            if (otherThreads != null)
            {
                if (!IsStarted)
                {
                    len = otherThreads.Length;
                    if (len > 0)
                    {
                        lock (_locker)
                        {
                            this._otherThreads = new WorkStealingThread[len];
                            Array.Copy(otherThreads, this._otherThreads, len);
                            IsReady = true;
                            retVal = true;
                        }
                    }
                }
            }
            return retVal;
        }
        /// <summary>
        /// Method used by other Thread instances to steal ATask objects
        /// </summary>
        /// <returns>ATask object that has been retrieved form the deque.</returns>
        public ATask Steal()
        {
            ATask retVal = null;
            
            if (!IsClosing && !IsAborted) 
            {
                retVal = _deque.Eject(); //Take tail node and return it. 
            }
            return retVal;
        }
        /// <summary>
        /// Method utilised by the Scheduler object to retrieve the completed ATasks 
        /// to update the _outstandingDependencies list.
        /// </summary>
        /// <returns>List of completed ATasks Guid's</returns>
        public List<Guid> RetrieveCompletedTasks()
        {
            List<Guid> tempList;
            lock (_locker)
            {
                tempList = new List<Guid>(_completedTasks);
                _completedTasks.Clear();
            }

            return tempList;
        }
        #endregion

        #region Overidden Methods
        /// <summary>
        /// Overriden method from AThread that enables ATask objects to be added to the
        /// _readyDeque data structure.
        /// </summary>
        /// <param name="task">ATask instance to be added to the _readyDeque.</param>
        /// <returns></returns>
        public override bool AddNewTask(ATask task)
        {
            bool retVal = false;
            if (task != null)
            {
                //Ensure that the ATask instance has valid delegate to be executed
                //& has not been aborted or perviously executed.
                if (task.Delegate != null && task.State == TaskState.ToBeExecuted)
                {
                    //Add ATask to the tail of the Deque.
                    retVal = _deque.Inject(task);
                }
            }
            return retVal;
        }
        /// <summary>
        /// Overriden method that encapsulates the function that executes on the C# Thread instance.
        /// </summary>
        protected override void threadLoop()
        {
            TaskDelegate localDelegate = null;
            ATask localTask = null;
            while (!IsAborted) //Only halts from this when the system has aborted.
            {
                if(IsClosing)
                {
                    break;
                }
                localTask = _deque.Pop(); //Take the head node from the Deque.
                if (localTask != null)
                {
                
                    if (localTask.State == TaskState.ToBeExecuted)
                    {
                        localDelegate = localTask.Delegate; //Assign ATask instances delegate to a local delegate object.
                        if (localDelegate != null)
                        {
                            try
                            {
                                localTask.State = TaskState.Executing;
                                _measure.StartedTask();
                                localDelegate.Invoke(); //Invoke the delegate thereby executing the code within it.
                                _measure.FinishedTask();
                                _measure.IncrementThroughput();
                                localTask.State = TaskState.Complete;
                            }
                            catch (Exception e)
                            {
                                _measure.FinishedTask();
                                localTask.State = TaskState.Aborted;
                            }
                        }
                        else
                        {
                            localTask.State = TaskState.Complete;
                        }
                        lock (_locker)
                        {
                            _completedTasks.Add(localTask.ID); //Add to completed task list to be collected by scheduler.
                        }
                        localTask = null;
                    }
                }
                else if (IsClosing) //Leave loop as system is closing && deque has 0 elements
                {
                    break;
                }
                else //Deque == 0 && not closing
                {
                    if(!stealingMechanism()) //Attempt to steal an ATask from other threads.
                    {
                        Thread.Sleep(200);
                    }
                }
            }
            _measure.Complete();
            GetScheduler.threadClosed(ID,ParentLocation); //Inform Scheduler of closure.
            cleanUp(); //Clean up data structures etc.
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Method to aid in the destruction of the object after closing/aborting.
        /// </summary>
        private void cleanUp()
        {
            _completedTasks = null;
            _deque = null;
            _otherThreads = null;
            abstractCleanUp();
        }
        /// <summary>
        /// The method through which attempts to steal ATask objects from other threads.
        /// </summary>
        /// <returns>Boolean value representing success.</returns>
        private bool stealingMechanism()
        {
            bool retVal = false;
            int threadCount = 0;
            int rVal = 0;
            ATask newTask = null;
            lock (_locker)
            {
                threadCount = _otherThreads.Length;
                rVal = GetRandom(threadCount - 1); //Random between 0 & (N-1)
                if (_otherThreads[rVal] != null) //Ensure thread object hasn't closed 
                {
                    _measure.StealAttempt();
                    newTask = _otherThreads[rVal].Steal(); 
                    if (newTask != null)
                    {
                        _measure.SuccessfulSteal();
                        retVal = _deque.Inject(newTask); //Attempt to add to deque.
                    }
                }
                
            }
            return retVal;
        }
        #endregion
    }
}
