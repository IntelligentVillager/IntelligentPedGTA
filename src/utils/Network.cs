using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GTA.NPCTest.src.config;

namespace GTA.NPCTest.src.utils
{

    public class Network
    {
        private static string USER_AGENT = "Mozilla/5.0";
        private static readonly string AUTH_URL = "https://sso-int-api-prod.rct.ai/auth/login";
        private static readonly string ACCESSTOKEN_URL = "https://socrates-api.rct.ai/v1/applications/95878/subusers";
        private static readonly string CREATE_NODE_URL = "https://socrates-api.rct.ai/v1/applications/95878/nodes/full";
        private static readonly string SET_NODE_URL = "https://socrates-api.rct.ai/v1/applications/95878/nodes/{0}/node_config";
        private static readonly string INTERACT_URL = "https://socrates-api.rct.ai/v1/applications/95878/nodes/{0}/conversation?accessKey={1}&accessToken={2}";
        public static readonly SynchronizationContext _synchronizationContext = SynchronizationContext.Current;

        public static async void getAuthToken(string email, string password, Action<string> callback)
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
                var content = new StringContent(String.Format("{{\"email\":\"{0}\", \"password\":\"{1}\"}}", email, password), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(AUTH_URL, content);
                Logger.INFO(response.StatusCode.ToString());
                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(responseContent);
                    if (json["code"].ToObject<int>() == 200)
                    {
                        Logger.INFO(responseContent);
                        callback(json["data"]["ssotoken"].ToObject<string>());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ERROR(ex);
            }
        }

        public static async void interactWithNode(string text, string nodeId, Action<string> callback)
        {
            try
            {
                Logger.INFO("interactWithNode " + nodeId);

                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);

                var content = new StringContent(String.Format("{{\"text\":\"{0}\"}}", text), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(String.Format(INTERACT_URL,nodeId, ConfigManager.getInstance.getAccessKey(), ConfigManager.getInstance.getAccessToken()), content);
                Logger.INFO(response.StatusCode.ToString());
                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync();
                Logger.INFO(responseContent);
                JObject json = JObject.Parse(responseContent);
                if (json["code"].ToObject<int>() == 0)
                {
                    callback(json["data"][0]["text"].ToObject<string>());
                }
            }
            catch (Exception ex)
            {
                Logger.ERROR(ex);
            }
        }

        public static async void getAccessKeyToken(string ssotoken, Action<Tuple<string, string>> callback)
        {
            try
            {
                Logger.INFO("getAccessKeyToken" + ssotoken);

                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
                client.DefaultRequestHeaders.Add("x-sso-token", ssotoken);

                var content = new StringContent("", Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(ACCESSTOKEN_URL, content);
                Logger.INFO(response.StatusCode.ToString());
                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync();
                Logger.INFO(responseContent);
                JObject json = JObject.Parse(responseContent);
                if (json["code"].ToObject<int>() == 0)
                {
                    callback(new Tuple<string, string>(json["data"]["subusers"]["access_key"].ToObject<string>(),
                        json["data"]["subusers"]["access_token"].ToObject<string>()));
                }
            }
            catch (Exception ex)
            {
                Logger.ERROR(ex);
            }
        }

    }
}
