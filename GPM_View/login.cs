using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GPM_View
{
    class login
    {
        public account Account { get; set; }
        public UndetectChromeDriver driver { get; set; }
        public login(UndetectChromeDriver driv,account st)
        {
            Account = st;
            this.driver = driv;
        }
        public bool Nanial(string url)
        {
            try { driver.Url = url; Thread.Sleep(3000); } catch { return false; }
            try
            {
                var Element = driver.FindElement(By.Id("identifierId"));
                if (Element != null)
                {
                    return true;
                }
                else
                    return false;
            }
            catch { return false; }
        }
        bool captcha(UndetectChromeDriver driver, out string info)
        {
            IJavaScriptExecutor js = driver;
            try
            {
                info = Convert.ToString(js.ExecuteScript("var content = document.getElementById('captchaimg').src; return content;"));

                return true;
            }
            catch { info = ""; return false; }
        }
        public bool StartLogin(out string Error)
        {
            driver.FindElement(By.Id("identifierId")).SendKeys(Account.email); TimeSpan.FromSeconds(1);
            clickNext(driver);
            Thread.Sleep(TimeSpan.FromSeconds(5));
            int wait = 20; string ids = string.Empty;
            while (wait > 0)
            {
                if (captcha(driver, out ids))
                {
                    if (ids.Length > 20)
                    {
                        Error = "captcha";
                        return false;
                    }
                }
                try { driver.FindElement(By.Name("Passwd")).SendKeys(Account.password); break; } catch { 
                    try { driver.FindElement(By.Name("password")).SendKeys(Account.password); break; } catch { Thread.Sleep(1000); } }
                wait -= 1;
            }
            if (wait == 0)
            {
                Error = "network";
                return false;
            }
            Thread.Sleep(TimeSpan.FromSeconds(2));
            clickNext(driver);
            Thread.Sleep(TimeSpan.FromSeconds(5));
            int times = 15;
            int timesClick = 0;
            while (times > 0)
            {
                if (driver.Url.Contains("nin/v2/challenge/selection"))
                {
                    try { driver.ExecuteScript("document.getElementsByClassName('vxx8jf')[" + timesClick + "].click();"); } catch { }
                    timesClick += 1;
                    Thread.Sleep(TimeSpan.FromSeconds(4));
                }
                if (driver.Url.Contains("/challenge/kpe?"))
                {
                    sendMailRecover(); Thread.Sleep(TimeSpan.FromSeconds(5));
                    Thread.Sleep(TimeSpan.FromSeconds(4));
                    break;
                }
                else
                {
                    if (driver.Url.Contains("om/signin/v2/challenge/ipe"))
                    {
                        driver.ExecuteScript("document.getElementsByClassName('VfPpkd-Jh9lGc')[1].click()"); Thread.Sleep(TimeSpan.FromSeconds(5));
                    }
                    else
                    {
                        driver.ExecuteScript("document.getElementsByClassName('VfPpkd-Jh9lGc')[0].click()"); Thread.Sleep(TimeSpan.FromSeconds(5));
                    }
                }
                if (driver.Url.Contains(".google.com/create/new?") || driver.Url.Contains(".google.com/dashboard"))
                {
                    break;
                }
                times -= 1; Thread.Sleep(1500);
            }
            //--------------------------------------------------------
            wait = 30;
            while (XetKieu(driver.Url, wait))
            {
                wait -= 1;
                Thread.Sleep(1000);
            }
            if (wait != 0)
            {
                Error = "";
                return true;
            }
            else
            {
                Error = "Timeout";
                return false;
            }
        }
        bool XetKieu(string url, int wait)
        {
            if (driver.Url.Contains("inoptions/recovery-options-collection") || driver.Url.Contains("/signin/v2/challenge/pwd") || driver.Url.Contains(".google.com/create/new?") || driver.Url.Contains(".google.com/dashboard"))
            {
                return false;
            }
            if (driver.Url.Contains("/signin/v2/challenge/pwd") || (driver.Url.Contains("m/signin/v2/identifier")) || (driver.Url.Contains("ccounts.google.com/speedbump/idvreenable")) || (driver.Url.Contains("m/signin/v2/disabled/explanation")))
            {
                return false;
            }
            if (driver.Url.Contains("om/signin/v2/challenge/iap") || (driver.Url.Contains("gle.com/signin/rejected")))
            {
                return false;
            }
            if (driver.Url.Contains("w/browser_not_supported?") || driver.Url.Contains("iness.google.com/create/new"))
            {
                return false;
            }
            if ((driver.Url.Contains("/disabled/explanation?")) || (driver.Url.Contains("om/aw/overview?")) || (driver.Url.Contains("/signinoptions")) || (wait > 0) && (driver.Url.Contains("business.google")) || (driver.Url.Contains("gle.com/interstitials/birthday")) || (driver.Url.Contains("w/browser_not_supported?")))
            {
                return false;
            }
            return true;
        }
        void clickNext(UndetectChromeDriver driver)
        {
            try { driver.FindElement(By.XPath("//span[text()='Next']")).Click(); }
            catch
            {
                driver.FindElement(By.XPath("//span[text()='Tiếp theo']")).Click();
            }
        }
        void sendMailRecover()
        {
            try { driver.FindElement(By.Name("knowledgePreregisteredEmailResponse")).SendKeys(Account.mail_kp); }
            catch
            {
                driver.FindElement(By.Id("knowledge-preregistered-email-response")).SendKeys(Account.mail_kp);
            }
            Thread.Sleep(3000);
            clickNext(driver);
        }
        void clickAction(UndetectChromeDriver driver, string Xpath)
        {
            var finishss = driver.FindElement(By.XPath(Xpath));
            Actions actionss = new Actions(driver);
            actionss.MoveToElement(finishss).Click().Build().Perform();
        }
    }
}
