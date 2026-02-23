using System;
using System.IO;
using Newtonsoft.Json;

namespace QOLFramework.Config
{
    /// <summary>
    /// Gestor opcional de configuração por módulo. Guarda/carrega JSON em ficheiros por nome de módulo.
    /// Define ConfigDirectory (ex.: pasta do plugin) antes de usar, ou usa o diretório atual.
    /// </summary>
    public static class ConfigManager
    {
        private static string _configDirectory;
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        /// <summary>Pasta onde os ficheiros de config são guardados (ex.: .../LabAPI/plugins/QOL). Se null, usa Environment.CurrentDirectory.</summary>
        public static string ConfigDirectory
        {
            get => string.IsNullOrEmpty(_configDirectory) ? Environment.CurrentDirectory : _configDirectory;
            set => _configDirectory = value;
        }

        /// <summary>Obtém o caminho do ficheiro de config para um módulo (nome do ficheiro sem extensão).</summary>
        public static string GetConfigPath(string moduleKey)
        {
            var safeName = string.IsNullOrEmpty(moduleKey) ? "config" : string.Join("_", moduleKey.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(ConfigDirectory, safeName + ".json");
        }

        /// <summary>Carrega a config do módulo desde disco. Se o ficheiro não existir, retorna new T().</summary>
        public static T Load<T>(string moduleKey) where T : class, new()
        {
            var path = GetConfigPath(moduleKey);
            if (!File.Exists(path))
                return new T();

            try
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<T>(json, Settings) ?? new T();
            }
            catch (Exception ex)
            {
                LabApi.Features.Console.Logger.Warn($"[QOL:Config] Failed to load config for '{moduleKey}': {ex.Message}");
                return new T();
            }
        }

        /// <summary>Guarda a config do módulo em disco.</summary>
        public static bool Save<T>(string moduleKey, T config) where T : class
        {
            if (config == null) return false;

            var path = GetConfigPath(moduleKey);
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonConvert.SerializeObject(config, Settings);
                File.WriteAllText(path, json);
                return true;
            }
            catch (Exception ex)
            {
                LabApi.Features.Console.Logger.Warn($"[QOL:Config] Failed to save config for '{moduleKey}': {ex.Message}");
                return false;
            }
        }

        /// <summary>Carrega ou cria config e guarda se for a primeira vez (SaveIfMissing = true).</summary>
        public static T LoadOrCreate<T>(string moduleKey, bool saveIfMissing = true) where T : class, new()
        {
            var path = GetConfigPath(moduleKey);
            if (File.Exists(path))
                return Load<T>(moduleKey);

            var created = new T();
            if (saveIfMissing)
                Save(moduleKey, created);
            return created;
        }
    }
}
