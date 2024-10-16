using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatProgram.Models
{
    public class User
    {
        public User(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
