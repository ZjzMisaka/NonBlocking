﻿// Copyright (c) Vladimir Sadov. All rights reserved.
//
// This file is distributed under the MIT License. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace NonBlocking
{
    internal sealed class DictionaryImplBoxed<TKey, TValue>
            : DictionaryImpl<TKey, Boxed<TKey>, TValue>
    {
        internal DictionaryImplBoxed(ConcurrentDictionary<TKey, TValue> topDict)
            : base(topDict)
        {
        }

        internal DictionaryImplBoxed(DictionaryImplBoxed<TKey, TValue> other, int capacity)
            : base(other, capacity)
        {
        }

        protected override bool TryClaimSlotForPut(ref Boxed<TKey> entryKey, TKey key, Counter slots)
        {
            var entryKeyValue = entryKey;
            if (entryKeyValue == null)
            {
                entryKeyValue = Interlocked.CompareExchange(ref entryKey, new Boxed<TKey>(key), null);
                if (entryKeyValue == null)
                {
                    // claimed a new slot
                    slots.increment();
                    return true;
                }
            }

            return _keyComparer.Equals(key, entryKey.Value);
        }

        protected override bool TryClaimSlotForCopy(ref Boxed<TKey> entryKey,Boxed<TKey> key, Counter slots)
        {
            var entryKeyValue = entryKey;
            if (entryKeyValue == null)
            {
                entryKeyValue = Interlocked.CompareExchange(ref entryKey, key, null);
                if (entryKeyValue == null)
                {
                    // claimed a new slot
                    slots.increment();
                    return true;
                }
            }

            return _keyComparer.Equals(key.Value, entryKey.Value);
        }

        protected override bool keyEqual(TKey key, Boxed<TKey> entryKey)
        {
            return _keyComparer.Equals(key, entryKey.Value);
        }

        protected override DictionaryImpl<TKey, Boxed<TKey>, TValue> CreateNew(int capacity)
        {
            return new DictionaryImplBoxed<TKey, TValue>(this, capacity);
        }

        protected override TKey keyFromEntry(Boxed<TKey> entryKey)
        {
            return entryKey.Value;
        }
    }

    internal class Boxed<T>
    {
        public readonly T Value;

        public Boxed(T key)
        {
            this.Value = key;
        }
    }
}