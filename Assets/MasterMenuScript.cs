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

    [SerializeField] TextMeshProUGUI stat; // Статистика

    // Поля для ЗАДАЧ (Task)
    [SerializeField] TMP_InputField titleTask;
    [SerializeField] TMP_InputField descriptionTask;
    
    // Поля для ЭПИКОВ (Epic)
    [SerializeField] TMP_InputField titleEpic;
    [SerializeField] TMP_InputField descriptionEpic;

    [SerializeField] TMP_Dropdown WorkerDropDown;
    
    // Единая ссылка на главное меню (твое название)
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
            int workerId = db.GetUserIdByEmail(WorkerDropDown.options[WorkerDropDown.value].text);
            
            if (!inputProblem)
            {
                // Берем ID мастера из menuScript (твоя ссылка)
                int masterId = menuScript.activeUserId;
                
                // Вызываем метод напарника (с masterId)
                db.AddTask(titleTask.text, descriptionTask.text, masterId, workerId);
                
                titleTask.text = "";
                descriptionTask.text = "";
                WorkerDropDown.value = 0;
            }
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
            // Метод AddEpic (из кода напарника)
            db.AddEpic(titleEpic.text, descriptionEpic.text, masterId);
            
            titleEpic.text = "";
            descriptionEpic.text = "";
        }
    }

    void LoadStat()
    {
        int masterId = menuScript.activeUserId;
        string answer = "";
        
        // Используем методы напарника
        List<string> tasks = db.AIGetTaskByMaster(masterId);
        for (int i = 0; i < tasks.Count; i+=2)
        {
            answer += "Задача " + tasks[i] + " выполнил " + tasks[i+1] +";\n";
        }
        
        List<string> epic = db.GetEpicByMaster(masterId);
        for (int i = 0; i < epic.Count; i++)
        {
            answer += "Эпик " + epic[i] + ";\n";
        }
        
        if(stat != null) stat.text = answer;
    }

    void LoadWorkers()
    {
        List<string> workers = db.GetUserEmailsByRole(3); 
        WorkerDropDown.ClearOptions();
        WorkerDropDown.AddOptions(workers);
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