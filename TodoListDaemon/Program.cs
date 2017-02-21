//----------------------------------------------------------------------------------------------
//    Copyright 2014 Microsoft Corporation
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// The following using statements were added for this sample.
using System.Globalization;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using System.Threading;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;
using System.Configuration;

using Newtonsoft.Json;

namespace TodoListDaemon
{
    public class TokenData
    {

        public string expires_in { get; set; }
        public string access_token { get; set; }
        //public string refresh_token { get; set; }
        //public string id_token { get; set; }
    }
    class Program
    {

        //
        // The Client ID is used by the application to uniquely identify itself to Azure AD.
        // The App Key is a credential used by the application to authenticate to Azure AD.
        // The Tenant is the name of the Azure AD tenant in which this application is registered.
        // The AAD Instance is the instance of Azure, for example public Azure or Azure China.
        // The Authority is the sign-in URL of the tenant.
        //
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string appKey = ConfigurationManager.AppSettings["ida:AppKey"];

        static string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

        //
        // To authenticate to the To Do list service, the client needs to know the service's App ID URI.
        // To contact the To Do list service we need it's URL as well.
        //
        private static string graphResourceId = ConfigurationManager.AppSettings["ida:GraphResourceId"];
        private static string graphUrl = ConfigurationManager.AppSettings["ida:GraphUrl"];

        private static HttpClient httpClient = new HttpClient();
        private static AuthenticationContext authContext = null;
        private static ClientCredential clientCredential = null;

        static void Main(string[] args)
        {
            //
            // Call the To Do service 10 times with short delay between calls.
            //

            authContext = new AuthenticationContext(authority);
            clientCredential = new ClientCredential(clientId, appKey);
            System.Diagnostics.Debug.WriteLine("this is run");

            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(3000);
                //PostTodo().Wait();
                Thread.Sleep(3000);
                Post().Wait();
            }
        }

        static async Task PostTodo()
        {
            //
            // Get an access token from Azure AD using client credentials.
            // If the attempt to get a token fails because the server is unavailable, retry twice after 3 seconds each.
            //
            AuthenticationResult result = null;
            int retryCount = 0;
            bool retry = false;

            do
            {
                retry = false;
                try
                {
                    // ADAL includes an in memory cache, so this call will only send a message to the server if the cached token is expired.
                    result = await authContext.AcquireTokenAsync(graphResourceId, clientCredential);
                }
                catch (AdalException ex)
                {
                    if (ex.ErrorCode == "temporarily_unavailable")
                    {
                        retry = true;
                        retryCount++;
                        Thread.Sleep(3000);
                    }

                    Console.WriteLine(
                        String.Format("An error occurred while acquiring a token\nTime: {0}\nError: {1}\nRetry: {2}\n",
                        DateTime.Now.ToString(),
                        ex.ToString(),
                        retry.ToString()));
                }

            } while ((retry == true) && (retryCount < 3));

            if (result == null)
            {
                Console.WriteLine("Canceling attempt to contact To Do list service.\n");
                return;
            }

            //
            // Post an item to the To Do list service.
            //

            // Add the access token to the authorization header of the request.

            var content = new FormUrlEncodedContent(new []
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", "6fdf0559-62d6-460f-9610-73af634a1f35"),
                new KeyValuePair<string, string>("client_secret", "VHM6KgjcJm5J6z58McCgI81xbmUc/kBiVG1wihiozq8="),
                new KeyValuePair<string, string>("resource", "https://graph.microsoft.com")

            });

            HttpResponseMessage response = await httpClient.PostAsync("https://login.windows.net/0bb196ec-0011-49ec-b952-c3c9e088ac04/oauth2/token", content);
            Console.WriteLine("testing extraction");
            string resultContent = await response.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<TokenData>(resultContent);
            Console.WriteLine(token.access_token);
            Console.WriteLine("end extraction");
            //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken + "1");

            //// Forms encode To Do item and POST to the todo list web api.
            //string timeNow = DateTime.Now.ToString();
            ////System.Diagnostics.Debug.WriteLine("Posting to To Do list at {0}", timeNow);
            //Console.WriteLine("Posting to To Do list at {0}", timeNow);
            //string todoText = "Task at time: " + timeNow;
            //HttpContent content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("Title", todoText) });
            //HttpResponseMessage response = await httpClient.GetAsync(graphUrl + "litescape.net/");
            //Console.WriteLine(response);

            //if (response.IsSuccessStatusCode == true)
            //{
            //    Console.WriteLine("Successfully posted new To Do item:  {0}\n", todoText);
            //}
            //else
            //{
            //    Console.WriteLine("Failed to post a new To Do item\nError:  {0}\n", response.ReasonPhrase);
            //}
        }

        static async Task Post()
        {
            //
            // Get an access token from Azure AD using client credentials.
            // If the attempt to get a token fails because the server is unavailable, retry twice after 3 seconds each.
            //
            AuthenticationResult result = null;
            int retryCount = 0;
            bool retry = false;

            do
            {
                retry = false;
                try
                {
                    // ADAL includes an in memory cache, so this call will only send a message to the server if the cached token is expired.
                    result = await authContext.AcquireTokenAsync(graphResourceId, clientCredential);
                }
                catch (AdalException ex)
                {
                    if (ex.ErrorCode == "temporarily_unavailable")
                    {
                        retry = true;
                        retryCount++;
                        Thread.Sleep(3000);
                    }

                    Console.WriteLine(
                        String.Format("An error occurred while acquiring a token\nTime: {0}\nError: {1}\nRetry: {2}\n",
                        DateTime.Now.ToString(),
                        ex.ToString(),
                        retry.ToString()));
                }

            } while ((retry == true) && (retryCount < 3));

            if (result == null)
            {
                Console.WriteLine("Canceling attempt to contact To Do list service.\n");
                return;
            }

            /////////////
            var content = new FormUrlEncodedContent(new[]
{
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", "6fdf0559-62d6-460f-9610-73af634a1f35"),
                new KeyValuePair<string, string>("client_secret", "VHM6KgjcJm5J6z58McCgI81xbmUc/kBiVG1wihiozq8="),
                new KeyValuePair<string, string>("resource", "https://graph.microsoft.com/")

            });

            HttpResponseMessage response = await httpClient.PostAsync("https://login.windows.net/0bb196ec-0011-49ec-b952-c3c9e088ac04/oauth2/token", content);
            

            string resultContent = await response.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<TokenData>(resultContent);
            Console.WriteLine(resultContent);
            Console.WriteLine(token.access_token);

            /////////////


            //var request = new HttpRequestMessage()
            //{
            //    RequestUri = new Uri("https://graph.microsoft.com/v1.0/users"),
            //    Method = HttpMethod.Get,
            //};



            //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
            HttpClient httpClientGet = new HttpClient();
            httpClientGet.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);
            HttpResponseMessage responseGet = await httpClientGet.GetAsync("https://graph.microsoft.com/v1.0/users");

            string resultContentGet = await responseGet.Content.ReadAsStringAsync();

            Console.WriteLine("test get\n");
            Console.WriteLine();
            Console.WriteLine(resultContentGet);
            Console.WriteLine("end test get\n");


            //
            // Read items from the To Do list service.
            //

            // Add the access token to the authorization header of the request.
            //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken + "123");

            // Call the To Do list service.
            //Console.WriteLine("Retrieving To Do list at {0}", DateTime.Now.ToString());
            //HttpResponseMessage response = await httpClient.GetAsync(graphUrl + "litescape.net/me");
            //Console.WriteLine("Bearer" + result.AccessToken + "123");
            //Console.WriteLine(graphUrl + "litescape.net/me");
            //Console.WriteLine(response);

            //if (response.IsSuccessStatusCode)
            //{
            //    Console.WriteLine("waddup");

            //    //// Read the response and output it to the console.
            //    //string s = await response.Content.ReadAsStringAsync();
            //    //JavaScriptSerializer serializer = new JavaScriptSerializer();
            //    //List<TodoItem> toDoArray = serializer.Deserialize<List<TodoItem>>(s);

            //    //int count = 0;
            //    //foreach (TodoItem item in toDoArray)
            //    //{
            //    //    Console.WriteLine(item.Title);
            //    //    count++;
            //    //}

            //    //Console.WriteLine("Total item count:  {0}\n", count);
            //}
            //else
            //{
            //    Console.WriteLine("Failed to retrieve To Do list\nError:  {0}/n", response.ReasonPhrase);
            //}
        }
    }
}
