﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gallifrey.Exceptions.JiraTimers
{
    public class DuplicateTimerException : Exception
    {
        public DuplicateTimerException(string message)
            : base(message)
        {

        }
    }
}
