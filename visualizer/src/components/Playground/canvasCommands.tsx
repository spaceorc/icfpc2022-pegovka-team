import { Instruction, InstructionType } from "../../contest-logic/Instruction";
import { MouseEvent } from "react";
import { Block } from "../../contest-logic/Block";
import { Point } from "../../contest-logic/Point";
import { getMousePos } from "./shared/helpers";
import { RGBA } from "../../contest-logic/Color";

export function getClickInstruction(
  canvasRef: any,
  event: MouseEvent<HTMLCanvasElement>,
  instrument: InstructionType,
  blocks: Map<string, Block>,
  color: RGBA
): Instruction | undefined {
  if (!blocks) {
  }
  const position = getMousePos(canvasRef.current, event);
  let currentBlock: Block;
  for (const block of blocks.values()) {
    if (position.isInside(block.bottomLeft, block.topRight)) {
      currentBlock = block;
      break;
    }
  }
  console.log(position);

  switch (instrument) {
    case InstructionType.HorizontalCutInstructionType: {
      return {
        // @ts-ignore
        blockId: currentBlock.id,
        typ: InstructionType.HorizontalCutInstructionType,
        lineNumber: Math.trunc(position.py),
      } as Instruction;
    }
    case InstructionType.VerticalCutInstructionType: {
      return {
        // @ts-ignore
        blockId: currentBlock.id,
        typ: InstructionType.VerticalCutInstructionType,
        lineNumber: Math.trunc(position.px),
      } as Instruction;
    }
    case InstructionType.PointCutInstructionType: {
      return {
        // @ts-ignore
        blockId: currentBlock.id,
        typ: InstructionType.PointCutInstructionType,
        point: new Point([Math.trunc(position.px), Math.trunc(position.py)]),
      } as Instruction;
    }
    case InstructionType.ColorInstructionType: {
      return {
        typ: InstructionType.ColorInstructionType,
        // @ts-ignore
        blockId: currentBlock.id,
        color,
      };
    }
  }
}