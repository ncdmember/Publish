using ManagerCrawl.DAL.Interfaces;
using ManagerCrawl.Entities;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Windows.Media.TextFormatting;

namespace ManagerCrawl.DAL.Business
{
    public class DolaJpManager : IManager
    {
        private ManagerCrawl _manager;
        public DolaJpManager(ManagerCrawl manager)
        {
            _manager = manager;
        }

        public void GetData(object sender, EventArgs e)
        {
            try
            {
                int totalPage = int.Parse(_manager.txtTotalPage.Text);
                for (int i = 0; i < totalPage; i++)
                {
                    var linkPage = _manager.txtLink.Text;
                    if (string.IsNullOrEmpty(linkPage))
                        break;
                    // Tạo đối tượng HtmlDocument từ chuỗi HTML
                    HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
                    htmlDocument.LoadHtml(_manager.webDriver.PageSource);

                    var inputPage = _manager.webDriver.FindElement(By.XPath("//span[@class='current']"));
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
                    int stt = 1;

                    string xpathExpression = "//a[@class='btnJob03 _JobListToDetail']";
                    var elementParent = htmlDocument.DocumentNode.SelectNodes(xpathExpression);
                    string domain = "https://doda.jp/";
                    foreach (HtmlAgilityPack.HtmlNode element in elementParent)
                    {
                        var attr = element.Attributes["href"];
                        if (attr != null)
                        {
                            string link = attr.Value;
                            if (link.StartsWith("."))
                            {
                                link = link.Replace("./", domain);
                            }
                            else if (link.StartsWith("/"))
                            {
                                link = link.TrimStart('/');
                                link = domain + link;
                            }
                            link = link.Replace("amp;", "");
                            LoadDataResult(link, stt, pageIndex).Wait();
                            stt++;
                        }
                    }

                    try
                    {
                        if (i < totalPage - 1)
                        {
                            IWebElement btnNext = _manager.webDriver.FindElement(By.XPath("//ul[@class='btnTypeSelect02 parts clrFix']//li[2]//a"));
                            btnNext.Click();
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            MessageBox.Show("Hoàn thành!");
                        }
                    }
                    catch (Exception ex)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không tìm được trang hiện tại!" + ex.Message);
                return;
            }
        }
        private async Task<string> CallService(string link)
        {
            using (HttpClient client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, link);
                request.Headers.Add("Cookie", "JSESSIONID=51446422208AC7A7D04136ACA8FDE5BC; AWSALB=FzcqOT/2pvOoAtmVt395xqpNsi9jlLoPGUX+KdHhNEaOZCkA03vU12B5WXK4XQZU1UUBeANrFngI2f6B6Y9y7zhcdKL0Uxrj4bCWe/2OdOGUI6kYmOdN0KP6AkVG; AWSALBCORS=FzcqOT/2pvOoAtmVt395xqpNsi9jlLoPGUX+KdHhNEaOZCkA03vU12B5WXK4XQZU1UUBeANrFngI2f6B6Y9y7zhcdKL0Uxrj4bCWe/2OdOGUI6kYmOdN0KP6AkVG; Apache=c06ea465.60e1cad6bccaf; HISTORYID=3009293924%2C20240104200412%2C10%2C6%3B; RECOMMEND_UID=\"\"; _abck=AABB7F9EEC604BF37EBFD0C6120DF547~-1~YAAQm+arcbjPD4GMAQAAQDYk1At9qvOEduIH+9Ekymq8FudzmZZO6826Ban5tqR1dp4lsxIZ2418h0D2OYbVKxwvMPVwn4tYks1xD180ZNaW33vO85yqlJAm8/PdQaxxRJdgmXrVOBw1iBqKaK0uTt8wRUinWjl26nYZ/yO9pImzkT3iR1tI7vnkw8dP/XeJtLdhsNEPQEZYE2fJ+r+17d+8c3O9d+WeNgf+yefFC0WphbbP6XreJeO8TphLdSsA0dlDBE4TKu1rUHIpJmWB64g1vRk4KlteLAy+IE6XCjX3Cmp3SIm610dYwtMkrP6bD6EsQCi+9D75OnOPpouPq4aLf3y2ktkXfOpeD3krlHkav0T/aRY=~-1~-1~-1; bm_sz=7560D27203676C85A50B95D815F0D070~YAAQm+arcbnPD4GMAQAAQDYk1BZ7sXxkLfc59K2/1WCg7XO//yg949Q0u9jBNcGnkD+NUesNvdt3IyMAuaXTaD3pAF5gznYthMmbdM3/DsqSn77xDxS9RvMuFsGQ00YyT18nHFefHGxh/VZeQo/4JSBfCkhSD5wscovkUbMP1yX/NC7BIdv5KD9o3GYthZ8H3XQ3Oc4q/JdIvDeUQyRIWR6eT3GSvlT8tEIgKSCkLuR9dBZY9phH7FVJNI1yosVlF5GqbI6OTUH/juMl72C+qWQDlNbDGrnrCHuV2vp6/vk=~4408385~4539192");
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                var res = await client.SendAsync(request);
                res.EnsureSuccessStatusCode();
                string content = await res.Content.ReadAsStringAsync();
                return content;
            }
        }
        private async Task LoadDataResult(string link, int stt, int page)
        {
            try
            {
                link = link.Replace("-tab__pr/", "-tab__jd/-fm__jobdetail/-mpsc_sid__10/-tp__1/");


                string content = await CallService(link);
                HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
                htmlDocument.LoadHtml(content);

                var row1 = htmlDocument.DocumentNode.SelectSingleNode("//h1").ChildNodes.Where(x => !string.IsNullOrEmpty(x.InnerText.Trim())).ToList();
                string name_job = row1[0].InnerText.Trim();
                string name_company = row1[1].InnerText.Trim();
                var row2 = htmlDocument.DocumentNode.SelectNodes("//ul[@class='head_icons clrFix']//li");
                string cRow2 = row2 != null ? string.Join("\n", row2.Select(x => x.InnerText)) : "";
                var row3 = htmlDocument.DocumentNode.SelectNodes("//table[@id='job_description_table']//p[@class='explain']").FirstOrDefault();
                string cRow3 = row3 != null ? row3.InnerText : "";
                HtmlAgilityPack.HtmlNode row4 = htmlDocument.DocumentNode.SelectSingleNode("//dl[@class='band_title space']//p");
                string cRow4 = row4 != null ? row4.InnerHtml : "";
                HtmlAgilityPack.HtmlNode row5 = htmlDocument.DocumentNode.SelectNodes("//table[@id='application_method_table']//td//dl[@class='band_title']//dd/p").LastOrDefault();
                string cRow5 = row5 != null ? row5.InnerHtml : "";
                HtmlAgilityPack.HtmlNode row6 = htmlDocument.DocumentNode.SelectSingleNode("//table[@id='company_profile_table']//tbody//tr[2]//td");
                string cRow6 = row6 != null ? row6.InnerText : "";
                string[] row5Split = cRow5.Split(new[] { "<br>" }, StringSplitOptions.None);
                string regexPatternTel = @"(?:(?:\+|0{1,2})81[\-\s]?)?(\d{1,4}[\-\s]?)?\d{1,4}[\-\s]?\d{3,4}[\-\s]?\d{3,4}";
                MatchCollection matches = Regex.Matches(cRow5, regexPatternTel);
                string regexPatternEmail = @"((?!\.)[\w\-_.]*[^.])(@\w+)(\.\w+(\.\w+)?[^.\W])";
                MatchCollection matches1 = Regex.Matches(cRow5, regexPatternEmail);
                string tel = "";
                string email = "";
                foreach (Match match in matches)
                {
                    if (!string.IsNullOrEmpty(tel)) tel += "\n";
                    tel += match.Value;
                }
                foreach (Match match in matches1)
                {
                    if (!string.IsNullOrEmpty(email)) email += "\n";
                    email += match.Value;
                }

                if (string.IsNullOrEmpty(tel))
                {
                    for (int i = 0; i < row5Split.Length; i++)
                    {
                        if (row5Split[i].Contains("TEL") || row5Split[i].Contains("電話"))
                        {
                            tel = row5Split[i].Replace("<br>", "\n").Trim();
                            break;
                        }
                    }
                }
                HtmlAgilityPack.HtmlNode row7 = htmlDocument.DocumentNode.SelectSingleNode("//table[@id='company_profile_table']//tr[last()]//td//a");
                string cRow7 = row7 != null ? row7.InnerText : "";
                var row8 = htmlDocument.DocumentNode.SelectSingleNode("//table[@id='job_description_table']//tbody//tr[7]//td//dl").ChildNodes;
                string cRow8 = string.Join("\n", row8.Select(x => HttpUtility.HtmlDecode(x.InnerText.Trim()))).Trim('\n');
                HtmlAgilityPack.HtmlNode row9 = htmlDocument.DocumentNode.SelectSingleNode("//*[@id='salary']//td//p");
                string cRow9 = row9 != null ? row9.InnerHtml : "";

                _manager.dataTable.Rows.Add(new[] {
                    $"{stt + (page - 1) * 50}",
                    $"{page}",
                    link,
                    name_job,
                    name_company,
                    cRow2.Trim(),
                    HttpUtility.HtmlDecode(cRow3.Trim()),
                    HttpUtility.HtmlDecode(cRow4.Trim()).Replace("<br>", "\n"),
                    tel,
                    email,
                    HttpUtility.HtmlDecode(cRow6.Trim()),
                    cRow5.Replace("<br>", "\n").Trim(),
                    cRow7.Trim(),
                    cRow8.Replace("<br>", "\n").Trim(),
                    HttpUtility.HtmlDecode(cRow9.Trim()).Replace("<br>", "\n")
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public List<InitObjectColumn> InitObjectColumns()
        {
            return new List<InitObjectColumn>()
            {
                new InitObjectColumn(){Name = "Index", Width = 50},
                new InitObjectColumn(){Name = "Page", Width = 50},
                new InitObjectColumn(){Name = "Link" },
                new InitObjectColumn(){Name = "Job Name " },
                new InitObjectColumn(){Name = "Company Name" },
                new InitObjectColumn(){Name = "Tags" },
                new InitObjectColumn(){Name = "Description" },
                new InitObjectColumn(){Name = "Detail" },
                new InitObjectColumn(){Name = "Tel" },
                new InitObjectColumn(){Name = "Email" },
                new InitObjectColumn(){Name = "Profile Company" },
                new InitObjectColumn(){Name = "Summary" },
                new InitObjectColumn(){Name = "Website" },
                new InitObjectColumn(){Name = "Recruitment Form" },
                new InitObjectColumn(){Name = "Salary" },
            };
        }
    }
}
