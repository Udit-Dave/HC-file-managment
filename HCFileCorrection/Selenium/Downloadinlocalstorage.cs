using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using OtpNet;
using HCFileCorrection.Utility;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V120.DOM;
using OpenQA.Selenium.Support.UI;
using Org.BouncyCastle.Crypto.Prng.Drbg;
using HCFileCorrection.Interfaces;
using System.Collections.ObjectModel;
using HCFileCorrection.Entities;
using OpenQA.Selenium.Interactions;
using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace HCFileCorrection.Selenium
{
    public class Downloadinlocalstorage
    {
        private readonly GetOtp genotp;
        private readonly IHCRepository _repository;
        private readonly IConfiguration _configuration;

        /* public Downloadinlocalstorage()
         {
         }*/

        public Downloadinlocalstorage(GetOtp genotp,IHCRepository repository,IConfiguration configuration)
        {
            this.genotp = genotp;
            _repository = repository;
            _configuration = configuration;
        }
        
        public bool localdirectory(string Tab,string period, DateTime? startDate, IWebDriver driver, DTHCPOSVendorPortalConfig configDetails)
        {

           string vendorPortalLink = configDetails.VendorPortalLink;

            /*WebDriverWait wait4 = new WebDriverWait(driver, TimeSpan.FromSeconds(50));
            wait4.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("kat-table-body kat-table-row:first-child a")));*/

            if (Tab == "sales" || Tab == "inventory" || Tab == "traffic")
            {

                driver.Navigate().GoToUrl($"{vendorPortalLink}/retail-analytics/dashboard/sales");
                Thread.Sleep(2000);
               // WebDriverWait wait15 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                /*try
                {
                    IWebElement shadowHost = wait15.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("kat-popover")));
                    shadowHost.Click();
                   
                    // Access shadow DOM and find the close button within the shadow DOM using JavaScript
                    IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)driver;
                    IWebElement closeButton = (IWebElement)jsExecutor.ExecuteScript(
                        "return arguments[0].shadowRoot.querySelector('svg')", shadowHost);

                    // Click the close button
                    closeButton.Click();
                }
                catch (WebDriverTimeoutException)
                {
                    
                }*/


                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                IWebElement downmngr = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("downloadManager")));
                Thread.Sleep(2000);
                
                IWebElement downloadmanager = driver.FindElement(By.Id("downloadManager"));
                downloadmanager.Click();
                Thread.Sleep(4000);
                Console.WriteLine("Navigated To Download Manager Page");
                WebDriverWait wait1 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                IWebElement pageselement = wait1.Until(ExpectedConditions.ElementIsVisible(By.TagName("kat-pagination")));
                IWebElement paginationElement0 = driver.FindElement(By.TagName("kat-pagination"));

                // Execute JavaScript to access the shadow DOM and retrieve the li elements
                var liElements = ((IJavaScriptExecutor)driver).ExecuteScript(@"
                  // Find the shadow root
                  var shadowRoot = arguments[0].shadowRoot;

                  // Select the ul element containing the li tags
                  var ulElement = shadowRoot.querySelector('ul.pages');

                  // Get all li elements under the ul
                   return ulElement.querySelectorAll('li');
                  ", paginationElement0);

                // Cast the result to a ReadOnlyCollection<IWebElement>
                ReadOnlyCollection<IWebElement> liTags = (ReadOnlyCollection<IWebElement>)liElements;
                int page = 1;
                foreach (var pages in liTags)
                {
                    if (page > 1)
                    {
                       
                        IWebElement paginationElement = driver.FindElement(By.TagName("kat-pagination"));

                        ((IJavaScriptExecutor)driver).ExecuteScript($"arguments[0].shadowRoot.querySelector('ul.pages > li:nth-child({page})').click()", paginationElement);
                        Thread.Sleep(2000);
                    }


                    IList<IWebElement> addedtodownmanager = driver.FindElements(By.CssSelector("Kat-table-row"));
                    Thread.Sleep(2000);
                    addedtodownmanager = addedtodownmanager.Skip(1).ToList();

                    int count = 0;
                    foreach (var file in addedtodownmanager)
                    {
                        count++;
                        IWebElement filenameelement = file.FindElement(By.CssSelector("kat-table-cell > div"));
                        Thread.Sleep(2000);

                        string filename = filenameelement.Text.Trim();
                        string[] nameparts = filename.Split('_');
                        //FileName = string.Join("_", nameparts.Take(6));
                        string FileTab = nameparts[0].ToLower();
                        string FilePeriod = nameparts[nameparts.Length - 3].ToLower();
                        string date = nameparts[nameparts.Length - 1];
                        string startDateString = startDate?.ToString("dd-MM-yyyy");

                        if (Tab == FileTab && period == FilePeriod && date == startDateString)
                        {
                            /*IWebElement downloadButton = driver.FindElement(By.CssSelector("kat-table-body kat-table-row:first-child a"));*/
                            IWebElement downloadButton = driver.FindElement(By.CssSelector($"kat-table-body kat-table-row:nth-child({count}) a"));
                            downloadButton.Click();
                            Thread.Sleep(1000);

                            Console.WriteLine($"File Name : {filename} ");

                            Thread.Sleep(60000);

                            return true;

                        }
                    }
                    page++;
                }
                   

                

                    

                    


            }
            else if (Tab == "publisherreportalc" || Tab == "publisherreportsubscription")
            {
                if (Tab == "publisherreportalc")
                {
                    driver.Navigate().GoToUrl($"{vendorPortalLink}/apa?ref_=vc_xx_subNav#/aLaCarteSales");
                }
                else if(Tab == "publisherreportsubscription")
                {
                    driver.Navigate().GoToUrl($"{vendorPortalLink}/apa?ref_=vc_xx_subNav#/subscriptions");
                }
                Thread.Sleep(2000);
                IWebElement daterange = driver.FindElement(By.Id("reportDateRangeFilterInput"));
                Actions action = new Actions(driver);
                action.DoubleClick(daterange).Perform();
                Thread.Sleep(2000);
                //daterange.Click();
               /* daterange.Click();*/
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].value = '';", daterange);
                Thread.Sleep(2000);
                DateTime date = startDate.Value;
                string dateString = date.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                string dateString1 = dateString + "-" + date.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                daterange.SendKeys(dateString1);
                Thread.Sleep(2000);
                daterange.SendKeys(Keys.Enter);
                /*IWebElement apply = driver.FindElement(By.ClassName("css-1hq541j"));
                apply.Click();*/
                Thread.Sleep(2000);
                IWebElement element = driver.FindElement(By.XPath("//*[@id='dateRangePopup']/div/div[5]/button[2]"));

                // Click on the element
                element.Click();
                Thread.Sleep(2000);
                IWebElement compareelement = driver.FindElement(By.Id("compareToFilterInput"));
                compareelement.Click();
                Thread.Sleep(2000);
                IWebElement popup = driver.FindElement(By.XPath("//div[contains(@class, 'ui') and contains(@class, 'popup') and contains(@class, 'visible')]"));


                IWebElement noneLabel = popup.FindElement(By.XPath(".//label[@for='radioOptionNone']"));
                Thread.Sleep(1000);
                noneLabel.Click();
                Thread.Sleep(2000);
                IWebElement downloadcurrentview = driver.FindElement(By.Id("downloadCurrentViewButton"));
                downloadcurrentview.Click();
                Thread.Sleep(2000);
                IWebElement downloadcsv = driver.FindElement(By.Id("downloadDetailedCsvLink"));
                downloadcsv.Click();
                Thread.Sleep(180000);
                return true;

            }
            else if (Tab == "forecast - mean" || Tab == "forecast - p70" || Tab == "forecast - p80" || Tab == "forecast - p90")
            {
                driver.Navigate().GoToUrl($"{vendorPortalLink}/retail-analytics/dashboard/sales");
                Thread.Sleep(2000);
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                IWebElement downmngr = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("downloadManager")));
                Thread.Sleep(2000);

                IWebElement downloadmanager = driver.FindElement(By.Id("downloadManager"));
                downloadmanager.Click();
                Thread.Sleep(4000);
                Console.WriteLine("Navigated To Download Manager Page");
                WebDriverWait wait1 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                IWebElement pageselement = wait1.Until(ExpectedConditions.ElementIsVisible(By.TagName("kat-pagination")));
                IWebElement paginationElement0 = driver.FindElement(By.TagName("kat-pagination"));

                // Execute JavaScript to access the shadow DOM and retrieve the li elements
                var liElements = ((IJavaScriptExecutor)driver).ExecuteScript(@"
                  // Find the shadow root
                  var shadowRoot = arguments[0].shadowRoot;

                  // Select the ul element containing the li tags
                  var ulElement = shadowRoot.querySelector('ul.pages');

                  // Get all li elements under the ul
                   return ulElement.querySelectorAll('li');
                  ", paginationElement0);

                // Cast the result to a ReadOnlyCollection<IWebElement>
                ReadOnlyCollection<IWebElement> liTags = (ReadOnlyCollection<IWebElement>)liElements;
                int page = 1;
                foreach (var pages in liTags)
                {
                    if (page > 1)
                    {

                        IWebElement paginationElement = driver.FindElement(By.TagName("kat-pagination"));

                        ((IJavaScriptExecutor)driver).ExecuteScript($"arguments[0].shadowRoot.querySelector('ul.pages > li:nth-child({page})').click()", paginationElement);
                        Thread.Sleep(2000);
                    }

                    IList<IWebElement> addedtodownmanager = driver.FindElements(By.CssSelector("Kat-table-row"));
                    Thread.Sleep(2000);

                    addedtodownmanager = addedtodownmanager.Skip(1).ToList();



                    int count = 0;
                    foreach (var file in addedtodownmanager)
                    {
                        count++;
                        IWebElement filenameelement = file.FindElement(By.CssSelector("kat-table-cell > div"));
                        Thread.Sleep(2000);

                        string filename = filenameelement.Text.Trim();
                        string[] nameparts = filename.Split('_');
                        //FileName = string.Join("_", nameparts.Take(6));
                        string FileTab = nameparts[0].ToLower();
                        string FilePeriod = nameparts[nameparts.Length - 2].ToLower();
                        DateOnly date = DateOnly.FromDateTime(DateTime.Today);
                        string todaysdate = date.ToString("dd/MM/yyyy");
                        IWebElement dateelment = file.FindElement(By.ClassName("ltr-1are86p"));
                        string dateText = dateelment.Text;
                        string[] parts = dateText.Split(',');
                        string dateString = parts[0].Trim();
                        /*DateOnly parsedDate = DateOnly.ParseExact(dateString, "MM/dd/yyyy", CultureInfo.InvariantCulture);*/
                        DateOnly parsedDate = DateOnly.ParseExact(dateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                        string formattedParsedDate = parsedDate.ToString("dd/MM/yyyy");
                        if (FilePeriod == "meanforecast")
                        {
                            FilePeriod = "forecast - mean";
                        }
                        else if (FilePeriod == "p70forecast")
                        {
                            FilePeriod = "forecast - p70";
                        }
                        else if (FilePeriod == "p80forecast")
                        {
                            FilePeriod = "forecast - p80";
                        }
                        else if (FilePeriod == "p90forecast")
                        {
                            FilePeriod = "forecast - p90";
                        }
                        if (Tab == FilePeriod && todaysdate == formattedParsedDate)
                        {
                            /*IWebElement downloadButton = driver.FindElement(By.CssSelector("kat-table-body kat-table-row:first-child a"));*/
                            IWebElement downloadButton = driver.FindElement(By.CssSelector($"kat-table-body kat-table-row:nth-child({count}) a"));
                            downloadButton.Click();

                            Console.WriteLine($"File Name : {filename} ");

                            Thread.Sleep(60000);
                            return true;
                        }


                    }
                    page++;
                }
                

            }
            else if (Tab == "catalogue")
            {
                driver.Navigate().GoToUrl($"{vendorPortalLink}/retail-analytics/dashboard/sales");



                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                IWebElement downmngr = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("downloadManager")));
                Thread.Sleep(2000);

                IWebElement downloadmanager = driver.FindElement(By.Id("downloadManager"));
                downloadmanager.Click();
                Thread.Sleep(4000);
                Console.WriteLine("Navigated To Download Manager Page");
                WebDriverWait wait1 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                IWebElement pageselement = wait1.Until(ExpectedConditions.ElementIsVisible(By.TagName("kat-pagination")));
                IWebElement paginationElement0 = driver.FindElement(By.TagName("kat-pagination"));

                // Execute JavaScript to access the shadow DOM and retrieve the li elements
                var liElements = ((IJavaScriptExecutor)driver).ExecuteScript(@"
                  // Find the shadow root
                  var shadowRoot = arguments[0].shadowRoot;

                  // Select the ul element containing the li tags
                  var ulElement = shadowRoot.querySelector('ul.pages');

                  // Get all li elements under the ul
                   return ulElement.querySelectorAll('li');
                  ", paginationElement0);

                // Cast the result to a ReadOnlyCollection<IWebElement>
                ReadOnlyCollection<IWebElement> liTags = (ReadOnlyCollection<IWebElement>)liElements;
                int page = 1;
                foreach (var pages in liTags)
                {
                    if (page > 1)
                    {

                        IWebElement paginationElement = driver.FindElement(By.TagName("kat-pagination"));

                        ((IJavaScriptExecutor)driver).ExecuteScript($"arguments[0].shadowRoot.querySelector('ul.pages > li:nth-child({page})').click()", paginationElement);
                        Thread.Sleep(2000);
                    }
                    IList<IWebElement> addedtodownmanager = driver.FindElements(By.CssSelector("Kat-table-row"));
                    Thread.Sleep(2000);

                    addedtodownmanager = addedtodownmanager.Skip(1).ToList();



                    int count = 0;
                    foreach (var file in addedtodownmanager)
                    {
                        count++;
                        IWebElement filenameelement = file.FindElement(By.CssSelector("kat-table-cell > div"));
                        Thread.Sleep(2000);

                        string filename = filenameelement.Text.Trim();
                        string[] nameparts = filename.Split('_');
                        //FileName = string.Join("_", nameparts.Take(6));
                        string FileTab = nameparts[0].ToLower();
                        string FilePeriod = nameparts[nameparts.Length - 2].ToLower();
                        DateOnly date = DateOnly.FromDateTime(DateTime.Today);
                        string todaysdate = date.ToString("dd/MM/yyyy");
                        IWebElement dateelment = file.FindElement(By.ClassName("ltr-1are86p"));
                        string dateText = dateelment.Text;
                        string[] parts = dateText.Split(',');
                        string dateString = parts[0].Trim();
                        /*DateOnly parsedDate = DateOnly.ParseExact(dateString, "MM/dd/yyyy", CultureInfo.InvariantCulture);*/
                        DateOnly parsedDate = DateOnly.ParseExact(dateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        string formattedParsedDate = parsedDate.ToString("dd/MM/yyyy");

                        if (FileTab == Tab && FilePeriod == "manufacturing" && formattedParsedDate == todaysdate)
                        {
                            /*IWebElement downloadButton = driver.FindElement(By.CssSelector("kat-table-body kat-table-row:first-child a"));*/
                            IWebElement downloadButton = driver.FindElement(By.CssSelector($"kat-table-body kat-table-row:nth-child({count}) a"));
                            downloadButton.Click();

                            Console.WriteLine($"File Name : {filename} ");

                            Thread.Sleep(4000);
                            return true;
                        }


                    }
                    page++;
                }

            }
            else if (Tab == "repeatpurchasebehaviour-asin" || Tab == "topsearchterms" || Tab == "marketbasketanalysis")
            {
                
                //driver.Navigate().GoToUrl($"https://vendorcentral.amazon.it/brand-analytics/dashboard/repeat-purchase-behavior?reporting-range=weekly&weekly-week=2024-03-30&view-id=repeat-purchase-asin-view");
                driver.Navigate().GoToUrl($"{vendorPortalLink}/brand-analytics/download-manager");
                Thread.Sleep(2000);
                Console.WriteLine("Navigated To Download Manager Page");
                string currentWindowHandle = driver.CurrentWindowHandle;
                ReadOnlyCollection<string> windowHandles = driver.WindowHandles;
                string newTabHandle = windowHandles.Last(); // Get the handle of the newly opened tab
                driver.SwitchTo().Window(newTabHandle);
                IList<IWebElement> element2 = driver.FindElements(By.CssSelector("div[role='rowgroup'] div[role='row']"));

                foreach (var r in element2)
                {

                    /*IWebElement nameelemnt = r.FindElement(By.CssSelector("div:nth-child(1) div.css-lhtkpl"));*/
                    IWebElement nameelemnt = r.FindElement(By.CssSelector("div:first-child"));
                    IWebElement periodelement = r.FindElement(By.CssSelector("div:nth-child(3)"));
                    IWebElement dateelement = r.FindElement(By.CssSelector("div:nth-child(5)"));
                    IWebElement progresselement = r.FindElement(By.CssSelector("div:nth-child(1) span"));

                    string nametext = nameelemnt.GetAttribute("innerHTML");
                    nametext = nametext.ToLower().Replace(" ", "");
                    string datetext = dateelement.GetAttribute("innerHTML");
                    string periodtext = periodelement.GetAttribute("innerHTML");
                    var progresstext = progresselement.GetAttribute("innerHTML");
                    string startDateText = startDate?.ToString("dd/MM/yyyy");
                    if (nametext == Tab && periodtext == "Weekly" && progresstext == "Download" && startDateText == datetext)
                    {
                        /*ScrollPageHorizontally(driver, 500);*/
                        IWebElement downloadbutton = r.FindElement(By.CssSelector(".css-p1ypz0"));
                        Thread.Sleep(2000);

                        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                        wait.Until(ExpectedConditions.ElementToBeClickable(downloadbutton));



                        downloadbutton.Click();
                        Thread.Sleep(180000);

                       return true;
                    }
                    
                }
                /*IList<IWebElement> rows = driver.FindElements(By.CssSelector(".tr.css-0"));
                int count = 1;
                foreach (IWebElement row in rows)
                {
                    // Find the first column value
                    try
                    {
                        IWebElement baseclass = driver.FindElement(By.ClassName("css-p1ypz0"));
                        baseclass.Click();
                        Thread.Sleep(10000);
                        break;
                      

                    }
                    catch { }

                    // Click the download button
                    

                }*/
            }
            return false;

        }



    }
}
