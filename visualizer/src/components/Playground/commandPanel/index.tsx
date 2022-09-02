import { FC, useState } from "react";
import { InstructionType } from "../../../contest-logic/Instruction";
import "./styles.css";
import { RGBA } from "../../../contest-logic/Color";

interface ICommandsPanel {
  instrument: InstructionType;
  color: RGBA;
  setColor(value: RGBA): void;
  setInstrument(value: InstructionType): void;
}

export const CommandsPanel: FC<ICommandsPanel> = ({
  instrument,
  color,
  setColor,
  setInstrument,
}) => {
  return (
    <div className={"commandPanel"}>
      {Object.values(InstructionType).map((value, key) => {
        const style = value === instrument ? "commandPanel__active-item" : "commandPanel__item";
        if (value === InstructionType.ColorInstructionType) {
          return (
            <div className={"commandPanel__item-with-color"}>
              <span key={key} className={style} onClick={() => setInstrument(value)}>
                {value}
              </span>

              <input type="color" onInput={(event) => setColor(hexToRGBA(event.target.value))} />
            </div>
          );
        }
        return (
          <span key={key} className={style} onClick={() => setInstrument(value)}>
            {value}
          </span>
        );
      })}
    </div>
  );
};

function hexToRGBA(hex: string) {
  const r = parseInt(hex.slice(1, 3), 16);
  const g = parseInt(hex.slice(3, 5), 16);
  const b = parseInt(hex.slice(5, 7), 16);
  return new RGBA([r, g, b, 255]);
}
