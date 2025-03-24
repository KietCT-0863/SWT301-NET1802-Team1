using Microsoft.Playwright;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Runtime.InteropServices;

namespace DemoPlaywright
{
    public class TestLogin
    {
        private IPlaywright playwright;
        private IBrowser browser;
        private IPage page;

        [SetUp]
        public async Task Setup()
        {
            playwright = await Playwright.CreateAsync();        // khởi tạo playwright trong ram

            // Chromium là 1 trình duyệt web mã nguồn mở, được phát triển bởi Google
            // Engine hiển thị web của Chromium là Blink
            // Trình duyệt được viết dựa trên Blink : Google Chrome , Cốc cốc, Opera, Edge
            browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,       // hiển thị UI khi chạy test case
                Channel = "chrome",     // Google Chrome
                //Channel = "msedge",     // Edge
                Args = new[] {
                    "--incognito"         // Chế độ ẩn danh của Chrome
                    //"--inprivate"        // Chế độ ẩn danh của Edge
                }
            });
            // Firefox là 1 trình duyệt web mã nguồn, được phát triển bởi Mozilla
            // Engine hiển thị web của Firefox là Gecko
            // Trình duyệt được viết dựa trên Gecko : Waterfox, LibreWolf,  Tor Browser
            //browser = await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
            //{
            //    Headless = false,
            //    Channel = "firefox",    // Firefox
            //    Args = new[] {
            //        "--private-window"  // Chế độ ẩn danh của Firefox
            //    }
            //});

            // Tạo context mới
            IBrowserContext context = await browser.NewContextAsync();
            page = await context.NewPageAsync();
        }

        [TearDown]
        public async Task Cleanup()
        {
            await browser.DisposeAsync();       // đóng browser và giải phóng tất cả tài nguyên của browser trong ram
            playwright.Dispose();       // đóng playwright và giải phóng tài nguyên playwright chiếm trong ram
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

            // điền thông tin
            // cách 1 : tự độ fill dự liệu vào ô
            await page.FillAsync("#usernameOrEmail", account[0]);
            // cách 2 : chọn vào ô và nhập dữ liệu từ bàn phím
            await page.ClickAsync("#password");
            await page.Keyboard.TypeAsync(account[1]);

            // click nút Login 
            // cách 1 : JavaScript
            await page.EvaluateAsync(@"() => {
                const button = document.querySelector('button[type=""submit""]');
                if (button) button.click();
                }");
            // cách 2 : CSS Selector
            //await page.ClickAsync("button[type='submit']");

            // kiểm tra toast message
            // cách 1 : XPath
            //var toastMessage = await page.WaitForSelectorAsync("//div[contains(text(),'Đăng nhập thành công!')]");
            // cách 2 : Css Selector
            var toastMessage = await page.WaitForSelectorAsync("div[role='alert'] div:nth-child(2)");

            // kiểm tra passed/failed thông qua NUnit
            string? messageText = await toastMessage.TextContentAsync();
            Assert.That(messageText, Is.EqualTo("Đăng nhập thành công!"));

            await Task.Delay(3000);
        }
    }
}