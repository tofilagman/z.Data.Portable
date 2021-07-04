using System;
using System.Collections.Generic;
using System.Text;

namespace z.Data
{
    public interface IPair : IPair<string, object>
    {
        object this[string key] { get; set; }

        object this[int index] { get; }

        object this[int index, object defaultValue] { get; }

        object this[string key, object defaultValue] { get; set; }

        IPair Add(string Key, object Value);

        void Add<T, TValue>(IEnumerable<T> data, Func<T, string> key, Func<T, TValue> value);

        IPair AddOrUpdate(string Key, object Value);

        string ToString();

        IPair IgnoreCase();

    }

    public interface IPair<TKey, TValue> : IDictionary<TKey, TValue>, IDisposable
    {

        TValue this[TKey key] { get; set; }

        TValue this[int index] { get; }

        TValue this[int index, TValue defaultValue] { get; }

        TValue this[TKey key, TValue defaultValue] { get; set; }

        IPair<TKey, TValue> Add(TKey Key, TValue Value);

        void Add<T>(IEnumerable<T> data, Func<T, TKey> key, Func<T, TValue> value);

        IPair<TKey, TValue> AddOrUpdate(TKey Key, TValue Value);

        string ToString();

        IPair<TKey, TValue> IgnoreCase();

    }
}
