using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceSimulator.Models
{
    public class Competitor
    {
        private int _bib;

        private Person _person;

        public int Bib 
        { 
            get { return _bib; }
            set { _bib = value; }
        }
        public Person Person 
        {
            get { return _person; }
            set { _person = value; } 
        }

        public Competitor()
        {

        }

        public Competitor(Person person, int bib)
        {
            Bib = bib;
            Person = person;
        }
    }
}
