using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat.Nube
{
   public class NubeException:Exception
    {
        public NubeException(string message) : base(message) { }
    }
}
