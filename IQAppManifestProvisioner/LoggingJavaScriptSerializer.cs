using System;
using System.Diagnostics;
using System.Web.Script.Serialization;

namespace IQAppResourceServices.ClientCommunication
{
    public class LoggingJavaScriptSerializer
    {
        public string Serialize(object instance)
        {
            try
            {
                var js = new JavaScriptSerializer();
                return js.Serialize(instance);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Serialization exception | " + instance + " | " + ex);
                return string.Empty;
            }
        }

        public object Deserialize(string json, Type t)
        {
            try
            {
                var js = new JavaScriptSerializer();
                return js.Deserialize(json, t);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Serialization exception | " + t + " | " + json + " | " + ex);
                return null;
            }
        }
    }
}