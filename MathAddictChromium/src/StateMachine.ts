/*
TSX class for handling the state throughout the extension's active lifetime.
From the moment it pops up with the initialization button on the target website, to the moment the user
leaves and it's closed off.

See Graph in MathAddictChromium/Misc/StateTransitions.png for info on state transitions.
 */

type InactiveState = "Pre-init" | "DashBoard" | "Exit";
type ActiveState = "Question" | "Answered" | "SlotRoll" | "Fail" | "Payout";

// store current state

// have mapping of src->dst list pairings (directed graph)

// have a transition to function that checks curr and dst states and returns bool if possible and transitions