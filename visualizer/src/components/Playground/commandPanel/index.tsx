import { FC, useState } from "react";
import { InstructionType } from "../../../contest-logic/Instruction";
import "./styles.css";

interface ICommandsPanel {
  instrument: InstructionType;
  setInstrument(value: InstructionType): void;
}

export const CommandsPanel: FC<ICommandsPanel> = ({ instrument, setInstrument }) => {
  return (
    <div className={"commandPanel"}>
      {Object.values(InstructionType).map((value, key) => {
        const style = value === instrument ? "commandPanel__active" : "commandPanel__item";
        return (
          <span key={key} className={style} onClick={() => setInstrument(value)}>
            {value}
          </span>
        );
      })}
    </div>
  );
};
