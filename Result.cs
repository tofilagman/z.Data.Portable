using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Web;

namespace z.Data //.Models
{
    [DataContract]
    [StructLayout(LayoutKind.Auto)]
    public struct Result : IDisposable
    {
        //public Result(dynamic R)
        //{
        //    this.Status = 0;
        //    this.MethodName = "";
        //    R = this;
        //}

        //public Result()
        //{
        //    this.Status = 0;
        //}

        /// <summary>
        /// Name of Procedure who calls the Result Context
        /// </summary>
        [DataMember] 
        public string MethodName { get; set; }

        [DataMember]
        public object ResultSet { get; set; }

        [DataMember, DefaultValue(0)]
        public int Status { get; set; }

        [DataMember]
        public string ErrorMsg { get; set; }

        [DataMember]
        public object Tag { get; set; }

        public void Dispose()
        {
            Tag = null;
            ResultSet = null;
            ErrorMsg = null;

            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}