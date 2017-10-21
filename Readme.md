## 如何快速部署Server?
### ①修改PAServer.cs，ServerNet.cs，Conn.cs
* 根据情况修改PAServer.cs端口
* 修改PAServer.cs里面的显示
* 修改ServerNet.cs `NewRoomIndex`和`AddRoom`方法
* 在ServerNet.cs的Start，AcceptCb内加入说明代码
* 【可选】在ServerNet.cs的ReceiveCb，Send内加入说明代码

#### NewRoomIndex
```
        public int NewRoomIndex () {
            if (rooms == null)
                return -1;

            for (int i = 0; i < rooms.Length; i++) {
                if (rooms[i].isUse == false) return i;
            }

            return -1;
        }
```


#### AddRomm

```
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
    
```

#### Start
```
        } catch (Exception e) {
            Console.WriteLine ("SYSTEM\nServer Start Error: " + e.Message);
            Console.Write ("> ");
        }
```

#### AcceptCb
```
        } catch (Exception e) {
            Console.WriteLine ("SYSTEM\nAccept Error: " + e.Message);
            Console.Write ("> ");
        }
```

#### ReceiveCb
```
        } catch (Exception e) {
            if (ListenReceiveError) {
                Console.WriteLine ("SYSTEM\nReceive Error: " + e.Message);
                Console.Write ("> ");
            }
        }
```

#### Send
```
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
```

* 在Conn.cs内Close处加入说明代码

#### Close

```
        if (ServerNet.instance.ListenAccept) {
            IPEndPoint t = (IPEndPoint) socket.RemoteEndPoint;
            Console.WriteLine ("SYSTEM\nLeave: " + t.Address + " Current connections: " + ServerNet.instance.Connnum + " Time: " + DateTime.Now.ToString ());
            Console.Write ("> ");
        }
```

### ②其余文件全部复制
### ③阿里云服务器安全组设置
* 开放公网入方向TCP/UDP指定端口（默认相同）

### ④客户端逻辑参考
```
    public void StartServer()
    {
        if (ClientGroup[0].active)
        {
            Information.text = "请先退出多人游戏再创建服务器";
            return;
        }

        server_type = 0;
        Client.instance.questroom = "-1";

        for (int i = 0; i < ServeGroup.Length; i++)
        {
            ServeGroup[i].SetActive(true);
        }

        if (!Client.IsUse() && !ServerNet.IsUse())
        {
            ServerNet server = new ServerNet();
            server.Start(Network.player.ipAddress, Port);

            Client.instance.Connect(Network.player.ipAddress, Port);
        }
        else
        {
            Information.text = "错误：已经加入游戏或创建服务器！";
        }
    }
    public void StartRoom()
    {
        if (ClientGroup[0].active)
        {
            Information.text = "请先退出多人游戏再创建房间";
            return;
        }

        server_type = 1;
        Client.instance.questroom = "-1";

        for (int i = 0; i < ServeGroup.Length; i++)
        {
            ServeGroup[i].SetActive(true);
        }

        if (!Client.IsUse() && !ServerNet.IsUse())
        {
            Client.instance.Connect("119.23.52.136", Port);
        }
        else
        {
            Information.text = "错误：已经加入游戏或创建房间！";
        }
    }
    public void StartAddRoom()
    {
        if (ServeGroup[0].active)
        {
            Information.text = "请先退出服务器模式";
            return;
        }
        AddTip.text = "房间号：";
        server_type = 1;
        Client.instance.roomnum = 0;

        if (ClientGroup[0].active == false)
        {
            for (int i = 0; i < ClientGroup.Length; i++)
            {
                ClientGroup[i].SetActive(true);
            }

            Information.text = "请输入房间号，并再次点击加入游戏";
        }
        else
        {
            if (ServeIP.text == "") return;

            if (!Client.IsUse() && !ServerNet.IsUse())
            {
                Client client = new Client();
                Client.instance.questroom = ServeIP.text;
                client.Connect("119.23.52.136", Port);
            }
            else
            {
                Information.text = "错误：已经加入游戏或创建服务器！";
            }
        }
    }
    public void StartConnect()
    {
        if (ServeGroup[0].active)
        {
            Information.text = "请先退出服务器模式";
            return;
        }

        AddTip.text = "输入IP：";
        Client.instance.questroom = "-1";
        server_type = 0;

        if (ClientGroup[0].active == false)
        {
            for (int i = 0; i < ClientGroup.Length; i++)
            {
                ClientGroup[i].SetActive(true);
            }

            Information.text = "请输入服务器IP，并再次点击加入游戏";
        }
        else
        {
            if (!Client.IsUse() && !ServerNet.IsUse())
            {
                Client client = new Client();
                if (client.Connect(ServeIP.text, Port) == false)
                {
                    Information.text = "加入游戏失败，请确认服务器IP";
                }
            }
            else
            {
                Information.text = "错误：已经加入游戏或创建服务器！";
            }
        }
    }
    public void Disconnect()
    {
        //Network.Disconnect();
        Information.text = "";
        IPdisplay.text = "本机地址： ";
        if (ServeGroup[0].active)
        {
            for (int i = 0; i < ServeGroup.Length; i++)
            {
                ServeGroup[i].SetActive(false);
            }
        }

        if (ClientGroup[0].active)
        {
            for (int i = 0; i < ClientGroup.Length; i++)
            {
                ClientGroup[i].SetActive(false);
            }
        }

        if (ServerNet.IsUse())
            ServerNet.instance.Close();
        if (Client.IsUse())
            Client.instance.Close();
    }
```