import React from "react";

function EngineOutput() {
    const buttonClass = "border border-gray-400 bg-gray-100 rounded px-2 py-1 mb-2";
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
            <span id="BestOut">BestMove:</span><br/>
            <span id="DepthOut">Depth:</span><br/>
            <span id="ScoreOut">Score:</span><br/>
            <span id="NodesOut">Nodes:</span><br/>
            <span id="OrderingOut">Ordering:</span><br/>
            <span id="TimeOut">Time:</span><br/><br/>
            <button type="button" className={buttonClass} id="SearchButton">Move Now</button><br/>
            <button type="button" className={buttonClass} id="NewGameButton">New Game</button><br/>
            <button type="button" className= {buttonClass}id="FlipButton">Flip Board</button><br/><br/>
            <button type="button" className={buttonClass} id="TakeButton">Take Back</button><br/><br/><br/>
            <span id="GameStatus"></span>
                       
        </div>
    );
}

export default EngineOutput;
