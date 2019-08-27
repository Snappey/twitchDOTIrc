using System;
using System.Collections.Generic;
using System.Text;

namespace twitchDotIRC.Util
{
    class FixedSizeQueue<T>
    { 
        private Queue<T> queue;

        public int Limit { get; set; }

        public bool Enqueue(T item)
        {
            queue.Enqueue(item);

            bool removed = false;
            while (queue.Count > Limit)
            {
                bool suc = queue.TryDequeue(out T _);
                if (suc)
                {
                    removed = true;
                }        
            }

            return removed;
        }

        public T Dequeue()
        {
            bool suc = queue.TryDequeue(out T item);

            if (suc)
            {
                return item;
            }
            return default(T);
        }

        public T Peek()
        {
            return queue.Peek();
        }
    }
}
