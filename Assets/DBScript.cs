using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

[System.Serializable] 
public class DBScript
{
    [Header("Настройки Сети")]
    public string serverUrl = "https://77c4e57b901a5ec9-5-18-87-197.serveousercontent.com/api/"; 

    [Header("Кэш цветов")]
    public string[] statusColors = new string[] { "FFFFFF", "FFFF00", "00FF00", "FF2B00" };
    public List<string> statusNames = new List<string>();

    // --- STRUCтURES FOR JSON (DTO) ---
    // Эти классы нужны, чтобы превращать JSON от PHP в переменные C#
    
    [Serializable] 
    public class ServerResponseLogin { 
        public string status; 
        public int id; 
        public int role; 
        public string message; 
    }
    
    [Serializable] 
    public class TaskDTO {
        public int idTask; 
        public string title; 
        public string description; 
        public int status_id; 
        public int master_id; 
        public int user_task_id;
        public string master_name; 
        public string worker_name;
    }
    [Serializable] public class TaskListWrapper { public TaskDTO[] tasks; }

    [Serializable] 
    public class EpicDTO {
        public int idEpic; 
        public string title; 
        public string description;
        public int chat_epic_id; 
        public int master_id; 
        public string master_name; 
        public string chat_name;
    }
    [Serializable] public class EpicListWrapper { public EpicDTO[] epics; }

    [Serializable] 
    public class SubTaskDTO {
        public int idSubtask; 
        public string title; 
        public string description;
        public int status_id; 
        public int user_subtask_id; 
        public string worker_name;
    }
    [Serializable] public class SubTaskListWrapper { public SubTaskDTO[] subtasks; }

    [Serializable] public class StatusDTO { public string name; public string color; }
    [Serializable] public class StatusListWrapper { public StatusDTO[] statuses; }
    
    [Serializable] public class WorkerListWrapper { public string[] workers; }

    // Для Чата
    [Serializable] public class ChatItem { public string name; public string message; public string timestamp; }
    [Serializable] public class ChatWrapper { public ChatItem[] messages; }
    
    // Простые структуры для передачи данных внутри Unity
    public struct ChatMessageData { public string SenderName; public string Text; public string Time; }
    public struct UserLoginData { public bool Success; public int Id; public int Role; }

    // =================================================================================
    // МЕТОДЫ (API CALLS)
    // =================================================================================

    // --- 1. ВХОД В СИСТЕМУ ---
    public IEnumerator TryLoginWeb(string login, string password, Action<UserLoginData> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("name", login);
        form.AddField("password", password); // Отправляем как есть, PHP захеширует и проверит

        // Используем UnityWebRequest для связи
        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "login.php", form))
        {
            yield return www.SendWebRequest();
            
            UserLoginData data = new UserLoginData { Success = false };

            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = www.downloadHandler.text;
                Debug.Log($"Login Response: {json}"); // Лог для отладки
                
                try {
                    var response = JsonUtility.FromJson<ServerResponseLogin>(json);
                    if (response != null && response.status == "success") {
                        data.Success = true;
                        data.Id = response.id;
                        data.Role = response.role;
                    }
                } catch (Exception e) { Debug.LogError("JSON Error: " + e.Message); }
            }
            else
            {
                Debug.LogError("Network Error: " + www.error);
            }
            callback(data);
        }
    }

    // --- 2. ЗАГРУЗКА ЦВЕТОВ И СТАТУСОВ ---
    public IEnumerator LoadColorsWeb()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(serverUrl + "get_statuses.php"))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                try {
                    // Оборачиваем массив в объект, чтобы JsonUtility смог его съесть
                    string json = "{ \"statuses\": " + www.downloadHandler.text + "}";
                    var wrapper = JsonUtility.FromJson<StatusListWrapper>(json);
                    
                    if (wrapper != null && wrapper.statuses != null) {
                        statusColors = new string[wrapper.statuses.Length];
                        statusNames.Clear();
                        for (int i = 0; i < wrapper.statuses.Length; i++) {
                            statusNames.Add(wrapper.statuses[i].name);
                            // Убираем решетку, если она пришла из базы
                            statusColors[i] = wrapper.statuses[i].color.Replace("#", "");
                        }
                    }
                } catch {}
            }
        }
    }

    // --- 3. ПОЛУЧЕНИЕ ЗАДАЧ (ДЛЯ МАСТЕРА) ---
    public IEnumerator GetTasksByMasterWeb(int masterId, Action<List<AppTask>> callback)
    {
        WWWForm form = new WWWForm(); form.AddField("master_id", masterId);
        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "get_tasks_master.php", form))
        {
            yield return www.SendWebRequest();
            List<AppTask> result = new List<AppTask>();
            
            if (www.result == UnityWebRequest.Result.Success) {
                try {
                    string json = "{ \"tasks\": " + www.downloadHandler.text + "}";
                    var wrapper = JsonUtility.FromJson<TaskListWrapper>(json);
                    if (wrapper != null && wrapper.tasks != null)
                        foreach(var t in wrapper.tasks)
                            result.Add(new AppTask(t.idTask, t.title, t.worker_name, t.user_task_id, t.description, t.status_id, t.master_name, t.master_id));
                } catch {}
            }
            callback(result);
        }
    }

    // --- 4. ПОЛУЧЕНИЕ ЗАДАЧ (ДЛЯ ВОРКЕРА) ---
    public IEnumerator GetTasksByWorkerWeb(int workerId, Action<List<AppTask>> callback)
    {
        WWWForm form = new WWWForm(); form.AddField("worker_id", workerId);
        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "get_tasks_worker.php", form))
        {
            yield return www.SendWebRequest();
            List<AppTask> result = new List<AppTask>();
            if (www.result == UnityWebRequest.Result.Success) {
                try {
                    string json = "{ \"tasks\": " + www.downloadHandler.text + "}";
                    var wrapper = JsonUtility.FromJson<TaskListWrapper>(json);
                    if (wrapper != null && wrapper.tasks != null)
                        foreach(var t in wrapper.tasks)
                            result.Add(new AppTask(t.idTask, t.title, t.worker_name, t.user_task_id, t.description, t.status_id, t.master_name, t.master_id));
                } catch {}
            }
            callback(result);
        }
    }

    // --- 5. ЭПИКИ И ПОДЗАДАЧИ ---
    public IEnumerator GetEpicsByMasterWeb(int masterId, Action<List<Epic>> callback)
    {
        WWWForm form = new WWWForm(); form.AddField("master_id", masterId);
        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "get_epics_master.php", form))
        {
            yield return www.SendWebRequest();
            List<Epic> result = new List<Epic>();
            if (www.result == UnityWebRequest.Result.Success) {
                try {
                    string json = "{ \"epics\": " + www.downloadHandler.text + "}";
                    var wrapper = JsonUtility.FromJson<EpicListWrapper>(json);
                    if (wrapper != null && wrapper.epics != null)
                        foreach(var e in wrapper.epics)
                            result.Add(new Epic(e.idEpic, e.title, e.description, e.chat_epic_id, e.master_name, e.master_id, e.chat_name, 0));
                } catch {}
            }
            callback(result);
        }
    }

    public IEnumerator GetSubTasksByEpicWeb(int epicId, int masterId, string masterName, Action<List<AppTask>> callback)
    {
        WWWForm form = new WWWForm(); form.AddField("epic_id", epicId);
        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "get_subtasks.php", form))
        {
            yield return www.SendWebRequest();
            List<AppTask> result = new List<AppTask>();
            if (www.result == UnityWebRequest.Result.Success) {
                try {
                    string json = "{ \"subtasks\": " + www.downloadHandler.text + "}";
                    var wrapper = JsonUtility.FromJson<SubTaskListWrapper>(json);
                    if (wrapper != null && wrapper.subtasks != null)
                        foreach(var s in wrapper.subtasks)
                            result.Add(new AppTask(s.idSubtask, s.title, s.worker_name, s.user_subtask_id, s.description, s.status_id, masterName, masterId));
                } catch {}
            }
            callback(result);
        }
    }

    // --- 6. ДОБАВЛЕНИЕ (POST) ---
    public IEnumerator AddTaskWeb(string title, string description, int masterId, int workerId, Action<int> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("title", title); form.AddField("description", description);
        form.AddField("master_id", masterId); form.AddField("worker_id", workerId);
        
        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "add_task.php", form)) {
            yield return www.SendWebRequest();
            int newId = 0;
            if (www.result == UnityWebRequest.Result.Success) int.TryParse(www.downloadHandler.text, out newId);
            callback?.Invoke(newId);
        }
    }

    public IEnumerator AddEpicWeb(string title, string description, int masterId, Action<int> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("title", title); form.AddField("description", description); form.AddField("master_id", masterId);
        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "add_epic.php", form)) {
            yield return www.SendWebRequest();
            int newId = 0;
            if (www.result == UnityWebRequest.Result.Success) int.TryParse(www.downloadHandler.text, out newId);
            callback?.Invoke(newId);
        }
    }

    public IEnumerator AddSubTaskWeb(string title, string description, int epicId, int workerId, Action<int> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("title", title); form.AddField("description", description);
        form.AddField("epic_id", epicId); form.AddField("worker_id", workerId);
        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "add_subtask.php", form)) {
            yield return www.SendWebRequest();
            int newId = 0;
            if (www.result == UnityWebRequest.Result.Success) int.TryParse(www.downloadHandler.text, out newId);
            callback?.Invoke(newId);
        }
    }

    // --- 7. ЧАТ ---
    public IEnumerator SendMessageWeb(int entityId, int userId, string message, string type)
    {
        WWWForm form = new WWWForm();
        form.AddField("entity_id", entityId); // ID задачи/эпика/подзадачи
        form.AddField("user_id", userId);
        form.AddField("message", message);
        form.AddField("type", type); // 'task', 'epic' или 'subtask'
        
        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "send_message.php", form)) 
        { 
            yield return www.SendWebRequest(); 
        }
    }

    public IEnumerator GetChatMessagesWeb(int entityId, string type, Action<List<ChatMessageData>> callback)
    {
        WWWForm form = new WWWForm(); 
        form.AddField("entity_id", entityId);
        form.AddField("type", type);

        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "get_chat.php", form))
        {
            yield return www.SendWebRequest();
            List<ChatMessageData> result = new List<ChatMessageData>();
            if (www.result == UnityWebRequest.Result.Success)
            {
                try {
                    var wrapper = JsonUtility.FromJson<ChatWrapper>(www.downloadHandler.text);
                    if (wrapper != null && wrapper.messages != null)
                        foreach (var msg in wrapper.messages) 
                            result.Add(new ChatMessageData { SenderName = msg.name, Text = msg.message, Time = msg.timestamp });
                } catch { }
            }
            callback(result);
        }
    }

    // --- 8. УТИЛИТЫ (ChangeStatus, Users) ---
    public IEnumerator ChangeStatusWeb(int taskId, int newStatusId)
    {
        WWWForm form = new WWWForm();
        form.AddField("task_id", taskId); form.AddField("status_id", newStatusId);
        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "change_status.php", form)) { yield return www.SendWebRequest(); }
    }

    // Админская функция
    public IEnumerator AddUserWeb(string name, string email, string password, int roleId) {
        WWWForm form = new WWWForm(); 
        form.AddField("name", name); 
        form.AddField("email", email); 
        form.AddField("password", password); 
        form.AddField("role_id", roleId);
        
        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "add_user.php", form)) { 
            yield return www.SendWebRequest();
            Debug.Log("Add User Response: " + www.downloadHandler.text);
        }
    }

    // Получить ID работника по email (для создания задачи)
    public IEnumerator GetUserIdByEmailWeb(string email, Action<int> callback) {
        WWWForm form = new WWWForm(); form.AddField("email", email);
        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "get_worker_id.php", form)) {
            yield return www.SendWebRequest();
            int id = 0;
            if(www.result == UnityWebRequest.Result.Success) int.TryParse(www.downloadHandler.text, out id);
            callback(id);
        }
    }

    // Получить список всех email-ов работников (для выпадающего списка)
    public IEnumerator GetWorkersEmailsWeb(Action<List<string>> callback) {
        using (UnityWebRequest www = UnityWebRequest.Get(serverUrl + "get_workers.php")) {
            yield return www.SendWebRequest();
            List<string> res = new List<string>();
            try {
                var wrapper = JsonUtility.FromJson<WorkerListWrapper>(www.downloadHandler.text);
                if (wrapper != null && wrapper.workers != null) res.AddRange(wrapper.workers);
            } catch {}
            callback(res);
        }
    }

    // Локальные методы для UI
    public List<string> LoadStatuses() { 
        return statusNames.Count > 0 ? statusNames : new List<string> { "Новая", "В работе", "Завершена", "Отложена" }; 
    }
    
    public string GetStatusById(int id) {
        string[] def = { "Новая", "В работе", "Завершена", "Отложена" };
        if (id > 0 && id <= def.Length) return def[id - 1];
        if (statusNames.Count >= id && id > 0) return statusNames[id-1];
        return "";
    }
}