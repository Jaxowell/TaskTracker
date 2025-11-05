using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WorkerMenu : MonoBehaviour
{
    //public DBScript db = new DBScript();
    [SerializeField] MenuScript Mscript;
    [SerializeField] GameObject TaskLayout;
    [SerializeField] GameObject TaskPrefab;
    List<GameObject> taskLists = new List<GameObject>();
    [SerializeField] private GameObject ChatWindow;

    public void ExitToMenu()
    {
        Debug.Log("ExitToMenu called");
        
        // Очищаем список задач
        foreach(var task in taskLists)
        {
            Destroy(task);
        }
        taskLists.Clear();
        
        // Проверяем ссылку на MenuScript
        if (Mscript == null)
        {
            Debug.LogError("MenuScript reference is missing! Please assign it in Inspector.");
            return;
        }
        
        Debug.Log("Calling ChangeMenu(1)");
        Mscript.ChangeMenu(1);
    }
    // Start is called before the first frame update
    public void LoadMenu()
    {
        Debug.Log("������");
        FullTaskLayout(Mscript.activeUserId);
    }
    void FullTaskLayout(int id)
    {
        List<string> tasks = Mscript.db.GetTaskByUser(id);
        for (int i = 0; i < tasks.Count; i++)
        {
            GameObject taskButton = Instantiate(TaskPrefab);//, spawnPosition, Quaternion.identity);
            taskButton.name = $"TaskButton_{i}";
            taskButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = tasks[i].ToString();
            taskButton.transform.SetParent(TaskLayout.transform);
            taskLists.Add(taskButton);
            Debug.Log(tasks[i]);
        }
    }
    
    public void OpenChat()
    {
        if (ChatScript.Instance != null)
            ChatScript.Instance.OpenChat();
    }

}
