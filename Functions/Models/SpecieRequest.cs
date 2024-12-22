using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Durable.Functions.Models
{
    class SpecieRequest : BaseRequest
    {
       public required string Specie { get; set; }
    }
}
