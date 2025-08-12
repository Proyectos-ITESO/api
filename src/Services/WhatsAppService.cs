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

            if (!await CheckInternetConnectivityAsync())
            {
                _logger.LogWarning("No internet connection detected. WhatsApp service will work in offline mode.");
                LogOfflineApprovalInstructions(phoneNumber, approvalToken, visitorName, plates);
                return false;
            }

            var message = $"🚗 *SOLICITUD DE ACCESO*\n\n" +
                         $"Visitante: *{visitorName}*\n" +
                         $"Placas: *{plates}*\n\n" +
                         $"Para autorizar el acceso, haga clic en el siguiente enlace:\n" +
                         $"http://localhost:5173/approve/{approvalToken}\n\n" +
                         $"_Este enlace es válido por 24 horas._";

            return await SendWhatsAppMessageAsync(phoneNumber, message);
        }

        public async Task<bool> SendPreRegistrationNotificationAsync(string phoneNumber, string residentName, string preRegistrationName, DateTime preRegistrationDate)
        {
            _logger.LogInformation("Sending pre-registration notification to {Phone} for {ResidentName}", phoneNumber, residentName);

            if (!await CheckInternetConnectivityAsync())
            {
                _logger.LogWarning("No internet connection. Cannot send pre-registration notification to {Phone}", phoneNumber);
                return false;
            }

            var message = $"🔔 *AVISO DE PRE-REGISTRO*\n\n" +
                          $"Hola *{residentName}*,\n" +
                          $"Se ha creado un nuevo pre-registro a tu nombre:\n\n" +
                          $"👤 Nombre: *{preRegistrationName}*\n" +
                          $"📅 Fecha: *{preRegistrationDate:dd/MM/yyyy}*\n" +
                          $"⏰ Hora: *{preRegistrationDate:HH:mm} hs*\n\n" +
                          $"Por favor, notifica a la guardia si esperas a esta visita.";

            return await SendWhatsAppMessageAsync(phoneNumber, message);
        }

        private async Task<bool> SendWhatsAppMessageAsync(string phoneNumber, string message)
        {
            ChromeDriver? driver = null;
            try
            {
                var cleanPhone = phoneNumber.Replace("+", "").Replace(" ", "").Replace("-", "");
                var encodedMessage = Uri.EscapeDataString(message);
                var whatsappUrl = $"https://web.whatsapp.com/send?phone={cleanPhone}&text={encodedMessage}";

                _logger.LogInformation("Preparing to send WhatsApp via Selenium.");
                _logger.LogInformation("URL: {Url}", whatsappUrl);

                var options = new ChromeOptions();
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--disable-extensions");
                options.AddArgument("--window-size=1920,1080");
                options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                options.AddExcludedArgument("enable-automation");
                
                _logger.LogInformation("Setting up ChromeDriver using WebDriverManager...");
                new DriverManager().SetUpDriver(new ChromeConfig());
                _logger.LogInformation("ChromeDriver setup complete.");
                
                driver = new ChromeDriver(options);
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);

                _logger.LogInformation("Navigating to WhatsApp...");
                driver.Navigate().GoToUrl(whatsappUrl);

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));

                wait.Until(d => d.FindElement(By.Id("app")));
                
                try
                {
                    var qrCodeElement = driver.FindElement(By.CssSelector("div[data-testid='qr-code']"));
                     if (qrCodeElement.Displayed)
                    {
                        _logger.LogWarning("WhatsApp login required. Please scan the QR code within the next 45 seconds.");
                        await Task.Delay(45000);
                    }
                }
                catch (NoSuchElementException)
                {
                    _logger.LogInformation("QR code not found, assuming already logged in.");
                }

                 var sendButton = wait.Until(d => {
                    var elements = d.FindElements(By.CssSelector("button[aria-label='Send'], button[aria-label='Enviar'], span[data-testid='send']"));
                    return elements.FirstOrDefault(e => e.Enabled);
                });

                if (sendButton != null)
                {
                    sendButton.Click();
                    _logger.LogInformation("Send button clicked. Waiting for message to be sent...");
                    await Task.Delay(3000);
                    _logger.LogInformation("✅ WhatsApp message sent successfully to {Phone}", phoneNumber);
                    return true;
                }
                else
                {
                    _logger.LogError("Could not find or interact with the send button.");
                    return false;
                }
            }
            catch (WebDriverTimeoutException ex)
            {
                 _logger.LogError(ex, "Timeout waiting for WhatsApp elements for phone {Phone}. The page might have changed or requires login.", phoneNumber);
                 return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while sending WhatsApp to {Phone}.", phoneNumber);
                return false;
            }
            finally
            {
                driver?.Quit();
                driver?.Dispose();
            }
        }

        private async Task<bool> CheckInternetConnectivityAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                
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