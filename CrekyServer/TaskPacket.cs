using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrekyServer
{
    [Serializable]
    public class TaskPacket
    {
        public long Id;
        public long keyStart;
        public long keyEnd;
        public byte[]? input;
    }
}
