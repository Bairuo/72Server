using System;
using System.Net;

namespace PAServer {
    class BumpServer {
        static void Main (string[] args) {
            ServerNet server = new ServerNet ();
            string IP = "172.18.8.253";
            int Port = 9970;
            string input;

            Console.WriteLine ("72 Bump Server  Version 1.0");
            Console.WriteLine ("Powered by Patner Adventure Studio");
            Console.WriteLine ("\tStart\t--- Start Sever");
            Console.WriteLine ("\tClose\t--- Close Sever");
            Console.WriteLine ("\tList\t--- Display all connections");
            Console.WriteLine ("\t-h\t--- For more help");

            Console.WriteLine ();

            while (true) {
                Console.Write ("> ");
                input = Console.ReadLine ();
                if (input == "Close") {
                    server.Close ();
                } else if (input == "List") {
                    int i = 0;
                    Console.WriteLine ("Current connection number：" + server.Connnum);
                    foreach (var item in server.conns) {
                        if (item.isUse) {
                            i++;
                            IPEndPoint t = (IPEndPoint) item.socket.RemoteEndPoint;
                            Console.WriteLine (i + " " + t.Address.ToString () + " Room: " + item.roomid);
                        }

                    }
                } else if (input == "Start") {
                    server.Start (IP, Port);
                } else if (input == "Quit") {
                    return;
                } else if (input == "LS" || input == "listenSend") {
                    server.ListenSend = !server.ListenSend;
                } else if (input == "LA" || input == "ListenAccept") {
                    server.ListenAccept = !server.ListenAccept;
                } else if (input == "LRE" || input == "ListenReceiveError") {
                    server.ListenReceiveError = !server.ListenReceiveError;
                } else if (input == "") {

                } else {
                    Console.WriteLine (input + " is not a server command. See -h for help");
                }
            }

        }
    }
}