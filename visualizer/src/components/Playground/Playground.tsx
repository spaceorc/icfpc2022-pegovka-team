import { useRef, useState } from "react";
import { Canvas } from "../../contest-logic/Canvas";
import { RGBA } from "../../contest-logic/Color";
import { Interpreter } from "../../contest-logic/Interpreter";
import { instructionToString, InstructionType } from "../../contest-logic/Instruction";
import { Painter } from "../../contest-logic/Painter";
import { RandomInstructionGenerator } from "../../contest-logic/RandomInstructionGenerator";
import { CommandsPanel } from "./commandPanel";

export const Playground = (): JSX.Element => {
  const [width, setWidth] = useState(400);
  const [height, setHeight] = useState(400);

  const [playgroundCode, setPlaygroundCode] = useState("");
  const [paintedCanvas, setPaintedCanvas] = useState(
    new Canvas(width, height, new RGBA([255, 255, 255, 255]))
  );
  const [instrument, setInstrument] = useState<InstructionType>(InstructionType.NopInstructionType);
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

    const painter = new Painter();
    const renderedData = painter.draw(result.canvas);
    const canvas = canvasRef.current!;
    const context = canvas.getContext("2d")!;

    console.log(result.canvas.blocks);

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
  };

  return (
    <div
      style={{
        display: "flex",
        maxWidth: "100vw",
        gap: "20px",
      }}
    >
      <div>
        <div>
          <button onClick={handleClickGenerateInstruction}>Generate Instruction</button>
          <button onClick={handleClickRenderCanvas}>Render Canvas</button>
          <button onClick={handleReset}>Reset</button>
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
        }}
      >
        <canvas
          style={{ border: "1px solid black" }}
          width={width}
          height={height}
          ref={canvasRef}
        />
      </div>
      <CommandsPanel instrument={instrument} setInstrument={setInstrument} />
    </div>
  );
};
