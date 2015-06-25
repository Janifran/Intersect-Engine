﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Intersect_Client.Classes
{
    public static class Network
    {
        public static TcpClient MySocket;
        private static NetworkStream _myStream;
        public static bool Connected;
        public static bool Connecting;
        private static byte[] _tempBuff;
        private static ByteBuffer _myBuffer = new ByteBuffer();
        private static Object _bufferLock = new Object();

        public static void InitNetwork()
        {
            if (MySocket != null)
            {
                MySocket.Close();
            }

            MySocket = new TcpClient { NoDelay = true };
            _tempBuff = new byte[MySocket.ReceiveBufferSize];
            MySocket.BeginConnect(Globals.ServerHost, Globals.ServerPort, ConnectCb, null);
            Connecting = true;
        }

        private static void ConnectCb(IAsyncResult result)
        {
            try
            {
                MySocket.EndConnect(result);
                if (MySocket.Connected)
                {
                    Connected = true;
                    Connecting = false;
                    _myStream = MySocket.GetStream();
                    _myStream.BeginRead(_tempBuff, 0, MySocket.ReceiveBufferSize, ReceiveCb, null);
                }
                else
                {
                    Connected = false;
                    Connecting = false;
                }
            }
            catch (Exception)
            {
                Connected = false;
                Connecting = false;
            }
        }

        public static void CheckNetwork()
        {
            if (Connected == false && Connecting == false)
            {
                InitNetwork();
            }
            else
            {
                if (!Connected)
                {
                    //PROBLEM!
                }
                else
                {
                    TryHandleData();
                }
            }
        }

        private static void ReceiveCb(IAsyncResult result)
        {
            try
            {
                var readAmt = _myStream.EndRead(result);
                if (readAmt <= 0)
                {
                    HandleDc();
                }
                var receivedData = new byte[readAmt];
                Buffer.BlockCopy(_tempBuff, 0, receivedData, 0, readAmt);
                lock (_bufferLock)
                {
                    _myBuffer.WriteBytes(receivedData);
                }
                _myStream.BeginRead(_tempBuff, 0, MySocket.ReceiveBufferSize, ReceiveCb, null);
            }
            catch (Exception)
            {
                HandleDc();
            }
        }

        public static void DestroyNetwork()
        {
            try
            {
                _myStream.Close();
                MySocket.Close();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private static void HandleDc()
        {
            MessageBox.Show(@"Disconnected!");
            GameMain.IsRunning = false;
        }

        public static void SendPacket(byte[] data)
        {
            try
            {
                var buff = new ByteBuffer();
                buff.WriteInteger(data.Length);
                buff.WriteBytes(data);
                _myStream.Write(buff.ToArray(), 0, buff.Count());
            }
            catch (Exception)
            {
                HandleDc();
            }
        }

        private static void TryHandleData()
        {
            lock (_bufferLock)
            {
                while (_myBuffer.Length() >= 4)
                {
                    var packetLen = _myBuffer.ReadInteger(false);
                    if (_myBuffer.Length() >= packetLen + 4)
                    {
                        _myBuffer.ReadInteger();
                        PacketHandler.HandlePacket(_myBuffer.ReadBytes(packetLen));
                    }
                    else
                    {
                        break;
                    }
                }
                if (_myBuffer.Length() == 0)
                {
                    _myBuffer.Clear();
                }
            }
        }
    }
}