//using Microsoft.UI.Xaml.Controls.Primitives;
using System.Data;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using Microsoft.Maui.Controls;
using Tarotora.BD;
namespace Tarotora;

public partial class Addcards : ContentPage
{
   
    private DBfuncional db;
    private FileResult? fileResult;
    public Addcards()
    {
        InitializeComponent();
        InitDB();  
    }
    
    private async void InitDB()
    {
        db = await DBfuncional.GetDB();
    }


    
    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (db == null) return; // защита на случай, если база еще не инициализирована

        string title = Titl.Text;       
        string desc = Descrip.Text;
        var img = "";

        if (fileResult != null)
        {
            img = Path.Combine(FileSystem.Current.AppDataDirectory, fileResult.FileName);

            using (var sourceStream = await fileResult.OpenReadAsync())
            using (var destinationStream = File.Open(img, FileMode.Create))
            {
                await sourceStream.CopyToAsync(destinationStream);
            }
        }

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(desc))
        {
            await DisplayAlert("Ошибка", "Введите категорию, название, описание", "ОК");
            return;
        }

        // Создаём объект карты
        var currentCard = new Card
        {
            Title = title,
            Description = desc,
            Image = img,
        };

        await db.AddCard(currentCard); 

        await DisplayAlert("Сохранено", "Карта успешно добавлена!", "ОК");

        // Очищаем поля после сохранения
        Titl.Text = string.Empty;
        Descrip.Text = string.Empty;
        PreviewImage.Source = null;
        PreviewImage.IsVisible = false;
    }


    private async void LoadImage(object sender, EventArgs e)
    {
        var type = new Dictionary<DevicePlatform, IEnumerable<string>>();
        type[DevicePlatform.WinUI] = new List<string>
        {
            ".png",
            ".jpg",
            ".jpeg",
            ".webp"
        };
        PickOptions pickOptions = new PickOptions();
        pickOptions.FileTypes = new FilePickerFileType(type);
        FileResult? fileResult = await FilePicker.Default.PickAsync(pickOptions);
        if (fileResult != null)
        {
            Stream stream = await fileResult.OpenReadAsync();
            PreviewImage.Source = ImageSource.FromStream(() => stream);
            this.fileResult = fileResult;
        }
        else
            await DisplayAlert("Ошибка", "Не выбран файл", "Ок");
    }
}