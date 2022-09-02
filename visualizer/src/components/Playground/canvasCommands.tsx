import { Instruction, instructionToString, InstructionType } from "../../contest-logic/Instruction";
import { MouseEvent } from "react";
import { Block } from "../../contest-logic/Block";
import { Point } from "../../contest-logic/Point";
import { getMousePoint } from "./shared/helpers";
import { RGBA } from "../../contest-logic/Color";
import { Interpreter } from "../../contest-logic/Interpreter";

let prevPoint: Point | null = null;
let prevSelectedBlockId: string | undefined;

function getBlockByPoint(blocks: Map<string, Block>, point: Point) {
    return [...blocks.values()].find(block => point.isInside(block.bottomLeft, block.topRight));
}

function getNewBlocks(code: string, instructions: Instruction[]) {
    const interpreter = new Interpreter();
    const result = interpreter.run(`${code}\n${instructions.map(i => instructionToString(i)).join('\n')}`);

    return result.canvas.blocks;
}

function getCenterPoint(p1: Point, p2: Point) {
    const diffX = p2.px - p1.px;
    const diffY = p2.py - p1.py;

    return new Point([Math.trunc(p2.px - diffX / 2), Math.trunc(p2.py - diffY / 2)]);
}

export function getClickInstruction(
  canvasRef: any,
  event: MouseEvent<HTMLCanvasElement>,
  instrument: InstructionType,
  blocks: Map<string, Block>,
  color: RGBA,
  code: string,
): Instruction | Instruction[] | undefined {
  if (!blocks) {
  }
  const point = getMousePoint(canvasRef.current, event);
  const currentBlock = getBlockByPoint(blocks, point);

  switch (instrument) {
    case InstructionType.HorizontalCutInstructionType: {
      return {
        // @ts-ignore
        blockId: currentBlock.id,
        typ: InstructionType.HorizontalCutInstructionType,
        lineNumber: point.py,
      } as Instruction;
    }
    case InstructionType.VerticalCutInstructionType: {
      return {
        // @ts-ignore
        blockId: currentBlock.id,
        typ: InstructionType.VerticalCutInstructionType,
        lineNumber: point.px,
      } as Instruction;
    }
    case InstructionType.PointCutInstructionType: {
      return {
        // @ts-ignore
        blockId: currentBlock.id,
        typ: InstructionType.PointCutInstructionType,
        point,
      } as Instruction;
    }
    case InstructionType.ColorInstructionType: {
      return {
        typ: InstructionType.ColorInstructionType,
        // @ts-ignore
        blockId: currentBlock.id,
        color,
      } as Instruction;
    }
    case InstructionType.SwapInstructionType: {
      if (!prevSelectedBlockId) {
        // @ts-ignore
        prevSelectedBlockId = currentBlock.id;
        return;
      }

      const res = {
        typ: InstructionType.SwapInstructionType,
        blockId1: prevSelectedBlockId,
        // @ts-ignore
        blockId2: currentBlock.id,
      } as Instruction;
      prevSelectedBlockId = undefined;
      return res;
    }
    case InstructionType.MergeInstructionType: {
      if (!prevSelectedBlockId) {
        // @ts-ignore
        prevSelectedBlockId = currentBlock.id;
        return;
      }

      const res = {
        typ: InstructionType.MergeInstructionType,
        blockId1: prevSelectedBlockId,
        // @ts-ignore
        blockId2: currentBlock.id,
      } as Instruction;
      prevSelectedBlockId = undefined;
      return res;
    }

    case InstructionType.Rectangle: {
      if (!prevPoint || !prevSelectedBlockId) {
        prevPoint = point;
        // @ts-ignore
        prevSelectedBlockId = currentBlock.id;
        return;
      }

      const instructions: Instruction[] = [];
      const firstCut: Instruction = {
        typ: InstructionType.PointCutInstructionType,
        blockId: prevSelectedBlockId,
        point: prevPoint,
      };
      instructions.push(firstCut)
      const blocks1 = getNewBlocks(code, instructions);
      const block1 = getBlockByPoint(blocks1, point);
      const secondCut: Instruction = {
        typ: InstructionType.PointCutInstructionType,
        blockId: block1!.id,
        point,
      };
      instructions.push(secondCut)
      const blocks2 = getNewBlocks(code, instructions);
      const block2 = getBlockByPoint(blocks2, getCenterPoint(prevPoint, point));
      const colorMove: Instruction = {
        typ: InstructionType.ColorInstructionType,
        color,
        blockId: block2!.id
      }
      instructions.push(colorMove);


      prevPoint = null;
      prevSelectedBlockId = undefined;
      return instructions;
    }
  }
}
