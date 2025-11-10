using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tarotora.BD
{
    public class DTOobject
    {
       public List<User> users { get; set; }
        public List<Card> cards {  get; set; }
        public List<Test> tests {  get; set; }
    }
}
