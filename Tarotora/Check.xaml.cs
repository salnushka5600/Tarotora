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

            string enteredTitle = TitleEntry.Text?.Trim() ?? ""; 
            string enteredKeywords = KeywordsEditor.Text?.Trim() ?? ""; 

          
            if (string.IsNullOrWhiteSpace(enteredTitle) && string.IsNullOrWhiteSpace(enteredKeywords))
            {
                await DisplayAlert("Ошибка", "Введите хотя бы название или ключевые слова", "OK"); 
                return;
            }

            int progress = 0; 

            
            if (!string.IsNullOrEmpty(enteredTitle) && string.Equals(enteredTitle, currentCard.Title, StringComparison.OrdinalIgnoreCase))
                progress += 50;


            var keywords = currentCard.Description.Split(' ', StringSplitOptions.RemoveEmptyEntries); 
            if (!string.IsNullOrWhiteSpace(enteredKeywords)) 
            {
                var enteredWords = enteredKeywords.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int matched = keywords.Count(k => enteredWords.Any(ew => ew.Equals(k, StringComparison.OrdinalIgnoreCase))); 
                progress += (int)(50.0 * matched / keywords.Length); 
            }

            var tests = await db.GetTests(); 
            var test = tests.FirstOrDefault(t => t.IdUser == currentUser.Id && t.IdCard == currentCard.Id); 

            if (test == null)
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
            else 
            {
                if (progress > test.Progress) 
                {
                    test.Progress = progress; 
                    test.CompletedAt = DateTime.Now; 
                    await db.UpdateTest(test);
                }
            }

            await DisplayAlert("Результат", $"Вы прошли карту на {progress}%", "OK");

            await LoadRandomCard();
        }
        
    }
}


