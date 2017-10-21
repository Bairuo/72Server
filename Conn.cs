using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

namespace PAServer {
    public class Conn {

        public const int BUFFER_SIZE = 1024;
        public Socket socket;
        public EndPoint UDPRemote = null;
        public bool isUse = false;

        public byte[] readBuff = new byte[BUFFER_SIZE];
        public int buffCount = 0;
        public byte[] lenBytes = new byte[sizeof (UInt32)];
        public Int32 msgLength = 0;

        // 心跳时间
        public long lastTickTime = long.MinValue;
        //对应的Player
        public int id = 0;
        public int idinroom = 0;
        public int roomid = -1;

        public Conn () {
            readBuff = new byte[BUFFER_SIZE];
        }

        public void Init (Socket socket) {
            this.socket = socket;
            isUse = true;
            buffCount = 0;
            //心跳处理
            lastTickTime = Sys.GetTimeStamp ();
        }

        public int BuffRemain () {
            return BUFFER_SIZE - buffCount;
        }

        public string GetAdress () {

            return socket.RemoteEndPoint.ToString();
        }

        public void Close () {
            if (!isUse)
                return;
            //玩家退出处理

            if (ServerNet.instance.ListenAccept) {
                IPEndPoint t = (IPEndPoint) socket.RemoteEndPoint;
                Console.WriteLine ("SYSTEM\nLeave: " + t.Address + " Current connections: " + ServerNet.instance.Connnum + " Time: " + DateTime.Now.ToString ());
                Console.Write ("> ");
            }
            UDPRemote = null;
            socket.Shutdown (SocketShutdown.Both);
            
            socket.Close ();
            isUse = false;
        }

    }
}