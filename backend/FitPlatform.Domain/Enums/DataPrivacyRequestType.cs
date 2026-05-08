namespace FitPlatform.Domain.Enums;

public enum DataPrivacyRequestType
{
    ExportData = 1,
    DeleteAccount = 2,
    AnonymizeData = 3
}

public enum DataPrivacyRequestStatus
{
    Pending = 1,
    Completed = 2,
    Rejected = 3
}
