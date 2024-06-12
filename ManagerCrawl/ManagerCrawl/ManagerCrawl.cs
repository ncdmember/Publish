using ManagerCrawl.DAL.Business;
using ManagerCrawl.DAL.Interfaces;
using ManagerCrawl.Entities;
using Microsoft.Win32;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;

namespace ManagerCrawl
{
    public partial class ManagerCrawl : Form
    {
        public IWebDriver webDriver;
        private IManager _manager;
        public string fullDomain;
        public DataTable dataTable;
        public ManagerCrawl()
        {
            InitializeComponent();
            OnInit();
        }

        private void OnInit()
        {
            btnGetData.Enabled = false;
            txtTotalPage.Text = "20";
            try
            {
                string chromeVersionApp = FindChromeVersion();
                string chromeVersionDri = ConfigurationManager.AppSettings["ChromeDriverVersion"];
                if (!chromeVersionApp.Equals(chromeVersionDri))
                {
                    MessageBox.Show("Updating Chrome. Vui lòng đợi!!!");
                    var chromeDriverFolder = new DriverManager().SetUpDriver(new ChromeConfig(), VersionResolveStrategy.MatchingBrowser);
                    //var chromeDriverFolder = @"D:\WinForm\HelloWork\bin\Debug\Chrome\124.0.6367.201\X64\chromedriver.exe";

                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings["ChromeDriverVersion"].Value = chromeVersionApp;
                    config.AppSettings.Settings["ChromeDriverPath"].Value = chromeDriverFolder;
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("appSettings");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private string FindChromeVersion()
        {
            string version = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string chromePath = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\chrome.exe", null, null);
                if (chromePath == null)
                {
                    string[] commonChromePaths = {
                        @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                        @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                        Environment.GetEnvironmentVariable("ProgramFiles") + @"\Google\Chrome\Application\chrome.exe",
                        Environment.GetEnvironmentVariable("ProgramFiles(x86)") + @"\Google\Chrome\Application\chrome.exe",
                    };
                    foreach (string path in commonChromePaths)
                    {
                        if (File.Exists(path))
                        {
                            chromePath = path;
                        }
                    }
                }

                version = FileVersionInfo.GetVersionInfo(chromePath).FileVersion;
            }
            return version;
        }

        private void btnIndeed_Click(object sender, EventArgs e)
        {
            _manager = new IndeedManager(this);
            dataTable = new DataTable("Indeed");
            LoadHeaderDataResult();
            txtLink.Text = "https://jp.indeed.com/jobs?q=%E6%AD%A3%E7%A4%BE%E5%93%A1&l=%E6%9D%B1%E4%BA%AC%E9%83%BD";
        }

        private void btnHelloWork_Click(object sender, EventArgs e)
        {
            _manager = new HelloWorkManager(this);
            dataTable = new DataTable("HelloWork");
            LoadHeaderDataResult();
            txtLink.Text = "https://www.hellowork.mhlw.go.jp/kensaku/GECA110010.do?action=initDisp&screenId=GECA110010";
        }

        private void btnDolaJp_Click(object sender, EventArgs e)
        {
            _manager = new DolaJpManager(this);
            dataTable = new DataTable("DolaJp");
            LoadHeaderDataResult();
            txtLink.Text = "https://doda.jp/DodaFront/View/JobSearchList.action?sid=TopSearch&usrclk=PC_logout_kyujinSearchArea_searchButton";
            Uri uri = new Uri(txtLink.Text);
            string scheme = uri.Scheme;
            string domain = uri.Host;
            fullDomain = scheme + "://" + domain;
        }

        private void btnMyNavi_Click(object sender, EventArgs e)
        {
            _manager = new MyNaviManager(this);
            dataTable = new DataTable("MyNavi");
            LoadHeaderDataResult();
            txtLink.Text = "https://job.mynavi.jp/25/pc/search/query.html?OC:160/QuickSearch";
        }

        private void LoadHeaderDataResult()
        {
            var inits = _manager.InitObjectColumns();
            foreach (var item in inits)
            {
                dataTable.Columns.Add(item.Name, typeof(string));
            }

            var initObjects = inits.Take(4).ToList();
            dgvResult.ColumnCount = initObjects.Count;
            for (int i = 0; i < initObjects.Count; i++)
            {
                InitObjectColumn initObject = initObjects[i];
                dgvResult.Columns[i].Name = initObject.Name;
                dgvResult.Columns[i].HeaderText = initObject.Text ?? initObject.Name;
                if (initObject.Width.HasValue)
                    dgvResult.Columns[i].Width = initObject.IsWidthPercent ? (int)(dgvResult.Width * (int)initObject.Width / 100) : (int)initObject.Width;
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtLink.Text))
                return;

            string profileDir = @"C:\Users\Admin\AppData\Local\Google\Chrome\User Data";
            string profileName = "Profile 4";
            string profilePath = Path.Combine(profileDir, profileName);

            string chromePath = ConfigurationManager.AppSettings["ChromeDriverPath"];
            var chrome = ChromeDriverService.CreateDefaultService(chromePath);
            chrome.HideCommandPromptWindow = true;

            var options = new ChromeOptions();
            options.AddArgument("--disable-gpu");
            options.AddArgument("--log-level=3");
            options.AddArgument("--test-type");
            options.AddArgument("--disable-default-apps");
            options.AddArgument("--disable-volume-adjust-sound");
            options.AddArgument("--mute-audio");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddExcludedArgument("enable-automation");
            //options.AddAdditionalCapability("useAutomationExtension", false);
            options.AddUserProfilePreference("credentials_enable_service", false);
            options.AddUserProfilePreference("profile.password_manager_enabled", false);
            options.AddArgument($@"--user-data-dir={profileDir}");
            options.AddArgument($"--profile-directory={profileName}");
            webDriver = new ChromeDriver(chrome);

            webDriver.Navigate().GoToUrl(txtLink.Text);

            btnGetData.Enabled = true;
        }

        private void btnGetData_Click(object sender, EventArgs e)
        {
            _manager.GetData(sender, e);
        }

        private void btnExcel_Click(object sender, EventArgs e)
        {
            // Saving the Excel file
            string fileName = $"{dataTable.TableName}_{DateTime.Now.ToString("ddMMyyyy_HHmmss")}.xlsx";
            //workbook.Save($"{fileName}");
            FileInfo excelFile = new FileInfo(fileName);
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            const string ExcelColumn = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            using (ExcelPackage package = new ExcelPackage(excelFile))
            {
                // Tạo một sheet mới
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add($"{dataTable.TableName}");
                string lastColumn = ExcelColumn[dataTable.Columns.Count - 1].ToString();
                using (var range = worksheet.Cells[$"A1:{lastColumn}1"])
                {
                    range.Style.Fill.SetBackground(Color.Yellow, ExcelFillStyle.Solid);
                    range.Style.Font.Bold = true;
                    range.Style.Font.Size = 12;
                }
                worksheet.Cells[$"A1:{lastColumn}1"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[$"A1:{lastColumn}1"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[$"A1:{lastColumn}1"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[$"A1:{lastColumn}1"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[$"A1:{lastColumn}1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[$"A1:{lastColumn}1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Columns[3, dataTable.Columns.Count].Width = 40;

                // Ghi dữ liệu từ DataTable vào Excel
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);

                // Lưu file Excel
                package.Save();
            }

            var process = new System.Diagnostics.Process();
            process.StartInfo = new System.Diagnostics.ProcessStartInfo() { UseShellExecute = true, FileName = fileName };

            process.Start();
        }

        private void txtTotalPage_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Kiểm tra xem ký tự nhập vào có phải là số hay không
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                // Nếu không phải số, hủy sự kiện KeyPress
                e.Handled = true;
            }
        }
    }
}
