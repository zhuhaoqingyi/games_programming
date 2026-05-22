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
        ShipAssembly,
        BasicBoard,
        ReinforcedBoard,
        AdvancedBoard
    }

    public enum BoardType
    {
        None,
        BasicBoard,
        ReinforcedBoard,
        AdvancedBoard
    }

    public enum BuildingCategory
    {
        Core,
        Power,
        Production,
        Logistics,
        Storage,
        Special,
        Board
    }

    public enum BuildingStatus
    {
        Placed,
        Active,
        Inactive,
        Disabled
    }

    public enum BuildDirection
    {
        North,
        South,
        East,
        West
    }
}