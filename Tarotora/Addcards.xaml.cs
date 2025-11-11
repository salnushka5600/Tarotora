//using Microsoft.UI.Xaml.Controls.Primitives;
using System.Data;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using Microsoft.Maui.Controls;
using Tarotora.BD;
namespace Tarotora;

public partial class Addcards : ContentPage
{
    private readonly DBfuncional db;

    public Addcards(DBfuncional database)
    {
        InitializeComponent();
        db = database;

    }

    private void OnPreviewClicked(object sender, EventArgs e)
    {
        string imgPath = ImageEntry.Text; 
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
        string title = Titl.Text;
        string desc = Descrip.Text;
        string img = ImageEntry.Text;

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(desc) || string.IsNullOrWhiteSpace(img))
        {
            await DisplayAlert("Ошибка", "Введите категорию, название, описание и изображение карты", "ОК");
            return;
        }
        var currentCard = new Card
        {

            Title = title,
            Description = desc,
            Image = img,
        };
        
            await db.AddCard(currentCard);

        await DisplayAlert("Сохранено", "Карта успешно добавлена!", "ОК");
        await Navigation.PushAsync(new Prosmotrkolod(db));
    }
}