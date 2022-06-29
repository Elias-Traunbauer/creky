using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrekyServer
{
    [Serializable]
    public class RequestPacket
    {
        public long Id;
        public bool foundMatch;
        public string[]? results;
    }
}
