using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GPM_View
{
    internal class acton
    {
        public account act { get; set; }
        GPMLoginAPI api { get; set; }
        UndetectChromeDriver driver { get; set; }
        public acton(account account, GPMLoginAPI api)
        {
            this.act = account;
            this.api = api;
        }
        public JObject getLst(string names , List<JObject> profiles)
        {
            if (profiles != null)
            {
                foreach (JObject profile in profiles)
                {
                    string name = Convert.ToString(profile["name"]);
                    string id = Convert.ToString(profile["id"]);
                    if (name.Contains(names))
                    {
                        return profile;
                    }
                    Console.WriteLine($"ID: {id} | Name: {name}");
                }
            }
            return null;
        }
        public UndetectChromeDriver openProfile(string createdProfileId,int thread)
        {
            int z_index = thread % 300;
            JObject startedResult = api.Start(createdProfileId);
            Thread.Sleep(3000);
            if (startedResult != null)
            {
                string browserLocation = Convert.ToString(startedResult["browser_location"]);
                string seleniumRemoteDebugAddress = Convert.ToString(startedResult["selenium_remote_debug_address"]);
                string gpmDriverPath = Convert.ToString(startedResult["selenium_driver_location"]);

                // Init selenium
                FileInfo gpmDriverFileInfo = new FileInfo(gpmDriverPath);

                ChromeDriverService service = ChromeDriverService.CreateDefaultService(gpmDriverFileInfo.DirectoryName, gpmDriverFileInfo.Name);
                service.HideCommandPromptWindow = true;
                ChromeOptions options = new ChromeOptions();
                options.BinaryLocation = browserLocation;
                options.DebuggerAddress = seleniumRemoteDebugAddress;

                driver = new UndetectChromeDriver(service, options);
                driver.Manage().Window.Position = new Point(50 * Convert.ToInt32(z_index / 25) , 35 * Convert.ToInt32(z_index % 25));
                driver.Manage().Window.Size = new Size(1200, 800);
            }
            return driver;
        }
        public void close()
        {
            driver.Close();
            driver.Quit();
            driver.Dispose();
        }
    }
}
