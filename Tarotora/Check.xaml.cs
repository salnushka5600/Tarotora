using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Tarotora.BD;

namespace Tarotora
{
    public partial class Check : ContentPage
    {
        private DBfuncional db;      
        private Card currentCard;     
        private User currentUser;    
        public Check()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing() //при заходе на страницу срабатывает этот метод
        {
            base.OnAppearing(); 

            currentUser = User.GetUser(); // currentUser это текущий пользователь и мы в него передаем список пользователей

            db = await DBfuncional.GetDB(); //в переменную передаем базу данных
            await LoadRandomCard(); //метод загрузки рандомных карт
        }



        
        private async Task LoadRandomCard() //метод загрузки рандомных карт
        {
            var allCards = await db.GetCards(); //передаем список карт в переменную allCards
            var tests = (await db.GetTests()) //передаем список тестов в переменную tests
                        .Where(t => t.IdUser == currentUser.Id) // t это тесты у тестов есть t.IdUser и сравнивает currentUser у текущего пользователя чтобы совпадали id
                        .ToDictionary(t => t.IdCard, t => t.Progress); // тут смотрится у какой карты какой прогресс по id 

            var rand = new Random(); 
            currentCard = allCards[rand.Next(allCards.Count)]; // он принимает все карты которые есть и рандомит следующую карту

            CardTitleLabel.Text = "Угадайте карту!"; 
            CardImage.Source = currentCard.Image; //он выводит изображение карты 
            TitleEntry.Text = string.Empty; //очищение полей 
            KeywordsEditor.Text = string.Empty; //очищение полей

            
            if (tests.TryGetValue(currentCard.Id, out int prevProgress) && prevProgress > 0) //это чтобы показывала результат сразу после того как ты написал название и описание у карты 
            {
                await DisplayAlert("Информация", $"Эта карта уже частично пройдена: {prevProgress}%", "OK");
            }
        }


       
        private async void OnCheckCardClicked(object sender, EventArgs e) //кнопка проверить как работает
        {
            if (currentCard == null) return; // если текущая карта пустая то пропускаем остальной код

            string enteredTitle = TitleEntry.Text?.Trim() ?? ""; // получаем название карты, введённое пользователем, убираем пробелы
            string enteredKeywords = KeywordsEditor.Text?.Trim() ?? ""; // получаем ключевые слова, убираем пробелы

          
            if (string.IsNullOrWhiteSpace(enteredTitle) && string.IsNullOrWhiteSpace(enteredKeywords)) //это проверку на то чтобы не было пустых карт IsNullOrWhiteSpace он проверяет пустая ли это строка и состоит ли это слово из пробелов
            {
                await DisplayAlert("Ошибка", "Введите хотя бы название или ключевые слова", "OK"); 
                return;
            }

            int progress = 0; 

            
            if (!string.IsNullOrEmpty(enteredTitle) && string.Equals(enteredTitle, currentCard.Title, StringComparison.OrdinalIgnoreCase)) //IsNullOrEmpty проверяет что есть в строке пустая ли она string.Equals проверяет правильность названия StringComparison.OrdinalIgnoreCase указывает правило сравнение строк то есть это сравнение по байтам 
                progress += 50;


            var keywords = currentCard.Description.Split(' ', StringSplitOptions.RemoveEmptyEntries); // Split это разбивает предложение на пробелы StringSplitOptions.RemoveEmptyEntries а это говорит чтобы удалил пустые элементы
            if (!string.IsNullOrWhiteSpace(enteredKeywords)) // если пользователь ввёл ключевые слова
            {
                var enteredWords = enteredKeywords.Split(' ', StringSplitOptions.RemoveEmptyEntries); // разбиваем на слова
                int matched = keywords.Count(k => enteredWords.Any(ew => ew.Equals(k, StringComparison.OrdinalIgnoreCase))); // считаем совпадения
                progress += (int)(50.0 * matched / keywords.Length); 
            }

            var tests = await db.GetTests(); 
            var test = tests.FirstOrDefault(t => t.IdUser == currentUser.Id && t.IdCard == currentCard.Id); //тест текущего пользователя для этой карты

            if (test == null) // теста нет
            {
                test = new Test //создается новый тест
                {
                    IdUser = currentUser.Id, 
                    IdCard = currentCard.Id, 
                    Progress = progress, 
                    CompletedAt = DateTime.Now 
                };
                await db.AddTest(test); //добавление теста в базу данных
            }
            else //тест есть
            {
                if (progress > test.Progress) // если прогресс стал больше
                {
                    test.Progress = progress; // обновление
                    test.CompletedAt = DateTime.Now; // обновление 
                    await db.UpdateTest(test);
                }
            }

            await DisplayAlert("Результат", $"Вы прошли карту на {progress}%", "OK");

            await LoadRandomCard();
        }
        
    }
}


