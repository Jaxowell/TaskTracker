using System.Collections;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using System.Data;
using UnityEditor.MemoryProfiler;
using UnityEditor.VersionControl;

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
}
