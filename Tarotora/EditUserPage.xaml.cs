using Microsoft.Extensions.Logging.Abstractions;
using Tarotora.BD;
namespace Tarotora;

[QueryProperty(nameof(UserId), "userId")] // позволяет получать параметр userId через Shell навигацию
public partial class EditUserPage : ContentPage
{
    private DBfuncional db; 
    private User currentUser; 
    private User editedUser; 

    public EditUserPage()
    {
        InitializeComponent();
        //контекст данных для XAML (чтобы IsAdmin работал)
        BindingContext = this;
    }

    private int userId; 
    public int UserId 
    {
        get => userId;
        set
        {
            userId = value; 
            _ = LoadUser(); 
        }
    }

    public bool IsAdmin => User.GetUser()?.IsAdmin ?? false; // является ли текущий пользователь админом

    private async Task LoadUser() 
    {
        currentUser = User.GetUser(); 
        db = await DBfuncional.GetDB();

        editedUser = await db.GetUserById(UserId); 

        if (editedUser == null) 
        {
            await DisplayAlert("Ошибка", "Пользователь не найден", "OK"); 
            await Shell.Current.GoToAsync(".."); 
            return;
        }

        NameEntry.Text = editedUser.Name; 
        PasswordEntry.Text = editedUser.Password; 
        SubscribeSwitch.IsToggled = editedUser.Subscribe; 
        AdminSwitch.IsToggled = editedUser.IsAdmin; 
        AdminSwitch.IsEnabled = IsAdmin && (editedUser.Id != currentUser.Id); 
        SubscribeSwitch.IsEnabled = IsAdmin || editedUser.Id == currentUser.Id; 
    }

    private async void OnSaveClicked(object sender, EventArgs e) 
    {
        if (editedUser == null) return; 

        editedUser.Name = NameEntry.Text; 
        editedUser.Password = PasswordEntry.Text; 
        editedUser.Subscribe = SubscribeSwitch.IsToggled; 

        if (IsAdmin) 
            editedUser.IsAdmin = AdminSwitch.IsToggled;

        await db.UpdateUser(editedUser); 

        if (currentUser.Id == editedUser.Id) 
            User.PostUser(editedUser); 

        await DisplayAlert("Успешно", "Профиль обновлён", "OK"); 
        await Shell.Current.GoToAsync(".."); 
    }

}