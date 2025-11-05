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
        if (titleMenu.text == "")
        {
            titleMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "������� ��������!";
            inputProblem = true;
        }
        if (descriptionMenu.text == "")
        {
            descriptionMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "������� ��������!";
            inputProblem = true;
        }
        if(WorkerDropDown.value==0)
        {
            //descriptionMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "������� ��������!";
            inputProblem = true;
        }
        int workerId = db.GetUserIdByEmail(WorkerDropDown.options[WorkerDropDown.value].text); 
        if (!inputProblem)
        {
            db.AddTask(titleMenu.text,descriptionMenu.text,workerId);
            titleMenu.text = "";
            descriptionMenu.text = "";
            WorkerDropDown.value = 0;
            //passwordMenu.text = "";

        }
    }
    void LoadWorkers()
    {
        List<string> workers=db.GetUserEmailsByRole(3);
        WorkerDropDown.AddOptions(workers);
        //Debug.Log("���������!");
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

    public void OpenChat()
    {
        if (ChatScript.Instance != null)
            ChatScript.Instance.OpenChat();
    }
}
