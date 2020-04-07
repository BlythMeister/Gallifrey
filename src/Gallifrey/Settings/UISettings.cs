﻿using Newtonsoft.Json;
using System.ComponentModel;

namespace Gallifrey.Settings
{
    public interface IUiSettings
    {
        int Height { get; set; }
        int Width { get; set; }
        string Theme { get; set; }
        bool TopMostOnFlyoutOpen { get; set; }
    }

    public class UiSettings : IUiSettings
    {
        [DefaultValue(0)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Height { get; set; }

        [DefaultValue(0)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Width { get; set; }

        [DefaultValue("Light.Blue")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string Theme { get; set; }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool TopMostOnFlyoutOpen { get; set; }

        public UiSettings()
        {
            Theme = "Light.Blue";
            TopMostOnFlyoutOpen = false;
        }
    }
}
