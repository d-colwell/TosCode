using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TosCode
{
    public class InstanceManager
    {
        private static InstanceManager _instance;

        private IDictionary<string, object> primaryObjects = new Dictionary<string,object>();

        public static InstanceManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new InstanceManager();
                return _instance;
            }
        }

        public object GetInstance(Type type)
        {
            if (!primaryObjects.ContainsKey(type.FullName))
            {
                var obj = Activator.CreateInstance(type);
                primaryObjects.Add(type.FullName, obj);
            }

            return primaryObjects[type.FullName];
        }

    }
}
