using Tarotora.BD;

namespace Tarotora
{
    public partial class MainPage : ContentPage
    {
        private DBfuncional db; // объект базы
        private User currentUser; // текущий пользователь

        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing() // когда страница отображается
        {
            base.OnAppearing();

            currentUser = User.GetUser(); // получаем текущего пользователя
            if (currentUser == null) // если пользователь не найден
            {
                await DisplayAlert("Ошибка", "Пользователь не найден", "ОК");
                await Shell.Current.GoToAsync("Login"); // возвращаемся на страницу входа
                return;
            }

            db = await DBfuncional.GetDB(); // получаем объект базы

            UserNameLabel.Text = currentUser.Name; // показываем имя
            UserSubscribeLabel.Text = $"Подписка: {currentUser.Subscribe}"; // подписка

            var allCards = await db.GetCards(); // получаем все карты
            var tests = (await db.GetTests())
                        .Where(t => t.IdUser == currentUser.Id)
                        .ToDictionary(t => t.IdCard, t => t.Progress); // получаем прогресс пользователя по картам

            var completedCards = allCards
                .Where(c => tests.ContainsKey(c.Id) && tests[c.Id] > 0) // фильтруем пройденные карты
                .Select(c =>
                {
                    c.Progress = tests[c.Id]; // записываем прогресс
                    return c;
                })
                .ToList();

            CompletedCardsView.ItemsSource = completedCards; // отображаем в CollectionView

            if (completedCards.Count > 0) // если есть пройденные карты
            {
                int totalProgress = completedCards.Sum(c => c.Progress) / completedCards.Count; // средний прогресс
                UserProgressLabel.Text = $"Пройдено всего: {totalProgress}% ({completedCards.Count} карт)";
            }
            else
            {
                UserProgressLabel.Text = "Вы еще не прошли ни одной карты"; // если нет прогресса
            }
        }

        private async void OnEditProfileClicked(object sender, EventArgs e) // кнопка редактирования профиля
        {
            await Shell.Current.GoToAsync($"EditUser?userId={currentUser.Id}"); // переходим на EditUserPage
        }

        private async void Exit(object sender, EventArgs e)
        {
            currentUser = null;
            User.PostUser(null);
            await Shell.Current.GoToAsync("Login");
        }
    }
}

