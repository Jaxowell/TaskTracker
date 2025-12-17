using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MasterMenuScript : MonoBehaviour
{
    int activeMenuId = 0;
    public DBScript db = new DBScript();
    
    [SerializeField] GameObject MainMenu;
    [SerializeField] GameObject CreateTaskMenu;
    [SerializeField] GameObject CreateEpicMenu;
    [SerializeField] GameObject StatisticMenu;

    [SerializeField] TextMeshProUGUI stat; // Текст для статистики

    [SerializeField] TMP_InputField titleTask;
    [SerializeField] TMP_InputField descriptionTask;
    
    [SerializeField] TMP_InputField titleEpic;
    [SerializeField] TMP_InputField descriptionEpic;

    [SerializeField] TMP_Dropdown WorkerDropDown;
    
    [SerializeField] MenuScript menuScript; 

    public void Start()
    {
        LoadWorkers();
        CreateTaskMenu.SetActive(false);
        CreateEpicMenu.SetActive(false);
        StatisticMenu.SetActive(false);
        MainMenu.SetActive(true);
    }

    public void AddTask()
    {
        bool inputProblem = false;
        if (string.IsNullOrEmpty(titleTask.text))
        {
            if(titleTask.placeholder.GetComponent<TextMeshProUGUI>())
                titleTask.placeholder.GetComponent<TextMeshProUGUI>().text = "Введите название!";
            inputProblem = true;
        }
        if (string.IsNullOrEmpty(descriptionTask.text))
        {
            if(descriptionTask.placeholder.GetComponent<TextMeshProUGUI>())
                descriptionTask.placeholder.GetComponent<TextMeshProUGUI>().text = "Введите описание!";
            inputProblem = true;
        }
        
        // Проверка Dropdown
        if (WorkerDropDown.options.Count > 0)
        {
            string workerEmail = WorkerDropDown.options[WorkerDropDown.value].text;
            
            // Сначала получаем ID работника по email (асинхронно)
            StartCoroutine(db.GetUserIdByEmailWeb(workerEmail, (workerId) => 
            {
                if (!inputProblem && workerId > 0)
                {
                    int masterId = menuScript.activeUserId;
                    // Потом отправляем задачу
                    StartCoroutine(db.AddTaskWeb(titleTask.text, descriptionTask.text, masterId, workerId));
                    
                    // Очистка
                    titleTask.text = "";
                    descriptionTask.text = "";
                    WorkerDropDown.value = 0;
                }
            }));
        }
    }

    public void AddEpic()
    {
        bool inputProblem = false;
        if (string.IsNullOrEmpty(titleEpic.text))
        {
            if(titleEpic.placeholder.GetComponent<TextMeshProUGUI>())
                titleEpic.placeholder.GetComponent<TextMeshProUGUI>().text = "Введите название!";
            inputProblem = true;
        }
        if (string.IsNullOrEmpty(descriptionEpic.text))
        {
            if(descriptionEpic.placeholder.GetComponent<TextMeshProUGUI>())
                descriptionEpic.placeholder.GetComponent<TextMeshProUGUI>().text = "Введите описание!";
            inputProblem = true;
        }

        if (!inputProblem)
        {
            int masterId = menuScript.activeUserId;
            StartCoroutine(db.AddEpicWeb(titleEpic.text, descriptionEpic.text, masterId));
            
            titleEpic.text = "";
            descriptionEpic.text = "";
        }
    }

    void LoadStat()
    {
        int masterId = menuScript.activeUserId;
        StartCoroutine(db.GetStatsWeb(masterId, (text) => 
        {
            if(stat != null) stat.text = text;
        }));
    }

    void LoadWorkers()
    {
        StartCoroutine(db.GetWorkersEmailsWeb((emails) => 
        {
            WorkerDropDown.ClearOptions();
            WorkerDropDown.AddOptions(emails);
        }));
    }

    public void ChangeMenu(int newId)
    {
        Debug.Log(activeMenuId + " --> " + newId);
        switch (activeMenuId)
        {
            case 0: MainMenu.SetActive(false); break;
            case 1: CreateTaskMenu.SetActive(false); break;
            case 2: CreateEpicMenu.SetActive(false); break;
            case 3: StatisticMenu.SetActive(false); break;
        }

        switch (newId)
        {
            case 0: MainMenu.SetActive(true); break;
            case 1: CreateTaskMenu.SetActive(true); break;
            case 2: CreateEpicMenu.SetActive(true); break;
            case 3: 
                LoadStat();
                StatisticMenu.SetActive(true); 
                break;
        }
        activeMenuId = newId;
    }

    // Твой метод (сохранен)
    public void OpenMasterChat()
    {
        if (ChatScript.Instance != null)
        {
            if (menuScript == null) 
            {
                Debug.LogError("В MasterMenuScript не привязан MenuScript!");
                return;
            }

            int myId = menuScript.activeUserId;
            ChatScript.Instance.OpenChat(0, myId);
        }
        else
        {
            Debug.LogError("ChatScript не найден!");
        }
    }

    // Твой метод выхода (сохранен)
    public void ExitToMenu()
    {
        if (menuScript == null)
        {
            Debug.LogError("MenuScript reference is missing!");
            return;
        }
        menuScript.ChangeMenu(1);
    }
}