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

    [SerializeField] TMP_InputField titleMenu;
    [SerializeField] TMP_InputField descriptionMenu;
    [SerializeField] TMP_Dropdown WorkerDropDown;
    
    // Обратите внимание: тут переменная называется menuScript
    [SerializeField] MenuScript menuScript; 

    // Start is called before the first frame update
    public void Start()
    {
        LoadWorkers();
        CreateTaskMenu.SetActive(false);
        MainMenu.SetActive(true);
    }

    public void AddTask()
    {
        bool inputProblem = false;
        if (titleMenu.text == "")
        {
            if(titleMenu.placeholder.GetComponent<TextMeshProUGUI>())
                titleMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "Введите название!";
            inputProblem = true;
        }
        if (descriptionMenu.text == "")
        {
            if(descriptionMenu.placeholder.GetComponent<TextMeshProUGUI>())
                descriptionMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "Введите описание!";
            inputProblem = true;
        }
        if(WorkerDropDown.value == 0) // Обычно 0 элемент это "Выберите...", но зависит от реализации
        {
             // Тут можно добавить проверку, если первый элемент пустой
        }

        // Защита от пустого списка
        if (WorkerDropDown.options.Count > 0)
        {
            int workerId = db.GetUserIdByEmail(WorkerDropDown.options[WorkerDropDown.value].text); 
            if (!inputProblem)
            {
                db.AddTask(titleMenu.text, descriptionMenu.text, workerId);
                titleMenu.text = "";
                descriptionMenu.text = "";
                WorkerDropDown.value = 0;
            }
        }
    }

    void LoadWorkers()
    {
        List<string> workers = db.GetUserEmailsByRole(3); // 3 - это роль Worker
        WorkerDropDown.ClearOptions(); // Хорошая практика очищать перед добавлением
        WorkerDropDown.AddOptions(workers);
    }

    public void ChangeMenu(int newId)
    {
        Debug.Log(activeMenuId + " --> " + newId);
        switch (activeMenuId)
        {
            case 0:
                MainMenu.SetActive(false);
                break;
            case 1:
                CreateTaskMenu.SetActive(false);
                break;
        }

        switch (newId)
        {
            case 0:
                MainMenu.SetActive(true);
                break;
            case 1:
                CreateTaskMenu.SetActive(true);
                break;
        }
        activeMenuId = newId;
    }

    public void OpenMasterChat()
    {
        if (ChatScript.Instance != null)
        {
            if (menuScript == null) 
            {
                Debug.LogError("В MasterMenuScript не привязан MenuScript! Перетяни его в инспекторе.");
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

    public void ExitToMenu()
    {
        Debug.Log("Master ExitToMenu called");
        
        // В MasterMenu у нас нет списка кнопок taskLists, поэтому удалять нечего.
        // Просто выходим.

        if (menuScript == null)
        {
            Debug.LogError("MenuScript reference is missing in MasterMenuScript!");
            return;
        }
        
        // Возвращаемся в меню входа (id=1)
        menuScript.ChangeMenu(1);
    }
}