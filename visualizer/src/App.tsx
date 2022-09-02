import { useState } from 'react';

type Block = {
    id: string;
    color: string;
};

function App() {
    const [width, setWidth] = useState(400);
    const [height, setHeight] = useState(400);
    const [code, setCode] = useState('');

    // const drawToCanvas = () => {
    //     const painter = new Painter();
    //     const renderedData = painter.draw(paintedCanvas);
    //     const canvas = canvasRef.current!;
    //     const context = canvas.getContext('2d')!;

    //     canvas.width = paintedCanvas.width;
    //     canvas.height = paintedCanvas.height;
    //     const imgData = context.getImageData(0, 0, canvas.width, canvas.height);
    //     renderedData.forEach((pixel: RGBA, index: number) => {
    //         imgData.data[index * 4] = pixel.r;
    //         imgData.data[index * 4 + 1] = pixel.g;
    //         imgData.data[index * 4 + 2] = pixel.b;
    //         imgData.data[index * 4 + 3] = pixel.a;
    //     });
    //     context.putImageData(imgData, 0, 0);
    // };

    return (
        <>
            <div>
                <label>width<br/><input type="text" value={width} onChange={event => setWidth(Number(event.target.value))} /></label><br/>
                <label>height<br/><input type="text" value={width} onChange={event => setHeight(Number(event.target.value))} /></label><br/>
                <label>
                    code<br/>
                    <textarea value={code} onChange={event => setCode(event.target.value)} />
                </label>
            </div>
            <canvas style={{border: '1px solid black'}} width={width} height={height} />
        </>
    );
}

export default App;
