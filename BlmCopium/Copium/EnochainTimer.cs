using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lumina.Data.Parsing;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using static FFXIVClientStructs.FFXIV.Client.UI.RaptureAtkHistory.Delegates;
using Dalamud.Hooking;
using static FFXIVClientStructs.FFXIV.Client.System.String.Utf8String.Delegates;
using Dalamud.Game.ClientState.Objects.SubKinds;
using System.Runtime.InteropServices;
using LuminaAction = Lumina.Excel.Sheets.Action;
using Lumina.Excel;

namespace BlmCopium.Copium;

internal unsafe class EnochainTimer
{
    private readonly uint fireAction = 141;
    private readonly uint fireIIIAction = 152;
    private readonly uint blizzardAction = 152;
    private readonly uint blizzardIIIAction = 154;
    private readonly uint paradoxAction = 25797;
    private readonly uint flareAction = 162;
    private readonly uint highFireIIAction = 25794;
    private readonly uint highBlizzardIIAction = 25795;
    private readonly uint freezeAction = 159;
    private readonly uint despairAction = 16505;
    private readonly uint manafontAction = 158;
    private readonly uint transposeAction = 149;

    private readonly uint fireIVAction = 3577;
    private readonly uint blizzardIVAction = 3576;
    private readonly uint flarestarAction = 36989;

    private readonly uint enochainDuration = 15;

    private readonly uint interruptActionId = 2; //This is currently jump

    private ComboDetail combo = new ComboDetail();

    private delegate void OnActionUsedDelegate(uint sourceId, IntPtr sourceCharacter, IntPtr pos, IntPtr effectHeader, IntPtr effectArray, IntPtr effectTrail);
    private Hook<OnActionUsedDelegate>? onActionUsedHook;

    private ExcelSheet<LuminaAction>? actionSheet = Plugin.DataManager.GetExcelSheet<LuminaAction>();

    /*private delegate void OnActorControlDelegate(uint entityId, uint id, uint unk1, uint type, uint unk2, uint unk3, uint unk4, uint unk5, UInt64 targetId, byte unk6);
    private Hook<OnActorControlDelegate>? _onActorControlHook;

    private delegate void OnCastDelegate(uint sourceId, IntPtr sourceCharacter);
    private Hook<OnCastDelegate>? _onCastHook;*/

    private Configuration Configuration;

    public EnochainTimer(Plugin plugin)
    {
        try
        {
            onActionUsedHook = Plugin.GameInteropProvider.HookFromSignature<OnActionUsedDelegate>(
                "40 ?? 56 57 41 ?? 41 ?? 41 ?? 48 ?? ?? ?? ?? ?? ?? ?? 48",
                OnActionUsed
            );
            onActionUsedHook?.Enable();

            /*_onActorControlHook = Plugin.GameInteropProvider.HookFromSignature<OnActorControlDelegate>(
                "E8 ?? ?? ?? ?? 0F B7 0B 83 E9 64",
                OnActorControl
            );
            _onActorControlHook?.Enable();

            _onCastHook = Plugin.GameInteropProvider.HookFromSignature<OnCastDelegate>(
                "40 56 41 56 48 81 EC ?? ?? ?? ?? 48 8B F2",
                OnCast
            );
            _onCastHook?.Enable();*/
        }
        catch (Exception e)
        {
            Plugin.Log.Error("Error initiating hooks: " + e.Message);
        }

        Configuration = plugin.Configuration;
    }

    protected void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        //Plugin.Framework.Update -= Update;

        onActionUsedHook?.Disable();
        onActionUsedHook?.Dispose();

        /*_onActorControlHook?.Disable();
        _onActorControlHook?.Dispose();

        _onCastHook?.Disable();
        _onCastHook?.Dispose();*/
    }

    private bool CantCastWithoutEnochain(uint action)
    {
        if (
            action == fireIVAction ||
            action == blizzardIVAction ||
            action == flarestarAction
            )
            return true;
        return false;
    }

    private bool DoesActinoResetEnochain(uint action)
    {
        if (
            action == fireAction ||
            action == fireIIIAction ||
            action == blizzardAction ||
            action == blizzardIIIAction ||
            action == paradoxAction ||
            action == flareAction ||
            action == highFireIIAction ||
            action == highBlizzardIIAction ||
            action == freezeAction ||
            action == despairAction ||
            action == manafontAction ||
            action == transposeAction
            )
            return true;
        return false;
    }

    public const int
        EnochainTimerNode = BlmCopiumNodeBase + 1,
        BlmCopiumNodeBase = 0x53540000;

    AtkTextNode* textNode = null;

    private void SetupTextNode()
    {
        if (textNode != null) return;

        var addon = "_ParameterWidget";

        var paramWidget = (AtkUnitBase*)Plugin.GameGui.GetAddonByName(addon, 1);

        for (var i = 0; i < paramWidget->UldManager.NodeListCount; i++)
        {
            if (paramWidget->UldManager.NodeList[i] == null) continue;
            if (paramWidget->UldManager.NodeList[i]->NodeId == EnochainTimerNode)
            {
                textNode = (AtkTextNode*)paramWidget->UldManager.NodeList[i];
                /*if (reset)
                {
                    paramWidget->UldManager.NodeList[i]->ToggleVisibility(false);
                    continue;
                }*/

                break;
            }
        }

        if (textNode == null)
        {
            var newTextNode = (AtkTextNode*)IMemorySpace.GetUISpace()->Malloc((ulong)sizeof(AtkTextNode), 8);
            if (newTextNode != null)
            {
                var lastNode = paramWidget->RootNode;
                if (lastNode == null) return;

                IMemorySpace.Memset(newTextNode, 0, (ulong)sizeof(AtkTextNode));
                newTextNode->Ctor();
                textNode = newTextNode;

                newTextNode->AtkResNode.Type = NodeType.Text;
                newTextNode->AtkResNode.NodeFlags = NodeFlags.AnchorLeft | NodeFlags.AnchorTop;
                newTextNode->AtkResNode.DrawFlags = 0;
                newTextNode->AtkResNode.SetPositionShort(1, 1);
                newTextNode->AtkResNode.SetWidth(200);
                newTextNode->AtkResNode.SetHeight(14);

                newTextNode->LineSpacing = 24;
                newTextNode->AlignmentFontType = 0x14;
                newTextNode->FontSize = 12;
                newTextNode->TextFlags = (byte)(TextFlags.Edge);
                newTextNode->TextFlags2 = 0;

                newTextNode->AtkResNode.NodeId = EnochainTimerNode;

                newTextNode->AtkResNode.Color.A = 0xFF;
                newTextNode->AtkResNode.Color.R = 0xFF;
                newTextNode->AtkResNode.Color.G = 0xFF;
                newTextNode->AtkResNode.Color.B = 0xFF;

                if (lastNode->ChildNode != null)
                {
                    lastNode = lastNode->ChildNode;
                    while (lastNode->PrevSiblingNode != null)
                    {
                        lastNode = lastNode->PrevSiblingNode;
                    }

                    newTextNode->AtkResNode.NextSiblingNode = lastNode;
                    newTextNode->AtkResNode.ParentNode = paramWidget->RootNode;
                    lastNode->PrevSiblingNode = (AtkResNode*)newTextNode;
                }
                else
                {
                    lastNode->ChildNode = (AtkResNode*)newTextNode;
                    newTextNode->AtkResNode.ParentNode = lastNode;
                }

                textNode->TextColor.A = 0xFF;
                textNode->TextColor.R = 0xFF;
                textNode->TextColor.G = 0xFF;
                textNode->TextColor.B = 0xFF;

                textNode->EdgeColor.A = 0xFF;
                textNode->EdgeColor.R = 0xF0;
                textNode->EdgeColor.G = 0x8E;
                textNode->EdgeColor.B = 0x37;

                paramWidget->UldManager.UpdateDrawNodeList();
            }
        }
    }

    private void CastWithType(uint actionId, TimelineItemType type)
    {
        if ((type == TimelineItemType.Action || type == TimelineItemType.OffGCD) && DoesActinoResetEnochain(actionId))
        {
            if(actionId == transposeAction && combo.Timer <= 0)
            {
                return;
            }
            combo.Action = actionId;
            combo.Timer = enochainDuration;
        }
    }

    /*private void CastStarted(uint actionId)
    {
        combo.Action = actionId;
        combo.Timer = enochainDuration;
    }

    private void CastCancelled(uint actionId)
    {
        combo.Action = actionId;
        combo.Timer = enochainDuration;
    }*/

    private void SetupComboText()
    {
        textNode->AtkResNode.ToggleVisibility(true);
        textNode->X = (float)Configuration.TimerXCoord - 45;
        textNode->Y = (float)Configuration.TimerYCoord + 15;
        textNode->DrawFlags |= 0x1;
        textNode->AlignmentFontType = 0x14;
        textNode->TextFlags |= (byte)TextFlags.MultiLine;
        /*textNode->EdgeColor.R = (byte)(Config.EdgeColor.X * 0xFF);
        textNode->EdgeColor.G = (byte)(Config.EdgeColor.Y * 0xFF);
        textNode->EdgeColor.B = (byte)(Config.EdgeColor.Z * 0xFF);
        textNode->EdgeColor.A = (byte)(Config.EdgeColor.W * 0xFF);

        textNode->TextColor.R = (byte)(Config.Color.X * 0xFF);
        textNode->TextColor.G = (byte)(Config.Color.Y * 0xFF);
        textNode->TextColor.B = (byte)(Config.Color.Z * 0xFF);
        textNode->TextColor.A = (byte)(Config.Color.W * 0xFF);

        textNode->FontSize = (byte)(Config.FontSize);
        textNode->LineSpacing = (byte)(Config.FontSize);*/
        textNode->CharSpacing = 1;
        var comboTimer = (combo.Timer).ToString($"{("00")}{("")}");
        textNode->SetText($"{comboTimer}");
    }

    private void UpdateComboTimer()
    {
        if (combo.Timer <= 0)
        {
            combo.Action = 0;
            combo.Timer = 0;
        }
        else
        {
            combo.Timer -= (float)Plugin.Framework.UpdateDelta.TotalSeconds;
        }
    }

    public void Update()
    {
        if (textNode == null) SetupTextNode();

        if (textNode == null) return;

        UpdateComboTimer();
        SetupComboText();

        if(Configuration.InterruptCastsWhenTimerIsZero)
        {
            var player = Plugin.ClientState.LocalPlayer;
            if (combo.Timer <= 0 &&  player.IsCasting)
            {
                var actionInstance = ActionManager.Instance();
                if(actionInstance != null && CantCastWithoutEnochain(actionInstance->CastActionId))
                {
                    ActionManager.Instance()->UseAction(ActionType.GeneralAction, interruptActionId, 0);
                }
            }
        }
    }

    public enum TimelineItemType
    {
        Action = 0,
        CastStart = 1,
        CastCancel = 2,
        OffGCD = 3,
        AutoAttack = 4,
        Item = 5
    }

    private TimelineItemType TypeForAction(LuminaAction action)
    {
        // off gcd or sprint
        if (action.ActionCategory.RowId is 4 || action.RowId == 3)
        {
            return TimelineItemType.OffGCD;
        }

        if (action.ActionCategory.RowId is 1)
        {
            return TimelineItemType.AutoAttack;
        }

        return TimelineItemType.Action;
    }

    private TimelineItemType? TypeForID(uint id)
    {
        LuminaAction? action = actionSheet?.GetRowOrDefault(id);
        if (action != null)
        {
            return TypeForAction(action.Value);
        }

        /*LuminaItem? item = _itemSheet?.GetRowOrDefault(id) ?? _itemSheet?.GetRowOrDefault(id - 1000000);
        if (item != null)
        {
            return TimelineItemType.Item;
        }*/

        return TimelineItemType.Action;
    }

    private void OnActionUsed(uint sourceId, IntPtr sourceCharacter, IntPtr pos, IntPtr effectHeader,
            IntPtr effectArray, IntPtr effectTrail)
    {
        onActionUsedHook?.Original(sourceId, sourceCharacter, pos, effectHeader, effectArray, effectTrail);

        IPlayerCharacter? player = Plugin.ClientState.LocalPlayer;
        if (player == null || sourceId != player.GameObjectId) { return; }

        int actionId = Marshal.ReadInt32(effectHeader, 0x8);
        TimelineItemType? type = TypeForID((uint)actionId);
        if (!type.HasValue) { return; }

        CastWithType((uint)actionId, type.Value);
    }

    /*private void OnActorControl(uint entityId, uint type, uint buffID, uint direct, uint actionId, uint sourceId, uint arg4, uint arg5, ulong targetId, byte a10)
    {
        _onActorControlHook?.Original(entityId, type, buffID, direct, actionId, sourceId, arg4, arg5, targetId, a10);

        if (type != 15) { return; }

        IPlayerCharacter? player = Plugin.ClientState.LocalPlayer;
        if (player == null || entityId != player.GameObjectId) { return; }

        CastCancelled(actionId);
    }

    private void OnCast(uint sourceId, IntPtr ptr)
    {
        _onCastHook?.Original(sourceId, ptr);

        IPlayerCharacter? player = Plugin.ClientState.LocalPlayer;
        if (player == null || sourceId != player.GameObjectId) { return; }

        int value = Marshal.ReadInt16(ptr);
        uint actionId = value < 0 ? (uint)(value + 65536) : (uint)value;

        CastStarted(actionId);
    }*/
}
