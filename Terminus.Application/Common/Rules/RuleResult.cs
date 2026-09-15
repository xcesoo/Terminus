namespace Terminus.Application.Common.Rules;

public class RuleResult
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }

    private RuleResult(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static RuleResult Ok() => new(true, null);
    public static RuleResult Failed(string message) => new(false, message);
    
    public static implicit operator bool(RuleResult result) => result.IsSuccess;
}