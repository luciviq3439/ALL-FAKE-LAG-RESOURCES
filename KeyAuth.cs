using System;
using System.Net;
using System.Text;
using System.Text.Json;

namespace FreezeLagCMD
{
    public class api
    {
        public string name;
        public string ownerid;
        public string secret;
        public string version;
        public Response response;

        public api(string name, string ownerid, string secret, string version)
        {
            this.name = name;
            this.ownerid = ownerid;
            this.secret = secret;
            this.version = version;
            response = new Response();
        }

        public void init()
        {
            Console.WriteLine("KeyAuth initialized...");
        }

        public void login(string username, string password)
        {
            // Basit simülasyon
            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                response.success = true;
                response.message = "Giriş başarılı!";
            }
            else
            {
                response.success = false;
                response.message = "Kullanıcı adı veya şifre boş!";
            }
        }

        public class Response
        {
            public bool success = false;
            public string message = "";
        }
    }
}
