using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tarotora.BD
{
   public class Test
    {
        public int Id { get; set; }
        public int Score { get; set; }
        public int IdUser { get; set; }
        public User User { get; set; }
        public int IdCard { get; set; }
        public Card Card { get; set; }
        public int Progress { get; set; }

    }
}
