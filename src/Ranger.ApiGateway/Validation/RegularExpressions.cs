namespace Ranger.ApiGateway
{
    public static class RegularExpressions
    {
        public static readonly string NAME = @"^[a-zA-Z]{1,48}[_\-\,\.\']{0,1}$";
        public static readonly string PASSWORD_SPECIAL_CHARACTER = @"[_\-\`\~\!\@\#\$\%\^\&\*\(\)\+\=\{\}\[\]\\|\;\:\\'\""\,\<\.\>\/\?]";
        public static readonly string PASSWORD_NUMBER = @"[0-9]";
        public static readonly string PASSWORD_LOWERCASE_LETTER = @"[a-z]";
        public static readonly string PASSWORD_UPPERCASE_LETTER = @"[A-Z]";
        public static readonly string ORGANIZATION_NAME = @"^[a-zA-Z0-9]{1}[a-zA-Z0-9_\-\s\']{1,46}[a-zA-Z0-9]{1}$";
        public static readonly string ORGANIZATION_DOMAIN = @"^[a-zA-Z0-9]{1}[a-zA-Z0-9\-]{1,26}[a-zA-Z0-9]{1}$";
        public static readonly string PROJECT_NAME = @"^[a-zA-Z0-9]+[a-zA-Z0-9_\-\'\,\.\s]{1,126}[a-z0-9]{1}$";
        public static readonly string GEOFENCE_INTEGRATION_NAME = @"^[a-z0-9]+[a-z0-9\-]{1,126}[a-z0-9]{1}$";
        public static readonly string GEOFENCE_SEARCH_NAME = @"[A-Za-z0-9\-]{1,128}";
    }
}