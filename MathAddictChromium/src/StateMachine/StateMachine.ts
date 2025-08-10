/*
TSX class for handling the state throughout the extension's active lifetime.
From the moment it pops up with the initialization button on the target website, to the moment the user
leaves and it gets closed off.

See Graph in MathAddictChromium/Misc/StateTransitions.png for info on state transitions.
 */

import {
    type MachineState,
    InactiveState,
    ActiveState,
} from "./States.ts";

type TransitionHandler = (source: MachineState) => void;
type TransitionConfig = {
    allowedFrom: MachineState[];
    correspondingHandler: TransitionHandler;
}
type TransitionMap = { [newState in MachineState]?: TransitionConfig };


/////////////////////////
// State Machine Class //
/////////////////////////

export class StateMachine {

    private currentState: MachineState;
    private prevState: MachineState;
    private transitions: TransitionMap;  // this map is reversed, mapping destination states to source states

    constructor() {
        this.currentState = InactiveState.PreInit;
        this.prevState = InactiveState.PreInit;
        this.transitions = this.generateTransitionMap()
    }

    // transitions between states if valid and returns if the transition was successful
    transition(newState: MachineState) : boolean {

        console.log(`Arriving at ${newState} from ${this.currentState}`);

        const conf : TransitionConfig = this.transitions[this.currentState]!;  // all possible keys are in the Map
        const validSources : MachineState[] = conf.allowedFrom;

        if (!validSources.includes(this.currentState)) {
            // DONT TRANSITION
            return false;
        }

        // otherwise, update state and call handler with source state
        this.prevState = this.currentState;
        this.currentState = newState;

        conf.correspondingHandler(this.prevState);
        return true;
    }

    // function for assigning transitions to the state machine as well as its handler functions
    generateTransitionMap() : TransitionMap {
        return {
            [InactiveState.PreInit]: {
                allowedFrom: [],
                correspondingHandler: ArriveAtPreInit
            },
            [InactiveState.DashBoard]: {
                allowedFrom: [],
                correspondingHandler: ArriveAtDashBoard
            },
            [InactiveState.Exit]: {
                allowedFrom: [],
                correspondingHandler: ArriveAtExit
            },
            [ActiveState.Question]: {
                allowedFrom: [],
                correspondingHandler: ArriveAtQuestion
            },
            [ActiveState.Answered]: {
                allowedFrom: [],
                correspondingHandler: ArriveAnswered
            },
            [ActiveState.SlotRoll]: {
                allowedFrom: [],
                correspondingHandler: ArriveAtSlotRoll
            },
            [ActiveState.FailAnswer]: {
                allowedFrom: [],
                correspondingHandler: ArriveAtFailAnswer
            },
            [ActiveState.PayOut]: {
                allowedFrom: [],
                correspondingHandler: ArriveAtPayOut
            }
        }
    }
}


///////////////////////
// Handler functions //   <== may need to be defined on the class that controls the items displayed
///////////////////////

function ArriveAtPreInit(source: MachineState): void {
    // show "Start" popup, cleanup any other in-page stuff
}

function ArriveAtDashBoard(source: MachineState): void {
    // show "Running" popup with extra details on player wallet & progress
}

function ArriveAtExit(source: MachineState): void {
    // shutdown everything, clean up objects and stop popup
}

function ArriveAtQuestion(source: MachineState): void {
    // show slots machine on the side waiting to get activated
    // popup with extra details on player wallet & progress visible
}

function ArriveAnswered(source: MachineState): void {
    // show slots machine on the side waiting to get activated
    // popup with extra details on player wallet & progress visible
    // forward result of question captured through crawler onto the UI and trigger a spin
}

function ArriveAtSlotRoll(source: MachineState): void {
    // show slots machine on the side getting activated
    // popup with extra details on player wallet & progress visible (with possible updates)
}

function ArriveAtFailAnswer(source: MachineState): void {
    // show slots machine on the side NOT getting activated
    // popup with extra details on player wallet & progress visible (with possible updates)
    // go back to Question state by default
}

function ArriveAtPayOut(source: MachineState): void {
    // show slots machine on the side post reward
    // popup with extra details on player wallet & progress visible (with possible updates)
    // go back to Question state by default
}