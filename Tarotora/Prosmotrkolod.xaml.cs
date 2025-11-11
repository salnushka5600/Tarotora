using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using Tarotora.BD;

namespace Tarotora;

public partial class Prosmotrkolod : ContentPage
{
    private readonly DBfuncional db;
    
    public Prosmotrkolod(DBfuncional database)
    {
        InitializeComponent();
        db = database;
        BindingContext = this;
        LoadCards(); // покажет все карты
    }

   
    

    private async void LoadCards()
    {
        var cards = await db.GetCard();
        CardsView.ItemsSource = null; //очищение
        CardsView.ItemsSource = cards; //показ изображения

    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var card = button.CommandParameter as Card; 
        if (card == null) return;
        string newTitle = await DisplayPromptAsync("Редактирование", "Введите новое название:", initialValue: card.Title); //принимает значение title и выводит название которое было введено до редактирования
        if (newTitle == null) return;

        string newDesc = await DisplayPromptAsync("Редактирование", "Введите новое описание:", initialValue: card.Description);
        if (newDesc == null) return;

        bool changeImage = await DisplayAlert("Изображение", "Хотите изменить изображение?", "Да", "Нет");
        if (changeImage) 
        {
            string newImage = await DisplayPromptAsync("Редактирование", "Введите путь к изображению (например: fool.png):", initialValue: card.Image);
            if(!string.IsNullOrWhiteSpace(newImage)) 
                card.Image = newImage;
        }

        card.Title = newTitle;
        card.Description = newDesc;
        await db.UpdateCard(card);
        await DisplayAlert("Сохранено", "Изменения успешно применены!", "ОК");
        LoadCards();
    }
    private async void DeleteClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var card = button.CommandParameter as Card;
        if (card == null) return;
        bool ready = await DisplayAlert("Подтверждение", "Хотите удалить карту?", "Да", "Нет");
        if (!ready) return; 
        await db.RemoveCard(card.Id); 
        LoadCards();
    }

    private async void OnPassCardClicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        var card = btn.CommandParameter as Card;
        if (card == null) return;

        var current = await db.GetCurrentUser();
        if (current == null)
        {
            var users = await db.GetUser();
            if (users.Count == 0)
            {
                await DisplayAlert("Нет пользователя", "Сначала зарегистрируйте пользователя на экране Check.", "OK");
                return;
            }
            var nameToId = users.ToDictionary(u => u.Name, u => u.Id);
            var picked = await DisplayActionSheet("Кто проходит карту?", "Отмена", null, nameToId.Keys.ToArray());
            if (string.IsNullOrEmpty(picked) || !nameToId.TryGetValue(picked, out var pickedId)) return;
            await db.SetCurrentUser(pickedId);
            current = await db.GetCurrentUser();
        }

        await db.AddCompletedCard(current.Id, card.Id, score: 1, progress: 100);
        await DisplayAlert("Сохранено", $"Карта «{card.Title}» добавлена в историю пользователя «{current.Name}».", "OK");
    }
    private void SaveClicked(object sender, EventArgs e)
    {
        DBfuncional.SavetoFile(DBfuncional.GetDB);
    }
}
