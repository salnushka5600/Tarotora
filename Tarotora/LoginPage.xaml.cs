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

    private async void Login(object sender, EventArgs e) //кнопка войти
    {
        string login = LoginEntry.Text; //получаем логин из поля ввода Entry
        string password = PasswordEntry.Text; //тут тоже самое только с паролем

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password)) //чтобы не были пустыми и без пробелов
        {
            await DisplayAlert("Ошибка", "Введите логин и пароль", "ОК"); 
            return;
        }

        var dbLocal = await DBfuncional.GetDB(); //получаем подключение к бд асинхронно
        var user = await dbLocal.Authenticate(login, password); // пытаемся найти пользователя в бд с таким логином и паролем
        if (user != null) //если пользователь найден
        {
            User.PostUser(user); //сохраняем пользователя как текущего авторизованного
            ((AppShell)Shell.Current).UpdateMenu(); //обновляем меню и показываем меню для авторизованного пользвателя
            await Shell.Current.GoToAsync("Main"); //переходим на окно Main
        }
        else 
        {
            await DisplayAlert("Ошибка", "Неверный логин или пароль", "ОК"); 
        }
    }

    private async void Registration(object sender, EventArgs e) 
    {
        await Shell.Current.GoToAsync("Registre"); //переход на регистрацию
    }
}