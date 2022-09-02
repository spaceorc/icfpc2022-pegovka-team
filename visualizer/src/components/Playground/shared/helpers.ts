import React from "react";
import {Point} from "../../../contest-logic/Point";

export function getMousePos(canvas: HTMLCanvasElement | null, event: React.MouseEvent<HTMLCanvasElement>) {
    if (!canvas) return new Point([ -1, -1]);
    const rect = canvas.getBoundingClientRect();
    return new Point([event.clientX - rect.left, rect.height - (event.clientY - rect.top)]);
}
