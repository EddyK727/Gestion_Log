using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace Recuperation_fichier
{

    class Loueur
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public int Ko { get; set; }
        public int Timeout { get; set; }

        public Loueur(string name, int id, int ko, int timeout)
        {
            Name = name;
            Id = id;
            Ko = ko;
            Timeout = timeout;
        }

    }

}
