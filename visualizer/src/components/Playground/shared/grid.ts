import _ from 'lodash';
import { Block } from "../../../contest-logic/Block";

export type GridColumn = {
    width: number;
  };

export  type GridRow = {
    height: number;
    columns: GridColumn[];
  };

export  type Grid = {
    rows: GridRow[];
  };

export  type SimpleGridRow = [number, number[]];
  export type SimpleGrid = SimpleGridRow[];


 export const mapToSimple = (grid: Grid): SimpleGrid => {
    return grid.rows.map((row) => [row.height, row.columns.map((column) => column.width)]);
  };
 export const mapFromSimple = (simpleGrid: SimpleGrid): Grid => {
    return simpleGrid.reduce<Grid>(
      (grid, row) => {
        const [height, columns] = row;
        grid.rows.push({
          height,
          columns: columns.map((column) => ({ width: column })),
        });

        return grid;
      },
      { rows: [] }
    );
  };

export const getGridByBlocks = (blocks: Map<string, Block>): SimpleGrid => {
    try {
        const grid: Grid = { rows: [] };
        const blocksList = [...blocks.values()];
        const groups = _.groupBy(blocksList, (block: Block) => {
            return block.bottomLeft.py;
        })

        const rows = Object
            .keys(groups)
            .sort((a, b) => Number(a) - Number(b))
            .map(key => groups[key]);

        rows.forEach((row: Block[]) => {
            console.log(row);
            const columns: Block[] = [...row].sort((a, b) => Number(a.bottomLeft.px) - Number(b.bottomLeft.px));
            grid.rows.push({
                height: row[0].size.py,
                columns: columns.map(column => ({
                    width: column.size.px
                }))
            })
        })
        console.log(grid);

        return mapToSimple(grid);
    } catch (error) {
        console.error(error);
        return [];
    }
};
