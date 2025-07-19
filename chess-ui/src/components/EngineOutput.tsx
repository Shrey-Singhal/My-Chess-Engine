import React, { useEffect, useState } from "react";

type EngineStats = {
    bestMove: string;
    depth: number;
    scoreText: string;
    nodes: number;
    ordering: string;
    time: string;
};

function EngineOutput() {
    const buttonClass = "border border-gray-400 bg-gray-100 rounded px-2 py-1 mb-2";

    const [engineStats, setEngineStats] = useState<EngineStats>({
        bestMove: "",
        depth: 0,
        scoreText: "",
        nodes: 0,
        ordering: "",
        time: "",
    });

    // Fetch stats every second (while searching)
    useEffect(() => {
        const interval = setInterval(() => {
            fetch("http://localhost:5045/api/chess/enginestats")
                .then((res) => res.json())
                .then((data) => setEngineStats(data));
        }, 1000);

        return () => clearInterval(interval);
    }, []);

    return (
        <div className="absolute left-[600px] top-[250px]">
            Thinking Time: <br/>
            <select className="border border-gray-400 bg-white rounded px-2 py-1">
                <option value = "1">1s</option>
                <option value = "2">2s</option>
                <option value = "4">4s</option>
                <option value = "6">6s</option>
                <option value = "8">8s</option>
                <option value = "10">10s</option>
            </select><br/><br/><br/>
            <span id="BestOut">BestMove: {engineStats.bestMove}</span><br/>
            <span id="DepthOut">Depth: {engineStats.depth}</span><br/>
            <span id="ScoreOut">Score: {engineStats.scoreText}</span><br/>
            <span id="NodesOut">Nodes: {engineStats.nodes}</span><br/>
            <span id="OrderingOut">Ordering: {engineStats.ordering}</span><br/>
            <span id="TimeOut">Time: {engineStats.time}</span><br/><br/>
            <button type="button" className={buttonClass} id="SearchButton">Move Now</button><br/>
            <button type="button" className={buttonClass} id="NewGameButton">New Game</button><br/>
            <button type="button" className={buttonClass} id="FlipButton">Flip Board</button><br/><br/>
            <button type="button" className={buttonClass} id="TakeButton">Take Back</button><br/><br/><br/>
            <span id="GameStatus"></span>
        </div>
    );
}

export default EngineOutput;
