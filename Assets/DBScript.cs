using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class DBScript
{
    private string serverUrl = "http://localhost/api/"; 

    [Serializable]
    public class TaskItemData
    {
        public int idTask;
        public string title;
        public string description;
        public string status_name;
    }

    [Serializable]
    public class TaskListWrapper
    {
        public TaskItemData[] tasks;
    }

    // Данные для авторизации и статистики
    [Serializable] public class ServerResponseLogin { public string status; public int id; public int role; }
    [Serializable] public class WorkerListWrapper { public string[] workers; }
    [Serializable] public class TaskStatItem { public string title; public string email; }
    [Serializable] public class StatWrapper { public TaskStatItem[] items; }
    
    // Данные для чата
    [Serializable] public class ChatItem { public string name; public string message; public string timestamp; }
    [Serializable] public class ChatWrapper { public ChatItem[] messages; }

    public struct UserLoginData { public bool Success; public int Id; public int Role; }
    public struct ChatMessageData { public string SenderName; public string Text; public string Time; }

    // Добавление пользователя
    public IEnumerator AddUserWeb(string name, string email, string password, int roleId)
    {
        WWWForm form = new WWWForm();
        form.AddField("name", name);
        form.AddField("email", email);
        form.AddField("password", password);
        form.AddField("role_id", roleId);

        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "add_user.php", form))
        {
            yield return www.SendWebRequest();
            if(www.result == UnityWebRequest.Result.Success) Debug.Log("User added: " + www.downloadHandler.text);
        }
    }

    // Отправка сообщения в чат
    public IEnumerator SendMessageWeb(int taskId, int userId, string message)
    {
        WWWForm form = new WWWForm();
        form.AddField("task_id", taskId);
        form.AddField("user_id", userId);
        form.AddField("message", message);

        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "send_message.php", form))
        {
            yield return www.SendWebRequest();
        }
    }

    // Загрузка сообщений чата
    public IEnumerator GetChatMessagesWeb(int taskId, Action<List<ChatMessageData>> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("task_id", taskId);

        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "get_chat.php", form))
        {
            yield return www.SendWebRequest();
            List<ChatMessageData> result = new List<ChatMessageData>();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var wrapper = JsonUtility.FromJson<ChatWrapper>(www.downloadHandler.text);
                if (wrapper != null && wrapper.messages != null)
                {
                    foreach (var msg in wrapper.messages)
                    {
                        result.Add(new ChatMessageData { 
                            SenderName = msg.name, 
                            Text = msg.message, 
                            Time = msg.timestamp 
                        });
                    }
                }
            }
            callback(result);
        }
    }

    // Авторизация
    public IEnumerator TryLoginWeb(string login, string password, Action<UserLoginData> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("name", login);
        form.AddField("password", password);

        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "login.php", form))
        {
            yield return www.SendWebRequest();
            UserLoginData data = new UserLoginData { Success = false };

            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<ServerResponseLogin>(www.downloadHandler.text);
                if (response != null && response.status == "success")
                {
                    data.Success = true;
                    data.Id = response.id;
                    data.Role = response.role;
                }
            }
            callback(data);
        }
    }

    // Получить ID пользователя по email
    public IEnumerator GetUserIdByEmailWeb(string email, Action<int> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("email", email);
        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "get_worker_id.php", form))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                if (int.TryParse(www.downloadHandler.text, out int id))
                    callback(id);
                else callback(0);
            }
            else callback(0);
        }
    }

    // Добавить эпик
    public IEnumerator AddEpicWeb(string title, string description, int masterId)
    {
        WWWForm form = new WWWForm();
        form.AddField("title", title);
        form.AddField("description", description);
        form.AddField("master_id", masterId);

        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "add_epic.php", form))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
                Debug.Log("Epic Created: " + www.downloadHandler.text);
        }
    }

    // Получить список работников (для выпадающего списка)
    public IEnumerator GetWorkersEmailsWeb(Action<List<string>> callback)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(serverUrl + "get_workers.php"))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                var wrapper = JsonUtility.FromJson<WorkerListWrapper>(www.downloadHandler.text);
                if (wrapper != null && wrapper.workers != null)
                    callback(new List<string>(wrapper.workers));
                else
                    callback(new List<string>());
            }
        }
    }


    // Добавить задачу
    public IEnumerator AddTaskWeb(string title, string description, int masterId, int workerId)
    {
        WWWForm form = new WWWForm();
        form.AddField("title", title);
        form.AddField("description", description);
        form.AddField("master_id", masterId);
        form.AddField("worker_id", workerId);

        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "add_task.php", form))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
                Debug.Log("Task Created: " + www.downloadHandler.text);
            else
                Debug.LogError("Error adding task: " + www.error);
        }
    }

    // Получить статистику для мастера
    public IEnumerator GetStatsWeb(int masterId, Action<string> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("master_id", masterId);

        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "get_stats.php", form))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = "{\"items\":" + www.downloadHandler.text + "}"; // Хак для JsonUtility
                var wrapper = JsonUtility.FromJson<StatWrapper>(json);
                
                string resultText = "";
                if (wrapper != null && wrapper.items != null)
                {
                    foreach(var item in wrapper.items)
                    {
                        resultText += $"Задача {item.title} выполняет {item.email};\n";
                    }
                }
                callback(resultText);
            }
        }
    }

    // Задачи конкретного работника
    public IEnumerator GetWorkerTasksWeb(int workerId, Action<List<TaskItemData>> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("worker_id", workerId);

        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "get_worker_tasks.php", form))
        {
            yield return www.SendWebRequest();

            List<TaskItemData> resultList = new List<TaskItemData>();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try 
                {
                    var wrapper = JsonUtility.FromJson<TaskListWrapper>(www.downloadHandler.text);
                    if (wrapper != null && wrapper.tasks != null)
                    {
                        resultList.AddRange(wrapper.tasks);
                    }
                }
                catch (Exception e) { Debug.LogError("JSON Error: " + e.Message); }
            }
            callback(resultList);
        }
    }
}