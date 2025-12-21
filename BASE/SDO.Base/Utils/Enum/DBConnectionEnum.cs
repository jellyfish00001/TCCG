using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Base.Utils
{
    public enum DBConnectionEnum : int
    {
        defaultKey = 0,
        SCDBKey = 1,
        RISDBKey = 2,
        IPCDBKey = 3,
        INNDBKey = 4,
        PWSDBKey = 5,
        RDDBKey = 6,
        MainDBKey = 7
    }
}
