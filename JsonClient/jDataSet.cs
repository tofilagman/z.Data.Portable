
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace z.Data.JsonClient
{
    /// <summary>
    /// For String Serializer use the method JsonModel
    /// </summary>
    public class jDataSet : List<jDataTable>, IDisposable
    {
        public DataSet DataSource { get; private set; }
        
        public jDataSet(DataSet DataSource)
        {
            this.DataSource = DataSource;
            foreach (DataTable dt in this.DataSource.Tables)
            {
                this.Add(new jDataTable(dt));
            }
        }

        public jDataSet() {
            this.DataSource = new DataSet();
        }

        public string DataSetName {
            get { return this.DataSource.DataSetName; }
            set { this.DataSource.DataSetName = value; }
        }
        
        public virtual void Add(DataTable dt){
            this.DataSource.Tables.Add(dt);
        }
        
        public jDataSetModel JsonModel()
        {
            jDataSetModel sm = new jDataSetModel();
            sm.DataSetName = (DataSource != null) ? DataSource.DataSetName : "";
            sm.Tables = new List<jDataModel>();
            foreach (jDataTable dt in this) sm.Tables.Add(dt.JsonModel());
            return sm;
        }
         
        public void Dispose(bool b)
        {
            this.DataSource.Dispose();
            GC.Collect();
            GC.SuppressFinalize(this);
        }

        ~jDataSet()
        {
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }

    }
}