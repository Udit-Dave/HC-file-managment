
using HCFileCorrection.Entities;
using HCFileCorrection.Interfaces;
using HCFileCorrection.Models;
using HCFileCorrection.Services;
using HCFileCorrection.Utility;
using Microsoft.AspNetCore.Http.HttpResults;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V120.Debugger;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using OtpNet;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Security.Policy;
using System.Xml.Linq;


namespace HCFileCorrection.Selenium
{
    public class FileDownloadJob
    {
        private readonly GetOtp genotp;
        private readonly IHCRepository _repository;
        public FileDownloadJob(GetOtp getotp, IHCRepository repository)
        {
            genotp = getotp;
            _repository = repository;
        }

        /*IWebDriver driver = null;*/
        public async void NavigateToVendorCentral(IWebDriver driver, DTHCPOSVendorPortalConfig configDetails)
        {
            string vendorPortalLink = configDetails.VendorPortalLink;
            try
            {
                driver.Navigate().GoToUrl(vendorPortalLink);
                Thread.Sleep(2000);

                Console.WriteLine("Navigated To Vendor Central Page");

                IWebElement emailelement = driver.FindElement(By.Id("ap_email"));
                Thread.Sleep(2000);

                emailelement.SendKeys(configDetails.UserName);
                Thread.Sleep(2000);
                Console.WriteLine("Entered the User Name");

                IWebElement passelement = driver.FindElement(By.Id("ap_password"));
                passelement.SendKeys(configDetails.Password);
                Console.WriteLine("Entered the Password");
                IWebElement loginButton = driver.FindElement(By.Id("signInSubmit"));
                loginButton.Click();
                Thread.Sleep(2000);
                Console.WriteLine("Clicked on Login button");
            }
            catch (NoSuchElementException ex)
            {
                throw new Exception(ex.Message);
            }
            try
            {

                IWebElement radbutton = driver.FindElement(By.ClassName("auth-TOTP"));
                radbutton.Click();
                Thread.Sleep(2000);
                Console.WriteLine("Selected OTP Option");

                IWebElement otpenterbutton = driver.FindElement(By.Id("auth-send-code"));
                otpenterbutton.Click();
                Thread.Sleep(2000);


                IWebElement otpelement1 = driver.FindElement(By.Id("auth-mfa-otpcode"));
                otpelement1.Click();
                Thread.Sleep(2000);

                var key1 = genotp.Decrypt(configDetails.OtpString);
                otpelement1.SendKeys(key1);
                Thread.Sleep(2000);
                Console.WriteLine("Entered OTP");

                IWebElement enterpage = driver.FindElement(By.Id("auth-signin-button"));
                enterpage.Click();
                Thread.Sleep(2000);
                Console.WriteLine("Clicked on Sign in button");


            }
            catch (NoSuchElementException)
            {
                IWebElement otpelement = driver.FindElement(By.Id("auth-mfa-otpcode"));
                otpelement.Click();
                Thread.Sleep(2000);

                var key = genotp.Decrypt(configDetails.OtpString);
                otpelement.SendKeys(key);
                Console.WriteLine("Entered otp");

                IWebElement signin = driver.FindElement(By.Id("auth-signin-button"));
                signin.Click();
                Thread.Sleep(2000);
                Console.WriteLine("Clicked on signin button");

            }
            try
            {
                WebDriverWait wait01 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                /*string url = "https://vendorcentral.amazon.it/account/choose?returnTo=%2F%3Fopenid.assoc_handle%3Damzn_vc_it_v2%26openid.claimed_id%3Dhttps%253A%252F%252Fvendorcentral.amazon.it%252Fap%252Fid%252Famzn1.account.AEVSXV5VR73HFQGJ672LKAOPDFGQ%26openid.identity%3Dhttps%253A%252F%252Fvendorcentral.amazon.it%252Fap%252Fid%252Famzn1.account.AEVSXV5VR73HFQGJ672LKAOPDFGQ%26openid.mode%3Did_res%26openid.ns%3Dhttp%253A%252F%252Fspecs.openid.net%252Fauth%252F2.0%26openid.op_endpoint%3Dhttps%253A%252F%252Fvendorcentral.amazon.it%252Fap%252Fsignin%26openid.response_nonce%3D2024-06-15T05%253A32%253A39Z-881823710800616461%26openid.return_to%3Dhttps%253A%252F%252Fvendorcentral.amazon.it%252F%26openid.signed%3Dassoc_handle%252Cclaimed_id%252Cidentity%252Cmode%252Cns%252Cop_endpoint%252Cresponse_nonce%252Creturn_to%252Cns.pape%252Cpape.auth_policies%252Cpape.auth_time%252Csigned%26openid.ns.pape%3Dhttp%253A%252F%252Fspecs.openid.net%252Fextensions%252Fpape%252F1.0%26openid.pape.auth_policies%3DSinglefactorWithPossessionChallenge%26openid.pape...";
                string currentUrl = driver.Url;
                Uri uri1 = new Uri(Uri.UnescapeDataString(url));
                Uri uri2 = new Uri(Uri.UnescapeDataString(currentUrl));*/
                IWebElement h1Element = driver.FindElement(By.CssSelector("h1[data-v-47be5af2]"));
                string h1Text = h1Element.Text;
                if (h1Text == "Select an account")
                {
                    WebDriverWait wait0 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    IWebElement divElement = wait0.Until(ExpectedConditions.ElementIsVisible(By.ClassName("full-page-account-switcher-column")));
                    IList<IWebElement> buttons = divElement.FindElements(By.TagName("button"));
                    buttons[2].Click();

                    IWebElement selectAccountButton = driver.FindElement(By.CssSelector("button.kat-button.kat-button--primary.kat-button--base"));

                    selectAccountButton.Click();
                }
            }

            catch(NoSuchElementException) 
            {

                //WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                //IWebElement popupElement = wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("take-tour-dialog-content-ctas-tertiary")));

                //IWebElement laterTourButton = driver.FindElement(By.ClassName("take-tour-dialog-content-ctas-tertiary"));
                //laterTourButton.Click();
                //Thread.Sleep(2000);
            }
            finally 
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
                IWebElement popupElement = wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("take-tour-dialog-content-ctas-tertiary")));

                IWebElement laterTourButton = driver.FindElement(By.ClassName("take-tour-dialog-content-ctas-tertiary"));
                laterTourButton.Click();
                Thread.Sleep(2000);
            }

        }

        public bool NavigateToTab(IWebDriver driver, string Tab, string period, DateTime? startDate, DTHCPOSVendorPortalConfig configDetails)
        {
            if(Tab == "catalogue")
            {
                Tab = "catalog";
            }
            string vendorPortalLink = configDetails.VendorPortalLink;
            


            if (Tab == "sales" || Tab == "traffic" || Tab == "inventory")
            {


                /*driver.Navigate().GoToUrl($"{vendorPortalLink}/retail-analytics/dashboard/{Tab.ToLower()}");
                Thread.Sleep(2000);

                WebDriverWait wait01 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                IWebElement timeperiod = wait01.Until(ExpectedConditions.ElementIsVisible(By.Id("time-period")));
                Thread.Sleep(2000);

                IWebElement dropdown = driver.FindElement(By.Id("time-period"));
                dropdown.Click();
                Thread.Sleep(2000);

                IWebElement shadowRoot = (IWebElement)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].shadowRoot.querySelector('.kat-select-container');", dropdown);

                IWebElement innerContainer = shadowRoot.FindElement(By.CssSelector(".option-inner-container"));
                Thread.Sleep(2000);

                IList<IWebElement> selectdropoptions = innerContainer.FindElements(By.TagName("kat-option"));
                foreach (var option in selectdropoptions)
                {
                    if (option.Text.Trim() == period)
                    {
                        option.Click();
                        Thread.Sleep(2000);

                        break;
                    }
                }*/
                switch (period)
                {
                    case "Daily":

                        DateTime date = startDate.Value;
                        string startdate = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        
                        driver.Navigate().GoToUrl($"{vendorPortalLink}/retail-analytics/dashboard/{Tab.ToLower()}?daily-day={startdate}&time-period=daily");
                    
                        Thread.Sleep(5000);
                        Console.WriteLine($"Navigate to {Tab} page");
                        /*WebDriverWait wait2 = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
                        IWebElement dailydate = wait2.Until(ExpectedConditions.ElementIsVisible(By.Id("daily-day")));

                        IWebElement dailyday = driver.FindElement(By.Id("daily-day"));
                        dailyday.Click();
                        Thread.Sleep(2000);

                        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                        js.ExecuteScript("arguments[0].value = '';", dailyday);
                        Thread.Sleep(2000);

                        DateTime date = startDate.Value;
                        string dateString = date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                        dailyday.SendKeys(dateString);
                        dailyday.SendKeys(Keys.Enter);*/

                        break;

                    case "Weekly":

                        /*IWebElement dropdown11 = driver.FindElement(By.Id("weekly-week"));
                        dropdown11.Click();
                        Thread.Sleep(2000);*/
                    
                        DateTime date0 = startDate.Value;
                        string dateString0 = date0.AddDays(-6).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        string dateString1 = dateString0 +"to"+ date0.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        driver.Navigate().GoToUrl($"{vendorPortalLink}/retail-analytics/dashboard/{Tab.ToLower()}?time-period=weekly&weekly-week={dateString1}");
                        Thread.Sleep(5000);

                        /*IWebElement shadowRoot11 = (IWebElement)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].shadowRoot.querySelector('.select-options');", dropdown11);

                        IWebElement innerContainer11 = shadowRoot11.FindElement(By.CssSelector(".option-inner-container"));
                        Thread.Sleep(2000);

                        IList<IWebElement> selectdropoptions11 = innerContainer11.FindElements(By.TagName("kat-option"));
                        foreach (var options in selectdropoptions11)
                        {
                            var selectoption = selectdropoptions11[0];
                            selectoption.Click();
                            Thread.Sleep(2000);

                            break;
                        }*/
                        break;

                   
                }
                IWebElement customcolumns1 = driver.FindElement(By.ClassName("css-4g6ai3"));
                customcolumns1.Click();
                Thread.Sleep(5000);

                IWebElement container1 = driver.FindElement(By.CssSelector(".ltr-mlseai"));
                ReadOnlyCollection<IWebElement> checkboxes = container1.FindElements(By.TagName("kat-checkbox"));
                Thread.Sleep(2000);
                int checkbox1 = 3;
                checkboxes[checkbox1].Click();
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

                
                js.ExecuteScript($"arguments[0].querySelectorAll('kat-checkbox')[{checkbox1}].checked = false;", container1);
                
                /*Thread.Sleep(3000);*/
                WebDriverWait wait7 = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
                wait7.Until(ExpectedConditions.ElementIsVisible(By.Id("customizeColumnsSaveButton")));
                IWebElement save1 = driver.FindElement(By.Id("customizeColumnsSaveButton"));
                save1.Click();
                Thread.Sleep(4000);

                IWebElement apply6 = driver.FindElement(By.ClassName("ltr-1cfden2"));
                apply6.Click();
                Thread.Sleep(4000);

                IWebElement buttoncontainer1 = driver.FindElement(By.TagName("kat-button-group"));
                string buttonLabel1 = "CSV";

                IJavaScriptExecutor js0 = (IJavaScriptExecutor)driver;
                js0.ExecuteScript($"arguments[0].querySelector('kat-button[label=\"{buttonLabel1}\"]').click();", buttoncontainer1);

                Thread.Sleep(5000);

                IWebElement downloadmanager1 = driver.FindElement(By.Id("downloadManager"));
                downloadmanager1.Click();
                Thread.Sleep(2000);


                WebDriverWait wait5 = new WebDriverWait(driver, TimeSpan.FromSeconds(300));
                wait5.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("kat-table-body kat-table-row:first-child a")));

                IWebElement progress = driver.FindElement(By.CssSelector("kat-table-body kat-table-row:first-child a"));
                if (progress != null)
                {
                    Console.WriteLine("The Requested File Added in Download Manager");
                    return true;
                }
                else
                {
                    Console.WriteLine("The Requested File Failed To Add In Download Manager");
                    return false;
                }
            }
            else if (Tab == "publisherreportalc" || Tab == "publisherreportsubscription")
            {
                return true;
            }
            else if (Tab.ToLower() == "forecast - mean" || Tab.ToLower() == "forecast - p70" || Tab.ToLower() == "forecast - p80" || Tab.ToLower() == "forecast - p90" || Tab.ToLower() == "catalog")
            {
                if (Tab.ToLower() == "forecast - mean" || Tab.ToLower() == "forecast - p70" || Tab.ToLower() == "forecast - p80" || Tab.ToLower() == "forecast - p90")

                {
                    driver.Navigate().GoToUrl($"{vendorPortalLink}/retail-analytics/dashboard/forecasting");
                    Console.WriteLine("Navigated To Forecast Page");

                    WebDriverWait wait8 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    IWebElement statistics = wait8.Until(ExpectedConditions.ElementIsVisible(By.Id("statistic")));
                    Thread.Sleep(2000);

                    int option;
                    if (Tab.ToLower() == "forecast - mean")
                    {
                        option = 0;
                    }
                    else if (Tab.ToLower() == "forecast - p70")
                    {
                        option = 1;
                    }
                    else if (Tab.ToLower() == "forecast - p80")
                    {
                        option = 2;
                    }
                    else { option = 3; }
                    IWebElement dropdown8 = driver.FindElement(By.Id("statistic"));
                    dropdown8.Click();
                    Thread.Sleep(2000);


                    IWebElement shadowRoot8 = (IWebElement)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].shadowRoot.querySelector('.select-options');", dropdown8);
                    Thread.Sleep(2000);

                    IWebElement innerContainer8 = shadowRoot8.FindElement(By.CssSelector(".option-inner-container"));
                    Thread.Sleep(2000);



                    IList<IWebElement> selectdropoptions8 = innerContainer8.FindElements(By.TagName("kat-option"));
                    foreach (var options in selectdropoptions8)
                    {
                        var selectoption = selectdropoptions8[option];
                        selectoption.Click();
                        Thread.Sleep(2000);
                        Console.WriteLine($"{Tab} option select from the dropdown");
                        break;
                    }
                    IWebElement customcolumns = driver.FindElement(By.ClassName("css-4g6ai3"));
                    customcolumns.Click();
                    Thread.Sleep(2000);


                    IWebElement container = driver.FindElement(By.CssSelector(".ltr-mlseai"));
                    Thread.Sleep(2000);
                    ReadOnlyCollection<IWebElement> checkboxes = container.FindElements(By.TagName("kat-checkbox"));
                    int checkbox = 3;
                    checkboxes[checkbox].Click();
                    Thread.Sleep(2000);
                    IJavaScriptExecutor js1 = (IJavaScriptExecutor)driver;
                    js1.ExecuteScript($"arguments[0].querySelectorAll('kat-checkbox')[{checkbox}].checked = false;", container);
                    WebDriverWait wait7 = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
                    wait7.Until(ExpectedConditions.ElementIsVisible(By.Id("customizeColumnsSaveButton")));
                    IWebElement save = driver.FindElement(By.Id("customizeColumnsSaveButton"));
                    save.Click();
                    Thread.Sleep(4000);

                }
                else
                {
                    driver.Navigate().GoToUrl($"{vendorPortalLink}/retail-analytics/dashboard/product-{Tab}");
                    Thread.Sleep(2000);
                    Console.WriteLine($"Navigated To {Tab} Page");
                    WebDriverWait wait2 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    IWebElement customcolumn = wait2.Until(ExpectedConditions.ElementIsVisible(By.ClassName("css-4g6ai3")));
                    IWebElement customcolumns1 = driver.FindElement(By.ClassName("css-4g6ai3"));
                    customcolumns1.Click();
                    Thread.Sleep(2000);
                    Console.WriteLine("Opened The Custom Column Options");
                    IWebElement container1 = driver.FindElement(By.CssSelector(".ltr-mlseai"));
                    ReadOnlyCollection<IWebElement> checkboxes = container1.FindElements(By.TagName("kat-checkbox"));
                    int checkbox1 = 8;
                    checkboxes[checkbox1].Click();
                    Thread.Sleep(2000);
                    IJavaScriptExecutor js1 = (IJavaScriptExecutor)driver;
                    js1.ExecuteScript($"arguments[0].querySelectorAll('kat-checkbox')[{checkbox1}].checked = false;", container1);
                    WebDriverWait wait7 = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
                    wait7.Until(ExpectedConditions.ElementIsVisible(By.Id("customizeColumnsSaveButton")));
                    IWebElement save1 = driver.FindElement(By.Id("customizeColumnsSaveButton"));
                    save1.Click();
                    Thread.Sleep(4000);
                    Console.WriteLine("Unchecked the Brand Option");

                }
                IWebElement apply2 = driver.FindElement(By.ClassName("ltr-1cfden2"));
                apply2.Click();
                Thread.Sleep(5000);
                Console.WriteLine("Clicked On Apply Button");
                IWebElement buttoncontainer2 = driver.FindElement(By.TagName("kat-button-group"));
                string buttonLabel2 = "CSV";

                IJavaScriptExecutor js2 = (IJavaScriptExecutor)driver;
                js2.ExecuteScript($"arguments[0].querySelector('kat-button[label=\"{buttonLabel2}\"]').click();", buttoncontainer2);
                Thread.Sleep(5000);
                Console.WriteLine("Clicked On CSV Button");


                IWebElement downloadmanager2 = driver.FindElement(By.Id("downloadManager"));
                downloadmanager2.Click();
                Thread.Sleep(2000);


                WebDriverWait wait6 = new WebDriverWait(driver, TimeSpan.FromSeconds(300));
                wait6.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("kat-table-body kat-table-row:first-child a")));
                IWebElement progress = driver.FindElement(By.CssSelector("kat-table-body kat-table-row:first-child a"));
                if (progress != null)
                {
                    Console.WriteLine("The Requested File Added in Download Manager");
                    return true;
                }
                else
                {
                    Console.WriteLine("The Requested File Failed To Add In Download Manager");
                    return false;
                }
            }
            else
            {
                switch (Tab.ToLower())
                {
                    case "repeatpurchasebehaviour-asin":
                        // driver.Navigate().GoToUrl($"{vendorPortalLink}/brand-analytics/dashboard/repeat-purchase-behavior?reporting-range=weekly&weekly-week=2024-04-13&view-id=repeat-purchase-brands-view");
                        string formattedDate = startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd");

                        driver.Navigate().GoToUrl($"{vendorPortalLink}/brand-analytics/dashboard/repeat-purchase-behavior?reporting-range=weekly&weekly-week={formattedDate}&view-id=repeat-purchase-asin-view");
                        Thread.Sleep(2000);
                        Console.WriteLine("Navigated To Repeat Purchase Behaviour Page");
                        // Find the parent element first
                        //-----------
                        IWebElement parentElement = driver.FindElement(By.ClassName("css-17mqf2o"));
                        parentElement.Click();
                        Thread.Sleep(2000);

                        //IWebElement asinview = parentElement.FindElement(By.ClassName("css-dy5cgg"));
                        IWebElement asinview = parentElement.FindElement(By.ClassName("css-dy5cgg"));
                        asinview.Click();
                        Thread.Sleep(2000);
                        Console.WriteLine("Clicked On Asin View");
                        // Execute JavaScript to retrieve the shadow root
                        /*IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                            //----------
                        /*IWebElement shadowRoot = (IWebElement)js.ExecuteScript("return arguments[0].shadowRoot",asinview);*//*

                        // Find the dropdown element within the shadow root
                        IWebElement dropdownElement = asinview.FindElement(By.CssSelector("kat-dropdown#reporting-range"));

                        // Click on the dropdown element
                        dropdownElement.Click();




                        IWebElement shadowRoot0 = (IWebElement)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].shadowRoot.querySelector('.select-options');", dropdownElement);

                        IWebElement innerContainer0 = shadowRoot0.FindElement(By.CssSelector(".option-inner-container"));

                        IList<IWebElement> selectdropoptions0 = innerContainer0.FindElements(By.TagName("kat-option"));
                        foreach (var options in selectdropoptions0)
                        {
                            var selectoption = selectdropoptions0[0];
                            selectoption.Click();
                            break;
                        }

                        IWebElement secondnddropdown = asinview.FindElement(By.CssSelector("kat-dropdown#weekly-week"));
                        secondnddropdown.Click();

                        IWebElement shadowRoot = (IWebElement)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].shadowRoot.querySelector('.select-options');", secondnddropdown);

                        IWebElement innerContainer = shadowRoot.FindElement(By.CssSelector(".option-inner-container"));

                        IList<IWebElement> selectdropoptions = innerContainer.FindElements(By.TagName("kat-option"));
                        foreach (var options in selectdropoptions)
                        {
                            var selectoption = selectdropoptions[0];
                            selectoption.Click();
                            break;
                        }*/

                        //------------------


                        // Find the parent element
                        //IWebElement popoverElement = asinview.FindElement(By.CssSelector("kat-popover[class='css-1qiisid']"));




                        // Now, find the apply button within the shadow DOM
                        /*IWebElement applyButton = popoverElement.FindElement(By.CssSelector("kat-button[class='css-1oy4rvw']"));
                        applyButton.Click();*/

                        IWebElement applyButton = asinview.FindElement(By.CssSelector("kat-button.css-1oy4rvw"));
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", applyButton);
                        Console.WriteLine("Clicked On Apply Button");



                        Thread.Sleep(2000);
                        IWebElement gendownbutton0 = asinview.FindElement(By.Id("GenerateDownloadButton"));
                        gendownbutton0.Click();
                        Thread.Sleep(2000);
                        IWebElement simpleview = driver.FindElement(By.ClassName("kat-radiobutton-icon"));
                        simpleview.Click();
                        IWebElement download0 = driver.FindElement(By.Id("downloadModalGenerateDownloadButton"));
                        download0.Click();
                        Thread.Sleep(4000);

                        IWebElement downloadmanager = driver.FindElement(By.Id("downloadModalGenerateDownloadButton"));
                        downloadmanager.Click();
                        Thread.Sleep(4000);

                        string currentWindowHandle = driver.CurrentWindowHandle;
                        ReadOnlyCollection<string> windowHandles = driver.WindowHandles;
                        string newTabHandle = windowHandles.Last(); // Get the handle of the newly opened tab
                        driver.SwitchTo().Window(newTabHandle);
                        /*driver.Navigate().GoToUrl($"{vendorPortalLink}/brand-analytics/download-manager");*/
                        Thread.Sleep(50000);



                        IList<IWebElement> element = driver.FindElements(By.CssSelector("div[role='rowgroup'] div.tr.css-0 div:nth-child(1) span"));

                        foreach (var r in element)
                        {
                            var text = r.GetAttribute("innerHTML");
                            if (text == "Download")
                            {
                                string originalTabHandle = driver.WindowHandles.First();

                                driver.SwitchTo().Window(originalTabHandle);
                                Thread.Sleep(2000);
                                Console.WriteLine("Requested File Added in Download Manager");
                                return true;
                            }
                            else { Console.WriteLine("Requested File Failed To Add in Download Manager"); return false; }
                        }



                        break;
                    case "topsearchterms":
                        //driver.Navigate().GoToUrl($"{vendorPortalLink}/brand-analytics/dashboard/top-search-terms?reporting-range=weekly&weekly-week=2024-04-13&view-id=top-search-terms-default-view");
                        string formattedDate0 = startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : DateTime.Now.ToString();

                        driver.Navigate().GoToUrl($"{vendorPortalLink}/brand-analytics/dashboard/top-search-terms?reporting-range=weekly&weekly-week={formattedDate0}&view-id=top-search-terms-default-view");

                        Thread.Sleep(2000);
                        Console.WriteLine("Navigated To Top Search Terms Page");
                        /*IWebElement week11 = driver.FindElement(By.Id("reporting-range"));
                        week11.Click();

                        Thread.Sleep(4000);

                        IWebElement shadowRoot9 = (IWebElement)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].shadowRoot.querySelector('.select-options');", week11);
                        Thread.Sleep(4000);

                        IWebElement innerContainer9 = shadowRoot9.FindElement(By.CssSelector(".option-inner-container"));
                        Thread.Sleep(4000);

                        IList<IWebElement> selectweek0 = innerContainer9.FindElements(By.TagName("kat-option"));
                        foreach (var options in selectweek0)
                        {
                            var selectoption = selectweek0[1];
                            selectoption.Click();
                            break;
                        }

                        IWebElement secondnddropdown1 = driver.FindElement(By.CssSelector("kat-dropdown#weekly-week"));
                        secondnddropdown1.Click();
                        Thread.Sleep(4000);

                        IWebElement shadowRoot1 = (IWebElement)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].shadowRoot.querySelector('.select-options');", secondnddropdown1);
                        Thread.Sleep(4000);

                        IWebElement innerContainer1 = shadowRoot1.FindElement(By.CssSelector(".option-inner-container"));
                        Thread.Sleep(4000);

                        IList<IWebElement> selectdropoptions1 = innerContainer1.FindElements(By.TagName("kat-option"));
                        foreach (var options in selectdropoptions1)
                        {
                            var selectoption = selectdropoptions1[0];
                            selectoption.Click();
                            break;
                        }*/

                        IWebElement apply11 = driver.FindElement(By.ClassName("css-1oy4rvw"));
                        apply11.Click();
                        Thread.Sleep(4000);
                        IWebElement gendownbutton9 = driver.FindElement(By.Id("GenerateDownloadButton"));
                        gendownbutton9.Click();
                        Thread.Sleep(2000);
                        IWebElement simpleview1 = driver.FindElement(By.ClassName("kat-radiobutton-icon"));
                        simpleview1.Click();
                        IWebElement download9 = driver.FindElement(By.Id("downloadModalGenerateDownloadButton"));
                        download9.Click();
                        Thread.Sleep(4000);
                        IWebElement downloadmanager0 = driver.FindElement(By.Id("downloadModalGenerateDownloadButton"));
                        downloadmanager0.Click();
                        Thread.Sleep(4000);

                        /*driver.Navigate().GoToUrl($"{vendorPortalLink}/brand-analytics/download-manager");*/
                        // string currentWindowHandle1 = driver.CurrentWindowHandle;
                        ReadOnlyCollection<string> windowHandles1 = driver.WindowHandles;
                        string newTabHandle1 = windowHandles1.Last(); // Get the handle of the newly opened tab
                        driver.SwitchTo().Window(newTabHandle1);

                        Thread.Sleep(50000);



                        IList<IWebElement> element1 = driver.FindElements(By.CssSelector("div[role='rowgroup'] div.tr.css-0 div:nth-child(1) span"));

                        foreach (var r in element1)
                        {

                            var text = r.GetAttribute("innerHTML");

                            if (text == "Download")
                            {
                                string originalTabHandle = driver.WindowHandles.First();

                                driver.SwitchTo().Window(originalTabHandle);
                                Thread.Sleep(2000);

                                return true;
                            }
                            else { return false; }
                        }
                        break;
                    case "marketbasketanalysis":
                        /*driver.Navigate().GoToUrl($"{vendorPortalLink}/retail-analytics/dashboard/forecastingew");*/
                        string formattedDate1 = startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : DateTime.Now.ToString();

                        driver.Navigate().GoToUrl($"{vendorPortalLink}/brand-analytics/dashboard/market-basket-analysis?reporting-range=weekly&weekly-week={formattedDate1}&product=all&view-id=market-basket-analysis-default-view");

                        Thread.Sleep(2000);
                        Console.WriteLine("Navigated To Market Basket Analysis Page");
                        /*IWebElement week12 = driver.FindElement(By.Id("reporting-range"));
                        week12.Click();
                        Thread.Sleep(2000);


                        IWebElement shadowRoot99 = (IWebElement)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].shadowRoot.querySelector('.select-options');", week12);

                        IWebElement innerContainer99 = shadowRoot99.FindElement(By.CssSelector(".option-inner-container"));

                        IList<IWebElement> selectweek09 = innerContainer99.FindElements(By.TagName("kat-option"));
                        foreach (var options in selectweek09)
                        {
                            var selectoption = selectweek09[0];
                            selectoption.Click();
                            Thread.Sleep(2000);

                            break;
                        }

                        IWebElement secondnddropdown2 = driver.FindElement(By.CssSelector("kat-dropdown#weekly-week"));
                        secondnddropdown2.Click();
                        Thread.Sleep(2000);

                        IWebElement shadowRoot2 = (IWebElement)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].shadowRoot.querySelector('.select-options');", secondnddropdown2);

                        IWebElement innerContainer2 = shadowRoot2.FindElement(By.CssSelector(".option-inner-container"));

                        IList<IWebElement> selectdropoptions2 = innerContainer2.FindElements(By.TagName("kat-option"));
                        foreach (var options in selectdropoptions2)
                        {
                            var selectoption = selectdropoptions2[0];
                            selectoption.Click();
                            Thread.Sleep(2000);

                            break;
                        }*/

                        IWebElement apply12 = driver.FindElement(By.ClassName("css-1oy4rvw"));
                        apply12.Click();
                        Thread.Sleep(4000);
                        Console.WriteLine("Clicked On Apply Button");
                        IWebElement gendownbutton99 = driver.FindElement(By.Id("GenerateDownloadButton"));
                        gendownbutton99.Click();
                        Thread.Sleep(2000);
                        IWebElement simpleview2 = driver.FindElement(By.ClassName("kat-radiobutton-icon"));
                        simpleview2.Click();
                        IWebElement download99 = driver.FindElement(By.Id("downloadModalGenerateDownloadButton"));
                        download99.Click();
                        Thread.Sleep(4000);
                        IWebElement downloadmanager1 = driver.FindElement(By.Id("downloadModalGenerateDownloadButton"));
                        downloadmanager1.Click();
                        Thread.Sleep(4000);

                        /*driver.Navigate().GoToUrl($"{vendorPortalLink}/brand-analytics/download-manager");
                        string currentWindowHandle2 = driver.CurrentWindowHandle;
                        ReadOnlyCollection<string> windowHandles2 = driver.WindowHandles;
                        string newTabHandle2 = windowHandles2.Last(); // Get the handle of the newly opened tab
                        driver.SwitchTo().Window(newTabHandle2);*/
                        List<string> windowHandles3 = new List<string>(driver.WindowHandles);
                        string mainWindowHandle = driver.CurrentWindowHandle;

                        // Switch to the newly opened tab
                        string newTabHandle3 = windowHandles3.Last(); // Get the handle of the newly opened tab
                        driver.SwitchTo().Window(newTabHandle3);

                        Thread.Sleep(50000);

                        IList<IWebElement> element2 = driver.FindElements(By.CssSelector("div[role='rowgroup'] div.tr.css-0 div:nth-child(1) span"));

                        foreach (var r in element2)
                        {
                            var text = r.GetAttribute("innerHTML");
                            if (text == "Download")
                            {
                                string originalTabHandle = driver.WindowHandles.First();

                                driver.SwitchTo().Window(originalTabHandle);
                                Thread.Sleep(2000);
                                Console.WriteLine($"The Requested File Top Search Terms For The Date {startDate} Is Added In Download Manager");
                                return true;
                            }
                            else { return false; }
                        }

                        break;
                }
            }

            return false;

            //------------

            /*     IWebElement apply = driver.FindElement(By.ClassName("ltr-1cfden2"));
                 apply.Click();
                 Thread.Sleep(4000);

                 IWebElement buttoncontainer = driver.FindElement(By.TagName("kat-button-group"));
                 string buttonLabel = "CSV";

                  js2 = (IJavaScriptExecutor)driver;
                 js2.ExecuteScript($"arguments[0].querySelector('kat-button[label=\"{buttonLabel}\"]').click();", buttoncontainer);



                 IWebElement downloadmanager = driver.FindElement(By.Id("downloadManager"));
                 downloadmanager.Click();


                 WebDriverWait wait4 = new WebDriverWait(driver, TimeSpan.FromSeconds(50));
                 wait4.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("kat-table-body kat-table-row:first-child a")));*/

            //--------------------------------

            /*IWebElement clsdwnmngr = driver.FindElement(By.ClassName("ltr-1upyiy5"));
            clsdwnmngr.Click();*/

            /*IList<IWebElement> addedtodownmanager = driver.FindElements(By.CssSelector("Kat-table-row"));
            addedtodownmanager = addedtodownmanager.Skip(1).ToList();
            string FileName="";
            foreach (var file in  addedtodownmanager)
            {
                IWebElement filenameelement = file.FindElement(By.CssSelector("kat-table-cell > div"));
                string filename = filenameelement.Text.Trim();
                string[] nameparts = filename.Split('_');
                FileName = string.Join("_", nameparts.Take(6));
                break;
            }

            IWebElement downloadButton = driver.FindElement(By.CssSelector("kat-table-body kat-table-row:first-child a"));
            downloadButton.Click();

            Thread.Sleep(5000);

            string[] files = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\downloads\", "*.csv");
            string File_Name = "";
            string FilePath = "";
            foreach (string file in files)
            {
                string FullfilePath = Path.GetFullPath(file);
                FilePath = Path.GetFileNameWithoutExtension(file);
                string[] nameparts = FilePath.Split("_");
                File_Name = string.Join("_", nameparts.Take(6));

                if (FileName == File_Name)
                {
                    bool Isdownload = true;
                    AddData(FullfilePath, File_Name, Isdownload);
                }


            }*/







            /*driver.Close();*/


            /*EmailSender sendmail = new EmailSender();
            sendmail.SendEmail("minoopsen@gmail.com","File Download","File Download Successfully");*/

        }

        public bool confirmaddedindwnmanager(IWebDriver driver, string Tab, int id, string period, DateTime? startdate,DTHCPOSVendorPortalConfig configDetails)
        {
            string vendorPortalLink = configDetails.VendorPortalLink;
            if (Tab.ToLower() == "sales" || Tab.ToLower() == "traffic" || Tab.ToLower() == "inventory")
            {
                driver.Navigate().GoToUrl($"{vendorPortalLink}/retail-analytics/dashboard/d/{Tab.ToLower()}");
                Thread.Sleep(4000);

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                IWebElement downmngr = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("downloadManager")));


                IWebElement downloadmanager = driver.FindElement(By.Id("downloadManager"));
                downloadmanager.Click();
                Thread.Sleep(4000);
                IList<IWebElement> addedtodownmanager = driver.FindElements(By.CssSelector("Kat-table-row"));
                addedtodownmanager = addedtodownmanager.Skip(1).ToList();


                switch (period)
                {

                    case "Daily":
                        


                        foreach (var file in addedtodownmanager)
                        {
                            IWebElement filenameelement = file.FindElement(By.CssSelector("kat-table-cell > div"));
                            string FileName = filenameelement.Text.Trim();
                            string[] nameparts = FileName.Split('_');
                            //string filename = string.Join("_", nameparts.Take(6));
                            string FileTab = nameparts[0].ToLower();
                            string FilePeriod = nameparts[nameparts.Length - 3].ToLower();
                            string date = nameparts[nameparts.Length - 1];
                            /*DateTime StartDate = DateTime.ParseExact(nameparts[nameparts.Length - 1], "dd-MM-yyyy", CultureInfo.InvariantCulture);*/
                            if (Tab.ToLower() == FileTab && period.ToLower() == FilePeriod)
                            {
                                Console.WriteLine("Your requested file for daily is added to download manager");
                                return true;

                            }

                        }
                        IWebElement paginationElement = driver.FindElement(By.TagName("kat-pagination"));
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].shadowRoot.querySelector('ul.pages > li:nth-child(2)').click()", paginationElement);
                        Thread.Sleep(2000);
                        IList<IWebElement> addedtodownmanager8 = driver.FindElements(By.CssSelector("Kat-table-row"));
                        addedtodownmanager8 = addedtodownmanager8.Skip(1).ToList();
                        foreach (var file in addedtodownmanager8)
                        {
                            IWebElement filenameelement = file.FindElement(By.CssSelector("kat-table-cell > div"));
                            string FileName = filenameelement.Text.Trim();
                            string[] nameparts = FileName.Split('_');
                            //string Filename = string.Join("_", nameparts.Take(6));
                            string FileTab = nameparts[0].ToLower();
                            string FilePeriod = nameparts[nameparts.Length - 3].ToLower();


                            if (Tab.ToLower() == FileTab && period.ToLower() == FilePeriod)
                            {
                                Console.WriteLine("Your requested file for weekly is added to download manager");
                                return true;

                            }

                        }



                        break;
                    case "Weekly":
                        IList<IWebElement> addedtodownmanager1 = driver.FindElements(By.CssSelector("Kat-table-row"));
                        addedtodownmanager1 = addedtodownmanager1.Skip(1).ToList();
                        foreach (var file in addedtodownmanager1)
                        {
                            IWebElement filenameelement = file.FindElement(By.CssSelector("kat-table-cell > div"));
                            string FileName = filenameelement.Text.Trim();
                            string[] nameparts = FileName.Split('_');
                            //string Filename = string.Join("_", nameparts.Take(6));
                            string FileTab = nameparts[0].ToLower();
                            string FilePeriod = nameparts[nameparts.Length - 3].ToLower();


                            if (Tab.ToLower() == FileTab && period.ToLower() == FilePeriod)
                            {
                                Console.WriteLine("Your requested file for weekly is added to download manager");
                                return true;

                            }
                            

                        }
                        IWebElement paginationElement3 = driver.FindElement(By.TagName("kat-pagination"));
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].shadowRoot.querySelector('ul.pages > li:nth-child(2)').click()", paginationElement3);
                        Thread.Sleep(2000);
                        IList<IWebElement> addedtodownmanager9 = driver.FindElements(By.CssSelector("Kat-table-row"));
                        addedtodownmanager1 = addedtodownmanager9.Skip(1).ToList();
                        foreach (var file in addedtodownmanager1)
                        {
                            IWebElement filenameelement = file.FindElement(By.CssSelector("kat-table-cell > div"));
                            string FileName = filenameelement.Text.Trim();
                            string[] nameparts = FileName.Split('_');
                            //string Filename = string.Join("_", nameparts.Take(6));
                            string FileTab = nameparts[0].ToLower();
                            string FilePeriod = nameparts[nameparts.Length - 3].ToLower();


                            if (Tab.ToLower() == FileTab && period.ToLower() == FilePeriod)
                            {
                                Console.WriteLine("Your requested file for weekly is added to download manager");
                                return true;

                            }

                        }
                        break;

                    
                }
            }
            else if (Tab.ToLower() == "forecast - mean" || Tab.ToLower() == "forecast - p70" || Tab.ToLower() == "forecast - p80" || Tab.ToLower() == "forecast - p90" || Tab.ToLower() == "catalogue")
            {
                driver.Navigate().GoToUrl($"{vendorPortalLink}/brand-analytics/download-manager");
                Thread.Sleep(4000);

               /* WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                IWebElement downmngr = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("downloadManager")));


                IWebElement downloadmanager = driver.FindElement(By.Id("downloadManager"));
                downloadmanager.Click();
                Thread.Sleep(4000);*/

                IList<IWebElement> addedtodownmanager = driver.FindElements(By.CssSelector("Kat-table-row"));
                addedtodownmanager = addedtodownmanager.Skip(1).ToList();
                if (Tab.ToLower() == "forecast - mean" || Tab.ToLower() == "forecast - p70" || Tab.ToLower() == "forecast - p80" || Tab.ToLower() == "forecast - p90")


                {


                    foreach (var file in addedtodownmanager)
                    {
                        IWebElement filenameelement = file.FindElement(By.CssSelector("kat-table-cell > div"));
                        string FileName = filenameelement.Text.Trim();
                        string[] nameparts = FileName.Split('_');
                        //string Filename = string.Join("_", nameparts.Take(6));
                        //string FileTab = nameparts[0].ToLower();
                        // string FilePeriod = nameparts[nameparts.Length - 1].ToLower();


                        if (Tab.ToLower() == "forecast - mean" || Tab.ToLower() == "forecast - p70" || Tab.ToLower() == "forecast - p80" || Tab.ToLower() == "forecast - p90")
                        {
                            Console.WriteLine("Your requested file for weekly is added to download manager");
                            return true;
                        }


                    }
                    IWebElement paginationElement3 = driver.FindElement(By.TagName("kat-pagination"));
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].shadowRoot.querySelector('ul.pages > li:nth-child(2)').click()", paginationElement3);
                    Thread.Sleep(2000);
                    IList<IWebElement> addedtodownmanager9 = driver.FindElements(By.CssSelector("Kat-table-row"));
                    addedtodownmanager9 = addedtodownmanager9.Skip(1).ToList();
                    foreach (var file in addedtodownmanager9)
                    {
                        IWebElement filenameelement = file.FindElement(By.CssSelector("kat-table-cell > div"));
                        string FileName = filenameelement.Text.Trim();
                        string[] nameparts = FileName.Split('_');
                        //string Filename = string.Join("_", nameparts.Take(6));
                        //string FileTab = nameparts[0].ToLower();
                        // string FilePeriod = nameparts[nameparts.Length - 1].ToLower();


                        if (Tab.ToLower() == "forecast - mean" || Tab.ToLower() == "forecast - p70" || Tab.ToLower() == "forecast - p80" || Tab.ToLower() == "forecast - p90")
                        {
                            Console.WriteLine("Your requested file for weekly is added to download manager");
                            return true;
                        }

                    }





                }
                else
                {
                    driver.Navigate().GoToUrl($"{vendorPortalLink}/brand-analytics/download-manager");
                    Thread.Sleep(4000);

                    /*WebDriverWait wait1 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    IWebElement downmngr1 = wait1.Until(ExpectedConditions.ElementIsVisible(By.Id("downloadManager")));


                    IWebElement downloadmanager1 = driver.FindElement(By.Id("downloadManager"));
                    downloadmanager1.Click();
                    Thread.Sleep(4000);*/
                    IList<IWebElement> addedtodownmanager0 = driver.FindElements(By.CssSelector("Kat-table-row"));
                    addedtodownmanager0 = addedtodownmanager0.Skip(1).ToList();
                    foreach (var file in addedtodownmanager0)
                    {
                        IWebElement filenameelement = file.FindElement(By.CssSelector("kat-table-cell > div"));
                        string FileName = filenameelement.Text.Trim();
                        string[] nameparts = FileName.Split('_');
                        //string Filename = string.Join("_", nameparts.Take(6));
                        string FileTab = nameparts[0].ToLower();
                        //string FilePeriod = nameparts[nameparts.Length-1].ToLower();


                        if (Tab.ToLower() == FileTab)
                        {
                            Console.WriteLine("Your requested file for weekly is added to download manager");
                            return true;
                        }

                    }
                    IWebElement paginationElement3 = driver.FindElement(By.TagName("kat-pagination"));
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].shadowRoot.querySelector('ul.pages > li:nth-child(2)').click()", paginationElement3);
                    Thread.Sleep(2000);
                    IList<IWebElement> addedtodownmanager9 = driver.FindElements(By.CssSelector("Kat-table-row"));
                    addedtodownmanager9 = addedtodownmanager9.Skip(1).ToList();
                    foreach (var file in addedtodownmanager9)
                    {
                        IWebElement filenameelement = file.FindElement(By.CssSelector("kat-table-cell > div"));
                        string FileName = filenameelement.Text.Trim();
                        string[] nameparts = FileName.Split('_');
                        //string Filename = string.Join("_", nameparts.Take(6));
                        string FileTab = nameparts[0].ToLower();
                        //string FilePeriod = nameparts[nameparts.Length-1].ToLower();


                        if (Tab.ToLower() == FileTab)
                        {
                            Console.WriteLine("Your requested file for weekly is added to download manager");
                            return true;
                        }

                    }

                }
            }
            else
            {
                switch (Tab.ToLower())
                {
                    case "repeatpurchasebehaviour-asin":
                        driver.Navigate().GoToUrl($"{vendorPortalLink}/retail-analytics/dashboard/forecastinger");
                        Thread.Sleep(2000);
                        //IList<IWebElement> rows1 = driver.FindElements(By.CssSelector("div[role='row'].css-0"));

                        try
                        {
                            IList<IWebElement> element2 = driver.FindElements(By.CssSelector("div[role='rowgroup'] div.tr.css-0 "));

                            foreach (var r in element2)
                            {
                                IWebElement nameelemnt = r.FindElement(By.CssSelector("div:nth-child(1) div.css-lhtkpl"));
                                IWebElement periodelement = r.FindElement(By.CssSelector("div:nth-child(3)"));
                                IWebElement progresselement = r.FindElement(By.CssSelector("div:nth-child(1) span"));

                                string nametext = nameelemnt.GetAttribute("innerHTML");
                                string periodtext = periodelement.GetAttribute("innerHTML");
                                var progresstext = progresselement.GetAttribute("innerHTML");
                                if (nametext == "Repeat Purchase Behaviour - ASIN" && periodtext == "Weekly" && progresstext == "Download")
                                {
                                    return true;
                                }
                                
                            }
                           return false;


                        }
                        catch (Exception ex)
                        {

                        }

                        break;
                    case "topsearchterms":
                        driver.Navigate().GoToUrl($"{vendorPortalLink}/retail-analytics/dashboard/forecastinger");
                        Thread.Sleep(2000);
                        //IList<IWebElement> rows2 = driver.FindElements(By.CssSelector("div[role='row'].css-0"));

                        try
                        {
                            IList<IWebElement> element2 = driver.FindElements(By.CssSelector("div[role='rowgroup'] div.tr.css-0 "));

                            foreach (var r in element2)
                            {
                                IWebElement nameelemnt = r.FindElement(By.CssSelector("div:nth-child(1) div.css-lhtkpl"));
                                IWebElement periodelement = r.FindElement(By.CssSelector("div:nth-child(3)"));
                                IWebElement progresselement = r.FindElement(By.CssSelector("div:nth-child(1) span"));

                                string nametext = nameelemnt.GetAttribute("innerHTML");
                                string periodtext = periodelement.GetAttribute("innerHTML");
                                var progresstext = progresselement.GetAttribute("innerHTML");
                                if (nametext == "Top Search Terms" && periodtext == "Weekly" && progresstext == "Download")
                                {
                                    return true;
                                }
                                
                            }

                        }
                        catch (Exception ex)
                        {

                        }
                        break;
                    case "marketbasketanalysis":
                        try
                        {
                            IList<IWebElement> element2 = driver.FindElements(By.CssSelector("div[role='rowgroup'] div.tr.css-0 "));

                            foreach (var r in element2)
                            {
                                IWebElement nameelemnt = r.FindElement(By.CssSelector("div:nth-child(1) div.css-lhtkpl"));
                                IWebElement periodelement = r.FindElement(By.CssSelector("div:nth-child(3)"));
                                IWebElement progresselement = r.FindElement(By.CssSelector("div:nth-child(1) span"));

                                string nametext = nameelemnt.GetAttribute("innerHTML");
                                string periodtext = periodelement.GetAttribute("innerHTML");
                                var progresstext = progresselement.GetAttribute("innerHTML");
                                if (nametext == "Market Basket Analysis" && periodtext == "Weekly" && progresstext == "Download")
                                {
                                    return true;
                                }
                                else { return false; }
                            }
                        }
                        catch (Exception ex) { }
                        break;
                }
            }


            return false;
        }



    }

}
