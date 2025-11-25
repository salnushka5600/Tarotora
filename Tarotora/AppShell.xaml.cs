using System;
using Tarotora.BD;

namespace Tarotora
{
    public partial class AppShell : Shell
    {
        public DBfuncional db; 

        public AppShell()
        {
            InitializeComponent();

            // Инициализация базы асинхронно через Task.Run
            Task.Run(async () =>
            {
                db = await DBfuncional.GetDB();
                await db.InitDB(); // создаём дефолтные данные (админ, стартовые карты)
            });

           
            Routing.RegisterRoute("Registre", typeof(RegistrationPage));
            Routing.RegisterRoute("Login", typeof(LoginPage));
            Routing.RegisterRoute("Main", typeof(MainPage));
            Routing.RegisterRoute("CheckCard", typeof(Check));
            Routing.RegisterRoute("ProsmotrKolod", typeof(Prosmotrkolod));
            Routing.RegisterRoute("Addcards", typeof(Addcards));
            Routing.RegisterRoute("EditCard", typeof(EditCardPage));
            Routing.RegisterRoute("DeleteUsers", typeof(DeleteUser));
            Routing.RegisterRoute("EditUser", typeof(EditUserPage));

            Navigating += OnShellNavigating; 
        }


        
        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateMenu(); 
        }


        //  Проверка перед навигацией
        private void OnShellNavigating(object sender, ShellNavigatingEventArgs e)
        {
            // Игнорируем обратную навигацию
            if (e.Target.Location.OriginalString == null) return;

            var user = User.GetUser(); 

            // Если пользователь не залогинен и пытается перейти на любую страницу кроме Login или Registration, перенаправляем на Login
            if (user == null &&
                !e.Target.Location.OriginalString.Contains("Login") &&
                !e.Target.Location.OriginalString.Contains("Registre"))
            {
                e.Cancel(); // отменяем навигацию
                Shell.Current.GoToAsync("///Login"); // отправляем на LoginPage
            }
        }

        //   Обновляем меню в зависимости от пользователя
        public void UpdateMenu() 
        {
            Items.Clear(); 

            var user = User.GetUser(); 

            if (user == null)
            {
                
                Items.Add(new ShellContent { Title = "Login", Route = "Login", ContentTemplate = new DataTemplate(typeof(LoginPage)) });
                return;
            }

            // Общие пункты для всех пользователей
            Items.Add(new FlyoutItem { Title = "Профиль", Items = { new ShellContent { Title = "Профиль", ContentTemplate = new DataTemplate(typeof(MainPage)) } } });
            Items.Add(new FlyoutItem { Title = "Пройти карту", Items = { new ShellContent { Title = "Пройти карту", ContentTemplate = new DataTemplate(typeof(Check)) } } });
            Items.Add(new FlyoutItem { Title = "Колода", Items = { new ShellContent { Title = "Колода", ContentTemplate = new DataTemplate(typeof(Prosmotrkolod)) } } });

            // Админские пункты видны только администратору
            if (user.IsAdmin)
            {
                Items.Add(new FlyoutItem
                {
                    Title = "Админка",
                    Items =
                    {
                        new ShellContent { Title = "Добавить карту", ContentTemplate = new DataTemplate(typeof(Addcards)) },
                        new ShellContent { Title = "Пользователи", ContentTemplate = new DataTemplate(typeof(DeleteUser)) }
                    }
                });
            }

        }

    }
}
