using Microsoft.Extensions.Logging.Abstractions;
using System;
using Tarotora.BD;
namespace Tarotora;
public partial class LoginPage : ContentPage
{
    public LoginPage()
	{
		InitializeComponent();
	}

    private async void Login(object sender, EventArgs e) // метод при нажатии кнопки "Войти"
    {
        string login = LoginEntry.Text; // получаем логин
        string password = PasswordEntry.Text; // получаем пароль

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password)) // проверка заполнения
        {
            await DisplayAlert("Ошибка", "Введите логин и пароль", "ОК"); // предупреждаем
            return;
        }

        var dbLocal = await DBfuncional.GetDB(); // получаем объект базы
        var user = await dbLocal.Authenticate(login, password); // проверяем логин и пароль
        if (user != null) // если пользователь найден
        {
            User.PostUser(user); // сохраняем текущего пользователя
            ((AppShell)Shell.Current).UpdateMenu(); // обновляем меню под текущего пользователя
            await Shell.Current.GoToAsync("Main"); // переходим на MainPage
        }
        else // если не найден
        {
            await DisplayAlert("Ошибка", "Неверный логин или пароль", "ОК"); // предупреждаем
        }
    }

    private async void Registration(object sender, EventArgs e) // метод при нажатии "Регистрация"
    {
        await Shell.Current.GoToAsync("Registre"); // переход на RegistrationPage
    }
}