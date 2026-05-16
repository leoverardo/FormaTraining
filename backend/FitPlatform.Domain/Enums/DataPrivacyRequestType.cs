namespace FitPlatform.Domain.Enums;

public enum DataPrivacyRequestType
{
    Access = 1,
    Export = 2,
    Correction = 3,
    Deletion = 4,
    ConsentRevocation = 5,
    Other = 6,
    ExportData = 7,
    DeleteAccount = 8,
    AnonymizeData = 9
}

public enum DataPrivacyRequestStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Rejected = 4
}
