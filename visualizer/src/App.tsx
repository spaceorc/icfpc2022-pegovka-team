import { useState } from 'react';

function App() {
    const [width, setWidth] = useState(400);
    const [height, setHeight] = useState(400);
    const [code, setCode] = useState('');

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
            <canvas width={width} height={height} />
        </>
    );
}

export default App;
