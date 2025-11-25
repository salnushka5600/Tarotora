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
        db = await DBfuncional.GetDB(); 
        card = await db.GetCardById(cardId);

        if (card != null) //карта найдена
        {
            TitleEntry.Text = card.Title; 
            DescriptionEditor.Text = card.Description; 
            PreviewImage.Source = card.Image;  
            PreviewImage.IsVisible = !string.IsNullOrEmpty(card.Image);
        }
        else //карта не найдена
        {
            await DisplayAlert("Ошибка", "Карта не найдена", "ОК"); 
            await Shell.Current.GoToAsync(".."); 
        }
    }

   

    private async void OnSaveClicked(object sender, EventArgs e) 
    {
        if (card == null) return; 

        string newTitle = TitleEntry.Text; 
        string newDesc = DescriptionEditor.Text; 
       

        if (string.IsNullOrWhiteSpace(newTitle) || string.IsNullOrWhiteSpace(newDesc)) 
        {
            await DisplayAlert("Ошибка", "Заполните все поля", "ОК"); 
            return;
        }

        //обновляем
        card.Title = newTitle; 
        card.Description = newDesc;
       
        if (fileResult != null)
        {
            var img = Path.Combine(FileSystem.Current.AppDataDirectory, fileResult.FileName);

            using (var sourceStream = await fileResult.OpenReadAsync())
            using (var destinationStream = File.Open(img, FileMode.Create))
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