namespace Softellect.AddressProcessor

module AddressDataConfiguration =

    [<Literal>]
    let AddressDataCommandTimeOut = 3600


    [<Literal>]
    let AddressTblName = "EFAddresses"


    // Hint value of max row number in Melissa Data.
    let maxRecId = 1_000_000_000


    let maxAllowedPrintableErrCount = 10_000


    let maxAllowedErrCount = 10_000_000


    [<Literal>]
    let ZipCodesZip = __SOURCE_DIRECTORY__ + @"\..\Data\ZipCodes.zip"


    [<Literal>]
    let ZipCodesCsv = "ZipCodes.csv"
