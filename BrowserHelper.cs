using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using System.IO;
using System.IO.Compression;
using System.Text;
using System;

public class ProxyConfig
{
    public bool Enabled { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public bool IsValidHostPort()
    {
        return !string.IsNullOrWhiteSpace(Host) && Port > 0;
    }
}

public static class BrowserHelper
{
    // OpenBrowser: hỗ trợ proxy có/không auth. Nếu proxyConfig null hoặc Disabled -> không dùng proxy.
    public static IWebDriver OpenBrowser(ProxyConfig? proxyConfig = null)
    {
        // 1. Tự động tải ChromeDriver mới nhất
        new DriverManager().SetUpDriver(new ChromeConfig());

        // 2. Cấu hình Chrome
        var options = new ChromeOptions();
        options.AddArgument("--start-maximized");
        options.AddArgument("--disable-notifications");
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddExcludedArgument("enable-automation");
        options.AddArgument("--ignore-certificate-errors");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");

        // Logging để bắt lỗi trình duyệt/API
        options.SetLoggingPreference(LogType.Browser, LogLevel.All);
        options.SetLoggingPreference(LogType.Performance, LogLevel.All);

        // 3. Proxy (tùy chọn)
        if (proxyConfig != null && proxyConfig.Enabled && proxyConfig.IsValidHostPort())
        {
            // Nếu có user/pass thì gắn vào proxy server string (một số proxy auth vẫn hoạt động với Chrome/Selenium theo cách này)
            var proxyServer = string.IsNullOrWhiteSpace(proxyConfig.Username)
                ? $"{proxyConfig.Host}:{proxyConfig.Port}"
                : $"{proxyConfig.Username}:{proxyConfig.Password}@{proxyConfig.Host}:{proxyConfig.Port}";

            options.AddArgument($"--proxy-server=http://{proxyServer}");

            var seleniumProxy = new Proxy
            {
                HttpProxy = proxyServer,
                SslProxy = proxyServer,
                Kind = ProxyKind.Manual
            };
            options.Proxy = seleniumProxy;
            options.AddArgument("--ignore-certificate-errors");
        }

        // 4. Load Extension giải Captcha (Buster) nếu có sẵn
        string busterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Extensions", "buster.crx");
        if (File.Exists(busterPath))
        {
            options.AddExtension(busterPath);
        }

        return new ChromeDriver(options);
    }

}