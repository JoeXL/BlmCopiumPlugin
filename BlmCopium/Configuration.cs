using Dalamud.Configuration;
using Dalamud.Plugin;
using Lumina.Excel.Sheets;
using System;

namespace BlmCopium;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool InterruptCastsWhenTimerIsZero { get; set; } = true;

    public float EnochainDuration { get; set; } = 15.0f;

    public int TimerXCoord { get; set; } = -45;
    public int TimerYCoord { get; set; } = 15;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
