using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OtpNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LoginFaceBook
{
    class MyChrome
    {
        public IWebDriver driver;
        public bool LoggedFB = false;


        public const string CLICK = "CLICK";
        public const string SEND_KEYS = "SEND_KEYS";
        public const string CLEAR = "CLEAR";
        public const string GET_TEXT = "GET_TEXT";
        public const string GET_INNER_HTML = "GET_INNER_HTML";
        public const string GET_OUTER_HTML = "GET_OUTER_HTML";
        public const string SWITCH_FRAME = "SWITCH_FRAME";
        public const string SWITCH_DEFAULT = "SWITCH_DEFAULT";

        public MyChrome()
        {
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--disable-notifications");
            options.AddArgument("--window-size=1300,1000");
            options.AddArguments("--disable-extensions");
            //options.AddArgument("--window-position=-32000,-32000");
            //options.AddArguments("--proxy-server=");
            driver = new ChromeDriver(service, options);
        }

        public string ElementAction(string action, string xpath = "", int index = 0, string text = "", int waits = 60, int delay = 0, bool is_exception = true)
        {
            if (action.Equals(SWITCH_FRAME))
            {
                driver.SwitchTo().Frame(driver.FindElements(By.XPath(xpath))[index]);
                Sleep(delay);
                return "";
            }
            int wait = 0;
            while (driver.FindElements(By.XPath(xpath)).Count == 0 && wait < waits)
            {
                Sleep(1);
                wait++;
            }
            if (wait == waits)
            {
                if (is_exception)
                {
                    throw new Exception(string.Format("Xpath not found: {0}", xpath));
                }
                else
                {
                    return "";
                }
            }
            if (index < 0)
            {
                index += driver.FindElements(By.XPath(xpath)).Count;
            }
            switch (action)
            {
                case CLICK:
                    try
                    {
                        driver.FindElements(By.XPath(xpath))[index].Click();
                    }
                    catch
                    {
                        IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
                        executor.ExecuteScript("arguments[0].click();", driver.FindElements(By.XPath(xpath))[index]);
                    }
                    Sleep(delay);
                    return "";
                case CLEAR:
                    driver.FindElements(By.XPath(xpath))[index].Clear();
                    Sleep(delay);
                    return "";
                case SEND_KEYS:
                    driver.FindElements(By.XPath(xpath))[index].SendKeys(text);
                    Sleep(delay);
                    return "";
                case GET_TEXT:
                    string t = driver.FindElements(By.XPath(xpath))[index].Text;
                    Console.WriteLine("t: " + t);
                    Sleep(delay);
                    return t;
                case GET_INNER_HTML:
                    string innerHTML = driver.FindElements(By.XPath(xpath))[index].GetAttribute("innerHTML");
                    Sleep(delay);
                    return innerHTML;
                case GET_OUTER_HTML:
                    string outerHTML = driver.FindElements(By.XPath(xpath))[index].GetAttribute("outerHTML");
                    Sleep(delay);
                    return outerHTML;
                case SWITCH_FRAME:
                    driver.SwitchTo().Frame(driver.FindElements(By.XPath(xpath))[index]);
                    Sleep(delay);
                    return "";
                case SWITCH_DEFAULT:
                    driver.SwitchTo().DefaultContent();
                    Sleep(delay);
                    return "";
                default: return "";
            }
        }

        public void Login(string account, string password, string twofa)
        {
            string url = "https://www.facebook.com/";
            driver.Navigate().GoToUrl(url);
            ElementAction(SEND_KEYS, "//input[@name='email']", text: account);
            ElementAction(SEND_KEYS, "//input[@name='pass']", text: password);
            ElementAction(CLICK, "//button[@name='login']");

            Sleep(4);
            try
            {
                if (driver.FindElements(By.XPath("//input[@id='approvals_code']")).Count == 1)
                {
                    // A 2FA seed code was passed, let's generate the 2FA code
                    twofa = twofa.Trim().Replace(" ", "");
                    var otpKeyBytes = Base32Encoding.ToBytes(Convert.ToString(twofa));
                    var totp = new Totp(otpKeyBytes);
                    string twoFactorCode = totp.ComputeTotp();
                    // Enter the code into the UI
                    ElementAction(SEND_KEYS, "//input[@id='approvals_code']", text: twoFactorCode);
                    ElementAction(CLICK, "//button[@id='checkpointSubmitButton']");
                    Sleep(1);
                    ElementAction(CLICK, "//button[@id='checkpointSubmitButton']");
                }
            }
            catch (Exception)
            {

            }

            Sleep(2);
            LoggedFB = !driver.Url.Contains("login") && !driver.Url.Contains("checkpoint");
        }

        private void Sleep(double seconds)
        {
            Thread.Sleep(Convert.ToInt32(seconds * 1000));
        }

        public void Quit()
        {
            Sleep(5);
            driver.Quit();
        }
    }
}
