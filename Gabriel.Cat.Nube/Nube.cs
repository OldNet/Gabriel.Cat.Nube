using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat.Nube
{
    public abstract class Nube : INube
    {
        public static bool ShowDebugrMessages = true;

        public abstract bool CreateFolder(string path, string nameFolder);

        public abstract bool DeleteFolder(string path, string nameFolder);
        public abstract bool ExistFolder(string path, string nameFolder);
        public abstract void Upload(string path, string fileName, Stream strDatos);
        public  void Upload(string path, string fileName, Stream strDatos, bool closeStream)
        {
            Upload(path, fileName, strDatos);
            if (closeStream)
                strDatos.Close();
        }
        public void Upload(string path, string fileName, byte[] datosArchivo)
        {
            Upload(path, fileName, new MemoryStream(datosArchivo));
        }
        public void UploadFolder(string path, DirectoryInfo dir)
        {
            
            DirectoryInfo[] subDirs;
            FileInfo[] files;
            string pathDir = System.IO.Path.Combine(path, dir.Name);

            if (!ExistFolder(path, dir.Name))
            {
                CreateFolder(path, dir.Name);
            }

            subDirs = dir.GetDirectories();
            files = dir.GetFiles();

            Upload(path, dir.Name, files);

            for (int i = 0; i < subDirs.Length; i++)
                UploadFolder(pathDir, subDirs[i]);
        }

        public void Upload(string path, string folderName,params FileInfo[] files)
        {
            string pathFolder = System.IO.Path.Combine(path, folderName);
            for (int i = 0; i < files.Length; i++)
                Upload(pathFolder, files[i].Name, files[i].OpenRead());
        }

        public abstract Stream Download(string path, string fileName);
        public abstract IList<IElementoNube> GetElements(string path,bool recursive);
        public  IList<IElementoNube> GetElements(string path)
        {
            return GetElements(path, false);
        }
    }
}
