using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// DB 불러오기 위해서 선언
using MySql.Data;
using MySql.Data.MySqlClient;
using System;
using System.IO;
// Json선언
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

    public MySqlConnection connection; // 연결
    public MySqlDataReader reader; // 읽기

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
        if (!File.Exists(path)) // .Exists(path) -> path 경로에 파일이 있나요?
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
        // 현재 MySqlConnection open 상태가 아니라면?
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
        // 직접적으로 DB에서 데이터를 가지고 오는 메소드
        // 조회되는 데이터가 없다면 False
        // 조회가 되는 데이터가 있다면 True 값을 던지는데
        // 위에서 선언한 info 에다가 담은 다음에 던진다.

        
        try
        {
            //connection open 상황확인 -> 메소드화로 확인
            if (!connection_check(connection))
            {
                return false;
            }

            string SQL_command = string.Format(@"SELECT User_Name,User_Password FROM user_info
                                            WHERE User_Name = '{0}' AND User_Password = '{1}';",
                                            id,password);

            MySqlCommand cmd = new MySqlCommand(SQL_command, connection);
            reader = cmd.ExecuteReader();
            //Reader 읽은 데이터가 1개 이상 존재해?
            if(reader.HasRows)
            {
                //읽은 데이터를 하나씩 나열합니다.
                while (reader.Read())
                {
                    /*
                     * 삼항연산자
                     */
                    string name = (reader.IsDBNull(0)) ? string.Empty : (string)reader["User_Name"].ToString();
                    string Pass = (reader.IsDBNull(1)) ? string.Empty : (string)reader["User_Password"].ToString();
                    if(!name.Equals(string.Empty)||!Pass.Equals(string.Empty))
                    {
                        // 정상적으로 Data를 불러온 상황
                        info = new user_info(name, Pass);
                        if (!reader.IsClosed) reader.Close();
                        return true;
                    }
                    else // 로그인 실패
                    {
                        break;
                    }
                }// while 끝                
            }// if 끝
            if (!reader.IsClosed) reader.Close();
            return false;//로그인 실패
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
            if (!reader.IsClosed) reader.Close();
            return false;
        }
    }

}
