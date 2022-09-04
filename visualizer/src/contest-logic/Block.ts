/* eslint-disable */

import { Point } from "./Point";
import { RGBA } from "./Color";

export type Size = Point;
export enum BlockType {
  SimpleBlockType,
  ComplexBlockType,
  PngBlockType,
}
export type Block = SimpleBlock | ComplexBlock | PngBlock;

export class SimpleBlock {
  typ: BlockType.SimpleBlockType;

  id: string;

  bottomLeft: Point;

  topRight: Point;

  size: Size;

  color: RGBA;

  constructor(id: string, bottomLeft: Point, topRight: Point, color: RGBA) {
    this.typ = BlockType.SimpleBlockType;
    this.id = id;
    this.bottomLeft = bottomLeft;
    this.topRight = topRight;
    this.size = topRight.getDiff(bottomLeft);
    this.color = color;
    if (this.bottomLeft.px > this.topRight.px || this.bottomLeft.py > this.topRight.py) {
      throw Error("Invalid Block");
    }
  }

  getChildren() {
    return [this];
  }

  clone() {
    return new SimpleBlock(
      this.id,
      this.bottomLeft.clone(),
      this.topRight.clone(),
      this.color.clone()
    );
  }
}

export class ComplexBlock {
  typ: BlockType.ComplexBlockType;

  id: string;

  bottomLeft: Point;

  topRight: Point;

  size: Size;

  subBlocks: SimpleBlock[];

  constructor(id: string, bottomLeft: Point, topRight: Point, subBlocks: SimpleBlock[]) {
    this.typ = BlockType.ComplexBlockType;
    this.id = id;
    this.bottomLeft = bottomLeft;
    this.topRight = topRight;
    this.size = topRight.getDiff(bottomLeft);
    this.subBlocks = subBlocks;
    if (this.bottomLeft.px > this.topRight.px || this.bottomLeft.py > this.topRight.py) {
    }
  }

  getChildren() {
    return this.subBlocks;
  }

  clone() {
    return new ComplexBlock(
      this.id,
      this.bottomLeft.clone(),
      this.topRight.clone(),
      this.subBlocks.map((b) => b.clone())
    );
  }
}

export class PngBlock {
  typ: BlockType.PngBlockType;

  id: string;

  bottomLeft: Point;

  topRight: Point;

  size: Size;

  bytes: Uint8ClampedArray;

  constructor(id: string, bottomLeft: Point, topRight: Point, bytes: Uint8ClampedArray) {
    this.typ = BlockType.PngBlockType;
    this.id = id;
    this.bottomLeft = bottomLeft;
    this.topRight = topRight;
    this.size = topRight.getDiff(bottomLeft);
    this.bytes = bytes;
    if (this.bottomLeft.px > this.topRight.px || this.bottomLeft.py > this.topRight.py) {
    }
  }

  clone() {
    return new PngBlock(
      this.id,
      this.bottomLeft.clone(),
      this.topRight.clone(),
      this.bytes
    );
  }
}
