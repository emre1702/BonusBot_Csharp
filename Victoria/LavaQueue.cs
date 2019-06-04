using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Victoria.Entities;

namespace Victoria
{
    /// <summary>
    /// Queue based on <see cref="LinkedList" />. Follows FIFO.
    /// </summary>
    /// <see cref="AudioTrack" />
    /// </typeparam>
    public sealed class LavaQueue
    {
        private readonly LinkedList<AudioTrack> _linked;
        private readonly Random _random;
        private readonly object _lockObj;

        public event Func<Task> OnQueueChanged;

        /// <inheritdoc cref="LavaQueue" />
        public LavaQueue()
        {
            _random = new Random();
            _linked = new LinkedList<AudioTrack>();
            _lockObj = new object();
        }

        /// <summary>
        /// Returns the total count of items.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lockObj)
                {
                    return _linked.Count;
                }
            }
        }

        /// <inheritdoc cref="IEnumerable{AudioTrack}" />
        public IEnumerable<AudioTrack> Items
        {
            get
            {
                lock (_lockObj)
                {
                    for (var node = _linked.First; node != null; node = node.Next)
                        yield return (AudioTrack)Convert.ChangeType(node.Value, typeof(AudioTrack));
                }
            }
        }

        /// <summary>
        /// Adds an object.
        /// </summary>
        /// <param name="value">
        /// <see cref="AudioTrack" />
        /// </param>
        public void Enqueue(AudioTrack value)
        {
            lock (_lockObj)
            {
                _linked.AddLast(value);
            }
            OnQueueChanged?.Invoke();
        }

        /// <summary>
        /// Removes the first item from queue.
        /// </summary>
        /// <returns>
        /// <see cref="AudioTrack" />
        /// </returns>
        public AudioTrack Dequeue()
        {
            lock (_lockObj)
            {
                var result = _linked.First.Value;
                _linked.RemoveFirst();
                OnQueueChanged?.Invoke();
                return result;
            }
        }

        /// <summary>
        /// Safely removes the first item from queue.
        /// </summary>
        /// <param name="value">
        /// <see cref="AudioTrack" />
        /// </param>
        /// <returns><see cref="bool" /> based on if dequeue-ing was successful.</returns>
        public bool TryDequeue(out AudioTrack value)
        {
            lock (_lockObj)
            {
                if (_linked.Count < 1)
                {
                    value = default;
                    return false;
                }

                var result = _linked.First.Value;
                if (result == null)
                {
                    value = default;
                    return false;
                }

                _linked.RemoveFirst();
                value = result;
                OnQueueChanged?.Invoke();
                return true;
            }
        }

        /// <summary>
        /// Sneaky peaky the first time in list.
        /// </summary>
        /// <returns>
        /// <see cref="AudioTrack" />
        /// </returns>
        public AudioTrack Peek()
        {
            lock (_lockObj)
            {
                return _linked.First.Value;
            }
        }

        /// <summary>
        /// Removes an item from queue.
        /// </summary>
        /// <param name="value">
        /// <see cref="AudioTrack" />
        /// </param>
        public void Remove(AudioTrack value)
        {
            lock (_lockObj)
            {
                _linked.Remove(value);
            }
            OnQueueChanged?.Invoke();
        }

        /// <summary>
        /// Clears the queue.
        /// </summary>
        public void Clear()
        {
            lock (_lockObj)
            {
                _linked.Clear();
            }
            OnQueueChanged?.Invoke();
        }

        /// <summary>
        /// Shuffles the queue.
        /// </summary>
        public void Shuffle()
        {
            lock (_lockObj)
            {
                if (_linked.Count < 2)
                    return;

                var shadow = new AudioTrack[_linked.Count];
                var i = 0;
                for (var node = _linked.First; !(node is null); node = node.Next)
                {
                    var j = _random.Next(i + 1);
                    if (i != j)
                        shadow[i] = shadow[j];
                    shadow[j] = node.Value;
                    i++;
                }

                _linked.Clear();
                foreach (var value in shadow)
                    _linked.AddLast(value);
            }
            OnQueueChanged?.Invoke();
        }

        /// <summary>
        /// Removes an item based on the given index.
        /// </summary>
        /// <param name="index">Index of item.</param>
        /// <returns>
        /// <see cref="AudioTrack" />
        /// </returns>
        public AudioTrack RemoveAt(int index)
        {
            lock (_lockObj)
            {
                var currentNode = _linked.First;

                for (var i = 0; i <= index && currentNode != null; i++)
                {
                    if (i != index)
                    {
                        currentNode = currentNode.Next;
                        continue;
                    }

                    _linked.Remove(currentNode);
                    break;
                }

                OnQueueChanged?.Invoke();
                return currentNode.Value;
            }
        }

        /// <summary>
        /// Removes a item from given range.
        /// </summary>
        /// <param name="from">Start index.</param>
        /// <param name="to">End index.</param>
        public void RemoveRange(int from, int to)
        {
            lock (_lockObj)
            {
                var currentNode = _linked.First;
                for (var i = 0; i <= to && currentNode != null; i++)
                {
                    if (from <= i)
                    {
                        _linked.Remove(currentNode);
                        currentNode = currentNode.Next;
                        continue;
                    }

                    _linked.Remove(currentNode);
                }
            }
            OnQueueChanged?.Invoke();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            int i = 0;
            foreach (var track in Items)
            {
                builder.AppendLine($"{++i}. {track.ToString()}");
            }
            return builder.ToString();
        }
    }
}
