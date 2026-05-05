using System.Configuration;

namespace ControlPersonalAppWeb.Infrastructure
{
    public static class AppSettings
    {
        public static string SmtpHost => ConfigurationManager.AppSettings["Smtp:Host"] ?? "mail.ingenieriajcp.cl";
        public static int SmtpPort => int.TryParse(ConfigurationManager.AppSettings["Smtp:Port"], out var p) ? p : 25;
        public static string SmtpUser => ConfigurationManager.AppSettings["Smtp:User"] ?? "";
        public static string SmtpPassword => ConfigurationManager.AppSettings["Smtp:Password"] ?? "";
        public static string SmtpFromAddress => ConfigurationManager.AppSettings["Smtp:FromAddress"] ?? "";
        public static string SmtpFromName => ConfigurationManager.AppSettings["Smtp:FromName"] ?? "SGA JCP";
        public static bool SmtpEnableSsl => bool.TryParse(ConfigurationManager.AppSettings["Smtp:EnableSsl"], out var s) && s;
        public static string FileStoragePath => ConfigurationManager.AppSettings["FileStoragePath"] ?? @"C:\Data\Archivos\";
    }
}
