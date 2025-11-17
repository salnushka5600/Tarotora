using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tarotora.BD
{
   public class Card
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Subscribe { get; set; }
        public string Image { get; set; }

        public int Progress { get; set; } = 0; // Progress — это коробочка для числа, которая показывает, насколько что-то выполнено.  В начале там 0, потом можно положить любое другое число.


    }
}
