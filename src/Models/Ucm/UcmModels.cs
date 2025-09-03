namespace MicroJack.API.Models.Ucm;

public class UcmApiEnvelope<T>
{
    public T? Request { get; set; }
}

public class UcmApiRequest
{
    public string Action { get; set; } = string.Empty;
    public string? User { get; set; }
    public string? Version { get; set; }
    public string? Token { get; set; }
    public string? Cookie { get; set; }
    public string? Options { get; set; }
}

public class UcmApiResponseEnvelope<T>
{
    public int Status { get; set; }
    public T? Response { get; set; }
}

public class UcmChallengeResponse
{
    public string? Challenge { get; set; }
}

public class UcmLoginResponse
{
    public string? Cookie { get; set; }
}

public class UcmAccount
{
    public string? Extension { get; set; }
    public string? Fullname { get; set; }
    public string? Status { get; set; }
    public string? Account_Type { get; set; }
}

public class UcmListAccountResponse
{
    public List<UcmAccount> Account { get; set; } = new();
}

