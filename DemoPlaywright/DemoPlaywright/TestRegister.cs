using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DemoPlaywright
{
    public class TestRegister
    {
        private IPlaywright playwright;
        private IBrowser browser;
        private IPage page;

        [SetUp]
        public async Task Setup()
        {
            playwright = await Playwright.CreateAsync();
            browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,
                Channel = "chrome",     // Google Chrome
                //Channel = "msedge",     // Edge
                Args = new[] {
                    "--incognito"         // Chế độ ẩn danh của Chrome
                    //"--inprivate"        // Chế độ ẩn danh của Edge
                }
            });

            // Tạo context mới
            var context = await browser.NewContextAsync();
            page = await context.NewPageAsync();
        }

        [TearDown]
        public async Task Cleanup()
        {
            await Task.Delay(3000);
            await browser.DisposeAsync();
            playwright.Dispose();
        }

        public static List<List<string>> initData()
        {
            List<List<string>> listAccountTest = new();

            listAccountTest.Add(new List<string> { "TestRegister", "Tester", "test@gmail.com", "0987654321", "Test123", "Test123", "01-22-2001" });

            return listAccountTest;
        }

        [TestCaseSource(nameof(initData))]
        public async Task TestRegisterWithAvailableInput(List<string> account)
        {
            await page.GotoAsync("https://pregnancy-growth-tracking.vercel.app/register");

            await page.FillAsync("#username", account[0]);
            await page.FillAsync("#fullName", account[1]);
            await page.FillAsync("#email", account[2]);
            await page.FillAsync("#phone", account[3]);
            await page.FillAsync("#password", account[4]);
            await page.FillAsync("#confirmPassword", account[5]);
            
            // Xử lý nhập ngày tháng năm sinh
            await page.ClickAsync("#dob");
            await page.Keyboard.TypeAsync(account[6]);

            // Click vào nút submit bằng CSS Selector
            await page.ClickAsync("button[type='submit']");

            // Đợi để hệ thống tạo tài khoản
            await Task.Delay(5000);

            // Kiểm tra URL hiện tại
            string currentUrl = page.Url;
            Assert.That(currentUrl, Is.EqualTo("https://pregnancy-growth-tracking.vercel.app/login"));

            await Task.Delay(3000);
        }
    }
}
