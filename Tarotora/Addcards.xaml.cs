using System.Data;
using System.Diagnostics;
using Tarotora.BD;
namespace Tarotora;

public partial class Addcards : ContentPage
{
    private readonly DBfuncional db;
    private Card currentCard;

    public Addcards(DBfuncional database, Card card = null)
    {
        InitializeComponent();
        db = database;

        if (card != null)
        {
            currentCard = card;
            Titl.Text = card.Title;
            Descrip.Text = card.Description;
            ImageEntry.Text = card.Image;
            if (!string.IsNullOrWhiteSpace(card.Image))
            {
                PreviewImage.Source = card.Image;
                PreviewImage.IsVisible = true;
            }
        }
        else
        {
            currentCard = new Card();
        }
    }

    private void OnPreviewClicked(object sender, EventArgs e)
    {
        string imgPath = ImageEntry.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(imgPath))
        {
            PreviewImage.Source = imgPath;
            PreviewImage.IsVisible = true;
        }
        else
        {
            DisplayAlert("Ошибка", "Введите путь к изображению (например: fool.png)", "ОК");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        string title = Titl.Text?.Trim();
        string desc = Descrip.Text?.Trim();
        string img = ImageEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(desc))
        {
            await DisplayAlert("Ошибка", "Введите название и описание карты", "ОК");
            return;
        }

        currentCard.Title = title;
        currentCard.Description = desc;
        currentCard.Image = img;

        if (currentCard.Id != 0)
        {
            await db.UpdateCard(currentCard);
        }
        else
        {
            var allCards = await db.GetCard();
            currentCard.Id = allCards.Count > 0 ? allCards.Max(c => c.Id) + 1 : 1;
            await db.AddCard(currentCard);
        }

        await DisplayAlert("Сохранено", "Карта успешно добавлена/обновлена!", "ОК");
        await Navigation.PushAsync(new Prosmotrkolod(db));
    }
}