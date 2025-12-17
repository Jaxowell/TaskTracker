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

    public int activeUserId = 0; // По умолчанию 0
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
        
        // Проверки на пустоту (твоя улучшенная версия)
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
            // Используем твой безопасный метод входа (он не лочит базу)
            var loginResult = db.TryLoginFull(loginMenu.text, passwordMenu.text);
            
            if (loginResult.Success)
            {
                activeUserId = loginResult.Id;
                Debug.Log($"Успешный вход: {loginMenu.text} с ID {activeUserId}");
                ChangeMenu(loginResult.Role + 1); 
            }     
            else
            {
                passwordMenu.text = "";
                if(passwordMenu.placeholder.GetComponent<TextMeshProUGUI>())
                    passwordMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "Неверный логин\n или пароль!";
            }
        }
    }

    public void ChangeMenu(int newId)
    {
        Debug.Log($"ChangeMenu: {activeMenuId} --> {newId}");
        
        // Защита от дурака (твоя версия)
        if (SignInMenu == null || AdminMenu == null || MasterMenu == null || WorkerMenu == null)
        {
            Debug.LogError("Ссылки на меню не назначены в MenuScript!");
            return;
        }

        // 1. ОТКЛЮЧАЕМ ТЕКУЩЕЕ
        if (newId != 1) SignInMenu.SetActive(false);
        if (newId != 2) AdminMenu.SetActive(false);
        if (newId != 3) MasterMenu.SetActive(false);
        if (newId != 4) WorkerMenu.SetActive(false);

        // Дополнительно выключаем старое
        switch (activeMenuId)
        {
            case 1: SignInMenu.SetActive(false); break;
            case 2: AdminMenu.SetActive(false); break;
            case 3: MasterMenu.SetActive(false); break;
            case 4: WorkerMenu.SetActive(false); break;
        }

        // 2. ВКЛЮЧАЕМ НОВОЕ
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
                break;
            case 4:
                WorkerMenu.SetActive(true);
                if(workerMenu != null) workerMenu.LoadMenu();
                break;
        }
        
        activeMenuId = newId;
    }
}