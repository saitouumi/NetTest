﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ChatSystem
{
    class ChatSystem
    {
        private string _hostName;
        public string hostName
        {
            get => _hostName;
        }
        public enum ConnectMode { host = 0, client = 1 };
        private ConnectMode _connectMode;
        public ConnectMode connectMode
        {
            get => _connectMode;
        }

        private IPAddress _ipAddress;
        private Int32 _portNo;
        private IPEndPoint _localEndPoint;
        private Socket _connectSocet = null;
        private Socket _chatSocket = null;
        private int _maxChatLength;

        // termination of processes
        public enum EResult { success, exception, socketException, argumentOutOfRangeException, notInit };
        EResult eResult = EResult.success;
        public Exception exception;
        public SocketException socketException;
        public ArgumentOutOfRangeException argumentOutOfRangeException;
        public string resultMessage
        {
            get
            {
                string s = null;
                switch (eResult)
                {
                    case EResult.success:
                        s = "success";
                        break;
                    case EResult.exception:
                        s = "Exception:" + exception.Message;
                        break;
                    case EResult.socketException:
                        s = "SocketException" + socketException.Message;
                        break;
                    case EResult.argumentOutOfRangeException:
                        s = "ArgumentOutOfRangeException:" + argumentOutOfRangeException.Message;
                        break;
                    case EResult.notInit:
                        s = "Not Initialized";
                        break;
                    default:
                        break;
                }
                return s;
            }
        }
        public class Buffer
        {
            public int length;
            public int capacity;
            public byte[] content;
            public Buffer(int c)
            {
                capacity = c;
                content = new byte[capacity];
                length = 0;
            }
        }


        public ChatSystem(int maxChatLength = 0)
        {
            _maxChatLength = maxChatLength;
            _hostName = Dns.GetHostName();
        }
        /// <summary>
        /// Initialize as a Host
        /// </summary>
        /// <param name="ipAddress">ip Addressa</param>
        /// <param name="portNo"> port No</param>
        public EResult InitializeHost(IPAddress ipAddress, Int32 portNo)
        {
            _connectMode = ConnectMode.host;
            _ipAddress = ipAddress;
            _portNo = portNo;
            _localEndPoint = new IPEndPoint(ipAddress, portNo);
            //接続のためのソケットを作成
            _connectSocet = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            //通信の受け入れ準備
            try
            {
                _connectSocet.Bind(_localEndPoint);
            }
            catch (Exception e)
            {
                exception = e;
                return eResult = EResult.exception;
            }
            try
            {
                _connectSocet.Listen(10);
            }
            catch (Exception e)
            {
                exception = e;
                return eResult = EResult.exception;
            }
            //通信の確立
            try
            {
                _chatSocket = _connectSocet.Accept();
            }
            catch (Exception e)
            {
                exception = e;
                return eResult = EResult.exception;
            }
            return eResult = EResult.success;
        }
        /// <summary>
        /// Initialize as a Client
        /// </summary>
        /// <param name="ipAddress">ipAddress</param>
        /// <param name="portNo">portNo</param>
        /// <param name="e">Exception</param>
        /// <returns>bool result</returns>
        public EResult InitializeClient(IPAddress ipAddress, Int32 portNo)
        {
            _connectMode = ConnectMode.client;
            _ipAddress = ipAddress;
            _portNo = portNo;
            _localEndPoint = new IPEndPoint(ipAddress, portNo);
            //ソケットを作成
            _connectSocet = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            //接続する。失敗するとエラーで落ちる。
            try
            {
                _connectSocet.Connect(_localEndPoint);
            }
            catch (Exception e)
            {
                exception = e;
                return eResult = EResult.exception;
            }
            _chatSocket = _connectSocet;
            return eResult = EResult.success;
        }
        /// <summary>
        /// Receive connected Socket
        /// </summary>
        /// <returns>Suceed ,received string or ErrorMessage</returns>
        public EResult Receive(Buffer buffer)
        {
            if (_chatSocket != null)
            {   // 初期化済み
                int bytesRec;
                try
                {
                    bytesRec = _chatSocket.Receive(buffer.content, buffer.capacity, SocketFlags.None);
                }
                catch (SocketException e)
                {
                    _chatSocket = null;
                    _connectSocet = null;
                    socketException = e;
                    return eResult = EResult.socketException;
                }
                catch (ArgumentOutOfRangeException e)
                {
                    argumentOutOfRangeException = e;
                    return eResult = EResult.argumentOutOfRangeException;
                }
                // 正常に受信
                return eResult = EResult.success;
            }
            else
            {
                return eResult = EResult.notInit;
            }
        }
        public EResult Send(Buffer buffer)
        {
            try
            {
                _chatSocket.Send(buffer.content, buffer.length, SocketFlags.None);
            }
            catch (SocketException e)
            {   //ソケットへのアクセスを試行しているときにエラーが発生しました。
                _chatSocket = null;
                _connectSocet = null;
                socketException = e;
                return eResult = EResult.socketException;
            }
            /*
            catch (ArgumentNullException e)
            {
                throw;
            }
            catch (ObjectDisposedException e)
            {
                throw;
            }
            */
            return eResult = EResult.success;
        }

        public void ShutDownColse()
        {
            if (_connectSocet != null)
            {
                try
                {
                    _connectSocet.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException)
                {

                }
                _connectSocet.Close();
                _connectSocet = null;
            }
            return;
        }
    }

}
