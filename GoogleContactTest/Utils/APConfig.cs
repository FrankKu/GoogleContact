using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Utils
{
    public class APConfig
    {
        public static string GetAppConfig(string strKey)
        {

            ExeConfigurationFileMap configMap = new ExeConfigurationFileMap();
            configMap.ExeConfigFilename = string.Concat(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, @"APConfig.config");
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

            if (config != null)
            {
                return config.AppSettings.Settings[strKey].Value.ToString();
            }

            return null;
        }  
    }
}