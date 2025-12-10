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
            var loginResult = db.TryLoginFull(loginMenu.text, passwordMenu.text);
            if (loginResult.Success)
            {
                activeUserId = loginResult.Id;
                ChangeMenu(loginResult.Role + 1); // Твоя логика с +1
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
        
        // Если ссылки слетели - ищем их заново (защита от дурака)
        if (SignInMenu == null || AdminMenu == null || MasterMenu == null || WorkerMenu == null)
        {
            Debug.LogError("Ссылки на меню не назначены в MenuScript!");
            return;
        }

        // 1. ОТКЛЮЧАЕМ ТЕКУЩЕЕ (ИЛИ ВСЕ СРАЗУ ДЛЯ НАДЕЖНОСТИ)
        // Вместо switch по id, который может глючить, просто выключим всё лишнее
        if (newId != 1) SignInMenu.SetActive(false);
        if (newId != 2) AdminMenu.SetActive(false);
        if (newId != 3) MasterMenu.SetActive(false);
        if (newId != 4) WorkerMenu.SetActive(false);

        // Дополнительно выключаем то, что считалось активным (если вдруг логика выше пропустила)
        switch (activeMenuId)
        {
            case 1: SignInMenu.SetActive(false); break;
            case 2: AdminMenu.SetActive(false); break;
            case 3: MasterMenu.SetActive(false); break;
            case 4: WorkerMenu.SetActive(false); break;
            // Case 0 ничего не делает, и это нормально теперь
        }

        // 2. ВКЛЮЧАЕМ НОВОЕ
        switch (newId)
        {
            case 1:
                SignInMenu.SetActive(true);
                // Очищаем поля ввода при возврате в логин
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
        
        // Обновляем ID
        activeMenuId = newId;
    }
}
