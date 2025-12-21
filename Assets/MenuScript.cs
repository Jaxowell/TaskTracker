using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    int activeMenuId = 1;
    [SerializeField] GameObject SignInMenu;
    [SerializeField] GameObject AdminMenu;
    [SerializeField] GameObject MasterMenu;
    [SerializeField] GameObject WorkerMenu;

    [SerializeField] WorkerMenu workerMenu;
    [SerializeField] MasterMenuScript masterMenu;

    public int activeUserId = 0;
    public DBScript db = new DBScript();
    [SerializeField] TMP_InputField loginMenu;
    [SerializeField] TMP_InputField passwordMenu;

    private void Start()
    {
        SignInMenu.SetActive(true);
        AdminMenu.SetActive(false);
        MasterMenu.SetActive(false);
        WorkerMenu.SetActive(false);
    }

    public void SignIn()
    {
        bool inputProblem = false;
        
        if (string.IsNullOrEmpty(loginMenu.text))
        {
            if(loginMenu.placeholder.GetComponent<TextMeshProUGUI>())
                loginMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "Введите логин!";
            inputProblem = true;
        }
        if (string.IsNullOrEmpty(passwordMenu.text))
        {
            if(passwordMenu.placeholder.GetComponent<TextMeshProUGUI>())
                passwordMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "Введите пароль!";
            inputProblem = true;
        }

        if(!inputProblem)
        {
            // НОВЫЙ ВЫЗОВ
            StartCoroutine(db.TryLoginWeb(loginMenu.text, passwordMenu.text, (result) => 
            {
                if (result.Success)
                {
                    activeUserId = result.Id;
                    Debug.Log($"Успешный вход: {loginMenu.text} с ID {activeUserId}");
                    // Роль берем сразу из ответа
                    ChangeMenu(result.Role + 1);
                }
                else
                {
                    passwordMenu.text = "";
                    if(passwordMenu.placeholder.GetComponent<TextMeshProUGUI>())
                        passwordMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "Неверный логин\n или пароль!";
                }
            }));
        }
    }

    public void ChangeMenu(int newId)
    {
        Debug.Log($"ChangeMenu: {activeMenuId} --> {newId}");
        
        if (SignInMenu == null || AdminMenu == null || MasterMenu == null || WorkerMenu == null)
        {
            Debug.LogError("Ссылки на меню не назначены в MenuScript!");
            return;
        }

        if (newId != 1) SignInMenu.SetActive(false);
        if (newId != 2) AdminMenu.SetActive(false);
        if (newId != 3) MasterMenu.SetActive(false);
        if (newId != 4) WorkerMenu.SetActive(false);

        switch (activeMenuId)
        {
            case 1: SignInMenu.SetActive(false); break;
            case 2: AdminMenu.SetActive(false); break;
            case 3: MasterMenu.SetActive(false); break;
            case 4: WorkerMenu.SetActive(false); break;
        }

        switch (newId)
        {
            case 1:
                SignInMenu.SetActive(true);
                if(loginMenu != null) loginMenu.text = "";
                if(passwordMenu != null) passwordMenu.text = "";
                break;
            case 2:
                AdminMenu.SetActive(true);
                break;
            case 3:
                MasterMenu.SetActive(true);
                if (masterMenu != null) masterMenu.LoadMenu();
                break;
            case 4:
                WorkerMenu.SetActive(true);
                if (workerMenu != null) workerMenu.LoadMenu();
                break;
        }
        
        activeMenuId = newId;
    }
}