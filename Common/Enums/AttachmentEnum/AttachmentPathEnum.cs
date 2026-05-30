using Utilities.ExtensionMethods;

namespace Utilities.Enums.AttachmentEnum;

public enum AttachmentPathEnum
{
    [DisplayMessage("/Temp/")]
    Temp = 1,
    [DisplayMessage("/Events/")]
    Events = 2,
    [DisplayMessage("/Committees/")]
    Committees = 3,
    [DisplayMessage("/MemberCompany/")]
    MemberCompany = 4,
    [DisplayMessage("/Member/")]
    Member = 5,
    [DisplayMessage("/News/")]
    News = 6,
    [DisplayMessage("/Publication/")]
    Publication = 7,
    [DisplayMessage("/AboutUs/")]
    AboutUs = 8,
    [DisplayMessage("/AboutUsObjective/")]
    AboutUsObjective = 9,
    [DisplayMessage("/AboutUsLeadershipManagement/")]
    AboutUsLeadershipManagement = 10,
    [DisplayMessage("/RegistrationRequest/")]
    RegistrationRequest = 11,
    [DisplayMessage("/PaymentReceipts/")]
    PaymentReceipts = 12,
    [DisplayMessage("/FlightTickets/")]
    FlightTickets = 13,
    [DisplayMessage("/MediaContacts/")]
    MediaContacts = 14,
    [DisplayMessage("/Gallery/")]
    Gallery = 15,
    [DisplayMessage("/Speaker/")]
    Speaker = 16,
    [DisplayMessage("/Sponsor/")]
    Sponsor = 17
}
