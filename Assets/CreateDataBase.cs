using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System;

public class CreateDataBase// : MonoBehaviour
{
    public string connectionString;

    public void Start()
    {
        connectionString = "URI=file:" + Application.dataPath + "/UsersDatabase.sqlite";
        CreateTable();
        AddUser("Arik", "qwerty");
        AddUser("Miko", "carrot");
    }

    void CreateTable()
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS users (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    username TEXT UNIQUE NOT NULL,
                    password TEXT NOT NULL
                )";

            command.ExecuteNonQuery();
        }
    }

    public void AddUser(string username, string password)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO users (username, password) VALUES (@username, @password)";
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password); // В реальном проекте хэшируйте пароль!

            try
            {
                command.ExecuteNonQuery();
                Debug.Log("Пользователь добавлен: " + username);
            }
            catch (SqliteException ex)
            {
                Debug.LogError("Ошибка добавления пользователя: " + ex.Message);
            }
        }
    }

}