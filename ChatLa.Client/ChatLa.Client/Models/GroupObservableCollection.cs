using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
namespace ChatLa.Client.Models
{
    public class GroupObservableCollection : ObservableCollection<object>
    {
        public GroupObservableCollection(IEnumerable<object> collection) : base(collection)
        {
            Heading = "";
        }
        public string Heading { get; set; }
    }
}
