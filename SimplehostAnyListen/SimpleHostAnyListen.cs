﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace socketS
{
    class S
    {
        public static void Main()
        {
            Console.WriteLine("SimpleHost Any listen");
            SocketServer();
            Console.ReadKey();
        }

        public static void SocketServer()
        {
            //ここからIPアドレスやポートの設定
            // 着信データ用のデータバッファー。
            byte[] bytes = new byte[1024];
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var addresslist in ipHostInfo.AddressList)
            {
                Console.WriteLine($"found own address:{addresslist.ToString()}");
            }
            Console.Write($"Select address to listen(0 - {ipHostInfo.AddressList.Length-1}):");
            IPAddress ipAddress = ipHostInfo.AddressList[int.Parse(Console.ReadLine())];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
            //ここまでIPアドレスやポートの設定

            //ソケットの作成
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            //通信の受け入れ準備
            listener.Bind(localEndPoint);
            listener.Listen(10);

            //通信の確立
            Socket handler = listener.Accept();


            // 任意の処理
            //データの受取をReceiveで行う。
            int bytesRec = handler.Receive(bytes);
            string data1 = Encoding.UTF8.GetString(bytes, 0, bytesRec);
            Console.WriteLine(data1);

            //大文字に変更
            data1 = data1.ToUpper();

            //クライアントにSendで返す。
            byte[] msg = Encoding.UTF8.GetBytes(data1);
            handler.Send(msg);

            //ソケットの終了
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
    }
}
