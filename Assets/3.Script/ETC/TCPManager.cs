using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// .net ���̺귯��
using System;
// ��������� �ϱ� ���� ���̺귯��
using System.Net;
using System.Net.Sockets;
using System.IO;//�����͸� �а� ���� �ϱ� ���� ���̺귯��
using System.Threading;//��Ƽ ������ �ϱ� ���� ���̺귯��



public class TCPManager : MonoBehaviour
{
    public InputField IPAdress;
    public InputField Port;

    [SerializeField] private Text Status;

    //�⺻���� �������
    //.net -> ��Ŷ -> Stream
    // �����͸� �д� �κ� -> thread

    StreamReader reader; //�����͸� �д� ��
    StreamWriter writer; //�����͸� ���� ��

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

    private void ServerConnect()//������ �����ִ� �� -> ������ ����� ��
    {
        //���������� ��� -> update ��ó�� ���
        //-> �޼����� ���� ������ ������
        //�帧���ٰ� ����ó�� -> try - catch
        try
        {
            TcpListener tcp = new TcpListener(IPAddress.Parse(IPAdress.text), int.Parse(Port.text));
            //TCPListener ��ü ����
            tcp.Start();//������ ���� -> ������ ���ȴ�
            log.Enqueue("Server Open");

            TcpClient client = tcp.AcceptTcpClient();
            //TcpListener�� ������ �� ������ ��ٷȴٰ� ������ �Ǹ� client �Ҵ�
            log.Enqueue("Client ���� Ȯ�� �Ϸ�");
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

    private void client_connect()//������ �����ϴ� ��
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
        //���� �޼����� ���´ٸ�
        //���� ���� �޼����� message box�� ���� ��
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
