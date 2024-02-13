namespace Collections.Generic

type Stack<'t> = System.Collections.Generic.Stack<'t>

module Stack =

    let create<'t> () =
        let stack = new Stack<'t>()
        stack

    let push (value) (stack: Stack<'t>) =
        stack.Push(value)
        stack

    let pop (stack: Stack<'t>) =
        match stack.TryPop() with
        | true, value -> Some value
        | false, _ -> None

    let peek (stack: Stack<'t>) =
        match stack.TryPeek() with
        | true, value -> Some value
        | false, _ -> None

    let clear (stack: Stack<'t>) =
        stack.Clear()
        stack

    let count (stack: Stack<'t>) = stack.Count

    let toArray (stack: Stack<'t>) =
        stack.ToArray()

    let toList (stack: Stack<'t>) =
        stack.ToArray() |> List.ofArray
