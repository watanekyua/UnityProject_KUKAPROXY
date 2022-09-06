// 20220829

using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class TcpClient : SerializedMonoBehaviour
{
    // ▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰ Variables

    public bool autoInit = true;

    // 結束符號 /r/n 0d0a
    public bool add0X0A = true;

    public string targetAddress = "127.0.0.1";

    public int targetPort = 10000;

    public Encoding encoding = Encoding.ASCII;

    public Action<string> ReceivedString;
    public Action<byte[]> ReceivedBytes;

    private Socket clientSocket;
    private Thread connectedT;
    private Thread receiveT;

    // 連線狀態
    private StatusType status;
    public enum StatusType
    {
        Connecting,
        Connected,
        Disconnected
    }

    // Debug
    [TitleGroup("Debug")]
    public bool logSend = false;

    [TitleGroup("Debug")]
    public bool logReceive_String = false;
    [TitleGroup("Debug")]
    public bool logReceive_Bytes = false;

    // ▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰ Unity Functions

    private void Start()
    {
        status = StatusType.Disconnected;

        if (autoInit)
        {
            OnConnectedToServer();
        }
    }

    private void Update()
    {
        
    }

    private void OnDestroy()
    {
        OnDisconnect();
    }

    // ▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰ Custom Functions

    [Button]
    //連接服務器
    public void OnConnectedToServer()
    {
        if (connectedT != null)
        {
            connectedT.Interrupt();
            connectedT.Abort();
        }
        connectedT = new Thread(T_OnConnectedToServer);
        connectedT.Start();
    }

    // Tread ConnectedToServer
    private void T_OnConnectedToServer()
    {
        try
        {
            status = StatusType.Connecting;
            Debug.Log("連接TCP伺服器中...");

            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //用於連接服務器
            clientSocket.Connect(IPAddress.Parse(targetAddress), targetPort);

            status = StatusType.Connected;
            Debug.Log("連接TCP伺服器成功");

            if (receiveT != null)
            {
                receiveT.Interrupt();
                receiveT.Abort();
            }
            receiveT = new Thread(ReceiveMsg);
            receiveT.Start();
        }
        catch (System.Exception ex)
        {
            status = StatusType.Disconnected;
            Debug.Log("連接TCP伺服器失敗");
            Debug.Log(ex.Message);
        }
    }

    [Button]
    // 斷開連線
    public void OnDisconnect()
    {
        if (receiveT != null)
        {
            receiveT.Interrupt();
            receiveT.Abort();
        }

        if (connectedT != null)
        {
            connectedT.Interrupt();
            connectedT.Abort();
        }

        if (clientSocket != null)
        {
            if (clientSocket.Connected == true)
            {
                clientSocket.Shutdown(SocketShutdown.Both);
            }
            clientSocket.Close();
        }

        status = StatusType.Disconnected;
        Debug.Log("與TCP伺服器中斷");
    }

    public StatusType GetStatus()
    {
        return status;
    }

    //接收數據
    private void ReceiveMsg()
    {
        try
        {
            byte[] _data = new byte[1024];

            while (true)
            {
                if (clientSocket.Connected == false)
                {
                    status = StatusType.Disconnected;
                    Debug.Log("與服務器斷開連接");
                    break;
                }

                int length = 0;
                length = clientSocket.Receive(_data);

                if (length > 0)
                {
                    //string str = Encoding.UTF8.GetString(data, 0, data.Length);
                    //string str = encoding.GetString(data, 0, length - 2);

                    if (ReceivedString != null)
                    {
                        ReceivedString.Invoke(encoding.GetString(_data, 0, length));
                    }

                    if (ReceivedBytes != null)
                    {
                        ReceivedBytes.Invoke(_data);
                    }

                    // DebugLog
                    if (logReceive_String == true)
                    {
                        Debug.Log("[TCP Listener] " + encoding.GetString(_data, 0, length));
                    }

                    if (logReceive_Bytes == true)
                    {
                        string _bytesToStr = "";

                        for (int i = 0; i < length; i++)
                        {
                            _bytesToStr += _data[i] + " ";
                        }

                        Debug.Log("[TCP Listener] " + _bytesToStr);
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            status = StatusType.Disconnected;
            Debug.Log("與服務器斷開連接");
            Debug.Log(ex.Message);
        }
    }

    [Button]
    public void SendString(string _strData)
    {
        try
        {
            string strData = "";

            // 判斷是否加入結束符號
            if (add0X0A == true)
            {
                strData = _strData + Environment.NewLine;
            }
            else
            {
                strData = _strData;
            }

            // 轉為byte
            byte[] byteData = encoding.GetBytes(strData);

            clientSocket.Send(byteData);

            status = StatusType.Connected;
            if (logSend)
            {
                Debug.Log("[TCP SendString] " + strData);
            }
        }
        catch (System.Exception ex)
        {
            status = StatusType.Disconnected;
            Debug.Log("TCP封包傳送失敗");
            Debug.Log(ex.Message);
        }
    }

    [Button]
    public void SendBytes(byte[] _bytesData)
    {
        try
        {
            byte[] bytesData;

            // 判斷是否加入結束符號
            if (add0X0A == true)
            {
                byte[] tmpBytes = new byte[_bytesData.Length + 2];
                for (int i = 0; i < _bytesData.Length; i++)
                {
                    tmpBytes[i] = _bytesData[i];
                }
                tmpBytes[tmpBytes.Length - 2] = (byte)0x0d;
                tmpBytes[tmpBytes.Length - 1] = (byte)0x0a;

                bytesData = tmpBytes;
            }
            else
            {
                bytesData = _bytesData;
            }

            clientSocket.Send(bytesData);

            status = StatusType.Connected;
            if (logSend)
            {
                string strLogSend = "";
                for (int i = 0; i < bytesData.Length; i++)
                {
                    strLogSend += bytesData[i].ToString() + " ";
                }
                Debug.Log("[TCP SendBytes] " + strLogSend);
            }
        }
        catch (System.Exception ex)
        {
            status = StatusType.Disconnected;
            Debug.Log("TCP封包傳送失敗");
            Debug.Log(ex.Message);
        }
    }

}