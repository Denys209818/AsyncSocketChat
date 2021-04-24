using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AsyncSocketChatClient.Models
{
    public class DataModel
    {
        public byte[] bytes = new byte[256];
        public Socket socket { get; set; }
        public StringBuilder builder { get; set; } = new StringBuilder();
        public int countBytes { get; set; }
        public List<Socket> sockets { get; set; } = new List<Socket>();
    }
}
