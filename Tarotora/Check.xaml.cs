using Tarotora.BD;

namespace Tarotora;

public partial class Check : ContentPage
{
    DBfuncional db;
    private int _userId;
    private List<User> users;

    public Check(DBfuncional database)
    {
        InitializeComponent();
        db = database;
        LoadUsers();
       
        PercentageLabel.Text = $"{Slider.Value}%";
        Slider.ValueChanged += (sender, e) =>
        {
            PercentageLabel.Text = $"{e.NewValue}%";
        };
    }

 
    private async void LoadUsers()
    {
        users = await db.GetUser();
        UsersListView.ItemsSource = users;
    }

    private void OnUserSelected(object sender, SelectedItemChangedEventArgs e)
    {
        var selectedUser = e.SelectedItem as User;
        if (selectedUser != null)
        {
           _userId = selectedUser.Id;
            var testRecord = db.tests.FirstOrDefault(t => t.IdUser == this._userId && t.IdCard == 1);
            if (testRecord != null)
            {
                Slider.Value = testRecord.Progress;
                PercentageLabel.Text = $"{testRecord.Progress}%";
            }
      
            PercentageLabel.Text = $"{Slider.Value}%";
            Slider.ValueChanged += (sender, e) =>
            {
                PercentageLabel.Text = $"{e.NewValue}%";
            };

        }
    }
}
        

     