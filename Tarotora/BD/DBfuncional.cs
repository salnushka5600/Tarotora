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
        
        public List<User> Users { get; set; } = new();

       
        public List<Card> Cards { get; set; } = new();

        
        public List<Test> Tests { get; set; } = new();

        
        public int UserNextId { get; set; } = 1;
        public int CardNextId { get; set; } = 1;
        public int TestNextId { get; set; } = 1;
    }

    
    public class DBfuncional
    {
        private static DBfuncional dBfuncional;

        private static readonly string DbFile = Path.Combine(FileSystem.Current.AppDataDirectory, "tarot_db.json"); 

        
        private List<User> users = new();
        private List<Card> cards = new();
        private List<Test> tests = new();

        private int userNextId = 1;
        private int cardNextId = 1;
        private int testNextId = 1;

        public static implicit operator DBDTO(DBfuncional db)
        {
            return new DBDTO
            {
                Users = db.users,
                Cards = db.cards,
                Tests = db.tests,
                UserNextId = db.userNextId,
                CardNextId = db.cardNextId,
                TestNextId = db.testNextId
            };
        }

        public static void FromDTO(DBfuncional db, DBDTO dto)
        {
            db.users = dto.Users ?? new List<User>();
            db.cards = dto.Cards ?? new List<Card>();
            db.tests = dto.Tests ?? new List<Test>();

            db.userNextId = dto.UserNextId;
            db.cardNextId = dto.CardNextId;
            db.testNextId = dto.TestNextId;
        }

        public static async Task<DBfuncional> GetDB()
        {
            if (dBfuncional == null)
            {
                dBfuncional = new DBfuncional();
                if (File.Exists(DbFile))
                    await dBfuncional.ReadFile(); 
                else
                    await dBfuncional.InitDB();  
            }
            return dBfuncional;
        }

        private async Task ReadFile()
        {
            try
            {
                string json = await File.ReadAllTextAsync(DbFile);
                var dto = JsonSerializer.Deserialize<DBDTO>(json);
                if (dto != null)
                    FromDTO(this, dto);
            }
            catch
            {
                
            }
        }

        
        public async Task SaveFile()
        {
            try
            {
                await File.WriteAllTextAsync(
                    DbFile,
                    JsonSerializer.Serialize((DBDTO)this, new JsonSerializerOptions { WriteIndented = true })
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка сохранения: " + ex.Message);
            }
        }

         

        public async Task UpdateUser(User updated)
        {
            var user = users.FirstOrDefault(u => u.Id == updated.Id);
            if (user != null)
            {
                user.Name = updated.Name;
                user.Login = updated.Login;
                user.Password = updated.Password;
                user.IsAdmin = updated.IsAdmin;
                user.Subscribe = updated.Subscribe;
                user.IdCard = updated.IdCard;
                await SaveFile();
            }
        }

        
        public async Task RemoveUser(int id)
        {
            users.RemoveAll(u => u.Id == id);
            await SaveFile();
        }

        
        public async Task<User> GetUserById(int id)
        {
            await Task.Delay(1000); 
            return users.FirstOrDefault(u => u.Id == id);
        }

        
        public async Task<List<User>> GetUsers()
        {
            await Task.Delay(1000);
            return new List<User>(users);
        }

        
        public async Task<User> Register(string name, string login, string password, bool subscribe = false, bool isAdmin = false)
        {
            if (users.Any(u => u.Login == login))
                return null; 

            var user = new User
            {
                Id = userNextId++,
                Login = login,
                Password = password,
                Name = name,
                Subscribe = subscribe,
                IsAdmin = isAdmin
            };

            users.Add(user);
            await SaveFile();
            return user;
        }

        public async Task<User> Authenticate(string login, string password)
        {
            await Task.Delay(10);
            return users.FirstOrDefault(u => u.Login == login && u.Password == password);
        }

         

      
        public async Task AddCard(Card card)
        {
            card.Id = cardNextId++;
            cards.Add(card);
            await SaveFile();
        }

        
        public async Task UpdateCard(Card updated)
        {
            var c = cards.FirstOrDefault(x => x.Id == updated.Id);
            if (c != null)
            {
                c.Title = updated.Title;
                c.Description = updated.Description;
                c.Image = updated.Image;
                await SaveFile();
            }
        }

        public async Task RemoveCard(int id)
        {
            cards.RemoveAll(c => c.Id == id);
            tests.RemoveAll(t => t.IdCard == id);
            await SaveFile();
        }

      
        public async Task<Card> GetCardById(int id) => cards.FirstOrDefault(c => c.Id == id);

        
        public async Task<List<Card>> GetCards() => new List<Card>(cards);

         

       
        public async Task AddTest(Test test)
        {
            test.Id = testNextId++;
            tests.Add(test);
            await SaveFile();
        }

        
        public async Task UpdateTest(Test updated)
        {
            var t = tests.FirstOrDefault(x => x.Id == updated.Id);
            if (t != null)
            {
                t.IdUser = updated.IdUser;
                t.IdCard = updated.IdCard;
                t.Score = updated.Score;
                t.Progress = updated.Progress;
                t.CompletedAt = updated.CompletedAt;
                await SaveFile();
            }
        }

        public async Task RemoveTest(int id)
        {
            tests.RemoveAll(t => t.Id == id);
            await SaveFile();
        }

        public async Task<Test> GetTestById(int id) => tests.FirstOrDefault(t => t.Id == id);

        public async Task<List<Test>> GetTests() => new List<Test>(tests);

        public async Task InitDB()
        {
            if (!users.Any(u => u.IsAdmin))
            {
                await Register("Администратор", "admin", "admin", true, true);
            }

            if (!users.Any(u => !u.IsAdmin))
            {
                await Register("Пользователь", "user", "user");
            }

            if (!cards.Any())
            {
                var starterCards = new List<Card>
                {
                    new Card { Title = "Шут", Description = "Новые начинания, свобода, приключения", Image = "fool.png" },
                    new Card { Title = "Маг", Description = "Воля, способности, энергия, действие", Image = "mag.png" },
                    new Card { Title = "Жрица", Description = "Интуиция, тайна, знание, внутренняя сила", Image = "highpriestess.png" },
                };

                foreach (var card in starterCards)
                    await AddCard(card);
            }

            await SaveFile(); 
        }
    }
}
