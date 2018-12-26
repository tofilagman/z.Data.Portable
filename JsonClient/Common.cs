using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace z.Data.JsonClient
{
    public static class Common
    {

        //public static DataSet ToDataSet(string data)
        //{
        //    return Newtonsoft.Json.JsonConvert.DeserializeObject<DataSet>(data);
        //}

        public static DataTable ToDataTable(string data)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DataTable>(data);
        }

        //public static string ToString(DataSet data)
        //{
        //    return Newtonsoft.Json.JsonConvert.SerializeObject(data);
        //}

        public static string ToString(DataTable data)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(data);
        }

        public static T ToSomething<T>(string data) where T : class
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data);
        }

        public static Type GetjType(string pname)
        {
            Type tp = typeof(object);

            switch (pname)
            {
                case "string":
                    tp = typeof(string); break;
                case "int":
                case "int32":
                    tp = typeof(int);
                    break;
                case "boolean":
                case "bool":
                    tp = typeof(bool);
                    break;
                case "date":
                case "datetime":
                    tp = typeof(DateTime);
                    break;
                case "long":
                case "int64":
                    tp = typeof(Int64);
                    break;
                case "decimal":
                    tp = typeof(decimal);
                    break;
                case "float":
                    tp = typeof(float);
                    break;
            }

            return tp;
        }

    }
}
