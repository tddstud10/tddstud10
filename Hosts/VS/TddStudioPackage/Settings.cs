
namespace R4nd0mApps.TddStud10.Hosts.VS
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Settings;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Settings;

    [Export(typeof(Settings))]
    public class Settings
    {
        public const string IsTddStudioEnabled = "IsTddStudioEnabled";
        private const string CollectionPath = "TddStudio";
        private readonly WritableSettingsStore _writableSettingsStore;

        [ImportingConstructor]
        public Settings(SVsServiceProvider vsServiceProvider)
        {
            var shellSettingsManager = new ShellSettingsManager(vsServiceProvider);
            _writableSettingsStore = shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            if (!_writableSettingsStore.CollectionExists(CollectionPath))
            {
                _writableSettingsStore.CreateCollection(CollectionPath);
            }
        }

        public void SetSetting(string settingName, bool settingValue)
        {
            _writableSettingsStore.SetBoolean(CollectionPath, settingName, settingValue);
        }

        public bool GetSetting(string settingName)
        {
            return _writableSettingsStore.GetBoolean(CollectionPath, settingName, true);
        }
    }
}
