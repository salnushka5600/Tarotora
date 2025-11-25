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
        public string Name { get; set; }         
        public string Login { get; set; }        
        public string Password { get; set; }    
        public bool IsAdmin { get; set; }        
        public bool Subscribe { get; set; }      
        public int IdCard { get; set; }          
        public Card Card { get; set; }

        
        private static User user;

       

        public static User GetUser()
        {
            return user; 
        }
        
        
        public static void PostUser(User userr)
        {
            user = userr; 
        }
    }
}
