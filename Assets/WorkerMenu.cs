using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkerMenu : MonoBehaviour
{
    [SerializeField] GameObject[] Menus;//0-main,1-tasks, 2- the task 
    [SerializeField] GameObject TaskPanel;
    [SerializeField] GameObject TaskPrefab;

    [SerializeField] MenuScript Mscript;
    int activeMenuId = 0;
    int activeTaskId = 0;
    public DBScript db;
    List<Task> tasksByWorker = new List<Task>();

    [SerializeField] GameObject TaskTitle;
    [SerializeField] GameObject WorkerName;
    [SerializeField] GameObject TaskColor;
    [SerializeField] TMP_Dropdown StatusDropDown;
    //[SerializeField] GameObject TaskStatus;
    [SerializeField] GameObject TaskDescription;
    [SerializeField] GameObject MasterName;
    // Start is called before the first frame update
    public void Start()
    {
    }

    public void Exit()
    {
        Mscript.ChangeMenu(1);
        for (int i = 0; i < tasksByWorker.Count; i++)
        {
            Destroy(tasksByWorker[i].TaskButton);
        }
        tasksByWorker.Clear();
    }

    private void ShowTask(int id)
    {
        activeTaskId= id;
        //id = 0;
        TaskTitle.GetComponent<TMP_Text>().text = tasksByWorker[id].title;
        WorkerName.GetComponent<TMP_Text>().text = "Исполнитель: " + tasksByWorker[id].workerName;
        TaskDescription.GetComponent<TMP_Text>().text = tasksByWorker[id].description;
        MasterName.GetComponent<TMP_Text>().text = "Тимлид: " + tasksByWorker[id].masterName;
        //TaskStatus.GetComponent<TMP_Text>().text = db.GetStatusById(tasksByMaster[id].statusId);
        StatusDropDown.SetValueWithoutNotify(tasksByWorker[id].statusId-1);

        string colorCode = "#" + db.statusColors[tasksByWorker[id].statusId - 1];
        UnityEngine.ColorUtility.TryParseHtmlString(colorCode, out Color newColor);
        TaskColor.GetComponent<Image>().color = newColor;
        SwitchMenu(2);
    }
    void LoadStatuses()
    {
        StatusDropDown.ClearOptions();
        List<string> statuses = db.LoadStatuses();
        //statuses.Insert(0, "Статус");
        StatusDropDown.AddOptions(statuses);
        //Debug.Log("Загрузили!");
    }
    public void ChangeStatus()
    {
        string colorCode = "#" + db.statusColors[StatusDropDown.value];
        Debug.Log($"{colorCode} должен соответствовать {StatusDropDown.value+1}");
        UnityEngine.ColorUtility.TryParseHtmlString(colorCode, out Color newColor);
        TaskColor.GetComponent<Image>().color = newColor;
        tasksByWorker[activeTaskId].ChangeColor(colorCode);
        db.ChangeStatus(tasksByWorker[activeTaskId].id, StatusDropDown.value+1);
    }
    public void LoadMenu()
    {
        db = Mscript.db;
        tasksByWorker = db.GetTasksByWorker(Mscript.activeUserId);
        Debug.Log("Загрузили " + tasksByWorker.Count + " задач");
        for (int i = 0; i < tasksByWorker.Count; i++)
        {
            Debug.Log(tasksByWorker[i].Print());
            tasksByWorker[i].PutTaskInPanel(TaskPrefab, TaskPanel, db.statusColors[tasksByWorker[i].statusId-1],false);
            UnityEngine.UI.Button button = tasksByWorker[i].TaskButton.GetComponent<UnityEngine.UI.Button>();
            int taskIndex = i;
            button.onClick.AddListener(() =>
            {
                Debug.Log($"Это клик {taskIndex}");
                ShowTask(taskIndex);
            });
        }
        for (int i = 0; i < Menus.Length; i++)
        {
            Menus[i].SetActive(false);
        }
        LoadStatuses();
        Menus[activeMenuId].SetActive(true);
    }
    public void SwitchMenu(int newActiveId)
    {
        Menus[activeMenuId].SetActive(false);
        Menus[newActiveId].SetActive(true);
        activeMenuId = newActiveId;
    }

}
