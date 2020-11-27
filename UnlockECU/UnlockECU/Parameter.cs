using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    public class Parameter
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string DataType { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public int AccessLevel = -1;
    }
}
