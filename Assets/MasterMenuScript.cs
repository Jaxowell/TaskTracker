using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MasterMenuScript : MonoBehaviour
{
    [Header("Панели навигации")]
    // 0-Main, 1-TasksList, 2-TaskView, 3-NewTask, 4-EpicsList, 5-EpicView, 6-NewEpic, 7-NewSubTask, 8-SubTaskView
    [SerializeField] GameObject[] Menus; 

    [Header("Контейнеры")]
    [SerializeField] GameObject TaskPanel;
    [SerializeField] GameObject SubTaskPanel; 
    [SerializeField] GameObject EpicPanel;

    [Header("Префабы")]
    [SerializeField] GameObject TaskPrefab;
    [SerializeField] GameObject EpicPrefab;

    [Header("Главный скрипт")]
    [SerializeField] MenuScript Mscript;

    [Header("View Task")]
    [SerializeField] GameObject TaskTitle;
    [SerializeField] GameObject WorkerName;
    [SerializeField] GameObject TaskColor;
    [SerializeField] GameObject TaskStatus;
    [SerializeField] GameObject TaskDescription;
    [SerializeField] Button openChatButtonTask; 

    [Header("View Epic")]
    [SerializeField] GameObject EpicTitle;
    [SerializeField] Button openChatButtonEpic; 

    [Header("View SubTask")]
    [SerializeField] GameObject SubTaskTitle;
    [SerializeField] GameObject SubTaskWorkerName;
    [SerializeField] GameObject SubTaskColor;
    [SerializeField] GameObject SubTaskStatus;
    [SerializeField] GameObject SubTaskDescription;
    [SerializeField] Button openChatButtonSubTask;

    [Header("Inputs")]
    [SerializeField] TMP_InputField titleTask;
    [SerializeField] TMP_InputField descriptionTask;
    [SerializeField] TMP_Dropdown WorkerTaskDropDown;
    [SerializeField] TMP_InputField titleEpic;
    [SerializeField] TMP_InputField descriptionEpic;
    [SerializeField] TMP_InputField titleSubTask;
    [SerializeField] TMP_InputField descriptionSubTask;
    [SerializeField] TMP_Dropdown WorkerSubTaskDropDown;

    List<AppTask> tasksByMaster = new List<AppTask>();
    List<Epic> epicsByMaster = new List<Epic>();
    
    int activeEpicId = 0; 
    int activeEpicIndex = 0; 

    public void LoadMenu()
    {
        StartCoroutine(Mscript.db.LoadColorsWeb());
        
        StartCoroutine(Mscript.db.GetWorkersEmailsWeb((emails) => 
        {
            if(WorkerTaskDropDown) { WorkerTaskDropDown.ClearOptions(); WorkerTaskDropDown.AddOptions(emails); }
            if(WorkerSubTaskDropDown) { WorkerSubTaskDropDown.ClearOptions(); WorkerSubTaskDropDown.AddOptions(emails); }
        }));

        StartCoroutine(LoadAllDataRoutine());
    }

    IEnumerator LoadAllDataRoutine()
    {
        // 1. TASKS
        yield return StartCoroutine(Mscript.db.GetTasksByMasterWeb(Mscript.activeUserId, (tasks) => 
        {
            tasksByMaster = tasks;
            foreach(Transform child in TaskPanel.transform) Destroy(child.gameObject);

            for (int i = 0; i < tasksByMaster.Count; i++)
            {
                string c = GetColorSafe(tasksByMaster[i].statusId);
                tasksByMaster[i].PutInPanel(TaskPrefab, TaskPanel, c, true);
                
                int idx = i;
                tasksByMaster[i].TaskButton.GetComponent<Button>().onClick.AddListener(() => ShowTask(idx));
            }
        }));

        // 2. EPICS
        yield return StartCoroutine(Mscript.db.GetEpicsByMasterWeb(Mscript.activeUserId, (epics) => 
        {
            epicsByMaster = epics;
            foreach(Transform child in EpicPanel.transform) Destroy(child.gameObject);
        }));

        // 3. SUBTASKS
        yield return StartCoroutine(LoadSubTasksRecursively(0));
    }

    IEnumerator LoadSubTasksRecursively(int index)
    {
        if (index >= epicsByMaster.Count) 
        {
            SwitchMenu(0); 
            yield break;
        }

        Epic epic = epicsByMaster[index];
        yield return StartCoroutine(Mscript.db.GetSubTasksByEpicWeb(epic.id, Mscript.activeUserId, "Me", (subTasks) => 
        {
            epic.subTasks = subTasks;
            epic.PutInPanel(EpicPrefab, EpicPanel, true);
            
            int eIdx = index;
            epic.EpicButton.GetComponent<Button>().onClick.AddListener(() => ShowEpic(eIdx));
        }));

        StartCoroutine(LoadSubTasksRecursively(index + 1));
    }

    void ShowTask(int idx)
    {
        AppTask t = tasksByMaster[idx];
        TaskTitle.GetComponent<TMP_Text>().text = t.title;
        WorkerName.GetComponent<TMP_Text>().text = "Worker: " + t.workerName;
        TaskDescription.GetComponent<TMP_Text>().text = t.description;
        TaskStatus.GetComponent<TMP_Text>().text = Mscript.db.GetStatusById(t.statusId);
        
        string c = GetColorSafe(t.statusId);
        if(ColorUtility.TryParseHtmlString("#" + c, out Color newColor))
             TaskColor.GetComponent<Image>().color = newColor;

        if (openChatButtonTask != null)
        {
            openChatButtonTask.onClick.RemoveAllListeners();
            // ИСПРАВЛЕНИЕ: Добавлен "task"
            openChatButtonTask.onClick.AddListener(() => ChatScript.Instance.OpenChat(t.id, Mscript.activeUserId, "task"));
        }

        SwitchMenu(2);
    }

    void ShowEpic(int idx)
    {
        activeEpicIndex = idx;
        Epic e = epicsByMaster[idx];
        activeEpicId = e.id;

        EpicTitle.GetComponent<TMP_Text>().text = e.title;
        
        foreach(Transform child in SubTaskPanel.transform) Destroy(child.gameObject);
        
        if(e.subTasks != null)
        {
            for (int i = 0; i < e.subTasks.Count; i++)
            {
                var sub = e.subTasks[i];
                string c = GetColorSafe(sub.statusId);
                sub.PutInPanel(TaskPrefab, SubTaskPanel, c, true);

                int subIdx = i;
                sub.TaskButton.GetComponent<Button>().onClick.AddListener(() => ShowSubTask(e, subIdx));
            }
        }

        if (openChatButtonEpic != null)
        {
            openChatButtonEpic.onClick.RemoveAllListeners();
            // ИСПРАВЛЕНИЕ: Добавлен "epic"
            openChatButtonEpic.onClick.AddListener(() => ChatScript.Instance.OpenChat(activeEpicId, Mscript.activeUserId, "epic"));
        }

        SwitchMenu(5);
    }

    void ShowSubTask(Epic e, int subIndex)
    {
        AppTask sub = e.subTasks[subIndex];

        if(SubTaskTitle) SubTaskTitle.GetComponent<TMP_Text>().text = sub.title;
        if(SubTaskWorkerName) SubTaskWorkerName.GetComponent<TMP_Text>().text = "Worker: " + sub.workerName;
        if(SubTaskDescription) SubTaskDescription.GetComponent<TMP_Text>().text = sub.description;
        if(SubTaskStatus) SubTaskStatus.GetComponent<TMP_Text>().text = Mscript.db.GetStatusById(sub.statusId);

        if(SubTaskColor)
        {
            string c = GetColorSafe(sub.statusId);
            if(ColorUtility.TryParseHtmlString("#" + c, out Color newColor))
                SubTaskColor.GetComponent<Image>().color = newColor;
        }

        if (openChatButtonSubTask != null)
        {
            openChatButtonSubTask.onClick.RemoveAllListeners();
            // ИСПРАВЛЕНИЕ: Добавлен "subtask"
            openChatButtonSubTask.onClick.AddListener(() => ChatScript.Instance.OpenChat(sub.id, Mscript.activeUserId, "subtask"));
        }

        SwitchMenu(8); 
    }

    public void AddTask()
    {
        if(WorkerTaskDropDown.options.Count == 0) return;
        string email = WorkerTaskDropDown.options[WorkerTaskDropDown.value].text;
        
        StartCoroutine(Mscript.db.GetUserIdByEmailWeb(email, (workerId) => 
        {
            StartCoroutine(Mscript.db.AddTaskWeb(titleTask.text, descriptionTask.text, Mscript.activeUserId, workerId, (newId) => 
            {
                LoadMenu(); 
            }));
        }));
    }

    public void AddEpic()
    {
        StartCoroutine(Mscript.db.AddEpicWeb(titleEpic.text, descriptionEpic.text, Mscript.activeUserId, (newId) => 
        {
            LoadMenu();
        }));
    }

    public void AddSubTask()
    {
        if(WorkerSubTaskDropDown.options.Count == 0) return;
        string email = WorkerSubTaskDropDown.options[WorkerSubTaskDropDown.value].text;

        StartCoroutine(Mscript.db.GetUserIdByEmailWeb(email, (workerId) => 
        {
            StartCoroutine(Mscript.db.AddSubTaskWeb(titleSubTask.text, descriptionSubTask.text, activeEpicId, workerId, (newId) => 
            {
                LoadMenu(); 
            }));
        }));
    }

    public void Exit() { Mscript.ChangeMenu(1); }
    
    public void SwitchMenu(int id) 
    {
        foreach (var m in Menus) if(m) m.SetActive(false);
        if(id < Menus.Length && Menus[id]) Menus[id].SetActive(true);
    }
    
    public void OpenMainMenu() => SwitchMenu(0);
    public void OpenEpicsMenu() => SwitchMenu(4);
    public void BackToEpic() => SwitchMenu(5);

    string GetColorSafe(int id)
    {
        if (Mscript.db.statusColors != null && id > 0 && id <= Mscript.db.statusColors.Length)
            return Mscript.db.statusColors[id - 1];
        return "FFFFFF";
    }
}