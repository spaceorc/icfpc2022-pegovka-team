import React from "react";
import { Block } from "../../../contest-logic/Block";
import { Point } from "../../../contest-logic/Point";

export function getMousePoint(
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

export function canMergeBlocks(block1: Block, block2: Block) {
  const bottomToTop =
    (block1.bottomLeft.py === block2.topRight.py || block1.topRight.py === block2.bottomLeft.py) &&
    block1.bottomLeft.px === block2.bottomLeft.px &&
    block1.topRight.px === block2.topRight.px;
  const leftToRight =
    (block1.bottomLeft.px === block2.topRight.px || block1.topRight.px === block2.bottomLeft.px) &&
    block1.bottomLeft.py === block2.bottomLeft.py &&
    block1.topRight.py === block2.topRight.py;
  return bottomToTop || leftToRight;
}
