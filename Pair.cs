using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace z.Data
{
    /// <summary>
    /// LJ 20150807
    /// </summary>
    [Serializable]
    public class Pair : Dictionary<string, object>, IPair
    {
        public Pair()
            : base()
        { }
        
        public Pair(string key, object value)
        {
            this.Add(key, value);
        }

        public Pair(string data)
            : this()
        {
            foreach (KeyValuePair<string, object> dd in data.ToObject<Dictionary<string, object>>())
            {
                this.Add(dd.Key, dd.Value);
            }
        }

        public Pair(IEqualityComparer<string> comparer) : base(comparer) { }

        public Pair(DataRow dr)
            : this()
        {
            foreach (DataColumn dc in dr.Table.Columns)
            {
                this.Add(dc.ColumnName, dr[dc.ColumnName]);
            }
        }

        public new object this[string key]
        {
            get { return Get(key); }
            set { base[key] = value; }
        }

        public object this[int index]
        {
            get
            {
                return this.Get(index);
            }
        }

        public object this[int index, object defaultValue]
        {
            get
            {
                return this.Get(index, defaultValue);
            }
        }

        public object this[string key, object defaultValue]
        {
            get
            {
                return Get(key, defaultValue);
            }
            set
            {
                base[key] = value;
            }
        }

        protected virtual object Get(string Key, object DefaultValue = null)
        {
            if (this.Keys.Where(x => x.Equals(Key, StringComparison.OrdinalIgnoreCase)).Any())
                return base[Key] == null ? DefaultValue : base[Key];
            else
                return DefaultValue;
        }

        public new IPair Add(string Key, object Value)
        {
            base.Add(Key, Value);
            return this;
        }

        public void Add<T, TValue>(IEnumerable<T> data, Func<T, string> key, Func<T, TValue> value)
        {
            foreach (T g in data)
            {
                base.Add(key(g), value(g));
            }
        }

        public IPair AddOrUpdate(string Key , object Value)
        {
            if (base.ContainsKey(Key))
                base[Key] = Value;
            else
                Add(Key, Value); 
            return this;
        }

        public new string ToString()
        {
            return this.ToJson();
        }

        public void Dispose()
        {
            this.Clear();

            GC.SuppressFinalize(this);
            GC.Collect();
        }

        public IPair IgnoreCase()
        {
            var h = new Pair(StringComparer.OrdinalIgnoreCase);
            this.Each(x => h.Add(x.Key, x.Value));
            return h;
        }

        public static IPair New(string Key, object Value)
        {
            return new Pair().Add(Key, Value);
        }

        protected object Get(int index, object DefaultValue = null)
        {
            var g = this.Keys.ElementAt(index);
            return Get(g, DefaultValue);
        }

    }

    [Serializable]
    public class PairCollection : List<IPair>, IList<IPair>, IDisposable
    {
        public PairCollection() : base() { }

        public PairCollection(DataTable dt)
        {
            foreach (DataRow dr in dt.Rows) this.Add(new Pair(dr));
        }

        public string Serialize()
        {
            return this.ToJson();
        }

        public void Dispose()
        {
            this.Clear();
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }

}
