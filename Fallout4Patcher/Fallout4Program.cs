using ActorsWithoutHats;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Synthesis;

namespace Fallout4Patcher
{
    class Fallout4Program : Program<
        BipedObjectFlag,
        IFallout4Mod,
        IFallout4ModGetter,
        IKeywordGetter,
        IArmor,
        IArmorGetter
        >
    {
        private static readonly BipedObjectFlag coveringFlags = BipedObjectFlag.HairLong |
                BipedObjectFlag.FaceGenHead |
                BipedObjectFlag.Eyes |
                BipedObjectFlag.Beard |
                BipedObjectFlag.Mouth |
                BipedObjectFlag.Scalp;

        private static readonly BipedObjectFlag skipFlags = BipedObjectFlag.Body |
                BipedObjectFlag.TorsoUnderArmor;

        public static readonly HashSet<IFormLinkGetter<IKeywordGetter>> skipKeywords = new()
        {
            // TODO Fallout4.Keyword.ArmorTypePower
            ModKey.FromNameAndExtension("Fallout4.esm").MakeFormKey(0x4d84a1).AsLinkGetter<IKeywordGetter>()
        };

        public static readonly BipedObjectFlag headFlag = BipedObjectFlag.HairTop;

        // TODO Fallout4.Race.HumanRace
        public static readonly IFormLinkGetter<IRaceGetter> humanRace = ModKey.FromNameAndExtension("Fallout4.esm").MakeFormKey(0x013746).AsLinkGetter<IRaceGetter>();


        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<IFallout4Mod, IFallout4ModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.Fallout4, "Actors Without Hats.esp")
                .Run(args);
        }

        public Fallout4Program(ILoadOrder<IModListing<IFallout4ModGetter>> loadOrder, IFallout4Mod patchMod) : base(loadOrder, patchMod, coveringFlags, skipFlags, skipKeywords, headFlag) { }

        private static void RunPatch(IPatcherState<IFallout4Mod, IFallout4ModGetter> state) => new Fallout4Program(state.LoadOrder, state.PatchMod).Run();

        protected override IEnumerable<IArmorGetter> armors(ILoadOrder<IModListing<IFallout4ModGetter>> loadOrder) => loadOrder.PriorityOrder.Armor().WinningOverrides();

        protected override bool TryGetFirstPersonFlags(IArmorGetter armo, out BipedObjectFlag flags)
        {
            if (armo.BipedBodyTemplate is null)
            {
                flags = 0;
                return false;
            }
            flags = armo.BipedBodyTemplate.FirstPersonFlags;
            return true;
        }

        protected override bool raceFilter(IArmorGetter armo) => armo.Race.Equals(humanRace);
    }
}
