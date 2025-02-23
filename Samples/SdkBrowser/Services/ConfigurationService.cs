﻿using Microsoft.Maui.Devices;
using SDKBrowserMaui.Common;
using System;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace SDKBrowserMaui.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private const string ResourceLocation = "SDKBrowserMaui.config.xml";

        public Configuration Configuration { get; private set; }

        public ConfigurationService()
        {
            this.Configuration = LoadConfiguration(ResourceLocation);
        }

        private static Configuration LoadConfiguration(string resource)
        {
            var type = typeof(ConfigurationService);
            var typeInfo = type.GetTypeInfo();
            var assembly = typeInfo.Assembly;

            using (var stream = assembly.GetManifestResourceStream(resource))
            {
                var serializer = new XmlSerializer(typeof(Configuration));
                var configuration = (Configuration)serializer.Deserialize(stream);

                UpdateConfiguration(configuration);

                return configuration;
            }
        }

        private static void UpdateConfiguration(Configuration configuration)
        {
            var count = configuration.Controls.Count;

            for (int index = count - 1; index >= 0; index--)
            {
                var control = configuration.Controls[index];

                control.Configuration = configuration;

                if (IsExcluded(control.ExcludeFrom))
                {
                    configuration.Controls.RemoveAt(index);
                }
                else
                {
                    UpdateControl(control);

                    if (control.Categories.Count == 0)
                    {
                        configuration.Controls.RemoveAt(index);
                    }
                }
            }
        }

        private static void UpdateControl(Control control)
        {
            var count = control.Categories.Count;

            for (int index = count - 1; index >= 0; index--)
            {
                var category = control.Categories[index];

                category.Control = control;

                if (IsExcluded(category.ExcludeFrom))
                {
                    control.Categories.RemoveAt(index);
                }
                else
                {
                    UpdateCategory(category);

                    if (category.Examples.Count == 0)
                    {
                        control.Categories.RemoveAt(index);
                    }
                }
            }
        }

        private static void UpdateCategory(Category category)
        {
            var count = category.Examples.Count;

            for (int index = count - 1; index >= 0; index--)
            {
                var example = category.Examples[index];

                example.Category = category;

                if (IsExcluded(example.ExcludeFrom))
                {
                    category.Examples.RemoveAt(index);
                }
            }
        }

        private static bool IsExcluded(string exclusions)
        {
            if (string.IsNullOrEmpty(exclusions))
            {
                return false;
            }

            var platforms = exclusions.Split(',');

            string netVersion = string.Empty;

#if NET8_0
            netVersion = "net8";
#elif NET9_0
            netVersion = "net9";
#endif

            if (platforms.Contains(netVersion, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }

            return platforms.Contains(DeviceInfo.Platform.ToString(),
                StringComparer.OrdinalIgnoreCase);
        }
    }
}
