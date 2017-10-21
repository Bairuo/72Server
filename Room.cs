using System.Collections;
using System.Collections.Generic;

namespace PAServer {
    public class Room {
        public int max = 4;
        public int num = 0;
        int[] players = new int[4] {-1, -1, -1, -1 };

        public int id;
        public int Owner = -1;
        public int prepare = 0;
        public bool isFull = false;
        public bool isUse = false;

        enum STATE { Level, Curtain, Fight, Pause };
        STATE state = STATE.Level;

        public void AddPlayer (Conn conn) {
            if (num == 0) {
                isUse = true;
                Owner = conn.id;
            }

            conn.roomid = id;
            conn.idinroom = num;
            players[num++] = conn.id;

            if (num == max - 1) {
                isFull = true;
            }
        }

        public void DelPlayer (Conn conn) {
            if (num == 0) return;
            players[num--] = -1;

            if (num == 0) isUse = false;
            conn.roomid = -1;

            isFull = false;
        }

        public void Broadcast (ProtocolBase protocol) {
            for (int i = 0; i < num; i++) {
                if (!ServerNet.instance.conns[players[i]].isUse)
                    continue;

                ServerNet.instance.Send (players[i], protocol);
            }
        }

        public void UDPBroadcastOthers (ProtocolBase protocol, int Conn_id) {
            for (int i = 0; i < num; i++) {
                if (players[i] == Conn_id || !ServerNet.instance.conns[players[i]].isUse)
                    continue;
                ServerNet.instance.UDPSend (players[i], protocol);
            }
        }

        public void BroadcastOthers (ProtocolBase protocol, int Conn_id) {
            for (int i = 0; i < num; i++) {
                if (players[i] == Conn_id || !ServerNet.instance.conns[players[i]].isUse)
                    continue;

                ServerNet.instance.Send (players[i], protocol);
            }
        }

        public void BroadcastExceptOwner (ProtocolBase protocol) {
            for (int i = 0; i < num; i++) {
                if (players[i] == Owner || !ServerNet.instance.conns[players[i]].isUse)
                    continue;

                ServerNet.instance.Send (players[i], protocol);
            }
        }

        public void Close () {
            num = 0;
            prepare = 0;
            Owner = -1;
            isFull = false;
            isUse = false;
            state = STATE.Level;
        }
    }
}