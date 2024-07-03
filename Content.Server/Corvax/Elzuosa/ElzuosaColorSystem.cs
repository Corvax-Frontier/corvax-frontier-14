using Content.Server.Humanoid;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Content.Server.Chat.Systems;
using Content.Shared.Light;
using Content.Shared.Light.Components;
using Content.Shared.Tools.Components;
using FastAccessors;
using Robust.Server.GameObjects;
using Robust.Shared.Network;

namespace Content.Server.Corvax.Elzuosa
{
    public sealed class ElzuosaColorSystem : EntitySystem
    {
        [Dependency] private readonly HumanoidAppearanceSystem _humanoidAppearance = default!;
        [Dependency] private readonly SharedRgbLightControllerSystem _rgbSystem = default!;
        [Dependency] private readonly SharedPointLightSystem _sharedPointLightSystem = default!;
        [Dependency] private readonly INetManager _netManager = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ElzuosaColorComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<ElzuosaColorComponent, InteractUsingEvent>(OnInteractUsing);
            //SubscribeLocalEvent<ElzuosaColorComponent, MapInitEvent>(OnInit);
        }

        private void OnMapInit(EntityUid uid, ElzuosaColorComponent comp, MapInitEvent args)
        {
            if (!HasComp<HumanoidAppearanceComponent>(uid))
                return;
            if (TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
            {
                var color = humanoid.SkinColor;
                _sharedPointLightSystem.SetColor(uid, color);
            }
        }
        /*private void OnInit(EntityUid uid, ElzuosaColorComponent comp, AfterAutoHandleStateEvent args)
        {
            if (!HasComp<HumanoidAppearanceComponent>(uid))
                return;
            if (TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
            {
                var color = humanoid.SkinColor;
                _sharedPointLightSystem.SetColor(uid, color);
            }
        }*/

        private void OnInteractUsing(EntityUid uid, ElzuosaColorComponent comp, InteractUsingEvent args)
        {
            if (args.Handled)
                return;

            if (!TryComp(args.Used, out ToolComponent? tool) || !tool.Qualities.ContainsAny("Pulsing"))
                return;

            args.Handled = true;
            comp.Hacked = !comp.Hacked;

            if (comp.Hacked)
            {
                var rgb = EnsureComp<RgbLightControllerComponent>(uid);
                _rgbSystem.SetCycleRate(uid, comp.CycleRate, rgb);
            }
            else
                RemComp<RgbLightControllerComponent>(uid);
        }
    }
}
