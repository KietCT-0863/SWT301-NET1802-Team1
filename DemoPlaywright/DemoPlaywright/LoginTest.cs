using Microsoft.Playwright;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Runtime.InteropServices;

namespace DemoPlaywright
{
    public class LoginTest
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

            listAccountTest.Add(new List<string> { "vip1", "vip123" });
            listAccountTest.Add(new List<string> { "kiet", "kiet123" });
            listAccountTest.Add(new List<string> { "swt", "swt123" });
            listAccountTest.Add(new List<string> { "member1", "member123" });

            return listAccountTest;
        }

        [TestCaseSource(nameof(initData))]
        public async Task TestLoginWithAvailableMemberAccount(List<string> account)
        {
            await page.GotoAsync("https://pregnancy-growth-tracking.vercel.app/login");

            await page.FillAsync("#usernameOrEmail", account[0]);
            await page.FillAsync("#password", account[1]);

            // Click nút Login bằng JavaScript
            await page.EvaluateAsync(@"() => {
                const button = document.querySelector('button[type=""submit""]');
                if (button) button.click();
                }");

            // Kiểm tra toast message bằng XPath
            var toastMessage = await page.WaitForSelectorAsync("//div[contains(text(),'Đăng nhập thành công!')]");

            // Assert using NUnit
            string? messageText = await toastMessage.TextContentAsync();
            Assert.That(messageText, Is.EqualTo("Đăng nhập thành công!"));

            await Task.Delay(3000);
        }

    }
}