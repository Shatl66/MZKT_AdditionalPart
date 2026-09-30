using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace E3SetAdditionalPart
{
    public class WindchillHTTPClient
    {
        String serverProtocol = "";
        String serverName = "";
        String location = "";
        private HttpClient client = null;
        public WindchillHTTPClient(String serverProtocol, String serverName, bool ignoreSSLPolicyErrors)
        {
            this.serverProtocol = serverProtocol;
            this.serverName = serverName;
            this.location = serverProtocol + "://" + serverName + "/Windchill";
            if (ignoreSSLPolicyErrors)
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            }
            client = new HttpClient();
        }

        internal bool checkLogin(string userName, string password)
        {
            var authToken = Encoding.ASCII.GetBytes($"{userName}:{password}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(authToken));

            String jspFile = "netmarkets/jsp/by/iba/e3/http/checkLogin.jsp";
            String urlpath = location + "/" + jspFile;
            Task<HttpResponseMessage> taskResponse =  client.GetAsync(urlpath);
            taskResponse.Wait();
            taskResponse.Result.EnsureSuccessStatusCode();
            return true;
        }

        internal string getJSON(string json, string jspFile)
        {
            String urlpath = location + "/" + jspFile;
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            Task<HttpResponseMessage> taskResponse = client.PostAsync(urlpath, data);
            taskResponse.Wait();
            taskResponse.Result.EnsureSuccessStatusCode();
            Task<byte[]> taskByteArray = taskResponse.Result.Content.ReadAsByteArrayAsync();
            taskByteArray.Wait();
            var responseString = Encoding.UTF8.GetString(taskByteArray.Result, 0, taskByteArray.Result.Length);
            return responseString;
        }

        internal string updateE3DocumentationContent(string json, string jspFile, string filePath, string fileName)
        {
            String urlpath = location + "/" + jspFile;

            var content = new MultipartFormDataContent();

            var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
            content.Add(jsonContent, "attributes");
            byte[] data;
            using (FileStream fstream = File.OpenRead(filePath + "/" + fileName))
            {
                data = new byte[fstream.Length];
                fstream.Read(data, 0, data.Length);
            }
            var bytes = new ByteArrayContent(data);
            content.Add(bytes, "file", fileName);

            Task<HttpResponseMessage> taskResponse = client.PostAsync(urlpath, content);
            taskResponse.Wait();
            taskResponse.Result.EnsureSuccessStatusCode();
            Task<byte[]> taskByteArray = taskResponse.Result.Content.ReadAsByteArrayAsync();
            taskByteArray.Wait();
            var responseString = Encoding.UTF8.GetString(taskByteArray.Result, 0, taskByteArray.Result.Length);
            return responseString;
        }

        internal string updateE3PrjDocContent(string json, string jspFile, string filePath, string fileName)
        {
            return updateE3DocumentationContent(json, jspFile, filePath, fileName);
        }

        internal String downloadE3PrjDocContent(string json, string jspFile, string filePath, string fileName)
        {
            String urlpath = location + "/" + jspFile;
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            Task<HttpResponseMessage> taskResponse = client.PostAsync(urlpath, data);
            taskResponse.Wait();
            taskResponse.Result.EnsureSuccessStatusCode();
            Task<byte[]> taskByteArray = taskResponse.Result.Content.ReadAsByteArrayAsync();
            string localPath = Path.Combine(filePath, fileName);
            File.WriteAllBytes(localPath, taskByteArray.Result);
            return localPath;

        }

        internal bool isAuthorization()
        {
            return client.DefaultRequestHeaders.Authorization != null;
        }
    }
}
