using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombinedSolution.Classes
{
    public class Task : ATask
    {
        public Task(TaskDelegate t) 
            : base(false)
        {
            if (t == null)
            {
                throw new ArgumentNullException("TaskDelegate must not have a Null value.");
            }
            else
            {
                _delegate = t;
            }
        }
    }
    public class Task<T> : ATask
    {
        private T _returnValue;
        private Func<T> _rDelegate;
        private bool _hasReturned;
        public Task(Func<T> rt) 
            : base(false)
        {
            if (rt == null)
            {
                throw new ArgumentNullException("Delegate must not have a Null value.");
            }
            else
            {
                _hasReturned = false;
                _rDelegate = rt;
                _delegate = () => saveOutput();
            }
        }
        private void saveOutput()
        {
            lock (_locker)
            {
                _returnValue = _rDelegate.Invoke();
                _hasReturned = true;
            }
        }
        public T GetOutput
        {
            get
            {
                T retVal = default(T);
                if (_hasReturned)
                {
                    retVal = _returnValue;
                }
                return retVal;
            }
        }
    }
}
