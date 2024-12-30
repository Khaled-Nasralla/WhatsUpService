using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatsUpService.Core.Outils
{
    public class PathDb
    {


        public static string Getpath(string dbPath)
        {
            return $"Data Source={dbPath}";
        }
    }
}
