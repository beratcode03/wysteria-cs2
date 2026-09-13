namespace Wysteria.Core;

public enum EconomyPurchaseStatus
{
    Success,
    Unavailable,
    InsufficientFunds,
    InvalidProduct,
    Failed
}

public sealed record EconomyPurchaseRequest(
    ulong PlayerId,
    string ProductId,
    int Cost);

public sealed record EconomyPurchaseResult(
    EconomyPurchaseStatus Status,
    string? Message = null)
{
    public bool Succeeded => Status == EconomyPurchaseStatus.Success;

    public static EconomyPurchaseResult Success(string? message = null) =>
        new(EconomyPurchaseStatus.Success, message);

    public static EconomyPurchaseResult Unavailable(string? message = null) =>
        new(EconomyPurchaseStatus.Unavailable, message);

    public static EconomyPurchaseResult InsufficientFunds(string? message = null) =>
        new(EconomyPurchaseStatus.InsufficientFunds, message);

    public static EconomyPurchaseResult InvalidProduct(string? message = null) =>
        new(EconomyPurchaseStatus.InvalidProduct, message);

    public static EconomyPurchaseResult Failed(string? message = null) =>
        new(EconomyPurchaseStatus.Failed, message);
}

public interface IEconomyAdapter
{
    bool IsAvailable { get; }

    EconomyPurchaseResult TryPurchase(EconomyPurchaseRequest request);
}

public sealed class UnavailableEconomyAdapter : IEconomyAdapter
{
    public bool IsAvailable => false;

    public EconomyPurchaseResult TryPurchase(EconomyPurchaseRequest request) =>
        EconomyPurchaseResult.Unavailable(
            "JailShop/economy adapter kayıtlı değil; satın alma uygulanmadı.");
}

public static class WysteriaEconomy
{
    private static IEconomyAdapter _adapter = new UnavailableEconomyAdapter();

    public static IEconomyAdapter Adapter => _adapter;

    public static void RegisterAdapter(IEconomyAdapter adapter) =>
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
}
