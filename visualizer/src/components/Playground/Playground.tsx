import { Canvas } from "../../contest-logic/Canvas";
import React, { useEffect, useRef, useState } from "react";
import { RGBA } from "../../contest-logic/Color";
import { Interpreter, InterpreterResult } from "../../contest-logic/Interpreter";
import { instructionToString, InstructionType } from "../../contest-logic/Instruction";
import { Painter } from "../../contest-logic/Painter";
import { RandomInstructionGenerator } from "../../contest-logic/RandomInstructionGenerator";
import { CommandsPanel } from "./commandPanel";

import { Point } from "../../contest-logic/Point";
import { Block } from "../../contest-logic/Block";
import { getClickInstruction } from "./canvasCommands";
import { getMousePos } from "./shared/helpers";
import { SimilarityChecker } from "../../contest-logic/SimilarityCheck";

const modules = import.meta.glob("../../../../problems/*.png", { as: "url", eager: true });

function getImageData(imgRef: HTMLImageElement) {
  const canvas = document.createElement("canvas");
  const context = canvas.getContext("2d");
  const height = (canvas.height = imgRef.naturalHeight || imgRef.offsetHeight || imgRef.height);
  const width = (canvas.width = imgRef.naturalWidth || imgRef.offsetWidth || imgRef.width);

  if (!context) {
    return null;
  }

  context.drawImage(imgRef, 0, 0);

  const data = context.getImageData(0, 0, width, height);

  return data.data;
}

export const Playground = (): JSX.Element => {
  const [width, setWidth] = useState(400);
  const [height, setHeight] = useState(400);
  const [expectedOpacity, _setExpectedOpacity] = useState(
    Number(sessionStorage.getItem("opacity")) ?? 0
  );
  const setExpectedOpacity = (opacity: number) => {
    sessionStorage.setItem("opacity", opacity.toString());
    _setExpectedOpacity(opacity);
  };
  const [exampleId, setExampleId] = useState(1);
  const [similarity, setSimilarity] = useState(0);
  const [oldTotal, setOldTotal] = useState(0);

  const [playgroundCode, _setPlaygroundCode] = useState(sessionStorage.getItem("code") ?? "");
  const setPlaygroundCode = (code: string) => {
    sessionStorage.setItem("code", code);
    _setPlaygroundCode(code);
  };
  const [instrument, setInstrument] = useState<InstructionType>(InstructionType.NopInstructionType);
  const [interpretedResult, setInterpreterResult] = useState<InterpreterResult>(
    new InterpreterResult(new Canvas(400, 400, new RGBA([255, 255, 255, 255])), 0)
  );

  const [color, setColor] = useState<RGBA>(new RGBA([0, 0, 0, 255]));
  const [prevSelectedBlockId, setPrevSelectedBlockId] = useState<string>()

  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const imgRef = useRef<HTMLImageElement | null>(null);
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

  const handleClickRenderCanvas = (code: string) => {
    setOldTotal(interpretedResult.cost + similarity);
    clearCanvas();

    const interpreter = new Interpreter();
    const result = interpreter.run(code);
    setInterpreterResult(result);

    console.log(result.canvas.blocks);

    const painter = new Painter();
    const renderedData = painter.draw(result.canvas);
    const canvas = canvasRef.current!;
    const context = canvas.getContext("2d")!;
console.log(result.canvas.blocks)
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
    drawBlocks(result);

    if (imgRef.current) {
      const expectedData = getImageData(imgRef.current)!;
      const expectedFrame = SimilarityChecker.bufferToFrame(expectedData);
      const actualFrame = SimilarityChecker.bufferToFrame(imgData.data);

      setSimilarity(SimilarityChecker.imageDiff(expectedFrame, actualFrame));
    }
  };
  const handleReset = () => {
    setPlaygroundCode("");
    clearCanvas();
    setInterpreterResult(
      new InterpreterResult(new Canvas(400, 400, new RGBA([255, 255, 255, 255])), 0)
    );
  };
  const [drawBorder, setDrawBorder] = useState(true);
  const drawBlocks = (interpretedResult: InterpreterResult) => {
    if (!drawBorder) return;
    const context = canvasRef.current!.getContext("2d")!;
    const canvas = interpretedResult.canvas;
    const blocks = canvas.blocks;
    context.strokeStyle = "rgba(102, 255, 0, 1)";
    for (const [id, block] of blocks) {
      const frameTopLeft = new Point([block.bottomLeft.px, canvas.height - block.topRight.py]);
      const frameBottomRight = new Point([block.topRight.px, canvas.height - block.bottomLeft.py]);
      const sizeX = frameBottomRight.px - frameTopLeft.px;
      const sizeY = frameBottomRight.py - frameTopLeft.py;
      context.strokeRect(frameTopLeft.px, frameTopLeft.py, sizeX, sizeY);
    }
  };

  const [hoveringPoint, setHoveringPoint] = useState<Point | null>(null);
  const [hoveringBlocks, setHoveringBlocks] = useState<Block[]>([]);
  const onCanvasHover = (event: React.MouseEvent<HTMLCanvasElement>) => {
    const point = getMousePos(canvasRef.current, event);
    const block = Array.from(interpretedResult?.canvas.blocks.values() ?? []).filter((b) =>
      point.isInside(b.bottomLeft, b.topRight)
    );
    setHoveringBlocks(block);
    setHoveringPoint(point);
  };

  const total = interpretedResult?.cost + similarity;
  const diff = total - oldTotal;

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
          <label>
            Example id
            <input
              type="number"
              value={exampleId}
              onChange={(event) => setExampleId(Number(event.target.value))}
            />
          </label>
        </div>
        <div>
          <button onClick={handleClickGenerateInstruction}>Generate Instruction</button>
          <button onClick={() => handleClickRenderCanvas(playgroundCode)}>Render Canvas</button>
          <button onClick={handleReset}>Reset</button>
          <label>
            <input
              type="checkbox"
              checked={drawBorder}
              onChange={(e) => setDrawBorder(e.target.checked)}
            />
            border
          </label>
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
              <div
                style={{
                  display: "flex",
                  fontSize: "14px",
                  lineHeight: "18px",
                }}
              >
                <textarea
                  style={{
                    width: "500px",
                    height: "400px",
                    fontSize: "14px",
                    lineHeight: "18px",
                  }}
                  placeholder="Code to be submitted"
                  value={playgroundCode}
                  onChange={handlePlaygroundCode}
                />
                <div style={{ width: "5ch" }}>
                  {interpretedResult?.instructionCosts.map((cost, index) => (
                    <div key={index}>{cost}</div>
                  ))}
                </div>
              </div>
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
          position: "relative",
          maxWidth: width,
        }}
      >
        <canvas
          style={{ outline: "1px solid black" }}
          width={width}
          height={height}
          ref={canvasRef}
          onClick={(event) => {
            const instruction = getClickInstruction(
              canvasRef,
              event,
              instrument,
              interpretedResult.canvas.blocks,
              color,
              prevSelectedBlockId,
              setPrevSelectedBlockId,
            );
            if (instruction) {
              const code = `${playgroundCode}\n${instructionToString(instruction)}`;
              setPlaygroundCode(code);
              handleClickRenderCanvas(code);
            }
          }}
          onMouseMove={onCanvasHover}
          onMouseOver={onCanvasHover}
          onMouseLeave={() => {
            setHoveringBlocks([]);
            setHoveringPoint(null);
          }}
        />
        <img
          ref={imgRef}
          style={{
            position: "absolute",
            top: 0,
            left: 0,
            opacity: expectedOpacity,
            pointerEvents: "none",
          }}
          src={modules[`../../../../problems/problem${exampleId}.png`]}
        />
        <input
          type="range"
          min={0}
          max={1}
          step={0.01}
          value={expectedOpacity}
          onChange={(event) => setExpectedOpacity(Number(event.target.value))}
        />
        <div>
          Hovering: {hoveringPoint ? `(${hoveringPoint.px},${hoveringPoint.py})` : ""}{" "}
          {hoveringBlocks.map((b) => b.id).join(", ")}
        </div>
        <div>Cost: {interpretedResult?.cost}</div>
        <div>Similarity: {similarity}</div>
        <div>Total: {total}</div>
        <div style={{ color: diff > 0 ? 'red' : 'green' }}>Diff: {diff}</div>
      </div>
      <CommandsPanel
        color={color}
        setColor={setColor}
        instrument={instrument}
        setInstrument={setInstrument}
      />
    </div>
  );
};
