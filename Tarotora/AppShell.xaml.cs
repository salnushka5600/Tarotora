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

            
            Task.Run(async () => // он вызывается в самом начале и все это компилируется короче я сама не поняла
            {
                db = await DBfuncional.GetDB();
                await db.InitDB(); // создание
            });

           
            Routing.RegisterRoute("Registre", typeof(RegistrationPage)); //регистрация маршрутов для дальнешого использования а typeof(RegistrationPage)) это обозначение для какой страницы 
            Routing.RegisterRoute("Login", typeof(LoginPage));
            Routing.RegisterRoute("Main", typeof(MainPage));
            Routing.RegisterRoute("CheckCard", typeof(Check));
            Routing.RegisterRoute("ProsmotrKolod", typeof(Prosmotrkolod));
            Routing.RegisterRoute("Addcards", typeof(Addcards));
            Routing.RegisterRoute("EditCard", typeof(EditCardPage));
            Routing.RegisterRoute("DeleteUsers", typeof(DeleteUser));
            Routing.RegisterRoute("EditUser", typeof(EditUserPage));

            Navigating += OnShellNavigating; // эта строка проверяет чтобы ты находился в логине если ты не зарегистрирован этот метод ниже описан OnShellNavigating
        }


        
        protected override void OnAppearing() //этот метод вызывается при открытии окна 
        {
            base.OnAppearing(); // это обязательно он в любом случае вписывается
            UpdateMenu(); //передаем метод обновления меню
        }


        private void OnShellNavigating(object sender, ShellNavigatingEventArgs e) // этот метод нужен для того чтобы незарегистрированный пользователь не мог войти в приложение без регистрации
        {
            if (e.Target.Location.OriginalString == null) return; //если страницы нет то то остальной код пропускается return - это пропуск остального кода

            var user = User.GetUser();  // передаем список пользователей

            if (user == null && //если пользователя нет или он находится не в логине и регистре то отправляет в логин
                !e.Target.Location.OriginalString.Contains("Login") &&
                !e.Target.Location.OriginalString.Contains("Registre"))
            {
                e.Cancel(); //закрывается текущее окно
                Shell.Current.GoToAsync("///Login"); // это строка отправляет в логин три /// это возврат с текущего окна в логин
            }
        }

       
        public void UpdateMenu() //обновление меню
        {
            Items.Clear(); //очищение меню потому что у нас есть админ и пользователь и чтобы админская меню была в итоге у админа а у пользователя обычная платформа

            var user = User.GetUser(); //передаем в переменную user список пользователей

            if (user == null) // если пользователь не зарегистрирован 
            {
                Items.Add(new ShellContent { Title = "Login", Route = "Login", ContentTemplate = new DataTemplate(typeof(LoginPage)) }); // то в меню добавляется логин и путь к логину короче чтобы в меню был один логин 
                return;
            }

            // МЕНЮШКА
            Items.Add(new FlyoutItem { Title = "Профиль", Items = { new ShellContent { Title = "Профиль", ContentTemplate = new DataTemplate(typeof(MainPage)) } } }); // это то что у нас будет находиться в меню (typeof(MainPage)) это обозначение на какую страницу будет переход ContentTemplate = new DataTemplate(typeof(MainPage)) это для маршрута
            Items.Add(new FlyoutItem { Title = "Пройти карту", Items = { new ShellContent { Title = "Пройти карту", ContentTemplate = new DataTemplate(typeof(Check)) } } });
            Items.Add(new FlyoutItem { Title = "Колода", Items = { new ShellContent { Title = "Колода", ContentTemplate = new DataTemplate(typeof(Prosmotrkolod)) } } });

           //админка
            if (user.IsAdmin)
            {
                Items.Add(new FlyoutItem // создание разделения 
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
