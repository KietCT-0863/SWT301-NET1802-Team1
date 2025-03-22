using Microsoft.Playwright;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DemoPlaywright
{
    public class CreateReminders
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

            // listAccountTest.Add(new List<string> { "vip1", "vip123" });
            // listAccountTest.Add(new List<string> { "kiet", "kiet123" });
            listAccountTest.Add(new List<string> { "swt", "swt123" });
            // listAccountTest.Add(new List<string> { "member1", "member123" });

            return listAccountTest;
        }

        [TestCaseSource(nameof(initData))]
        public async Task TestCreateReminders(List<string> account)
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

            await page.ClickAsync("//button[normalize-space()='OK']");

            await page.ClickAsync("a[href='/member/calendar']");

            await page.ClickAsync(".new-event-button");

            await page.FillAsync("input[placeholder='Tiêu đề']", "Sức khỏe");

            // Lấy ngày và giờ hiện tại
            DateTime now = DateTime.Now;
            string currentDate = now.ToString("2025-03-22");  
            string currentTime = now.ToString("16:45");       

            // Điền ngày và giờ hiện tại vào form
            await page.FillAsync("input[type='date']", currentDate);
            await page.FillAsync("input[type='time']", currentTime);    

            await page.SelectOptionAsync("div[class='form-group'] select", new SelectOptionValue { Value = "Uống thuốc" });

            await page.FillAsync("textarea[placeholder='Thông báo (không bắt buộc)']", "Bạn hãy nhớ uống thuốc đúng giờ nhé.");

            await page.ClickAsync("button[class='save-btn']");

            await Task.Delay(3000);
        }

    }
}
