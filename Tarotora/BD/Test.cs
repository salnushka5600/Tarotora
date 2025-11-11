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
        public User Users { get; set; }
        public int IdCard { get; set; }
        public Card Cards { get; set; }
        public int Progress { get; set; }

        public DateTime CompletedAt { get; set; } = DateTime.Now;

    }
}
