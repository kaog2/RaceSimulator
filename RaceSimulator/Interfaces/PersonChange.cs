﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceSimulator.Interfaces
{
    public class PersonChange : IPersonChange
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged( string propertyName )
        {
            var handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

