using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace z.Data.JsonClient
{
    public class jDataRow : Dictionary<string, object>
    {
        
        public jDataRow()
        {
    
        }

        public jDataRow(string Source)
        {
            var h =  JsonConvert.DeserializeObject<Dictionary<string, object>>(Source);
            foreach (KeyValuePair<string, object> j in h)
            {
                this.Add(j.Key, j.Value);
            }
        }

        public jDataRow(JObject obj)
        {
            foreach (Newtonsoft.Json.Linq.JToken con in obj.Values())
            {
                if (con is JProperty)
                {
                    JProperty c  = con as JProperty;
                    this.Add(c.Name, c.Value);
                }
            }
        }

        public jDataRow(DataRow dr)
        {
            foreach (DataColumn dc in dr.Table.Columns)
            {
                this.Add(dc.ColumnName, dr[dc.ColumnName]);
            }
        }

        public string GetString(string key)
        {
            return Convert.ToString(this[key]);
        }

        public int GetInt(string key)
        {
            return Convert.ToInt32(this[key]);
        }

        public Int64 GetInt64(string key)
        {
            return Convert.ToInt64(this[key]);
        }

        public bool GetBool(string key)
        {
            return Convert.ToBoolean(this[key]);
        }

        public T Get<T>(string key)
        {
            return (T)this[key];
        }

        public DataRow ToRow()
        {
            using (var dt = new DataTable())
            {
                foreach (string k in this.Keys) dt.Columns.Add(k);
                DataRow dr = dt.NewRow();
                foreach (KeyValuePair<string, object> o in this) dr[o.Key] = o.Value;
                return dr;   
            }
        }

    }
}
