using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;   // для FileSystem.AppDataDirectory

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
                    _instance = ReadFile();
                return _instance;
            }
        }

        // ===== ДАННЫЕ =====
        public List<User> users { get; set; } = new();
        public List<Card> cards { get; set; } = new();
        public List<Test> tests { get; set; } = new();

        public int userAutoIncrement = 1;
        public int cardAutoIncrement = 1;
        public int testAutoIncrement = 1;

        public int CurrentUserId { get; private set; } = 0;

        // ===== СЕРИАЛИЗАЦИЯ =====
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

        // ===== USERS =====
        public async Task<User> RegisterUser(string name, bool subscribe = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя не может быть пустым.");

            var user = new User
            {
                Id = userAutoIncrement++,
                Name = name.Trim(),
                Subscribe = subscribe
            };
            users.Add(user);
            SavetoFile(this);
            return user;
        }

        public async Task SetCurrentUser(int userId)
        {
            if (!users.Any(u => u.Id == userId))
                throw new InvalidOperationException("Пользователь не найден.");
            CurrentUserId = userId;
            SavetoFile(this);
        }

        public async Task<User> GetCurrentUser() => users.FirstOrDefault(u => u.Id == CurrentUserId);
        public async Task<List<User>> GetUser() => users.ToList();

        public async Task AddUser(User user)
        {
            if (user.Id == 0) user.Id = userAutoIncrement++;
            users.Add(user);
            SavetoFile(this);
        }

        public async Task DeleteUser(int id)
        {
            var u = users.FirstOrDefault(x => x.Id == id);
            if (u != null)
            {
                users.Remove(u);
                tests.RemoveAll(t => t.IdUser == id);
                SavetoFile(this);
            }
        }

        // ===== CARDS =====
        public async Task AddCard(Card card)
        {
            if (card.Id == 0) card.Id = cardAutoIncrement++;
            cards.Add(card);
            SavetoFile(this);
        }

        public async Task<List<Card>> GetAllCards() => cards.ToList();

        // Совместимость: старый код вызывает GetCard(...)
        public async Task<List<Card>> GetCard(string search = null)
        {
            if (string.IsNullOrWhiteSpace(search))
                return cards.ToList();

            search = search.Trim();
            return cards
                .Where(c =>
                    (c.Title?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }

        public async Task<Card> GetCardById(int id) => cards.FirstOrDefault(c => c.Id == id);

        // Совместимость: старый код вызывает UpdateCard(...)
        public async Task UpdateCard(Card updated)
        {
            if (updated == null) return;
            var c = cards.FirstOrDefault(x => x.Id == updated.Id);
            if (c == null) return;

            c.Title = updated.Title;
            c.Description = updated.Description;
            c.Image = updated.Image;
            SavetoFile(this);
        }

        // Совместимость: старый код вызывает RemoveCard(...)
        public async Task RemoveCard(int id)
        {
            var c = cards.FirstOrDefault(x => x.Id == id);
            if (c != null)
            {
                cards.Remove(c);
                tests.RemoveAll(t => t.IdCard == id);
                SavetoFile(this);
            }
        }

        // ===== TESTS / ПРОЙДЕННЫЕ КАРТЫ =====
        public async Task AddTest(Test test)
        {
            if (test.Id == 0) test.Id = testAutoIncrement++;
            tests.Add(test);
            SavetoFile(this);
        }

        public async Task<List<Test>> GetTest() => tests.ToList();

        public async Task AddCompletedCard(int userId, int cardId, int score = 0, int progress = 100)
        {
            if (!users.Any(u => u.Id == userId))
                throw new InvalidOperationException("Пользователь не найден");
            if (!cards.Any(c => c.Id == cardId))
                throw new InvalidOperationException("Карта не найдена");

            // не дублируем одну и ту же карту (уберите это, если нужна многократная фиксация)
            if (tests.Any(t => t.IdUser == userId && t.IdCard == cardId))
                return;

            await AddTest(new Test
            {
                IdUser = userId,
                IdCard = cardId,
                Score = score,
                Progress = progress
            });
        }

        public async Task<List<Card>> GetCompletedCardsByUser(int userId)
        {
            var cardIds = tests.Where(t => t.IdUser == userId).Select(t => t.IdCard).Distinct().ToList();
            return cards.Where(c => cardIds.Contains(c.Id)).ToList();
        }
    }
}