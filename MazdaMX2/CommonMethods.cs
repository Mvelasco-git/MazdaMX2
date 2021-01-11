using System;
using System.Text.RegularExpressions;

using NUnit.Framework;

using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using SeleniumExtras.WaitHelpers;
using System.Threading;


namespace MazdaMX2Test
{
    public class DealerSession
    {
        private IWebDriver _driver;
        private WebDriverWait explicitWait;
        private string[] arrNameImage;

        public DealerSession(IWebDriver driver) 
        {
            _driver = driver;
            explicitWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(60));
        }

        public void IngresarURL(string enviorementUser, string enviorementQA, string dealerSite)
        {
            _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);
            _driver.Url = "https://" + enviorementUser + "@" + enviorementQA + "/distribuidores/mazda-"+ dealerSite;
            _driver.Manage().Window.Maximize();
            _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);
        }

        public void ClickMethod(IWebElement buttonTest, IWebDriver _driver)
        {
            Actions action = new Actions(_driver);
            //var elementButton = _driver.FindElement(By.XPath(buttonTest));
            action.MoveToElement(buttonTest).Build().Perform();
            buttonTest.Click();

        }

        public void WaitForClick(string elementTest)
        {
            explicitWait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(elementTest)));
        }

        public void WaitIsVisible(string elementWait)
        {
            explicitWait.Until(ExpectedConditions.ElementIsVisible(By.XPath(elementWait)));
        }

        public void WaitForPageLoad()
        {
            _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(5000);
        }

        public void OnlyWait()
        {
            Thread.Sleep(2500);
        }

        public static IWebElement WaitObjects(String objectWait, IWebDriver _driver, int sValor)
        {
            DefaultWait<IWebDriver> fluentWait = new DefaultWait<IWebDriver>(_driver);
            fluentWait.Timeout = TimeSpan.FromSeconds(50);
            fluentWait.PollingInterval = TimeSpan.FromSeconds(0.5);
            fluentWait.IgnoreExceptionTypes(typeof(NoSuchElementException));
            fluentWait.Message = "Elemento no encontrado";

            var fluentWaitID = fluentWait.Until(x =>
            {
                if (sValor == 0) {
                    x.Navigate().Refresh();
                }
                return x.FindElement(By.XPath(objectWait));
            });

            return fluentWaitID;
        }

        public void ValidationText(string text1, string text2)
        {

            try
            {
                Assert.AreEqual(text1, text2);
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public string ConvertText(string text1)
        {
            try
            {
                string newchain = text1.ToUpper();
                string[] newversion = newchain.Split(' ');

                if (newversion[0] != "SIGNATURE" && newversion[0] != "CARBON")
                {
                    newversion[0] = newversion[0].ToLower();
                }

                newchain = "";

                for (int i = 0; i < newversion.Length; i++)
                {
                    newchain = newchain + newversion[i] + " ";
                }

                return newchain.Trim();
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public void ValidationContentText(string text1, string text2)
        {
            try
            {
                Assert.That(text1, Does.Contain(text2));
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public string ObtainAttribute(string objTest, string attributeObtain)
        {
            try
            {
                DefaultWait<IWebDriver> fluentWait = new DefaultWait<IWebDriver>(_driver);
                fluentWait.Timeout = TimeSpan.FromSeconds(30);
                fluentWait.PollingInterval = TimeSpan.FromSeconds(0.5);
                fluentWait.IgnoreExceptionTypes(typeof(NoSuchElementException));
                fluentWait.Message = "Elemento no encontrado";

                var fluentWaitID = fluentWait.Until(x =>
                {
                    Thread.Sleep(1000);
                    var attributeText = x.FindElement(By.XPath(objTest)).GetAttribute(attributeObtain);
                    return attributeText;
                });

                return fluentWaitID;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public string ObtainText(string objTest)
        {
            try
            {
                DefaultWait<IWebDriver> fluentWait = new DefaultWait<IWebDriver>(_driver);
                fluentWait.Timeout = TimeSpan.FromSeconds(30);
                fluentWait.PollingInterval = TimeSpan.FromSeconds(0.5);
                fluentWait.IgnoreExceptionTypes(typeof(NoSuchElementException));
                fluentWait.Message = "Elemento no encontrado";

                var fluentWaitID = fluentWait.Until(x =>
                {
                    Thread.Sleep(1000);
                    var attributeText = x.FindElement(By.XPath(objTest)).Text;
                    return attributeText;
                });

                return fluentWaitID;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public void closeWindow()
        {
            try
            {
                this.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //this.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public void validateSEO(string siteDealer, string dealerName, string carName) 
        {
            try
            {
                this.ValidationContentText(siteDealer, dealerName);
                this.ValidationContentText(siteDealer, carName);
            }
            catch (Exception err)
            {
                throw err;
            }

        }

        public void validateFicha(string text1, string text2)
        {
            try
            {
                string downloadURL = _driver.FindElement(By.XPath("//*[@class='ng-link nm-link']")).GetAttribute("href");
                _driver.Url = downloadURL;
                this.WaitForPageLoad();

                if (text1.Contains("SEDÁN"))
                {
                    text1 = text1 + text2;
                    text1 = text1.Replace("SEDÁN", "SEDAN").Replace("-", "").Replace(" ", "").ToLower();
                }
                else
                {
                    text1 = text1 + text2;
                    text1 = text1.Replace("-", "").Replace(" ", "").ToLower();
                }
                this.ValidationContentText(downloadURL.Replace("-", "").Replace("_", ""), text1);
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public void ValidateImage(IWebElement descriptionCar,string nameImagen)
        {

            try
            {
                var imageVehicle = descriptionCar;
                var nameImage = imageVehicle.GetAttribute("src");
                Regex rx = new Regex(@".(png+?|gif+?|jpe?g+?|bmp+?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                char[] charSeparators = new char[] { '/' };
                arrNameImage = nameImage.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);

                foreach (string valueSplit in arrNameImage)
                {
                    MatchCollection matches = rx.Matches(valueSplit);
                    if (matches.Count.Equals(1))
                    {
                        string valueSplit2 = valueSplit.Replace("-", "").Replace("_","");
                        string descripcion2 = nameImage.ToLower().Replace("sedán", "sedan").Replace(" ", "").Replace("-", "").Replace("_", "");
                        string nameimagen2 = nameImagen.ToLower().Replace(" ", "").Replace("-", "").Replace("_", ""); ;
                        this.ValidationContentText(descripcion2, valueSplit2);
                        this.ValidationContentText(valueSplit2, nameimagen2);
                    }
                }

            }
            catch (Exception err)
            {
                throw err;
            }
            
        }
    }
}
