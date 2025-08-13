/*
Test class for the state machine.
 */

import {StateMachine} from "./StateMachine.ts";
import {ActiveState, InactiveState, type MachineState} from "./States.ts";

// pre-reqs for test to work: valid src->dst path, sm currently at src node!
function testCorrectTransition(sm: StateMachine, src: MachineState, dst: MachineState) : boolean {
    const res : boolean = sm.transition(dst) && (sm.getState() == dst) && (sm.getPreviousState() == src);
    console.log(`testCorrectTransition: ${res ? "PASSED" : "FAILED"}\n`);
    return res;
}

function testBadTransition(sm: StateMachine) : boolean {
    let res : boolean = sm.transition(InactiveState.Exit);
    if (!res) {
        console.log(`testBadTransition: Machine 'sm' not provided at correct source state!\n`);
        return false;
    }

    // attempt invalid exit -> pre-init transition
    res = res && !sm.transition(InactiveState.PreInit) && (sm.getState() != InactiveState.PreInit);
    console.log(`testBadTransition: ${res ? "PASSED" : "FAILED"}\n`);
    return res;
}

function testPreInitToDashBoardThroughFail(sm: StateMachine): boolean {
    let res: boolean = true;
    res = res && sm.transition(InactiveState.DashBoard, false);
    res = res && sm.transition(ActiveState.Question, false);
    res = res && sm.transition(ActiveState.Answered, false);
    res = res && sm.transition(ActiveState.FailAnswer, false);
    res = res && sm.transition(InactiveState.DashBoard, false);  // if res is false at one point, rest of calls get skipped

    res = res && (sm.getState() == InactiveState.DashBoard) && (sm.getPreviousState() == ActiveState.FailAnswer)
    console.log(`testPreInitToDashBoardThroughFail: ${res ? "PASSED" : "FAILED"}\n`);
    return res;
}

function testPreInitToExitThroughPayout(sm: StateMachine): boolean {
    let res: boolean = true;
    res = res && sm.transition(InactiveState.DashBoard, false);
    res = res && sm.transition(ActiveState.Question, false);
    res = res && sm.transition(ActiveState.Answered, false);
    res = res && sm.transition(ActiveState.SlotRoll, false);
    res = res && sm.transition(ActiveState.PayOut, false);
    res = res && sm.transition(InactiveState.DashBoard, false);
    res = res && sm.transition(InactiveState.Exit, false);  // if res is false at one point, rest of calls get skipped

    res = res && (sm.getState() == InactiveState.Exit) && (sm.getPreviousState() == InactiveState.DashBoard)
    console.log(`testPreInitToDashBoardThroughFail: ${res ? "PASSED" : "FAILED"}\n`);
    return res;
}


function runStateMachineTests() : void {
    let passedTests: boolean = true;
    const sm = new StateMachine(); // starts at pre-init

    // scheme each of the tests
    passedTests = testCorrectTransition(sm, InactiveState.PreInit, InactiveState.DashBoard) && passedTests;
    passedTests = testCorrectTransition(sm, InactiveState.DashBoard, InactiveState.Exit) && passedTests;
    sm.resetStateMachineState();

    passedTests = testBadTransition(sm)  && passedTests;
    sm.resetStateMachineState();

    passedTests = testPreInitToDashBoardThroughFail(sm) && passedTests;
    sm.resetStateMachineState();

    passedTests = testPreInitToExitThroughPayout(sm) && passedTests;
    sm.resetStateMachineState();

    console.log(`\nOverall testing results: ${passedTests ? "All Passed" : "Some Failed -- See Details Above"}`);
}

//////////
// Main //
//////////
runStateMachineTests();

