using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DREXCreateFunctionForTrussLink.Data
{
    public class UserModel
    {
        public int UserId { get; set; }
        public string Token { get; set; }
        public DateTime Limit { get; set; }

        public long Id { get; set; }
    }
}