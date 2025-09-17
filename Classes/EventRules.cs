using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class Event_Rules
    {
        public string Name {  get; set; }
        public string message { get; set; }
        public string type { get; set; }

        public Event_Rules( string name,string message,string type) 
        {
            this.Name = name;
            this.message = message;
            this.type = type;
        }
        
    }
    
    
}
