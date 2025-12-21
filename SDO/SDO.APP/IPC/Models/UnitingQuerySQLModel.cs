using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class UnitingQuerySQLModel
    {
        public StringBuilder SelectColumn { get; set; } = new StringBuilder();
        public StringBuilder JoinTable { get; set; } = new StringBuilder();
        public StringBuilder WhereSql { get; set; } = new StringBuilder();
        public bool HaveVW2 { get; set; } = false;
        public bool HaveM8 { get; set; } = false;
    }
}
