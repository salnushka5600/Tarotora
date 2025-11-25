using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Tarotora.BD
{
    public class DBfuncional
    {
        private static readonly string DbFile = Path.Combine(FileSystem.AppDataDirectory, "tarot_db.json");

        private static DBfuncional _instance;
        public static DBfuncional GetDB
        {
            get
            {
                if (_instance == null)
                {
                    _instance = ReadFile();
                    _instance.SeedCards();
                    _instance.SeedUsers();
                }
                return _instance;
            }
        }
        public List<User> users { get; set; } = new();
        public List<Card> cards { get; set; } = new();
        public List<Test> tests { get; set; } = new();

        public int userAutoIncrement = 0;
        public int cardAutoIncrement = 0;
        public int testAutoIncrement = 0;

        public int CurrentUserId { get; private set; } = 0;

        public static void SavetoFile(DBfuncional db)
        {
            try
            {
                var dto = new DTOobject
                {
                    users = db.users,
                    cards = db.cards,
                    tests = db.tests,
                    userAutoIncrement = db.userAutoIncrement,
                    cardAutoIncrement = db.cardAutoIncrement,
                    testAutoIncrement = db.testAutoIncrement,
                    currentUserId = db.CurrentUserId
                };

                var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(DbFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Save error: " + ex.Message);
            }
        }

        public static DBfuncional ReadFile()
        {
            try
            {
                if (!File.Exists(DbFile))
                {
                    var fresh = new DBfuncional();
                    SavetoFile(fresh);
                    return fresh;
                }

                var json = File.ReadAllText(DbFile);
                var dto = JsonSerializer.Deserialize<DTOobject>(json) ?? new DTOobject();

                return new DBfuncional
                {
                    users = dto.users ?? new List<User>(),
                    cards = dto.cards ?? new List<Card>(),
                    tests = dto.tests ?? new List<Test>(),
                    userAutoIncrement = dto.userAutoIncrement == 0 ? 1 : dto.userAutoIncrement,
                    cardAutoIncrement = dto.cardAutoIncrement == 0 ? 1 : dto.cardAutoIncrement,
                    testAutoIncrement = dto.testAutoIncrement == 0 ? 1 : dto.testAutoIncrement,
                    CurrentUserId = dto.currentUserId
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Load error: " + ex.Message);
                return new DBfuncional();
            }
        }
        public async Task AddUser(User user)
        {
            new User();
            if (user.Id == 0) user.Id = userAutoIncrement++;

            users.Add(user);
            SavetoFile(this);
        }

        public async Task UpdateUser(User updated)
        {
            var user = users.FirstOrDefault(c => c.Id == updated.Id);
            if (user != null)
            {
                user.Subscribe = updated.Subscribe;
                user.Name = updated.Name;
                user.IdCard = updated.IdCard;
                user.Card = updated.Card;
            }
            SavetoFile(this);
        }

        public async Task RemoveUser(int id)
        {
            var u = users.FirstOrDefault(x => x.Id == id);
            if (u != null)
            {
                users.Remove(u);
                tests.RemoveAll(t => t.IdUser == id);
                SavetoFile(this);
            }
        }

        public async Task<List<User>> GetUser() => users;
        public async Task GetUserId(int id) => users.FirstOrDefault(c => c.Id == id);


        //для карт



        public async Task AddCard(Card card)
        {
            new Card();
            if (card.Id == 0) card.Id = cardAutoIncrement++;
            cards.Add(card);
            SavetoFile(this);
        }

        public async Task UpdateCard(Card updated)
        {
            var card = cards.FirstOrDefault(c => c.Id == updated.Id);
            if (card != null)
            {
                card.Title = updated.Title;
                card.Description = updated.Description;
                card.Subscribe = updated.Subscribe;
                card.Image = updated.Image;
            }
            SavetoFile(this);
        }

        public async Task RemoveCard(int id)
        {
            var c = cards.FirstOrDefault(x => x.Id == id);
            if (c != null)
            {
                cards.Remove(c);
                tests.RemoveAll(t => t.IdCard == id);
            }
            SavetoFile(this);
        }
        public async Task<List<Card>> GetCard() => cards;
        public async Task GetCardId(int id) => cards.FirstOrDefault(c => c.Id == id);


        public async Task AddTest(Test test)
        {
            if (test.Id == 0) test.Id = testAutoIncrement++;
            tests.Add(test);
            SavetoFile(this);
        }

        public async Task UpdateTest(Test updated)
        {
            var test = tests.FirstOrDefault(c => c.Id == updated.Id);
            if (test != null)
            {
                test.Score = updated.Score;
                test.IdUser = updated.IdUser;
                test.Users = updated.Users;
                test.IdCard = updated.IdCard;
                test.Cards = updated.Cards;
                test.Progress = updated.Progress;
            }
            SavetoFile(this);
        }

        public async Task RemoveTest(int id) => tests.RemoveAll(c => c.Id == id);
        public async Task<List<Test>> GetTest() => tests;
        public async Task GetTestId(int id) => tests.FirstOrDefault(c => c.Id == id);


        public async Task SeedCards()
        {
            cards.Add(new Card { Id = 1, Title = "Шут", Description = "Новые возможности", Image = "fool.png" });
            cards.Add(new Card { Id = 2, Title = "Император", Description = "Лидерство и сила", Image = "emperor.png" });
            cards.Add(new Card { Id = 3, Title = "Императрица", Description = "Забота и плодородие", Image = "empress.png" });
        }

        public async Task<int> GetUserProgress(int userId)
        {
            var allCards = await GetCard();
            var allTests = await GetTest();

            // Считаем, сколько карт прошёл пользователь
            int viewedCount = allTests.Count(t => t.IdUser == userId);

            if (allCards.Count == 0)
                return 0;

            int progress = (int)((double)viewedCount / allCards.Count * 100);
            return progress;
        }

        public async Task SeedUsers()
        {
            if (users.Count == 0)
            {
                users.Add(new User { Id = 1, Name = "Алиса", Subscribe = true });
                users.Add(new User { Id = 2, Name = "Боб", Subscribe = false });
                users.Add(new User { Id = 3, Name = "Кира", Subscribe = true });
            }

            if (tests.Count == 0)
            {
                tests.Add(new Test { Id = 1, IdUser = 1, IdCard = 1, Progress = 30 });
                tests.Add(new Test { Id = 2, IdUser = 2, IdCard = 1, Progress = 70 });
                tests.Add(new Test { Id = 3, IdUser = 3, IdCard = 1, Progress = 90 });
            }
        }
    }
}
