using System;
using System.Linq;
using System.Configuration;

namespace IpoDataDartTest
{
    class clsAppConfig
    {
        public string AppConfigRead(string keyName)
        {
            string strReturn;
            Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (currentConfig.AppSettings.Settings.AllKeys.Contains(keyName))
                strReturn = currentConfig.AppSettings.Settings[keyName].Value;
            else
                strReturn = ""; //키가없으면.

            return strReturn;
        }

        public bool AppConfigWrite(string keyName, string value)
        {
            Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (currentConfig.AppSettings.Settings.AllKeys.Contains(keyName)) //키가 있으면
                currentConfig.AppSettings.Settings[keyName].Value = value;
            else       //키가 없으면
                currentConfig.AppSettings.Settings.Add(keyName, value);

            currentConfig.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");   // 내용 갱신              

            return true;
        }
    }
}
