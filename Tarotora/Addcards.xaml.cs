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
        db = await DBfuncional.GetDB(); // получаем бд get получение 
    }


    
    private async void OnSaveClicked(object sender, EventArgs e)
    {

        string title = Titl.Text;       
        string desc = Descrip.Text;
        var img = "";

        if (fileResult != null)
        {
            img = Path.Combine(FileSystem.Current.AppDataDirectory, fileResult.FileName); //Path.Combine он нужен для того чтобы хорошо склеился путь и нахвание файла

            using (var sourceStream = await fileResult.OpenReadAsync()) //для чтения
            using (var destinationStream = File.Open(img, FileMode.Create)) // открываем файл по нашему пути
            {
                await sourceStream.CopyToAsync(destinationStream); //содержимое передало в файл приложения
            }
        }

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(desc))
        {
            await DisplayAlert("Ошибка", "Введите название, описание", "ОК");
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

       //очистка
        Titl.Text = string.Empty;
        Descrip.Text = string.Empty;
        PreviewImage.Source = null;
    }


    private async void LoadImage(object sender, EventArgs e)
    {
        var type = new Dictionary<DevicePlatform, IEnumerable<string>>(); // dictionary словарь в котором хранятся расширения файла эта строчка просто хранит расширения файла
        type[DevicePlatform.WinUI] = new List<string> //создает расширения которые мы вписали ниже тут WinUI это виндовс платформа
        {
            ".png",
            ".jpg",
            ".jpeg",
            ".webp"
        };
        PickOptions pickOptions = new PickOptions(); //создает окно в котором мы будем выбирать файл
        pickOptions.FileTypes = new FilePickerFileType(type); //передаем какие типы файла можно загружать png и тд
        FileResult? fileResult = await FilePicker.Default.PickAsync(pickOptions); //те файлы которые выбрали в filepicker передаем их в fileresult
        if (fileResult != null) //если fileResult не нулевой
        {
            Stream stream = await fileResult.OpenReadAsync(); //открываем для чтения
            PreviewImage.Source = ImageSource.FromStream(() => stream); // передаем из файла который открыли для чтения в наше приложение короче картинку просто передаем в приложение
            this.fileResult = fileResult; // this.fileResult глобальная переменная и  fileResult локальная локальную ты не можешь использовать в других методах а глобаную можешь поэтому из локальной переменной переводим все в глобальную
        }
        else
            await DisplayAlert("Ошибка", "Не выбран файл", "Ок");
    }
}