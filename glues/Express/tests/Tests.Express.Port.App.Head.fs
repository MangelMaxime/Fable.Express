module Tests.Express.Port.App.Head

open Mocha
open Glutinum.ExpressServeStaticCore
open Glutinum.Express
open Fable.Core.JsInterop

describe "HEAD" (fun _ ->

    itAsync "should default to GET" (fun d ->
        let app = express.express ()

        app.get("/tobi", fun (req : Request) (res : Response) (next : NextFunction) ->
            res.send("tobi")
        )

        request(app)
            .head("/tobi")
            .expect(200, d)
            |> ignore
    )

    itAsync "should output the same headers as GET requests" (fun d ->
        let app = express.express ()

        app.get("/tobi", fun req (res : Response) next ->
            res.send("tobi")
        )

        request(app)
            .get("/tobi")
            .expect(
                200,
               (fun err res ->
                    if err.IsSome then
                        d err

                    let headers = res.headers

                    request(app)
                        .get("/tobi")
                        .expect(
                            200,
                            (fun err res ->
                                if err.IsSome then
                                    d err

                                jsDelete headers.Value?date
                                jsDelete res.headers.Value?date

                                Assert.deepStrictEqual(res.headers, headers)
                                d()
                            )
                        )
                        |> ignore
                )
            )
            |> ignore
    )

)

describe "app.head()" (fun _ ->

    itAsync "should override" (fun d ->

        let app = express.express ()
        let mutable called = false

        app.head("/tobi", fun req (res : Response<_,_>) next ->
            called <- true
            res.``end``()
        )

        app.get("/tobi", fun req (res : Response) next ->
            Assert.fail("should not call GET")
            res.send("tobi")
        )

        request(app)
            .head("/tobi")
            .expect(
                200,
                (fun err _ ->
                    Assert.strictEqual(called, true)
                    d ()
                )
            )
            |> ignore

    )

)
