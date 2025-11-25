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

        protected override async void OnAppearing() 
        {
            base.OnAppearing(); 

            currentUser = User.GetUser();
            if (currentUser == null) return; // если пользователь не залогинен, выходим 

            db = await DBfuncional.GetDB(); 
            await LoadRandomCard(); 
        }



        
        private async Task LoadRandomCard()
        {
            var allCards = await db.GetCards();
            var tests = (await db.GetTests()) 
                        .Where(t => t.IdUser == currentUser.Id) 
                        .ToDictionary(t => t.IdCard, t => t.Progress); 

            var rand = new Random(); 
            currentCard = allCards[rand.Next(allCards.Count)]; 

            CardTitleLabel.Text = "Угадайте карту!"; 
            CardImage.Source = currentCard.Image; 
            TitleEntry.Text = string.Empty; 
            KeywordsEditor.Text = string.Empty; 

            
            if (tests.TryGetValue(currentCard.Id, out int prevProgress) && prevProgress > 0)
            {
                await DisplayAlert("Информация", $"Эта карта уже частично пройдена: {prevProgress}%", "OK");
            }
        }


       
        private async void OnCheckCardClicked(object sender, EventArgs e)
        {
            if (currentCard == null) return; 

            string enteredTitle = TitleEntry.Text?.Trim() ?? ""; // получаем название карты, введённое пользователем, убираем пробелы
            string enteredKeywords = KeywordsEditor.Text?.Trim() ?? ""; // получаем ключевые слова, убираем пробелы

          
            if (string.IsNullOrWhiteSpace(enteredTitle) && string.IsNullOrWhiteSpace(enteredKeywords))
            {
                await DisplayAlert("Ошибка", "Введите хотя бы название или ключевые слова", "OK"); 
                return;
            }

            int progress = 0; 

            
            if (!string.IsNullOrEmpty(enteredTitle) && string.Equals(enteredTitle, currentCard.Title, StringComparison.OrdinalIgnoreCase))
                progress += 50;


            var keywords = currentCard.Description.Split(' ', StringSplitOptions.RemoveEmptyEntries); 
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
                test = new Test 
                {
                    IdUser = currentUser.Id, 
                    IdCard = currentCard.Id, 
                    Progress = progress, 
                    CompletedAt = DateTime.Now 
                };
                await db.AddTest(test); 
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


