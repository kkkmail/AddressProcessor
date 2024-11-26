namespace Softellect.AddressProcessor

// Generated, do not modify
// See StreetAbbr__<YYYYMMDD>.xlsx
type Direction =
    | North
    | East
    | South
    | West
    | Northeast
    | Southeast
    | Southwest
    | Northwest
    | Us
with
    static member all =
        [|
            ( Direction.North, "NORTH" )
            ( Direction.East, "EAST" )
            ( Direction.South, "SOUTH" )
            ( Direction.West, "WEST" )
            ( Direction.Northeast, "NORTHEAST" )
            ( Direction.Southeast, "SOUTHEAST" )
            ( Direction.Southwest, "SOUTHWEST" )
            ( Direction.Northwest, "NORTHWEST" )
            ( Direction.Us, "US" )
        |]


type DirectionAbbr =
    {
        common : string
        standard : string
        canBeSuffix : bool
        canBeMid : bool
        caseValue : Direction
    }
with
    static member all : DirectionAbbr[] =
        [|
            { common = "NORTH"; standard = "N"; canBeSuffix = true; canBeMid = false; caseValue = Direction.North }
            { common = "EAST"; standard = "E"; canBeSuffix = true; canBeMid = false; caseValue = Direction.East }
            { common = "SOUTH"; standard = "S"; canBeSuffix = true; canBeMid = false; caseValue = Direction.South }
            { common = "WEST"; standard = "W"; canBeSuffix = true; canBeMid = false; caseValue = Direction.West }
            { common = "NORTHEAST"; standard = "NE"; canBeSuffix = true; canBeMid = false; caseValue = Direction.Northeast }
            { common = "SOUTHEAST"; standard = "SE"; canBeSuffix = true; canBeMid = false; caseValue = Direction.Southeast }
            { common = "SOUTHWEST"; standard = "SW"; canBeSuffix = true; canBeMid = false; caseValue = Direction.Southwest }
            { common = "NORTHWEST"; standard = "NW"; canBeSuffix = true; canBeMid = false; caseValue = Direction.Northwest }
            // For street names like "US HWY 66"
            { common = "US"; standard = "US"; canBeSuffix = false; canBeMid = true; caseValue = Direction.Us }
        |]


type DirectionFactory () = inherit UnionFactory<Direction, string> (DirectionAbbr.all |> Array.map (fun e -> e.caseValue, e.standard))
