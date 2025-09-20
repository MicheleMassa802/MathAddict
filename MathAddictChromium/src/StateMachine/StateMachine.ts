/*
TSX class for handling the state throughout the extension's active lifetime.
From the moment it pops up with the initialization button on the target website, to the moment the user
leaves and it gets closed off.

See Graph in MathAddictChromium/Misc/StateTransitions.png for info on state transitions.
 */

import {ActiveState, InactiveState, type MachineState} from "./States.ts";

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
    transition(newState: MachineState, runHandler: boolean = true) : boolean {

        console.log(`Trying to move to ${newState} from ${this.currentState}`);

        const conf : TransitionConfig = this.transitions[newState]!;  // all possible keys are in the Map
        const validSources : MachineState[] = conf.allowedFrom;

        if (!validSources.includes(this.currentState)) {
            // DONT TRANSITION
            return false;
        }

        // otherwise, update state and call handler with source state
        this.prevState = this.currentState;
        this.currentState = newState;

        if (runHandler) {
            conf.correspondingHandler(this.prevState);
        }
        return true;
    }

    // reset function for testing purposes
    resetStateMachineState() : void {
        this.currentState = InactiveState.PreInit;
        this.prevState = InactiveState.PreInit;
    }

    // getters
    getState() : MachineState {
        return this.currentState;
    }

    getPreviousState() : MachineState {
        return this.prevState;
    }

    // function for assigning transitions to the state machine as well as its handler functions
    generateTransitionMap() : TransitionMap {
        return {
            [InactiveState.PreInit]: {
                allowedFrom: [InactiveState.DashBoard],
                correspondingHandler: ArriveAtPreInit
            },
            [InactiveState.DashBoard]: {
                allowedFrom: [InactiveState.PreInit, ActiveState.Question, ActiveState.Answered, ActiveState.SlotRoll, ActiveState.FailAnswer, ActiveState.PayOut],
                correspondingHandler: ArriveAtDashBoard
            },
            [InactiveState.Exit]: {
                allowedFrom: [InactiveState.PreInit, InactiveState.DashBoard, ActiveState.Question, ActiveState.Answered, ActiveState.SlotRoll, ActiveState.FailAnswer, ActiveState.PayOut],
                correspondingHandler: ArriveAtExit
            },
            [ActiveState.Question]: {
                allowedFrom: [InactiveState.DashBoard, ActiveState.PayOut, ActiveState.FailAnswer],
                correspondingHandler: ArriveAtQuestion
            },
            [ActiveState.Answered]: {
                allowedFrom: [ActiveState.Question],
                correspondingHandler: ArriveAnswered
            },
            [ActiveState.SlotRoll]: {
                allowedFrom: [ActiveState.Answered],
                correspondingHandler: ArriveAtSlotRoll
            },
            [ActiveState.FailAnswer]: {
                allowedFrom: [ActiveState.Answered],
                correspondingHandler: ArriveAtFailAnswer
            },
            [ActiveState.PayOut]: {
                allowedFrom: [ActiveState.SlotRoll],
                correspondingHandler: ArriveAtPayOut
            }
        }
    }
}


// NOTE THESE PARAMETERS ARE NOT OPTIONAL SO YOU WILL NEED TO REMOVE THE _ AT THE START TO HAVE THEM GO
// BACK TO NORMAL, FOR NOW I JUST WANT TO SKIP THE WARNING


///////////////////////
// Handler functions //   <== may need to be defined on the class that controls the items displayed
///////////////////////

function ArriveAtPreInit(_source: MachineState): void {
    // show "Start" popup, cleanup any other in-page stuff

}

function ArriveAtDashBoard(_source: MachineState): void {
    // show "Running" popup with extra details on player wallet & progress
}

function ArriveAtExit(_source: MachineState): void {
    // shutdown everything, clean up objects and stop popup
}

function ArriveAtQuestion(_source: MachineState): void {
    // show slots machine on the side waiting to get activated
    // popup with extra details on player wallet & progress visible
}

function ArriveAnswered(_source: MachineState): void {
    // show slots machine on the side waiting to get activated
    // popup with extra details on player wallet & progress visible
    // forward result of question captured through crawler onto the UI and trigger a spin
}

function ArriveAtSlotRoll(_source: MachineState): void {
    // show slots machine on the side getting activated
    // popup with extra details on player wallet & progress visible (with possible updates)
}

function ArriveAtFailAnswer(_source: MachineState): void {
    // show slots machine on the side NOT getting activated
    // popup with extra details on player wallet & progress visible (with possible updates)
    // go back to Question state by default
}

function ArriveAtPayOut(_source: MachineState): void {
    // show slots machine on the side post reward
    // popup with extra details on player wallet & progress visible (with possible updates)
    // go back to Question state by default
}