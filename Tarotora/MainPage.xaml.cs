using Tarotora.BD;

namespace Tarotora
{
    public partial class MainPage : ContentPage
    {
        private DBfuncional db; 
        private User currentUser; 

        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing() 
        {
            base.OnAppearing();

            currentUser = User.GetUser(); 
            
            db = await DBfuncional.GetDB(); 

            UserNameLabel.Text = currentUser.Name; 
            UserSubscribeLabel.Text = $"Подписка: {currentUser.Subscribe}"; 

            var allCards = await db.GetCards(); 
            var tests = (await db.GetTests())
                        .Where(t => t.IdUser == currentUser.Id)
                        .ToDictionary(t => t.IdCard, t => t.Progress); 

            var completedCards = allCards
                .Where(c => tests.ContainsKey(c.Id) && tests[c.Id] > 0) 
                .Select(c =>
                {
                    c.Progress = tests[c.Id]; 
                    return c;
                })
                .ToList();

            CompletedCardsView.ItemsSource = completedCards; 

            if (completedCards.Count > 0) 
            {
                int totalProgress = completedCards.Sum(c => c.Progress) / completedCards.Count; 
                UserProgressLabel.Text = $"Пройдено всего: {totalProgress}% ({completedCards.Count} карт)";
            }
            else
            {
                UserProgressLabel.Text = "Вы еще не прошли ни одной карты"; 
            }
        }

        private async void OnEditProfileClicked(object sender, EventArgs e) 
        {
            await Shell.Current.GoToAsync($"EditUser?userId={currentUser.Id}"); 
        }

        private async void Exit(object sender, EventArgs e)
        {
            currentUser = null;
            User.PostUser(null);
            await Shell.Current.GoToAsync("Login");
        }
    }
}

