using System;
using System.Collections.Generic;
using System.Text;

namespace z.Data.Models
{
    public class GroupItemList<TElement> : List<GroupItem<TElement>>
    {
        public void Add(string Name, Func<TElement, object> Query)
        {
            base.Add(new GroupItem<TElement>
            {
                Name = Name,
                Query = Query
            });
        }
    }

    public class GroupItem<TElement>
    {
        public string Name { get; set; }
        public Func<TElement, object> Query { get; set; }
    }

    public class GroupResult<Telement>
    {
        public string Name { get; set; }
        public object Key { get; set; }

        public int Count { get; set; }

        public IEnumerable<Telement> Items { get; set; }

        public IEnumerable<GroupResult<Telement>> SubGroups { get; set; }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Key, Count);
        }
    }
}
