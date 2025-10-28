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
        LoadCards();
    }

    private async void LoadCards()
    {
        var cards = await db.GetCard();
        CardsView.ItemsSource = null;
        CardsView.ItemsSource = cards;
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var card = button.CommandParameter as Card;
        if (card == null) return;

        string newTitle = await DisplayPromptAsync("��������������", "������� ����� ��������:", initialValue: card.Title);
        if (newTitle == null) return;
        string newDesc = await DisplayPromptAsync("��������������", "������� ����� ��������:", initialValue: card.Description);
        if (newDesc == null) return;

        bool changeImage = await DisplayAlert("�����������", "������ �������� �����������?", "��", "���");
        if (changeImage) 
        {
            string newImage = await DisplayPromptAsync("��������������", "������� ���� � ����������� (��������: fool.png):", initialValue: card.Image);
            if(!string.IsNullOrWhiteSpace(newImage)) 
                card.Image = newImage;
        }


        card.Title = newTitle;
        card.Description = newDesc;

        await db.UpdateCard(card);
        await DisplayAlert("���������", "��������� ������� ���������!", "��");
        LoadCards();
    }
}
