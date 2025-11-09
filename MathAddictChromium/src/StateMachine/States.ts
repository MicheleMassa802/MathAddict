export enum InactiveState {
    PreInit = "Pre-init",
    DashBoard = "DashBoard",
    Exit = "Exit"
}

export enum ActiveState {
    Question = "Question",
    Answered = "Answered",
    SlotRoll = "SlotRoll",
    FailAnswer = "FailAnswer",
    PayOut = "PayOut"
}

export type MachineState = InactiveState | ActiveState;