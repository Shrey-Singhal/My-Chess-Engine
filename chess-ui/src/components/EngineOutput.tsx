type EngineStats = {
    bestMove: string;
    depth: number;
    nodes: number;
    ordering: string;
    time: string;
};

type EngineOutputProps = {
    onEngineMove: (time: number) => void;
    setEngineTime: (t: number) => void;
    engineTime: number;
    onTakeBack: () => void;
    onNewGame: () => void;
    engineStats: EngineStats;
    setFlipped: React.Dispatch<React.SetStateAction<boolean>>;
    flipped: boolean;
}

function EngineOutput({onEngineMove, setEngineTime, engineTime, onTakeBack, onNewGame, engineStats, flipped, setFlipped}: EngineOutputProps) {
    const buttonClass = "border border-gray-400 bg-gray-100 rounded px-2 py-1 mb-2";


    return (
        <div className="absolute left-[800px] top-[250px]">
            Thinking Time: <br/>
            <select 
                className="border border-gray-400 bg-white rounded px-2 py-1"
                onChange={e => setEngineTime(Number(e.target.value) * 1000)}
            >
                <option value = "1">1s</option>
                <option value = "2">2s</option>
                <option value = "4">4s</option>
                <option value = "6">6s</option>
                <option value = "8">8s</option>
                <option value = "10">10s</option>
            </select><br/><br/><br/>
            <span id="BestOut">BestMove: {engineStats.bestMove}</span><br/>
            <span id="DepthOut">Depth: {engineStats.depth}</span><br/>
            <span id="NodesOut">Nodes: {engineStats.nodes}</span><br/>
            <span id="OrderingOut">Ordering: {engineStats.ordering}</span><br/>
            <span id="TimeOut">Time: {engineStats.time}</span><br/><br/>
            <button 
                type="button" 
                className={buttonClass} 
                id="SearchButton"
                onClick={() => onEngineMove(engineTime)}
            >
                    Move Now
            </button>
            <br/>
            <button 
                type="button" 
                className={buttonClass} 
                id="NewGameButton"
                onClick={onNewGame}
            >
                New Game
            </button><br/>
            <button 
                type="button" 
                className={buttonClass} 
                id="FlipButton"
                onClick={() => {
                    setFlipped(prev => !prev);
                    if (!flipped) onEngineMove(engineTime);
                }}
            >Flip Board</button><br/><br/>
            <button 
                type="button" 
                className={buttonClass} 
                id="TakeButton"
                onClick={onTakeBack}
            >
                Take Back
            </button><br/><br/><br/>
            <span id="GameStatus"></span>
        </div>
    );
}

export default EngineOutput;
