using Mutagen.Bethesda;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;

namespace ActorsWithoutHats
{
    class SkyrimProgram : Program<
        BipedObjectFlag,
        ISkyrimMod,
        ISkyrimModGetter,
        IKeywordGetter,
        IArmor,
        IArmorGetter
        >
    {
        private static readonly BipedObjectFlag coveringFlags = BipedObjectFlag.Head |
                BipedObjectFlag.Hair |
                BipedObjectFlag.LongHair |
                BipedObjectFlag.Ears;

        private static readonly BipedObjectFlag skipFlags = (BipedObjectFlag)0;

        public static readonly HashSet<IFormLinkGetter<IKeywordGetter>> skipKeywords = new();

        public static readonly BipedObjectFlag headFlag = BipedObjectFlag.Circlet;

        public static readonly HashSet<IFormLinkGetter<IRaceGetter>> playerRaces = new() {
            Skyrim.Race.ArgonianRace,
            Skyrim.Race.ArgonianRaceVampire,
            Skyrim.Race.BretonRace,
            Skyrim.Race.BretonRaceVampire,
        };


        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "Actors Without Hats.esp")
                .Run(args);
        }

        public SkyrimProgram(ILoadOrder<IModListing<ISkyrimModGetter>> loadOrder, ISkyrimMod patchMod) : base(loadOrder, patchMod, coveringFlags, skipFlags, skipKeywords, headFlag) { }

        private static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state) => new SkyrimProgram(state.LoadOrder, state.PatchMod).Run();

        protected override IEnumerable<IArmorGetter> armors(ILoadOrder<IModListing<ISkyrimModGetter>> loadOrder) => loadOrder.PriorityOrder.Armor().WinningOverrides();

        protected override bool TryGetFirstPersonFlags(IArmorGetter armo, out BipedObjectFlag flags)
        {
            if (armo.BodyTemplate is null)
            {
                flags = 0;
                return false;
            }
            flags = armo.BodyTemplate.FirstPersonFlags;
            return true;
        }

        protected override bool raceFilter(IArmorGetter armo) => playerRaces.Contains(armo.Race);
    }
}
