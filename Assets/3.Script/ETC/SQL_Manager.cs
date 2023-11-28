using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// DB �ҷ����� ���ؼ� ����
using MySql.Data;
using MySql.Data.MySqlClient;
using System;
using System.IO;
// Json����
using LitJson;

public class user_info
{
    public string User_name { get; private set; }
    public string User_Password { get; private set; }

    public user_info(string name, string password)
    {
        User_name = name;
        User_Password = password;
    }
}


public class SQL_Manager : MonoBehaviour
{
    public user_info info;

    public MySqlConnection connection; // ����
    public MySqlDataReader reader; // �б�

    public string DB_Path = string.Empty;


    public static SQL_Manager instance = null;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DB_Path = Application.dataPath + "/Database";

        string serverinfo = ServerSet(DB_Path);


        try
        {
            if(serverinfo.Equals(string.Empty))
            {
                Debug.Log("SQL Server Json Error!");
                return;
            }
            connection = new MySqlConnection(serverinfo);
            connection.Open();
            Debug.Log("SQL Server open complete!");

        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    

    private string ServerSet(string path)
    {
        if (!File.Exists(path)) // .Exists(path) -> path ��ο� ������ �ֳ���?
        {
            Directory.CreateDirectory(path);
        }
        string Jsonstring = File.ReadAllText(path + "/config.json");

        JsonData itemdata = JsonMapper.ToObject(Jsonstring);
        string serverinfo = $"Server ={itemdata[0]["IP"]}; Database={itemdata[0]["TableName"]};" +
            $"Uid={itemdata[0]["ID"]};" +
            $"Pwd={itemdata[0]["PW"]};" +
            $"Port={itemdata[0]["PORT"]};" +
            $"CharSet=utf8";

        return serverinfo;
    }

    private bool connection_check(MySqlConnection con)
    {
        // ���� MySqlConnection open ���°� �ƴ϶��?
        if(con.State != System.Data.ConnectionState.Open)
        {
            con.Open();
            if(con.State != System.Data.ConnectionState.Open)
            {
                return false;
            }
        }
        return true;
    }

    public bool Login(string id,string password)
    {
        // ���������� DB���� �����͸� ������ ���� �޼ҵ�
        // ��ȸ�Ǵ� �����Ͱ� ���ٸ� False
        // ��ȸ�� �Ǵ� �����Ͱ� �ִٸ� True ���� �����µ�
        // ������ ������ info ���ٰ� ���� ������ ������.

        
        try
        {
            //connection open ��ȲȮ�� -> �޼ҵ�ȭ�� Ȯ��
            if (!connection_check(connection))
            {
                return false;
            }

            string SQL_command = string.Format(@"SELECT User_Name,User_Password FROM user_info
                                            WHERE User_Name = '{0}' AND User_Password = '{1}';",
                                            id,password);

            MySqlCommand cmd = new MySqlCommand(SQL_command, connection);
            reader = cmd.ExecuteReader();
            //Reader ���� �����Ͱ� 1�� �̻� ������?
            if(reader.HasRows)
            {
                //���� �����͸� �ϳ��� �����մϴ�.
                while (reader.Read())
                {
                    /*
                     * ���׿�����
                     */
                    string name = (reader.IsDBNull(0)) ? string.Empty : (string)reader["User_Name"].ToString();
                    string Pass = (reader.IsDBNull(1)) ? string.Empty : (string)reader["User_Password"].ToString();
                    if(!name.Equals(string.Empty)||!Pass.Equals(string.Empty))
                    {
                        // ���������� Data�� �ҷ��� ��Ȳ
                        info = new user_info(name, Pass);
                        if (!reader.IsClosed) reader.Close();
                        return true;
                    }
                    else // �α��� ����
                    {
                        break;
                    }
                }// while ��                
            }// if ��
            if (!reader.IsClosed) reader.Close();
            return false;//�α��� ����
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
            if (!reader.IsClosed) reader.Close();
            return false;
        }
    }

}
