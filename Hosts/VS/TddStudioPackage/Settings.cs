
using EnvDTE;

namespace R4nd0mApps.TddStud10.Hosts.VS
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Settings;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Settings;

    [Export(typeof(Settings))]
    public class Settings
    {
        private readonly SVsServiceProvider _vsServiceProvider;
        public const string IsTddStudioEnabled = "IsTddStudioEnabled";
        private const string CollectionPathPrefix = "TddStudio";
        private readonly WritableSettingsStore _writableSettingsStore;

        [ImportingConstructor]
        public Settings(SVsServiceProvider vsServiceProvider)
        {
            _vsServiceProvider = vsServiceProvider;
            var shellSettingsManager = new ShellSettingsManager(vsServiceProvider);
            _writableSettingsStore = shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            if (!_writableSettingsStore.CollectionExists(GetCollectionPath()))
            {
                _writableSettingsStore.CreateCollection(GetCollectionPath());
            }
        }

        public void SetSetting(string settingName, bool settingValue)
        {
            _writableSettingsStore.SetBoolean(GetCollectionPath(), settingName, settingValue);
        }

        public bool GetSetting(string settingName)
        {
            return _writableSettingsStore.GetBoolean(GetCollectionPath(), settingName, true);
        }

        private string GetCollectionPath()
        {
            var dte = (DTE)_vsServiceProvider.GetService(typeof(DTE));
            var solutionName = System.IO.Path.GetFileNameWithoutExtension(dte.Solution.FullName);
            return string.Format("{0}/{1}", CollectionPathPrefix, solutionName);
        }
    }
}
