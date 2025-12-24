using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using Tarotora.BD;

namespace Tarotora;

public partial class Prosmotrkolod : ContentPage
{
    private DBfuncional db;
    private User currentUser; // поле для хранения текущего авторизованного пользователя

    public bool IsAdmin => currentUser?.IsAdmin ?? false; //проверка на админа вернет true если админ если пользователя нет считаем что админа нет то false
    public Prosmotrkolod()
    {
        InitializeComponent();
        BindingContext = this; // устанавливаем контекст привязки данных чтобы в XAML можно было использовать свойства этой страницы (например IsAdmin)
    }

    private async Task LoadCards() //метод загружает карты и прогресс пользователя
    {
        var allCards = await db.GetCards(); //получаем все карты переменную
        var tests = (await db.GetTests()) // получаем все тесты
            .Where(t => t.IdUser == currentUser.Id) //оставляем тесты только текущего пользователя
            .ToList(); 

        foreach (var c in allCards) // проходим по каждой карте
        {
            var test = tests.FirstOrDefault(t => t.IdCard == c.Id);  // ищем тест, который относится к этой карте
            c.Progress = test?.Progress ?? 0; // если тест найден — берём прогресс  если нет — прогресс считается 0
        }

        CardsView.ItemsSource = allCards; // передаём список карт в элемент интерфейса для отображения
    }

    private async void OnEditClicked(object sender, EventArgs e) //кнопка редактировать
    {
        if (!IsAdmin) return; //если пользователь не админ то редактирование запрещено пропуск след кода
        if (sender is Button btn && btn.CommandParameter is Card card) //проверяем кнопка или нет если да перемещаем в переменную btn у кнопки есть CommandParameter и он проверяет карта это или нет если да то перемещает в переменную card
        {
            await Shell.Current.GoToAsync($"EditCard?cardId={card.Id}"); //переходим на редактирование и передаем id карты
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e) //удаление карты
    {
        if (!IsAdmin) return; //если пользователь не админ то редактирование запрещено пропуск след кода
        if (sender is Button btn && btn.CommandParameter is Card card) //проверяем кнопка или нет если да перемещаем в переменную btn у кнопки есть CommandParameter и он проверяет карта это или нет если да то перемещает в переменную card
        {
            bool confirm = await DisplayAlert("Удаление", $"Удалить карту {card.Title}?", "Да", "Нет"); 
            if (!confirm) return; //если нажали нет то пропуск след кода

            await db.RemoveCard(card.Id); //если да то удаляем карту из бд по id 
            await LoadCards(); //заново загружаем карты для обновления интерфейса
        }
    }

    protected async override void OnAppearing() // при отображении страницы
    {
        base.OnAppearing(); // базовая логика страницы
        currentUser = User.GetUser(); //получаем текущего авторизованного пользователя
        if (currentUser == null) return; //если пользователь не авторизован то пропуск след кода

        db = await DBfuncional.GetDB(); //получаем обьект бд 
        await LoadCards(); //загружаем карты и прогресс пользователя
    }

}
