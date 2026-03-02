using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace LurkerCommand.Services
{
    public static class ConfigManager
    {
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");
        private static readonly Dictionary<string, string> Settings = new(16);

        public static void Initialize()
        {
            if (File.Exists(ConfigPath))
            {
                Load();
            }
            else
            {
                SetDefaults();
                Save();
            }
        }

        private static void SetDefaults()
        {
            Settings["Width"] = "1440";
            Settings["Height"] = "1080";
            Settings["FullScreen"] = "false";
            Settings["WindowTitle"] = "Lurker Command";
            Settings["AltF4"] = "false";
            Settings["AllowResizing"] = "true";
            Settings["FullScreenKey"] = "F11";
        }

        public static T Get<T>(string key)
        {
            if (!Settings.TryGetValue(key, out var value)) return default;

            try
            {
                if (typeof(T) == typeof(int)) return (T)(object)int.Parse(value);
                else if (typeof(T) == typeof(float)) return (T)(object)float.Parse(value);
                else if (typeof(T) == typeof(bool)) return (T)(object)bool.Parse(value);
                else if (typeof(T) == typeof(Keys)) return (T)Enum.Parse(typeof(Keys), value);
                return (T)(object)value;
            }
            catch
            {
                return default;
            }
        }

        private static void Load()
        {
            foreach (string line in File.ReadLines(ConfigPath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                int separator = line.IndexOf('=');
                if (separator > 0)
                {
                    Settings[line[..separator].Trim()] = line[(separator + 1)..].Trim();
                }
            }
        }

        public static void Save()
        {
            using StreamWriter writer = new StreamWriter(ConfigPath);
            foreach (var kvp in Settings)
            {
                writer.WriteLine($"{kvp.Key}={kvp.Value}");
            }
        }
    }
}