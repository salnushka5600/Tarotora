using Tarotora.BD;

namespace Tarotora;
public partial class DeleteUser : ContentPage
{
    private DBfuncional db;

    public DeleteUser()
    {
        InitializeComponent();
    }
    protected async override void OnAppearing() // метод вызывается когда страница отображается
    {
        base.OnAppearing(); 

        db = await DBfuncional.GetDB(); // получаем объект базы данных (асинхронно)
        var users = await db.GetUsers(); // получаем список всех пользователей
        UsersView.ItemsSource = users; // связываем CollectionView с пользователями, чтобы их показать
    }

    private async void OnEditClicked(object sender, EventArgs e) // метод при нажатии кнопки "Редактировать"
    {
        if (sender is Button btn && btn.CommandParameter is User user) // проверяем, что кнопка отправила пользователя
        {
            await Shell.Current.GoToAsync($"EditUser?userId={user.Id}"); // переходим на страницу редактирования пользователя, передаём Id
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e) // метод при нажатии кнопки "Удалить"
    {
        if (sender is Button btn && btn.CommandParameter is User user) // проверяем, что кнопка отправила пользователя
        {
            if (user.IsAdmin) // если пытаются удалить админа
            {
                await DisplayAlert("Ошибка", "Админ не может быть удален", "ОК"); // показываем предупреждение
                return; // выходим из метода, удалять не будем
            }

            bool confirm = await DisplayAlert("Удаление", $"Удалить пользователя {user.Name}?", "Да", "Нет"); // спрашиваем подтверждение
            if (!confirm) return; // если пользователь отказался, выходим

            await db.RemoveUser(user.Id); // удаляем пользователя из базы
            UsersView.ItemsSource = await db.GetUsers(); // обновляем список пользователей в CollectionView
        }
    }
}