using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat.Nube
{
   public class User
    {
        string username;
        string lenguaje;
        string email;
        string uriPhoto;
        bool isEmailVerified;

        public User(string username, string lenguaje, string email, string uriPhoto, bool isEmailVerified)
        {
            this.username = username;
            this.lenguaje = lenguaje;
            this.email = email;
            this.uriPhoto = uriPhoto;
            this.isEmailVerified = isEmailVerified;
        }

        public string Username { get => username; set => username = value; }
        public string Lenguaje { get => lenguaje; set => lenguaje = value; }
        public string Email { get => email; set => email = value; }
        public string UriPhoto { get => uriPhoto; set => uriPhoto = value; }
        public bool IsValidated { get => isEmailVerified; set => isEmailVerified = value; }
    }
}
