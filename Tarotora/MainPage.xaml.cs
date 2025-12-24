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
            base.OnAppearing(); //базовая реализация метода

            currentUser = User.GetUser(); //получаем текущего авторизованного пользователя
            
            db = await DBfuncional.GetDB(); // получаем объект базы

            UserNameLabel.Text = currentUser.Name; // выводим имя пользователя на экран
            UserSubscribeLabel.Text = $"Подписка: {currentUser.Subscribe}"; // выводим информацию о подписке пользователя

            var allCards = await db.GetCards(); // получаем список ВСЕХ карт из базы
            var tests = (await db.GetTests()) //получаем все тесты
                        .Where(t => t.IdUser == currentUser.Id) //оставляем тесты только текущего пользователя
                        .ToDictionary(t => t.IdCard, t => t.Progress); // ToDictionary превращаем в словарь ключ Id карты значение прогресс по этой карте

            var completedCards = allCards
                .Where(c => tests.ContainsKey(c.Id) && tests[c.Id] > 0) // фильтруем пройденные карты и оставляем у которых прогресс больше нуля 
                .Select(c =>
                {
                    c.Progress = tests[c.Id]; // записываем прогресс из тестов прямо в объект карты
                    return c; //возвращаем обновленную карту 
                })
                .ToList(); //превращаем результат в список

            CompletedCardsView.ItemsSource = completedCards; // передаём список пройденных карт в элемент интерфейса

            if (completedCards.Count > 0) // если пользователь прошёл хотя бы одну карту
            {
                int totalProgress = completedCards.Sum(c => c.Progress) / completedCards.Count; // средний прогресс по всем картам
                UserProgressLabel.Text = $"Пройдено всего: {totalProgress}% ({completedCards.Count} карт)"; 
            }
            else
            {
                UserProgressLabel.Text = "Вы еще не прошли ни одной карты"; // нет прогресса
            }
        }

        private async void OnEditProfileClicked(object sender, EventArgs e) // кнопка редактирования профиля
        {
            await Shell.Current.GoToAsync($"EditUser?userId={currentUser.Id}");  // переходим на страницу редактирования профиля передаём id текущего пользователя 
        }

        private async void Exit(object sender, EventArgs e) //выход из аккаунта
        {
            currentUser = null; //очищаем текущего пользователя в памяти
            User.PostUser(null); //удаляем информацию об авторизованном пользователе
            await Shell.Current.GoToAsync("Login"); //переход на логин
        }
    }
}

