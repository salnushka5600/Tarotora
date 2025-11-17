//using Microsoft.UI.Xaml.Controls.Primitives;
using System.Data;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using Microsoft.Maui.Controls;
using Tarotora.BD;
namespace Tarotora;

public partial class Addcards : ContentPage
{
    // Переменная для работы с базой данных
    private DBfuncional db;
    public Addcards()
    {
        InitializeComponent();
        InitDB();  // Инициализация базы данных
    }
    //  Получаем экземпляр базы данных 
    private async void InitDB()
    {
        db = await DBfuncional.GetDB();
    }

    // Кнопка "Показать изображение"
    private void OnPreviewClicked(object sender, EventArgs e)
    {
        string imgPath = ImageEntry.Text; // Берём путь из Entry
        if (!string.IsNullOrWhiteSpace(imgPath))
        {
            PreviewImage.Source = imgPath; // Показываем изображение
            PreviewImage.IsVisible = true; // Делаем Image видимым
        }
        else
        {
            DisplayAlert("Ошибка", "Введите путь к изображению (например: fool.png)", "ОК");
        }
    }

    //  Кнопка "Сохранить карту" 
    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (db == null) return; // защита на случай, если база еще не инициализирована

        string title = Titl.Text;        // Название карты
        string desc = Descrip.Text;      // Описание
        string img = ImageEntry.Text;    // Путь к изображению

        // Проверка, что все поля заполнены
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(desc) || string.IsNullOrWhiteSpace(img))
        {
            await DisplayAlert("Ошибка", "Введите категорию, название, описание и изображение карты", "ОК");
            return;
        }

        // Создаём объект карты
        var currentCard = new Card
        {
            Title = title,
            Description = desc,
            Image = img,
        };

        await db.AddCard(currentCard); // Сохраняем карту в базе

        await DisplayAlert("Сохранено", "Карта успешно добавлена!", "ОК");

        // Очищаем поля после сохранения
        Titl.Text = string.Empty;
        Descrip.Text = string.Empty;
        ImageEntry.Text = string.Empty;
        PreviewImage.Source = null;
        PreviewImage.IsVisible = false;
    }
}