Remora.StateMachine
===================

Remora.StateMachine is a small library that can be used to build simple
[finite-state machines][1] which represent a system of distinct behavioural
states and the transitions between them.

Each state can be as complex or as simple as the user wants, and the library
does not make any particular distinctions between states and their behaviour.
The core purpose is to implement the driving machine itself, and leave the state
definitions up to the end user.

At its core, a state machine is made up of one or more types that implement the
`IState` interface. Potential transitions between states are declared by
implementing `ITransition<TState>`, and the initial state must implement `IInitialState`


[1]: https://en.wikipedia.org/wiki/Finite-state_machine
