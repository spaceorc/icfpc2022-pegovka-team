import {Instruction, InstructionType, LineNumber} from "../../contest-logic/Instruction";
import { MouseEvent } from "react";
import {Block} from "../../contest-logic/Block";
import {Point} from "../../contest-logic/Point";
import {getMousePos} from "./shared/helpers";



export function getClickInstruction(canvasRef: any, event: MouseEvent<HTMLCanvasElement>,
                              instrument: InstructionType, blocks: Map<string, Block>): Instruction|undefined {
    const position = getMousePos(canvasRef.current, event);
    let currentBlock: Block;
    for (const block of blocks.values()){
        if (position.isInside(block.bottomLeft, block.topRight)){
            currentBlock = block;
            break;
        }
    }
    console.log(position)

    switch (instrument){
        case InstructionType.HorizontalCutInstructionType: {
            return {
                // @ts-ignore
                blockId: currentBlock.id,
                typ: InstructionType.HorizontalCutInstructionType,
                lineNumber: position.py,
            } as Instruction;
        }
        case InstructionType.VerticalCutInstructionType: {
            return {
                // @ts-ignore
                blockId: currentBlock.id,
                typ: InstructionType.VerticalCutInstructionType,
                lineNumber: position.px,
            } as Instruction;
        }
        case InstructionType.PointCutInstructionType: {
            return {
                // @ts-ignore
                blockId: currentBlock.id,
                typ: InstructionType.PointCutInstructionType,
                point: position,
            } as Instruction;
        }

    }
}
