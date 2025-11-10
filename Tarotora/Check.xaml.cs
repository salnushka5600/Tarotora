using Tarotora.BD;

namespace Tarotora;

public partial class Check : ContentPage
{
    private readonly DBfuncional db;
    private int userId;
    private List<User> users = new();

    public Check(DBfuncional database)
    {
        InitializeComponent();
        db = database;
        LoadUsers();
    }

    private async void LoadUsers()
    {
        users = await db.GetUser();
        UsersListView.ItemsSource = users;
    }

    private async void OnUserSelected(object sender, SelectionChangedEventArgs e) //хранит события
    {
        if (e.CurrentSelection.FirstOrDefault() is not User selectedUser) // список выбранных элементов которые хранят события можем выбрать только один первый который подходит под условие
            return;

        userId = selectedUser.Id;

        var allTests = await db.GetTest();
        var record = allTests.FirstOrDefault(t => t.IdUser == userId); //первая запись теста которая пренадлежит выбранному пользователю есть пользователь или нет
        int progress = record?.Progress ?? 0;

        Slider.Value = progress;
        PercentageLabel.Text = $"{selectedUser.Name} прошёл {progress}% карт";
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        if (users.Count == 0)
        {
            await DisplayAlert("Ошибка", "Нет пользователей для обновления", "ОК");
            return;
        }

        var rand = new Random();
        var tests = await db.GetTest();

        foreach (var test in tests)
        {
            test.Progress = rand.Next(0, 101);
        }

        if (userId != 0)
        {
            var updated = tests.FirstOrDefault(t => t.IdUser == userId);
            var currentUser = users.FirstOrDefault(u => u.Id == userId);

            if (updated != null && currentUser != null)
            {
                Slider.Value = updated.Progress;
                PercentageLabel.Text = $"{currentUser.Name} прошёл(ла) {updated.Progress}% карт";
            }
        }
        else
        {
            await DisplayAlert("Подсказка", "Сначала выберите пользователя", "ОК");
        }
    }
}



