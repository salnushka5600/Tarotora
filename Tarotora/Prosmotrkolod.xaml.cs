using System.Diagnostics.Metrics;
using Tarotora.BD;

namespace Tarotora;

public partial class Prosmotrkolod : ContentPage
{
    DBfuncional db;
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
            await Navigation.PushAsync(new Addcards(db));
    }
}