using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTA.NPCTest.src.config
{
    class ConfigManager
    {
        private static ConfigManager _instance;
        public static ConfigManager getInstance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ConfigManager();
                }
                return _instance;
            }
        }

        private string sso_token = null;
        private string access_token = null;
        private string access_key = null;

        private ConfigManager() { 
        }

        public bool isAccessTokenAndKeySetup()
        {
            return access_key != null && access_token!= null;
        }

        public string getSSOToken()
        {
            return this.sso_token != null ? sso_token : "";
        }

        public void setSSOToken(string s0)
        {
            this.sso_token = s0;
        }

        public string getAccessToken()
        {
            return this.access_token != null ? access_token : "";
        }

        public void setAccessToken(string s0)
        {
            this.access_token = s0;
        }

        public string getAccessKey()
        {
            return this.access_key != null ? access_key : "";
        }

        public void setAccessKey(string s0)
        {
            this.access_key = s0;
        }
    }
}
