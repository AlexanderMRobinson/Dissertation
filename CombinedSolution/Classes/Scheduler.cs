using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace CombinedSolution.Classes
{
    public class Scheduler
    {
        #region Singleton
        private static readonly Scheduler _instance = new Scheduler();
        /// <summary>
        /// Property through which the Scheduler instance can be retrieved.
        /// Required so that the class is not specified as beforefieldinit.
        /// If this was not done then the class would be necessarily be instanciated
        /// when a new a instance was created (could happen before).
        /// Ensures thatcreation occurs at a specific time.
        /// </summary>
        public static Scheduler Instance
        {
            get
            {
                return _instance;
            }
        }
        /// <summary>
        /// Static Constructor for Scheduler class
        /// </summary>
        static Scheduler()
        {
        }
        /// <summary>
        /// Private constructor that builds the Shceduler object.
        /// </summary>
        private Scheduler()
        {
            _locker = new object();
            _maxThreads = 8;
            _state = SchedulerState.NotStarted;
            _mechanism = SchedulingMechanism.WorkSharing;
            _readyQueue = new Queue<ATask>();
            _outstandingDependenciesList = new List<ATask>();
            _taskCount = 0;
            _threadString = new string[_maxThreads];
        }
        #endregion

        private readonly object _locker;
        private SchedulingMechanism _mechanism;
        private int _maxThreads;
        private SchedulerState _state;

        private Thread _schedulerThread;     
        private List<Guid> _completedTasks;
        private List<ATask> _outstandingDependenciesList;
        private Guid[] _currentlyExecuting;
        private AThread[] _threadArray;
        private int _threadCount;
        private int _taskCount;
        private string[] _threadString;

        private Queue<ATask> _readyQueue;
        private Random _rand;

        #region Properties
        /// <summary>
        /// Property to return a the number of tasks with outstanding dependencies,
        /// which is gathered in a thread safe manner.
        /// </summary>
        private int OutstandingTaskCount
        {
            get
            {
                int retval = 0;
                lock (_locker)
                {
                    retval = _outstandingDependenciesList.Count;
                }
                return retval;
            }
        }
        /// <summary>
        /// Property to return a the number of tasks within the ready queue,
        /// which is gathered in a thread safe manner.
        /// </summary>
        private int ReadyQueueCount
        {
            get
            {
                int retVal = 0;
                lock(_locker)
                {
                    retVal = _readyQueue.Count;
                }
                return retVal;
            }
        }
        /// <summary>
        /// Property that returns or modifies the _maxthreads variable 
        /// in a thread safe manner.
        /// </summary>
        public int MaxThreads
        {
            get
            {
                int retVal = 0;
                lock (_locker)
                {
                    retVal = _maxThreads;
                }
                return retVal;
            }
            set
            {
                lock (_locker)
                {
                    //Cannot modify max threads once the system has started
                    if (State == SchedulerState.NotStarted)
                    {
                        //Max thread count must be greater than 0
                        if(value > 0)
                        {
                            //Max thread count must not exceed 2 * number of available logical cores
                            //to prevent performance loss from switching between threads.
                            if(value <= (Environment.ProcessorCount * 2))
                            {
                                _maxThreads = value;
                            }
                        }

                    }
                }
            }
        }
        /// <summary>
        /// Property that returns an integer representing the current number of 
        /// live thread objects in a thread safe manner.
        /// </summary>
        public int ThreadCount
        {
            get
            {
                int retVal = 0;
                lock (_locker)
                {
                    retVal = _threadCount;
                }
                return retVal;
            }
        }
        /// <summary>
        /// Property that returns or modifies the scheduling mechanism being employed.
        /// </summary>
        public SchedulingMechanism Mechanism
        {
            get
            {
                SchedulingMechanism retVal = SchedulingMechanism.WorkSharing;
                lock (_locker)
                {
                    retVal = _mechanism;
                }
                return retVal;
            }
            set
            {
                if (State == SchedulerState.NotStarted)
                {
                    if (value != Mechanism)
                    {
                        lock (_locker)
                        {
                            _mechanism = value;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Property that returns or modifies the Schedulers state enumerator in a thread safe manner.
        /// </summary>
        public SchedulerState State
        {
            get
            {
                SchedulerState retVal = SchedulerState.Running;
                lock (_locker)
                {
                    retVal = _state;
                }
                return retVal;
            }
            private set
            {
                if (_state != SchedulerState.Aborted && _state != SchedulerState.Closing)
                {
                    lock (_locker)
                    {
                        _state = value;
                    }
                }
            }
        }
        #endregion

        #region Public
        /// <summary>
        /// Method that creates the associated worker thread instances, starts them
        /// and starts the scheduling thread.
        /// </summary>
        public void Start()
        {
            int numThreads, i;
            if (State == SchedulerState.NotStarted)
            {
                State = SchedulerState.Running;
                numThreads = MaxThreads;
                _completedTasks = new List<Guid>();

                _currentlyExecuting = new Guid[MaxThreads];
                switch (Mechanism)
                {
                    case SchedulingMechanism.WorkSharing:
                        _threadArray = new WorkSharingThread[numThreads];
                        for (i = 0; i < numThreads; ++i)
                        {
                            _threadArray[i] = new WorkSharingThread(this,i);
                        }
                        _schedulerThread = new Thread(sharingThreadLoop);
                        break;
                    case SchedulingMechanism.WorkStealing:
                        _rand = new Random();
                        _threadArray = new WorkStealingThread[numThreads];
                        for (i = 0; i < numThreads; ++i)
                        {
                            _threadArray[i] = new WorkStealingThread(this,i);
                        }
                        assignTaskQueue(); //Assign the Tasks that have previously been added to the scheduler to appropriate threads.
                        passOtherThreads(); //Pass the threads references toi other threads within the system.
                        _schedulerThread = new Thread(stealingThreadLoop);
                        break;
                }
                startThreads();
                _schedulerThread.Start();
            }
        }
        /// <summary>
        /// Method that informs the associated threads that the system will be closing 
        /// after the fulfillment of all outstanding tasks.
        /// </summary>
        public void Close()
        {
            State = SchedulerState.Closing;
            //This mechanism is employed within the sharingThreadLoop for the work
            //sharing mechanism.
        }
        /// <summary>
        /// Method used to end all execution immediately.
        /// </summary>
        public void Abort()
        {
            if (State == SchedulerState.Running)
            {
                _schedulerThread.Abort();
                lock (_locker)
                {
                    int threadCount = _threadArray.Length;
                    for (int i = 0; i < threadCount; ++i)
                    {
                        if (_threadArray[i] != null)
                        {
                            _threadArray[i].Abort();
                        }
                    }
                }
            }
            State = SchedulerState.Aborted;
        }
        /// <summary>
        /// Method to add ATask objects to the scheduler and the associated threads.
        /// </summary>
        /// <param name="task">ATask object to be added</param>
        /// <returns>Boolean representing success.</returns>
        public bool AddNewTask(ATask task)
        {
            bool retVal = false;
            if (State == SchedulerState.NotStarted || State == SchedulerState.Running)
            {
                lock (_locker)
                {
                    if (task != null)
                    {
                        if(task.State == TaskState.ToBeExecuted)
                        {
                            task.enteredScheduler();

                            ++_taskCount;
                            if (State == SchedulerState.Running) //Tasks will only have been completed when the Scheduler is in Running state
                            {
                                if (task.DependencyCount > 0)
                                {
                                    foreach (Guid id in _completedTasks)
                                    {
                                        task.RemoveDependancy(id);
                                    }
                                }
                            }
                            if (task.DependencyCount > 0) //Add to seperate list as there are outstanding dependencies.
                            {
                                _outstandingDependenciesList.Add(task);
                                retVal = true;
                            }
                            else
                            {
                                switch (Mechanism)
                                {
                                    case SchedulingMechanism.WorkSharing:
                                        _readyQueue.Enqueue(task);
                                        retVal = true;
                                        break;
                                    case SchedulingMechanism.WorkStealing:
                                        if (State == SchedulerState.NotStarted)
                                        {
                                            //Add to ready queue as the threads have not been created yet.
                                            _readyQueue.Enqueue(task);
                                            retVal = true;
                                        }
                                        else if (State == SchedulerState.Running)
                                        {
                                            //Automatically assign as the threads have been created.
                                            retVal = assignTaskToThread(task);
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            return retVal;
        }
        /// <summary>
        /// Method to add an array of ATasks to the scheduler and associated threads.
        /// Utilises the AddNewTask(ATask) method to fulfill this.
        /// </summary>
        /// <param name="tasks">Array of ATask's to be added.</param>
        /// <returns>Integer representing success (-1) or failure (array point of failing ATask)</returns>
        public int AddNewTasks(ATask[] tasks)
        {
            int retVal = -1;
            int tLen = tasks.Length;
            if (tLen == 0)
            {
                retVal = 0;
            }
            else
            {
                for (int i = 0; i < tLen; ++i)
                {
                    if (!AddNewTask(tasks[i]))
                    {
                        retVal = i;
                        break;
                    }
                }
            }
            return retVal;
        }
        #endregion

        #region Non-Public
        /// <summary>
        /// Metod used to clean up the necessary data structures upon the closure of a worker thread instance.
        /// </summary>
        /// <param name="id">Guid representing the closing thread.</param>
        internal void threadClosed(Guid id, int location)
        {
            List<Guid> tempList= null;
            lock (_locker)
            {
                if (_threadArray[location] != null)
                {
                    if (_threadArray[location].ID == id)
                    {
                        if (Mechanism == SchedulingMechanism.WorkStealing)
                        {
                            WorkStealingThread instance = (WorkStealingThread)_threadArray[location];
                            tempList = instance.RetrieveCompletedTasks();
                        }
                        else
                        {
                            if (_currentlyExecuting[location] != Guid.Empty)
                            {
                                tempList = new List<Guid>();
                                tempList.Add(_currentlyExecuting[location]);
                                _currentlyExecuting[location] = Guid.Empty;
                            }
                        }
                        _threadString[location] = "ID:- " + location.ToString() + "\r\n" + _threadArray[location].ToString();
                        _threadArray[location] = null;
                        --_threadCount;
                    }
                }
            }
            if (tempList != null)
            {
                if (tempList.Count > 0)
                {
                    foreach (Guid guid in tempList)
                    {
                        removeFromOutstandingList(guid);
                    }
                }
            }
        }
        /// <summary>
        /// Method to remove dependemncies that have executed from ATask's within the 
        /// _outstandingDependencies list
        /// </summary>
        /// <param name="id">Guid of the ATask that has completed execution.</param>
        private void removeFromOutstandingList(Guid id)
        {
            
            //Remove dependencies with GUID id from Tasks in _outstandingDependenciesList
            lock (_locker)
            {
                List<ATask> removed = new List<ATask>();
                int count = _outstandingDependenciesList.Count;
                ATask t = null;
                for(int i = 0; i < count; ++i)
                {
                    t = _outstandingDependenciesList[i];
                    if (t != null)
                    {
                        t.RemoveDependancy(id);
                        if (t.DependencyCount == 0)
                        {
                            switch (_mechanism)
                            {
                                case SchedulingMechanism.WorkStealing:
                                    if (assignTaskToThread(t))
                                    {
                                        removed.Add(t);
                                    }
                                    break;
                                case SchedulingMechanism.WorkSharing:
                                    _readyQueue.Enqueue(t);
                                    removed.Add(t);
                                    break;
                            }
                        }
                    }
                }
                foreach(ATask tas in removed)
                {
                    _outstandingDependenciesList.Remove(tas);
                }
                _completedTasks.Add(id);
            }
        }
        /// <summary>
        /// Method to call the Start() method on all worker threads within the system.
        /// </summary>
        private void startThreads()
        {
            int len = _threadArray.Length;
            for (int i = 0; i < len; ++i)
            {
                _threadArray[i].Start();
                lock (_locker)
                {
                    ++_threadCount;
                }
            }
        }
        /// <summary>
        /// Method to clean up the scheduler before GC after closure/abort.
        /// </summary>
        private void cleanUp()
        {
            
            _maxThreads = 0;    
            _completedTasks = null;
            _outstandingDependenciesList = null;

            switch (Mechanism)
            {
                case SchedulingMechanism.WorkSharing:
                    _currentlyExecuting = null;
                    _readyQueue = null;
                    break;
                case SchedulingMechanism.WorkStealing:
                    _rand = null;
                    break;
            }
            Console.WriteLine(Mechanism.ToString());
            Console.Write(ToString());
        }

        private void closeThreads()
        {
            lock (_locker)
            {
                foreach (AThread at in _threadArray)
                {
                    at.Close();
                }
            }
        }
        #endregion

        #region WorkSharing Methods
        /// <summary>
        /// Method that is used to schedule ATask objects onto the WorkSharingThreads
        /// </summary>
        private void sharingThreadLoop()
        {
            int count = 0;
            int i = 0;
            ATask task = null;
            WorkSharingThread thread = null;
            while (State != SchedulerState.Aborted)
            {
                if (ThreadCount == 0) //All threads have closed therefore system has closed.
                {
                    break;
                }
                lock (_locker)
                {
                    count = ThreadCount;
                    //Iterate over the thread array to find any sleeping threads.
                    for (i = 0; i < count; ++i)
                    {
                        if (_threadArray[i] != null)
                        {
                            thread = (WorkSharingThread)_threadArray[i];
                            if (thread.IsSleeping)
                            {
                                //Update outstandingDependencies list as thread has completed execution.
                                if (_currentlyExecuting[i] != Guid.Empty) 
                                {
                                    removeFromOutstandingList(_currentlyExecuting[i]);
                                    _currentlyExecuting[i] = Guid.Empty;
                                }
                                //Add ATask to thread object if there is an ATask available.
                                if (ReadyQueueCount > 0)
                                {
                                    task = _readyQueue.Dequeue();
                                    if (task != null)
                                    {
                                        if (thread.AddNewTask(task))
                                        {
                                            _currentlyExecuting[i] = task.ID;
                                        }
                                        else
                                        {
                                            //error
                                        }
                                        task = null;
                                        thread = null;
                                    }
                                }
                            }
                        }
                    }
                    //Closing mechanism for the threads.
                    if (State == SchedulerState.Closing)
                    {
                        if (ReadyQueueCount == 0) 
                        {
                            if (OutstandingTaskCount == 0) 
                            {
                                lock (_locker)
                                {
                                    if (_taskCount == _completedTasks.Count)
                                    {
                                        closeThreads();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                if (ThreadCount == 0)
                {
                    break;
                }
                Thread.Sleep(100);
            }
            while (true)
            {
                if (ThreadCount == 0)
                {
                    cleanUp();
                    break;
                }
            }
        }
        #endregion

        #region WorkStealing Methods
        /// <summary>
        /// Method to assign tasks to threads upon starting the system.
        /// </summary>
        private void assignTaskQueue()
        {
            foreach (ATask task in _readyQueue)
            {
                assignTaskToThread(task);
            }
            _readyQueue = null;
        }
        /// <summary>
        /// Method to pass workstealing thread instances copies of references to
        /// the other threads within the system.
        /// </summary>
        private void passOtherThreads()
        {
            int numThreads = MaxThreads;
            WorkStealingThread[] ots = new WorkStealingThread[numThreads - 1];
            WorkStealingThread current = null;
            for (int i = 0; i < numThreads; ++i)
            {
                if (i == 0)
                {
                    Array.Copy(_threadArray, 1, ots, 0, numThreads - 1);
                }
                else
                {
                    ots[i - 1] = (WorkStealingThread)_threadArray[i - 1];
                }
                current = (WorkStealingThread)_threadArray[i];
                current.AddOtherThreads(ots);
            }
        }
        /// <summary>
        /// Method that updates the outstanding dependencies list based upon 
        /// the tasks that have executed on the systems threads.
        /// </summary>
        private void stealingThreadLoop()
        {
            List<Guid> _idList = new List<Guid>();
            WorkStealingThread instance;
            while (State != SchedulerState.Aborted)
            {
                if (ThreadCount == 0)
                {
                    break;
                }
                //Iterate over the threads copying the executed ATask guids to a local list.
                for (int i = 0; i < _maxThreads; ++i)
                {
                    lock (_locker)
                    {
                        instance = (WorkStealingThread)_threadArray[i];
                    }
                    if (instance != null)
                    {
                        _idList.AddRange(instance.RetrieveCompletedTasks());
                            
                    }
                }
                //Iterate over the populated list removing each guid from outstanding dependencies.
                foreach (Guid id in _idList)
                {
                    removeFromOutstandingList(id);
                }
                _idList.Clear();
                _idList.Clear();
                lock (_locker)
                {
                    if (_state == SchedulerState.Closing)
                    {
                        if (_outstandingDependenciesList.Count == 0)
                        {
                            if (_taskCount == _completedTasks.Count)
                            {
                                closeThreads();
                                break;
                            }
                        }
                    }
                }
            }
            while (true)
            {
                if (ThreadCount == 0)
                {
                    cleanUp();
                    break;
                }
            }
        }
        /// <summary>
        /// Method to assign an ATask instance to a random thread instance.
        /// </summary>
        /// <param name="task">ATask to be assigned.</param>
        /// <returns>Boolean representing success.</returns>
        private bool assignTaskToThread(ATask task)
        {
            bool retVal = false;
            lock (_locker)
            {
                if (task != null)
                {
                    AThread thr = _threadArray[GetRand];
                    if (thr != null)
                    {
                        retVal = thr.AddNewTask(task);
                    }
                }
            }
            return retVal;
        }
        /// <summary>
        /// Property through which true random values can be retrieved in a thread safe manner.
        /// </summary>
        private int GetRand
        {
            get
            {
                int retVal = 0;
                lock (_locker)
                {
                    retVal = _rand.Next(0, MaxThreads);
                }
                return retVal;
            }
        }
        #endregion

        public override string ToString()
        {
            string retVal = "";
            lock (_locker)
            {
                foreach(string s in _threadString)
                {
                    retVal += s + "\r\n";
                }
            }
            return retVal;
        }
    }
}
