## ��ο��ٲ���Server?
### ���޸�PAServer.cs��ServerNet.cs��Conn.cs
* ��������޸�PAServer.cs�˿�
* �޸�PAServer.cs�������ʾ
* �޸�ServerNet.cs `NewRoomIndex`��`AddRoom`����
* ��ServerNet.cs��Start��AcceptCb�ڼ���˵������
* ����ѡ����ServerNet.cs��ReceiveCb��Send�ڼ���˵������

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

* ��Conn.cs��Close������˵������

#### Close

```
        if (ServerNet.instance.ListenAccept) {
            IPEndPoint t = (IPEndPoint) socket.RemoteEndPoint;
            Console.WriteLine ("SYSTEM\nLeave: " + t.Address + " Current connections: " + ServerNet.instance.Connnum + " Time: " + DateTime.Now.ToString ());
            Console.Write ("> ");
        }
```

### �������ļ�ȫ������
### �۰����Ʒ�������ȫ������
* ���Ź����뷽��TCP/UDPָ���˿ڣ�Ĭ����ͬ��

### �ܿͻ����߼��ο�
```
    public void StartServer()
    {
        if (ClientGroup[0].active)
        {
            Information.text = "�����˳�������Ϸ�ٴ���������";
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
            Information.text = "�����Ѿ�������Ϸ�򴴽���������";
        }
    }
    public void StartRoom()
    {
        if (ClientGroup[0].active)
        {
            Information.text = "�����˳�������Ϸ�ٴ�������";
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
            Information.text = "�����Ѿ�������Ϸ�򴴽����䣡";
        }
    }
    public void StartAddRoom()
    {
        if (ServeGroup[0].active)
        {
            Information.text = "�����˳�������ģʽ";
            return;
        }
        AddTip.text = "����ţ�";
        server_type = 1;
        Client.instance.roomnum = 0;

        if (ClientGroup[0].active == false)
        {
            for (int i = 0; i < ClientGroup.Length; i++)
            {
                ClientGroup[i].SetActive(true);
            }

            Information.text = "�����뷿��ţ����ٴε��������Ϸ";
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
                Information.text = "�����Ѿ�������Ϸ�򴴽���������";
            }
        }
    }
    public void StartConnect()
    {
        if (ServeGroup[0].active)
        {
            Information.text = "�����˳�������ģʽ";
            return;
        }

        AddTip.text = "����IP��";
        Client.instance.questroom = "-1";
        server_type = 0;

        if (ClientGroup[0].active == false)
        {
            for (int i = 0; i < ClientGroup.Length; i++)
            {
                ClientGroup[i].SetActive(true);
            }

            Information.text = "�����������IP�����ٴε��������Ϸ";
        }
        else
        {
            if (!Client.IsUse() && !ServerNet.IsUse())
            {
                Client client = new Client();
                if (client.Connect(ServeIP.text, Port) == false)
                {
                    Information.text = "������Ϸʧ�ܣ���ȷ�Ϸ�����IP";
                }
            }
            else
            {
                Information.text = "�����Ѿ�������Ϸ�򴴽���������";
            }
        }
    }
    public void Disconnect()
    {
        //Network.Disconnect();
        Information.text = "";
        IPdisplay.text = "������ַ�� ";
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