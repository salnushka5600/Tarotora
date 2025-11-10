using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using Tarotora.BD;

namespace Tarotora;

public partial class Prosmotrkolod : ContentPage
{
    private readonly DBfuncional db;
    private readonly string? category; //nullable так как может быть null

    // Конструктор без категории — для вызова из MainPage
    public Prosmotrkolod(DBfuncional database)
    {
        InitializeComponent();
        db = database;
        BindingContext = this;
        LoadCards(); // покажет все карты
    }

    // Конструктор с категорией — для вызова из Addcards
    public Prosmotrkolod(DBfuncional database, string selectedCategory)
    {
        InitializeComponent();
        db = database;
        category = selectedCategory;
        BindingContext = this;
        Title = $"Карты категории: {category}";
        LoadCards(); // покажет только выбранную категорию
    }

    private async void LoadCards()
    {
        var cards = await db.GetCard();
        CardsView.ItemsSource = null; //очищение
        CardsView.ItemsSource = cards; //показ изображения

        var filtered = cards.Where(c => c.CategPicker == category).ToList(); //совпадает ли выбранная карта с выбранной категорией

        CardsView.ItemsSource = filtered;
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

 
private void SaveClicked(object sender, EventArgs e)
    {
        DBfuncional.SavetoFile(DBfuncional.GetDB);
    }
}
