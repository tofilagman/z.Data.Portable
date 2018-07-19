using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace z.Data
{
    /// <summary>
    /// This will get all posible Properties in a class, even it has a object member
    /// </summary>
    public class ObjectKeys
    {
        public ObjectKeyCollection Keys { get; private set; }

        public ObjectKeys(Type obj)
        {
            this.Keys = new ObjectKeyCollection();
            this.Build(obj);
        }

        public void Build(Type obj)
        {
            foreach (var j in obj.GetProperties())
            {
                if (j.PropertyType.IsBuiltIn())
                    this.Keys.Add(new ObjectKey { Name = j.Name, Type = j.PropertyType });
                else
                    Build(j.PropertyType);
            }
        }
    }

    public class ObjectKey
    {
        public string Name { get; set; }
        public Type Type { get; set; }
    }

    public class ObjectKeyCollection : List<ObjectKey>
    {
        public bool Contains(string Name)
        {
            return this.Any(x => x.Name == Name);
        }
    }
}
