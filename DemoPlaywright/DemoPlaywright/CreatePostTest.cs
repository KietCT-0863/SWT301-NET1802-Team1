using Microsoft.Playwright;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO;

namespace DemoPlaywright
{
    public class CreatePostTest
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
                Args = new[] { "--incognito" }  // Chế độ ẩn danh
            });

            var context = await browser.NewContextAsync();
            page = await context.NewPageAsync();
        }

        [TearDown]
        public async Task Cleanup()
        {
            await browser.CloseAsync();
            playwright.Dispose();
        }

        public static List<object[]> InitData()
        {
            return new List<object[]>
            {
                new object[] { "vip1", "vip123", "Dinh dưỡng mẹ bầu", "Con tôi trong bụng 5 tháng đã biết đạp", "Nutrition" },
                new object[] { "vip1", "vip123", "Chăm sóc thai kỳ", "Hôm nay tôi đi khám thai, bé rất khỏe mạnh", "PregnancyCare" }
            };
        }

        [TestCaseSource(nameof(InitData))]
        public async Task CreatePostsTest(string username, string password, string postTitle, string postContent, string postTag)
        {
            // Truy cập trang đăng nhập
            await page.GotoAsync("https://pregnancy-growth-tracking.vercel.app/login");

            // Điền thông tin đăng nhập
            await page.FillAsync("#usernameOrEmail", username);
            await page.FillAsync("#password", password);

            // Nhấn nút Login
            await page.ClickAsync("button[type='submit']");

            // Chờ và kiểm tra toast message
            var toastMessage = await page.WaitForSelectorAsync(
                "//div[contains(text(),'Đăng nhập thành công!')]",
                new() { Timeout = 15000 }
            );

            await page.ClickAsync("//button[normalize-space()='OK']");

            // Điều hướng đến trang cộng đồng
            await page.ClickAsync("a[href='/member/community']");

            // Chờ trang tải hoàn toàn, bao gồm các yêu cầu mạng
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 15000 });

            // Tìm container chứa các post-card và tăng thời gian chờ
            var postsContainer = await page.WaitForSelectorAsync(
                "div.posts-container",
                new() { Timeout = 5000 }
            );
            Assert.IsNotNull(postsContainer, "Không tìm thấy posts-container!");

            // Nhấn nút tạo bài viết
            await page.ClickAsync("button[class='create-post-button']");

            // Điền nội dung bài viết từ tham số
            await page.FillAsync("input[placeholder='Tiêu đề bài viết']", postTitle);
            await page.FillAsync("textarea[placeholder='Chia sẻ điều gì đó với cộng đồng...']", postContent);
            await page.FillAsync("input[placeholder='Thêm thẻ (gõ và nhấn Enter)']", postTag);

            // Nhấn nút thêm thẻ (nếu cần)
            await page.ClickAsync("button[class='tag-button']");

            // Upload ảnh
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Img", "walking_17492446.png");
            if (!File.Exists(imagePath))
            {
                Assert.Fail($"File ảnh không tồn tại tại đường dẫn: {imagePath}. Vui lòng kiểm tra thư mục Img và đảm bảo file được sao chép vào thư mục đầu ra.");
            }
            await page.SetInputFilesAsync("#post-image", imagePath);

            // Kiểm tra trạng thái nút "Đăng bài"
            var submitButton = await page.QuerySelectorAsync("button[class='submit-post']");
            Assert.IsNotNull(submitButton, "Không tìm thấy nút Đăng bài!");
            var isDisabled = await submitButton.EvaluateAsync<bool>("el => el.disabled");
            Assert.IsFalse(isDisabled, "Nút Đăng bài đang bị vô hiệu hóa! Kiểm tra xem có trường bắt buộc nào chưa được điền không.");

            // Nhấn nút đăng bài
            await page.ClickAsync("button[class='submit-post']");

            // Chờ và kiểm tra thông báo "Bài viết đã được tạo thành công"
            var successMessage = await page.WaitForSelectorAsync(
                "//div[contains(text(),'Bài viết đã được tạo thành công')]",
                new() { Timeout = 15000 }
            );
            Assert.IsNotNull(successMessage, "Không thấy thông báo 'Bài viết đã được tạo thành công'! Đăng bài thất bại.");
        }
    }
}