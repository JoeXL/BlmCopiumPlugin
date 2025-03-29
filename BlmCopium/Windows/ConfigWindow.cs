using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace BlmCopium.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("BLM Copium Config")
    {
        Flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(380, 200);
        SizeCondition = ImGuiCond.FirstUseEver;

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw() { }

    public override void Draw()
    {
        bool shouldSave = false;
        // can't ref a property, so use a local copy
        var enochainDurationValue = Configuration.EnochainDuration;
        if (ImGui.DragFloat("Enochain Duration", ref enochainDurationValue, 0.2f, 0, 60))
        {
            Configuration.EnochainDuration = enochainDurationValue;
            shouldSave = true;
        }

        var interruptValue = Configuration.InterruptCastsWhenTimerIsZero;
        if (ImGui.Checkbox("Interrupt Casts", ref interruptValue))
        {
            Configuration.InterruptCastsWhenTimerIsZero = interruptValue;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            shouldSave = true;
        }

        var xCoord = Configuration.TimerXCoord;
        if (ImGui.DragInt("X", ref xCoord))
        {
            Configuration.TimerXCoord = xCoord;
            shouldSave = true;
        }

        var yCoord = Configuration.TimerYCoord;
        if (ImGui.DragInt("Y", ref yCoord))
        {
            Configuration.TimerYCoord = yCoord;
            shouldSave = true;
        }

        var scale = Configuration.Scale;
        if(ImGui.DragFloat("Scale", ref scale, 0.2f, 0, 10))
        {
            Configuration.Scale = scale;
            shouldSave = true;
        }

        if(shouldSave)
        {
            Configuration.Save();
        }
    }
}
