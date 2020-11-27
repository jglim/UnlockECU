using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    public class Definition
    {
        public string EcuName { get; set; }
        public int AccessLevel { get; set; }
        public int SeedLength { get; set; }
        public int KeyLength { get; set; }
        public string Provider { get; set; }
        public string Origin { get; set; }
        public List<Parameter> Parameters { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string ParamParent;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder($"Name: {EcuName} ({Origin}), Level: {AccessLevel}, Seed Length: {SeedLength}, Key Length: {KeyLength}, Provider: {Provider}");
            return sb.ToString();
            /*
            // uncomment and remove the return above to print verbose parameter data
            foreach (Parameter row in Parameters)
            {
                sb.AppendLine();
                sb.Append($"Parameter[{row.Key}] ({row.DataType}) : {row.Value}");
            }
            return sb.ToString();
            */
        }
        
    }
}
