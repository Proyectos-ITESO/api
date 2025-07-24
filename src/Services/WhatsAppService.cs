using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using MicroJack.API.Services.Interfaces;
using System.Net.Sockets;

namespace MicroJack.API.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(ILogger<WhatsAppService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendApprovalWhatsAppAsync(string phoneNumber, string approvalToken, string visitorName, string plates)
        {
            _logger.LogInformation("Sending WhatsApp approval to {Phone} for visitor {Visitor}", phoneNumber, visitorName);
            
            // Check network connectivity first
            bool hasInternet = await CheckInternetConnectivityAsync();
            if (!hasInternet)
            {
                _logger.LogWarning("No internet connection detected. WhatsApp service will work in offline mode.");
                LogOfflineApprovalInstructions(phoneNumber, approvalToken, visitorName, plates);
                return false; // Return false but gracefully handle the offline state
            }
            
            // Try Flatpak direct method first (more reliable than Selenium)
            try
            {
                var cleanPhone = phoneNumber.Replace("+", "").Replace(" ", "").Replace("-", "");
                var approvalUrl = $"http://localhost:5173/approve/{approvalToken}";
                var message = $"🚗 *SOLICITUD DE ACCESO*\n\n" +
                             $"Visitante: *{visitorName}*\n" +
                             $"Placas: *{plates}*\n\n" +
                             $"Para autorizar el acceso, haga clic en el siguiente enlace:\n" +
                             $"{approvalUrl}\n\n" +
                             $"_Este enlace es válido por 24 horas._";

                var encodedMessage = Uri.EscapeDataString(message);
                // Use WhatsApp Web directly to avoid scheme handler issues
                var whatsappDirectUrl = $"https://web.whatsapp.com/send?phone={cleanPhone}&text={encodedMessage}";
                
                _logger.LogInformation("📱 WHATSAPP MESSAGE INFO:");
                _logger.LogInformation("Phone: {Phone}", phoneNumber);
                _logger.LogInformation("Direct WhatsApp Link: {Url}", whatsappDirectUrl);
                _logger.LogInformation("Approval URL: {ApprovalUrl}", approvalUrl);
                
                _logger.LogInformation("Trying Flatpak direct method...");
                
                var flatpakProcess = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "flatpak",
                        Arguments = $"run org.chromium.Chromium --no-default-browser-check --disable-default-apps \"{whatsappDirectUrl}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                
                flatpakProcess.Start();
                _logger.LogInformation("✅ Opened WhatsApp in browser via Flatpak for {Phone}", phoneNumber);
                
                // Don't wait for the process to finish since it opens a browser window
                return true;
            }
            catch (Exception flatpakEx)
            {
                if (IsNetworkException(flatpakEx))
                {
                    _logger.LogWarning("Network error in Flatpak method for {Phone}. Trying offline fallback.", phoneNumber);
                    LogOfflineApprovalInstructions(phoneNumber, approvalToken, visitorName, plates);
                    return false;
                }
                _logger.LogError(flatpakEx, "Flatpak method failed for {Phone}: {Error}", phoneNumber, flatpakEx.Message);
                
                // Try alternative method with xdg-open
                try
                {
                    var cleanPhone = phoneNumber.Replace("+", "").Replace(" ", "").Replace("-", "");
                    var approvalUrl = $"http://localhost:5173/approve/{approvalToken}";
                    var message = $"🚗 *SOLICITUD DE ACCESO*\n\n" +
                                 $"Visitante: *{visitorName}*\n" +
                                 $"Placas: *{plates}*\n\n" +
                                 $"Para autorizar el acceso, haga clic en el siguiente enlace:\n" +
                                 $"{approvalUrl}\n\n" +
                                 $"_Este enlace es válido por 24 horas._";

                    var encodedMessage = Uri.EscapeDataString(message);
                    // Use WhatsApp Web directly to avoid scheme handler issues
                    var whatsappDirectUrl = $"https://web.whatsapp.com/send?phone={cleanPhone}&text={encodedMessage}";
                    
                    _logger.LogInformation("Trying xdg-open method...");
                    
                    var xdgProcess = new System.Diagnostics.Process
                    {
                        StartInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "xdg-open",
                            Arguments = $"\"{whatsappDirectUrl}\"",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        }
                    };
                    
                    xdgProcess.Start();
                    _logger.LogInformation("✅ Opened WhatsApp via xdg-open for {Phone}", phoneNumber);
                    return true;
                }
                catch (Exception xdgEx)
                {
                    _logger.LogError(xdgEx, "xdg-open method also failed for {Phone}: {Error}", phoneNumber, xdgEx.Message);
                    
                    // Try direct browser launch
                    try
                    {
                        var cleanPhone = phoneNumber.Replace("+", "").Replace(" ", "").Replace("-", "");
                        var approvalUrl = $"http://localhost:5173/approve/{approvalToken}";
                        var message = $"🚗 *SOLICITUD DE ACCESO*\n\n" +
                                     $"Visitante: *{visitorName}*\n" +
                                     $"Placas: *{plates}*\n\n" +
                                     $"Para autorizar el acceso, haga clic en el siguiente enlace:\n" +
                                     $"{approvalUrl}\n\n" +
                                     $"_Este enlace es válido por 24 horas._";

                        var encodedMessage = Uri.EscapeDataString(message);
                        var whatsappDirectUrl = $"https://web.whatsapp.com/send?phone={cleanPhone}&text={encodedMessage}";
                        
                        _logger.LogInformation("Trying direct browser launch...");
                        
                        var browserProcess = new System.Diagnostics.Process
                        {
                            StartInfo = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "/var/lib/flatpak/app/org.chromium.Chromium/current/active/export/bin/org.chromium.Chromium",
                                Arguments = $"--new-window --no-default-browser-check --disable-default-apps \"{whatsappDirectUrl}\"",
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true
                            }
                        };
                        
                        browserProcess.Start();
                        _logger.LogInformation("✅ Opened WhatsApp via direct browser launch for {Phone}", phoneNumber);
                        return true;
                    }
                    catch (Exception browserEx)
                    {
                        _logger.LogError(browserEx, "Direct browser launch also failed for {Phone}, trying Selenium...", phoneNumber);
                    }
                }
            }
            
            // Selenium fallback method
            ChromeDriver? driver = null;
            try
            {
                _logger.LogInformation("Trying Selenium method as fallback...");
                
                // Clean phone number (remove + and spaces)
                var cleanPhone = phoneNumber.Replace("+", "").Replace(" ", "").Replace("-", "");
                
                var approvalUrl = $"http://localhost:5173/approve/{approvalToken}";
                var message = $"🚗 *SOLICITUD DE ACCESO*\n\n" +
                             $"Visitante: *{visitorName}*\n" +
                             $"Placas: *{plates}*\n\n" +
                             $"Para autorizar el acceso, haga clic en el siguiente enlace:\n" +
                             $"{approvalUrl}\n\n" +
                             $"_Este enlace es válido por 24 horas._";

                var encodedMessage = Uri.EscapeDataString(message);
                var whatsappUrl = $"https://web.whatsapp.com/send?phone={cleanPhone}&text={encodedMessage}";

                _logger.LogInformation("Selenium - WhatsApp URL: {Url}", whatsappUrl);

                // Configure Chrome options for Chromium
                var options = new ChromeOptions();
                
                // Try to find Chromium binary (Flatpak first, then user installation)
                var chromiumPaths = new[]
                {
                    "/var/lib/flatpak/app/org.chromium.Chromium/current/active/export/bin/org.chromium.Chromium",
                    $"/home/{Environment.UserName}/.local/share/flatpak/app/org.chromium.Chromium/current/active/export/bin/org.chromium.Chromium",
                    "/usr/bin/chromium",
                    "/usr/bin/chromium-browser",
                    "/snap/bin/chromium"
                };
                
                string? chromiumPath = null;
                foreach (var path in chromiumPaths)
                {
                    if (File.Exists(path))
                    {
                        chromiumPath = path;
                        _logger.LogInformation("Found Chromium at: {Path}", path);
                        break;
                    }
                }
                
                if (chromiumPath != null)
                {
                    options.BinaryLocation = chromiumPath;
                }
                
                // Add Chrome options for better compatibility and Flatpak support
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--disable-web-security");
                options.AddArgument("--disable-features=VizDisplayCompositor");
                options.AddArgument("--disable-blink-features=AutomationControlled");
                options.AddArgument("--disable-extensions");
                options.AddArgument("--window-size=1920,1080");
                options.AddArgument("--user-agent=Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/137.0.0.0 Safari/537.36");
                
                // Flatpak specific arguments
                options.AddArgument("--disable-features=MediaRouter");
                options.AddArgument("--disable-background-timer-throttling");
                options.AddArgument("--disable-backgrounding-occluded-windows");
                options.AddArgument("--disable-renderer-backgrounding");
                options.AddArgument("--allow-running-insecure-content");
                options.AddArgument("--ignore-certificate-errors");
                
                // Add preferences to avoid automation detection
                options.AddExcludedArgument("enable-automation");
                options.AddAdditionalOption("useAutomationExtension", false);
                
                // Try to setup ChromeDriver automatically
                try
                {
                    new DriverManager().SetUpDriver(new ChromeConfig());
                    _logger.LogInformation("ChromeDriver setup completed successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "WebDriverManager failed, using system ChromeDriver");
                }
                
                driver = new ChromeDriver(options);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);

                _logger.LogInformation("Browser started successfully");
                _logger.LogInformation("Navigating to WhatsApp Web URL: {Url}", whatsappUrl);
                
                // Try to navigate to WhatsApp
                try
                {
                    driver.Navigate().GoToUrl(whatsappUrl);
                    _logger.LogInformation("Navigation command sent");
                    
                    // Wait for initial page load
                    await Task.Delay(5000);
                    
                    // Check current URL
                    var currentUrl = driver.Url;
                    _logger.LogInformation("Current URL after navigation: {CurrentUrl}", currentUrl);
                    
                    if (currentUrl.Contains("data:,") || currentUrl == "data:,")
                    {
                        _logger.LogWarning("Browser opened to data:, URL. Trying alternative approach...");
                        
                        // Try navigating to WhatsApp web first, then to the specific URL
                        driver.Navigate().GoToUrl("https://web.whatsapp.com");
                        await Task.Delay(3000);
                        
                        _logger.LogInformation("Navigated to WhatsApp Web, now redirecting to message URL...");
                        driver.Navigate().GoToUrl(whatsappUrl);
                        await Task.Delay(3000);
                        
                        currentUrl = driver.Url;
                        _logger.LogInformation("URL after second attempt: {CurrentUrl}", currentUrl);
                    }
                }
                catch (Exception navEx)
                {
                    _logger.LogError(navEx, "Error during navigation to WhatsApp");
                    throw;
                }

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
                
                try
                {
                    // Step 1: Wait for WhatsApp to load (look for the chat input or login screen)
                    _logger.LogInformation("Waiting for WhatsApp Web to load...");
                    
                    // Check if we need to scan QR code
                    try
                    {
                        var qrCode = driver.FindElement(By.CssSelector("[data-testid='qr-code']"));
                        if (qrCode != null)
                        {
                            _logger.LogWarning("WhatsApp Web requires QR code scan. Please scan the QR code to continue.");
                            await Task.Delay(30000); // Wait 30 seconds for user to scan QR
                        }
                    }
                    catch
                    {
                        _logger.LogInformation("No QR code found, proceeding...");
                    }

                    // Step 2: Wait for the message input field to be available
                    _logger.LogInformation("Looking for message input field...");
                    var messageInput = wait.Until(d => 
                    {
                        try
                        {
                            return d.FindElement(By.CssSelector("[data-testid='conversation-compose-box-input']")) ??
                                   d.FindElement(By.CssSelector("div[contenteditable='true'][data-tab='10']")) ??
                                   d.FindElement(By.XPath("//div[@contenteditable='true' and @role='textbox']"));
                        }
                        catch
                        {
                            return null;
                        }
                    });

                    if (messageInput != null)
                    {
                        _logger.LogInformation("Message input found, typing message...");
                        
                        // Clear any existing text and type our message
                        messageInput.Clear();
                        messageInput.SendKeys(message);
                        
                        await Task.Delay(1000);

                        // Step 3: Look for and click the send button
                        _logger.LogInformation("Looking for send button...");
                        var sendButton = wait.Until(d => 
                        {
                            try
                            {
                                return d.FindElement(By.CssSelector("[data-testid='send']")) ??
                                       d.FindElement(By.CssSelector("button[aria-label*='Send']")) ??
                                       d.FindElement(By.CssSelector("button[aria-label*='Enviar']")) ??
                                       d.FindElement(By.XPath("//button[contains(@aria-label, 'Send') or contains(@aria-label, 'Enviar')]")) ??
                                       d.FindElement(By.XPath("//span[@data-testid='send']"));
                            }
                            catch
                            {
                                return null;
                            }
                        });

                        if (sendButton != null && sendButton.Enabled)
                        {
                            _logger.LogInformation("Send button found, clicking...");
                            sendButton.Click();
                            
                            await Task.Delay(3000); // Wait for message to send
                            
                            _logger.LogInformation("✅ WhatsApp message sent successfully to {Phone}", phoneNumber);
                            return true;
                        }
                        else
                        {
                            _logger.LogWarning("Send button not found or not enabled for {Phone}", phoneNumber);
                            return false;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Message input field not found for {Phone}", phoneNumber);
                        return false;
                    }
                }
                catch (WebDriverTimeoutException ex)
                {
                    _logger.LogWarning("Timeout waiting for WhatsApp interface elements for {Phone}: {Error}", phoneNumber, ex.Message);
                    
                    // Try to get page title for debugging
                    try
                    {
                        var pageTitle = driver.Title;
                        var currentUrl = driver.Url;
                        _logger.LogInformation("Current page title: {Title}, URL: {Url}", pageTitle, currentUrl);
                    }
                    catch { }
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Selenium method failed for {Phone}: {Error}", phoneNumber, ex.Message);
                
                // Fallback: Try using Flatpak directly
                try
                {
                    _logger.LogInformation("Trying Flatpak direct method...");
                    
                    // Clean phone number (remove + and spaces)
                    var fallbackCleanPhone = phoneNumber.Replace("+", "").Replace(" ", "").Replace("-", "");
                    
                    var fallbackApprovalUrl = $"http://localhost:5173/approve/{approvalToken}";
                    var fallbackMessage = $"🚗 *SOLICITUD DE ACCESO*\n\n" +
                                         $"Visitante: *{visitorName}*\n" +
                                         $"Placas: *{plates}*\n\n" +
                                         $"Para autorizar el acceso, haga clic en el siguiente enlace:\n" +
                                         $"{fallbackApprovalUrl}\n\n" +
                                         $"_Este enlace es válido por 24 horas._";
                    
                    var encodedMessage = Uri.EscapeDataString(fallbackMessage);
                    var whatsappDirectUrl = $"https://wa.me/{fallbackCleanPhone}?text={encodedMessage}";
                    
                    var flatpakProcess = new System.Diagnostics.Process
                    {
                        StartInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "flatpak",
                            Arguments = $"run org.chromium.Chromium \"{whatsappDirectUrl}\"",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        }
                    };
                    
                    flatpakProcess.Start();
                    _logger.LogInformation("✅ Opened WhatsApp in browser via Flatpak for {Phone}", phoneNumber);
                    
                    // Don't wait for the process to finish since it opens a browser window
                    return true;
                }
                catch (Exception flatpakEx)
                {
                    _logger.LogError(flatpakEx, "Flatpak method also failed for {Phone}", phoneNumber);
                }
                
                // Final fallback: log the WhatsApp link for manual sending
                var finalFallbackApprovalUrl = $"http://localhost:5173/approve/{approvalToken}";
                var finalFallbackMessage = $"🚗 *SOLICITUD DE ACCESO*\n\nVisitante: *{visitorName}*\nPlacas: *{plates}*\n\nPara autorizar el acceso, haga clic en el siguiente enlace:\n{finalFallbackApprovalUrl}\n\n_Este enlace es válido por 24 horas._";
                var finalCleanPhone = phoneNumber.Replace("+", "").Replace(" ", "").Replace("-", "");
                
                _logger.LogInformation("📱 FALLBACK - Manual WhatsApp sending required:");
                _logger.LogInformation("Phone: {Phone}", phoneNumber);
                _logger.LogInformation("Copy this link and open in browser: https://wa.me/{Phone}?text={Message}", 
                    finalCleanPhone, 
                    Uri.EscapeDataString(finalFallbackMessage));
                
                return false;
            }
            finally
            {
                try
                {
                    // Give some time to see the result before closing (only if not headless)
                    await Task.Delay(5000);
                    
                    driver?.Quit();
                    driver?.Dispose();
                    _logger.LogInformation("Browser closed successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing WebDriver");
                }
            }
        }

        private async Task<bool> CheckInternetConnectivityAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                
                // Try multiple reliable endpoints
                var endpoints = new[]
                {
                    "https://www.google.com",
                    "https://1.1.1.1",
                    "https://8.8.8.8"
                };

                foreach (var endpoint in endpoints)
                {
                    try
                    {
                        var response = await client.GetAsync(endpoint);
                        if (response.IsSuccessStatusCode)
                        {
                            _logger.LogDebug("Internet connectivity confirmed via {Endpoint}", endpoint);
                            return true;
                        }
                    }
                    catch (HttpRequestException)
                    {
                        _logger.LogDebug("Failed to connect to {Endpoint}", endpoint);
                        continue;
                    }
                    catch (TaskCanceledException)
                    {
                        _logger.LogDebug("Timeout connecting to {Endpoint}", endpoint);
                        continue;
                    }
                }

                _logger.LogWarning("No internet connectivity detected after testing multiple endpoints");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking internet connectivity");
                return false;
            }
        }

        private bool IsNetworkException(Exception ex)
        {
            return ex is HttpRequestException ||
                   ex is SocketException ||
                   ex is TaskCanceledException ||
                   ex.Message.Contains("network", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("internet", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("dns", StringComparison.OrdinalIgnoreCase);
        }

        private void LogOfflineApprovalInstructions(string phoneNumber, string approvalToken, string visitorName, string plates)
        {
            var approvalUrl = $"http://localhost:5173/approve/{approvalToken}";
            var message = $"🚗 *SOLICITUD DE ACCESO*\n\n" +
                         $"Visitante: *{visitorName}*\n" +
                         $"Placas: *{plates}*\n\n" +
                         $"Para autorizar el acceso, haga clic en el siguiente enlace:\n" +
                         $"{approvalUrl}\n\n" +
                         $"_Este enlace es válido por 24 horas._";

            var cleanPhone = phoneNumber.Replace("+", "").Replace(" ", "").Replace("-", "");
            var whatsappUrl = $"https://wa.me/{cleanPhone}?text={Uri.EscapeDataString(message)}";

            _logger.LogWarning("🌐 MODO OFFLINE - WhatsApp no pudo enviarse automáticamente");
            _logger.LogInformation("📱 INSTRUCCIONES MANUALES:");
            _logger.LogInformation("Teléfono: {Phone}", phoneNumber);
            _logger.LogInformation("Visitante: {Visitor}", visitorName);
            _logger.LogInformation("Placas: {Plates}", plates);
            _logger.LogInformation("Link de aprobación: {ApprovalUrl}", approvalUrl);
            _logger.LogInformation("URL de WhatsApp (copiar y abrir en navegador cuando haya internet): {WhatsAppUrl}", whatsappUrl);
            _logger.LogInformation("Mensaje a enviar manualmente:");
            _logger.LogInformation("{Message}", message);
        }
    }
}