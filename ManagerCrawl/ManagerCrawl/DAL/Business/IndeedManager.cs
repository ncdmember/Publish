using ManagerCrawl.DAL.Interfaces;
using ManagerCrawl.Entities;
using ManagerCrawl.Entities.Indeed;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.TextFormatting;

namespace ManagerCrawl.DAL.Business
{
    public class IndeedManager : IManager
    {
        private ManagerCrawl _manager;
        bool doneTask = false;
        int pageError = 0;
        public IndeedManager(ManagerCrawl manager)
        {
            _manager = manager;
        }

        public void GetData(object sender, EventArgs e)
        {
            doneTask = false;
            try
            {
                if (string.IsNullOrEmpty(_manager.txtTotalPage.Text))
                {
                    _manager.txtTotalPage.Text = "10";
                }
                var lastSub = _manager.txtLink.Text.LastIndexOf("/");
                _manager.fullDomain = _manager.txtLink.Text.Substring(0, lastSub + 1);
                int totalPage = int.Parse(_manager.txtTotalPage.Text);
                LoadContentPage(0, totalPage, _manager.fullDomain);

                if (doneTask)
                {
                    //webDriver.Quit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã có lỗi xảy ra!" + ex.Message);
                return;
            }
        }

        private void LoadContentPage(int page, int totalPage, string domain)
        {
            for (int i = page; i < totalPage; i++)
            {
                try
                {
                    var inputPage = _manager.webDriver.FindElement(By.XPath("//a[@data-testid='pagination-page-current']"));
                    string nameInputPage = inputPage.Text;
                    if (int.TryParse(nameInputPage, out int pageOut))
                    {
                        _manager.txtPage.Text = nameInputPage;
                    }
                    _manager.txtPage.Text = nameInputPage;

                    LoadDataResult(_manager.webDriver, null);
                }
                catch (Exception ex)
                {
                    pageError = i;
                    if (string.IsNullOrEmpty(_manager.txtTotalPage.Text))
                    {
                        _manager.txtTotalPage.Text = "10";
                    }
                    LoadContentPage(pageError, int.Parse(_manager.txtTotalPage.Text), domain);
                    return;
                }

                if (i == totalPage - 1)
                {
                    doneTask = true;
                }

                try
                {
                    if (!doneTask)
                    {
                        IWebElement btnNext = _manager.webDriver.FindElement(By.XPath($"//a[@data-testid='pagination-page-next']"));
                        string linkNext = btnNext.GetAttribute("href");
                        if (linkNext.StartsWith("."))
                        {
                            linkNext = linkNext.Replace("./", domain);
                        }
                        else if (linkNext.StartsWith("/"))
                        {
                            linkNext = linkNext.TrimStart('/');
                            linkNext = domain + linkNext;
                        }
                        linkNext = linkNext.Replace("amp;", "");
                        _manager.webDriver.Navigate().GoToUrl(linkNext);
                        Thread.Sleep(1500);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Không tìm được trang tiếp theo!" + ex.ToString());
                    return;
                }
            }
        }

        private void LoadDataResult(IWebDriver driver, string link)
        {
            if (!string.IsNullOrEmpty(link))
            {
                driver.Navigate().GoToUrl(link);
            }

            string lines = Scripts.Content;
            int timeDelay = 0;
            if (!int.TryParse(_manager.txtDelayTime.Text, out timeDelay))
            {
                timeDelay = 3;
            }
            lines = lines.Replace("[[timeDelay]]", timeDelay * 1000 + "");

            driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromMinutes(5);
            IJavaScriptExecutor js = (IJavaScriptExecutor)_manager.webDriver;
            Task<object> task = Task.Run(() => js.ExecuteScript(lines));
            task.Wait();
            string jsonResult = task.Result.ToString();

            var arrayOfObjects = System.Text.Json.JsonSerializer.Deserialize<List<ObjectResult>>(jsonResult);
            int start = _manager.dataTable.Rows.Count;
            int i = 0;
            foreach (var item in arrayOfObjects)
            {
                _manager.dataTable.Rows.Add(new[] { $"{start + i}", $"{item.page}", $"{item.name}", $"{item.content.Substring(0, item.content.Length > 50 ? 50 : item.content.Length)}", $"{item.link}" });
                i++;
            }
        }

        public List<InitObjectColumn> InitObjectColumns()
        {
            return new List<InitObjectColumn>()
            {
                new InitObjectColumn(){Name = "Index", Width = 50},
                new InitObjectColumn(){Name = "Page", Width = 50},
                new InitObjectColumn(){Name = "Name Job" },
                new InitObjectColumn(){Name = "Content" },
                new InitObjectColumn(){Name = "Link Job" },
                new InitObjectColumn(){Name = "Company Name" },
                new InitObjectColumn(){Name = "Job Location" },
                new InitObjectColumn(){Name = "Salary Min" },
                new InitObjectColumn(){Name = "Salary Max" },
                new InitObjectColumn(){Name = "Tags" },
                new InitObjectColumn(){Name = "Emails" },
                new InitObjectColumn(){Name = "Phones" },
            };
        }
    }
    public class ObjectResult
    {
        public string name { get; set; }
        public string link { get; set; }
        public string linkApi { get; set; }
        public string page { get; set; }
        public string content { get; set; }
        public string companyName { get; set; }
        public string jobLocation { get; set; }
        public decimal? salaryMax { get; set; }
        public decimal? salaryMin { get; set; }
        public string tags { get; set; }
        public string email { get; set; }
        public string phoneNumber { get; set; }
    }
    public static class Scripts
    {
        public static string Content = "async function getData(e, n, a) { var o, t = e.querySelector(\"h2\"), i = e.querySelector(\"a\"), r = i.getAttribute(\"data-jk\"), l = `https://jp.indeed.com/viewjob?jk=${r}&from=vjs&tk=${i.getAttribute(\"data-mobtk\")}&viewtype=embedded&continueUrl=${\"/\" + n}&spa=1&hidecmpheader=0`, d = `https://jp.indeed.com/viewjob?jk=${r}&from=serp&vjs=3`; try { let c = await fetch(l), s = await c?.json(); var b = s.body.jobInfoWrapperModel.jobInfoModel.sanitizedJobDescription, p = document.createElement(\"div\"); p.innerHTML = b; var m = p.innerText, y = s.body.jobInfoWrapperModel.jobInfoModel.jobInfoHeaderModel.companyName, f = s.body.jobLocation, j = s.body.salaryInfoModel?.salaryMax, u = s.body.salaryInfoModel?.salaryMin, M = s.body.jobInfoWrapperModel.jobInfoModel.jobTagModel?.tags?.join(\", \"), h = s.body.jobInfoWrapperModel?.jobInfoModel?.sanitizedJobDescription || \"\", g = s.body?.jobInfoWrapperModel?.sectionedJobInfoModel?.semanticSegmentModels, I = g?.find(e => \"company-tel\" == e.semanticLabel)?.sanitizedContent, v = g?.find(e => \"apply-info\" == e.semanticLabel)?.sanitizedContent, k = g?.find(e => \"apply-method\" == e.semanticLabel)?.sanitizedContent; return { name: t.innerText, link: d, linkApi: l, page: a, content: m, companyName: y, jobLocation: f, salaryMax: j, salaryMin: u, tags: M, email: findEmails(h), phoneNumber: findPhoneNumbers(h.concat(v || \"\", k || \"\"), I) } } catch (w) { return { name: t.innerText, link: d, linkApi: l, page: a, content: \"\", companyName: \"\", jobLocation: \"\", salaryMax: null, salaryMin: null, tags: \"\", email: \"\", phoneNumber: \"\" } } } function findPhoneNumbers(e, n) { var a = e.match(/\\b(\\d[-\\d]*\\d{3})\\b/g); return [...new Set(a || []), n || 0].filter(e => e.length > 8).join(\"; \") } function findEmails(e) { return (e.match(/[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}/g) || []).join(\"; \") } async function loadData() { var e = document.querySelectorAll(\"#mosaic-provider-jobcards > ul > li\"), n = document.location.href.split(\"/\").pop(), a = document.querySelector('a[data-testid=\"pagination-page-current\"]').innerText; let o = Array.from(e).filter(e => e.querySelector(\"h2\")).map(async (e, o) => new Promise(async (t, i) => { setTimeout(async () => { try { let o = await getData(e, n, a); t(o) } catch (r) { i(r) } }, [[timeDelay]] * o) })); return await Promise.all(o) };return JSON.stringify(await loadData());";
    }
}
