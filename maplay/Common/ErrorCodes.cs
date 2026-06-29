namespace Maplay.Common;

/// <summary>集中管理 errorCode，前後端與測試共用。</summary>
public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string TokenExpired = "TOKEN_EXPIRED";
    public const string InternalError = "INTERNAL_ERROR";
    public const string NotImplemented = "NOT_IMPLEMENTED";

    // auth
    public const string EmailAlreadyExists = "EMAIL_ALREADY_EXISTS";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string RefreshInvalid = "REFRESH_INVALID";
    public const string OAuthFailed = "OAUTH_FAILED";

    // spot-management
    public const string SpotNotFound = "SPOT_NOT_FOUND";
    public const string TempDateInvalid = "TEMP_DATE_INVALID";
    public const string FileTypeInvalid = "FILE_TYPE_INVALID";
    public const string FileTooLarge = "FILE_TOO_LARGE";
    public const string TooManyFiles = "TOO_MANY_FILES";

    // reviews
    public const string ReviewDuplicate = "REVIEW_DUPLICATE";
    public const string ReviewNotFound = "REVIEW_NOT_FOUND";
    public const string SpotNotApproved = "SPOT_NOT_APPROVED";

    // gov-import
    public const string ImportFailed = "IMPORT_FAILED";
    public const string ImportFormatInvalid = "IMPORT_FORMAT_INVALID";
}
