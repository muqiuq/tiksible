﻿using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Handler
{
    public interface IHandler
    {
        public Command GetCommand();

    }
}
