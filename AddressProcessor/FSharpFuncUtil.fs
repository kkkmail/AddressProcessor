namespace Softellect.AddressProcessor

    // https://gist.github.com/chamook/48a5481ec7428a29a8fc#file-funchelper-fs
    open System.Runtime.CompilerServices

    [<Extension>]
    type public FSharpFuncUtil =

        [<Extension>]
        static member ToFSharpFunc<'a> (func:System.Func<'a>) = fun () -> func.Invoke()

        [<Extension>]
        static member ToFSharpFunc<'a,'b> (func:System.Converter<'a,'b>) = fun x -> func.Invoke(x)

        [<Extension>]
        static member ToFSharpFunc<'a> (func:System.Action<'a>) = fun x -> func.Invoke(x)

        [<Extension>]
        static member ToFSharpFunc<'a,'b> (func:System.Action<'a,'b>) = fun x -> func.Invoke(x)

        [<Extension>]
        static member ToFSharpFunc<'a,'b> (func:System.Func<'a,'b>) = fun x -> func.Invoke(x)

        [<Extension>]
        static member ToFSharpFunc<'a,'b,'c> (func:System.Func<'a,'b,'c>) = fun x y -> func.Invoke(x,y)

        [<Extension>]
        static member ToFSharpFunc<'a,'b,'c,'d> (func:System.Func<'a,'b,'c,'d>) = fun x y z -> func.Invoke(x,y,z)

        static member Create<'a> (func:System.Func<unit, 'a>) = FSharpFuncUtil.ToFSharpFunc func

        static member Create<'a,'b> (func:System.Func<'a,'b>) = FSharpFuncUtil.ToFSharpFunc func

        static member Create<'a,'b,'c> (func:System.Func<'a,'b,'c>) = FSharpFuncUtil.ToFSharpFunc func

        static member Create<'a,'b,'c,'d> (func:System.Func<'a,'b,'c,'d>) = FSharpFuncUtil.ToFSharpFunc func
