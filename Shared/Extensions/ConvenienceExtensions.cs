// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;
using System.Linq;
using Microsoft.FSharp.Collections;

namespace Shared
{
    public static class ConvenienceExtensions
    {
        /// <summary>
        /// C#-based folding function for FSharp map
        /// </summary>
        /// <typeparam name="TKey">Key</typeparam>
        /// <typeparam name="TValue">Value</typeparam>
        /// <param name="existingMap">Existing map</param>
        /// <param name="newMap">New map</param>
        /// <returns>Folded map</returns>
        public static FSharpMap<TKey, TValue> FoldMap<TKey, TValue>(this FSharpMap<TKey, TValue> existingMap, FSharpMap<TKey, TValue> newMap) =>
            existingMap.Aggregate(newMap, (oldState, newItem) => oldState.Add(newItem.Key, newItem.Value));

        /// <summary>
        /// Union one value to F# set
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="set">Existing set</param>
        /// <param name="item">Item</param>
        /// <returns>Unioned set</returns>
        public static FSharpSet<T> UnionOne<T>(this FSharpSet<T> set, T item) =>
            SetModule.Union(set, SetModule.OfSeq(new T[] { item }));
    }
}
