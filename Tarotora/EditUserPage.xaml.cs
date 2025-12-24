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
    // поле класса — здесь хранится id пользователя, которого редактируем

    public int UserId
    // публичное свойство, через которое извне передают id пользователя
    {
        get => userId;
        // при чтении свойства просто возвращаем сохранённый id

        set
        {
            userId = value;
            // сохраняем переданный id в поле

            _ = LoadUser(); // метод для загрузки пользователей
            // запускаем загрузку данных пользователя
            // знак _ означает: мы не ждём результат, просто запускаем метод
        }
    }

    public bool IsAdmin => User.GetUser()?.IsAdmin ?? false; // является ли текущий пользователь админом ?? false — если пользователя нет, считаем что он НЕ админ

    private async Task LoadUser() 
    {
        currentUser = User.GetUser(); // получаем текущего авторизованного пользователя приложения
        db = await DBfuncional.GetDB(); //передаем бд в переменную

        editedUser = await db.GetUserById(UserId); // получаем из базы пользователя по переданному id

        if (editedUser == null) //если пользователь с таким id не найден 
        {
            await DisplayAlert("Ошибка", "Пользователь не найден", "OK"); 
            await Shell.Current.GoToAsync(".."); //возвращение назад
            return; //остальной код пропускаем
        }

        NameEntry.Text = editedUser.Name; //заполняем поле именем из базы и также с остальными пароль подписка
        PasswordEntry.Text = editedUser.Password; 
        SubscribeSwitch.IsToggled = editedUser.Subscribe; // устанавливаем переключатель "подписка"
        AdminSwitch.IsToggled = editedUser.IsAdmin; // устанавливаем переключатель "админ"
        AdminSwitch.IsEnabled = IsAdmin && (editedUser.Id != currentUser.Id); // включаем переключатель "админ" ТОЛЬКО если: 1) текущий пользователь — админ  2) он НЕ редактирует сам себя
        SubscribeSwitch.IsEnabled = IsAdmin || editedUser.Id == currentUser.Id; // переключатель подписки доступен если: либо текущий пользователь — админ, либо пользователь редактирует СЕБЯ
    }

    private async void OnSaveClicked(object sender, EventArgs e) //сохранение
    {
        if (editedUser == null) return; //если пользователь не загружен то пропускаем следующий код

        editedUser.Name = NameEntry.Text; //сохраняем новое имя из поля ввода и так с остальными
        editedUser.Password = PasswordEntry.Text; 
        editedUser.Subscribe = SubscribeSwitch.IsToggled; 
        //админ
        if (IsAdmin) 
            editedUser.IsAdmin = AdminSwitch.IsToggled; //менять роль админа может только сам админ

        await db.UpdateUser(editedUser); //обновляем данные пользователя в бд

        if (currentUser.Id == editedUser.Id) //текущий пользователь редактировал сам себя 
            User.PostUser(editedUser); //обновляем данные текущего пользователя в приложении

        await DisplayAlert("Успешно", "Профиль обновлён", "OK"); 
        await Shell.Current.GoToAsync(".."); //возврат на предыдущую страницу
    }

}