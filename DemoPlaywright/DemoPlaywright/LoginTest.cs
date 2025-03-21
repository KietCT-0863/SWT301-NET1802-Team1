using Microsoft.Playwright;
using System.Threading.Tasks;
using NUnit.Framework;

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

            listAccountTest.Add(new List<string> { "swt", "swt123" });
            listAccountTest.Add(new List<string> { "member1", "member123" });
            listAccountTest.Add(new List<string> { "kiet", "kiet123" });

            return listAccountTest;
        }

        [TestCaseSource(nameof(initData))]
        public async Task TestLoginWithAvailableAccount(List<string> account)
        {
            await page.GotoAsync("https://pregnancy-growth-tracking.vercel.app/login");
            
            await page.FillAsync("#usernameOrEmail", account[0]);
            await page.FillAsync("#password", account[1]);
            await page.ClickAsync("button:has-text('Đăng nhập')");

            // Đợi cho toast message xuất hiện và kiểm tra nội dung
            IElementHandle? toastMessage = await page.WaitForSelectorAsync(".Toastify__toast-body > div:nth-child(2)");
            string? messageText = await toastMessage.TextContentAsync();
            
            // Assert using NUnit
            Assert.That(messageText, Is.EqualTo("Đăng nhập thành công!"));

            await Task.Delay(3000);

            // Click vào "Lịch Trình Thăm Khám"
            await page.ClickAsync("a[href='/member/calendar']");
            // Click vào nút "Thêm sự kiện"
            await page.ClickAsync(".add-event-btn");
            // Điền thông tin sự kiện
            await page.FillAsync("input[placeholder='Tiêu đề']", "Sức Khỏe");
            await page.FillAsync("input[type='date']", "2025-03-22");
            await page.FillAsync("input[type='time']", "11:00");
            // Chọn danh mục từ 
            await page.SelectOptionAsync("div.modal-content form select", new SelectOptionValue { Value = "Uống thuốc" });
            // Điền thông báo
            await page.FillAsync("textarea[placeholder='Thông báo']", "Bạn hãy nhớ uống thuốc đúng giờ nhé.");
            
            await page.ClickAsync("button[type='submit']");

            await Task.Delay(5000);

            toastMessage = await page.WaitForSelectorAsync(".Toastify__toast-body > div:nth-child(2)");
            messageText = await toastMessage.TextContentAsync();

            Assert.That(messageText, Is.EqualTo("Lưu thành công!"));

            await Task.Delay(5000); 
            
        }
    }
}