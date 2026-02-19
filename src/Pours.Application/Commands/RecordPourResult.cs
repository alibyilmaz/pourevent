namespace Pours.Application.Commands;

public sealed class RecordPourResult
{
    public bool IsNew { get; init; }
    public bool IsDuplicate { get; init; }
    public bool IsInvalid { get; init; }
    public IReadOnlyList<ValidationError> Errors { get; init; } = [];

    public static RecordPourResult Created() => new() { IsNew = true };
    public static RecordPourResult Duplicate() => new() { IsDuplicate = true };
    public static RecordPourResult Invalid(IReadOnlyList<ValidationError> errors) => new() { IsInvalid = true, Errors = errors };
}

public sealed record ValidationError(string Field, string Message);
