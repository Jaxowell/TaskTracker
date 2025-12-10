using System.Collections;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using System.Data;
using UnityEditor.MemoryProfiler;
using UnityEditor.VersionControl;
using System.Xml.Linq;

public class DBScript
{
    string filePath = "Data Source=" + Application.dataPath + "/Database.db;Pooling=False;Version=3;";
    
    public bool VerifyLogin(string name, string password)
    {
        //password = Hasher.HashPassword(password);

        using (var connection = new SqliteConnection(filePath))
        {
            OpenConnectionWithRetry(connection); 

            var command = connection.CreateCommand();
            command.CommandText = "SELECT password FROM users WHERE name = @name";
            command.Parameters.AddWithValue("@name", name);

            string hash="";
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    hash = reader.GetString(0);// ��������� �������
                }
            }
            //Debug.Log(password+" "+hash);
            return Hasher.VerifyPassword(password, hash);
        }
    }
    public void AddUser(string name, string email, string password, int roleId)
    {
        using (var connection = new SqliteConnection(filePath))
        {
            OpenConnectionWithRetry(connection); 

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
                Debug.Log("������������ ��������: " + name);
            }
            catch (SqliteException ex)
            {
                Debug.LogError("������ ���������� ������������: " + ex.Message);
            }
        }
    }
    public void AddTask(string title, string description, int workerId)
    {
        using (var connection = new SqliteConnection(filePath))
        {
            OpenConnectionWithRetry(connection); 

            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO task (title, description, status_id, user_task_id) VALUES" +
                " (@title, @description, @status_id, @user_task_id)";
            command.Parameters.AddWithValue("@title", title);
            command.Parameters.AddWithValue("@description", description);
            command.Parameters.AddWithValue("@status_id", 1);
            command.Parameters.AddWithValue("@user_task_id", workerId);
            //command.Parameters.AddWithValue("@chat_task_id", 1);

            try
            {
                command.ExecuteNonQuery();
                Debug.Log("������ ���������: " + title);
            }
            catch (SqliteException ex)
            {
                Debug.LogError("������ ���������� ������: " + ex.Message);
            }
        }
    }
    public int GetUserRole(string name)
    {
        using (var connection = new SqliteConnection(filePath))
        {
            OpenConnectionWithRetry(connection); 

            var command = connection.CreateCommand();
            command.CommandText = "SELECT role_id FROM users WHERE name = @name";
            command.Parameters.AddWithValue("@name", name);

            int roleId = 0;
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    roleId = reader.GetInt32(0);//.GetString(0);// ��������� �������
                }
            }
            return roleId;
        }
    }
    public int GetUserIdByEmail(string email)
    {
        using (var connection = new SqliteConnection(filePath))
        {
            OpenConnectionWithRetry(connection); 

            var command = connection.CreateCommand();
            command.CommandText = "SELECT idUser FROM users WHERE email = @email";
            command.Parameters.AddWithValue("@email", email);

            int userId = 0;
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    userId = reader.GetInt32(0);//.GetString(0);// ��������� �������
                }
            }
            return userId;
        }
    }
    public int GetUserIdByName(string name)
    {
        using (var connection = new SqliteConnection(filePath))
        {
            OpenConnectionWithRetry(connection); 

            var command = connection.CreateCommand();
            command.CommandText = "SELECT idUser FROM users WHERE name = @name";
            command.Parameters.AddWithValue("@name", name);

            int userId = 0;
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    userId = reader.GetInt32(0);//.GetString(0);// ��������� �������
                }
            }
            return userId;
        }
    }

    public List<string> GetTaskByUser(int userId)
    {
        List<string> tasks = new List<string>();
        // ������ ����������� � ���� ������

        using (var connection = new SqliteConnection(filePath))
        {
            OpenConnectionWithRetry(connection); 

            var command = connection.CreateCommand();
            command.CommandText = "SELECT title FROM task WHERE user_task_id = @user_id";
            command.Parameters.AddWithValue("@user_id", userId);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    // ��������� email � ������, ���� �� �� NULL
                    if (!reader.IsDBNull(0))
                    {
                        tasks.Add(reader.GetString(0));
                    }
                }
            }
        }
        return tasks;
    }

    public List<string> GetUserEmailsByRole(int roleId)
    {
        List<string> emails = new List<string>();
        // ������ ����������� � ���� ������

        using (var connection = new SqliteConnection(filePath))
        {
            OpenConnectionWithRetry(connection); 

            var command = connection.CreateCommand();
            command.CommandText = "SELECT email FROM users WHERE role_id = @role_id";
            command.Parameters.AddWithValue("@role_id", roleId);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    // ��������� email � ������, ���� �� �� NULL
                    if (!reader.IsDBNull(0))
                    {
                        emails.Add(reader.GetString(0));
                    }
                }
            }
        }

        return emails;
    }

    public string GetUserNameById(int id)
    {
        using (var connection = new SqliteConnection(filePath))
        {
            OpenConnectionWithRetry(connection); 

            var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM users WHERE idUser = @id";
            command.Parameters.AddWithValue("@id", id);

            string userName = string.Empty;
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                        userName = reader.GetString(0);
                }
            }
            return userName;
        }
    }

    // ---- БЛОК ЧАТА ----

    // 1. Инициализация таблицы сообщений (Запустить 1 раз или проверять при старте)
    public void InitChatTables()
    {
        using (var connection = new SqliteConnection(filePath))
        {
            OpenConnectionWithRetry(connection); 
            using (var command = connection.CreateCommand())
            {
                // Таблица сообщений
                // id: уникальный номер сообщения
                // task_id: к какой задаче относится
                // user_id: кто написал
                // message: текст
                // timestamp: время (для сортировки)
                string query = @"
                    CREATE TABLE IF NOT EXISTS messages (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        task_id INTEGER NOT NULL,
                        user_id INTEGER NOT NULL,
                        message TEXT NOT NULL,
                        timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
                    );";
                command.CommandText = query;
                command.ExecuteNonQuery();
            }
        }
    }

    // Вспомогательный класс для данных
    public struct ChatMessageData
    {
        public string SenderName;
        public string Text;
        public string Time;
    }

    // 2. Отправка сообщения
    public void SendMessage(int taskId, int userId, string message)
    {
        using (var connection = new SqliteConnection(filePath))
        {
            OpenConnectionWithRetry(connection); 

            var command = connection.CreateCommand();
            
            // Используем параметры, чтобы нельзя было сломать базу спецсимволами
            command.CommandText = "INSERT INTO messages (task_id, user_id, message) VALUES (@tid, @uid, @msg)";
            command.Parameters.AddWithValue("@tid", taskId);
            command.Parameters.AddWithValue("@uid", userId);
            command.Parameters.AddWithValue("@msg", message);

            try { command.ExecuteNonQuery(); }
            catch (SqliteException ex) { Debug.LogError("DB Error Sending Msg: " + ex.Message); }
        }
    }

    // 3. Получение сообщений (сразу с именами пользователей)
    public List<ChatMessageData> GetChatMessages(int taskId)
        {
            var list = new List<ChatMessageData>();

            using (var connection = new SqliteConnection(filePath))
            {
                OpenConnectionWithRetry(connection); 
                var command = connection.CreateCommand();

                // COALESCE(users.name, 'Неизвестный') означает:
                // "Если имя нашлось — бери его. Если нет — пиши 'Неизвестный'"
                command.CommandText = @"
                    SELECT 
                        COALESCE(users.name, 'Неизвестный'), 
                        messages.message, 
                        messages.timestamp 
                    FROM messages
                    LEFT JOIN users ON messages.user_id = users.idUser
                    WHERE messages.task_id = @tid
                    ORDER BY messages.id ASC"; 
                
                command.Parameters.AddWithValue("@tid", taskId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ChatMessageData data = new ChatMessageData();
                        // Индекс 0 - это имя, 1 - текст
                        data.SenderName = reader.IsDBNull(0) ? "Неизвестный" : reader.GetString(0);
                        data.Text = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        data.Time = reader.IsDBNull(2) ? "" : reader.GetString(2);
                        
                        list.Add(data);
                    }
                }
            }
            return list;
        }

        private void OpenConnectionWithRetry(SqliteConnection connection)
        {
            if (connection.State == ConnectionState.Open) return;

            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA busy_timeout = 2000;"; // Ждать 2 секунды перед ошибкой locked
                command.ExecuteNonQuery();
            }
        }

    public UserLoginData TryLoginFull(string login, string password)
    {
        UserLoginData result = new UserLoginData { Success = false };

        using (var connection = new SqliteConnection(filePath))
        {
            OpenConnectionWithRetry(connection);
            
            // 1. Проверяем пароль
            var command = connection.CreateCommand();
            command.CommandText = "SELECT idUser, role_id, password FROM users WHERE name = @name";
            command.Parameters.AddWithValue("@name", login);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    int role = reader.GetInt32(1);
                    string dbHash = reader.GetString(2);

                    if (Hasher.VerifyPassword(password, dbHash))
                    {
                        result.Success = true;
                        result.Id = id;
                        result.Role = role;
                    }
                }
            }
        }
        return result;
    }

    
    public struct UserLoginData
    {
        public bool Success;
        public int Id;
        public int Role;
    }


}
