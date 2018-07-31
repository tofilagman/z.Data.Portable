using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using z.Data.JsonClient;

namespace z.Data
{
    /// <summary>
    /// LJ 20141003
    /// </summary>
    public static class Extensions
    {
        public static jDataSetModel JsonModel(this DataSet ds)
        {
            using (jDataSet jds = new jDataSet(ds))
            {
                jDataSetModel sm = new jDataSetModel();
                sm.DataSetName = (jds.DataSource != null) ? jds.DataSource.DataSetName : "";
                sm.Tables = new List<jDataModel>();
                foreach (jDataTable dt in jds) sm.Tables.Add(dt.JsonModel());
                return sm;
            }
        }

        public static jDataModel JsonModel(this DataTable dt)
        {
            using (jDataTable jdt = new jDataTable(dt))
            {
                var m = new jDataModel();
                m.TableName = jdt.TableName;
                m.Columns = jdt.Columns;
                m.Rows = new List<Dictionary<string, object>>();
                foreach (jDataRow dr in jdt.Rows) m.Rows.Add(dr);
                return m;
            }
        }

        public static jDataRow JsonModel(this DataRow dr)
        {
            return new jDataRow(dr);
        }

        public static List<Dictionary<string, object>> JsonModel(this DataRowCollection dr)
        {
            List<Dictionary<string, object>> j = new List<Dictionary<string, object>>();
            foreach (DataRow d in dr)
            {
                var g = new Dictionary<string, object>();
                foreach (DataColumn dc in d.Table.Columns)
                    g.Add(dc.ColumnName, d[dc.ColumnName]);
                j.Add(g);
            }
            return j;
        }

        public static string PairModel(this DataRow dr)
        {
            using (Pair p = new Pair(dr)) return p.Serialize();
        }

        public static string PairModel(this DataTable dt)
        {
            using (PairCollection pc = new PairCollection(dt)) return pc.Serialize();
        }

        public static string ToJson(this jDataModel model, bool format = false)
        {
            return JsonConvert.SerializeObject(model, format ? Formatting.Indented : Formatting.None);
        }

        public static string ToJson<T>(this List<T> model, bool format = false)
        {
            return JsonConvert.SerializeObject(model, format ? Formatting.Indented : Formatting.None);
        }

        public static Int32 ToInt32(this object obj)
        {
            return Convert.ToInt32(obj);
        }

        public static bool ToBool(this object obj)
        {
            int k;
            if (int.TryParse(obj.ToString(), out k))
            {
                return Convert.ToBoolean(k);
            }
            return Convert.ToBoolean(obj);
        }

        public static DateTime ToDate(this object obj)
        {
            return Convert.ToDateTime(obj);
        }

        public static string ToDate(this object obj, string Format)
        {
            return obj.ToDate().ToString(Format);
        }

        /// <summary>
        /// All serializable class can convert
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string ToJson<T>(this T model, bool format = false)
        {
            return JsonConvert.SerializeObject(model, format ? Formatting.Indented : Formatting.None);
        }

        public static string ToJson(this jDataRow model, bool format = false)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(model, format ? Formatting.Indented : Formatting.None);
        }

        /// <summary>
        /// Convert Json to Object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public static T ToObject<T>(this string model)
        {
            if (string.IsNullOrEmpty(model))
                return default(T);

            return JsonConvert.DeserializeObject<T>(model);
        }

        /// <summary>
        /// Convert Json Object to Explicit Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public static T ToObject<T>(this object model)
        {
            if (model == null)
                return default(T);
            return model.ToString().ToObject<T>();
        }

        /// <summary>
        /// Cast Replacement as Safe Nullable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T To<T>(this object obj) where T : class
        {
            return obj as T;
        }

        public static T JsonCast<T>(this object model)
        {
            return JsonConvert.DeserializeObject<T>(model.ToString());
        }

        /// <summary>
        /// Save bytes to file it was created for
        /// </summary>
        /// <param name="_ByteArray"></param>
        /// <param name="_FileName"></param>
        /// <returns></returns>
        public static bool ToFile(this byte[] _ByteArray, string _FileName)
        {
            try
            {
                using (System.IO.FileStream _FileStream =
                      new System.IO.FileStream(_FileName, System.IO.FileMode.Create,
                                               System.IO.FileAccess.Write))
                {

                    _FileStream.Write(_ByteArray, 0, _ByteArray.Length);
                    _FileStream.Close();

                    return true;
                }
            }
            catch (Exception _Exception)
            {
                // Error
                Console.WriteLine("Exception caught in process: {0}",
                                  _Exception.ToString());
            }

            // error occured, return false
            return false;
        }

        public static int IndexOf(this byte[] searchWithin, byte[] serachFor, int startIndex)
        {
            int index = 0;
            int startPos = Array.IndexOf(searchWithin, serachFor[0], startIndex);

            if (startPos != -1)
            {
                while ((startPos + index) < searchWithin.Length)
                {
                    if (searchWithin[startPos + index] == serachFor[index])
                    {
                        index++;
                        if (index == serachFor.Length)
                        {
                            return startPos;
                        }
                    }
                    else
                    {
                        startPos = Array.IndexOf<byte>(searchWithin, serachFor[0], startPos + index);
                        if (startPos == -1)
                        {
                            return -1;
                        }
                        index = 0;
                    }
                }
            }

            return -1;
        }

        public static byte[] ToByteArray(this Stream stream)
        {
            byte[] buffer = new byte[32768];
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        return ms.ToArray();
                    ms.Write(buffer, 0, read);
                }
            }
        }

        /// <summary>
        /// Auto Set Argument placeholder base on arg count set
        /// must have defualt 0 placeholder in string to set where the arguments may takes place
        /// </summary>
        /// <param name="data"></param>
        /// <param name="Count"></param>
        /// <param name="Separator"></param>
        /// <returns></returns>
        [Obsolete]
        public static string CFormat(this string data, int Count, string Separator = ", ")
        {
            List<string> std = new List<string>();
            for (int i = 0; i < Count; i++) std.Add("{" + i + "}");
            return string.Format(data, string.Join(Separator, std.ToArray()));
        }

        [Obsolete]
        public static Dictionary<T1, T2> Apnd<T1, T2>(this Dictionary<T1, T2> dct, T1 Key, T2 Value)
        {
            dct.Add(Key, Value);
            return dct;
        }

        [Obsolete]
        public static List<string> AppendLine(this List<string> lst, object Data = null)
        {
            Data = Data == null ? "" : Data.ToString();
            lst.Add($"{Data.ToString()}\r\n");
            return lst;
        }

        public static jDataSet TojDataSet(this jDataSetModel model)
        {
            using (jDataSet ds = new jDataSet())
            {
                ds.DataSetName = model.DataSetName;
                model.Tables.ForEach(x => ds.Add(new jDataTable(x)));
                return ds;
            }
        }

        public static string Join<T>(this IEnumerable<T> list, string Delimiter = ",") => string.Join(Delimiter, list);

        public static void ForEach(this DataRowCollection dr, Action<DataRow> action)
        {
            dr.Cast<DataRow>().Each(action);
        }

        public static void CopyTo<TKey, TValue>(this IDictionary<TKey, TValue> Source, IDictionary<TKey, TValue> Dest, bool ClearFirst = false)
        {
            if (ClearFirst) Dest.Clear();
            foreach (KeyValuePair<TKey, TValue> s in Source)
            {
                Dest.Add(s);
            }
        }

        #region  Reflection 

        /// <summary>
        /// LJ 20160105
        /// Set class properties coming from Result Model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Model"></param>
        /// <returns></returns>
        public static List<T> ModelToObject<T>(this jDataModel Model) where T : class
        {
            var k = new List<T>();
            foreach (var rw in Model.Rows)
            {
                T h = Activator.CreateInstance<T>();
                foreach (var j in typeof(T).GetProperties())
                    SetObjectProperty<T>(h, j.Name, rw[j.Name]);
                k.Add(h as T);
            }
            return k;
        }

        /// <summary>
        /// LJ 20160105
        /// Set class properties coming from Result Model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public static T ModelToObject<T>(this jDataRow model) where T : class
        {
            var k = Activator.CreateInstance<T>();
            foreach (var j in typeof(T).GetProperties())
                SetObjectProperty<T>(k, j.Name, model[j.Name]);
            return k;
        }

        /// <summary>
        /// LJ 20160120
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        [Obsolete]
        public static T ModelToObject<T>(this Dictionary<string, object> model) where T : class
        {
            var k = Activator.CreateInstance<T>();
            foreach (var j in typeof(T).GetProperties())
                SetObjectProperty<T>(k, j.Name, model[j.Name]);
            return k;
        }

        public static T ModelToObject<T>(this Dictionary<string, object> model, bool force = true) where T : class
        {
            var k = Activator.CreateInstance<T>();
            foreach (var j in typeof(T).GetProperties())
                if (force)
                    SetObjectProperty<T>(k, j.Name, model[j.Name]);
                else
                {
                    if (model.ContainsKey(j.Name))
                        SetObjectProperty<T>(k, j.Name, model[j.Name]);
                }
            return k;
        }

        /// <summary>
        /// Convert Object to Pair Data Model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Pair ToPair<T>(this T obj) where T : class
        {
            var p = new Pair();
            foreach (var j in typeof(T).GetProperties())
                p.Add(j.Name, GetObjectProperty(obj, j.Name));
            return p;
        }

        /// <summary>
        /// Convert Array to PairCollection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static PairCollection ToPairCollection<T>(this IEnumerable<T> obj) where T : class
        {
            var pp = new PairCollection();
            foreach (var o in obj)
                pp.Add(ToPair(o));
            return pp;
        }

        /// <summary>
        /// LJ 20160105
        /// Set Class property value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="theObject"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void SetObjectProperty<T>(this T theObject, string propertyName, object value)
        {
            if (value == null) return;
            Type type = theObject.GetType();
            var property = type.GetProperty(propertyName);
            var setter = property.SetMethod;
            setter.Invoke(theObject, new object[] { value.ChangeType(property.PropertyType) });
        }

        public static object GetObjectProperty<T>(this T theObject, string PropertyName)
        {
            Type type = theObject.GetType();
            var prop = type.GetProperty(PropertyName);
            var getter = prop.GetMethod;
            return getter.Invoke(theObject, null);
        }

        /// <summary>
        /// LJ 20160301
        /// Object Modeling for DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static IEnumerable<T> ModelToObject<T>(this DataTable dt) where T : class
        {
            foreach (DataRow dr in dt.Rows)
            {
                var k = Activator.CreateInstance<T>();
                foreach (var j in typeof(T).GetProperties())
                    k.SetObjectProperty(j.Name, dr[j.Name]);
                yield return k;
            }
        }

        /// <summary>
        /// Check the object property if exists by name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static bool ContainsKey<T>(this T obj, string Name) where T : class
        {
            return obj.GetType().GetProperties().Any(x => x.Name == Name);
        }

        public static void ModelToTable<T>(this DataTable dt, T ctx) where T : class
        {
            DataRow dr = dt.NewRow();
            for (int j = 0; j < typeof(T).GetProperties().Length; j++)
            {
                PropertyInfo info = typeof(T).GetProperties()[j];
                dr[info.Name] = GetObjectProperty(ctx, info.Name);
            }
            dt.Rows.Add(dr);
        }

        public static void BindingModelToTable<T>(this BindingList<T> dgBinding, DataTable dt) where T : class
        {
            dt.Rows.Clear();
            foreach (T ctx in dgBinding)
                dt.ModelToTable(ctx);
        }

        public static void ObservableToTable<T>(this ObservableCollection<T> dgCollection, DataTable dt) where T : class
        {
            dt.Rows.Clear();
            foreach (T ctx in dgCollection)
                dt.ModelToTable(ctx);
        }

        public static void TableToBindingModel<T>(this DataTable dt, BindingList<T> dgBinding) where T : class
        {
            foreach (T ctx in dt.ModelToObject<T>())
                dgBinding.Add(ctx);
        }

        public static void TableToObservableCollection<T>(this DataTable dt, ObservableCollection<T> dgCollection) where T : class
        {
            foreach (T ctx in dt.ModelToObject<T>())
                dgCollection.Add(ctx);
        }

        public static BindingList<T> CreateBindingObject<T>(this IEnumerable array) where T : class
        {
            BindingList<T> dd = new BindingList<T>();
            foreach (var j in array)
            {
                var k = Activator.CreateInstance<T>();
                foreach (var m in typeof(T).GetProperties())
                    k.SetObjectProperty(m.Name, k.GetObjectProperty(m.Name));
                dd.Add(k);
            }
            return dd;
        }

        public static ObservableCollection<T> CreateObservableCollection<T>(this IEnumerable<T> array) where T : class
        {
            var j = new ObservableCollection<T>();
            foreach (var g in array)
            {
                //Note: String is immutable so fucking remember that it cannot be created when it is instantiated
                if (typeof(T) == typeof(string))
                {
                    j.Add(g);
                }
                else
                {
                    var f = Activator.CreateInstance<T>();
                    foreach (var h in typeof(T).GetProperties())
                        f.SetObjectProperty(h.Name, g.GetObjectProperty(h.Name));
                    j.Add(f);
                }
            }
            return j;
        }

        public static T ChangeType<T>(this object value)
        {
            var t = typeof(T);

            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return default(T);
                }

                t = Nullable.GetUnderlyingType(t);
            }

            return (T)Convert.ChangeType(value, t);
        }

        public static object ChangeType(this object value, Type conversion)
        {
            var t = conversion;

            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }

                t = Nullable.GetUnderlyingType(t);
            }

            return Convert.ChangeType(value, t);
        }

        public static bool IsCollection(this Type type)
        {
            if (type.IsArray) return true;
            if (type.IsGenericType)
                return type.GetGenericTypeDefinition() == typeof(List<>);
            return false;
        }

        public static bool IsBuiltIn(this Type type)
        {
            return new Type[] {
                        typeof(Enum),
                        typeof(String),
                        typeof(Decimal),
                        typeof(DateTime),
                        typeof(DateTimeOffset),
                        typeof(TimeSpan),
                        typeof(Guid)
                    }.Contains(type) ||
                    type.IsPrimitive ||
                    Convert.GetTypeCode(type) != TypeCode.Object ||
                    (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && IsBuiltIn(type.GetGenericArguments()[0]));
        }

        /// <summary>
        /// Update to other type of object
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTO"></typeparam>
        /// <param name="fromObject"></param>
        /// <param name="toObject"></param>
        /// <param name="Condition"></param>
        public static void UpdateTo<TFrom, TTO>(this TFrom fromObject, TTO toObject, Action<IDataExpression<TFrom>> Condition) where TFrom : class
        {
            var dx = new DataExpression<TFrom>();
            Condition?.Invoke(dx);

            var h = typeof(TTO).GetProperties();
            foreach (var j in typeof(TFrom).GetProperties())
            {
                if (h.Any(x => x.Name == j.Name)) // && (cn == null || cn == true)
                {
                    if (Condition == null)
                    {
                        var val = GetObjectProperty(fromObject, j.Name);
                        SetObjectProperty(toObject, j.Name, val);
                    }
                    else if ((dx.ExpType == DataExpressionType.Include && dx.Result(j.Name)) || (dx.ExpType == DataExpressionType.Exclude && !dx.Result(j.Name)))
                    {
                        var val = GetObjectProperty(fromObject, j.Name);
                        SetObjectProperty(toObject, j.Name, val);
                    }
                }
            }
        }

        /// <summary>
        /// Update to other type of object
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTO"></typeparam>
        /// <param name="fromObject"></param>
        /// <param name="toObject"></param>
        /// <param name="Condition"></param>
        public static void UpdateTo<TFrom, TTO>(this TFrom fromObject, TTO toObject, Func<PropertyInfo, bool> Condition) where TFrom : class
        {
            var h = typeof(TTO).GetProperties();
            foreach (var j in typeof(TFrom).GetProperties())
            {
                var cn = Condition?.Invoke(j);
                if (h.Any(x => x.Name == j.Name && (cn == null || cn == true)))
                {
                    var val = GetObjectProperty(fromObject, j.Name);
                    SetObjectProperty(toObject, j.Name, val);
                }
            }
        }

        public static void UpdateTo<TFrom, TTO>(this TFrom fromObject, TTO toObject) where TFrom : class => UpdateTo(fromObject, toObject, null);

        public static T CreateIfNull<T>(this T obj) where T : class
        {
            if (obj == null)
                return Activator.CreateInstance<T>();
            else
                return obj;
        }

        public static T[] CreateIfNull<T>(this T[] obj) where T : class
        {
            if (obj == null)
                return Array.CreateInstance(typeof(T), 0) as T[];
            else
                return obj;
        }

        public static List<T> CreateIfNull<T>(this List<T> obj) where T : class
        {
            if (obj == null)
                return new List<T>();
            else
                return obj;
        }

        /// <summary>
        /// Update a single type of object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fromObject"></param>
        /// <param name="toObject"></param>
        /// <param name="Condition"></param>
        public static void Update<T>(this T fromObject, T toObject, Action<IDataExpression<T>> Condition) where T : class => UpdateTo(fromObject, toObject, Condition);

        /// <summary>
        /// Update a single type of object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fromObject"></param>
        /// <param name="toObject"></param>
        /// <param name="Condition"></param>
        public static void Update<T>(this T fromObject, T toObject, Func<PropertyInfo, bool> Condition) where T : class => UpdateTo(fromObject, toObject, Condition);

        public static void Update<T>(this T fromObject, T toObject) where T : class => UpdateTo(fromObject, toObject);

        /// <summary>
        /// This will get all posible Properties in a class, even it has a object member
        /// </summary>
        public static ObjectKeyCollection GetKeys(this Type obj)
        {
            return new ObjectKeys(obj).Keys;
        }

        /// <summary>
        /// This will get all posible Properties in a class, even it has a object member
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ObjectKeyCollection GetKeys<T>() where T : class => typeof(T).GetKeys();

        #endregion

        #region  Linq Extension

        /// <summary>
        /// Each is a Reverse loop array use Map instead
        /// Lj 20151120
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Linq"></param>
        /// <param name="action"></param>
        public static void Each<T>(this IEnumerable<T> Linq, Action<T> action)
        {
            Linq.HasRow(x =>
            {
                //reverse count, case when deletion occur
                for (int i = x.Count() - 1; i >= 0; --i)
                    action(x.ToArray()[i]);
            });
        }

        /// <summary>
        /// loop through items and update through loop and return the updated items
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="Linq"></param>
        /// <param name="Action"></param>
        /// <returns></returns>
        public static IEnumerable<TResult> Map<TIn, TResult>(this IEnumerable<TIn> Linq, Func<TIn, int, TResult> Action)
        {
            var i = 0;
            foreach (var item in Linq)
            {
                yield return Action(item, i);
                i++;
            }
        }

        /// <summary>
        /// loop through items and update through loop without index and return the updated items
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="Linq"></param>
        /// <param name="Action"></param>
        /// <returns></returns>
        public static IEnumerable<TResult> Map<TIn, TResult>(this IEnumerable<TIn> Linq, Func<TIn, TResult> Action)
        {
            return Map(Linq, (x, i) => Action(x));
        }

        /// <summary>
        /// loop through items and update through loop
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Linq"></param>
        /// <param name="Action"></param>
        public static void Map<T>(this IEnumerable<T> Linq, Action<T, int> Action)
        {
            var i = 0;
            foreach (var item in Linq)
            {
                Action(item, i);
                i++;
            }
        }

        /// <summary>
        /// loop through items and update through loop without index
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Linq"></param>
        /// <param name="Action"></param>
        public static void Map<T>(this IEnumerable<T> Linq, Action<T> Action) => Map(Linq, (x, i) => Action(x));

        public static void HasRow<T>(this IEnumerable<T> linq, Action<IEnumerable<T>> action)
        {
            if (linq.Any()) action(linq);
        }

        public static void HasRow<T>(this T[] linq, Action<T[]> action)
        {
            if (linq.Any()) action(linq);
        }

        public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(this IEnumerable<TSource> source, int size)
        {
            return Batch(source, size, x => x);
        }

        public static IEnumerable<TResult> Batch<TSource, TResult>(this IEnumerable<TSource> source, int size,
           Func<IEnumerable<TSource>, TResult> resultSelector)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (size <= 0) throw new ArgumentOutOfRangeException("size");
            if (resultSelector == null) throw new ArgumentNullException("resultSelector");
            return BatchImpl(source, size, resultSelector);
        }

        private static IEnumerable<TResult> BatchImpl<TSource, TResult>(this IEnumerable<TSource> source, int size,
            Func<IEnumerable<TSource>, TResult> resultSelector)
        {

            TSource[] bucket = null;
            var count = 0;

            foreach (var item in source)
            {
                if (bucket == null)
                {
                    bucket = new TSource[size];
                }

                bucket[count++] = item;

                // The bucket is fully buffered before it's yielded
                if (count != size)
                {
                    continue;
                }

                // Select is necessary so bucket contents are streamed too
                yield return resultSelector(bucket.Select(x => x));

                bucket = null;
                count = 0;
            }

            // Return the last bucket with all remaining elements
            if (bucket != null && count > 0)
            {
                yield return resultSelector(bucket.Take(count));
            }
        }

        /// <summary>
        /// Lj 20151127
        /// Get the values that are not exists in other comparing list
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="Tinner"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="Source"></param>
        /// <param name="Inner"></param>
        /// <param name="SourceKey"></param>
        /// <param name="InnerKey"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> NotExist<TSource, Tinner, TKey>(this IEnumerable<TSource> Source,
                IEnumerable<Tinner> Inner, Func<TSource, TKey> SourceKey, Func<Tinner, TKey> InnerKey)
        {
            if (Source == null) throw new ArgumentNullException("source");
            if (Inner == null) throw new ArgumentNullException("Inner");
            foreach (var s in Source)
            {
                if (Inner.Count() == 0)
                    yield return s;
                if (!Inner.Where(y => object.Equals(SourceKey(s), InnerKey(y))).Any())
                    yield return s;
            }
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> Source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenkeys = new HashSet<TKey>();
            TSource[] k = Source.ToArray();
            for (int i = k.Length - 1; i >= 0; --i)
                if (seenkeys.Add(keySelector(k[i])))
                    yield return k[i];
        }

        /// <summary>
        /// LJ 20160127
        /// Except a key in list
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="Source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> Source, Func<TSource, bool> keySelector)
        {
            foreach (TSource s in Source)
                if (keySelector(s) != true) yield return s;
        }

        public static IEnumerable<TSource> RemoveIndex<TSource>(this IEnumerable<TSource> Source, int Index)
        {
            TSource[] ar = Source.ToArray();
            for (int i = 0; i < ar.Length; i++)
                if (i != Index) yield return ar[i];
        }

        public static IEnumerable<TSource> Substring<TSource>(this IEnumerable<TSource> Source, int Index, int Length)
        {
            TSource[] ar = Source.ToArray();
            for (int i = 0; i < ar.Length; i++)
                if (i >= Index && ar.Length - i <= Length) yield return ar[i];
        }

        public static IEnumerable<TSource> Substring<TSource>(this IEnumerable<TSource> Source, int Index)
        {
            return Source.Substring(Index, Source.Count());
        }

        /// <summary>
        /// Merge Collection to another
        /// LJ 20160217
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TMerge"></typeparam>
        /// <param name="Source"></param>
        /// <param name="MergeData"></param>
        /// <returns></returns>
        public static IEnumerable<TResult> Merge<TSource, TMerge, TResult>(this IEnumerable<TSource> Source, IEnumerable<TMerge> MergeData, Func<TSource, IEnumerable<TMerge>, TResult> OnMerge)
        {
            foreach (TSource s in Source)
                yield return OnMerge(s, MergeData);
        }

        public static IEnumerable<TSource> Remove<TSource, TCompare>(this IEnumerable<TSource> Source, IEnumerable<TCompare> CompareList, Func<TSource, TCompare, bool> keySelector)
        {
            foreach (TSource s in Source)
            { // t,t,f
                HashSet<TSource> ss = new HashSet<TSource>();
                foreach (TCompare compare in CompareList)
                    if (keySelector(s, compare) == true) ss.Add(s);
                if (ss.Count == 0) yield return s;
            }
        }

        public static IEnumerable<TSource> Remove<TSource>(this IEnumerable<TSource> Source, Func<TSource, bool> keySelector)
        {
            TSource[] ar = Source.ToArray();
            for (int i = 0; i < ar.Length; i++)
                if (!keySelector(ar[i]))
                    yield return ar[i];
        }

        public static IEnumerable<TSource> OrderByN<TSource>(this IEnumerable<TSource> Source, Func<TSource, int> Item, int N)
        {
            List<Tuple<TSource, int, int>> d = new List<Tuple<TSource, int, int>>();
            foreach (var g in Source)
            {
                int s = Math.Abs(Item(g));
                int a = s >= N ? s - N : N - s;
                d.Add(new Tuple<TSource, int, int>(g, s, a));
            }
            foreach (var w in d.OrderBy(x => x.Item3).ThenBy(x => x.Item2))
                yield return w.Item1;
        }

        public static IEnumerable<TSource> OrderByNDesc<TSource>(this IEnumerable<TSource> Source, Func<TSource, int> Item, int N)
        {
            List<Tuple<TSource, int, int>> d = new List<Tuple<TSource, int, int>>();
            foreach (var g in Source)
            {
                int s = Math.Abs(Item(g));
                int a = s >= N ? s - N : N - s;
                d.Add(new Tuple<TSource, int, int>(g, s, a));
            }
            foreach (var w in d.OrderByDescending(x => x.Item3).ThenBy(x => x.Item2))
                yield return w.Item1;
        }

        #endregion

        #region Compression

        public static string CompressToBase64(this string data) => new LZ().compressToBase64(data); //new LZ().compressToBase64(data);

        public static string CompressToUTF16(this string data) => new LZ().compressToUTF16(data);

        public static byte[] CompressToUint8Array(this string data) => new LZ().compressToUint8Array(data);

        public static string CompressUriEncoded(this string data) => new LZ().compressToEncodedURIComponent(data);

        public static string CompressFromBase64(this string data) => new LZ().decompressFromBase64(data);

        public static string CompressFromUTF16(this string data) => new LZ().decompressFromUTF16(data);

        public static string CompressFromUInt8Array(this byte[] data) => new LZ().decompressFromUint8Array(data);

        public static string CompressFromUriEncoded(this string data) => new LZ().decompressFromEncodedURIComponent(data);

        #endregion

        [System.Diagnostics.DebuggerHidden]
        public static bool IsJson(this string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException)
                {
                    return false;
                }
                catch (Exception) //some other exception
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool IsJsonArray(this string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    JArray.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException)
                {
                    return false;
                }
                catch (Exception) //some other exception
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static IEnumerable<DataRow> Where(this DataTable datatable, Func<DataRow, bool> predicate)
        {
            return datatable.Rows.Cast<DataRow>().Where(predicate);
        }

        public static object IsNull(this object inp, object AlterVal)
        {
            return (inp == null) ? AlterVal : ((inp == DBNull.Value) ? AlterVal : inp);
        }

        public static bool IsNull<T>(this T obj)
        {
            return obj == null;
        }

        public static string SqlToJs(this string sql) => Converters.SqlToJs.Convert(sql);

        public static string GetName<T>(this Expression<T> expression)
        {
            return (expression.Body as MemberExpression ?? ((UnaryExpression)expression.Body).Operand as MemberExpression).Member.Name;
        }

        /// <summary>
        /// Returns true if the expression is null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static Func<T, bool> DefaultFilter<T>(this Func<T, bool> filter)
        {
            if (filter == null)
                return x => true;
            else
                return filter;
        }

        /// <summary>
        /// Update an array property from an object
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTo"></typeparam>
        /// <typeparam name="TFromResult"></typeparam>
        /// <typeparam name="TToResult"></typeparam>
        /// <param name="fromObject"></param>
        /// <param name="toObject"></param>
        /// <param name="ColumnFrom"></param>
        /// <param name="ColumnTo"></param>
        public static void MapReference<TFrom, TTo, TFromResult, TToResult>(this TFrom fromObject, IEnumerable<TTo> toObject, Action<TTo, TFrom> Result, Func<TTo, bool> filter = null)
        {
            if (fromObject == null) throw new ArgumentNullException("fromObject");
            if (toObject == null) throw new ArgumentNullException("toObject");

            foreach (var tod in toObject.Where(filter.DefaultFilter()))
                Result(tod, fromObject);

        }

        /// <summary>
        /// Update the target array with specified filter
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTo"></typeparam>
        /// <param name="fromObject"></param>
        /// <param name="toObject"></param>
        /// <param name="Compare"></param>
        /// <param name="Result"></param>
        public static void MapReference<TFrom, TTo>(
            this IEnumerable<TFrom> fromObject,
            IEnumerable<TTo> toObject,
            Func<TFrom, TTo, bool> Compare,
            Action<TTo, TFrom> Result)
        {
            if (fromObject == null) throw new ArgumentNullException("fromObject");
            if (toObject == null) throw new ArgumentNullException("toObject");
            if (Result == null) throw new ArgumentNullException("Result");

            foreach (var frm in fromObject)
                foreach (var tod in toObject)
                    if (Compare(frm, tod))
                        Result(tod, frm);
        }
    }
}
