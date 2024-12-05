namespace Softellect.AddressProcessorTests

module Primitives =

    /// F# does not play well with C# default parameters.
    /// The default value of "because" parameter in FluentAssertions is "".
    /// So, here we have to pass it to make the code compile.
    [<Literal>]
    let EmptyBecause = ""
