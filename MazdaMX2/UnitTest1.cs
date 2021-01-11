using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using SpreadsheetLight;

using MazdaMX2Test;

namespace SeleniumExtentReportTest
{
    [TestFixture]
    public class SeleniumExtentReport
    {
        public IWebDriver driver;
        protected ExtentReports _extent;
        protected ExtentTest _test;
        public WebDriverWait wait;

        public string pathFile;
        public string nombreVehiculo;
        public string userEnviorement;
        public string enviorement;
        public string[] arrNameImage;
        public string[,] arrVehiculos;
        public string carPrice2;
        public string errorMessage;
        public string nameDealer;
        public string urlDealer;
        public bool seoCheck;
        public bool fichaCheck;

        //Crear la dirección y el template del reporte en HTML
        [OneTimeSetUp]
        public void BeforeClass()
        {
            try
            {
                //Crear directorio del reporte y agregar datos

                _extent = new ExtentReports();
                var dir = AppDomain.CurrentDomain.BaseDirectory.Replace("\\bin\\Debug", "");
                DirectoryInfo di = Directory.CreateDirectory(dir + "\\Test_Execution_Reports");
                var htmlReporter = new ExtentHtmlReporter(dir + "\\Test_Execution_Reports" + "\\Automation_Report" + ".html");
                _extent.AddSystemInfo("Environment", "Mazda STAGE");
                _extent.AddSystemInfo("User Name", "Manuel Velasco");
                _extent.AttachReporter(htmlReporter);
            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        //Obtener el nombre de la ejecución actual
     
        [SetUp]
        public void BeforeTest()
        {
            try
            {
                _test = _extent.CreateTest(TestContext.CurrentContext.Test.Name);
                var dir = AppDomain.CurrentDomain.BaseDirectory.Replace("\\bin\\Debug", "");
                pathFile = dir + "Dealers.xlsx";
                SLDocument sl = new SLDocument(pathFile);
            
                int iRow = 2;
                int iColumn= 1;
                arrVehiculos = new string[iRow - 1, 25];

                while (!string.IsNullOrEmpty(sl.GetCellValueAsString(iRow, 1)))
                {
                    string[,] newArray = new string[iRow-1, 25];
                    Array.Copy(arrVehiculos, newArray, arrVehiculos.Length);
                    arrVehiculos = newArray;
                    iColumn = 1;
                    while (!string.IsNullOrEmpty(sl.GetCellValueAsString(iRow, iColumn)))
                    {
                    nombreVehiculo = sl.GetCellValueAsString(iRow, iColumn);
                    arrVehiculos[iRow-2, iColumn-1] = nombreVehiculo;
                    iColumn++;
                    }
                iRow++;
                iColumn = 1;
                }

                //userEnviorement = "mazda-stage:stage1209";
                //enviorement = "stage.mazda.mx";
                enviorement = "www.mazda.mx";
                seoCheck = false;
                fichaCheck = true;
                driver = new ChromeDriver();
            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        [Test]
        public void Acapulco()
        {
            try
            {
                String masterUrlDealer = "acapulco";
                String masternameDealer = "Acapulco";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")){dealerSession.closeWindow();}

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();
                    
                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }
                        
                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                
                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']", 
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }
        
        [Test]
        public void Acueducto()
        {
            try
            {

                string masterUrlDealer = "acueducto";
                string masternameDealer = "Acueducto";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Aguascalientes()
        {
            try
            {

                string masterUrlDealer = "aguascalientes";
                string masternameDealer = "Aguascalientes";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Americas()
        {
            try
            {

                string masterUrlDealer = "americas";
                string masternameDealer = "Américas";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Bajio()
        {
            try
            {

                string masterUrlDealer = "bajio";
                string masternameDealer = "Bajío";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }
        
        [Test]
        public void Campeche()
        {
            try
            {

                string masterUrlDealer = "campeche";
                string masternameDealer = "Campeche";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Cancun()
        {
            try
            {

                string masterUrlDealer = "cancun";
                string masternameDealer = "Cancún";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Carmen()
        {
            try
            {

                string masterUrlDealer = "carmen";
                string masternameDealer = "Carmen";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Chiapas()
        {
            try
            {

                string masterUrlDealer = "chiapas";
                string masternameDealer = "Chiapas";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Chihuahua()
        {
            try
            {

                string masterUrlDealer = "chihuahua";
                string masternameDealer = "Chihuahua";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Churubusco()
        {
            try
            {

                string masterUrlDealer = "churubusco";
                string masternameDealer = "Churubusco";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void CdJuarez()
        {
            try
            {

                string masterUrlDealer = "ciudad-juarez";
                string masternameDealer = "Ciudad Juárez";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Coatzacoalcos()
        {
            try
            {

                string masterUrlDealer = "coatzacoalcos";
                string masternameDealer = "Coatzacoalcos";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Colima()
        {
            try
            {

                string masterUrlDealer = "colima";
                string masternameDealer = "Colima";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Cuautla()
        {
            try
            {

                string masterUrlDealer = "cuautla";
                string masternameDealer = "Cuautla";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Cuernavaca()
        {
            try
            {

                string masterUrlDealer = "cuernavaca";
                string masternameDealer = "Cuernavaca";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Culiacan()
        {
            try
            {

                string masterUrlDealer = "culiacan";
                string masternameDealer = "Culiacán";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Cumbres()
        {
            try
            {

                string masterUrlDealer = "cumbres";
                string masternameDealer = "Cumbres";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void DelValle()
        {
            try
            {

                string masterUrlDealer = "del-valle";
                string masternameDealer = "Del Valle";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Durango()
        {
            try
            {

                string masterUrlDealer = "durango";
                string masternameDealer = "Durango";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Ensenada()
        {
            try
            {

                string masterUrlDealer = "ensenada";
                string masternameDealer = "Ensenada";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Galerias()
        {
            try
            {

                string masterUrlDealer = "galerias";
                string masternameDealer = "Galerías";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Gonzalitos()
        {
            try
            {

                string masterUrlDealer = "gonzalitos";
                string masternameDealer = "Gonzalitos";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Hermosillo()
        {
            try
            {

                string masterUrlDealer = "hermosillo";
                string masternameDealer = "Hermosillo";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Interlomas()
        {
            try
            {

                string masterUrlDealer = "interlomas";
                string masternameDealer = "Interlomas";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Irapuato()
        {
            try
            {

                string masterUrlDealer = "irapuato";
                string masternameDealer = "Irapuato";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void LaJoya()
        {
            try
            {

                string masterUrlDealer = "la-joya";
                string masternameDealer = "La Joya";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Laguna()
        {
            try
            {

                string masterUrlDealer = "laguna";
                string masternameDealer = "Laguna";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void LasTorres()
        {
            try
            {

                string masterUrlDealer = "las-torres";
                string masternameDealer = "Las Torres";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void LazaroCardenas()
        {
            try
            {

                string masterUrlDealer = "lazaro-cardenas";
                string masternameDealer = "Lázaro Cárdenas";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Lindavista()
        {
            try
            {

                string masterUrlDealer = "lindavista";
                string masternameDealer = "Lindavista";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Manzanillo()
        {
            try
            {

                string masterUrlDealer = "manzanillo";
                string masternameDealer = "Manzanillo";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Mazatlan()
        {
            try
            {

                string masterUrlDealer = "mazatlan";
                string masternameDealer = "Mazatlan";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Metepec()
        {
            try
            {

                string masterUrlDealer = "metepec";
                string masternameDealer = "Metepec";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Mexicali()
        {
            try
            {

                string masterUrlDealer = "mexicali";
                string masternameDealer = "Mexicali";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Monclova()
        {
            try
            {

                string masterUrlDealer = "monclova";
                string masternameDealer = "Monclova";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Oaxaca()
        {
            try
            {

                string masterUrlDealer = "oaxaca";
                string masternameDealer = "Oaxaca";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Obregon()
        {
            try
            {

                string masterUrlDealer = "obregon";
                string masternameDealer = "Obregón";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Pachuca()
        {
            try
            {

                string masterUrlDealer = "pachuca";
                string masternameDealer = "Pachuca";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Picacho()
        {
            try
            {

                string masterUrlDealer = "picacho";
                string masternameDealer = "Picacho";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void PicachoSuc()
        {
            try
            {

                string masterUrlDealer = "picacho-suc-san-angel";
                string masternameDealer = "Picacho San Ángel";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void PiedrasNegras()
        {
            try
            {

                string masterUrlDealer = "piedras-negras";
                string masternameDealer = "Piedras Negras";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Plasencia()
        {
            try
            {

                string masterUrlDealer = "plasencia";
                string masternameDealer = "Plasencia";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Polanco()
        {
            try
            {

                string masterUrlDealer = "polanco";
                string masternameDealer = "Polanco";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Puebla()
        {
            try
            {

                string masterUrlDealer = "puebla";
                string masternameDealer = "Puebla";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Ral()
        {
            try
            {

                string masterUrlDealer = "ral";
                string masternameDealer = "Ral";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Ravisa()
        {
            try
            {

                string masterUrlDealer = "ravisa";
                string masternameDealer = "Ravisa";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void RavisaMexico()
        {
            try
            {

                string masterUrlDealer = "ravisa-mexico";
                string masternameDealer = "Ravisa México";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void RavisaUruapan()
        {
            try
            {

                string masterUrlDealer = "ravisa-uruapan";
                string masternameDealer = "Ravisa Uruapan";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Reynosa()
        {
            try
            {

                string masterUrlDealer = "reynosa";
                string masternameDealer = "Reynosa";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Salamanca()
        {
            try
            {

                string masterUrlDealer = "salamanca";
                string masternameDealer = "Salamanca";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Saltillo()
        {
            try
            {

                string masterUrlDealer = "saltillo";
                string masternameDealer = "Saltillo";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void SanLuis()
        {
            try
            {

                string masterUrlDealer = "san-luis";
                string masternameDealer = "San Luis";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void SanLuisCarranza()
        {
            try
            {

                string masterUrlDealer = "san-luis-carranza";
                string masternameDealer = "San Luis Carranza";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void SanLuisCarretera()
        {
            try
            {

                string masterUrlDealer = "san-luis-carretera-57";
                string masternameDealer = "San Luis Carretera 57";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void SantaAnita()
        {
            try
            {

                string masterUrlDealer = "santa-anita";
                string masternameDealer = "Santa Anita";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void SantaFe()
        {
            try
            {

                string masterUrlDealer = "santa-fe";
                string masternameDealer = "Santa Fe";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Sendero()
        {
            try
            {

                string masterUrlDealer = "sendero";
                string masternameDealer = "Sendero";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Serdan()
        {
            try
            {

                string masterUrlDealer = "serdan";
                string masternameDealer = "Serdán";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Sureste()
        {
            try
            {

                string masterUrlDealer = "sureste";
                string masternameDealer = "Sureste";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Tabasco()
        {
            try
            {

                string masterUrlDealer = "tabasco";
                string masternameDealer = "Tabasco";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Tampico()
        {
            try
            {

                string masterUrlDealer = "tampico";
                string masternameDealer = "Tampico";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Tapachula()
        {
            try
            {

                string masterUrlDealer = "tapachula";
                string masternameDealer = "Tapachula";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Tepic()
        {
            try
            {

                string masterUrlDealer = "tepic";
                string masternameDealer = "Tepic";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Tijuana()
        {
            try
            {

                string masterUrlDealer = "tijuana";
                string masternameDealer = "Tijuana";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Tlahuac()
        {
            try
            {

                string masterUrlDealer = "tlahuac";
                string masternameDealer = "Tláhuac";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Universidad()
        {
            try
            {

                string masterUrlDealer = "universidad";
                string masternameDealer = "Universidad";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Vallarta()
        {
            try
            {

                string masterUrlDealer = "vallarta";
                string masternameDealer = "Vallarta";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void ValleOriente()
        {
            try
            {

                string masterUrlDealer = "valle-oriente";
                string masternameDealer = "Valle Oriente";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Veracruz()
        {
            try
            {

                string masterUrlDealer = "veracruz";
                string masternameDealer = "Veracruz";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Xalapa()
        {
            try
            {

                string masterUrlDealer = "xalapa";
                string masternameDealer = "Xalapa";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Zacatecas()
        {
            try
            {

                string masterUrlDealer = "zacatecas";
                string masternameDealer = "Zacatecas";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Zapata()
        {
            try
            {

                string masterUrlDealer = "zapata";
                string masternameDealer = "Zapata";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void ZCelaya()
        {
            try
            {

                string masterUrlDealer = "zapata-celaya";
                string masternameDealer = "Zapata Celaya";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void ZCuautitlan()
        {
            try
            {

                string masterUrlDealer = "zapata-cuautitlan";
                string masternameDealer = "Zapata Cuautitlán";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void ZLindavista()
        {
            try
            {

                string masterUrlDealer = "zapata-lindavista";
                string masternameDealer = "Zapata Lindavista";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void zQueretaro()
        {
            try
            {

                string masterUrlDealer = "zapata-queretaro";
                string masternameDealer = "Zapata Querétaro";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void ZSanJuan()
        {
            try
            {

                string masterUrlDealer = "zapata-san-juan-del-rio";
                string masternameDealer = "Zapata San Juan del Río";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void ZTorreNorte()
        {
            try
            {

                string masterUrlDealer = "zapata-torre-norte";
                string masternameDealer = "Zapata Torre Norte";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void ZZonaEsmeralda()
        {
            try
            {

                string masterUrlDealer = "zapata-zona-esmeralda";
                string masternameDealer = "Zapata Zona Esmeralda";

                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.IngresarURL(userEnviorement, enviorement, masterUrlDealer);

                IWebElement lnkVehiculos = DealerSession.WaitObjects("//*[@data-analytics-link-description='VEHÍCULOS']", driver, 0);
                //dealerSession.WaitForClick("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                //dealerSession.ClickMethod("//*[@class='mdp-flexiblecontent-alertbox-exitbtn-cta icon-mazda-mx-close']");
                dealerSession.ClickMethod(lnkVehiculos, driver);
                dealerSession.WaitForPageLoad();

                if (enviorement.Contains("stage")) { dealerSession.closeWindow(); }

                for (int i = 0; i < arrVehiculos.GetLength(0); i++)
                {
                    String textName = "";
                    String carPrice = "";
                    bool verify = Convert.ToBoolean(arrVehiculos.GetValue(i, 0));
                    String nameImage = arrVehiculos.GetValue(i, 1).ToString();
                    String carname = arrVehiculos.GetValue(i, 2).ToString();
                    String cartype = arrVehiculos.GetValue(i, 3).ToString().Replace("N/A", "");
                    String descripcion = carname + " " + cartype;
                    descripcion = descripcion.Trim();
                    String modelcar = arrVehiculos.GetValue(i, 4).ToString();
                    String price = arrVehiculos.GetValue(i, 6).ToString();

                    if (verify == true)
                    {
                        IWebElement imgVehiculo = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + descripcion + "']/div[@class='carBox ng-mazda']/img", driver, 0);
                        dealerSession.ValidateImage(imgVehiculo, nameImage);

                        List<IWebElement> textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                        List<IWebElement> priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                        textName = textNameCar[i].Text;
                        carPrice = priceCar[i].Text;

                        while (textName.Length == 0)
                        {
                            textNameCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='type2 carName table-title']")));
                            priceCar = new List<IWebElement>(driver.FindElements(By.XPath("//*[@class='carPrice table-title']")));
                            textName = textNameCar[i].Text;
                            carPrice = priceCar[i].Text;
                        }

                        carPrice2 = carPrice.Substring(8, 7);
                        dealerSession.ValidationText(textName, descripcion + " " + modelcar);
                        dealerSession.ValidationText(carPrice2, price);

                        dealerSession.ClickMethod(imgVehiculo, driver);
                        dealerSession.WaitIsVisible("//*[@class='mde-specs-title']");
                        String txtVehicle = dealerSession.ObtainText("//*[@class='mde-specs-title']");
                        dealerSession.ValidationText(descripcion + " " + modelcar, txtVehicle);

                        nameDealer = driver.Title;
                        urlDealer = driver.Url;

                        if (seoCheck)
                        {
                            dealerSession.validateSEO(nameDealer, masternameDealer, textName);
                        }

                        dealerSession.ValidationContentText(urlDealer, masterUrlDealer);

                        for (int j = 5; j < arrVehiculos.GetUpperBound(1); j += 5)
                        {
                            if (arrVehiculos[i, j] != null)
                            {
                                String versionCar = arrVehiculos.GetValue(i, j).ToString().Trim();
                                String price2 = arrVehiculos.GetValue(i, j + 1).ToString();
                                String hpTest = arrVehiculos.GetValue(i, j + 2).ToString();
                                String torqueTest = arrVehiculos.GetValue(i, j + 3).ToString();
                                String motorTest = arrVehiculos.GetValue(i, j + 4).ToString();

                                IWebElement carVersion = DealerSession.WaitObjects("//*[@data-analytics-link-description='" + versionCar + "']", driver, 0);
                                dealerSession.ClickMethod(carVersion, driver);
                                String price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");

                                while (price3.Length == 0)
                                {
                                    price3 = dealerSession.ObtainText("//*[@class='mde-price-detail-ms active']");
                                }

                                price3 = price3.Substring(9, 7);
                                String hpVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[1]");
                                String tVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[2]");
                                String mVehiculo = dealerSession.ObtainText("(//*[@class='mde-specs-ms__stats--item active']/div[@class='item-stats']/div[@class='item-stats--value'])[3]");

                                dealerSession.ValidationText(price2, price3);
                                dealerSession.ValidationText(hpTest, hpVehiculo);
                                dealerSession.ValidationText(torqueTest, tVehiculo);
                                dealerSession.ValidationText(motorTest, mVehiculo);

                                IWebElement btnCotiza = DealerSession.WaitObjects("//*[@data-analytics-link-description='COTIZA TU MAZDA']", driver, 1);
                                dealerSession.ClickMethod(btnCotiza, driver);
                                dealerSession.WaitForPageLoad();

                                nameDealer = driver.Title;
                                dealerSession.ValidationContentText(nameDealer, masternameDealer);

                                String vehiculo = dealerSession.ObtainAttribute("//*[@class='select2-selection__rendered active-input']", "title");
                                String versionCar2 = dealerSession.ConvertText(versionCar);
                                String version = dealerSession.ObtainAttribute("//*[@title='" + versionCar2 + "']",
                                                                               "title");
                                dealerSession.ValidationText(descripcion + " " + modelcar, vehiculo);
                                dealerSession.ValidationText(versionCar2, version);

                                driver.Navigate().Back();
                                dealerSession.WaitForPageLoad();
                            }
                            else
                            {
                                j = arrVehiculos.GetLength(1);
                            }
                        }

                        if (fichaCheck)
                        {
                            dealerSession.validateFicha(descripcion, modelcar);
                        }

                        driver.Navigate().Back();
                        dealerSession.WaitForPageLoad();
                        IWebElement btnBack = DealerSession.WaitObjects("//*[@data-analytics-link-description='REGRESAR A VEHÍCULOS']", driver, 1);
                        dealerSession.ClickMethod(btnBack, driver);
                        dealerSession.WaitForPageLoad();

                    }

                }
            }
            catch (Exception err)
            {
                throw (err);
            }
        }
        
        //Finalmente ejecutar y registrar los detalles en el reporte

        [TearDown]
        //Agregar Codigo para el TearDown
        public void AfterTest()
        {
            try
            {
                var status = TestContext.CurrentContext.Result.Outcome.Status;
                var stacktrace = "" + TestContext.CurrentContext.Result.StackTrace + "";
                var errorMessage = TestContext.CurrentContext.Result.Message;

                Status logstatus;
                switch (status)
                {
                    case TestStatus.Failed:
                        logstatus = Status.Fail;
                        string screenShotPath = Capture(driver, TestContext.CurrentContext.Test.Name);
                        _test.Log(logstatus, "Estatus de la Prueba: " + logstatus + " – " + errorMessage);
                        _test.Log(logstatus, "Print del Error: " + _test.AddScreenCaptureFromPath(screenShotPath));
                        break;
                    case TestStatus.Skipped:
                        logstatus = Status.Skip;
                        _test.Log(logstatus, "Estatus de la Prueba: " + logstatus);
                        break;
                    default:
                        logstatus = Status.Pass;
                        _test.Log(logstatus, "Estatus de la Prueba: " + logstatus);
                        break;
                }
                driver.Quit();
            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        //Se crea y almacena el reporte en la ruta establecida
        [OneTimeTearDown]
        public void AfterClass()
        {
            try
            {
                _extent.Flush();
            }
            catch (Exception e)
            {
                throw (e);
            }
            //driver.Quit();
        }

        //Capturar Pantallas para agregar al reporte.
        private string Capture(IWebDriver driver, string screenShotName)
        {
            string localpath = "";
            try
            {
                Thread.Sleep(4000);
                ITakesScreenshot ts = (ITakesScreenshot)driver;
                Screenshot screenshot = ts.GetScreenshot();
                string pth = System.Reflection.Assembly.GetCallingAssembly().CodeBase;
                var dir = AppDomain.CurrentDomain.BaseDirectory.Replace("\\bin\\Debug", "");
                DirectoryInfo di = Directory.CreateDirectory(dir + "\\Defect_Screenshots\\");
                string finalpth = pth.Substring(0, pth.LastIndexOf("bin")) + "\\Defect_Screenshots\\" + screenShotName + ".png";
                localpath = new Uri(finalpth).LocalPath;
                screenshot.SaveAsFile(localpath);
            }
            catch (Exception e)
            {
                throw (e);
            }
            return localpath;
        }
    }
}
