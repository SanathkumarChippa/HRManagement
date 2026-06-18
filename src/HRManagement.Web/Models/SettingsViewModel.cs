namespace HRManagement.Web.Models
{
    public class SettingsViewModel
    {
        public string ThemePreference { get; set; } = "light";
        public bool EmailAlertsEnabled { get; set; } = true;
        public bool InAppNotificationsEnabled { get; set; } = true;
    }
}
