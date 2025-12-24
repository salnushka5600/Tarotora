using Tarotora.BD;

namespace Tarotora;

[QueryProperty(nameof(CardId), "cardId")] // позволяет передавать параметр cardId через Shell навигацию
public partial class EditCardPage : ContentPage
{
    private DBfuncional db; 
    private Card card;
    private FileResult? fileResult;

    public EditCardPage()
    {
        InitializeComponent();
    }

    private int cardId; 
    public int CardId 
    {
        get => cardId;
        set
        {
            cardId = value; 
            _ = LoadCard(); 
        }
    }

    private async Task LoadCard() 
    {
        db = await DBfuncional.GetDB(); //получаем бд
        card = await db.GetCardById(cardId); //получаем карты по id

        if (card != null) //карта найдена
        {
            TitleEntry.Text = card.Title; //выводит название
            DescriptionEditor.Text = card.Description; 
            PreviewImage.Source = card.Image;  
            PreviewImage.IsVisible = !string.IsNullOrEmpty(card.Image); //если у карты есть картинка показывает картинку короче чтобы не было большого пробела но эта строка по сути тут не нужна но удалять страшно
        }
        else //карта не найдена
        {
            await DisplayAlert("Ошибка", "Карта не найдена", "ОК"); 
            await Shell.Current.GoToAsync(".."); // возврат назад
        }
    }

   

    private async void OnSaveClicked(object sender, EventArgs e) //кнопка сохранить изменения
    {
        if (card == null) return; //если карта пустая то пропускаем следующий код 

        string newTitle = TitleEntry.Text; //то что вписано помещается в переменную 
        string newDesc = DescriptionEditor.Text; 
       

        if (string.IsNullOrWhiteSpace(newTitle) || string.IsNullOrWhiteSpace(newDesc)) // чтобы пустой не сохранили
        {
            await DisplayAlert("Ошибка", "Заполните все поля", "ОК"); 
            return;
        }

        //обновляем
        card.Title = newTitle; 
        card.Description = newDesc;
       
        if (fileResult != null) //
        {
            var img = Path.Combine(FileSystem.Current.AppDataDirectory, fileResult.FileName); //FileSystem.Current.AppDataDirectory это путь к файлу, fileResult.FileName это название файла, Path.Combine чтобы хорошо склеил путь и название

            using (var sourceStream = await fileResult.OpenReadAsync()) //открывает для чтения файл
            using (var destinationStream = File.Open(img, FileMode.Create)) // 
            {
                await sourceStream.CopyToAsync(destinationStream);
            }
            card.Image = img;
        }

        await db.UpdateCard(card); 

        await DisplayAlert("Сохранено", "Карта успешно обновлена", "ОК"); 

        await Shell.Current.GoToAsync(".."); 
    }

    private async void LoadImage(object sender, EventArgs e)
    {
        var type = new Dictionary<DevicePlatform, IEnumerable<string>>(); // dictionary словарь в котором хранятся расширения файла эта строчка просто хранит расширения файла
        type[DevicePlatform.WinUI] = new List<string> //задаем типы расширения файлов которые можно добавлять, для windows платформы
        {
            ".png",
            ".jpg",
            ".jpeg",
            ".webp"
        };
        PickOptions pickOptions = new PickOptions(); //создает окно в котором мы будем выбирать файлы
        pickOptions.FileTypes = new FilePickerFileType(type); //передаем какие типы файла можно загружать png и тд
        FileResult? fileResult = await FilePicker.Default.PickAsync(pickOptions); //те файлы которые выбрали в filepicker передаем их в fileresult
        if (fileResult != null) 
        {
            Stream stream = await fileResult.OpenReadAsync(); // открываем файл для чтения
            PreviewImage.Source = ImageSource.FromStream(() => stream); // с того файла который открыли для чтения переносим в этот файл приложения
            this.fileResult = fileResult; // this.fileResult глобальная переменная и  fileResult локальная локальную ты не можешь использовать в других методах а глобаную можешь поэтому из локальной переменной переводим все в глобальную
        }
        else 
            await DisplayAlert("Ошибка", "Не выбран файл", "Ок");

    }
}