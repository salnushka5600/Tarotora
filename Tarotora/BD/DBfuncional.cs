using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace Tarotora.BD
{

    public class DBDTO
    {
        public List<User> Users { get; set; } = new();  // список пользователей, по умолчанию пустой

        public List<Card> Cards { get; set; } = new();  // список карт, по умолчанию пустой

        public List<Test> Tests { get; set; } = new();  // список тестов, по умолчанию пустой

        // Следующий ID, который будет выдан новому пользователю (чтобы ID были уникальные)
        public int UserNextId { get; set; } = 1;

        // Следующий ID для новой карты
        public int CardNextId { get; set; } = 1;

        // Следующий ID для нового теста
        public int TestNextId { get; set; } = 1;
    }

    // Главный класс базы данных: хранит данные в списках и сохраняет/читает их из JSON файла
    public class DBfuncional
    {
        // Singleton: хранит единственный экземпляр DBfuncional (одна база на всё приложение)
        private static DBfuncional dBfuncional;

        // Полный путь к файлу базы в папке приложения: .../tarot_db.json
        // Path.Combine "склеивает" папки/имя файла правильно для ОС
        private static readonly string DbFile =
            Path.Combine(FileSystem.AppDataDirectory, "tarot_db.json");

        // Эти списки — "база в памяти" (пока приложение работает)
        private List<User> users = new();   // все пользователи
        private List<Card> cards = new();   // все карты
        private List<Test> tests = new();   // все тесты

        // Счётчики для автоматической выдачи новых уникальных ID
        private int userNextId = 1;  // следующий ID пользователя
        private int cardNextId = 1;  // следующий ID карты
        private int testNextId = 1;  // следующий ID теста

        // implicit operator — позволяет "превратить" DBfuncional в DBDTO автоматически:
        // например при сохранении: (DBDTO)this
        public static implicit operator DBDTO(DBfuncional db)
        {
            return new DBDTO
            {
                Users = db.users,           // копируем список пользователей из базы
                Cards = db.cards,           // копируем список карт
                Tests = db.tests,           // копируем список тестов
                UserNextId = db.userNextId, // копируем счётчик ID пользователей
                CardNextId = db.cardNextId, // копируем счётчик ID карт
                TestNextId = db.testNextId  // копируем счётчик ID тестов
            };
        }

        // FromDTO — обратная операция: загружаем данные из DTO обратно в DBfuncional
        public static void FromDTO(DBfuncional db, DBDTO dto)
        {
            // Если вдруг dto.Users == null, то ставим пустой список, чтобы не было ошибок
            db.users = dto.Users ?? new List<User>();

            db.cards = dto.Cards ?? new List<Card>();

            db.tests = dto.Tests ?? new List<Test>();

            // Восстанавливаем счётчики, чтобы новые объекты получали правильный следующий ID
            db.userNextId = dto.UserNextId;
            db.cardNextId = dto.CardNextId;
            db.testNextId = dto.TestNextId;
        }

        // Получение базы данных:
        // если экземпляр ещё не создан — создаём и загружаем данные из файла (или создаём начальную базу)
        public static async Task<DBfuncional> GetDB()
        {
            // Если базы ещё нет (первый вызов)
            if (dBfuncional == null)
            {
                dBfuncional = new DBfuncional(); // создаём пустую базу в памяти

                // Если файл базы существует — читаем из него
                if (File.Exists(DbFile))
                    await dBfuncional.ReadFile(); // загрузка данных из JSON
                else
                    await dBfuncional.InitDB();   // создаём стартовые данные (админ, карты)
            }

            return dBfuncional; // возвращаем тот же самый экземпляр базы
        }

        // Чтение базы из файла JSON
        private async Task ReadFile()
        {
            try
            {
                // читаем весь файл как текст (JSON строка)
                string json = await File.ReadAllTextAsync(DbFile);

                // пытаемся превратить JSON в объект DBDTO
                var dto = JsonSerializer.Deserialize<DBDTO>(json);

                // если получилось — переносим данные из dto в текущую базу
                if (dto != null)
                    FromDTO(this, dto);
            }
            catch
            {
                // если файл битый/ошибка чтения — просто молча пропускаем
                // (в таком виде это скрывает проблему, но приложение не упадёт)
            }
        }

        // Сохранение базы в файл JSON
        public async Task SaveFile()
        {
            try
            {
                // Записываем в файл:
                // 1) (DBDTO)this — превращаем текущую базу в DTO
                // 2) JsonSerializer.Serialize — преобразуем DTO в JSON строку
                // 3) WriteIndented = true — делает JSON красивым/читабельным
                await File.WriteAllTextAsync(
                    DbFile,
                    JsonSerializer.Serialize(
                        (DBDTO)this,
                        new JsonSerializerOptions { WriteIndented = true }
                    )
                );
            }
            catch (Exception ex)
            {
                // если сохранить не удалось — выводим причину в консоль
                Console.WriteLine("Ошибка сохранения: " + ex.Message);
            }
        }

        // ---------------------- ПОЛЬЗОВАТЕЛИ ----------------------

        // Обновить пользователя (заменяем поля в списке users и сохраняем файл)
        public async Task UpdateUser(User updated)
        {
            // ищем пользователя в списке по ID
            var user = users.FirstOrDefault(u => u.Id == updated.Id);

            // если нашли — обновляем поля
            if (user != null)
            {
                user.Name = updated.Name;         // новое имя
                user.Login = updated.Login;       // новый логин
                user.Password = updated.Password; // новый пароль
                user.IsAdmin = updated.IsAdmin;   // админ или нет
                user.Subscribe = updated.Subscribe; // подписка
                user.IdCard = updated.IdCard;     // какая-то привязка к карте (если используется)

                await SaveFile(); // сохраняем изменения в JSON файл
            }
        }

        // Удалить пользователя по ID
        public async Task RemoveUser(int id)
        {
            // удаляем всех пользователей, у кого совпал Id (обычно это 1 человек)
            users.RemoveAll(u => u.Id == id);

            await SaveFile(); // сохраняем изменения
        }

        // Получить пользователя по ID
        public async Task<User> GetUserById(int id)
        {
            await Task.Delay(1000); // искусственная задержка (имитация "долгой базы")
            return users.FirstOrDefault(u => u.Id == id); // ищем в списке и возвращаем (или null)
        }

        // Получить список всех пользователей
        public async Task<List<User>> GetUsers()
        {
            await Task.Delay(1000); // искусственная задержка
            return new List<User>(users); // возвращаем копию списка, чтобы снаружи не ломали оригинал
        }

        // Регистрация: создаём нового пользователя
        // ВАЖНО: у вас порядок параметров: (string name, string login, string password,...)
        public async Task<User> Register(
            string name,
            string login,
            string password,
            bool subscribe = false,
            bool isAdmin = false)
        {
            // если уже есть пользователь с таким логином — регистрацию не делаем
            if (users.Any(u => u.Login == login))
                return null;

            // создаём нового пользователя
            var user = new User
            {
                Id = userNextId++,   // берём текущий userNextId и увеличиваем на 1
                Login = login,       // сохраняем логин
                Password = password, // сохраняем пароль
                Name = name,         // сохраняем имя
                Subscribe = subscribe, // подписка
                IsAdmin = isAdmin    // роль админа
            };

            users.Add(user);    // добавляем в список пользователей в памяти
            await SaveFile();   // сохраняем в файл
            return user;        // возвращаем созданного пользователя
        }

        // Вход (проверка логина/пароля)
        public async Task<User> Authenticate(string login, string password)
        {
            await Task.Delay(10); // маленькая задержка (почти незаметная)
            // ищем пользователя, у которого совпали и логин, и пароль
            return users.FirstOrDefault(u => u.Login == login && u.Password == password);
        }

        // ---------------------- КАРТЫ ----------------------

        // Добавить карту
        public async Task AddCard(Card card)
        {
            card.Id = cardNextId++; // выдаём карте новый уникальный ID
            cards.Add(card);        // добавляем карту в список
            await SaveFile();       // сохраняем изменения
        }

        // Обновить карту
        public async Task UpdateCard(Card updated)
        {
            // ищем карту по ID
            var c = cards.FirstOrDefault(x => x.Id == updated.Id);

            // если нашли — меняем поля
            if (c != null)
            {
                c.Title = updated.Title;           // новое название
                c.Description = updated.Description; // новое описание
                c.Image = updated.Image;           // новое изображение (имя файла/путь)
                await SaveFile();                  // сохраняем
            }
        }

        // Удалить карту
        public async Task RemoveCard(int id)
        {
            cards.RemoveAll(c => c.Id == id);        // удаляем карту из списка
            tests.RemoveAll(t => t.IdCard == id);    // удаляем все тесты, которые относятся к этой карте
            await SaveFile();                        // сохраняем изменения
        }

        // Получить карту по ID
        public async Task<Card> GetCardById(int id)
            => cards.FirstOrDefault(c => c.Id == id); // просто ищем в списке

        // Получить все карты
        public async Task<List<Card>> GetCards()
            => new List<Card>(cards); // возвращаем копию списка карт

        // ---------------------- ТЕСТЫ ----------------------

        // Добавить тест
        public async Task AddTest(Test test)
        {
            test.Id = testNextId++; // выдаём уникальный ID тесту
            tests.Add(test);        // добавляем тест в список
            await SaveFile();       // сохраняем
        }

        // Обновить тест
        public async Task UpdateTest(Test updated)
        {
            // ищем тест по ID
            var t = tests.FirstOrDefault(x => x.Id == updated.Id);

            // если нашли — обновляем поля
            if (t != null)
            {
                t.IdUser = updated.IdUser;         // какой пользователь проходил
                t.IdCard = updated.IdCard;         // по какой карте
                t.Score = updated.Score;           // результат/баллы
                t.Progress = updated.Progress;     // прогресс в процентах или как задано
                t.CompletedAt = updated.CompletedAt; // дата завершения
                await SaveFile();                  // сохраняем
            }
        }

        // Удалить тест по ID
        public async Task RemoveTest(int id)
        {
            tests.RemoveAll(t => t.Id == id); // удаляем тест из списка
            await SaveFile();                 // сохраняем
        }

        // Получить тест по ID
        public async Task<Test> GetTestById(int id)
            => tests.FirstOrDefault(t => t.Id == id); // ищем в списке

        // Получить все тесты
        public async Task<List<Test>> GetTests()
            => new List<Test>(tests); // возвращаем копию списка

        // ---------------------- ИНИЦИАЛИЗАЦИЯ БАЗЫ ----------------------

        // Создание стартовых данных (когда файла ещё нет)
        public async Task InitDB()
        {
            // Если нет ни одного админа — создаём админа
            if (!users.Any(u => u.IsAdmin))
            {
                // ВНИМАНИЕ: тут у вас вызов Register("Администратор", "admin", "admin", true, true)
                // Это соответствует сигнатуре Register(name, login, password, subscribe, isAdmin)
                await Register("Администратор", "admin", "admin", true, true);
            }

            // Если нет ни одного обычного пользователя — создаём обычного
            if (!users.Any(u => !u.IsAdmin))
            {
                await Register("Пользователь", "user", "user");
            }

            // Если карт ещё нет — добавляем стартовый набор
            if (!cards.Any())
            {
                var starterCards = new List<Card>
                {
                    // создаём карту "Шут"
                    new Card
                    {
                        Title = "Шут",
                        Description = "Новые начинания, свобода, приключения",
                        Image = "fool.png"
                    },

                    // создаём карту "Маг"
                    new Card
                    {
                        Title = "Маг",
                        Description = "Воля, способности, энергия, действие",
                        Image = "mag.png"
                    },

                    // создаём карту "Жрица"
                    new Card
                    {
                        Title = "Жрица",
                        Description = "Интуиция, тайна, знание, внутренняя сила",
                        Image = "highpriestess.png"
                    },
                };

                // добавляем каждую карту в базу через AddCard (там присваивается ID и идёт SaveFile)
                foreach (var card in starterCards)
                    await AddCard(card);
            }

            await SaveFile(); // финально сохраняем всю базу (на всякий случай)
        }
    }
}
