using System.Data;
using System.Diagnostics;
using Tarotora.BD;
namespace Tarotora;

public partial class Addcards : ContentPage
{
    DBfuncional db;
    Card currentCard; 

    public Addcards(DBfuncional database, Card card = null)
	{
		InitializeComponent();
		db = database;
		Load();
        if (card != null)
        {
            currentCard = card;
            Titl.Text = currentCard.Title;
            Descrip.Text = currentCard.Description;
        }
        else
        {
            currentCard = new Card();
        }
    }
    public async void Load()
    {
        var images = await db.GetImage();
    }
    private async void Prosmotrkolod(object sender, EventArgs e)
    {
        string title = Titl.Text?.Trim();
        string dis = Descrip.Text?.Trim();

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(dis))
        {
            await DisplayAlert("Ошибка", "Напишите название и описание карты", "Ок");
            return;
        }

        currentCard.Title = title;
        currentCard.Description = dis;

        if (currentCard.Id != 0)
        {
            await db.UpdateCard(currentCard);
        }
        else
        {
            await db.AddCard(currentCard);
        }

        await Navigation.PushAsync(new Prosmotrkolod(db));
    }

    
	
}