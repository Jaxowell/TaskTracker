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

    public int activeUserId = 0;
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
            loginMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "������� �����!";
            inputProblem = true;
        }
        if (passwordMenu.text == "")
        {
            passwordMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "������� ������!";
            inputProblem = true;
        }
        if(!inputProblem)
        {
            bool res = db.VerifyLogin(loginMenu.text, passwordMenu.text);
            if (res)
            {
                activeUserId = db.GetUserIdByName(loginMenu.text);
                ChangeMenu(db.GetUserRole(loginMenu.text)+1);
            }
            else
            {
                passwordMenu.text = "";
                passwordMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "�������� ������\n ��� �����!";
            }
            //Debug.Log(res);
        }
        //return res;
    }
    public void ChangeMenu(int newId)
    {
        Debug.Log($"ChangeMenu: {activeMenuId} --> {newId}");
        
        // Проверяем ссылки на меню
        if (SignInMenu == null || AdminMenu == null || MasterMenu == null || WorkerMenu == null)
        {
            Debug.LogError("One or more menu references are missing in MenuScript!");
            return;
        }

        Debug.Log($"Deactivating menu {activeMenuId}");
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
                break;
            case 4:
                WorkerMenu.SetActive(true);
                workerMenu.LoadMenu();
                break;
        }
        activeMenuId = newId;
    }
}
