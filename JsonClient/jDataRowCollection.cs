
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace z.Data.JsonClient
{
    public class jDataRowCollection : List<jDataRow>, IDisposable
    {
        private DataRowCollection DataSource;

        public jDataRowCollection() { }

        public jDataRowCollection(List<Dictionary<string, object>> drs)
        {
            Rebuild(drs);
        }

        public jDataRowCollection(string SourceString)
        {
            var j = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(SourceString);
            Rebuild(j);
        }

        public jDataRowCollection(DataRowCollection dr, DataColumnCollection dc)
        {
            this.DataSource = dr;
            foreach (DataRow idr in dr)
            {
                var d = new jDataRow();
                foreach (DataColumn c in dc)
                {
                    d.Add(c.ColumnName, idr[c.ColumnName]);
                }
                this.Add(d);
            }
        }

        void Rebuild(List<Dictionary<string, object>> drs)
        {
            foreach (Dictionary<string, object> dr in drs)
            {
                var d = new jDataRow();
                foreach (KeyValuePair<string, object> idr in dr)
                {
                    d.Add(idr.Key, (idr.Value == null) ? DBNull.Value : idr.Value);
                }
                this.Add(d);
            }
        }

        public void UpdateDataTable(DataTable dt)
        {
            dt.Rows.Clear();
            foreach (jDataRow row in this)
            {
                DataRow dr = dt.NewRow();
                foreach (KeyValuePair<string, object> jl in row)
                {
                    var dc = dt.Columns[jl.Key];
                    if (dc == null) continue;
                    if (jl.Value.ToString().Equals(""))
                    {
                        switch (dc.DataType.Name.ToLower())
                        {
                            case "datetime": dr[jl.Key] = DBNull.Value;
                                break;
                            case "decimal":
                            case "int32":
                                if (dc.AllowDBNull) dr[jl.Key] = DBNull.Value;
                                else dr[jl.Key] = 0;
                                break;
                            default:
                                if (dc.AllowDBNull) dr[jl.Key] = DBNull.Value;
                                else dr[jl.Key] = jl.Value;
                                break;
                        }
                    }
                    else
                    {
                        dr[jl.Key] = (jl.Value.ToString() == "NULL") ? DBNull.Value : jl.Value;
                    }
                }
                    
                dt.Rows.Add(dr);
            }
        }

        public void NewRow(jDataColumn dc)
        {
            var d = new jDataRow();
            foreach (string c in dc.ColumnNames)
            {
                if (c == "ID")
                {
                    d.Add(c, 0);
                }
                else
                {
                    d.Add(c, DBNull.Value);
                }
            }
            this.Add(d);
        }

        public void Dispose(bool b)
        {
            this.DataSource = null;
            GC.Collect();
            GC.SuppressFinalize(this);
        }

        ~jDataRowCollection()
        {
            Dispose(true);
        }

        void IDisposable.Dispose()
        {
            Dispose(false);
        }

    }
}