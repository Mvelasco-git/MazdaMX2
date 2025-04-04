using System;
using System.IO;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Config;
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
                DateTime fechaHoraActual = DateTime.Now;
                var fechaArchivo = fechaHoraActual.ToString("yyyyMMdd_HHmmss");
                var htmlReporter = new ExtentSparkReporter(dir + "\\Test_Execution_Reports" + "\\Automation_Report_" + fechaArchivo + ".html");
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
                using (SLDocument sl = new SLDocument(pathFile))
                {
                    int iRow = 2;
                    int iColumn = 1;
                    arrVehiculos = new string[iRow - 1, 35];

                    while (!string.IsNullOrEmpty(sl.GetCellValueAsString(iRow, 1)))
                    {
                        string[,] newArray = new string[iRow - 1, 35];
                        Array.Copy(arrVehiculos, newArray, arrVehiculos.Length);
                        arrVehiculos = newArray;
                        iColumn = 1;
                        while (!string.IsNullOrEmpty(sl.GetCellValueAsString(iRow, iColumn)))
                        {
                            nombreVehiculo = sl.GetCellValueAsString(iRow, iColumn);
                            arrVehiculos[iRow - 2, iColumn - 1] = nombreVehiculo;
                            iColumn++;
                        }
                        iRow++;
                        iColumn = 1;
                    }
                }

                //userEnviorement = "mazda-qa:qaqwpozxmn09";
                //enviorement = "qa.mdp.mzd.mx";
                //enviorement = "www.mazda.mx";

                seoCheck = true;
                fichaCheck = true;

                string chromeProfilePath = @"C:\Users\mvelasc2\source\repos\Perfil";
                var chromeOptions = new ChromeOptions();
                chromeOptions.AddArgument($"user-data-dir={chromeProfilePath}");
                chromeOptions.AddArguments("--no-sandbox");
                chromeOptions.AddArguments("--disable-extensions");
                chromeOptions.AddArguments("--disable-infobars");
                chromeOptions.AddArguments("--remote-debugging-port=9222");

                new DriverManager().SetUpDriver(new ChromeConfig());
                driver = new ChromeDriver(chromeOptions);
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
                DealerSession dealerSession = new DealerSession(driver);
                String s = string.Empty;
                dealerSession.reviewPrices("acapulco", "Acapulco", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("acueducto", "Acueducto", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("aguascalientes", "Aguascalientes", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("americas", "Américas", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("bajio", "Bajío", arrVehiculos, seoCheck, fichaCheck);
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Buenavista()
        {
            try
            {
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("buenavista", "Buenavista", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("campeche", "Campeche", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("cancun", "Cancún", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("carmen", "Carmen", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("chiapas", "Chiapas", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("chihuahua", "Chihuahua", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("churubusco", "Churubusco", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("ciudad-juarez", "Ciudad Juárez", arrVehiculos, seoCheck, fichaCheck);
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void CdVictoria()
        {
            try
            {
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("ciudad-victoria", "Ciudad Victoria", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("coatzacoalcos", "Coatzacoalcos", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("colima", "Colima", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("cuautla", "Cuautla", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("cuernavaca", "Cuernavaca", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("culiacan", "Culiacán", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("cumbres", "Cumbres", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("del-valle", "Del Valle", arrVehiculos, seoCheck, fichaCheck);
                }
            catch (Exception err)
            {
                throw (err);
            }
        }
        
        [Test]
        public void DiazMiron()
        {
            try
            {
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("diaz-miron", "Díaz Mirón", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("durango", "Durango", arrVehiculos, seoCheck, fichaCheck);
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Ecatepec()
        {
            try
            {
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("ecatepec", "Ecatepec", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("ensenada", "Ensenada", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("galerias", "Galerías", arrVehiculos, seoCheck, fichaCheck);
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void GonzalezGallo()
        {
            try
            {
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("gonzalez-gallo", "González Gallo", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("gonzalitos", "Gonzalitos", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("hermosillo", "Hermosillo", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("interlomas", "Interlomas", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("irapuato", "Irapuato", arrVehiculos, seoCheck, fichaCheck);
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Ixtapaluca()
        {
            try
            {
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("ixtapaluca", "Ixtapaluca", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("la-joya", "La Joya", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("laguna", "Laguna", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("las-torres", "Las Torres", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("lazaro-cardenas", "Lázaro Cárdenas", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("lindavista", "Lindavista", arrVehiculos, seoCheck, fichaCheck);
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void LosCabos()
        {
            try
            {
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("los-cabos", "Los Cabos", arrVehiculos, seoCheck, fichaCheck);
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void LosFuertes()
        {
            try
            {
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("los-fuertes", "Los Fuertes", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("manzanillo", "Manzanillo", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("mazatlan", "Mazatlán", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("metepec", "Metepec", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("mexicali", "Mexicali", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("monclova", "Monclova", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("oaxaca", "Oaxaca", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("obregon", "Obregón", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("pachuca", "Pachuca", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("picacho", "Picacho", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("picacho-suc-san-angel", "Picacho Suc. San Ángel", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("piedras-negras", "Piedras Negras", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("plasencia", "Plasencia", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("polanco", "Polanco", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("puebla", "Puebla", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("ral", "Ral", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("ravisa", "Ravisa", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("ravisa-mexico", "Ravisa México", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("ravisa-uruapan", "Ravisa Uruapan", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("reynosa", "Reynosa", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("salamanca", "Salamanca", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("saltillo", "Saltillo", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("san-luis", "San Luis", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("san-luis-carranza", "San Luis Carranza", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("san-luis-carretera-57", "San Luis Carretera 57", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("santa-anita", "Santa Anita", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("santa-fe", "Santa Fe", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("sendero", "Sendero", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("serdan", "Serdán", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("sureste", "Sureste", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("tabasco", "Tabasco", arrVehiculos, seoCheck, fichaCheck);
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Tamaulipas()
        {
            try
            {
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("tamaulipas", "Tamaulipas", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("tampico", "Tampico", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("tapachula", "Tapachula", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("tepic", "Tepic", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("tijuana", "Tijuana", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("tlahuac", "Tláhuac", arrVehiculos, seoCheck, fichaCheck);
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void Tlaxcala()
        {
            try
            {
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("tlaxcala", "Tlaxcala", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("universidad", "Universidad", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("vallarta", "Vallarta", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("valle-oriente", "Valle Oriente", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("veracruz", "Veracruz", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("xalapa", "Xalapa", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("zacatecas", "Zacatecas", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("zapata", "Zapata", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("zapata-celaya", "Zapata Celaya", arrVehiculos, seoCheck, fichaCheck);
            }
            catch (Exception err)
            {
                throw (err);
            }
        }

        [Test]
        public void ZCorregidora()
        {
            try
            {
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("zapata-corregidora", "Zapata Corregidora", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("zapata-cuautitlan", "Zapata Cuautitlán", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("zapata-lindavista", "Zapata Lindavista", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("zapata-queretaro", "Zapata Querétaro", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("zapata-san-juan-del-rio", "Zapata San Juan del Río", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("zapata-torre-norte", "Zapata Torre Norte", arrVehiculos, seoCheck, fichaCheck);
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
                DealerSession dealerSession = new DealerSession(driver);
                dealerSession.reviewPrices("zapata-zona-esmeralda", "Zapata Zona Esmeralda", arrVehiculos, seoCheck, fichaCheck);
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
                        screenShotPath = Capture(driver, TestContext.CurrentContext.Test.Name);
                        _test.Log(logstatus, "Estatus de la Prueba: " + logstatus);
                        _test.Log(logstatus, "Print" + _test.AddScreenCaptureFromPath(screenShotPath));
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
                DateTime fechaTakeScreen = DateTime.Now;
                var fechaArchivo = fechaTakeScreen.ToString("yyyyMMdd_HHmmss");

                Thread.Sleep(4000);
                ITakesScreenshot ts = (ITakesScreenshot)driver;
                Screenshot screenshot = ts.GetScreenshot();
                string pth = System.Reflection.Assembly.GetCallingAssembly().CodeBase;
                var dir = AppDomain.CurrentDomain.BaseDirectory.Replace("\\bin\\Debug", "");
                DirectoryInfo di = Directory.CreateDirectory(dir + "\\Defect_Screenshots\\");
                string finalpth = pth.Substring(0, pth.LastIndexOf("bin")) + "\\Defect_Screenshots\\" + screenShotName + "_" + fechaTakeScreen.ToString("yyyyMMdd_HHmmss") + ".png";
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
