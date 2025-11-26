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

    private async void Registration(object sender, EventArgs e) 
    {
        string name = NameEntry.Text; 
        string login = LoginEntry.Text; 
        string password = PasswordEntry.Text; 
        bool Subscription = SubscriptionSwitch.IsToggled; 

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password)) 
        {
            await DisplayAlert("Ошибка", "Введите имя, логин и пароль", "ОК");
            return;
        }

        var dbLocal = await DBfuncional.GetDB(); 
        var user = await dbLocal.Register(login, password, name, isAdmin: false, subscribe: Subscription); 

        if (user != null) 
        {
            await DisplayAlert("Успех", $"Регистрация выполнена, добро пожаловать {user.Name}!", "ОК");
            await Shell.Current.GoToAsync("///Login");
        }
        else
        {
            await DisplayAlert("Ошибка", "Пользователь с таким логином уже существует", "ОК");
        }
    }

    private async void Login(object sender, EventArgs e) 
    {
        await Shell.Current.GoToAsync("Login"); 
    }
}