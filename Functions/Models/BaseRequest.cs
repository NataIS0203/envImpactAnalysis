using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Durable.Functions.Models
{
    class BaseRequest
    {
       public string? Region { get; set; }

       public int? Percentage { get; set; }
    }
}
