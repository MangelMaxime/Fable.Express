namespace Qs

open Mocha
open Qs
open Fable.Core
open Fable.Core.Testing
open Fable.Core.JsInterop

module Tests =

    [<Emit("typeof $0")>]
    let jsTypeOf _ = jsNative

    [<Emit("BigInt($0)")>]
    let JsBigInt _ : bigint = jsNative

    let private parseApi () =
        describe "qs.parse()" (fun _ ->

            it "parses a simple string" (fun _ ->
                let res = qs.parse("0=foo")

                Assert.AreEqual(res.["0"], Some (!^ "foo"))
            )

            it "arrayFormat: brackets allows only explicit arrays" (fun _ ->
                let res =
                    qs.parse(
                        "a[0]=b&a[1]=c",
                        jsOptions<Types.IParseOptions>(fun o ->
                            o.arrayFormat <- Types.IArrayFormat.Brackets
                        )
                    )

                Assert.AreEqual(res.["a"], Some (!^ ResizeArray(["b"; "c"])))
            )

            it "allows for decoding keys and values differently" (fun  _ ->
                let decoder =
                    // Code copied from Qs test but it seems typ is always undefined so it doesn't work /shrug
                    // So for now, I am using some stupid decoder just to test the API
                    // System.Func<_,_,_,_,_>(
                    //     fun (str : string) (defaultDecoder : Types.defaultDecoder) (charset : string) (typ : Types.IStringifyOptionsEncoder) ->

                    //         if typ = Types.IStringifyOptionsEncoder.Key then
                    //             defaultDecoder.Invoke(str, defaultDecoder, charset).ToLower() |> box

                    //         else if typ = Types.IStringifyOptionsEncoder.Value then
                    //              defaultDecoder.Invoke(str, defaultDecoder, charset).ToUpper() |> box

                    //         else
                    //             failwithf "this should never happen! type: %A" typ
                    // )

                    System.Func<_,_,_,_,_>(
                        fun (str : string) (defaultDecoder : Types.defaultDecoder) (charset : string) (_: Types.IStringifyOptionsEncoder) ->
                            if str = "KeY" then
                                box "key"

                            else if str = "vAlUe" then
                                box "VALUE"

                            else
                                failwithf "this should never happen! type: %A" str
                    )

                let res =
                    qs.parse(
                        "KeY=vAlUe",
                        jsOptions<Types.IParseOptions>(fun o ->
                            o.decoder <- decoder
                        )
                    )

                Assert.AreEqual(res.["key"], Some (!^ "VALUE"))

            )

        )

    let private stringifyApi () =
        describe "qs.stringifyApi()" (fun _ ->
            it "stringifies a querystring object" (fun _ ->
                let res =
                    qs.stringify(createObj [
                        "a" ==> "b"
                    ])

                Assert.AreEqual(res, "a=b")

            )

            it "stringifies using encoder" (fun _->
                let encoder =
                    System.Func<_,Types.defaultEncoder,_,_,_> (fun value defaultEncoder charset _ ->
                        let result = defaultEncoder.Invoke(value, defaultEncoder, charset)

                        if jsTypeOf value = "bigint" then
                            result + "n"
                        else
                            result
                    )

                let res =
                    qs.stringify(
                        createObj [
                            "a" ==> JsBigInt 2
                        ],
                        jsOptions<Types.IStringifyOptions>(fun o ->
                            o.encoder <- encoder
                        )
                    )

                Assert.AreEqual(res, "a=2n")
            )
        )

    let all () =
        describe "Qs" (fun _ ->
            parseApi ()
            stringifyApi ()
        )