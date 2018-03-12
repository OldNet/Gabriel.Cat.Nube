using Dropbox;
using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Sharing;
using Gabriel.Cat.Extension;
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
        public DropBox(string loginAccessToken)
        {
            DropboxClientConfig config;
            HttpClient httpClient;
            Dropbox.Api.Users.FullAccount userDropoBox;

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
            AccessToken = loginAccessToken;
            client = new Dropbox.Api.DropboxClient(AccessToken, config);
            userDropoBox = client.Users.GetCurrentAccountAsync().Result;


            User = new User(userDropoBox.Name.DisplayName, userDropoBox.Locale, userDropoBox.Email, userDropoBox.ProfilePhotoUrl, userDropoBox.EmailVerified);
        }


        public DropboxClient Client { get { return client; } }

        public override byte[] Download(string path, string fileName)
        {
            Task<byte[]> taskStream;
            Task<Dropbox.Api.Stone.IDownloadResponse<FileMetadata>> taskDownload = client.Files.DownloadAsync(System.IO.Path.Combine(path, fileName).Replace('\\', '/'));

            taskStream = taskDownload.Result.GetContentAsByteArrayAsync();


            return taskStream.Result;
        }

        public override bool CreateFolder(string path, string nameFolder)
        {
            if (ShowDebugrMessages || System.Diagnostics.Debugger.IsAttached)
                Console.WriteLine("--- Creating Folder ---");

            CreateFolderArg folderArg = new CreateFolderArg(System.IO.Path.Combine(path, nameFolder).Replace('\\', '/'));
            Task<CreateFolderResult> tskFolder = client.Files.CreateFolderV2Async(folderArg);
            CreateFolderResult folder;
            bool result;
            try
            {

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
            bool deleted;
            try
            {
                deleted = client.Files.DeleteV2Async(System.IO.Path.Combine(path, nameFolder).Replace('\\', '/')).Result.Metadata.IsDeleted;
            }
            catch { deleted = false; }
            return deleted;
        }


        public override bool ExistFolder(string path, string nameFolder)
        {

            return false;
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
            FileMetadata file = null;
            byteRead = strDatos.Read(buffer, 0, chunkSize);
            memStream = new MemoryStream(buffer, 0, byteRead);
            asyncTaskUploadResult = client.Files.UploadSessionStartAsync(body: memStream);

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

                    file = client.Files.UploadSessionFinishAsync(cursor, new CommitInfo(System.IO.Path.Combine(path, fileName)), memStream).Result;
                }

                else
                {
                    client.Files.UploadSessionAppendV2Async(cursor, body: memStream).Wait();
                }
                memStream.Close();

            }
        }

        public override IList<IElementoNube> GetElements(string path, bool recursive)
        {
            ListFolderResult list;
            Task<ListFolderResult> taskList = client.Files.ListFolderAsync(path, recursive);
            list = taskList.Result;
            return list.Entries.Cast<IElementoNube>().ToArray();
        }



        public override string GetTemporalPath(IElementoNube elemento)
        {
            return client.Files.GetTemporaryLinkAsync(elemento.PathNube).Result.Link;
        }

        public override string GetIdShareFolder(IElementoNube elemento)
        {
            string idShare = null;
            if (elemento.IsAFolder)
            {
                idShare = client.Sharing.ShareFolderAsync(elemento.PathNube).Result.AsComplete?.Value.SharedFolderId;
            }
            else
            {
                throw new Exception("Los archivos no tienen id");
            }

            return idShare;
        }

        public override bool[] Share(IElementoNube elemento, bool notify = true, string message = null, SharingLevel level = SharingLevel.ViewerNoComment, params string[] emailsUsersToShare)
        {
            bool[] shared = new bool[emailsUsersToShare.Length];
            List<FileMemberActionResult> results;
            Dropbox.Api.Sharing.AddMember[] membersFolder;
            Dropbox.Api.Sharing.MemberSelector[] membersFile;
            Dropbox.Api.Sharing.AccessLevel shareLevel = GetShareLevel(level);
            if (elemento.IsAFolder)
            {
                membersFolder = new Dropbox.Api.Sharing.AddMember[emailsUsersToShare.Length];
                for (int i = 0; i < membersFolder.Length; i++)
                    membersFolder[i] = new Dropbox.Api.Sharing.AddMember(new Dropbox.Api.Sharing.MemberSelector.Email(emailsUsersToShare[i]), shareLevel);
                client.Sharing.AddFolderMemberAsync(GetIdShareFolder(elemento), membersFolder, notify, message).Wait();
            }
            else
            {
                membersFile = new MemberSelector[emailsUsersToShare.Length];
                for (int i = 0; i < membersFile.Length; i++)
                    membersFile[i] = new MemberSelector.Email(emailsUsersToShare[i]);
                results = client.Sharing.AddFileMemberAsync(elemento.PathNube, membersFile, message, notify, shareLevel).Result;
                for (int i = 0; i < results.Count; i++)
                    shared[i] = results[i].Result.IsSuccess;
            }
            return shared;
        }
        /// <summary>
        /// Open a login form and return a User DropoBox account
        /// </summary>
        /// <param name="apiKey"> Add an ApiKey (from https://www.dropbox.com/developers/apps) here</param>
        /// <returns>return null if user don't login successfully</returns>
        public static DropBox Login(string apiKey)
        {

            Application app;
            LoginForm login = null;
            DropBox dropBoxAccount;


            try
            {
                login = new LoginForm(apiKey);

                app = new Application();
                app.Run(login);
            }
            catch (InvalidOperationException ex)
            {
                login.ShowDialog();
            }
            catch { throw; }

            if (login.Success)
            {
                dropBoxAccount = new DropBox(login.AccessToken);
            }
            else
            {
                dropBoxAccount = null;
            }

            return dropBoxAccount;
        }
        public static AccessLevel GetShareLevel(SharingLevel level)
        {
            AccessLevel access;
            switch (level)
            {
                case SharingLevel.Owner:
                    access = AccessLevel.Owner.Instance;
                    break;
                case SharingLevel.Editor:
                    access = AccessLevel.Editor.Instance;
                    break;
                case SharingLevel.Viewer:
                    access = AccessLevel.Viewer.Instance;
                    break;
                case SharingLevel.ViewerNoComment:
                    access = AccessLevel.ViewerNoComment.Instance;
                    break;
                case SharingLevel.Other:
                    access = AccessLevel.Other.Instance;
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
            return access;
        }
    }
}
