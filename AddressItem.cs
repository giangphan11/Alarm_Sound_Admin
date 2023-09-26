using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundClient
{
    class AddressItem
    {
        public String ip { get; set; }
        public String name { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            AddressItem other = (AddressItem)obj;
            return this.ip == other.ip && this.name == other.name;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return ip.GetHashCode() ^ name.GetHashCode();
        }
    }
}
