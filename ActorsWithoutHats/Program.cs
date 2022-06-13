using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Synthesis;

namespace ActorsWithoutHats
{
    public class Program
    {
        private readonly ILoadOrder<IModListing<IFallout4ModGetter>> LoadOrder;

        private readonly IFallout4Mod PatchMod;

        private static readonly BipedObjectFlag coveringFlags = BipedObjectFlag.HairLong |
                BipedObjectFlag.FaceGenHead |
                BipedObjectFlag.Eyes |
                BipedObjectFlag.Beard |
                BipedObjectFlag.Mouth |
                BipedObjectFlag.Scalp;

        private static readonly BipedObjectFlag skipFlags = 
            BipedObjectFlag.Body | // eg. hazmat suit
            BipedObjectFlag.TorsoUnderArmor; // torso armor with integrated helmet, eg. "Super Mutant Bearskin Outfit" from Far Harbor

        private static readonly HashSet<IFormLinkGetter<IKeywordGetter>> skipKeywords = new()
        {
            // It is not possible to have an invisible Power Armor Helmet, as power armor helmet detection uses one of the slots we would need to remove.
            // TODO Fallout4.Keyword.ArmorTypePower
            ModKey.FromNameAndExtension("Fallout4.esm").MakeFormKey(0x04d8a1).AsLinkGetter<IKeywordGetter>()
        };

        private static readonly BipedObjectFlag headFlag = BipedObjectFlag.HairTop;

        private static readonly IFormLinkGetter<IRaceGetter> humanRace = ModKey.FromNameAndExtension("Fallout4.esm").MakeFormKey(0x013746).AsLinkGetter<IRaceGetter>();

        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<IFallout4Mod, IFallout4ModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.Fallout4, "Actors Without Hats.esp")
                .Run(args);
        }

        private static void RunPatch(IPatcherState<IFallout4Mod, IFallout4ModGetter> state) => new Program(state.LoadOrder, state.PatchMod).Run();

        private Program(
            ILoadOrder<IModListing<IFallout4ModGetter>> loadOrder,
            IFallout4Mod patchMod)
        {
            LoadOrder = loadOrder;
            PatchMod = patchMod;
        }

        private void Run()
        {
            var removeFlags = ~coveringFlags;

            // annoyingly, the workflow for Skyrim is almost identical, but the classes are sufficiently different that a combined program is larger than two separate ones.
            foreach (var armo in LoadOrder.PriorityOrder.Armor().WinningOverrides())
            {
                var bodt = armo.BipedBodyTemplate;
                if (bodt is null) continue;

                var firstPersonFlags = bodt.FirstPersonFlags;
                if (!firstPersonFlags.HasFlag(headFlag)) continue;

                if ((firstPersonFlags & skipFlags) != 0) continue;

                if ((firstPersonFlags & coveringFlags) == 0) continue;

                // TODO Fallout4.Race.HumanRace
                if (!armo.Race.Equals(humanRace))
                    continue;

                if (armo.Keywords is not null)
                    if (skipKeywords.Any(k => armo.Keywords.Contains(k)))
                        continue;

                var newArmo = PatchMod.Armors.GetOrAddAsOverride(armo);

                newArmo.BipedBodyTemplate!.FirstPersonFlags = firstPersonFlags & removeFlags;
                newArmo.Armatures.Clear();
            }
        }
    }
}
