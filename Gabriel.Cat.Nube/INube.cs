using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Gabriel.Cat.Nube
{
   public interface INube
    {
        byte[] Download(string path, string fileName);
        void Upload(string path, string fileName, Stream strDatos);
        bool ExistFolder(string path, string nameFolder);
        bool CreateFolder(string path, string nameFolder);
        bool DeleteFolder(string path, string nameFolder);
        IList<IElementoNube> GetElements(string path, bool recursive);
        string GetTemporalPath(IElementoNube elemento);
        string GetIdShareFolder(IElementoNube elemento);
        bool[] Share(IElementoNube elemento,bool notify = true, string message = null, SharingLevel level = SharingLevel.ViewerNoComment, params string[] emailsUsersToShare);
    }
}
