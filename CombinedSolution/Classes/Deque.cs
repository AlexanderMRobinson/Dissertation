using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombinedSolution.Classes
{
    /// <summary>
    /// Class describing a double ended queue.
    /// </summary>
    public class Deque<T>
    {
        //Locker object
        private readonly object _locker; //Should it be read only??
        //Represents the max capacity of the deque.
        private int _capacity = 8;
        //Represents the circular buffer that will store the T elements.
        private T[] _circularBuffer;
        //Represents the number of elements in the deque at present.
        private int _count;
        //Integer values representing the head and tail values of the deque within the buffer.
        private int _head;
        private int _tail;

        private int _rhEdge, _lhEdge;

        public Deque()
            : this(8)
        { }

        public Deque(int capacity)
        {
            this._locker = new object();
            this._capacity = capacity;
            _lhEdge = 0;
            _rhEdge = capacity;
            this._circularBuffer = new T[this._capacity];
        }

        /// <summary>
        /// Adds an element to the tail of the deque.
        /// </summary>
        /// <param name="element">Element to be added to deque.</param>
        /// <returns>Boolean value detailing the success of injection(true = success)</returns>
        public bool Inject(T element)
        {
            bool retVal = true;
            if (element != null)
            {
                lock (this._locker)
                {
                    //Check size of deque does not exceed capacity
                    if (!checkSpace())
                    {
                        //Resize buffer as capacity has been reached.
                        retVal = resizeArray();
                    }
                    if (retVal)
                    {
                        //Add to tail of buffer
                        retVal = AddToTail(element);
                    }
                }
            }
            else
            {
                retVal = false;
            }
            return retVal;
        }

        /// <summary>
        /// Adds an element to the head of the deque.
        /// </summary>
        /// <param name="element">Element to be added to deque.</param>
        /// <returns>Boolean value detailing the success of injection(true = success)</returns>
        public bool Push(T element)
        {
            bool retVal = true;
            if (element != null)
            {
                lock (this._locker)
                {
                    //Check size of deque does not exceed capacity
                    if (!checkSpace())
                    {
                        //Resize buffer as capacity has been reached.
                        retVal = resizeArray();
                    }
                    if (retVal)
                    {
                        //Add to tail of buffer
                        retVal = AddToHead(element);
                    }
                }
            }
            else
            {
                retVal = false;
            }
            return retVal;
        }

        /// <summary>
        /// Returns the last most element in the deque.
        /// </summary>
        /// <returns>Element that was previously at the tail of the deque.</returns>
        public T Eject()
        {
            T outVal = default(T);
            lock (this._locker)
            {
                if (this.Count != 0)
                {
                    //Make a copy of T
                    outVal = _circularBuffer[this._tail];
                    _circularBuffer[this._tail] = default(T);
                    this._count = this._count - 1;
                    //Adjust the tail reference node to correct value.
                    if (this._tail > 0)
                    {
                        this._tail = this._tail - 1;
                    }
                    else
                    {
                        //Set it as the furthest element as it has previously wrapped around.
                        this._tail = this._capacity - 1;
                    }
                }
            }
            //Return copy of T
            return outVal;
        }

        /// <summary>
        /// Returns the front most element of the deque. 
        /// </summary>
        /// <returns>Element that was previously at the head of the deque.</returns>
        public T Pop()
        {
            T outVal = default(T);
            lock (this._locker)
            {
                if (this.Count != 0)
                {
                    //Make a copy of T
                    outVal = _circularBuffer[this._head];
                    _circularBuffer[this._head] = default(T);
                    this._count = this._count - 1;
                    //Adjust the head reference node to correct value.
                    if (this._head == (this._capacity - 1))
                    {
                        //Set it as the first element as it has previously wrapped around.
                        this._head = 0;
                    }
                    else
                    {
                        this._head = this._head + 1;
                    }
                }
            }
            //Return copy of T
            return outVal;
        }

        /// <summary>
        /// Class property representing the number of elements currently within the deque.
        /// </summary>
        public int Count
        {
            get
            {
                lock (this._locker)
                {
                    return this._count;
                }
            }
        }

        /// <summary>
        /// Checks to see if there is space for another element to be added.
        /// </summary>
        /// <returns></returns>
        private bool checkSpace()
        {
            bool val;
            lock (_locker)
            {
                if (this.Count < _capacity)
                {
                    val = true;
                }
                else
                {
                    val = false;
                }
            }
            return val;
        }

        /// <summary>
        /// Adds element passed in at the tail of the buffer.
        /// </summary>
        /// <param name="element">Element to be added.</param>
        /// <returns>Boolean representing success</returns>
        private bool AddToTail(T element)
        {
            bool retVal = false;
            if (this.Count > 0)
            {
                lock (_locker)
                {
                    if ((_tail + 1) < _rhEdge)
                    {
                        ++_tail;
                    }
                    else
                    {
                        _tail = 0;
                    }

                    _circularBuffer[_tail] = element;
                    ++_count;
                    retVal = true;
                }
            }
            else
            {
                lock (_locker)
                {
                    this._tail = (_capacity / 2) + 1;
                    this._head = this._tail;
                    this._circularBuffer[this._tail] = element;
                    this._count = this._count + 1;
                    retVal = true;
                }
            }
            return retVal;
        }

        /// <summary>
        /// Adds element passed in at the head of the buffer.
        /// </summary>
        /// <param name="element">Element to be added.</param>
        /// <returns>Boolean representing success.</returns>
        private bool AddToHead(T element)
        {
            bool retVal = false;
            if (this.Count > 0)
            {
                if ((_head - 1) > _lhEdge)
                {
                    --_head;
                }
                else
                {
                    _head = _capacity - 1;
                }

                _circularBuffer[_head] = element;
                ++_count;
                retVal = true;
                
            }
            else
            {
                this._head = (_capacity / 2) - 1;
                this._tail = this._head;
                this._circularBuffer[this._head] = element;
                this._count = this._count + 1;
                retVal = true;
            }
            return retVal;
        }

        /// <summary>
        /// Resizes the buffer as it has reached capacity.
        /// </summary>
        /// <returns>Boolean representing success.</returns>
        private bool resizeArray()
        {
            bool retVal;
            try
            {
                //Modify capacity value 
                _capacity = _capacity * 2;
                _rhEdge = _capacity;
                //Create new array of size Count
                T[] tempArray = new T[this.Count];

                //Copy the values from the old array into the new one
                Array.Copy(_circularBuffer, tempArray, this.Count);

                //Resize circular buffer to cpacity
                _circularBuffer = new T[this._capacity];

                //Get insertion index (capacity / 4 so that we have equal number of empties on head and tail ends)
                int insertionIndex = (this._capacity / 4);

                //Copy values from temp array to circular buffer
                if (this._head != 0)
                {
                    //Copy from head to end of array
                    Array.Copy(tempArray, this._head, _circularBuffer, insertionIndex, (this.Count - this._head));

                    //Copy from front of array to element before head
                    Array.Copy(tempArray, 0, _circularBuffer, (insertionIndex + (this.Count - this._head)), this._head);

                    //Modify head and tail references
                    this._head = insertionIndex;
                    this._tail = (insertionIndex + this.Count) - 1;
                }
                else
                {
                    //Copy whole array across as the order is correct
                    Array.Copy(tempArray, 0, _circularBuffer, insertionIndex, this.Count);
                    //Modify head and tail references
                    this._head = insertionIndex;
                    this._tail = (insertionIndex + this.Count);
                }
                retVal = true;
            }
            catch
            {
                retVal = false;
            }
            return retVal;
        }
    }
}
