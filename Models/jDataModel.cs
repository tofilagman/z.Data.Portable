using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace z.Data.JsonClient
{
   [Serializable]
   public class jDataModel
    {
        [DataMember]
        public string TableName { get; set; }

        [DataMember]
        public List<Dictionary<string, object>> Columns { get; set; }

        [DataMember]
        public List<Dictionary<string, object>> Rows { get; set; }
    }
}
