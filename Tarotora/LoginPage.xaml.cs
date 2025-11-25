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

    private async void Login(object sender, EventArgs e) 
    {
        string login = LoginEntry.Text; 
        string password = PasswordEntry.Text; 

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password)) 
        {
            await DisplayAlert("Ошибка", "Введите логин и пароль", "ОК"); 
            return;
        }

        var dbLocal = await DBfuncional.GetDB(); 
        var user = await dbLocal.Authenticate(login, password); 
        if (user != null) 
        {
            User.PostUser(user); 
            ((AppShell)Shell.Current).UpdateMenu();
            await Shell.Current.GoToAsync("Main"); 
        }
        else
        {
            await DisplayAlert("Ошибка", "Неверный логин или пароль", "ОК"); 
        }
    }

    private async void Registration(object sender, EventArgs e) 
    {
        await Shell.Current.GoToAsync("Registre"); 
    }
}