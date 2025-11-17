using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Tarotora.BD;

namespace Tarotora
{
    public partial class Check : ContentPage
    {
        private DBfuncional db;      // переменная для базы данных
        private Card currentCard;     // карта, которую пользователь проходит сейчас
        private User currentUser;     // текущий пользователь
        public Check()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing() // метод, который вызывается когда страница отображается
        {
            base.OnAppearing(); 

            currentUser = User.GetUser(); // получаем текущего пользователя (если он залогинен)
            if (currentUser == null) return; // если пользователь не залогинен, выходим из метода

            db = await DBfuncional.GetDB(); // получаем объект базы данных (асинхронно)
            await LoadRandomCard(); // загружаем случайную карту для прохождения
        }



        //  Загружаем случайную карту 
        private async Task LoadRandomCard()
        {
            var allCards = await db.GetCards(); // получаем все карты из базы
            var tests = (await db.GetTests()) // получаем все тесты
                        .Where(t => t.IdUser == currentUser.Id) // оставляем только тесты текущего пользователя
                        .ToDictionary(t => t.IdCard, t => t.Progress); // превращаем в словарь: Id карты → прогресс

            var rand = new Random(); // создаём генератор случайных чисел
            currentCard = allCards[rand.Next(allCards.Count)]; // выбираем случайную карту из списка

            CardTitleLabel.Text = "Угадайте карту!"; // показываем подсказку пользователю
            CardImage.Source = currentCard.Image; // показываем изображение карты
            TitleEntry.Text = string.Empty; // очищаем поле для ввода названия карты
            KeywordsEditor.Text = string.Empty; // очищаем поле для ввода ключевых слов

            // Если карта уже частично пройдена, показываем прогресс
            if (tests.TryGetValue(currentCard.Id, out int prevProgress) && prevProgress > 0)
            {
                await DisplayAlert("Информация", $"Эта карта уже частично пройдена: {prevProgress}%", "OK");
            }
        }


        // Кнопка проверить
        private async void OnCheckCardClicked(object sender, EventArgs e)
        {
            if (currentCard == null) return; // если карта не загружена, выходим

            string enteredTitle = TitleEntry.Text?.Trim() ?? ""; // получаем название карты, введённое пользователем, убираем пробелы
            string enteredKeywords = KeywordsEditor.Text?.Trim() ?? ""; // получаем ключевые слова, убираем пробелы

            // если пользователь ничего не ввёл
            if (string.IsNullOrWhiteSpace(enteredTitle) && string.IsNullOrWhiteSpace(enteredKeywords))
            {
                await DisplayAlert("Ошибка", "Введите хотя бы название или ключевые слова", "OK"); // показываем ошибку
                return;
            }

            int progress = 0; // переменная для подсчёта прогресса

            // если название карты угадано правильно, даём 50% прогресса
            if (!string.IsNullOrEmpty(enteredTitle) && string.Equals(enteredTitle, currentCard.Title, StringComparison.OrdinalIgnoreCase))
                progress += 50;


            var keywords = currentCard.Description.Split(' ', StringSplitOptions.RemoveEmptyEntries); // разбиваем описание карты на слова
            if (!string.IsNullOrWhiteSpace(enteredKeywords)) // если пользователь ввёл ключевые слова
            {
                var enteredWords = enteredKeywords.Split(' ', StringSplitOptions.RemoveEmptyEntries); // разбиваем на слова
                int matched = keywords.Count(k => enteredWords.Any(ew => ew.Equals(k, StringComparison.OrdinalIgnoreCase))); // считаем совпадения
                progress += (int)(50.0 * matched / keywords.Length); // добавляем прогресс пропорционально угаданным словам
            }

            var tests = await db.GetTests(); // получаем все тесты
            var test = tests.FirstOrDefault(t => t.IdUser == currentUser.Id && t.IdCard == currentCard.Id); // ищем тест текущего пользователя для этой карты

            if (test == null) // если теста ещё нет
            {
                test = new Test // создаём новый тест
                {
                    IdUser = currentUser.Id, // пользователь
                    IdCard = currentCard.Id, // карта
                    Progress = progress, // прогресс
                    CompletedAt = DateTime.Now // время прохождения
                };
                await db.AddTest(test); // сохраняем в базе
            }
            else // если тест уже есть
            {
                if (progress > test.Progress) // если прогресс стал больше
                {
                    test.Progress = progress; // обновляем прогресс
                    test.CompletedAt = DateTime.Now; // обновляем дату прохождения
                    await db.UpdateTest(test); // сохраняем изменения в базе
                }
            }

            await DisplayAlert("Результат", $"Вы прошли карту на {progress}%", "OK"); // показываем результат

            await LoadRandomCard(); // загружаем следующую карту
        }
        
    }
}


