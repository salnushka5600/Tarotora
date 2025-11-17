using Tarotora.BD;

namespace Tarotora;

[QueryProperty(nameof(CardId), "cardId")] // позволяет передавать параметр cardId через Shell навигацию
public partial class EditCardPage : ContentPage
{
    private DBfuncional db; // объект базы данных
    private Card card; // карта, которую редактируем

    public EditCardPage()
    {
        InitializeComponent();
    }

    private int cardId; // переменная для хранения Id карты
    public int CardId // свойство для привязки параметра навигации
    {
        get => cardId;
        set
        {
            cardId = value; // сохраняем Id
            _ = LoadCard(); // запускаем метод загрузки карты асинхронно
        }
    }

    private async Task LoadCard() // метод загрузки карты из базы
    {
        db = await DBfuncional.GetDB(); // получаем объект базы данных
        card = await db.GetCardById(cardId); // получаем карту по Id

        if (card != null) // если карта найдена
        {
            TitleEntry.Text = card.Title; // показываем название
            DescriptionEditor.Text = card.Description; // описание карты
            ImageEntry.Text = card.Image; // путь к изображению
            PreviewImage.Source = card.Image; // показываем изображение
            PreviewImage.IsVisible = !string.IsNullOrEmpty(card.Image); // делаем видимым только если есть изображение
        }
        else // если карта не найдена
        {
            await DisplayAlert("Ошибка", "Карта не найдена", "ОК"); // показываем ошибку
            await Shell.Current.GoToAsync(".."); // возвращаемся на предыдущую страницу
        }
    }

    private void OnPreviewClicked(object sender, EventArgs e) // метод при нажатии кнопки "Показать изображение"
    {
        string imgPath = ImageEntry.Text; // получаем путь из поля
        if (!string.IsNullOrWhiteSpace(imgPath)) // если что-то введено
        {
            PreviewImage.Source = imgPath; // показываем изображение
            PreviewImage.IsVisible = true; // делаем видимым
        }
        else
        {
            DisplayAlert("Ошибка", "Введите путь к изображению (например: fool.png)", "ОК"); // предупреждаем
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e) // метод при нажатии кнопки "Сохранить"
    {
        if (card == null) return; // если карта не загружена, выходим

        string newTitle = TitleEntry.Text; // новое название
        string newDesc = DescriptionEditor.Text; // новое описание
        string newImage = ImageEntry.Text; // новый путь к изображению

        if (string.IsNullOrWhiteSpace(newTitle) || string.IsNullOrWhiteSpace(newDesc) || string.IsNullOrWhiteSpace(newImage)) // проверка заполнения
        {
            await DisplayAlert("Ошибка", "Заполните все поля", "ОК"); // предупреждаем
            return;
        }

        card.Title = newTitle; // обновляем название карты
        card.Description = newDesc; // обновляем описание
        card.Image = newImage; // обновляем путь к изображению

        await db.UpdateCard(card); // сохраняем изменения в базе

        await DisplayAlert("Сохранено", "Карта успешно обновлена", "ОК"); // показываем сообщение

        await Shell.Current.GoToAsync(".."); // возвращаемся на предыдущую страницу
    }
}