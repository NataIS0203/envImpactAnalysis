using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Durable.Functions.Models
{
    class ResourceRequest : BaseRequest
    {
       public required string Resource { get; set; }
    }
}
