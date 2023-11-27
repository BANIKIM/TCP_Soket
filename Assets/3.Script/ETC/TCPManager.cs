using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// .net 라이브러리
using System;
// 소켓통신을 하기 위한 라이브러리
using System.Net;
using System.Net.Sockets;
using System.IO;//데이터를 읽고 쓰고 하기 위한 라이브러리
using System.Threading;//멀티 쓰레딩 하기 위한 라이브러리



public class TCPManager : MonoBehaviour
{
    public InputField IPAdress;
    public InputField Port;

    [SerializeField] private Text Status;

    //기본적인 소켓통신
    //.net -> 패킷 -> Stream
    // 데이터를 읽는 부분 -> thread

    StreamReader reader; //데이터를 읽는 놈
    StreamWriter writer; //데이터를 쓰는 놈

    public InputField Message_Box;
    private Message_Pooling message;

    private Queue<string> log = new Queue<string>();
    void status_Message()
    {
        if(log.Count>0)
        {
            Status.text = log.Dequeue();
        }
    }

/*    private void Start()
    {
        Thread th = new Thread(dd);
        th.Start();
    }

    private void dd()
    {
        transform.position = new Vector3(0, 1f, 0);
    }*/


    #region Server
    public void Server_Open()
    {
        message = FindObjectOfType<Message_Pooling>();
        Thread thread = new Thread(ServerConnect);
        thread.IsBackground = true;
        thread.Start();
    }

    private void ServerConnect()//서버를 열어주는 쪽 -> 서버를 만드는 쪽
    {
        //지속적으로 사용 -> update 문처럼 사용
        //-> 메세지가 들어올 때마다 열어줌
        //흐름에다가 예외처리 -> try - catch
        try
        {
            TcpListener tcp = new TcpListener(IPAddress.Parse(IPAdress.text), int.Parse(Port.text));
            //TCPListener 객체 생성
            tcp.Start();//서버가 시작 -> 서버가 열렸다
            log.Enqueue("Server Open");

            TcpClient client = tcp.AcceptTcpClient();
            //TcpListener에 연결이 될 때까지 기다렸다가 연결이 되면 client 할당
            log.Enqueue("Client 접속 확인 완료");
            reader = new StreamReader(client.GetStream());
            writer = new StreamWriter(client.GetStream());
            writer.AutoFlush = true;

            while (client.Connected)
            {
                string readData = reader.ReadLine();
                message.Message(readData);
            }

        }
        catch (Exception e)
        {
            log.Enqueue(e.Message);
        }

    }



    #endregion


    #region Client
    public void Client_Connect()
    {
        message = FindObjectOfType<Message_Pooling>();
        log.Enqueue("Client_connect");
        Thread thread = new Thread(client_connect);
        thread.IsBackground = true;
        thread.Start();
    }

    private void client_connect()//서버에 접근하는 쪽
    {
        try
        {
            TcpClient client = new TcpClient();
            //Server = IP Start point -> cleint = ip end point
            IPEndPoint ipent = new IPEndPoint(IPAddress.Parse(IPAdress.text), int.Parse(Port.text));
            client.Connect(ipent);
            log.Enqueue("client Server Connect Compelete!");

            reader = new StreamReader(client.GetStream());
            writer = new StreamWriter(client.GetStream());
            writer.AutoFlush = true;

            while (client.Connected)
            {
                string readerData = reader.ReadLine();
                message.Message(readerData);
            }
        }
        catch(Exception e)
        {
            log.Enqueue(e.Message);
        }
    }

    #endregion


    public void Sending_btn()
    {
        //만약 메세지를 보냈다면
        //내가 보낸 메세지도 message box에 넣을 것
        if(sending_Message(Message_Box.text))
        {
            message.Message(Message_Box.text);
            Message_Box.text = string.Empty;
        }
    }
    private bool sending_Message(string me)
    {
        if(writer != null)
        {
            writer.WriteLine(me);
            return true;
        }
        else
        {
            Debug.Log("Writer Null");
            return false;
        }
    }


    private void Update()
    {
        status_Message();
    }
}
