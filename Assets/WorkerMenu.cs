using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkerMenu : MonoBehaviour
{
    [Header("Панели")]
    [SerializeField] GameObject[] Menus; // 0-Load, 1-TasksList, 2-TaskView

    [Header("Контейнер списка")]
    [SerializeField] GameObject TaskPanel;
    [SerializeField] GameObject TaskPrefab;
    [SerializeField] MenuScript Mscript;

    [Header("Просмотр задачи")]
    [SerializeField] GameObject TaskTitle;
    [SerializeField] GameObject WorkerName;
    [SerializeField] GameObject TaskColor;
    [SerializeField] TMP_Dropdown StatusDropDown;
    [SerializeField] GameObject TaskDescription;
    [SerializeField] GameObject MasterName;
    [SerializeField] Button openChatButton; // Кнопка чата

    List<AppTask> tasksByWorker = new List<AppTask>();
    int activeTaskIndex = 0;

    public void LoadMenu()
    {
        StartCoroutine(LoadDataRoutine());
    }

    IEnumerator LoadDataRoutine()
    {
        // 1. Цвета
        yield return StartCoroutine(Mscript.db.LoadColorsWeb());
        
        // 2. Задачи
        yield return StartCoroutine(Mscript.db.GetTasksByWorkerWeb(Mscript.activeUserId, (tasks) => 
        {
            tasksByWorker = tasks;
            foreach(Transform child in TaskPanel.transform) Destroy(child.gameObject);

            for (int i = 0; i < tasksByWorker.Count; i++)
            {
                string c = GetColorSafe(tasksByWorker[i].statusId);
                tasksByWorker[i].PutInPanel(TaskPrefab, TaskPanel, c, false);
                
                int index = i;
                tasksByWorker[i].TaskButton.GetComponent<Button>().onClick.AddListener(() => ShowTask(index));
            }
            StatusDropDown.ClearOptions();
            StatusDropDown.AddOptions(Mscript.db.LoadStatuses());

            SwitchMenu(1); 
        }));
    }

    void ShowTask(int index)
    {
        activeTaskIndex = index;
        AppTask t = tasksByWorker[index];

        TaskTitle.GetComponent<TMP_Text>().text = t.title;
        WorkerName.GetComponent<TMP_Text>().text = t.workerName;
        TaskDescription.GetComponent<TMP_Text>().text = t.description;
        MasterName.GetComponent<TMP_Text>().text = "Master: " + t.masterName;
        
        StatusDropDown.SetValueWithoutNotify(t.statusId - 1);
        UpdateColorBox(t.statusId);

        // --- ИСПРАВЛЕНИЕ ЗДЕСЬ ---
        if (openChatButton != null)
        {
            openChatButton.onClick.RemoveAllListeners();
            // Добавили "task"
            openChatButton.onClick.AddListener(() => 
            {
                if (ChatScript.Instance != null)
                    ChatScript.Instance.OpenChat(t.id, Mscript.activeUserId, "task");
            });
        }

        SwitchMenu(2);
    }

    public void ChangeStatus()
    {
        int dropdownIndex = StatusDropDown.value; 
        int newStatusId = dropdownIndex + 1;
        int taskId = tasksByWorker[activeTaskIndex].id;

        UpdateColorBox(newStatusId);
        StartCoroutine(Mscript.db.ChangeStatusWeb(taskId, newStatusId));
    }

    void UpdateColorBox(int statusId)
    {
        string c = GetColorSafe(statusId);
        if(ColorUtility.TryParseHtmlString("#" + c, out Color newColor))
             TaskColor.GetComponent<Image>().color = newColor;
    }

    // --- ИСПРАВЛЕНИЕ ЗДЕСЬ (Если этот метод вызывается старой кнопкой) ---
    public void OpenChat()
    {
        if (ChatScript.Instance != null && tasksByWorker.Count > 0)
        {
            int taskId = tasksByWorker[activeTaskIndex].id;
            // Добавили "task"
            ChatScript.Instance.OpenChat(taskId, Mscript.activeUserId, "task");
        }
    }

    public void Exit() { Mscript.ChangeMenu(1); }

    void SwitchMenu(int id)
    {
        foreach (var m in Menus) if(m) m.SetActive(false);
        if(id < Menus.Length) Menus[id].SetActive(true);
    }

    string GetColorSafe(int id)
    {
        if (Mscript.db.statusColors != null && id > 0 && id <= Mscript.db.statusColors.Length)
            return Mscript.db.statusColors[id - 1];
        return "FFFFFF";
    }
}