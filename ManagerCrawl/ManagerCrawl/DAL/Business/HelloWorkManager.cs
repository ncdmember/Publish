using ManagerCrawl.DAL.Interfaces;
using ManagerCrawl.Entities;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.TextFormatting;

namespace ManagerCrawl.DAL.Business
{
    public class HelloWorkManager : IManager
    {
        private ManagerCrawl _manager;
        public HelloWorkManager(ManagerCrawl manager)
        {
            _manager = manager;
        }

        public void GetData(object sender, EventArgs e)
        {
            //string-length: số lượng ký tự trong chuỗi, không tính ký tự cuối cùng của chuỗi
            try
            {
                var inputPage = _manager.webDriver.FindElement(By.XPath("//input[starts-with(@name, 'fwListNaviBtn') and @disabled and substring(@name, string-length(@name)) >= '0' and substring(@name, string-length(@name)) <= '9']"));
                string nameInputPage = (inputPage.GetAttribute("name") + "").Replace("fwListNaviBtn", "");
                if (int.TryParse(nameInputPage, out int pageOut))
                {
                    _manager.txtPage.Text = nameInputPage;
                }

                if (string.IsNullOrEmpty(_manager.txtTotalPage.Text))
                {
                    _manager.txtTotalPage.Text = "20";
                }

                int pageIndex = int.Parse(_manager.txtPage.Text);
                int totalPage = int.Parse(_manager.txtTotalPage.Text);
                for (int i = 0; i < totalPage; i++)
                {
                    int stt = 1;
                    pageIndex += i;
                    _manager.txtPage.Text = pageIndex + "";
                    // Tạo đối tượng HtmlDocument từ chuỗi HTML
                    HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
                    htmlDocument.LoadHtml(_manager.webDriver.PageSource);
                    var elementParent = htmlDocument.DocumentNode.SelectNodes("//a[@id='ID_dispDetailBtn']");
                    foreach (HtmlAgilityPack.HtmlNode element in elementParent)
                    {
                        var attr = element.Attributes["href"];
                        if (attr != null)
                        {
                            string link = attr.Value;
                            if (link.StartsWith("."))
                            {
                                var lastSub = _manager.txtLink.Text.LastIndexOf("/");
                                string domain = _manager.txtLink.Text.Substring(0, lastSub + 1);
                                link = link.Replace("./", domain);
                                link = link.Replace("amp;", "");
                            }
                            LoadDataResult(_manager.webDriver, link, stt, pageIndex);
                            stt++;
                        }
                    }

                    try
                    {
                        IWebElement btnNext = _manager.webDriver.FindElement(By.XPath($"//input[@name='fwListNaviBtn{pageIndex + 1}']"));
                        btnNext.Click();
                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        break;
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show($"Không tìm được trang hiện tại!");
                return;
            }

            _manager.webDriver.Quit();
        }

        private void LoadDataResult(IWebDriver driver, string link, int stt, int page)
        {
            driver.Navigate().GoToUrl(link);

            HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.LoadHtml(driver.PageSource);

            HtmlAgilityPack.HtmlNode row1 = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='ID_kjNo']");
            string cRow1 = row1 != null ? row1.InnerText : "";
            HtmlAgilityPack.HtmlNode row21 = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='ID_jgshMeiKana']");
            HtmlAgilityPack.HtmlNode row22 = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='ID_jgshMei']");
            string cRow2 = string.Concat(row21 != null ? row21.InnerText : "", "\n", row22 != null ? row22.InnerText : "");
            HtmlAgilityPack.HtmlNode row31 = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='ID_szciYbn']");
            HtmlAgilityPack.HtmlNode row32 = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='ID_szci']");
            string cRow3 = string.Concat(row31 != null ? row31.InnerText : "", '\n', row32 != null ? row32.InnerText : "");
            HtmlAgilityPack.HtmlNode row4 = htmlDocument.DocumentNode.SelectSingleNode("//a[@id='ID_hp']");
            string cRow4 = row4 != null ? row4.InnerText : "";
            HtmlAgilityPack.HtmlNode row5 = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='ID_shigotoNy']");
            string cRow5 = row5 != null ? row5.InnerText : "";
            HtmlAgilityPack.HtmlNode row6 = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='ID_hakenUkeoiToShgKeitai']");
            string cRow6 = row6 != null ? row6.InnerText : "";
            HtmlAgilityPack.HtmlNode row7 = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='ID_ttsTel']");
            string cRow7 = row7 != null ? row7.InnerText : "";
            HtmlAgilityPack.HtmlNode row8 = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='ID_ttsFax']");
            string cRow8 = row8 != null ? row8.InnerText : "";
            HtmlAgilityPack.HtmlNode row9 = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='ID_ttsEmail']");
            string cRow9 = row9 != null ? row9.InnerText : "";
            HtmlAgilityPack.HtmlNode row10 = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='ID_kjKbn']");
            string cRow10 = row10 != null ? row10.InnerText : "";
            HtmlAgilityPack.HtmlNode row11 = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='ID_khky']");
            string cRow11 = row11 != null ? row11.InnerText : "";
            HtmlAgilityPack.HtmlNode row12 = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='ID_saiyoNinsu']");
            string cRow12 = row12 != null ? row12.InnerText : "";
            HtmlAgilityPack.HtmlNode row13 = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='ID_sksu']");
            string cRow13 = row13 != null ? row13.InnerText : "";
            _manager.dataTable.Rows.Add(new[] { $"{stt}", $"{page}", $"{link}", $"{cRow1}", $"{cRow10}", $"{cRow2}", $"{cRow3}", $"{cRow4}", $"{cRow5}", $"{cRow6}", $"{cRow11}", $"{cRow7}", $"{cRow8}", $"{cRow9}", $"{cRow12}", $"{cRow13}" });
        }

        public List<InitObjectColumn> InitObjectColumns()
        {
            return new List<InitObjectColumn>()
            {
                new InitObjectColumn(){Name = "Index", Width = 50},
                new InitObjectColumn(){Name = "Page", Width = 50},
                new InitObjectColumn(){Name = "リンク" },
                new InitObjectColumn(){Name = "求人番号" },
                new InitObjectColumn(){Name = "求人区分 " },
                new InitObjectColumn(){Name = "事業所名" },
                new InitObjectColumn(){Name = "所在地" },
                new InitObjectColumn(){Name = "ホームページ" },
                new InitObjectColumn(){Name = "仕事内容" },
                new InitObjectColumn(){Name = "派遣・請負等" },
                new InitObjectColumn(){Name = "基本給（ａ）" },
                new InitObjectColumn(){Name = "電話番号" },
                new InitObjectColumn(){Name = "ＦＡＸ" },
                new InitObjectColumn(){Name = "Ｅメール" },
                new InitObjectColumn(){Name = "採用人数" },
                new InitObjectColumn(){Name = "職種" },
            };
        }
    }
}
