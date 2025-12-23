using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    int activeMenuId = 1;
    [Header("Панели меню")]
    [SerializeField] GameObject SignInMenu;
    [SerializeField] GameObject AdminMenu;
    [SerializeField] GameObject MasterMenu;
    [SerializeField] GameObject WorkerMenu;

    [Header("Скрипты-контроллеры")]
    [SerializeField] WorkerMenu workerMenu;
    [SerializeField] MasterMenuScript masterMenu;

    public int activeUserId = 0;
    
    // Сюда в инспекторе повесь URL NGROK (например https://xxxx.ngrok.app/api/)
    public DBScript db; 

    [Header("Поля ввода")]
    [SerializeField] TMP_InputField loginMenu;
    [SerializeField] TMP_InputField passwordMenu;

    private void Start()
    {
        // Если db не назначен в инспекторе, создаем новый
        if(db == null) db = new DBScript(); 

        SignInMenu.SetActive(true);
        if(AdminMenu) AdminMenu.SetActive(false);
        if(MasterMenu) MasterMenu.SetActive(false);
        if(WorkerMenu) WorkerMenu.SetActive(false);
    }

    public void SignIn()
    {
        if (string.IsNullOrEmpty(loginMenu.text) || string.IsNullOrEmpty(passwordMenu.text)) return;

        // Используем Login через WEB
        StartCoroutine(db.TryLoginWeb(loginMenu.text, passwordMenu.text, (result) => 
        {
            if (result.Success)
            {
                activeUserId = result.Id;
                Debug.Log($"Вход выполнен! ID: {activeUserId}, Роль: {result.Role}");
                // В базе роли 1-Admin, 2-Master, 3-Worker
                // Переключаем меню по ID роли + 1 (если у тебя такая логика) или напрямую
                // Судя по твоему коду: 1-Login, 2-Admin, 3-Master, 4-Worker.
                // Значит roleId + 1 подходит.
                ChangeMenu(result.Role + 1);
            }
            else
            {
                Debug.LogWarning("Ошибка входа: неверный пароль или логин");
                passwordMenu.text = "";
                if(passwordMenu.placeholder.GetComponent<TextMeshProUGUI>())
                    passwordMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "Error!";
            }
        }));
    }

    public void ChangeMenu(int newId)
    {
        if (SignInMenu) SignInMenu.SetActive(false);
        if (AdminMenu) AdminMenu.SetActive(false);
        if (MasterMenu) MasterMenu.SetActive(false);
        if (WorkerMenu) WorkerMenu.SetActive(false);

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
                if(masterMenu) masterMenu.LoadMenu(); 
                break;
            case 4: 
                WorkerMenu.SetActive(true); 
                if(workerMenu) workerMenu.LoadMenu(); 
                break;
        }
        activeMenuId = newId;
    }
}