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
        public bool Subscribe { get; set; }
        public int IdCard {  get; set; }
        public Card Card { get; set; }
    }
}
