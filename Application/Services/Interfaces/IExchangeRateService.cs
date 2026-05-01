namespace MadeByMe.Application.Services.Interfaces
{
    public interface IExchangeRateService
    {
        Task<decimal> GetUsdRateAsync();
    }
}