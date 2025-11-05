using System.Collections;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using System.Data;
using UnityEditor.MemoryProfiler;
using UnityEditor.VersionControl;
using System.Xml.Linq;

public class DBScript// : MonoBehaviour
{
    string filePath = "URI=file:" + Application.dataPath + "/Database.db";

    
    public bool VerifyLogin(string name, string password)
    {
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
        }
    }
    public void AddTask(string title, string description, int workerId)
    {
        using (var connection = new SqliteConnection(filePath))
        {
            connection.Open();

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
                Debug.Log("Задача добавлена: " + title);
            }
            catch (SqliteException ex)
            {
                Debug.LogError("Ошибка добавления задачи: " + ex.Message);
            }
        }
    }
    public void AddEpic(string title, string description)
    {
        using (var connection = new SqliteConnection(filePath))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO epic (title, description, status_id, user_task_id) VALUES" +
                " (@title, @description, @status_id, @user_task_id)";
            command.Parameters.AddWithValue("@title", title);
            command.Parameters.AddWithValue("@description", description);
            command.Parameters.AddWithValue("@status_id", 1);
            //command.Parameters.AddWithValue("@user_task_id", workerId);
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
            return userId;
        }
    }
    public int GetUserIdByName(string name)
    {
        using (var connection = new SqliteConnection(filePath))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT idUser FROM users WHERE email = @name";
            command.Parameters.AddWithValue("@name", name);

            int userId = 0;
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    userId = reader.GetInt32(0);//.GetString(0);// Сравнение паролей
                }
            }
            return userId;
        }
    }

    public List<string> GetTaskByUser(int userId)
    {
        List<string> tasks = new List<string>();
        // Строка подключения к базе данных

        using (var connection = new SqliteConnection(filePath))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT title FROM task WHERE user_task_id = @user_id";
            command.Parameters.AddWithValue("@user_id", userId);

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
        }
        return tasks;
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
        }

        return emails;
    }
}
