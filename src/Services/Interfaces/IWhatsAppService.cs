namespace MicroJack.API.Services.Interfaces
{
    public interface IWhatsAppService
    {
        Task<bool> SendApprovalWhatsAppAsync(string phoneNumber, string approvalToken, string visitorName, string plates);
    }
}