using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Tarotora.BD;

namespace Tarotora
{
    public partial class Check : ContentPage
    {
        private readonly DBfuncional db = DBfuncional.GetDB;

        // ВАЖНО: нужен конструктор без параметров
        public Check()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await ReloadUsers();
            await LoadCompletedForCurrent();
        }

        private async Task ReloadUsers()
        {
            var users = await db.GetUser();
            UserListView.ItemsSource = users;
        }

        private async Task LoadCompletedForCurrent()
        {
            var u = await db.GetCurrentUser();
            if (u == null)
            {
                CompletedCardsView.ItemsSource = null;
                Slider.Value = 0;
                PercentageLabel.Text = "Выберите пользователя";
                return;
            }

            // показываем пройденные карты
            CompletedCardsView.ItemsSource = await db.GetCompletedCardsByUser(u.Id);

            // считаем прогресс по тестам (если есть метод GetTest)
            var tests = await db.GetTest();
            var userTests = tests.Where(t => t.IdUser == u.Id).ToList();

            if (userTests.Count == 0)
            {
                Slider.Value = 0;
                PercentageLabel.Text = $"Прогресс пользователя {u.Name}: 0%";
            }
            else
            {
                var avg = userTests.Average(t => t.Progress);
                Slider.Value = avg;
                PercentageLabel.Text = $"Прогресс пользователя {u.Name}: {avg:0}%";
            }
        }

        private async void OnAddUserClicked(object sender, EventArgs e)
        {
            string name = await DisplayPromptAsync("Регистрация", "Введите имя пользователя:");
            if (string.IsNullOrWhiteSpace(name)) return;

            try
            {
                var user = await db.RegisterUser(name);
                await db.SetCurrentUser(user.Id);
                await ReloadUsers();
                await LoadCompletedForCurrent();
                await DisplayAlert("Готово", $"Пользователь «{user.Name}» зарегистрирован и выбран активным.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }

        private async void OnMakeActiveClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is int id)
            {
                await db.SetCurrentUser(id);
                await LoadCompletedForCurrent();
                var u = await db.GetCurrentUser();
                await DisplayAlert("Активный пользователь", $"Теперь активен: {u?.Name}", "OK");
            }
        }

        private async void OnUserSelected(object sender, SelectionChangedEventArgs e)
        {
            var selected = e.CurrentSelection?.FirstOrDefault() as User;
            if (selected == null)
            {
                CompletedCardsView.ItemsSource = null;
                Slider.Value = 0;
                PercentageLabel.Text = "Выберите пользователя";
                return;
            }

            await db.SetCurrentUser(selected.Id);
            await LoadCompletedForCurrent();
        }

        // Кнопка "Обновить прогресс"
        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await LoadCompletedForCurrent();
        }
    }
}
