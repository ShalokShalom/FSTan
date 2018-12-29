﻿module rec FSTan.Control.State
open FSTan.HKT
open FSTan.Monad

type state<'s, 'a> = hkt<StateMonad<'s>, 'a>
and StateMonad<'s>() =
    inherit monad<StateMonad<'s>>() with
        override __.bind<'a, 'b> (m: state<'s, 'a>) (k: 'a -> state<'s, 'b>) =
            let m = State' <| fun s ->
                let a, s = runState m s
                runState (k a) s
            m :> _  

        override __.pure'<'a> (a: 'a) : state<'s, 'a> = 
            let m =  State' <| fun s -> (a, s)
            m :> _

and stateData<'s, 'a> = 
    | State' of ('s -> ('a * 's))
    interface state<'s, 'a>

[<GeneralizableValue>]
let runState<'s, 'a>(m: state<'s, 'a>): ('s -> 'a * 's) =
    let (State' f): stateData<'s, 'a> = m :?> _
    f

[<GeneralizableValue>]
let get<'s> : state<'s, 's> =  
    State'(fun s -> (s, s)) :> _

let put<'s, 'a> (s: 's) : state<'s, unit> = 
    State'(fun _ -> ((), s)) :> _

let State<'s, 'a> (f: 's -> ('a * 's)): state<'s, 'a> = 
    upcast (State' f)

let (|State|) (s: state<'s, 'a>) =
    let (State' f) = s :?> stateData<'s, 'a>
    in State f