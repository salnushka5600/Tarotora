using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using Tarotora.BD;

namespace Tarotora;

public partial class Prosmotrkolod : ContentPage
{
    private DBfuncional db; // объект базы
    private User currentUser; // текущий пользователь

    public bool IsAdmin => currentUser?.IsAdmin ?? false; // проверка на админа
    public Prosmotrkolod()
    {
        InitializeComponent();
        BindingContext = this; // чтобы IsAdmin работал в XAML
    }

    private async Task LoadCards() // метод загрузки карт
    {
        var allCards = await db.GetCards(); // получаем все карты
        var tests = (await db.GetTests()).Where(t => t.IdUser == currentUser.Id).ToList(); // прогресс текущего пользователя

        foreach (var c in allCards)
        {
            var test = tests.FirstOrDefault(t => t.IdCard == c.Id); // находим прогресс для карты
            c.Progress = test?.Progress ?? 0; // записываем прогресс, если есть
        }

        CardsView.ItemsSource = allCards; // выводим карты в CollectionView
    }

    private async void OnEditClicked(object sender, EventArgs e) // кнопка редактирования карты
    {
        if (!IsAdmin) return; // только админ
        if (sender is Button btn && btn.CommandParameter is Card card)
        {
            await Shell.Current.GoToAsync($"EditCard?cardId={card.Id}"); // переходим на EditCardPage
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e) // кнопка удаления карты
    {
        if (!IsAdmin) return; // только админ
        if (sender is Button btn && btn.CommandParameter is Card card)
        {
            bool confirm = await DisplayAlert("Удаление", $"Удалить карту {card.Title}?", "Да", "Нет"); // подтверждение
            if (!confirm) return;

            await db.RemoveCard(card.Id); // удаляем карту из базы
            await LoadCards(); // обновляем CollectionView
        }
    }

    protected async override void OnAppearing() // при отображении страницы
    {
        base.OnAppearing();
        currentUser = User.GetUser(); // текущий пользователь
        if (currentUser == null) return;

        db = await DBfuncional.GetDB(); // объект базы
        await LoadCards(); // загружаем карты
    }

}
