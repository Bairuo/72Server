using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace PAServer {
    public class ServerNet {
        public Socket listenfd;
        public Socket UDPsocket;
        EndPoint TempRemote;
        EndPoint UDPRemote;

        const int BUFFER_SIZE = 1024;
        public Conn[] conns;
        public int maxConn = 50;
        public int Connnum = 0;

        public static ServerNet instance;
        public ProtocolBase proto = new ProtocolBytes ();

        System.Timers.Timer timer = new System.Timers.Timer (1000);
        public long heartBeatTime = 90;

        public HandleServerMsg handleServerMsg = new HandleServerMsg ();

        public bool isUse = false;
        public bool isAlsoClient = false;

        public bool ListenSend = false;
        public bool ListenAccept = true;
        public bool ListenReceiveError = false;

        public byte[] UDPreadBuff = new byte[BUFFER_SIZE];
        byte[] UDPlenBytes = new byte[sizeof (UInt32)];

        //Room
        public Room[] rooms;

        public ServerNet () {
            instance = this;
        }

        public static bool IsUse () {
            if (instance == null) {
                return false;
            } else {
                return instance.isUse;
            }
        }

        public int NewIndex () {
            if (conns == null)
                return -1;
            for (int i = 0; i < conns.Length; i++) {
                if (conns[i] == null) {
                    conns[i] = new Conn ();
                    return i;
                } else if (conns[i].isUse == false) {
                    return i;
                }
            }
            return -1;
        }

        public int NewRoomIndex () {
            if (rooms == null)
                return -1;

            for (int i = 0; i < rooms.Length; i++) {
                if (rooms[i].isUse == false) return i;
            }

            return -1;

        }

        public bool AddRoom (Conn conn, int questid) {
            if (questid < 0 || questid >= conns.Length) {
                int RoomIndex = NewRoomIndex ();

                if (RoomIndex < 0) {
                    ClosePlayer (conn);

                    return false;
                }
                rooms[RoomIndex].AddPlayer (conn);

                IPEndPoint t = (IPEndPoint) conn.socket.RemoteEndPoint;
                Console.WriteLine ("SYSTEM\n" + t.Address + " Creat Room " + RoomIndex);
                Console.Write ("> ");

                return true;
            } else {
                if (rooms[questid].isFull) {
                    ClosePlayer (conn);

                    return false;
                } else {
                    rooms[questid].AddPlayer (conn);

                    IPEndPoint t = (IPEndPoint) conn.socket.RemoteEndPoint;
                    Console.WriteLine ("SYSTEM\n" + t.Address + " Join the Room " + questid);
                    Console.Write ("> ");

                    return true;
                }
            }

        }

        public void Start (string host, int port) {
            conns = new Conn[maxConn];
            rooms = new Room[maxConn];
            for (int i = 0; i < maxConn; i++) {
                conns[i] = new Conn ();
                conns[i].id = i;

                rooms[i] = new Room ();
                rooms[i].id = i;

            }
            listenfd = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            UDPsocket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            isUse = true;

            IPAddress ipAdr = IPAddress.Parse (host);
            IPEndPoint ipEp = new IPEndPoint (ipAdr, port);

            IPEndPoint UDPipEp = new IPEndPoint (IPAddress.Any, port - 1);
            try {
                listenfd.Bind (ipEp);
                UDPsocket.Bind (UDPipEp);

                listenfd.Listen (maxConn);

                listenfd.BeginAccept (AcceptCb, null);

                IPEndPoint t = new IPEndPoint (IPAddress.Any, 0);
                TempRemote = (EndPoint) t;
                UDPRemote = (EndPoint) t;

                UDPsocket.BeginReceiveFrom (UDPreadBuff, 0, BUFFER_SIZE, SocketFlags.None, ref TempRemote, UDPReceiveCb, UDPreadBuff);

                timer.Elapsed += new System.Timers.ElapsedEventHandler (HandleMainTimer);
                timer.AutoReset = false;
                timer.Enabled = true;
            } catch (Exception e) {
                Console.WriteLine ("SYSTEM\nServer Start Error: " + e.Message);
                Console.Write ("> ");
            }

        }

        private void UDPReceiveCb (IAsyncResult ar) {
            int count = UDPsocket.EndReceiveFrom (ar, ref UDPRemote);

            Array.Copy (UDPreadBuff, UDPlenBytes, sizeof (Int32));
            int msgLength = BitConverter.ToInt32 (UDPlenBytes, 0);

            ProtocolBase protocolBase = proto.Decode (UDPreadBuff, sizeof (Int32), msgLength);
            ProtocolBytes protocol = (ProtocolBytes) protocolBase;
            int start = 0;
            int conn_id = protocol.GetInt (start, ref start);

            string protoName = protocol.GetString (start, ref start);

            if (protoName == "A") {
                conns[conn_id].UDPRemote = UDPRemote;
                UDPsocket.BeginReceiveFrom (UDPreadBuff, 0, BUFFER_SIZE, SocketFlags.None, ref TempRemote, UDPReceiveCb, UDPreadBuff);
                return;
            } else if (protoName != "U") {
                UDPsocket.BeginReceiveFrom (UDPreadBuff, 0, BUFFER_SIZE, SocketFlags.None, ref TempRemote, UDPReceiveCb, UDPreadBuff);
                return;
            }

            string id = protocol.GetString (start, ref start);
            int forward = protocol.GetInt (start, ref start);
            bool stand = protocol.GetBool (start, ref start);

            float x = protocol.Getfloat (start, ref start);
            float y = protocol.Getfloat (start, ref start);
            float z = protocol.Getfloat (start, ref start);

            ProtocolBytes Recombine = new ProtocolBytes ();
            Recombine.AddString (protoName);
            Recombine.AddString (id);
            Recombine.AddInt (forward);
            Recombine.AddBool (stand);

            Recombine.AddFloat (x);
            Recombine.AddFloat (y);
            Recombine.AddFloat (z);

            HandleMsg (conns[conn_id], Recombine);

            Array.Copy (UDPreadBuff, sizeof (Int32) + msgLength, UDPreadBuff, 0, count);

            UDPsocket.BeginReceiveFrom (UDPreadBuff, 0, BUFFER_SIZE, SocketFlags.None, ref TempRemote, UDPReceiveCb, UDPreadBuff);
        }

        private void AcceptCb (IAsyncResult ar) {
            try {
                Socket socket = listenfd.EndAccept (ar);
                int index = NewIndex ();

                if (index < 0) {
                    socket.Close ();
                } else {
                    Conn conn = conns[index];
                    conn.Init (socket);

                    //客户端连接
                    Connnum++;
                    conn.socket.BeginReceive (conn.readBuff, conn.buffCount, conn.BuffRemain (), SocketFlags.None, ReceiveCb, conn);
                    listenfd.BeginAccept (AcceptCb, null);

                    if (ListenAccept) {
                        IPEndPoint t = (IPEndPoint) socket.RemoteEndPoint;
                        Console.WriteLine ("SYSTEM\nAccept: " + t.Address + " Current connections: " + Connnum + " Time: " + DateTime.Now.ToString ());
                        Console.Write ("> ");
                    }

                    SendSuccess (conn);

                }
            } catch (Exception e) {
                Console.WriteLine ("SYSTEM\nAccept Error: " + e.Message);
                Console.Write ("> ");
            }
        }

        private void ReceiveCb (IAsyncResult ar) {
            Conn conn = (Conn) ar.AsyncState; //获取BeginReceivec传入的Conn对象
            lock (conn) {
                try {
                    int count = conn.socket.EndReceive (ar); //获取接受的字节数
                    if (count < 0) {
                        //conn.Close();
                        return;
                    }
                    conn.buffCount += count;

                    ProcessData (conn);

                    conn.socket.BeginReceive (conn.readBuff, conn.buffCount, conn.BuffRemain (), SocketFlags.None, ReceiveCb, conn);
                } catch (Exception e) {
                    if (ListenReceiveError) {
                        Console.WriteLine ("SYSTEM\nReceive Error: " + e.Message);
                        Console.Write ("> ");
                    }

                }
            }

        }

        public void HandleMainTimer (object sender, System.Timers.ElapsedEventArgs e) {
            HeartBeat ();
            timer.Start ();
        }

        public void HeartBeat () {
            long timeNow = Sys.GetTimeStamp ();

            for (int i = 0; i < conns.Length; i++) {
                Conn conn = conns[i];
                if (conn == null) continue;
                if (!conn.isUse) continue;

                if (conn.lastTickTime < timeNow - heartBeatTime) {
                    ClosePlayer (conn);
                }
            }
        }

        //粘包分包处理
        private void ProcessData (Conn conn) {
            /******是否开始处理******/

            if (conn.buffCount < sizeof (Int32)) {
                return;
            }
            Array.Copy (conn.readBuff, conn.lenBytes, sizeof (Int32));
            conn.msgLength = BitConverter.ToInt32 (conn.lenBytes, 0);

            if (conn.buffCount < sizeof (Int32) + conn.msgLength) {
                return;
            }

            /******处理消息******/

            ProtocolBase protocol = proto.Decode (conn.readBuff, sizeof (Int32), conn.msgLength);
            HandleMsg (conn, protocol);

            /******去掉已经处理的消息******/
            int count = conn.buffCount - conn.msgLength - sizeof (Int32);
            //要复制的数据，复制开始索引，复制目标，存储开始索引，元素数目
            Array.Copy (conn.readBuff, sizeof (Int32) + conn.msgLength, conn.readBuff, 0, count);
            conn.buffCount = count;
            if (conn.buffCount > 0) {
                ProcessData (conn);
            }
        }

        //处理消息
        private void HandleMsg (Conn conn, ProtocolBase protobBase) {
            string name = protobBase.GetName ();

            MethodInfo mm = handleServerMsg.GetType ().GetMethod (name);
            if (mm == null) {
                //HandleMsg没有对应方法
                return;
            }
            Object[] obj = new object[] { conn, protobBase };
            mm.Invoke (handleServerMsg, obj);

        }

        public void UDPSend (int i, ProtocolBase protocol) {
            byte[] bytes = protocol.Encode ();
            byte[] length = BitConverter.GetBytes (bytes.Length);
            byte[] sendbuff = length.Concat (bytes).ToArray ();
            if (conns[i].UDPRemote != null) {
                UDPsocket.SendTo (sendbuff, conns[i].UDPRemote);
            }
        }

        public void Send (int i, ProtocolBase protocol) {
            if (ListenSend) {
                string name = protocol.GetName ();
                if (name != "UpdateUnitInfo" && name != "UpdateInfo") {
                    Console.WriteLine ("SYSTEM\n" + name + " Conn id " + i);
                    Console.Write ("> ");
                }

            }
            Send (conns[i], protocol);
        }

        public void Send (Conn conn, ProtocolBase protocol) {
            byte[] bytes = protocol.Encode ();
            byte[] length = BitConverter.GetBytes (bytes.Length);
            byte[] sendbuff = length.Concat (bytes).ToArray ();
            try {
                conn.socket.BeginSend (sendbuff, 0, sendbuff.Length, SocketFlags.None, null, null);
            } catch (Exception e) {
                Console.WriteLine ("SYSTEM\nSend Error: " + e.Message);
                Console.Write ("> ");
            }
        }

        public void Broadcast (ProtocolBase protocol) {
            for (int i = 0; i < conns.Length; i++) {
                if (!conns[i].isUse)
                    continue;
                //if(conns[i].player == null)

                Send (conns[i], protocol);
            }
        }

        //发送协议部分

        public void SendSuccess (Conn conn) {
            ProtocolBytes proto = new ProtocolBytes ();
            proto.AddString ("Success");
            Send (conn, proto);
        }

        public void SendID (Conn conn) {
            ProtocolBytes proto = new ProtocolBytes ();
            proto.AddString ("ID");
            proto.AddInt (conn.idinroom);
            proto.AddInt (conn.roomid);
            proto.AddInt (conn.id);
            Send (conn, proto);
        }

        public int GetRoomNum(Conn conn){
            return rooms[conn.roomid].num;
        }

        public void SendRoomNum(int roomid, int num){
            ProtocolBytes proto = new ProtocolBytes();
            proto.AddString("RoomNum");
            proto.AddInt(num);
            rooms[roomid].Broadcast(proto);
        }

        public void SendRoomNum (Conn conn) {
            ProtocolBytes proto = new ProtocolBytes ();
            proto.AddString ("RoomNum");
            int num = rooms[conn.roomid].num;
            proto.AddInt (num);
            rooms[conn.roomid].Broadcast (proto);
        }

        //关闭服务器
        public void Close () {
            for (int i = 0; i < conns.Length; i++) {
                Conn conn = conns[i];
                if (conn == null) continue;
                if (!conn.isUse) continue;

                ClosePlayer (conn);

            }
            lock (listenfd) {
                listenfd.Close ();
            }

            isUse = false;
        }

        public void ClosePlayer (Conn conn) {
            lock (conn) {
                Connnum--;
                if (conn.roomid >= 0) rooms[conn.roomid].DelPlayer (conn);
                conn.Close ();
            }
        }

        public string GetlocalIp () {
            IPHostEntry ipHostInfo = Dns.Resolve (Dns.GetHostName ());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            string ipStr = ipAddress.ToString ();
            return ipStr;
        }
    }
}