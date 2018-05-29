using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace z.Data.JsonClient
{
  public class jDataSetModel
    {

      [DataMember]
      public string DataSetName { get; set; }

      [DataMember]
      public List<jDataModel> Tables;

    }
}
