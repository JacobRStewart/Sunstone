/* eslint-disable @typescript-eslint/no-unused-vars */
export interface CounterState {
    
    data: number;
    title: string;

}

const initialState: CounterState = {
    data: 42,
    title: "YARC: Yet another redux counter"
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export default function counterReducer(state = initialState, action: any) {

    return state;
}