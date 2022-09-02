import { useRef, useState } from "react";
import { Canvas } from "../../contest-logic/Canvas";
import { RGBA } from "../../contest-logic/Color";
import { Interpreter, InterpreterResult } from "../../contest-logic/Interpreter";
import { instructionToString, InstructionType } from "../../contest-logic/Instruction";
import { Painter } from "../../contest-logic/Painter";
import { RandomInstructionGenerator } from "../../contest-logic/RandomInstructionGenerator";
import { CommandsPanel } from "./commandPanel";

function getMousePos(canvas: any, event: any) {
  var rect = canvas.getBoundingClientRect();
  return {
    x: event.clientX - rect.left,
    y: event.clientY - rect.top,
  };
}
import { Point } from "../../contest-logic/Point";

export const Playground = (): JSX.Element => {
  const [width, setWidth] = useState(400);
  const [height, setHeight] = useState(400);
  const [cost, setCost] = useState(0);
  const [result, setResut] = useState();

  const [playgroundCode, setPlaygroundCode] = useState("");
  const [paintedCanvas, setPaintedCanvas] = useState(
    new Canvas(width, height, new RGBA([255, 255, 255, 255]))
  );
  const [instrument, setInstrument] = useState<InstructionType>(InstructionType.NopInstructionType);
  const [interpretedResult, setInterpreterResult] = useState<InterpreterResult | null>(null);
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const handlePlaygroundCode = (e: any) => {
    setPlaygroundCode(e.target.value as string);
  };
  const handleClickGenerateInstruction = () => {
    const interpreter = new Interpreter();
    const result = interpreter.run(playgroundCode);

    const instruction = RandomInstructionGenerator.generateRandomInstruction(result.canvas);
    setPlaygroundCode(`${playgroundCode}\n${instructionToString(instruction)}`);
  };

  const clearCanvas = () => {
    const canvas = canvasRef.current!;
    const context = canvas.getContext("2d")!;

    canvas.width = width;
    canvas.height = height;
    const imgData = context.getImageData(0, 0, canvas.width, canvas.height);
    imgData.data.forEach((value, index) => {
      imgData.data[index] = 255;
    });
  };

  const handleClickRenderCanvas = () => {
    clearCanvas();

    const interpreter = new Interpreter();
    const result = interpreter.run(playgroundCode);
    setInterpreterResult(result);

    const painter = new Painter();
    const renderedData = painter.draw(result.canvas);
    const canvas = canvasRef.current!;
    const context = canvas.getContext("2d")!;

    console.log(result.canvas.blocks);
    setCost(result.cost);

    canvas.width = result.canvas.width;
    canvas.height = result.canvas.height;
    const imgData = context.getImageData(0, 0, canvas.width, canvas.height);
    renderedData.forEach((pixel: RGBA, index: number) => {
      imgData.data[index * 4] = pixel.r;
      imgData.data[index * 4 + 1] = pixel.g;
      imgData.data[index * 4 + 2] = pixel.b;
      imgData.data[index * 4 + 3] = pixel.a;
    });
    context.putImageData(imgData, 0, 0);
  };
  const handleReset = () => {
    setPlaygroundCode("");
    clearCanvas();
    setInterpreterResult(null);
  };
  const drawBlocks = () => {
    const context = canvasRef.current!.getContext("2d")!;
    if (!interpretedResult) return;
    const canvas = interpretedResult.canvas;
    const blocks = canvas.blocks;
    context.font = "12px sans-serif";
    context.strokeStyle = "rgba(0, 0, 0, 0.25)";
    for (const [id, block] of blocks) {
      const frameTopLeft = new Point([block.bottomLeft.px, canvas.height - block.topRight.py]);
      const frameBottomRight = new Point([block.topRight.px, canvas.height - block.bottomLeft.py]);
      const sizeX = frameBottomRight.px - frameTopLeft.px;
      const sizeY = frameBottomRight.py - frameTopLeft.py;
      context.strokeRect(frameTopLeft.px, frameTopLeft.py, sizeX, sizeY);
    }
  };
  const onCanvasClick = (event: any) => {
    console.log(getMousePos(canvasRef.current, event));
  };

  return (
    <div
      style={{
        display: "flex",
        maxWidth: "100vw",
        gap: "20px",
        marginTop: 10,
      }}
    >
      <div>
        <div>
          <button onClick={handleClickGenerateInstruction}>Generate Instruction</button>
          <button onClick={handleClickRenderCanvas}>Render Canvas</button>
          <button onClick={handleReset}>Reset</button>
          <button onClick={drawBlocks}>Draw borders</button>
        </div>
        <div>
          <div>
            <label>
              width
              <br />
              <input
                type="text"
                value={width}
                onChange={(event) => setWidth(Number(event.target.value))}
              />
            </label>
            <br />
            <label>
              height
              <br />
              <input
                type="text"
                value={height}
                onChange={(event) => setHeight(Number(event.target.value))}
              />
            </label>
            <br />
            <label>
              code
              <br />
              <textarea
                style={{
                  width: "500px",
                  height: "400px",
                }}
                placeholder="Code to be submitted"
                value={playgroundCode}
                onChange={handlePlaygroundCode}
              />
            </label>
          </div>
        </div>
      </div>
      <div
        style={{
          flexShrink: 0,
          display: "flex",
          flexDirection: "column",
          gap: 10,
        }}
      >
        <canvas
          style={{ outline: "1px solid black" }}
          width={width}
          height={height}
          ref={canvasRef}
          onClick={onCanvasClick}
        />
        Cost: {cost}
      </div>
      <CommandsPanel instrument={instrument} setInstrument={setInstrument} />
    </div>
  );
};
