using Dropbox;
using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Gabriel.Cat.Nube
{
    public class DropBox : Nube
    {
        Dropbox.Api.DropboxClient client;
        public DropBox(string apiKey)
        {
            DropboxClientConfig config;
            Application app;
            LoginForm login;
            HttpClient httpClient;
            try
            {
                app = new Application();
                login = new LoginForm(apiKey);
                app.Run(login);
                if (login.Result)
                {
                    httpClient = new HttpClient(new WebRequestHandler { ReadWriteTimeout = 10 * 1000 })
                    {
                        // Specify request level timeout which decides maximum time that can be spent on
                        // download/upload files.
                        Timeout = TimeSpan.FromMinutes(20)
                    };


                    config = new DropboxClientConfig()
                    {
                        HttpClient = httpClient
                    };

                    client = new Dropbox.Api.DropboxClient(login.AccessToken, config);


                }
                else
                {
                    throw new NubeException("No ha hecho login");
                }
            }
            catch (Exception ex)
            {
                throw new NubeException(ex.Message);
            }
        }

        public DropboxClient Client { get {return client; } }

        public override Stream Download(string path, string fileName)
        {
            Task<Stream> taskStream;
            Task<Dropbox.Api.Stone.IDownloadResponse<FileMetadata>> taskDownload= client.Files.DownloadAsync(System.IO.Path.Combine(path, fileName));

            taskDownload.RunSynchronously();
            taskStream = taskDownload.Result.GetContentAsStreamAsync();
            taskStream.RunSynchronously();

            return taskStream.Result;
        }

        public override bool CreateFolder(string path, string nameFolder)
        {
            if (ShowDebugrMessages || System.Diagnostics.Debugger.IsAttached)
                Console.WriteLine("--- Creating Folder ---");

            CreateFolderArg folderArg = new CreateFolderArg(path);
            Task<CreateFolderResult> tskFolder =  client.Files.CreateFolderV2Async(folderArg);
            CreateFolderResult folder;
            bool result;
            try
            {
                tskFolder.RunSynchronously();
                folder = tskFolder.Result;
               
                if (ShowDebugrMessages || System.Diagnostics.Debugger.IsAttached)
                    Console.WriteLine("Folder: " + path + " created!");
                result = true;
            }
            catch { result = false; }
            return result;
        }

        public override bool DeleteFolder(string path, string nameFolder)
        {
            throw new NotImplementedException();
        }


        public override bool ExistFolder(string path, string nameFolder)
        {
            throw new NotImplementedException();
        }

        public override void Upload(string path, string fileName, Stream strDatos)
        {
            const int chunkSize = 128 * 1024;

            int byteRead;
            int numChunks = (int)Math.Ceiling((double)strDatos.Length / chunkSize);
            UploadSessionStartResult result;
            byte[] buffer = new byte[chunkSize];
            string sessionId = null;
            UploadSessionCursor cursor;
            MemoryStream memStream;
            Task<UploadSessionStartResult> asyncTaskUploadResult;

            byteRead = strDatos.Read(buffer, 0, chunkSize);
            memStream = new MemoryStream(buffer, 0, byteRead);
            asyncTaskUploadResult = client.Files.UploadSessionStartAsync(body: memStream);
            asyncTaskUploadResult.RunSynchronously();
            result = asyncTaskUploadResult.Result;
            memStream.Close();

            sessionId = result.SessionId;
            if (ShowDebugrMessages || System.Diagnostics.Debugger.IsAttached)
                Console.WriteLine("Start Upload file {0} at folder {1}", fileName, path);

            for (int idx = 1; idx < numChunks; idx++)
            {
                if (ShowDebugrMessages || System.Diagnostics.Debugger.IsAttached)
                    Console.WriteLine("Start uploading chunk {0}", idx);
                byteRead = strDatos.Read(buffer, 0, chunkSize);

                memStream = new MemoryStream(buffer, 0, byteRead);


                cursor = new UploadSessionCursor(sessionId, (ulong)(chunkSize * idx));

                if (idx == numChunks - 1)
                {
                    client.Files.UploadSessionFinishAsync(cursor, new CommitInfo(path + "/" + fileName), memStream).RunSynchronously();
                }

                else
                {
                    client.Files.UploadSessionAppendV2Async(cursor, body: memStream).RunSynchronously();
                }
                memStream.Close();

            }
        }

        public override IList<IElementoNube> GetElements(string path,bool recursive)
        {
            ListFolderResult list;
            Task<ListFolderResult> taskList =  client.Files.ListFolderAsync(path,recursive);
            taskList.RunSynchronously();
            list = taskList.Result;
            return list.Entries.Cast<IElementoNube>().ToArray(); 
        }

       
    }
}
