namespace Application.Contracts.Services.CommitteeSessions;

public record CommitteeSessionInviteMailRecipient(string? Email, string DisplayName);

public record CommitteeSessionInviteMailContext(
    long SessionId,
    long CommitteeId,
    string CommitteeName,
    DateTime ScheduledAt,
    string? Location,
    List<long> UserIds);

public interface ISessionInviteMailService
{
    Task ScheduleInviteEmails(CommitteeSessionInviteMailContext context);

    Task<bool> SendInviteEmailAsync(
        CommitteeSessionInviteMailContext context,
        CommitteeSessionInviteMailRecipient recipient);
}
