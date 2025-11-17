using Microsoft.Extensions.Logging.Abstractions;
using System;
using Tarotora.BD;

namespace Tarotora;

public partial class RegistrationPage : ContentPage
{
	public RegistrationPage()
	{
		InitializeComponent();
	}

    private async void Registration(object sender, EventArgs e) // кнопка регистрации
    {
        string name = NameEntry.Text; // имя
        string login = LoginEntry.Text; // логин
        string password = PasswordEntry.Text; // пароль
        bool Subscription = SubscriptionSwitch.IsToggled; // подписка

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password)) // проверка
        {
            await DisplayAlert("Ошибка", "Введите имя, логин и пароль", "ОК");
            return;
        }

        var dbLocal = await DBfuncional.GetDB(); // объект базы
        var user = await dbLocal.Register(login, password, name, isAdmin: false, subscribe: Subscription); // создаём пользователя

        if (user != null) // если регистрация успешна
        {
            await DisplayAlert("Успех", $"Регистрация выполнена, добро пожаловать {user.Name}!", "ОК");
            await Shell.Current.GoToAsync("///Login"); // переходим на страницу входа
        }
        else
        {
            await DisplayAlert("Ошибка", "Пользователь с таким логином уже существует", "ОК");
        }
    }

    private async void Login(object sender, EventArgs e) // кнопка "Войти" на странице регистрации
    {
        await Shell.Current.GoToAsync("Login"); // переходим на LoginPage
    }
}