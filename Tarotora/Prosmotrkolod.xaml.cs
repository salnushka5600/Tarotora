using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using Tarotora.BD;

namespace Tarotora;

public partial class Prosmotrkolod : ContentPage
{
    private DBfuncional db;
    private User currentUser; 

    public bool IsAdmin => currentUser?.IsAdmin ?? false; 
    public Prosmotrkolod()
    {
        InitializeComponent();
        BindingContext = this; 
    }

    private async Task LoadCards() 
    {
        var allCards = await db.GetCards(); 
        var tests = (await db.GetTests()).Where(t => t.IdUser == currentUser.Id).ToList(); 

        foreach (var c in allCards)
        {
            var test = tests.FirstOrDefault(t => t.IdCard == c.Id); 
            c.Progress = test?.Progress ?? 0; 
        }

        CardsView.ItemsSource = allCards; 
    }

    private async void OnEditClicked(object sender, EventArgs e) 
    {
        if (!IsAdmin) return;
        if (sender is Button btn && btn.CommandParameter is Card card)
        {
            await Shell.Current.GoToAsync($"EditCard?cardId={card.Id}"); 
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e) 
    {
        if (!IsAdmin) return; 
        if (sender is Button btn && btn.CommandParameter is Card card)
        {
            bool confirm = await DisplayAlert("Удаление", $"Удалить карту {card.Title}?", "Да", "Нет"); 
            if (!confirm) return;

            await db.RemoveCard(card.Id); 
            await LoadCards(); 
        }
    }

    protected async override void OnAppearing() // при отображении страницы
    {
        base.OnAppearing();
        currentUser = User.GetUser(); 
        if (currentUser == null) return;

        db = await DBfuncional.GetDB();
        await LoadCards(); 
    }

}
