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
        string name = NameEntry.Text; //получаем имя поьзователя из поля ввода
        string login = LoginEntry.Text; 
        string password = PasswordEntry.Text; 
        bool Subscription = SubscriptionSwitch.IsToggled; 

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password)) //чтобы не были пустыми и с пробелами
        {
            await DisplayAlert("Ошибка", "Введите имя, логин и пароль", "ОК");
            return;
        }

        var dbLocal = await DBfuncional.GetDB(); //получение обьекта бд
        var user = await dbLocal.Register(login, password, name, isAdmin: false, subscribe: Subscription); //создаем нового пользователя isAdmin: false - обычный пользователь подписка из переключателя 

        if (user != null) // если регистрация прошла успешно 
        {
            await DisplayAlert("Успех", $"Регистрация выполнена, добро пожаловать {user.Name}!", "ОК"); 
            await Shell.Current.GoToAsync("///Login"); //возвращаемся на логин /// переход к корню навигации
        }
        else
        {
            await DisplayAlert("Ошибка", "Пользователь с таким логином уже существует", "ОК");
        }
    }

    private async void Login(object sender, EventArgs e) //кнопка войти
    {
        await Shell.Current.GoToAsync("Login"); //переход на страницу входа
    }
}