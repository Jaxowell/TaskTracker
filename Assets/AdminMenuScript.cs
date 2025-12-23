using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdminMenuScript : MonoBehaviour
{
    [Header("UI Элементы")]
    [SerializeField] TMP_InputField loginMenu;
    [SerializeField] TMP_InputField passwordMenu;
    [SerializeField] TMP_InputField emailMenu;
    [SerializeField] TMP_Dropdown roleMenu;
    [SerializeField] GameObject AddButton; // Кнопка "Добавить"
    
    [Header("Ссылки")]
    [SerializeField] MenuScript menuScript; // Ссылка на главный скрипт для доступа к DB

    // Метод вызывается кнопкой "Добавить пользователя"
    public void SignUp()
    {
        bool inputProblem = false;
        
        // Проверка на пустые поля
        if (string.IsNullOrEmpty(loginMenu.text)) inputProblem = true;
        if (string.IsNullOrEmpty(passwordMenu.text)) inputProblem = true;
        if (string.IsNullOrEmpty(emailMenu.text)) inputProblem = true;

        // В Dropdown: 0-Admin, 1-Master, 2-Worker.
        // В Базе: 1-Admin, 2-Master, 3-Worker.
        // Поэтому +1
        int roleId = roleMenu.value + 1;

        if (!inputProblem)
        {
            // Блокируем кнопку, чтобы не нажать дважды
            if(AddButton) AddButton.GetComponent<Button>().interactable = false;

            // Отправляем запрос
            StartCoroutine(menuScript.db.AddUserWeb(loginMenu.text, emailMenu.text, passwordMenu.text, roleId));
            
            // Очищаем поля
            loginMenu.text = "";
            emailMenu.text = "";
            passwordMenu.text = "";

            // Разблокируем кнопку через полсекунды (имитация)
            Invoke("EnableButton", 0.5f);
        }
        else
        {
            Debug.LogWarning("Не все поля заполнены!");
        }
    }

    void EnableButton()
    {
        if(AddButton) AddButton.GetComponent<Button>().interactable = true;
    }

    // Кнопка выхода (если есть в меню админа)
    public void Exit()
    {
        menuScript.ChangeMenu(1); // 1 - это меню входа
    }
}