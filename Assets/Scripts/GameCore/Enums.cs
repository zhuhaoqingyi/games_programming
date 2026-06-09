namespace GameCore
{
    public enum ResourceType
    {
        None,
        SpaceOre,
        MetalMaterial,
        BasicPart,
        AdvancedPart
    }

    public enum BuildingType
    {
        None,
        EmergencyShelter,
        MiningPlatform,
        Thruster,
        NuclearReactor,
        SolarArray,
        StorageDock,
        FurnaceRefinery,
        PartAssembly,
        AdvancedFactory,
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
        Storage,
        Propulsion,
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
        East,
        South,
        West,
        North
    }
}