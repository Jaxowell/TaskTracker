using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
//using System.Runtime.InteropServices;

public class MasterMenuScript : MonoBehaviour
{
    //int activeMenuIdd = 0;
    //[SerializeField] GameObject MainMenu;//0
    //[SerializeField] GameObject CreateTaskMenu;//1
    //[SerializeField] GameObject CreateEpicMenu;//2
    //[SerializeField] GameObject StatisticMenu;//3

    [SerializeField] GameObject[] Menus;//0-main,1-tasks, 2- the task, 3 -new task, 4- epics, 5 - the epic, 6- new epic, 7 - new subtask, 8 - the subtask

    //1-epics, 2-the epic, 3-new epic, 4-tasks, 5-the task, 6-new task 7-stat, 

    [SerializeField] GameObject TaskPrefab;
    [SerializeField] GameObject EpicPrefab;

    [SerializeField] GameObject TaskPanel;
    [SerializeField] GameObject EpicPanel;

    int activeMenuId = 0;
    //int activeTaskId = 0;

    [SerializeField] MenuScript Mscript;
    public DBScript db;

    [SerializeField] TextMeshProUGUI stat;

    List<Task> tasksByMaster = new List<Task>();
    

    List<Epic> epicsByMaster = new List<Epic>();
    //[SerializeField] MenuScript menuScript;

    // Start is called before the first frame update
    public void Start()
    {
        //db = Mscript.db;
    }

    public void LoadMenu()
    {
        db = Mscript.db;
        //CreateTaskMenu.SetActive(false);
        //CreateEpicMenu.SetActive(false);
        //StatisticMenu.SetActive(false);
        //MainMenu.SetActive(true);


        tasksByMaster = db.GetTasksByMaster(Mscript.activeUserId);
        Debug.Log("Загрузили "+ tasksByMaster.Count+ " задач");
        for (int i = 0; i < tasksByMaster.Count; i++)
        {
            Debug.Log(tasksByMaster[i].Print());
            tasksByMaster[i].PutInPanel(TaskPrefab, TaskPanel, db.statusColors[tasksByMaster[i].statusId-1],true);
            UnityEngine.UI.Button button = tasksByMaster[i].TaskButton.GetComponent<UnityEngine.UI.Button>();
            int taskIndex = i;
            button.onClick.AddListener(() =>
            {
                ShowTask(taskIndex);
            });
        }
        LoadWorkers();

        epicsByMaster =db.GetEpicsByMaster(Mscript.activeUserId);
        Debug.Log("Загрузили " + epicsByMaster.Count + " задач");
        for (int i = 0; i < epicsByMaster.Count; i++)
        {
            //Debug.Log(tasksByMaster[i].Print());
            epicsByMaster[i].PutInPanel(EpicPrefab, EpicPanel, true);//(TaskPrefab, TaskPanel, db.statusColors[tasksByMaster[i].statusId - 1], true);
            UnityEngine.UI.Button button = epicsByMaster[i].EpicButton.GetComponent<UnityEngine.UI.Button>();
            int epicIndex = i;
            button.onClick.AddListener(() =>
            {
                ShowEpic(epicIndex);
            });
        }
        for (int i = 0; i < Menus.Length; i++)
        {
            Menus[i].SetActive(false);
        }

        Menus[activeMenuId].SetActive(true);

    }

    [SerializeField] GameObject TaskTitle;
    [SerializeField] GameObject WorkerName;
    [SerializeField] GameObject TaskColor;
    [SerializeField] GameObject TaskStatus;
    [SerializeField] GameObject TaskDescription;
    [SerializeField] GameObject MasterTaskName;

    [SerializeField] GameObject EpicTitle;
    //[SerializeField] GameObject ChatName;
    //[SerializeField] GameObject TaskColor;
    //[SerializeField] GameObject TaskStatus;
    [SerializeField] GameObject EpicDescription;
    //[SerializeField] GameObject MasterEpicName;
    private void ShowTask(int id)
    {
        //id = 0;
        TaskTitle.GetComponent<TMP_Text>().text = tasksByMaster[id].title;
        WorkerName.GetComponent<TMP_Text>().text = "Исполнитель: "+tasksByMaster[id].workerName;
        TaskDescription.GetComponent<TMP_Text>().text = tasksByMaster[id].description;
        MasterTaskName.GetComponent<TMP_Text>().text = "Тимлид: " + tasksByMaster[id].masterName;
        TaskStatus.GetComponent<TMP_Text>().text = db.GetStatusById(tasksByMaster[id].statusId);

        string colorCode = "#" + db.statusColors[tasksByMaster[id].statusId - 1];
        UnityEngine.ColorUtility.TryParseHtmlString(colorCode, out Color newColor);
        TaskColor.GetComponent<Image>().color = newColor;
        SwitchMenu(2);
    }
    private void ShowEpic(int id)
    {
        //id = 0;
        EpicTitle.GetComponent<TMP_Text>().text = epicsByMaster[id].title;
        //ChatName.GetComponent<TMP_Text>().text = tasksByMaster[id].workerName;
        EpicDescription.GetComponent<TMP_Text>().text = epicsByMaster[id].description;
        //MasterEpicName.GetComponent<TMP_Text>().text = "Тимлид: " + tasksByMaster[id].masterName;
        //TaskStatus.GetComponent<TMP_Text>().text = db.GetStatusById(tasksByMaster[id].statusId);

        //string colorCode = "#" + db.statusColors[tasksByMaster[id].statusId - 1];
        //UnityEngine.ColorUtility.TryParseHtmlString(colorCode, out Color newColor);
        //TaskColor.GetComponent<Image>().color = newColor;
        SwitchMenu(5);
    }
    public void Exit()
    {
        Mscript.ChangeMenu(1);
        for (int i = 0; i < tasksByMaster.Count; i++)
        {
            Destroy(tasksByMaster[i].TaskButton);
        }
        tasksByMaster.Clear();
        for (int i = 0; i < epicsByMaster.Count; i++)
        {
            Destroy(epicsByMaster[i].EpicButton);
        }
        epicsByMaster.Clear();
    }

    [SerializeField] TMP_InputField titleTask;
    [SerializeField] TMP_InputField descriptionTask;
    [SerializeField] TMP_Dropdown WorkerTaskDropDown;
    public void AddTask()
    {
        bool inputProblem = false;
        if (titleTask.text == "")
        {
            titleTask.placeholder.GetComponent<TextMeshProUGUI>().text = "Укажите название!";
            inputProblem = true;
        }
        if (descriptionTask.text == "")
        {
            descriptionTask.placeholder.GetComponent<TextMeshProUGUI>().text = "Введите описание!";
            inputProblem = true;
        }
        if (WorkerTaskDropDown.value == 0)
        {
            //descriptionMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "Введите описание!";
            inputProblem = true;
        }
        int workerId = db.GetUserIdByEmail(WorkerTaskDropDown.options[WorkerTaskDropDown.value].text);
        if (!inputProblem)
        {
            int masterId = Mscript.activeUserId;
            db.AddTask(titleTask.text, descriptionTask.text, masterId, workerId);
            //passwordMenu.text = "";

            Task task = new Task(db.GetLastTaskId(), titleTask.text, db.GetNameById(workerId), workerId, descriptionTask.text, 1, db.GetNameById(masterId), masterId); ;
            tasksByMaster.Add(task);
            task.PutInPanel(TaskPrefab, TaskPanel, db.statusColors[0], true);

            Debug.Log(tasksByMaster[tasksByMaster.Count - 1].Print());
            //tasksByMaster[tasksByMaster.Count - 1].PutTaskInPanel(TaskPrefab, TaskPanel, db.statusColors[tasksByMaster[tasksByMaster.Count - 1].statusId - 1]);
            UnityEngine.UI.Button button = tasksByMaster[tasksByMaster.Count - 1].TaskButton.GetComponent<UnityEngine.UI.Button>();
            int taskIndex = tasksByMaster.Count - 1;
            button.onClick.AddListener(() =>
            {
                ShowTask(taskIndex);
            });

            titleTask.text = "";
            descriptionTask.text = "";
            WorkerTaskDropDown.value = 0;
            Debug.Log($"задач: {tasksByMaster.Count} ");
            //tasksByMaster.
        }
    }
    [SerializeField] TMP_InputField titleSubTask;
    [SerializeField] TMP_InputField descriptionSubTask;
    [SerializeField] TMP_Dropdown WorkerSubTaskDropDown;
    public void AddSubTask()
    {
        bool inputProblem = false;
        if (titleSubTask.text == "")
        {
            titleSubTask.placeholder.GetComponent<TextMeshProUGUI>().text = "Укажите название!";
            inputProblem = true;
        }
        if (descriptionSubTask.text == "")
        {
            descriptionSubTask.placeholder.GetComponent<TextMeshProUGUI>().text = "Введите описание!";
            inputProblem = true;
        }
        if (WorkerSubTaskDropDown.value == 0)
        {
            //descriptionMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "Введите описание!";
            inputProblem = true;
        }
        int workerId = db.GetUserIdByEmail(WorkerSubTaskDropDown.options[WorkerSubTaskDropDown.value].text);
        if (!inputProblem)
        {
            int masterId = Mscript.activeUserId;
            ////////////////////////////////////////////////db.AddSubTask(titleSubTask.text, descriptionSubTask.text, masterId, workerId);
            //passwordMenu.text = "";

            ///////////////Task task = new Task(db.GetLastSubTaskId(), titleSubTask.text, db.GetNameById(workerId), workerId, descriptionSubTask.text, 1, db.GetNameById(masterId), masterId); ;
            //////////epicsByMaster.Add(task);
            ///////////////task.PutInPanel(TaskPrefab, TaskPanel, db.statusColors[0], true);

            //Debug.Log(tasksByMaster[tasksByMaster.Count - 1].Print());
            //tasksByMaster[tasksByMaster.Count - 1].PutTaskInPanel(TaskPrefab, TaskPanel, db.statusColors[tasksByMaster[tasksByMaster.Count - 1].statusId - 1]);
            /**
            UnityEngine.UI.Button button = tasksByMaster[tasksByMaster.Count - 1].TaskButton.GetComponent<UnityEngine.UI.Button>();
            int taskIndex = tasksByMaster.Count - 1;
            button.onClick.AddListener(() =>
            {
                ShowTask(taskIndex);
            });
            */

            titleSubTask.text = "";
            descriptionSubTask.text = "";
            WorkerSubTaskDropDown.value = 0;
            //Debug.Log($"подзадач: {tasksByMaster.Count} ");
            //tasksByMaster.
        }
    }
    [SerializeField] TMP_InputField titleEpic;
    [SerializeField] TMP_InputField descriptionEpic;
    [SerializeField] TMP_Dropdown WorkerEpicDropDown;
    public void AddEpic()
    {
        bool inputProblem = false;
        if (titleEpic.text == "")
        {
            titleEpic.placeholder.GetComponent<TextMeshProUGUI>().text = "Укажите название!";
            inputProblem = true;
        }
        if (descriptionEpic.text == "")
        {
            descriptionEpic.placeholder.GetComponent<TextMeshProUGUI>().text = "Введите описание!";
            inputProblem = true;
        }
        //int workerId = db.GetUserIdByEmail(WorkerDropDown.options[WorkerDropDown.value].text);
        if (!inputProblem)
        {
            int masterId =Mscript.activeUserId;
            Debug.Log("неа");
            //db.AddEpic(titleEpic.text, descriptionEpic.text, masterId);//, workerId);
            titleEpic.text = "";
            descriptionEpic.text = "";
            //passwordMenu.text = "";

        }
    }
    //void LoadStat()
    //{
    //    int masterId = Mscript.activeUserId;
    //    string answer = "";
    //    List<string> tasks = db.AIGetTaskByMaster(masterId);
    //    for (int i = 0; i < tasks.Count; i+=2)
    //    {
    //        answer += "Задача " + tasks[i] + " выполняет " + tasks[i+1] +";\n";
    //    }
    //    List<string> epic = db.GetEpicByMaster(masterId);
    //    for (int i = 0; i < epic.Count; i++)
    //    {
    //        answer += "Эпик " + epic[i] + ";\n";
    //    }
    //    //бежим по таскам, потом по эпикам и запоминаем всех у кого master_id= masterId
    //    stat.text = answer;
    //}
    void LoadWorkers()
    {
        WorkerTaskDropDown.ClearOptions();
        WorkerEpicDropDown.ClearOptions();
        List<string> workers=db.GetUserEmailsByRole(3);
        workers.Insert(0,"Исполнитель");
        WorkerTaskDropDown.AddOptions(workers);
        WorkerEpicDropDown.AddOptions(workers);
        //Debug.Log("Загрузили!");
    }
    public void SwitchMenu(int newActiveId)
    {
        Menus[activeMenuId].SetActive(false);
        Menus[newActiveId].SetActive(true);
        activeMenuId = newActiveId;
    }
    //public void ChangeMenu(int newId)
    //{
    //    Debug.Log(activeMenuId + " --> " + newId);
    //    switch (activeMenuId)
    //    {
    //        case 0:
    //            MainMenu.SetActive(false);
    //            break;
    //        case 1:
    //            CreateTaskMenu.SetActive(false);
    //            break;
    //        case 2:
    //            CreateEpicMenu.SetActive(false);
    //            break;
    //        case 3:
    //            StatisticMenu.SetActive(false);
    //            break;
    //    }


    //    switch (newId)
    //    {
    //        case 0:
    //            MainMenu.SetActive(true);
    //            break;
    //        case 1:
    //            CreateTaskMenu.SetActive(true);
    //            break;
    //        case 2:
    //            CreateEpicMenu.SetActive(true);
    //            break;
    //        case 3:
    //            LoadStat();
    //            StatisticMenu.SetActive(true);
    //            break;
    //    }
    //    activeMenuId = newId;
    //}
}
