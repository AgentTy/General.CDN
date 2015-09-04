using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace General.Model
{
    [DataContract]
    public class JsonObject
    {
        public string ToJson()
        {
            string retVal = null;
            System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(this.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, this);
                retVal = Encoding.Default.GetString(ms.ToArray());
            }

            return retVal;
        }

        public static T FromJson<T>(string json)
        {
            T obj = Activator.CreateInstance<T>();
            using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(obj.GetType());
                obj = (T)serializer.ReadObject(ms);
            }

            return obj;
        }

        public override string ToString()
        {
            return ToJson();
        }

    }
}
