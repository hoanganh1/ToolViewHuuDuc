using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;

namespace GPM_View
{
    public partial class Form1 : Form
    {
        List<string> listComments = new List<string>();
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        List<account> lstAccount;
        List<string> lstProxy;
        int proxyNumber = 0;
        int numberRow = 0;
        Random random = new Random();
        private void btnEmail_Click(object sender, EventArgs e)
        {
            checkopentab = true;
            lstAccount = new List<account>();
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            if (ofd.FileName.Length > 0)
            {
                var lines = File.ReadAllLines(ofd.FileName);
                int num = 0;
                foreach (var line in lines)
                {
                    account account = new account();
                    account.stt = num; num += 1;
                    account.email = line.Split('\t')[0].Trim();
                    account.password = line.Split('\t')[1].Trim();
                    account.mail_kp = line.Split('\t')[2].Trim();
                    account.status = "";
                    lstAccount.Add(account);
                }
            }
            dataGrid.DataSource = lstAccount;
            dataGrid.Columns[0].Width = 50;
            dataGrid.Columns[1].Width = 100;
            dataGrid.Columns[2].Width = 70;
            dataGrid.Columns[3].Width = 50;
            dataGrid.Columns["status"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            numberRow = 0;
        }
        void loadProxy()
        {
            lstProxy = new List<string>();
            var item = File.ReadAllLines("data\\proxy.txt");
            foreach (string line in item)
            {
                lstProxy.Add(line.Trim());
            }
        }
        void saveError(account mails, string error)
        {
            for (int i = 0; i < 1000; i++)
            {
                try { File.AppendAllText("Error.txt", mails.email + "|" + mails.password + "|" + mails.mail_kp + "|" + error + "\r\n"); break; } catch { Thread.Sleep(100); }
            }
        }
        void resave(string proxy)
        {
            lstProxy.Remove(proxy);
            try
            {
                File.WriteAllText("data\\proxy.txt", String.Join("\r\n", lstProxy));
            }
            catch { }
        }
        void createThread(int indexKichBan)
        {
            for (int i = 0; i < nbThread.Value; i++)
            {
                Thread st = new Thread(() =>
                {
                    if (flag_stop == false)
                    {
                        run(indexKichBan);
                    }
                });
                st.IsBackground = true;
                st.Start();
                listThread.Add(st);
                Thread.Sleep(1000);
            }
        }
        void addStatus(int index, string Text)
        {
            dataGrid.Rows[index].Cells["status"].Value = Text;
        }
        bool clickDeSau(UndetectChromeDriver driver)
        {
            try
            {
                driver.ExecuteScript("document.getElementsByClassName('VfPpkd-Jh9lGc')[0].click()"); return true;
            }
            catch
            {
                ClickXacMinh(driver);
                return false;
            }
        }
        bool ClickXacMinh(UndetectChromeDriver driver)
        {
            try
            {
              
                driver.ExecuteScript("document.getElementsByClassName('ZFr60d CeoRYc')[1].click()"); return true;
            }
            catch
            {
                return false;

            }
        }
        void addbirthday(UndetectChromeDriver driver)
        {
            driver.Navigate().GoToUrl(urlLogin);
            Thread.Sleep(7000);
        }
        List<string> list = new List<string>();

        string urlLogin = "https://accounts.google.com/signin/v2/identifier?service=lbc&passive=1209600&continue=https%3A%2F%2Fbusiness.google.com%2F%3FskipPagesList%3D1%26skipLandingPage%3Dtrue%26hl%3Den%26gmbsrc%3Dus-en-z-z-z-gmb-l-z-d~mhp-hom_sig-u&followup=https%3A%2F%2Fbusiness.google.com%2F%3FskipPagesList%3D1%26skipLandingPage%3Dtrue%26hl%3Den%26gmbsrc%3Dus-en-z-z-z-gmb-l-z-d~mhp-hom_sig-u&hl=en&flowName=GlifWebSignIn&flowEntry=ServiceLogin";
        UndetectChromeDriver driver2 = null;
        void kichban2()
        {
            int dem_kb2 = 0;
            int index = numberRow;
            numberRow += 1;
            if (index >= dataGrid.Rows.Count)
            {
                return;
            }
            if (!checkopentab)
            {
                return;
            }
            account act = lstAccount[index];
            driver2 = null;
            try
            {
                addStatus(index, "starting");
                GPMLoginAPI api = new GPMLoginAPI("http://" + APP_URL.Text);
                acton sts = new acton(act, api);
                Thread.Sleep(1000);
                JObject ojb = sts.getLst(act.email, profiles);
                string id_profile = "";
                if (ojb == null)
                {
                    int prox = proxyNumber;
                    loadProxy();
                    proxyNumber += 1;
                    if (prox >= lstProxy.Count)
                    {
                        proxyNumber = 0;
                        prox = proxyNumber;
                    }
                    string proxy = lstProxy[prox];
                    resave(proxy);
                 
                    ojb = api.Create(act.email, proxy, true);
                    if (ojb != null)
                    {
                        //Tạo thành công
                        id_profile = ojb["profile_id"].ToString();
                        addStatus(index, "tạo profile thành công");
                        saveProfile(act, proxy);
                    }
                }
                else
                {
                    //đã có profile
                    id_profile = ojb["id"].ToString();
                }
                Thread.Sleep(1000);
                bool lockWasTaken = false;
                var temp = obj;
                try
                {
                    Monitor.Enter(temp, ref lockWasTaken);
                    addStatus(index, "đang mở profile");
                    try { driver2 = sts.openProfile(id_profile, index); }
                    catch
                    {
                        addStatus(index, "Lỗi mở profile");
                        saveError(act, "Lỗi mở profile");
                        goto ketthuc;
                    }
                }
                finally
                {
                    if (lockWasTaken)
                    {
                        Monitor.Exit(temp);
                    }
                }
                while (dem_kb2 < nbThread.Value - 1)
                {
                    dem_kb2++;
                    Thread.Sleep(2000);
                }
                addStatus(index, "truy cập google");
                try { driver2.Get(urlLogin); } catch {

                    addStatus(index, "Lỗi truy cập google");
                    driver2.Close();
                    driver2.Dispose();
                    driver2.Quit();
                    goto ketthuc;}

                Thread.Sleep(2000);
               
                if (driver2.Url.Contains("business.google.com/create/new"))
                {
                    goto searchz;
                }
                login st = new login(driver2, act); int demnha = 0;
                string Error = string.Empty;
            lainha:
                if (st.Nanial(urlLogin))
                {
                    if (!st.StartLogin(out Error))
                    {
                        if (Error == "captcha")
                        {
                            demnha += 1;
                            if (demnha == 7)
                            {
                                addStatus(index, "Lỗi captcha !");
                                driver2.Close();
                                driver2.Dispose();
                                driver2.Quit();
                                goto ketthuc;
                            }
                            goto lainha;
                        }

                        //Cảnh bảo lỗi
                    }
                }
                Thread.Sleep(TimeSpan.FromSeconds(3));
                //create account 
                addStatus(index, "Đã login mail !");
                clickDeSau(driver2); Thread.Sleep(TimeSpan.FromSeconds(5));
                if (driver2.Url.Contains("inoptions/recovery-options-collection"))
                {
                    driver2.Navigate().GoToUrl(urlLogin);
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
                if (driver2.Url.Contains("om/signin/v2/challenge/iap") || (driver2.Url.Contains("gle.com/signin/rejected")))
                {
                    saveError(act, "Very phone");
                    addStatus(index, "very phone !");
                    driver2.Close();
                    driver2.Dispose();
                    driver2.Quit();
                    goto ketthuc;
                }
                if (driver2.Url.Contains(".com/interstitials/birthday") || (driver2.Url.Contains("ogle.com/web/chip")) || (driver2.Url.Contains("/info/unknownerror")))
                {
                    addStatus(index, "Vui lòng thêm ngày sinh");
                    addbirthday(driver2);
                }
                if ((driver2.Url.Contains("m/signin/v2/identifier")) || (driver2.Url.Contains("ccounts.google.com/speedbump/idvreenable")) || (driver2.Url.Contains("m/signin/v2/disabled/explanation")))
                {
                    saveError(act, "Đăng nhập không thành công!");
                    addStatus(index, "Đăng nhập không thành công");
                    driver2.Close();
                    driver2.Dispose();
                    driver2.Quit();
                    goto ketthuc;
                }
                if (driver2.Url.Contains("ogle.com/web/chip"))
                {
                    addbirthday(driver2);
                }
            searchz:
                if (driver2.Url.Contains("/signin/v2/challenge/pwd"))
                {
                    saveError(act, "Đăng nhập không thành công");
                    addStatus(index, "Đăng nhập không thành công");
                    driver2.Close();
                    driver2.Dispose();
                    driver2.Quit();
                    goto ketthuc;
                }
                else
                {
                    acYoutube active = new acYoutube(driver2);
                    active.gotoHome();
                    Thread.Sleep(2000);
                vireyt:
                    try
                    { 
                        IJavaScriptExecutor executorUseData = driver2;
                        string name = txtKeyword.Text;
                        addStatus(index, "Tìm kiếm theo key " + name);
                        active.searchKeyword(name);
                        Thread.Sleep(TimeSpan.FromSeconds(4));
                        try
                        {
                            var wait = new WebDriverWait(driver1, TimeSpan.FromSeconds(5));
                            var myElement = wait.Until(x => x.FindElement(By.XPath("//ytd-video-renderer//ytd-thumbnail//a")));
                        }
                        catch
                        {
                            addStatus(index, "lỗi load trang youtube");
                            driver2.Close();
                            driver2.Dispose();
                            driver2.Quit();
                        }
                       
                        var items = driver2.FindElements(By.XPath("//ytd-video-renderer//ytd-thumbnail//a"));
                        
                        foreach (var item in items)
                        {
                            string urlVideo = item.GetAttribute("href");
                            if (urlVideo.Contains(strTenChannel))
                            {
                                executorUseData.ExecuteScript("arguments[0].click()", item);
                                break;
                            }
                        }
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                        countTime time = new countTime();

                        int timevideo = 0;
                        for (int i = 0; i < 300; i++)
                        {
                            if (i % 50 == 0)
                            {
                                driver2.Navigate().Refresh();
                                Thread.Sleep(2000);
                            }
                            timevideo = active.getTimeVideo();
                            while (timevideo < 300) // time video < 300 (s) tuc la co quang cao ngay dau
                            {
                                Thread.Sleep(TimeSpan.FromSeconds(60));
                                timevideo = active.getTimeVideo();
                            }
                            if (timevideo != 0)
                            {
                                break;
                            }
                            Thread.Sleep(1500);
                        }
                        time.reset();
                        Thread.Sleep(TimeSpan.FromSeconds(30));
                        addStatus(index, "Reset video ve 0");
                        // Khởi tạo đối tượng thuộc Actions class
                        Actions action = new Actions(driver2);

                        action.KeyDown(OpenQA.Selenium.Keys.NumberPad0);

                        action.SendKeys(OpenQA.Selenium.Keys.NumberPad0).Perform();
                        driver2.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(6);
                        Thread.Sleep(TimeSpan.FromSeconds(5));

                        int rand = rand = random.Next(3, 6);
                        int sleepTime = rand * timevideo / 100;
                        addStatus(index, "Chuyen video sau : " + sleepTime + " s");
                        driver2.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(12);
                        Thread.Sleep(TimeSpan.FromSeconds(10));

                        //Sleep 1min
                        //Scroll after 20min
                        if (sleepTime > 1 * 60)
                        {
                            int sleepCount = sleepTime / (60);
                            int sleepCountDiv = sleepTime % 60;
                            if (sleepCountDiv > 0)
                            {
                                sleepCount += 1;
                            }
                            for (int j = 0; j < sleepCount; j++)
                            {
                                if (j == sleepCount - 1)
                                {
                                    addStatus(index, sleepCountDiv.ToString());
                                    driver2.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(sleepTime + 2 - (j - 1) * 60);
                                    Thread.Sleep(TimeSpan.FromSeconds(sleepCountDiv));
                                }
                                else
                                {
                                    addStatus(index, "đang xem video mồi");
                                    driver2.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1 * 60);
                                    Thread.Sleep(TimeSpan.FromMinutes(1));
                                    if ((j + 1) % 20 == 0)
                                    {
                                        int x = 100;
                                        driver2.ExecuteScript("window.scrollTo(100," + (x * j + ")"));
                                        driver2.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(4);
                                        Thread.Sleep(TimeSpan.FromSeconds(2));
                                        addStatus(index, "2s");
                                        driver2.ExecuteScript("window.scrollTo({ top: 0, behavior: 'smooth' });");
                                    }

                                }
                            }
                        }
                        else
                        {
                            driver2.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(sleepTime + 2);
                            Thread.Sleep(TimeSpan.FromSeconds(sleepTime));
                        }
                      
                        Thread.Sleep(5000);

                        addStatus(index, "Click Tim PLL");

                        driver2.FindElement(By.XPath("//a[@class='yt-simple-endpoint style-scope ytd-video-owner-renderer']")).Click();

                        Thread.Sleep(10000);
                        
                        //
                        driver2.FindElement(By.XPath("//*[@id='tabsContent']/tp-yt-paper-tab[3]")).Click();

                        Thread.Sleep(10000);
                        int str = Convert.ToInt32(driver2.ExecuteScript("var t = document.getElementsByClassName('ytd-playlist-thumbnail').length; return t;"));
                        if (str > 0)
                        {
                            int t = random.Next(0, str);
                            driver2.ExecuteScript("document.getElementsByClassName('ytd-playlist-thumbnail')[" + t + "].click()");
                        }
                        else
                        {
                            driver2.FindElement(By.XPath("//*[@id='tabsContent']/tp-yt-paper-tab[4]")).Click();
                            Thread.Sleep(10000);
                            str = Convert.ToInt32(driver2.ExecuteScript("var t = document.getElementsByClassName('ytd-playlist-thumbnail').length; return t;"));
                            int t = random.Next(0, str);
                            driver2.ExecuteScript("document.getElementsByClassName('ytd-playlist-thumbnail')[" + t + "].click()");
                        }

                        Thread.Sleep(10000);
                        int videoNext = 0;
                       for(int i = 0; i < 1000; i++) {

                            videoNext += 1;
                            bool is_comment = false;

                            timevideo = active.getTimeVideo();

                            while (timevideo < 300) // time video < 300 (s) tuc la co quang cao ngay dau
                            {
                                Thread.Sleep(TimeSpan.FromSeconds(60));
                                timevideo = active.getTimeVideo();
                            }
                            int rand_video_sub= random.Next(3, 8); // random khoang time sub channel
                            if (videoNext == rand_video_sub)
                            {
                                
                                if (sub.Checked)
                                {

                                    try { active.subVideo(); }
                                    catch
                                    {
                                        continue;
                                    }
                                }
                            }
                            Thread.Sleep(5000);

                            addStatus(index, "Reset video ve 0");
                            // Khởi tạo đối tượng thuộc Actions class
                            action = new Actions(driver2);

                            action.KeyDown(OpenQA.Selenium.Keys.NumberPad0);

                            action.SendKeys(OpenQA.Selenium.Keys.NumberPad0).Perform();
                            driver2.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(6);
                            Thread.Sleep(TimeSpan.FromSeconds(5));

                            rand = random.Next(15, 30);

                            sleepTime = rand * timevideo / 100;
                            addStatus(index, "Chuyen video sau : " + sleepTime + " s");
                            driver2.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(12);
                            Thread.Sleep(TimeSpan.FromSeconds(10));

                            //Sleep 1min
                            //Scroll after 20min
                            if (sleepTime > 1 * 60)
                            {
                                int sleepCount = sleepTime / (60);
                                int sleepCountDiv = sleepTime % 60;
                                if (sleepCountDiv > 0)
                                {
                                    sleepCount += 1;
                                }
                                int rand_cmt_time = random.Next(2, sleepCount);// random khoang time comment don vi phut

                                for (int j = 0; j < sleepCount; j++)
                                {
                                    if (j == sleepCount - 1)
                                    {
                                        addStatus(index, sleepCountDiv.ToString());
                                        driver2.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(sleepTime + 2 - (j - 1) * 60);
                                        Thread.Sleep(TimeSpan.FromSeconds(sleepCountDiv));
                                    }
                                    else
                                    {
                                        addStatus(index, "Đang xem video PLL");
                                        driver2.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1 * 60);
                                        Thread.Sleep(TimeSpan.FromMinutes(1));
                                        if ((j + 1) % 20 == 0)
                                        {
                                            int x = 100;
                                            driver2.ExecuteScript("window.scrollTo(100," + (x * j + ")"));
                                            driver2.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(4);
                                            Thread.Sleep(TimeSpan.FromSeconds(2));
                                            addStatus(index, "2s");
                                            driver2.ExecuteScript("window.scrollTo({ top: 0, behavior: 'smooth' });");
                                        }
                                        if (is_comment == false)
                                        {
                                            int rand_video_cmt = random.Next(2, 6);
                                            Thread.Sleep(10000);
                                            if (videoNext % rand_video_cmt == 0)
                                            {
                                                if (j == rand_cmt_time)
                                                {
                                                    if (listComments.Count > 0)
                                                    {
                                                        try
                                                        {
                                                            int randomCommend = random.Next(listComments.Count);
                                                            active.goToComment(listComments[randomCommend]);
                                                            is_comment = true;
                                                            Thread.Sleep(6000);
                                                        }
                                                        catch
                                                        {
                                                            continue;
                                                        }
                                                    }
                                                }

                                            }

                                        }

                                    }
                                   

                                }
                            }
                            else
                            {
                                driver2.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(sleepTime + 2);
                                Thread.Sleep(TimeSpan.FromSeconds(sleepTime));
                            }
                            int tr = random.Next(0, 2);
                            if (videoNext == 1)
                            {
                                tr = 1;
                            }
                            addStatus(index, "Click Video Next");
                            if (tr == 0)
                            {
                                try { driver2.ExecuteScript("document.getElementsByClassName('ytp-prev-button ytp-button')[0].click()"); }
                                catch
                                {
                                    Thread.Sleep(1000);
                                }
                                //Back
                            }
                            else
                            {
                                //forword
                                try { driver2.ExecuteScript("document.getElementsByClassName('ytp-next-button ytp-button')[0].click()"); }
                                catch
                                {
                                    Thread.Sleep(1000);
                                }
                            }
                            Thread.Sleep(10000);
                            if(i == 999)
                            {
                                addStatus(index, "xem hết pll");
                                driver2.Close();
                                driver2.Dispose();
                                driver2.Quit();
                                return;
                            }
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        addStatus(index, "timecout access youtube");
                        driver2.Close();
                        driver2.Dispose();
                        driver2.Quit();
                        goto ketthuc;
                    }
                }

            }
            catch (Exception ex)
            {
                addStatus(index, ex.Message.ToString());
                goto ketthuc;
            }
        ketthuc:
            runNew(2);
            return;
        }
        bool flag_start = true;
        List<JObject> profiles;
        int slThread = 0;
        void run(int indexKichBan)
        {
            if(flag_start == true)
            {
                GPMLoginAPI api = new GPMLoginAPI("http://" + APP_URL.Text);
                profiles = api.GetProfiles();
            }
            slThread++;
            flag_start = false;
            if (indexKichBan == 2)
            {
                kichban2();
            }
            else if(indexKichBan == 1)
            {
                kichban1();
            }
            else
            {
                kichban3();
            }
        }
        private static readonly Object obj = new Object();

        UndetectChromeDriver driver1 = null;
        private void kichban1()
        {
            int dem_kb1 = 0;
            int index = numberRow;
            numberRow += 1;
            if (index >= dataGrid.Rows.Count)
            {
                return;
            }
            if (!checkopentab)
            {
                return;
            }
            account act = lstAccount[index];
            driver1 = null;
            try
            {
                addStatus(index, "starting");
                GPMLoginAPI api = new GPMLoginAPI("http://" + APP_URL.Text);
                acton sts = new acton(act, api);
                Thread.Sleep(1000);
                JObject ojb = sts.getLst(act.email, profiles);
                string id_profile = "";
                if (ojb == null)
                {
                    int prox = proxyNumber;
                    loadProxy();
                    proxyNumber += 1;
                    if (prox >= lstProxy.Count)
                    {
                        proxyNumber = 0;
                        prox = proxyNumber;
                    }
                    string proxy = lstProxy[prox];
                    resave(proxy);

                    ojb = api.Create(act.email, proxy, true);
                    if (ojb != null)
                    {
                        //Tạo thành công
                        id_profile = ojb["profile_id"].ToString();
                        addStatus(index, "tạo profile thành công");
                        saveProfile(act, proxy);
                    }
                }
                else
                {
                    //đã có profile
                    id_profile = ojb["id"].ToString();
                }
                Thread.Sleep(1000);
               
                bool lockWasTaken = false;
                var temp = obj;
                try
                {
                    Monitor.Enter(temp, ref lockWasTaken);
                    addStatus(index, "đang mở profile");
                    try { driver1 = sts.openProfile(id_profile, index); }
                    catch
                    {
                        addStatus(index, "Lỗi mở profile");
                        saveError(act, "Lỗi mở profile");
                        goto ketthuc;
                    }
                }
                finally
                {
                    if (lockWasTaken)
                    {
                        Monitor.Exit(temp);
                    }
                }
                while (dem_kb1 < nbThread.Value - 1)
                {
                    dem_kb1++;
                    Thread.Sleep(2000);
                }
                addStatus(index, "truy cập google");

                try { driver1.Get(urlLogin); } 
                
                catch {
                    addStatus(index, "Lỗi truy cập google !");
                    driver1.Close();
                    driver1.Dispose();
                    driver1.Quit();
                    goto ketthuc;
                }

                if (driver1.Url.Contains("business.google.com/create/new"))
                {
                    goto searchz;
                }
                login st = new login(driver1, act); int demnha = 0;
                string Error = string.Empty;
            lainha:
                if (st.Nanial(urlLogin))
                {
                    if (!st.StartLogin(out Error))
                    {
                        if (Error == "captcha")
                        {
                            demnha += 1;
                            if (demnha == 7)
                            {
                                addStatus(index, "Lỗi captcha !");
                                driver1.Close();
                                driver1.Dispose();
                                driver1.Quit();
                                goto ketthuc;
                            }
                            goto lainha;
                        }

                        //Cảnh bảo lỗi
                    }
                }
                Thread.Sleep(TimeSpan.FromSeconds(3));

                addStatus(index, "Đã login mail !");
                clickDeSau(driver1); Thread.Sleep(TimeSpan.FromSeconds(5));
                if (driver1.Url.Contains("inoptions/recovery-options-collection"))
                {
                    driver1.Navigate().GoToUrl(urlLogin);
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
                if (driver1.Url.Contains("om/signin/v2/challenge/iap") || (driver1.Url.Contains("gle.com/signin/rejected")))
                {
                    saveError(act, "Very phone");
                    addStatus(index, "very phone !");
                    driver1.Close();
                    driver1.Dispose();
                    driver1.Quit();
                    goto ketthuc;
                }
                if (driver1.Url.Contains(".com/interstitials/birthday") || (driver1.Url.Contains("ogle.com/web/chip")) || (driver1.Url.Contains("/info/unknownerror")))
                {
                    addStatus(index, "Vui lòng thêm ngày sinh");
                    addbirthday(driver1);
                }
                if ((driver1.Url.Contains("m/signin/v2/identifier")) || (driver1.Url.Contains("ccounts.google.com/speedbump/idvreenable")) || (driver1.Url.Contains("m/signin/v2/disabled/explanation")))
                {
                    saveError(act, "Đăng nhập không thành công!");
                    addStatus(index, "Đăng nhập không thành công");
                    driver1.Close();
                    driver1.Dispose();
                    driver1.Quit();
                    goto ketthuc;
                }
                if (driver1.Url.Contains("ogle.com/web/chip"))
                {
                    addbirthday(driver1);
                }
            searchz:
                if (driver1.Url.Contains("/signin/v2/challenge/pwd"))
                {
                    saveError(act, "Đăng nhập không thành công");
                    addStatus(index, "Đăng nhập không thành công");
                    driver1.Close();
                    driver1.Dispose();
                    driver1.Quit();
                    goto ketthuc;
                }
                else
                {
                    acYoutube active = new acYoutube(driver1);
                    active.gotoHome();
                    Thread.Sleep(2000);
                vireyt:
                    try
                    {
                        IJavaScriptExecutor executorUseData = driver1;
                        string name = txtKeyword.Text;
                        addStatus(index, "Tìm kiếm theo key " + name);
                        active.searchKeyword(name);
                        Thread.Sleep(TimeSpan.FromSeconds(4));

                        try
                        {
                            var wait = new WebDriverWait(driver1, TimeSpan.FromSeconds(5));
                            var myElement = wait.Until(x => x.FindElement(By.XPath("//ytd-video-renderer//ytd-thumbnail//a")));
                        }
                        catch
                        {
                            addStatus(index, "lỗi load trang youtube");
                            driver1.Close();
                            driver1.Dispose();
                            driver1.Quit();
                        }
                       
                        var items = driver1.FindElements(By.XPath("//ytd-video-renderer//ytd-thumbnail//a"));
                        foreach (var item in items)
                        {
                            string urlVideo = item.GetAttribute("href");
                            if (urlVideo.Contains(strTenChannel))
                            {
                                executorUseData.ExecuteScript("arguments[0].click()", item);
                                break;
                            }
                        }
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                       
                        int rand = random.Next(3, 6);
                        int timevideo = 0;
                        for (int i = 0; i < iSoLanMoLink; i++)
                        {
                            bool is_comment = false;
                            int rand_num_cmt = random.Next(2, 5);
                            countTime time = new countTime();
                            
                            for (int k = 0; k < 300; k++)
                            {
                                if (k % 50 == 0)
                                {
                                    driver1.Navigate().Refresh();
                                    Thread.Sleep(2000);
                                }
                                timevideo = active.getTimeVideo();

                                while (timevideo < 300) // time video < 300 (s) tuc la co quang cao ngay dau
                                {
                                    Thread.Sleep(TimeSpan.FromSeconds(60));
                                    timevideo = active.getTimeVideo();
                                }
                                if (timevideo != 0)
                                {
                                    break;
                                }
                                Thread.Sleep(1500);
                            }
                            addStatus(index, "Time Video " + timevideo);
                            Thread.Sleep(3000);
                            time.reset();

                            // random comment o moi lan click link

                            Thread.Sleep(TimeSpan.FromSeconds(30));
                            addStatus(index, "Reset video ve 0");
                            // Khởi tạo đối tượng thuộc Actions class
                            Actions action = new Actions(driver1);

                            action.KeyDown(OpenQA.Selenium.Keys.NumberPad0);

                            action.SendKeys(OpenQA.Selenium.Keys.NumberPad0).Perform();
                            driver1.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(6);
                            Thread.Sleep(TimeSpan.FromSeconds(5));

                            if(i == 0)
                            {
                                rand = random.Next(3,6);
                            }
                            else
                            {
                                rand = random.Next(15, 30);
                            }
                            int sleepTime = rand * timevideo / 100;
                            addStatus(index, "Chuyen video sau : " + sleepTime + " s");
                            driver1.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(12);
                            Thread.Sleep(TimeSpan.FromSeconds(10));
                            if (sub.Checked)
                            {
                                try { active.subVideo(); }
                                catch
                                {
                                    continue;
                                }
                            Thread.Sleep(5000);
                            if (sleepTime > 1 * 60)
                            {
                                int sleepCount = sleepTime / (60);
                                int sleepCountDiv = sleepTime % 60;
                                if (sleepCountDiv > 0)
                                {
                                    sleepCount += 1;
                                }
                                for (int j = 0; j < sleepCount; j++)
                                {
                                    if (j == sleepCount - 1)
                                    {
                                        addStatus(index, sleepCountDiv.ToString());
                                        driver1.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(sleepTime + 2 - (j - 1) * 60);
                                        Thread.Sleep(TimeSpan.FromSeconds(sleepCountDiv));
                                    }
                                    else
                                    {
                                        if(i > 0)
                                        {
                                            addStatus(index, "đang xem video lần click thứ" + i);
                                        }
                                        
                                        driver1.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1 * 60);
                                        Thread.Sleep(TimeSpan.FromMinutes(1));
                                        if ((j + 1) % 20 == 0)
                                        {
                                            int x = 100;
                                            driver1.ExecuteScript("window.scrollTo(100," + (x * j + ")"));
                                            driver1.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(4);
                                            Thread.Sleep(TimeSpan.FromSeconds(2));
                                            addStatus(index, "2s");
                                            driver1.ExecuteScript("window.scrollTo({ top: 0, behavior: 'smooth' });");

                                        }
                                        if(is_comment == false)
                                            {
                                                if (i % rand_num_cmt == 0) // comment tai lan click Des random
                                                {
                                                    int rand_commenTime = random.Next(1, sleepCount); //comment tai so phut random
                                                    if(j == rand_commenTime)
                                                    {
                                                        if (listComments.Count > 0)
                                                        {
                                                            try
                                                            {
                                                                int randomCommend = random.Next(listComments.Count);
                                                                active.goToComment(listComments[randomCommend]);
                                                                is_comment = true;
                                                                Thread.Sleep(6000);
                                                            }
                                                            catch
                                                            {
                                                                continue;
                                                            }
                                                        }
                                                    }
                                                    
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                driver1.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(sleepTime + 2);
                                Thread.Sleep(TimeSpan.FromSeconds(sleepTime));

                            }
                            if (i != iSoLanMoLink - 1)
                            {
                                int x = 100;
                                driver1.ExecuteScript("window.scrollTo(100," + (x * 2 + ")"));
                                driver1.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(4);
                                Thread.Sleep(TimeSpan.FromSeconds(2));
                                addStatus(index, "2s");
                                driver1.ExecuteScript("window.scrollTo({ top: 0, behavior: 'smooth' });");

                                addStatus(index, "Sleep 5s");
                                driver1.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5 + 2);
                                Thread.Sleep(TimeSpan.FromSeconds(5));
                                addStatus(index, "Sleep 5s1");
                                try
                                {
                                    /* driver.FindElement(By.XPath("/html/body/ytd-app/div[1]/ytd-page-manager/ytd-watch-flexy/div[5]/div[1]/div/div[2]/ytd-watch-metadata/div/div[3]/div[1]/div/ytd-text-inline-expander/div[1]/span[1]/yt-formatted-string/a[1]")).Click();*/
                                    var eleDes = driver1.FindElement(By.XPath("//div[@id='description']"));
                                    executorUseData.ExecuteScript("arguments[0].click()", eleDes);
                                    Thread.Sleep(2000);
                                    string contentDes = driver1.FindElement(By.XPath("//div[@id='description']")).Text;
                                    string[] lines = contentDes.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                                  
                                    string linkDes = lines.FirstOrDefault(t => t.StartsWith("http", StringComparison.OrdinalIgnoreCase));

                                    if (string.IsNullOrWhiteSpace(linkDes))
                                    {
                                        iSoLuongDangChay--;
                                        addStatus(index, "Xong");
                                        try
                                        {
                                            driver1.Close();
                                            driver1.Quit();
                                        }
                                        catch
                                        {
                                            driver1.Navigate().GoToUrl(linkDes);
                                        }
                                    }
                                    else
                                    {
                                        addStatus(index, "Chạy link ở des: " + i);
                                        driver1.Navigate().GoToUrl(linkDes);
                                    }
                                }
                                catch (Exception e)
                                {
                                    addStatus(index, "Sleep 5s1 ERROR" + e);
                                }
                                addStatus(index, "Sleep 5s2");
                                driver1.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5 + 2);
                                Thread.Sleep(TimeSpan.FromSeconds(5));
                            }
                        }
                        addStatus(index, "Xong");
                        driver1.Close();
                        driver1.Dispose();
                        driver1.Quit();
                    }
                    catch (Exception ex) // catch 1
                    {
                        addStatus(index, "timeout access youtube ");
                        driver1.Close();
                        driver1.Dispose();
                        driver1.Quit();
                        goto ketthuc;
                    }
                }
        }catch (Exception ex) // catch 1
            {
                addStatus(index, ex.Message.ToString());
                goto ketthuc;
            }
        ketthuc:
            runNew(1);
            return;
        }

        UndetectChromeDriver driver3 = null;
        private void kichban3()
        {

            int dem_kb3 = 0;
            int index = numberRow;
            numberRow += 1;
            if (index >= dataGrid.Rows.Count)
            {
                return;
            }
            if (!checkopentab)
            {
                return;
            }
            account act = lstAccount[index];

            driver3 = null;

            try
            {
                addStatus(index, "starting");
                GPMLoginAPI api = new GPMLoginAPI("http://" + APP_URL.Text);
                acton sts = new acton(act, api);
                
                Thread.Sleep(1000);
                JObject ojb = sts.getLst(act.email, profiles);
                string id_profile = "";
                if (ojb == null)
                {
                    int prox = proxyNumber;
                    loadProxy();
                    proxyNumber += 1;
                    if (prox >= lstProxy.Count)
                    {
                        proxyNumber = 0;
                        prox = proxyNumber;
                    }
                    string proxy = lstProxy[prox];
                    resave(proxy);
                    
                    ojb = api.Create(act.email, proxy, true);
                    if (ojb != null)
                    {
                        //Tạo thành công
                        id_profile = ojb["profile_id"].ToString();
                        addStatus(index, "tạo profile thành công");
                        saveProfile(act, proxy);
                    }
                }
                else
                {
                    //đã có profile
                    id_profile = ojb["id"].ToString();
                }
                Thread.Sleep(1000);

                bool lockWasTaken = false;
                var temp = obj;
                try
                {
                    Monitor.Enter(temp, ref lockWasTaken);
                    addStatus(index, "đang mở profile");
                    try { driver3 = sts.openProfile(id_profile, index); }
                    catch
                    {
                        addStatus(index, "Lỗi mở profile");
                        saveError(act, "Lỗi mở profile");
                        goto ketthuc;
                    }
                }
                finally
                {
                    if (lockWasTaken)
                    {
                        Monitor.Exit(temp);
                    }
                }
                while (dem_kb3 < nbThread.Value - 1)
                {
                    dem_kb3++;
                    Thread.Sleep(2000);
                }
                addStatus(index, "truy cập google");

                try { driver3.Get(urlLogin); } 

                catch {
                    addStatus(index, "Lỗi truy cập google !");
                    driver3.Close();
                    driver3.Dispose();
                    driver3.Quit();
                    goto ketthuc;
                }
                Thread.Sleep(2000);
                
                if (driver3.Url.Contains("business.google.com/create/new"))
                {
                    goto searchz;
                }
                login st = new login(driver3, act); int demnha = 0;
                string Error = string.Empty;
            lainha:
                if (st.Nanial(urlLogin))
                {
                    if (!st.StartLogin(out Error))
                    {
                        if (Error == "captcha")
                        {
                            demnha += 1;
                            if (demnha == 7)
                            {
                                addStatus(index, "Lỗi captcha !");
                                driver3.Close();
                                driver3.Dispose();
                                driver3.Quit();
                                goto ketthuc;
                            }
                            goto lainha;
                        }

                        //Cảnh bảo lỗi
                    }
                }
                Thread.Sleep(TimeSpan.FromSeconds(3));

                addStatus(index, "Đã login mail !");
                clickDeSau(driver3); Thread.Sleep(TimeSpan.FromSeconds(5));
                if (driver3.Url.Contains("inoptions/recovery-options-collection"))
                {
                    driver3.Navigate().GoToUrl(urlLogin);
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
                if (driver3.Url.Contains("om/signin/v2/challenge/iap") || (driver3.Url.Contains("gle.com/signin/rejected")))
                {
                    saveError(act, "Very phone");
                    addStatus(index, "very phone !");
                    driver3.Close();
                    driver3.Dispose();
                    driver3.Quit();
                    goto ketthuc;
                }
                if (driver3.Url.Contains(".com/interstitials/birthday") || (driver3.Url.Contains("ogle.com/web/chip")) || (driver3.Url.Contains("/info/unknownerror")))
                {
                    addStatus(index, "Vui lòng thêm ngày sinh");
                    addbirthday(driver3);
                }
                if ((driver3.Url.Contains("m/signin/v2/identifier")) || (driver3.Url.Contains("ccounts.google.com/speedbump/idvreenable")) || (driver3.Url.Contains("m/signin/v2/disabled/explanation")))
                {
                    saveError(act, "Đăng nhập không thành công!");
                    addStatus(index, "Đăng nhập không thành công");
                    driver3.Close();
                    driver3.Dispose();
                    driver3.Quit();
                    goto ketthuc;
                }
                if (driver3.Url.Contains("ogle.com/web/chip"))
                {
                    addbirthday(driver3);
                }
            searchz:
                if (driver3.Url.Contains("/signin/v2/challenge/pwd"))
                {
                    saveError(act, "Đăng nhập không thành công");
                    addStatus(index, "Đăng nhập không thành công");
                    driver3.Close();
                    driver3.Dispose();
                    driver3.Quit();
                    goto ketthuc;
                }
                else
                {
                    acYoutube active = new acYoutube(driver3);
                    active.gotoHome();
                    Thread.Sleep(2000);
                vireyt:
                    try
                    {
                        IJavaScriptExecutor executorUseData = driver3;
                        string name = txtKeyword.Text;
                        addStatus(index, "Tìm kiếm theo key " + name);
                        active.searchKeyword(name);
                        Thread.Sleep(TimeSpan.FromSeconds(4));

                        try
                        {
                            var wait = new WebDriverWait(driver1, TimeSpan.FromSeconds(5));
                            var myElement = wait.Until(x => x.FindElement(By.XPath("//ytd-video-renderer//ytd-thumbnail//a")));
                        }
                        catch
                        {
                            addStatus(index, "lỗi load trang youtube");
                            driver3.Close();
                            driver3.Dispose();
                            driver3.Quit();
                        }

                        var items = driver3.FindElements(By.XPath("//ytd-video-renderer//ytd-thumbnail//a"));
                        foreach (var item in items)
                        {
                            string urlVideo = item.GetAttribute("href");
                            if (urlVideo.Contains(strTenChannel))
                            {
                                executorUseData.ExecuteScript("arguments[0].click()", item);
                                break;
                            }
                        }
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                        countTime time = new countTime();

                        int timevideo = 0;
                        for (int i = 0; i < 300; i++)
                        {
                            if (i % 50 == 0)
                            {
                                driver3.Navigate().Refresh();
                                Thread.Sleep(2000);
                            }
                            timevideo = active.getTimeVideo();
                            while (timevideo < 300) // time video < 300 (s) tuc la co quang cao ngay dau
                            {
                                Thread.Sleep(TimeSpan.FromSeconds(60));
                                timevideo = active.getTimeVideo();
                            }
                            if (timevideo != 0)
                            {
                                break;
                            }
                            Thread.Sleep(1500);
                        }
                        addStatus(index, "Time Video " + timevideo);
                       
                        time.reset();
               
                        driver3.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
                        Thread.Sleep(TimeSpan.FromSeconds(30));
                        addStatus(index, "reset video time về 0");

                        Actions action = new Actions(driver3);

                        action.KeyDown(OpenQA.Selenium.Keys.NumberPad0);

                        action.SendKeys(OpenQA.Selenium.Keys.NumberPad0).Perform();

                        int rand = random.Next(3, 6);
                        int sleepTime = rand * timevideo / 100;
                        addStatus(index, "Chuyen video sau : " + sleepTime + " s");
                        driver3.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(12);
                        Thread.Sleep(TimeSpan.FromSeconds(10));

                        //Sleep 1min
                        //Scroll after 20min
                        if (sleepTime > 1 * 60)
                        {
                            int sleepCount = sleepTime / (60);
                            int sleepCountDiv = sleepTime % 60;
                            if (sleepCountDiv > 0)
                            {
                                sleepCount += 1;
                            }
                            for (int j = 0; j < sleepCount; j++)
                            {
                                if (j == sleepCount - 1)
                                {
                                    addStatus(index, sleepCountDiv.ToString());
                                    driver3.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(sleepTime + 2 - (j - 1) * 60);
                                    Thread.Sleep(TimeSpan.FromSeconds(sleepCountDiv));
                                }
                                else
                                {
                                    addStatus(index, "60s");
                                    driver3.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1 * 60);
                                    Thread.Sleep(TimeSpan.FromMinutes(1));
                                    if ((j + 1) % 20 == 0)
                                    {
                                        int x = 100;
                                        driver3.ExecuteScript("window.scrollTo(100," + (x * j + ")"));
                                        driver3.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(4);
                                        Thread.Sleep(TimeSpan.FromSeconds(2));
                                        addStatus(index, "2s");
                                        driver3.ExecuteScript("window.scrollTo({ top: 0, behavior: 'smooth' });");

                                    }

                                }
                            }
                        }
                        else
                        {
                            driver3.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(sleepTime + 2);
                            Thread.Sleep(TimeSpan.FromSeconds(sleepTime));

                        }
                        Thread.Sleep(5000);
                        //
                        /*driver.FindElement(By.XPath("/html/body/ytd-app/div[1]/ytd-page-manager/ytd-watch-flexy/div[5]/div[1]/div/div[2]/ytd-watch-metadata/div/div[3]/div[1]/div/ytd-text-inline-expander/div[1]/span[1]/yt-formatted-string/a[1]")).Click();*/

                        var eleDes = driver3.FindElement(By.XPath("//div[@id='description']"));
                        executorUseData.ExecuteScript("arguments[0].click()", eleDes);
                        Thread.Sleep(2000);
                        string contentDes = driver3.FindElement(By.XPath("//div[@id='description']")).Text;
                        string[] lines = contentDes.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                        string linkDes = lines.FirstOrDefault(t => t.StartsWith("http", StringComparison.OrdinalIgnoreCase));

                        if (string.IsNullOrWhiteSpace(linkDes))
                        {
                            iSoLuongDangChay--;
                            addStatus(index, "Xong");
                            try
                            {
                                driver3.Close();
                                driver3.Quit();
                                driver3.Dispose();
                            }
                            catch
                            {
                                driver3.Navigate().GoToUrl(linkDes);
                            }
                        }
                        else
                        {
                            addStatus(index, "Chuyển sang click PLL");
                            driver3.Navigate().GoToUrl(linkDes);
                        }
                        Thread.Sleep(10000);
                        // xem pll 
                        int videoNext = 0;
                       
                        for(int i = 0; i < 1000; i++)
                        {
                            bool is_comment = false;
                            videoNext += 1;

                            timevideo = active.getTimeVideo();

                            while (timevideo < 300) // time video < 300 (s) tuc la co quang cao ngay dau
                            {
                                Thread.Sleep(TimeSpan.FromSeconds(60));
                                timevideo = active.getTimeVideo();
                            }

                            int rand_video_next = random.Next(3, 8);
                            if (videoNext == rand_video_next)
                            {
                                if (sub.Checked)
                                {
                                    try { active.subVideo(); }
                                    catch
                                    {
                                        continue;
                                    }
                                }
                            }
                            addStatus(index, "Reset video ve 0");
                            // Khởi tạo đối tượng thuộc Actions class
                            action = new Actions(driver3);

                            action.KeyDown(OpenQA.Selenium.Keys.NumberPad0);

                            action.SendKeys(OpenQA.Selenium.Keys.NumberPad0).Perform();
                            driver3.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(6);
                            Thread.Sleep(TimeSpan.FromSeconds(5));

                            rand = random.Next(15, 30);

                            sleepTime = rand * timevideo / 100;
                            addStatus(index, "Chuyen video sau : " + sleepTime + " s");
                            driver3.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(12);
                            Thread.Sleep(TimeSpan.FromSeconds(10));

                            //Sleep 1min
                            //Scroll after 20min
                            if (sleepTime > 1 * 60)
                            {
                                int sleepCount = sleepTime / (60);
                                int sleepCountDiv = sleepTime % 60;
                                if (sleepCountDiv > 0)
                                {
                                    sleepCount += 1;
                                }
                                int rand_cmt_time = random.Next(2, sleepCount);
                                for (int j = 0; j < sleepCount; j++)
                                {
                                    if (j == sleepCount - 1)
                                    {
                                        addStatus(index, sleepCountDiv.ToString());
                                        driver3.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(sleepTime + 2 - (j - 1) * 60);
                                        Thread.Sleep(TimeSpan.FromSeconds(sleepCountDiv));
                                    }
                                    else
                                    {
                                        addStatus(index, "60s");
                                        driver3.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1 * 60);
                                        Thread.Sleep(TimeSpan.FromMinutes(1));
                                        if ((j + 1) % 20 == 0)
                                        {
                                            int x = 100;
                                            driver3.ExecuteScript("window.scrollTo(100," + (x * j + ")"));
                                            driver3.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(4);
                                            Thread.Sleep(TimeSpan.FromSeconds(2));
                                            addStatus(index, "2s");
                                            driver3.ExecuteScript("window.scrollTo({ top: 0, behavior: 'smooth' });");
                                        }
                                        if (is_comment == false)
                                        {
                                            int rand_video_cmt = random.Next(2, 6);
                                            Thread.Sleep(10000);
                                            if (videoNext % rand_video_cmt == 0)
                                            {
                                                if (j == rand_cmt_time)
                                                {
                                                    if (listComments.Count > 0)
                                                    {
                                                        try
                                                        {
                                                            int randomCommend = random.Next(listComments.Count);
                                                            active.goToComment(listComments[randomCommend]);
                                                            is_comment = true;
                                                            Thread.Sleep(6000);
                                                        }
                                                        catch
                                                        {
                                                            continue;
                                                        }
                                                    }
                                                }

                                            }

                                        }
                                    }
                                }
                            }
                            else
                            {
                                driver3.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(sleepTime + 2);
                                Thread.Sleep(TimeSpan.FromSeconds(sleepTime));
                            }
                            int tr = random.Next(0, 2);
                            if (videoNext == 1)
                            {
                                tr = 1;
                            }
                            if (tr == 0)
                            {
                                try { driver3.ExecuteScript("document.getElementsByClassName('ytp-prev-button ytp-button')[0].click()"); }
                                catch
                                {
                                    Thread.Sleep(1000);
                                }
                                //Back
                            }
                            else
                            {
                                //forword
                                try { driver3.ExecuteScript("document.getElementsByClassName('ytp-next-button ytp-button')[0].click()"); }
                                catch
                                {
                                    Thread.Sleep(1000);
                                }
                            }
                            Thread.Sleep(10000);
                            if (i == 999)
                            {
                                addStatus(index, "xem hết pll");
                                driver3.Close();
                                driver3.Dispose();
                                driver3.Quit();
                                return;
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        addStatus(index, "error time out access youtube");
                        driver3.Close();
                        driver3.Dispose();
                        driver3.Quit();
                        goto ketthuc;
                        
                    }
                }

            }
            catch (Exception ex)
            {
                addStatus(index, ex.Message.ToString());
                goto ketthuc;
            }
        ketthuc:
            runNew(3);
            return;
        }
        void saveProfile(account taikhoan, string proxy = "")
        {
            while (true)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(proxy))
                    {
                        File.AppendAllText("data\\info_profile.txt", taikhoan.email + "|" + taikhoan.password + "|" + taikhoan.mail_kp + "\r\n");
                    }
                    else
                    {
                        File.AppendAllText("data\\info_profile.txt", taikhoan.email + "|" + taikhoan.password + "|" + taikhoan.mail_kp + "|" + proxy + "\r\n");
                    }
                    break;
                }
                catch
                {
                    Thread.Sleep(100);
                }
            }
        }
        bool flag_stop = false;
        List<Thread> listThread = new List<Thread>();
        void runNew(int indexKichBan)
        {
            Thread st = new Thread(() =>
            {
                if(flag_stop == false)
                {
                    run(indexKichBan);
                }
            });
            st.IsBackground = true;
            st.Start();
            listThread.Add(st);
        }
        void clickGotit(UndetectChromeDriver driver)
        {
            try { driver.FindElement(By.XPath("//yt-upsell-dialog-renderer//tp-yt-paper-button//yt-formatted-string")).Click(); } catch { }
        }
        int countTimeAll = 0;
        void addLink(string link, int time)
        {

        }
        void save()
        {

        }
        string ConvertListToString()
        {
            string data = "";
            foreach (var item in lstLink.ToList())
            {
                data += item.link_yt + "|" + item.count + "\r\n";
            }
            return data;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
        }
        void savep()
        {
            try { File.WriteAllText("data\\phut.txt", countTimeAll.ToString()); } catch { Thread.Sleep(100); }
        }
        List<link> lstLink;

        private void btnStop_Click_1(object sender, EventArgs e)
        {
            flag_stop = true;
            clearchrome();
            foreach(Thread t in listThread)
            {
                t.Abort();
            }
            listThread = new List<Thread>();
        }
        void clearchrome()
        {

           Process[] chromeDriverProcesses = Process.GetProcessesByName("gpmdriver.exe");
            foreach (var chromeDriverProcess in chromeDriverProcesses)
            {
                try { chromeDriverProcess.Kill(); } catch { }
            }
            Process[] chromed = Process.GetProcessesByName("chrome");
            foreach (var chrome in chromed)
            {
                try { chrome.Kill(); } catch { }
            }

            Process.Start("taskkill", "/F /IM gpmdriver.exe");
            Process.Start("taskkill", "/F /IM chromedriver.exe");

            flag_view_dx = false;
            flag_view_dx = false;
            flag_view_dx = false;
        }
        bool checkopentab = true;

        private int iSoLuongEmail;
        private int iSoLuong;
        private int iSoLuongDangChay;
        private int iIndexDangChay;
        private string strKeyWork;
        private string strTenChannel;
        private int iSoLanMoLink = 0;


        private void btnSelectCommentFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Text file (*.txt)|*.txt";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    listComments = File.ReadAllLines(openFileDialog.FileName).ToList();
                }
            }
        }
        

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }
        bool flag_view_dx = false;
        bool flag_view_pll = false;
        bool flag_viewPll_video = false;
        private void btnViewDX_CheckedChanged(object sender, EventArgs e)
        {
            flag_view_dx = true;
        }

        private void btnPLL_CheckedChanged_1(object sender, EventArgs e)
        {
            flag_view_pll = true;
        }

        private void btnPLL_Video_CheckedChanged(object sender, EventArgs e)
        {
            flag_viewPll_video = true;
        }

        private void btnWait_Click(object sender, EventArgs e)
        {
            checkopentab = !checkopentab;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            flag_stop = false;
            iSoLuongDangChay = 0;
            iSoLuongEmail = dataGrid.Rows.Count;
            if (iSoLuongEmail == 0)
            {
                return;
            }
            if (iSoLuongEmail < nbThread.Value)
            {
                nbThread.Value = iSoLuongEmail;
            }
            iSoLuong = (int)nbThread.Value;
            iIndexDangChay = 0;
            strKeyWork = txtKeyword.Text.Trim();
            strTenChannel = txtChannel.Text.Trim();
            iSoLanMoLink = int.Parse(txtSoLanMoLink.Text);
            if (flag_view_dx == true)
            {   
                Thread st = new Thread(() =>
                {
                    createThread(1);
                });
                st.IsBackground = true;
                st.Start();
                listThread.Add(st);
            }
            else if(flag_view_pll == true)
            {
                Thread st = new Thread(() =>
                {
                    createThread(2);
                });
                st.IsBackground = true;
                st.Start();
                listThread.Add(st);
            }
            else if(flag_viewPll_video == true)
            {
                Thread st = new Thread(() =>
                {
                    createThread(3);
                });
                st.IsBackground = true;
                st.Start();
                listThread.Add(st);
            }
            else
            {
                MessageBox.Show("Hãy Chọn Loại View");

            }
        }

        private void sub_CheckedChanged(object sender, EventArgs e)
        {

        }
       

    }
}
