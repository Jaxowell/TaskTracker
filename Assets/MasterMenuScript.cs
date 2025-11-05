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


    [SerializeField] TMP_InputField titleTask;
    [SerializeField] TMP_InputField descriptionTask;
    [SerializeField] TMP_InputField titleEpic;
    [SerializeField] TMP_InputField descriptionEpic;
    [SerializeField] TMP_Dropdown WorkerDropDown;
    //[SerializeField] MenuScript menuScript;

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
        if (WorkerDropDown.value == 0)
        {
            //descriptionMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "Введите описание!";
            inputProblem = true;
        }
        int workerId = db.GetUserIdByEmail(WorkerDropDown.options[WorkerDropDown.value].text);
        if (!inputProblem)
        {
            db.AddTask(titleTask.text, descriptionTask.text, workerId);
            titleTask.text = "";
            descriptionTask.text = "";
            WorkerDropDown.value = 0;
            //passwordMenu.text = "";

        }
    }
    public void AddEpic()
    {
        bool inputProblem = false;
        if (titleEpic.text == "")
        {
            titleTask.placeholder.GetComponent<TextMeshProUGUI>().text = "Укажите название!";
            inputProblem = true;
        }
        if (descriptionEpic.text == "")
        {
            descriptionTask.placeholder.GetComponent<TextMeshProUGUI>().text = "Введите описание!";
            inputProblem = true;
        }
        //int workerId = db.GetUserIdByEmail(WorkerDropDown.options[WorkerDropDown.value].text);
        if (!inputProblem)
        {
            //db.AddTask(titleTask.text, descriptionTask.text, workerId);
            titleEpic.text = "";
            descriptionEpic.text = "";
            //passwordMenu.text = "";

        }
    }
    void LoadWorkers()
    {
        List<string> workers=db.GetUserEmailsByRole(3);
        WorkerDropDown.AddOptions(workers);
        //Debug.Log("Загрузили!");
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
            case 2:
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
}
