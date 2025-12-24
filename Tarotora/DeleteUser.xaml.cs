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
        UsersView.ItemsSource = users; // св€зываем CollectionView
    }

    private async void OnEditClicked(object sender, EventArgs e) //метод редактировани€ sender это сама кнопка котора€ лежит в object
    {
        if (sender is Button btn && btn.CommandParameter is User user) // если кнопка нажата и в ней лежит пользователь продолжаем sender is Button btn провер€ет это кнопка или нет если да то запихиваем в переменную btn у кнопки есть параметр btn.CommandParameter и она провер€ет пользователь это или нет если да то помещаем в переменную user
        {
            await Shell.Current.GoToAsync($"EditUser?userId={user.Id}"); //переход с текущей страницы на страницу редактировани€
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e) //удаление
    {
        if (sender is Button btn && btn.CommandParameter is User user) 
        {
            if (user.IsAdmin) //чтобы пользователь не могу удалить админа
            {
                await DisplayAlert("ќшибка", "јдмин не может быть удален", "ќ ");
                return; 
            }

            bool confirm = await DisplayAlert("”даление", $"”далить пользовател€ {user.Name}?", "ƒа", "Ќет");
            if (!confirm) return; //если нажали нет то пропускаем остальной код

            await db.RemoveUser(user.Id); // если нажали да то удал€ем пользовател€ по id из базы данных
            UsersView.ItemsSource = await db.GetUsers(); // загружаем список пользователей из базы данных обновл€ем список и пользователь исчезает с вьюпанели
        }
    }
}