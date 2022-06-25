using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using dnSpy.Contracts.Settings;
using HoLLy.dnSpyExtension.Common;
using HoLLy.dnSpyExtension.Common.CodeInjection;

namespace HoLLy.dnSpyExtension.PluginSettings
{
    [Export(typeof(Settings))]
    internal class SettingsExportable : Settings
    {
        private readonly ISettingsService settingsService;
        private const string SectionRecentInjections = "RecentInjections";
        private const string SectionRecentInjectionsInjection = "Injection";

        [ImportingConstructor]
        public SettingsExportable(ISettingsService settingsService)
        {
            this.settingsService = settingsService;

            ISettingsSection sect = settingsService.GetOrCreateSection(Constants.SettingsGuid);
            UnderlineManagedAssemblies = sect.Attribute<bool?>(nameof(UnderlineManagedAssemblies)) ?? UnderlineManagedAssemblies;
            CopyInjectedDllToTemp = sect.Attribute<bool?>(nameof(CopyInjectedDllToTemp)) ?? CopyInjectedDllToTemp;
            AutoMapDllImports = sect.Attribute<bool?>(nameof(AutoMapDllImports)) ?? AutoMapDllImports;
            AutoMapOverrides = sect.Attribute<bool?>(nameof(AutoMapOverrides)) ?? AutoMapOverrides;

            var sectInjections = sect.TryGetSection(SectionRecentInjections);
            if (!(sectInjections is null))
                RecentInjections = sectInjections.SectionsWithName(SectionRecentInjectionsInjection).Select(InjectionArguments.FromSection).ToList();

            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            ISettingsSection sect = settingsService.RecreateSection(Constants.SettingsGuid);
            sect.Attribute(nameof(UnderlineManagedAssemblies), UnderlineManagedAssemblies);
            sect.Attribute(nameof(CopyInjectedDllToTemp), CopyInjectedDllToTemp);
            sect.Attribute(nameof(AutoMapDllImports), AutoMapDllImports);
            sect.Attribute(nameof(AutoMapOverrides), AutoMapOverrides);

            ISettingsSection sectInjections = sect.GetOrCreateSection(SectionRecentInjections);
            foreach (var injection in RecentInjections) {
                ISettingsSection sectInjection = sectInjections.CreateSection(SectionRecentInjectionsInjection);
                sectInjection.Attribute(nameof(injection.Path), injection.Path);
                sectInjection.Attribute(nameof(injection.Namespace), injection.Namespace);
                sectInjection.Attribute(nameof(injection.Type), injection.Type);
                sectInjection.Attribute(nameof(injection.Method), injection.Method);
                if (injection.Argument != null) sectInjection.Attribute(nameof(injection.Argument), injection.Argument);
            }
        }
    }
}
