// Learn more about F# at http://fsharp.org

open NCoreUtils


[<EntryPoint>]
let main argv =

    let inline tryDiv i =
        match i with
        | 0 -> None
        | _ -> Some i

    Seq.tryHead argv
    >>= tryInt
    >>= tryDiv
    |> Option.getOrDef 1
    |> printf "%A"

    0 // return an integer exit code
