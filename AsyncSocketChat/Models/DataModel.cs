using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncSocketChat.Models
{
    public class DataModel
    {
        public AutoResetEvent resetEvent { get; set; } = new AutoResetEvent(false);
        public string text { get; set; }
        public Socket currentSocket { get; set; }
        public int countBytes { get; set; }
        public byte[] elements { get; set; } = new byte[256];
        public StringBuilder builder { get; set; } = new StringBuilder();
    }
}
