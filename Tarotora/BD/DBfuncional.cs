using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tarotora.BD
{
   public class DBfuncional
    {
        List<User> users = new();
        List<Card> cards = new();
        List<Test> tests = new();

        //для пользователей
        public async Task AddUser(User user) => users.Add(user);

        public async Task UpdateUser(User updated)
        {
            var user = users.FirstOrDefault(c => c.Id == updated.Id);
            if (user != null)
            {
                user.Subscribe = updated.Subscribe;
                user.IdCard = updated.IdCard;
                user.Card = updated.Card;
            }

        }

        public async Task RemoveUser(int id) => users.RemoveAll(c => c.Id == id);
        public async Task<List<User>> GetUser() => users;
        public async Task GetUserId (int id) => users.FirstOrDefault(c => c.Id == id);


        //для карт



        public async Task AddCard(Card card) => cards.Add(card);

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

        }

        public async Task RemoveCard(int id) => cards.RemoveAll(c => c.Id == id);
        public async Task<List<Card>> GetCard() => cards;
        public async Task GetCardId(int id) => cards.FirstOrDefault(c => c.Id == id);

        //public async  Task SaveFile()
        //{
        //    try
        //    {
        //        object value = await File.WriteAllTextAsync(FileSystem.Current.AppDataDirectory + "/Prosmotrkolod"); 

        //    }
        //    catch (Exception ex) { }


        //для проверки



        public async Task AddTest(Test test) => tests.Add(test);

        public async Task UpdateTest(Test updated)
        {
            var test = tests.FirstOrDefault(c => c.Id == updated.Id);
            if (test != null)
            {
                test.Score = updated.Score;
                test.IdUser = updated.IdUser;
                test.User = updated.User;
                test.IdCard = updated.IdCard;
                test.Card = updated.Card;
                test.Progress = updated.Progress;
            }

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

    }
}
