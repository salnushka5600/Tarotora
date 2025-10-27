using System.Diagnostics.Metrics;
using Tarotora.BD;

namespace Tarotora;

public partial class Prosmotrkolod : ContentPage
{
    DBfuncional db;
    private Card currentCard;
    public Prosmotrkolod(DBfuncional database)
	{
		InitializeComponent();
        BindingContext = this;
        db = database;
        LoadCards();
    }
    private async void LoadCards()
    {
        var cards = await db.GetCard();
        Cards = cards;
        OnPropertyChanged(nameof(Cards));
    }
   
    public List<Card> Cards { get; set; }
    private void Switch_Toggled(object sender, ToggledEventArgs e)
    {
        Switch switchItem = (Switch)sender;
        if (!switchItem.IsToggled)
        {
            switchItem.Background = new SolidColorBrush(Color.FromArgb("#FFF4D2D7"));
        }

    }

    private async void CardEdit(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var card = button.CommandParameter as Card;

        //// Открываем страницу редактирования, передаём карту и базу
        //var editPage = new Addcards(db, card);

        //// Подписываемся на событие, чтобы обновить страницу после сохранения
        //editPage.Disappearing += (s, args) =>
        //{
        //    LoadCards(); // ? обновляем данные при возвращении
        //};
        await Navigation.PushAsync(new Addcards(db));
        // Переключаем режим редактирования
        bool editing = !TitleEntry.IsEnabled;

        TitleEntry.IsEnabled = editing;
        DescriptionEntry.IsEnabled = editing;

        if (editing)
        {
            await DisplayAlert("Редактирование", "Теперь вы можете изменить данные и нажать Enter для сохранения.", "ОК");
        }
    }

    private async void OnEntryCompleted(object sender, EventArgs e)
    {
        if (currentCard == null) return;

        currentCard.Title = TitleEntry.Text;
        currentCard.Description = DescriptionEntry.Text;

        await db.UpdateCard(currentCard);

        TitleEntry.IsEnabled = false;
        DescriptionEntry.IsEnabled = false;

        await DisplayAlert("Сохранено", "Изменения успешно обновлены!", "ОК");
    }
}
}