using ManagerCrawl.DAL.Interfaces;
using ManagerCrawl.Entities;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ManagerCrawl.DAL.Business
{
    public class MyNaviManager : IManager
    {
        private ManagerCrawl _manager;
        public MyNaviManager(ManagerCrawl manager)
        {
            _manager = manager;
        }

        public void GetData(object sender, EventArgs e)
        {
            //string-length: số lượng ký tự trong chuỗi, không tính ký tự cuối cùng của chuỗi
            try
            {
                var inputPage = _manager.webDriver.FindElement(By.CssSelector(".pagingLink li > span"));
                string nameInputPage = inputPage.Text;
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
                    int index = i;
                    int stt = (index * 100) + 1;
                    pageIndex += i;
                    _manager.txtPage.Text = pageIndex + "";
                    // Tạo đối tượng HtmlDocument từ chuỗi HTML
                    HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
                    htmlDocument.LoadHtml(_manager.webDriver.PageSource);
                    var elementParent = htmlDocument.DocumentNode.SelectNodes("//a[@class='js-add-examination-list-text']");
                    foreach (HtmlAgilityPack.HtmlNode element in elementParent)
                    {
                        var attr = element.Attributes["href"];
                        if (attr != null)
                        {
                            string link = attr.Value;
                            if (link.StartsWith("/"))
                            {
                                link = $"{_manager.fullDomain}{link}";
                            }
                            LoadDataResult(link, stt, pageIndex);
                            stt++;
                            Console.WriteLine(DateTime.Now);
                            Thread.Sleep(1000);
                        }
                    }

                    try
                    {
                        IWebElement btnNext = _manager.webDriver.FindElement(By.CssSelector("li.right"));
                        btnNext.Click();
                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không tìm được trang hiện tại!");
                return;
            }
        }

        public async Task LoadDataResult(string link, int stt, int page)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage httpResponse = await client.GetAsync(link);
                    httpResponse.EnsureSuccessStatusCode();
                    string html = await httpResponse.Content.ReadAsStringAsync();

                    HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
                    htmlDocument.LoadHtml(html);

                    HtmlAgilityPack.HtmlNode row1 = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='category']/ul");
                    string cRow1 = row1 != null ? row1.InnerText.Trim() : "";
                    HtmlAgilityPack.HtmlNode row2 = htmlDocument.DocumentNode.SelectSingleNode("//tr[@id='courseTitle']/td[@class='sameSize']");
                    string cRow2 = row2 != null ? row2.InnerText.Trim() : "";
                    HtmlAgilityPack.HtmlNode row3 = htmlDocument.DocumentNode.SelectSingleNode("//tr[@id='employmentStatus']/td[@class='sameSize']");
                    string cRow3 = row3 != null ? row3.InnerText.Trim() : "";
                    HtmlAgilityPack.HtmlNode row4 = htmlDocument.DocumentNode.SelectSingleNode("//td[@id='accessInfoListDescText110']");
                    string cRow4 = row4 != null ? row4.InnerText.Trim() : "";
                    HtmlAgilityPack.HtmlNode row5 = htmlDocument.DocumentNode.SelectSingleNode("//td[@id='accessInfoListDescText120']");
                    string cRow5 = row5 != null ? row5.InnerText.Trim() : "";
                    HtmlAgilityPack.HtmlNode row6 = htmlDocument.DocumentNode.SelectSingleNode("//td[@id='accessInfoListDescText130']");
                    string cRow6 = row6 != null ? row6.InnerText.Trim() : "";

                    _manager.dataTable.Rows.Add(new[] { $"{stt}", $"{page}", $"{cRow1}", $"{cRow2}", $"{cRow3}", $"{cRow4}", $"{cRow5}", $"{cRow6}" });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public List<InitObjectColumn> InitObjectColumns()
        {
            return new List<InitObjectColumn>()
            {
                new InitObjectColumn(){Name = "Index", Width = 50},
                new InitObjectColumn(){Name = "Page", Width = 50},
                new InitObjectColumn(){Name = "業種" },
                new InitObjectColumn(){Name = "コース名 " },
                new InitObjectColumn(){Name = "雇用形態 " },
                new InitObjectColumn(){Name = "問合せ先" },
                new InitObjectColumn(){Name = "URL" },
                new InitObjectColumn(){Name = "E-MAIL" },
            };
        }
    }
}
