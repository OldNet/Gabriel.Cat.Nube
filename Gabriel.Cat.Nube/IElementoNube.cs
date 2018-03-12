using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat.Nube
{
   public interface IElementoNube
    {
        bool IsAFolder { get; }
        string Name { get; }
        string PathNube { get; }
    }
}
