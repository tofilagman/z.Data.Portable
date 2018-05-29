
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace z.Data.JsonClient
{
    /// <summary>
    /// Please Note JSONModel Method if need a json return instance
    /// this class cannot serialize alone
    /// </summary>
    [DataContract]
    public class jDataTable : IDisposable
    {
        [DataMember]
        public string TableName { get; private set; }

        [DataMember]
        public jDataColumn Columns { get; private set; }

        [DataMember]
        public jDataRowCollection Rows { get; private set; }

        private DataTable DataSource;

        public jDataTable() : this("Table")
        { }

        public jDataTable(string TableName)
        {
            this.DataSource = new DataTable(TableName);
            this.Columns = new jDataColumn();
            this.Rows = new jDataRowCollection();
            this.TableName = TableName;
        }

        public jDataTable(DataTable dt)
        {
            try
            {
                this.TableName = dt.TableName;
                this.DataSource = dt;
                this.Columns = new jDataColumn(dt.Columns);
                this.Rows = new jDataRowCollection(dt.Rows, dt.Columns);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public jDataTable(jDataModel model)
        {
            try
            {
                this.TableName = model.TableName;
                this.Columns = new jDataColumn(model.Columns);
                this.Rows = new jDataRowCollection(model.Rows);
                this.SetDataSource();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, object> ToDictionary()
        {
            var d = new Dictionary<string, object>();
            d.Add("TableName", this.TableName);
            d.Add("Columns", this.Columns);
            d.Add("Rows", this.Rows);
            return d;
        }

        public void parseJSON(string dt)
        {
            jDataModel rdt = Newtonsoft.Json.JsonConvert.DeserializeObject<jDataModel>(dt);
            this.TableName = rdt.TableName;
            this.Columns = new jDataColumn(rdt.Columns);
            this.Rows = new jDataRowCollection(rdt.Rows);

            DataSource = new DataTable(this.TableName);
            this.Columns.UpdateDataTable(DataSource);
            this.Rows.UpdateDataTable(DataSource);
        }

        protected void SetDataSource()
        {
            this.DataSource = new DataTable(this.TableName);
            this.Columns.UpdateDataTable(this.DataSource);
            this.Rows.UpdateDataTable(this.DataSource);
        }

        public DataTable DataTable
        {
            get
            {
                return DataSource;
            }
        }

        /// <summary>
        /// Must Call if need to use as JSON Object
        /// </summary>
        /// <returns></returns>
        public jDataModel JsonModel()
        {
            var m = new jDataModel();
            m.TableName = this.TableName;
            m.Columns = this.Columns;
            m.Rows = new List<Dictionary<string, object>>();
            foreach (jDataRow dr in this.Rows) m.Rows.Add(dr);
            return m;
        }

        ~jDataTable()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (this.DataSource != null) this.DataSource.Dispose();
            if (this.Columns != null) this.Columns.Dispose();
            if (this.Rows != null) this.Rows.Dispose(true);
            GC.Collect();
            GC.SuppressFinalize(this);
        }

    }
}