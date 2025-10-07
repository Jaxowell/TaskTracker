using Mono.Data.Sqlite;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignInScript : MonoBehaviour
{
    CreateDataBase cDB= new CreateDataBase();

    public GameObject show;
    public TMP_InputField login;
    public TMP_InputField password;
    // Start is called before the first frame update
    void Start()
    {
        cDB.Start();
    }

    // Update is called once per frame
    public void SignIn()
    {
        if(VerifyLogin(login.text,password.text))
        {
            show.SetActive(true);
        }
        else
        {
            Debug.Log(login.text + " " + password.text);
        }
        /*
        if (login.text == "Arik" && password.text == "qwerty")
            show.SetActive(true);
        else
        {
            Debug.Log(login.text+" "+password.text);
        }
        */
    }

    public bool VerifyLogin(string username, string password)
    {
        using (var connection = new SqliteConnection(cDB.connectionString))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT password FROM users WHERE username = @username";
            command.Parameters.AddWithValue("@username", username);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    string storedPassword = reader.GetString(0);
                    return storedPassword == password; // Сравнение паролей
                }
            }
        }
        return false;
    }
}
