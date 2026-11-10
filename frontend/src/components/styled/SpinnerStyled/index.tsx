"use client"

import React from "react"
import styled from "styled-components"

export function SpinnerStyled() {
    return (
        <StyledWrapper>
            <svg
                xmlns="http://www.w3.org/2000/svg"
                viewBox="0 0 128 128"
                height="128px"
                width="128px"
                className="pl"
            >
                <circle
                    strokeDashoffset="-376.4"
                    strokeDasharray="377 377"
                    strokeLinecap="round"
                    transform="rotate(-90,64,64)"
                    strokeWidth={8}
                    stroke="hsl(171, 100%, 35%)" /* teal */
                    fill="none"
                    r={60}
                    cy={64}
                    cx={64}
                    className="pl__ring1"
                />
                <circle
                    strokeDashoffset="-329.3"
                    strokeDasharray="329.9 329.9"
                    strokeLinecap="round"
                    transform="rotate(-90,64,64)"
                    strokeWidth={7}
                    stroke="hsl(185, 85%, 40%)" /* aqua blue */
                    fill="none"
                    r="52.5"
                    cy={64}
                    cx={64}
                    className="pl__ring2"
                />
                <circle
                    strokeDashoffset="-288.6"
                    strokeDasharray="289 289"
                    strokeLinecap="round"
                    transform="rotate(-90,64,64)"
                    strokeWidth={6}
                    stroke="hsl(190, 80%, 45%)" /* cyan */
                    fill="none"
                    r={46}
                    cy={64}
                    cx={64}
                    className="pl__ring3"
                />
                <circle
                    strokeDashoffset={-254}
                    strokeDasharray="254.5 254.5"
                    strokeLinecap="round"
                    transform="rotate(-90,64,64)"
                    strokeWidth={5}
                    stroke="hsl(155, 70%, 42%)" /* greenish teal */
                    fill="none"
                    r="40.5"
                    cy={64}
                    cx={64}
                    className="pl__ring4"
                />
                <circle
                    strokeDashoffset="-225.8"
                    strokeDasharray="226.2 226.2"
                    strokeLinecap="round"
                    transform="rotate(-90,64,64)"
                    strokeWidth={4}
                    stroke="hsl(150, 65%, 45%)" /* soft green */
                    fill="none"
                    r={36}
                    cy={64}
                    cx={64}
                    className="pl__ring5"
                />
                <circle
                    strokeDashoffset="-203.9"
                    strokeDasharray="204.2 204.2"
                    strokeLinecap="round"
                    transform="rotate(-90,64,64)"
                    strokeWidth={3}
                    stroke="hsl(165, 70%, 50%)" /* mint green */
                    fill="none"
                    r="32.5"
                    cy={64}
                    cx={64}
                    className="pl__ring6"
                />
            </svg>
        </StyledWrapper>
    )
}

const StyledWrapper = styled.div`
    display: flex;
    justify-content: center;
    align-items: center;

    .pl {
        width: 8em;
        height: 8em;
        filter: drop-shadow(0 0 8px rgba(64, 224, 208, 0.3));
    }

    .pl circle {
        transform-box: fill-box;
        transform-origin: 50% 50%;
    }

    .pl__ring1 {
        animation: ring1_ 4s 0s ease-in-out infinite;
    }

    .pl__ring2 {
        animation: ring2_ 4s 0.04s ease-in-out infinite;
    }

    .pl__ring3 {
        animation: ring3_ 4s 0.08s ease-in-out infinite;
    }

    .pl__ring4 {
        animation: ring4_ 4s 0.12s ease-in-out infinite;
    }

    .pl__ring5 {
        animation: ring5_ 4s 0.16s ease-in-out infinite;
    }

    .pl__ring6 {
        animation: ring6_ 4s 0.2s ease-in-out infinite;
    }

    /* Keyframes giữ nguyên animation gốc */
    @keyframes ring1_ {
        from {
            stroke-dashoffset: -376.2;
            transform: rotate(-0.25turn);
            animation-timing-function: ease-in;
        }
        23% {
            stroke-dashoffset: -94.24;
            transform: rotate(1turn);
            animation-timing-function: ease-out;
        }
        46%,
        50% {
            stroke-dashoffset: -376.2;
            transform: rotate(2.25turn);
            animation-timing-function: ease-in;
        }
        73% {
            stroke-dashoffset: -94.24;
            transform: rotate(3.5turn);
            animation-timing-function: ease-out;
        }
        96%,
        to {
            stroke-dashoffset: -376.2;
            transform: rotate(4.75turn);
        }
    }

    /* Các vòng sau giữ nguyên animation */
    @keyframes ring2_ {
        from {
            stroke-dashoffset: -329.2;
            transform: rotate(-0.25turn);
            animation-timing-function: ease-in;
        }
        23% {
            stroke-dashoffset: -82.46;
            transform: rotate(1turn);
            animation-timing-function: ease-out;
        }
        46%,
        50% {
            stroke-dashoffset: -329.2;
            transform: rotate(2.25turn);
            animation-timing-function: ease-in;
        }
        73% {
            stroke-dashoffset: -82.46;
            transform: rotate(3.5turn);
            animation-timing-function: ease-out;
        }
        96%,
        to {
            stroke-dashoffset: -329.2;
            transform: rotate(4.75turn);
        }
    }
    @keyframes ring3_ {
        from {
            stroke-dashoffset: -288.4;
            transform: rotate(-0.25turn);
            animation-timing-function: ease-in;
        }
        23% {
            stroke-dashoffset: -72.25;
            transform: rotate(1turn);
            animation-timing-function: ease-out;
        }
        46%,
        50% {
            stroke-dashoffset: -288.4;
            transform: rotate(2.25turn);
            animation-timing-function: ease-in;
        }
        73% {
            stroke-dashoffset: -72.25;
            transform: rotate(3.5turn);
            animation-timing-function: ease-out;
        }
        96%,
        to {
            stroke-dashoffset: -288.4;
            transform: rotate(4.75turn);
        }
    }
    @keyframes ring4_ {
        from {
            stroke-dashoffset: -253.9;
            transform: rotate(-0.25turn);
            animation-timing-function: ease-in;
        }
        23% {
            stroke-dashoffset: -63.61;
            transform: rotate(1turn);
            animation-timing-function: ease-out;
        }
        46%,
        50% {
            stroke-dashoffset: -253.9;
            transform: rotate(2.25turn);
            animation-timing-function: ease-in;
        }
        73% {
            stroke-dashoffset: -63.61;
            transform: rotate(3.5turn);
            animation-timing-function: ease-out;
        }
        96%,
        to {
            stroke-dashoffset: -253.9;
            transform: rotate(4.75turn);
        }
    }
    @keyframes ring5_ {
        from {
            stroke-dashoffset: -225.7;
            transform: rotate(-0.25turn);
            animation-timing-function: ease-in;
        }
        23% {
            stroke-dashoffset: -56.54;
            transform: rotate(1turn);
            animation-timing-function: ease-out;
        }
        46%,
        50% {
            stroke-dashoffset: -225.7;
            transform: rotate(2.25turn);
            animation-timing-function: ease-in;
        }
        73% {
            stroke-dashoffset: -56.54;
            transform: rotate(3.5turn);
            animation-timing-function: ease-out;
        }
        96%,
        to {
            stroke-dashoffset: -225.7;
            transform: rotate(4.75turn);
        }
    }
    @keyframes ring6_ {
        from {
            stroke-dashoffset: -203.7;
            transform: rotate(-0.25turn);
            animation-timing-function: ease-in;
        }
        23% {
            stroke-dashoffset: -51.05;
            transform: rotate(1turn);
            animation-timing-function: ease-out;
        }
        46%,
        50% {
            stroke-dashoffset: -203.7;
            transform: rotate(2.25turn);
            animation-timing-function: ease-in;
        }
        73% {
            stroke-dashoffset: -51.05;
            transform: rotate(3.5turn);
            animation-timing-function: ease-out;
        }
        96%,
        to {
            stroke-dashoffset: -203.7;
            transform: rotate(4.75turn);
        }
    }
`
