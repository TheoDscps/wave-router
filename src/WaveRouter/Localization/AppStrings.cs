namespace WaveRouter.Localization;

/// <summary>The two supported language catalogs, keyed by "Area.Key". English is the fallback whenever a
/// key is missing from another language, so a gap shows up as English text rather than a blank/raw key.</summary>
internal static class AppStrings
{
    public static readonly Dictionary<string, string> French = new()
    {
        ["MainWindow.RulesHeading"] = "Règles",
        ["MainWindow.AddRule"] = "+ Ajouter une règle",
        ["MainWindow.ImportExisting"] = "Importer les assignations existantes",
        ["MainWindow.ImportExistingTooltip"] = "Importe les routages déjà configurés dans Windows pour les apps actuellement ouvertes",
        ["MainWindow.NoRulesYet"] = "Aucune règle pour le moment.",
        ["MainWindow.SelectOrAddRule"] = "Sélectionne une règle, ou ajoutes-en une nouvelle.",
        ["MainWindow.ExecutableLabel"] = "Exécutable",
        ["MainWindow.TrackLabel"] = "Piste Wave Link",
        ["MainWindow.Save"] = "Enregistrer",
        ["MainWindow.Delete"] = "Supprimer",
        ["MainWindow.DismissStatus"] = "Fermer ce message",

        ["NewAppPrompt.Title"] = "Nouvelle app détectée",
        ["NewAppPrompt.Heading"] = "Nouvelle app détectée",
        ["NewAppPrompt.TrackQuestion"] = "Router vers quelle piste ?",
        ["NewAppPrompt.Route"] = "Router",
        ["NewAppPrompt.Ignore"] = "Ignorer",

        ["Tray.OpenRules"] = "Ouvrir les règles",
        ["Tray.Settings"] = "Réglages",
        ["Tray.Quit"] = "Quitter",
        ["Tray.RoutingDoneTitle"] = "Routage effectué",
        ["Tray.RoutingFailedTitle"] = "Échec du routage",
        ["Tray.NoRuleTitle"] = "Nouvelle source audio",
        ["Tray.NoRuleSuffix"] = "aucune règle",

        ["Validation.RequiredFields"] = "L'exécutable et la piste sont obligatoires.",
        ["Rule.DuplicateTitle"] = "Règle en double",
        ["Rule.DuplicateMessage"] = "Une règle existe déjà pour \"{0}\". L'écraser ?",
        ["Import.NothingFound"] = "Aucune assignation Wave Link ou Windows trouvée à importer.",
        ["Import.OneImported"] = "1 assignation importée.",
        ["Import.ManyImported"] = "{0} assignations importées.",

        ["Settings.Title"] = "Réglages",
        ["Settings.Theme"] = "Thème",
        ["Settings.ThemeDark"] = "Sombre",
        ["Settings.ThemeLight"] = "Clair",
        ["Settings.Language"] = "Langue",
        ["Settings.LanguageFrench"] = "Français",
        ["Settings.LanguageEnglish"] = "English",
        ["Settings.StartWithWindows"] = "Démarrer avec Windows",
        ["Settings.StartWithWindowsFailedTitle"] = "Échec",
        ["Settings.StartWithWindowsFailedMessage"] = "Impossible de modifier le démarrage automatique avec Windows.",
        ["Settings.Close"] = "Fermer",
    };

    public static readonly Dictionary<string, string> English = new()
    {
        ["MainWindow.RulesHeading"] = "Rules",
        ["MainWindow.AddRule"] = "+ Add a rule",
        ["MainWindow.ImportExisting"] = "Import existing assignments",
        ["MainWindow.ImportExistingTooltip"] = "Imports routing already configured in Windows for currently open apps",
        ["MainWindow.NoRulesYet"] = "No rules yet.",
        ["MainWindow.SelectOrAddRule"] = "Select a rule, or add a new one.",
        ["MainWindow.ExecutableLabel"] = "Executable",
        ["MainWindow.TrackLabel"] = "Wave Link Track",
        ["MainWindow.Save"] = "Save",
        ["MainWindow.Delete"] = "Delete",
        ["MainWindow.DismissStatus"] = "Dismiss this message",

        ["NewAppPrompt.Title"] = "New app detected",
        ["NewAppPrompt.Heading"] = "New app detected",
        ["NewAppPrompt.TrackQuestion"] = "Route to which track?",
        ["NewAppPrompt.Route"] = "Route",
        ["NewAppPrompt.Ignore"] = "Ignore",

        ["Tray.OpenRules"] = "Open rules",
        ["Tray.Settings"] = "Settings",
        ["Tray.Quit"] = "Quit",
        ["Tray.RoutingDoneTitle"] = "Routing applied",
        ["Tray.RoutingFailedTitle"] = "Routing failed",
        ["Tray.NoRuleTitle"] = "New audio source",
        ["Tray.NoRuleSuffix"] = "no rule",

        ["Validation.RequiredFields"] = "The executable and the track are both required.",
        ["Rule.DuplicateTitle"] = "Duplicate rule",
        ["Rule.DuplicateMessage"] = "A rule for \"{0}\" already exists. Overwrite it?",
        ["Import.NothingFound"] = "No existing Wave Link or Windows audio assignment found to import.",
        ["Import.OneImported"] = "Imported 1 existing assignment.",
        ["Import.ManyImported"] = "Imported {0} existing assignments.",

        ["Settings.Title"] = "Settings",
        ["Settings.Theme"] = "Theme",
        ["Settings.ThemeDark"] = "Dark",
        ["Settings.ThemeLight"] = "Light",
        ["Settings.Language"] = "Language",
        ["Settings.LanguageFrench"] = "Français",
        ["Settings.LanguageEnglish"] = "English",
        ["Settings.StartWithWindows"] = "Start with Windows",
        ["Settings.StartWithWindowsFailedTitle"] = "Failed",
        ["Settings.StartWithWindowsFailedMessage"] = "Could not update the Windows startup setting.",
        ["Settings.Close"] = "Close",
    };
}
