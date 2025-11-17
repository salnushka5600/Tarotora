using Microsoft.Extensions.Logging.Abstractions;
using Tarotora.BD;
namespace Tarotora;

[QueryProperty(nameof(UserId), "userId")] // позвол€ет получать параметр userId через Shell навигацию
public partial class EditUserPage : ContentPage
{
    private DBfuncional db; // объект дл€ работы с базой
    private User currentUser; // текущий вошедший пользователь
    private User editedUser; // пользователь, которого редактируем

    public EditUserPage()
    {
        InitializeComponent();
        // устанавливаем контекст данных дл€ XAML (чтобы IsAdmin работал)
        BindingContext = this;
    }

    private int userId; // переменна€ дл€ хранени€ Id редактируемого пользовател€
    public int UserId // свойство дл€ прив€зки параметра навигации
    {
        get => userId;
        set
        {
            userId = value; // сохран€ем Id
            _ = LoadUser(); // загружаем пользовател€ асинхронно
        }
    }

    public bool IsAdmin => User.GetUser()?.IsAdmin ?? false; // проверка, €вл€етс€ ли текущий пользователь админом

    private async Task LoadUser() // метод загрузки пользовател€ из базы
    {
        currentUser = User.GetUser(); // получаем текущего пользовател€
        db = await DBfuncional.GetDB(); // инициализируем объект базы данных

        editedUser = await db.GetUserById(UserId); // загружаем пользовател€, которого редактируем

        if (editedUser == null) // если пользователь не найден
        {
            await DisplayAlert("ќшибка", "ѕользователь не найден", "OK"); // показываем предупреждение
            await Shell.Current.GoToAsync(".."); // возвращаемс€ на предыдущую страницу
            return;
        }

        NameEntry.Text = editedUser.Name; // показываем им€
        PasswordEntry.Text = editedUser.Password; // показываем пароль
        SubscribeSwitch.IsToggled = editedUser.Subscribe; // показываем подписку

        AdminSwitch.IsToggled = editedUser.IsAdmin; // показываем роль админа
        AdminSwitch.IsEnabled = IsAdmin && (editedUser.Id != currentUser.Id); // редактировать роль может только админ и не самого себ€

        SubscribeSwitch.IsEnabled = IsAdmin || editedUser.Id == currentUser.Id; // подписку можно мен€ть админу или самому себе
    }

    private async void OnSaveClicked(object sender, EventArgs e) // метод при нажатии кнопки "—охранить"
    {
        if (editedUser == null) return; // если нет загруженного пользовател€, выходим

        editedUser.Name = NameEntry.Text; // сохран€ем новое им€
        editedUser.Password = PasswordEntry.Text; // сохран€ем новый пароль
        editedUser.Subscribe = SubscribeSwitch.IsToggled; // сохран€ем подписку

        if (IsAdmin) // админ может мен€ть роль других
            editedUser.IsAdmin = AdminSwitch.IsToggled;

        await db.UpdateUser(editedUser); // сохран€ем изменени€ в базе

        if (currentUser.Id == editedUser.Id) // если редактируем самого себ€
            User.PostUser(editedUser); // обновл€ем текущего пользовател€

        await DisplayAlert("”спех", "ѕрофиль обновлЄн", "OK"); // показываем уведомление
        await Shell.Current.GoToAsync(".."); // возвращаемс€ на предыдущую страницу
    }

}