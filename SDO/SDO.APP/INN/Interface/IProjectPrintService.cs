using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IInnProjectPrintService
    {
        Task<InnProjectBasicFillModel> GetProjectPrint(string PROJECT_NO, string type);
    }
}
