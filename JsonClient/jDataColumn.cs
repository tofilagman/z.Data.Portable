using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace z.Data.JsonClient
{
    public class jDataColumn : List<Dictionary<string, object>>, IDisposable
    {
        private DataColumnCollection DataSource;

        public jDataColumn() { }

        public jDataColumn(List<Dictionary<string, object>> dc)
        {
            this.DataSource = null;

            foreach (Dictionary<string, object> c in dc)
            {
                var d = new Dictionary<string, object>();
                foreach (KeyValuePair<string, object> cl in c)
                {
                    d.Add(cl.Key, (cl.Value == null) ? DBNull.Value : cl.Value);
                }
                this.Add(d);
            }
        }

        public jDataColumn(DataColumnCollection dc)
        {
            this.DataSource = dc;
            foreach (DataColumn c in dc) this.Add(c);
        }

        public List<string> ColumnNames
        {
            get
            {
                var jl = new List<string>();
                foreach (Dictionary<string, object> j in this) jl.Add(j["ColumnName"].ToString());
                return jl;
            }
        }

        public void UpdateDataTable(DataTable dt)
        {
            dt.Columns.Clear();
            foreach (Dictionary<string, object> fg in this)
            {
                DataColumn dc = new DataColumn();
                dc.ColumnName = fg["ColumnName"].ToString();
                dc.DataType = Common.GetjType(fg["DataType"].ToString());
                dc.DefaultValue = fg["DefaultValue"];
                dc.Caption = fg["Caption"].ToString();
                dt.Columns.Add(dc);
            }
        }

        public void Add(DataColumn c)
        {
            var d = new Dictionary<string, object>();
            d.Add("ColumnName", c.ColumnName);
            d.Add("Ordinal", c.Ordinal);
            d.Add("DataType", c.DataType.Name.ToLower());
            d.Add("DefaultValue", c.DefaultValue);
            d.Add("Caption", c.Caption);
            this.Add(d);
        }

       

        ~jDataColumn()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            DataSource = null;
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}
