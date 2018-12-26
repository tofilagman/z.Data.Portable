using System;
using System.Collections.Generic;
using System.Text;

namespace z.Data
{
    public interface IPair : IDictionary<string, object>, IDisposable
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
}
