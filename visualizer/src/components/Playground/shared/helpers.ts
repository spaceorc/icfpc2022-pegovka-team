import React from "react";
import { Point } from "../../../contest-logic/Point";

export function getMousePos(
  canvas: HTMLCanvasElement | null,
  event: React.MouseEvent<HTMLCanvasElement>
) {
  if (!canvas) return new Point([-1, -1]);
  const rect = canvas.getBoundingClientRect();
  return new Point([
    Math.trunc(event.clientX - rect.left),
    Math.trunc(rect.height - (event.clientY - rect.top)),
  ]);
}
