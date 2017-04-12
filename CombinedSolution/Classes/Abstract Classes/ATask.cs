using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CombinedSolution.Classes
{
    public abstract class ATask 
    {
        private List<Guid> _dependencies; //List of all dependencies associated with this Task
        private TaskState _state; //Enum value representing current state
        protected TaskDelegate _delegate; //Delegate value representing Task to be executed.
        protected readonly object _locker; //Readonly object utilised in Locks, prevents deadlocks etc.
        private bool _enteredScheduler; //Boolean value representing if the Task has passed into the Scheduler instance.
        private Guid _id; //Represents the ID of the Task
        private bool _closingTask;

        public ATask(bool isClosingTask)
        {
            _id = Guid.NewGuid();
            _locker = new object();
            _state = TaskState.ToBeExecuted;
            _dependencies = null;
            _enteredScheduler = false;
            _closingTask = isClosingTask;
        }

        #region Public
        /// <summary>
        /// Method through which a user can replace the delegate associated with an ATask.
        /// </summary>
        /// <param name="del">TaskDelegate to be assigned.</param>
        /// <returns>Boolean representing success.</returns>
        public bool ReplaceDelegate(TaskDelegate del)
        {
            bool retVal = false;
            if (del != null)
            {
                //Cannot modify after the task has entered the scheduler.
                if (!_enteredScheduler)
                {
                    lock (_locker)
                    {
                        _delegate = del;
                        retVal = true;
                    }
                }
            }
            return retVal;
        }
        /// <summary>
        /// Method to add the Guid of an ATasks dependency.
        /// </summary>
        /// <param name="id">Guid of the dependency</param>
        /// <returns>Bool representing success.</returns>
        public bool AddDependency(Guid id)
        {
            bool retVal = false;
            if (id != Guid.Empty)
            {
                lock (_locker)
                {
                    //Cannot modify after entering scheduler.
                    if (!_enteredScheduler)
                    {
                        if (_dependencies == null)
                        {
                            _dependencies = new List<Guid>();
                        }
                        _dependencies.Add(id);
                        retVal = true;
                    }
                }
            }
            return retVal;
        }
        /// <summary>
        /// Method that adds a list of dependencies, in the form of Guid's, to the ATask.
        /// </summary>
        /// <param name="ids">The list of Guid's</param>
        /// <returns>Integer representing success (-1) or point of failure.</returns>
        public int AddDependencies(List<Guid> ids)
        {
            int retVal = -1;
            if (ids != null && ids.Count > 0 && !_enteredScheduler)
            {
                lock (_locker)
                {
                    int len = ids.Count();
                    for (int i = 0; i < len; ++i)
                    {
                        if (!AddDependency(ids[i]))
                        {
                            retVal = i;
                            break;
                        }
                    }
                }
            }
            else
            {
                retVal = 0;
            }
            return retVal;
        }
        /// <summary>
        /// Method to remove a dependency associated with the ATask.
        /// </summary>
        /// <param name="id">Guid of the dependency to remove.</param>
        public void RemoveDependancy(Guid id)
        {
            if (id != Guid.Empty)
            {
                if (this.DependencyCount > 0)
                {
                    lock (_locker)
                    {
                        foreach (Guid i in _dependencies)
                        {
                            if (i == id)
                            {
                                _dependencies.Remove(i);
                                break;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Internal
        /// <summary>
        /// Method used by scheduler to modify a boolean value that ensures the user doesn't modify
        /// the state of the ATask after it has passed the boundary fo the system.
        /// </summary>
        public void enteredScheduler()
        {
            lock (_locker)
            {
                _enteredScheduler = true;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Property to retrieve the Guid of the ATask
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
        /// <summary>
        /// Property to retrieve the number of dependencies associated with the ATask instance.
        /// </summary>
        public int DependencyCount
        {
            get
            {
                int retVal = 0;
                lock (_locker)
                {
                    if (_dependencies != null)
                    {
                        retVal = _dependencies.Count;
                    }
                }
                return retVal;
            }
        }
        /// <summary>
        /// Method to retrieve a list of dependencies associated with the ATask.
        /// </summary>
        public List<Guid> Dependencies
        {
            get
            {
                List<Guid> retVal = null;
                lock (_locker)
                {
                    if (_dependencies != null)
                    {
                        retVal = _dependencies;
                    }
                }
                return retVal;
            }
        }
        /// <summary>
        /// Property to retrieve the TaskDelegate associated with the ATask instance.
        /// </summary>
        public TaskDelegate Delegate
        {
            get
            {
                TaskDelegate retVal = null;
                lock (_locker)
                {
                    if (_delegate != null)
                    {
                        retVal = _delegate;
                    }
                }
                return retVal;
            }
        }
        /// <summary>
        /// Property to retrieve or modify the State enum associated with the ATask instance.
        /// </summary>
        public TaskState State
        {
            get
            {
                TaskState retVal = TaskState.ToBeExecuted;
                lock (_locker)
                {
                    retVal = _state;
                }
                return retVal;
            }
            set
            {
                if (value != TaskState.ToBeExecuted)
                {
                    lock (_locker)
                    {
                        _state = value;
                    }
                }
            }
        }
        #endregion
    }
}
