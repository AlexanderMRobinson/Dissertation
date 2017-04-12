using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CombinedSolution.Classes
{
    public abstract class AThread
    {
        protected readonly object _locker;
        private Guid _id;
        protected Thread _thread;
        private Scheduler _parent;
        private int _locationInParentArray;
        private bool _close, _abort, _started, _ready; //Boolean values representing different states of the Thread.
        protected Measurement.ThreadMeasure _measure;

        /// <summary>
        /// Standard constructor that is applicable to both thread styles.
        /// </summary>
        /// <param name="parent">Reference to Scheduler</param>
        public AThread(Scheduler parent, int arrLocation)
        {
            if (parent == null)
            {
                throw new ArgumentException("Parent object must be valid object.");
            }
            else if (parent.State == SchedulerState.Aborted || parent.State == SchedulerState.Closing)
            {
                throw new ArgumentException("Parent object must be in appropriate stae when creating threads.");
            }
            else
            {
                _id = Guid.NewGuid();
                _locker = new object();
                _parent = parent;
                _close = false;
                _abort = false;
                _started = false;
                _ready = false;
                _locationInParentArray = arrLocation;
            }
        }

        #region Thread Start/Close Methods
        /// <summary>
        /// Common Start mechanism that starts the Thread executing the threadLoop method. 
        /// </summary>
        /// <returns>Boolean value representing success.</returns>
        public bool Start()
        {
            bool retVal = false;
            if (!IsStarted && IsReady) //Ensure Thread is not already executing.
            {
                _thread = new Thread(threadLoop);
                _thread.Start();
                IsStarted = true;
                retVal = true;
            }
            return retVal;
        }
        /// <summary>
        /// Method to inform the Thread to close by modifying boolean _close value.
        /// </summary>
        public void Close()
        {
            IsClosing = true;
        }
        /// <summary>
        /// Method to inform the Thread to abort by modifying _abort variable.
        /// </summary>
        public void Abort()
        {
            IsAborted = true;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Property of the AThread that allows thread safe access to the local _ready vlaue. 
        /// </summary>
        public bool IsReady
        {
            get
            {
                bool retVal = false;
                lock (_locker)
                {
                    retVal = _ready;
                }
                return retVal;
            }
            set
            {
                lock (_locker)
                {
                    _ready = value;
                }
            }
        }
        /// <summary>
        /// Property to return a Scheduler reference that is thread safe.
        /// </summary>
        public Scheduler GetScheduler
        {
            get
            {
                Scheduler retVal = null;
                lock (_locker)
                {
                    retVal = _parent;
                }
                return retVal;
            }
        }
        /// <summary>
        /// Property to return boolean value of _started variable in a thread safe manner.
        /// </summary>
        public bool IsStarted
        {
            get
            {
                bool retVal = false;
                lock (_locker)
                {
                    retVal = _started;
                }
                return retVal;
            }
            private set
            {
                lock (_locker)
                {
                    if (_started == false) //Cannot go from true to false.
                    {
                        _started = value;
                    }
                }
            }
        }
        /// <summary>
        /// Property to return the value of _abort in a thread safe manner.
        /// </summary>
        public bool IsAborted
        {
            get
            {
                bool retVal = false;
                lock (_locker)
                {
                    retVal = _abort;
                }
                return retVal;
            }
            private set
            {
                lock (_locker)
                {
                    if (_abort == false) //Cannot go from true to false.
                    {
                        _abort = value;
                    }
                }
            }
        }
        /// <summary>
        /// Property to return the boolean value of _close in a thread safe manner.
        /// </summary>
        public bool IsClosing
        {
            get
            {
                bool retVal = false;
                lock (_locker)
                {
                    retVal = _close;
                }
                return retVal;
            }
            private set
            {
                lock (_locker)
                {
                    if (_close == false) //Cannot go from true to false.
                    {
                        _close = value;
                    }
                }
            }
        }
        /// <summary>
        /// Property to return the Guid of the Thread object in a safe manner.
        /// </summary>
        public Guid ID
        {
            get
            {
                Guid retVal = Guid.Empty;
                lock (_locker)
                {
                    retVal = _id;
                }
                return retVal;
            }
        }
        protected int ParentLocation
        {
            get
            {
                int retVal = -1;
                lock (_locker)
                {
                    retVal = _locationInParentArray;
                }
                return retVal;
            }
        }
        #endregion

        #region Virtual Functions
        /// <summary>
        /// Virtual method that is used to add ATask objects to the thread instance.
        /// </summary>
        /// <param name="task">ATask to be added.</param>
        /// <returns>Boolean representing success</returns>
        public virtual bool AddNewTask(ATask task)
        {
            return false;
        }
        /// <summary>
        /// Virtual function representing the threadLoop that all thread instances must possess.
        /// Presence allows the Start() method to be detailed at the abstract level.
        /// </summary>
        protected virtual void threadLoop()
        { }
        #endregion 

        /// <summary>
        /// Method to clean up the abstarct data points in prep for GC.
        /// </summary>
        public void abstractCleanUp()
        {
            _id = Guid.Empty;
            _parent = null;
        }

        public override string ToString()
        {
            return _measure.ToString();
        }
    }
}
