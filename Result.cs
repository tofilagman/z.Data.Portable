using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
 
namespace z.Data //.Models
{
    [DataContract]
    [StructLayout(LayoutKind.Auto)]
    public struct Result : IDisposable
    { 
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