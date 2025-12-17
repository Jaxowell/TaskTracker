using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    /// <summary>
    /// 1-Sign in
    /// 2-admin
    /// 3-master
    /// 4-worker
    /// </summary>
    int activeMenuId = 1;
    [SerializeField] GameObject SignInMenu;
    [SerializeField] GameObject AdminMenu;
    [SerializeField] GameObject MasterMenu;
    [SerializeField] GameObject WorkerMenu;


    [SerializeField] WorkerMenu workerMenu;
    [SerializeField] MasterMenuScript masterMenu;

    public int activeUserId = 5000;
    public DBScript db = new DBScript();
    [SerializeField] TMP_InputField loginMenu;
    [SerializeField] TMP_InputField passwordMenu;


    private void Start()
    {
        //db.AddUser("Admin","admin@mail.ru","qwerty",1);
        SignInMenu.SetActive(true);
        AdminMenu.SetActive(false);
        MasterMenu.SetActive(false);
        WorkerMenu.SetActive(false);
    }


    public void SignIn()
    {
        bool inputProblem = false;
        if (loginMenu.text == "")
        {
            loginMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "Укажите логин!";
            inputProblem = true;
        }
        if (passwordMenu.text == "")
        {
            passwordMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "Введите пароль!";
            inputProblem = true;
        }
        if(!inputProblem)
        {
            bool res = db.VerifyLogin(loginMenu.text, passwordMenu.text);
            if (res)
            {
                activeUserId = db.GetUserIdByName(loginMenu.text);
                Debug.Log("Сейчас активный:" + loginMenu.text+" с id "+activeUserId);
                ChangeMenu(db.GetUserRole(loginMenu.text)+1);
            }
            else
            {
                passwordMenu.text = "";
                passwordMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "Неверные пароль\n или логин!";
            }
            //Debug.Log(res);
        }
        //return res;
    }
    public void ChangeMenu(int newId)
    {
        Debug.Log("В главном меню "+activeMenuId+" --> "+newId);
        switch (activeMenuId)
        {
            case 1:
                SignInMenu.SetActive(false);
                break;
            case 2:
                AdminMenu.SetActive(false);
                break;
            case 3:
                MasterMenu.SetActive(false);
                break;
            case 4:
                WorkerMenu.SetActive(false);
                break;
        }


        switch (newId)
        {
            case 1:
                SignInMenu.SetActive(true);
                break;
            case 2:
                AdminMenu.SetActive(true);
                break;
            case 3:
                MasterMenu.SetActive(true);
                masterMenu.LoadMenu();
                break;
            case 4:
                WorkerMenu.SetActive(true);
                workerMenu.LoadMenu();
                break;
        }
        activeMenuId = newId;
    }
}
