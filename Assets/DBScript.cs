using System.Collections;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using System.Data;
using UnityEditor.MemoryProfiler;
using UnityEditor.VersionControl;
using System.Xml.Linq;
using Unity.VisualScripting;

public class DBScript// : MonoBehaviour
{
    string filePath = "URI=file:" + Application.streamingAssetsPath + "/Database.db";
    public string[] statusColors = new string[4];
    public void ChangeStatus(int taskId, int newStatusId)
    {
        using (var connection = new SqliteConnection(filePath))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "UPDATE task SET status_id = @aboba WHERE idTask = @huba;";
            command.Parameters.AddWithValue("@aboba", newStatusId);
            command.Parameters.AddWithValue("@huba", taskId);
            command.ExecuteNonQuery();
            //command.ExecuteReader();
            //Debug.Log(password+" "+hash);
            connection.Close();

        }
        
    }
    private void LoadColors()
    {
        using (var connection = new SqliteConnection(filePath))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT color FROM status";
            
            using (var reader = command.ExecuteReader())
            {
                int i = 0;
                while (reader.Read())
                {
                    statusColors[i] = reader.GetString(0);
                    i++;
                }
            }
            //Debug.Log(password+" "+hash);
            connection.Close();
           
        }
    }
    public List<string> LoadStatuses()
    {
        using (var connection = new SqliteConnection(filePath))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM status";
            List<string> statuses = new List<string>();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    statuses.Add(reader.GetString(0));
                }
            }
            //Debug.Log(password+" "+hash);
            connection.Close();
            return statuses;
        }
    }
    public int GetLastTaskId()
    {
        int id = -1;
        using (var connection = new SqliteConnection(filePath))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT MAX(idTask) FROM task";

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    id = reader.GetInt32(0);// Сравнение паролей
                }
            }
            //Debug.Log(password+" "+hash);
            connection.Close();
            return id;
        }
    }
    public string GetStatusById(int id)
    {
        string name = "";
        //int id = -1;
        using (var connection = new SqliteConnection(filePath))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM status WHERE idStatus = @Id";
            command.Parameters.AddWithValue("@Id", id);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    name = reader.GetString(0);// Сравнение паролей
                }
            }
            //Debug.Log(password+" "+hash);
            connection.Close();
            return name;
        }
    }
    public string GetNameById(int id)
    {
        string name = "";
        //int id = -1;
        using (var connection = new SqliteConnection(filePath))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM users WHERE idUser = @userId";
            command.Parameters.AddWithValue("@userId", id);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    name = reader.GetString(0);// Сравнение паролей
                }
            }
            //Debug.Log(password+" "+hash);
            connection.Close();
            return name;
        }
    }
    public bool VerifyLogin(string name, string password)
    {
        LoadColors();
        //password = Hasher.HashPassword(password);

        using (var connection = new SqliteConnection(filePath))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT password FROM users WHERE name = @name";
            command.Parameters.AddWithValue("@name", name);

            string hash="";
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    hash = reader.GetString(0);// Сравнение паролей
                }
            }
            //Debug.Log(password+" "+hash);
            connection.Close();
            return Hasher.VerifyPassword(password, hash);
        }
    }
    public void AddUser(string name, string email, string password, int roleId)
    {
        using (var connection = new SqliteConnection(filePath))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO users (name, email, password, role_id) VALUES (@username, @email, @password, @role_id)";
            command.Parameters.AddWithValue("@username", name);
            command.Parameters.AddWithValue("@email", email);
            password = Hasher.HashPassword(password);
            command.Parameters.AddWithValue("@password", password);
            command.Parameters.AddWithValue("@role_id", roleId);

            try
            {
                command.ExecuteNonQuery();
                Debug.Log("Пользователь добавлен: " + name);
            }
            catch (SqliteException ex)
            {
                Debug.LogError("Ошибка добавления пользователя: " + ex.Message);
            }
            connection.Close();
        }
    }
    public void AddTask(string title, string description, int masterid ,int workerId)
    {
        using (var connection = new SqliteConnection(filePath))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO task (title, description, status_id, master_id, user_task_id) VALUES" +
                " (@title, @description, @status_id, @master_Id, @user_task_id)";
            command.Parameters.AddWithValue("@title", title);
            command.Parameters.AddWithValue("@description", description);
            command.Parameters.AddWithValue("@status_id", 1);
            command.Parameters.AddWithValue("@master_id", masterid);
            command.Parameters.AddWithValue("@user_task_id", workerId);
            //command.Parameters.AddWithValue("@chat_task_id", 1);

            try
            {
                command.ExecuteNonQuery();
                Debug.Log("Задача добавлена: " + title);
            }
            catch (SqliteException ex)
            {
                Debug.LogError("Ошибка добавления задачи: " + ex.Message);
            }
            connection.Close();
        }
    }
    public void AddEpic(string title, string description, int masterId)
    {
        Debug.Log(" начинаем создавать эпик");
        using (var connection = new SqliteConnection(filePath))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO epic (title, description, status_id, master_id,  chat_epic_id) VALUES" +
                " (@title, @description,  @status_id, @master_id, @chat_epic_id)";
            command.Parameters.AddWithValue("@title", title);
            command.Parameters.AddWithValue("@description", description);
            command.Parameters.AddWithValue("@status_id", 1);
            command.Parameters.AddWithValue("@master_id", masterId);
            command.Parameters.AddWithValue("@chat_epic_id", 1);

            try
            {
                command.ExecuteNonQuery();
                Debug.Log("Эпик добавлен: " + title);
            }
            catch (SqliteException ex)
            {
                Debug.LogError("Ошибка добавления эпика: " + ex.Message);
            }
            connection.Close();
        }
    }
    public int GetUserRole(string name)
    {
        using (var connection = new SqliteConnection(filePath))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT role_id FROM users WHERE name = @name";
            command.Parameters.AddWithValue("@name", name);

            int roleId = 0;
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    roleId = reader.GetInt32(0);//.GetString(0);// Сравнение паролей
                }
            }
            connection.Close();
            return roleId;
        }
    }
    public int GetUserIdByEmail(string email)
    {
        using (var connection = new SqliteConnection(filePath))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT idUser FROM users WHERE email = @email";
            command.Parameters.AddWithValue("@email", email);

            int userId = 0;
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    userId = reader.GetInt32(0);//.GetString(0);// Сравнение паролей
                }
            }
            connection.Close();
            return userId;
        }
    }
    public int GetUserIdByName(string name)
    {
        using (var connection = new SqliteConnection(filePath))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT idUser FROM users WHERE name = @name";
            command.Parameters.AddWithValue("@name", name);

            int userId = 0;
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    userId = reader.GetInt32(0);//.GetString(0);// Сравнение паролей
                }
            }
            connection.Close();
            return userId;
        }
    }

    public List<string> GetTaskBySensei(int masterId)
    {
        List<string> tasks = new List<string>();
        // Строка подключения к базе данных

        using (var connection = new SqliteConnection(filePath))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT title FROM task WHERE master_id = @master_id";
            command.Parameters.AddWithValue("@master_id", masterId);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    // Добавляем email в список, если он не NULL
                    if (!reader.IsDBNull(0))
                    {
                        tasks.Add(reader.GetString(0));
                    }
                }
            }
            connection.Close();
        }
        return tasks;
    }
    public List<string> GetEpicByMaster(int masterId)
    {
        List<string> epic = new List<string>();
        // Строка подключения к базе данных

        using (var connection = new SqliteConnection(filePath))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT title FROM epic WHERE master_id = @master_id";
            command.Parameters.AddWithValue("@master_id", masterId);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    // Добавляем email в список, если он не NULL
                    if (!reader.IsDBNull(0))
                    {
                        epic.Add(reader.GetString(0));
                    }
                }
            }
            connection.Close();
        }
        return epic;
    }
    public List<string> AIGetTaskByMaster(int masterId)
    {
        List<string> task = new List<string>();

        using (var connection = new SqliteConnection(filePath))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
            SELECT e.title, u.email 
            FROM task e 
            LEFT JOIN users u ON e.user_task_id = u.idUser 
            WHERE e.master_id = @master_id";
            command.Parameters.AddWithValue("@master_id", masterId);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    // Добавляем название эпика
                    if (!reader.IsDBNull(0))
                    {
                        task.Add(reader.GetString(0));
                    }

                    // Добавляем email пользователя (может быть NULL)
                    if (!reader.IsDBNull(1))
                    {
                        task.Add(reader.GetString(1));
                    }
                    else
                    {
                        task.Add("No email"); // или пустая строка, если email отсутствует
                    }
                }
            }
            connection.Close();
        }
        return task;
    }
    public List<string> GetUserEmailsByRole(int roleId)
    {
        List<string> emails = new List<string>();
        // Строка подключения к базе данных

        using (var connection = new SqliteConnection(filePath))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT email FROM users WHERE role_id = 3";
            command.Parameters.AddWithValue("@role_id", roleId);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    // Добавляем email в список, если он не NULL
                    if (!reader.IsDBNull(0))
                    {
                        emails.Add(reader.GetString(0));
                    }
                }
            }
            connection.Close();
        }

        return emails;
    }
    public List<Task> GetTasksByMaster(int masterId)
    {
        List<Task> tasks=new List<Task>();
        using (var connection = new SqliteConnection(filePath))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT idTask, title, user_task_id, description, status_id FROM task WHERE master_id = @master_id";
            command.Parameters.AddWithValue("@master_id", masterId);
            //(int id, string title, string workerName, int workerId, string description, int statusId, string masterName, int masterId)


            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string title=reader.GetString(1);
                    int workerId=reader.GetInt32(2);
                    string description=reader.GetString(3);
                    int statusId=reader.GetInt32(4);
                    //Debug.Log(workerId);

                    var commandMinor = connection.CreateCommand();
                    commandMinor.CommandText = "SELECT name FROM users WHERE idUser = @worker_id";
                    commandMinor.Parameters.AddWithValue("@worker_id", workerId);

                    string workerName;
                    using (var readerMinor = commandMinor.ExecuteReader())
                    {
                        readerMinor.Read();
                        workerName = readerMinor.GetString(0);
                        //Debug.Log(workerName);
                    }
                    commandMinor.CommandText = "SELECT name FROM users WHERE idUser = @master_id";
                    commandMinor.Parameters.AddWithValue("@master_id", masterId);

                    string masterName;

                    using (var readerMinor = commandMinor.ExecuteReader())
                    {
                        readerMinor.Read();
                        masterName = readerMinor.GetString(0);
                        //Debug.Log(masterName);
                    }

                    Task task = new Task(id,title,workerName,workerId,description,statusId,masterName, masterId);
                    tasks.Add(task);
                }
            }
            connection.Close();
        }
        return tasks;
    }
    public List<Task> GetTasksByWorker(int workerId)
    {
        List<Task> tasks = new List<Task>();
        using (var connection = new SqliteConnection(filePath))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT idTask, title, master_id, description, status_id FROM task WHERE user_task_id = @aboba";
            command.Parameters.AddWithValue("@aboba", workerId);
            //(int id, string title, string workerName, int workerId, string description, int statusId, string masterName, int masterId)


            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string title = reader.GetString(1);
                    int masterId = reader.GetInt32(2);
                    string description = reader.GetString(3);
                    int statusId = reader.GetInt32(4);
                    //Debug.Log(workerId);

                    var commandMinor = connection.CreateCommand();
                    commandMinor.CommandText = "SELECT name FROM users WHERE idUser = @aboba";
                    commandMinor.Parameters.AddWithValue("@aboba", workerId);

                    string workerName;
                    using (var readerMinor = commandMinor.ExecuteReader())
                    {
                        readerMinor.Read();
                        workerName = readerMinor.GetString(0);
                        //Debug.Log(workerName);
                    }
                    commandMinor.CommandText = "SELECT name FROM users WHERE idUser = @aboba";
                    commandMinor.Parameters.AddWithValue("@aboba", masterId);

                    string masterName;

                    using (var readerMinor = commandMinor.ExecuteReader())
                    {
                        readerMinor.Read();
                        masterName = readerMinor.GetString(0);
                        //Debug.Log(masterName);
                    }

                    Task task = new Task(id, title, workerName, workerId, description, statusId, masterName, masterId);
                    tasks.Add(task);
                }
            }
            connection.Close();
        }
        return tasks;

    }
}
