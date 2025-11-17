using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tarotora.BD
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }         // Имя для отображения
        public string Login { get; set; }        // Логин для входа
        public string Password { get; set; }     // Пароль
        public bool IsAdmin { get; set; }        // Админ или нет
        public bool Subscribe { get; set; }      // Есть подписка
        public int IdCard { get; set; }          // Можно оставить для привязки карт
        public Card Card { get; set; }

        //  Переменная для хранения текущего пользователя
        private static User user;

        //  Метод для получения текущего пользователя 

        public static User GetUser()
        {
            return user; // Возвращает объект User, который сейчас сохранён в переменной user
        }
        //  Метод для сохранения пользователя
        
        public static void PostUser(User userr) //  Принимает объект User и кладёт его в переменную user
        {
            user = userr; // "положили" пользователя в общий ящик
        }
    }
}
