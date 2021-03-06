﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Ninu.Base
{
    public class TrackedMemory : IMemory
    {
        private readonly byte[] _memory;

        private readonly HashSet<ushort> _changedAddresses = new(128);
        private readonly Dictionary<ushort, byte> _changes = new(128);

        public TrackedMemory(int size)
        {
            if (size < 0 || size > 65536)
            {
                throw new ArgumentOutOfRangeException(nameof(size), $"The argument for parameter {nameof(size)} must be greater than or equal to 0 and less or equal to 65536.");
            }

            _memory = new byte[size];
        }

        public TrackedMemory(byte[] memory)
        {
            if (memory.Length > 65536)
            {
                throw new ArgumentOutOfRangeException(nameof(memory), $"The argument for parameter {nameof(memory)} must be less than or equal to 65536.");
            }

            _memory = new byte[memory.Length];

            Array.Copy(memory, _memory, memory.Length);
        }

        /// <summary>
        /// Determines if all of the changes between two tracked memory objects are the same. This
        /// method does not compare the backing store.
        /// </summary>
        /// <param name="left">The first tracked memory object.</param>
        /// <param name="right">The second tracked memory object.</param>
        /// <returns><c>true</c> if the changes between the two tracked memory objects are exactly the same; otherwise, <c>false</c>.</returns>
        public static bool AreChangesEqual(TrackedMemory left, TrackedMemory right)
        {
            if (left._changedAddresses.Count != right._changedAddresses.Count)
            {
                return false;
            }

            // Because we ensure that both objects have the same amount of changes, we only have to
            // iterate through one of the object's dictionary's keys. We check if the address in
            // the left object exists in the changes in the right. If not, they are different. If
            // so, we compare their values.
            foreach (var address in left._changes.Keys)
            {
                if (!right._changedAddresses.Contains(address))
                {
                    return false;
                }

                if (left._changes[address] != right._changes[address])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Merges all changes into the backing store.
        /// </summary>
        public void CommitChanges()
        {
            foreach (var change in _changes)
            {
                _memory[change.Key] = change.Value;
            }

            _changedAddresses.Clear();
            _changes.Clear();
        }

        public byte this[ushort address]
        {
            get
            {
                if (address >= _memory.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(address), $"The argument for parameter {nameof(address)} must be less than the total amount of memory ({_memory.Length}).");
                }

                return _changedAddresses.Contains(address) ? _changes[address] : _memory[address];
            }
            set
            {
                if (address >= _memory.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(address), $"The argument for parameter {nameof(address)} must be less than the total amount of memory ({_memory.Length}).");
                }

                if (_changedAddresses.Contains(address))
                {
                    if (_memory[address] == value)
                    {
                        _changedAddresses.Remove(address);
                        _changes.Remove(address);
                    }
                    else
                    {
                        _changes[address] = value;
                    }
                }
                else
                {
                    if (_memory[address] != value)
                    {
                        _changedAddresses.Add(address);
                        _changes[address] = value;
                    }
                }
            }
        }

        public ushort Size => (ushort)_memory.Length;

        public IEnumerator<byte> GetEnumerator()
        {
            foreach (var b in _memory)
            {
                yield return b;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => _memory.GetEnumerator();
    }
}