using Tarotora.BD;

namespace Tarotora;
public partial class DeleteUser : ContentPage
{
    private DBfuncional db;

    public DeleteUser()
    {
        InitializeComponent();
    }
    protected async override void OnAppearing() 
    {
        base.OnAppearing(); 

        db = await DBfuncional.GetDB(); 
        var users = await db.GetUsers(); 
        UsersView.ItemsSource = users; // связываем CollectionView
    }

    private async void OnEditClicked(object sender, EventArgs e) 
    {
        if (sender is Button btn && btn.CommandParameter is User user) // кнопка отправила пользователя
        {
            await Shell.Current.GoToAsync($"EditUser?userId={user.Id}"); 
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e) 
    {
        if (sender is Button btn && btn.CommandParameter is User user) 
        {
            if (user.IsAdmin)
            {
                await DisplayAlert("Ошибка", "Админ не может быть удален", "ОК");
                return; 
            }

            bool confirm = await DisplayAlert("Удаление", $"Удалить пользователя {user.Name}?", "Да", "Нет");
            if (!confirm) return; 

            await db.RemoveUser(user.Id);
            UsersView.ItemsSource = await db.GetUsers(); 
        }
    }
}