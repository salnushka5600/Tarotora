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
        public string CategPicker {  get; set; }

    }
}
