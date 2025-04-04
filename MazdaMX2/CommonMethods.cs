using System;
using System.Text.RegularExpressions;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using SeleniumExtras.WaitHelpers;
using System.Threading;
using System.Collections.Generic;

namespace MazdaMX2Test
{
    public class DealerSession
    {
        private static IWebDriver _driver;
        private WebDriverWait explicitWait;
        private string[] arrNameImage;
        private static string userEnviorement = "mazda-qa:qaqwpozxmn09";
        //private static string enviorement = "qa.mdp.mzd.mx";
        private string enviorement = "www.mazda.mx";
        

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
            _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
        }

        public void ClickMethod(IWebElement buttonTest, IWebDriver _driver)
        {
            Actions action = new Actions(_driver);
            //var elementButton = _driver.FindElement(By.XPath(buttonTest));
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(driver => buttonTest.Displayed);
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
                ClassicAssert.AreEqual(text1, text2);
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
                fluentWait.PollingInterval = TimeSpan.FromSeconds(1);
                fluentWait.IgnoreExceptionTypes(typeof(NoSuchElementException));
                fluentWait.Message = "Elemento no encontrado";

                var fluentWaitID = fluentWait.Until(x =>
                {
                    Thread.Sleep(1500);
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

        public void validateSEO(string siteDealer, string dealerName, string carName, string metaDescripcion, string priceCar) 
        {
            try
            {
                string[] wordCarName = carName.Split(' ');

                for (int i =0; i< wordCarName.Length; i++) {

                    if (wordCarName[i].Length > 0) {

                        if(!wordCarName[i].Contains("CX") && !wordCarName[i].Contains("MX") && !wordCarName[i].Contains("RF") && !wordCarName[i].Contains("BT")) {
                            wordCarName[i] = char.ToUpper(wordCarName[i][0]) + wordCarName[i].Substring(1).ToLower();
                        }
                    }
                }

                carName = string.Join(" ", wordCarName);

                if (siteDealer.Contains("Roadster") || siteDealer.Contains("ROADSTER")) {
                    carName = carName.Replace("Mazda","Roadster").Replace("°","");
                }

                this.ValidationContentText(siteDealer, dealerName);
                this.ValidationContentText(siteDealer, carName);
                this.ValidationContentText(metaDescripcion, "$"+priceCar);
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
                    //text1 = text1 + text2;
                    text1 = text1.Replace("SEDÁN", "SEDAN").Replace("-", "").Replace(" ", "").ToLower();
                }
                else
                {
                    //text1 = text1 + text2;
                    text1 = text1.Replace("-", "").Replace(" ", "").Replace("°","").ToLower();
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

        public System.Collections.IList obtenerListado(string objectFind)
        {

            try
            {
                List<IWebElement> totalVersiones = new List<IWebElement>(_driver.FindElements(By.XPath(objectFind)));
                return totalVersiones;
            }
            catch (Exception err)
            {
                throw err;
            }

        }

        public void validateNumbers(int number1, int number2)
        {

            try
            {
                ClassicAssert.AreEqual(number1, number2);
            }
            catch (Exception err)
            {
                throw err;
            }

        }

        public void reviewPrices(string masterUrlDealer, string masternameDealer, string[,] arrVehiculos, bool seoCheck, bool fichaCheck)
        {
            //string pathFile;
            //string nombreVehiculo;
            //public string[] arrNameImage;
            //string[,] arrVehiculos;
            //string errorMessage;
            //string carPrice2;
            string nameDealer;
            string urlDealer;

            try
            {

                DealerSession dealerSession = new DealerSession(_driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                /*IWebElement ctaAceptarCookies = DealerSession.WaitObjects("//a[@id='opt-accept']", _driver, 1);
                dealerSession.ClickMethod(ctaAceptarCookies,_driver);*/

                //IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", _driver, 0);
                //dealerSession.ClickMethod(lnkVehiculos, _driver);
                //dealerSession.WaitForPageLoad();

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    string textName = "";
                    string carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    string nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    string catVehiculo = arrVehiculos.GetValue(i, 2).ToString();
                    string carname = arrVehiculos.GetValue(i, 3).ToString();
                    string cartype = arrVehiculos.GetValue(i, 4).ToString().Replace("N/A", "");
                    string descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    string modelcar = arrVehiculos.GetValue(i, 5).ToString();
                    string price = arrVehiculos.GetValue(i, 7).ToString();
                    int totVersiones = 0;

                    if (verify == true)
                    {
                        
                        IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", _driver, 0);
                        dealerSession.ClickMethod(lnkVehiculos, _driver);
                        dealerSession.WaitForPageLoad();

                        IWebElement categoVehicle = DealerSession.WaitObjects("//div[@id='categories']/div[@data-category='"+ catVehiculo + "']", _driver, 1);
                        dealerSession.ClickMethod(categoVehicle,_driver);

                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox']/img", _driver, 1);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        dealerSession.OnlyWait();
                        List<IWebElement> textNameCar = new List<IWebElement>(_driver.FindElements(By.XPath("//*[@class='carName']")));
                        List<IWebElement> priceCar = new List<IWebElement>(_driver.FindElements(By.XPath("//*[@class='carPrice']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(_driver.FindElements(By.XPath("//*[@class='carName']")));
                            priceCar = new List<IWebElement>(_driver.FindElements(By.XPath("//*[@class='carPrice']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice = Regex.Replace(carPrice.Substring(8, carPrice.Length - 9), @"[\r\n]+", "");
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice, price);

                        dealerSession.ClickMethod(imgVehiculo, _driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        string txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        
                        if (seoCheck)
                        {
                            nameDealer = _driver.Title;
                            urlDealer = _driver.Url;
                            var metaDescriptionElement = _driver.FindElement(By.XPath("//meta[@name='description']"));
                            string metaDescription = metaDescriptionElement.GetAttribute("content");

                            dealerSession.validateSEO(nameDealer, masternameDealer, textName, metaDescription, price);
                            dealerSession.ValidationContentText(urlDealer, masterUrlDealer);
                        }

                        for (int j = 6; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();
                                totVersiones = totVersiones + 1;

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", _driver, 0);
                                dealerSession.ClickMethod(carVersion, _driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, price2.Length);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", _driver, 1);
                                dealerSession.ClickMethod(btnCotiza, _driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = _driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                _driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        int totalVersiones_2 = dealerSession.obtenerListado("//*[@class='component-navigation-1']/ul/li").Count;
                        dealerSession.validateNumbers(totVersiones, totalVersiones_2);

                        if (fichaCheck)
                        {
                            if (descripcion.Contains("MX-5 RF"))
                            {
                                descripcion = descripcion.Replace("MX-5 RF", "MX-5");
                            }
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        _driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", _driver, 1);
                        dealerSession.ClickMethod(btnBack, _driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }

        }
    }
}
