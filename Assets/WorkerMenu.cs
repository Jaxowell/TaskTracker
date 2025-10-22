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
    // Start is called before the first frame update
    public void LoadMenu()
    {
        Debug.Log("ГРузим");
        FullTaskLayout(Mscript.activeUserId);
    }
    void FullTaskLayout(int id)
    {
        List<string> tasks = Mscript.db.GetTaskByUser(id);
        for(int i = 0; i<tasks.Count; i++)
        {
            GameObject taskButton = Instantiate(TaskPrefab);//, spawnPosition, Quaternion.identity);
            taskButton.name = $"TaskButton_{i}"; 
            taskButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text= tasks[i].ToString();
            taskButton.transform.SetParent(TaskLayout.transform);
            taskLists.Add(taskButton);
            Debug.Log(tasks[i]);
        }
    }

}
