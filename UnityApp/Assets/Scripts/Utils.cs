public static class Utils
{
    public static string PREFIX_UNITY_STUDENT = "#w_s_";
    public static string PREFIX_WEB_STUDENT = "#u_s_";
    public static string PREFIX_WEB_TEACHER = "#w_t_";

    private static string[] _replacements = { PREFIX_UNITY_STUDENT, PREFIX_WEB_STUDENT, PREFIX_WEB_TEACHER };

    public static string stripPrefix(string usernameWithPrefix)
    {
        foreach (string to_replace in _replacements)
        {
            usernameWithPrefix = usernameWithPrefix.Replace(to_replace, "");
        }
        return usernameWithPrefix;
    }
}
