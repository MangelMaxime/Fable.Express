[<AutoOpen>]
module rec Npm.Exports

open Fable.Core

type Npm.Types.IExports with
    [<Import("default", "supertest")>]
    member __.supertest with get () : Types.SuperTest.IExports = jsNative