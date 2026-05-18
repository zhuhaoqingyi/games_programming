namespace GameCore
{
    public enum ResourceType
    {
        None,
        SpaceOre,
        SpaceDebris,
        AlloyIngot,
        MechanicalPart,
        ElectronicComponent,
        AdvancedAlloy
    }

    public enum BuildingType
    {
        None,
        EmergencyShelter,
        MiningPlatform,
        NuclearReactor,
        SolarArray,
        StorageDock,
        FurnaceRefinery,
        PartAssembly,
        AdvancedFactory,
        ConveyorBelt,
        Sorter,
        ShipAssembly
    }

    public enum BuildingCategory
    {
        Core,
        Power,
        Production,
        Logistics,
        Storage,
        Special
    }

    public enum BuildingStatus
    {
        Placed,
        Active,
        Inactive,
        Disabled
    }
}