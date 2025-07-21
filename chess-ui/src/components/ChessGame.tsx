import React, { useState, useEffect } from "react";
import Board from "./board";
import SetFen from "./setFEN";
import ResultModal from "./ResultModal";
import EngineOutput from "./engineOutput";

type GuiPiece = {
    fileClass: string;
    rankClass: string;
    imagePath: string;
}
type EngineStats = {
    bestMove: string;
    depth: number;
    nodes: number;
    ordering: string;
    time: string;
};
const ChessGame = () => {
    const [pieces, setPieces] = useState<GuiPiece[]>([]);
    const [modalMsg, setModalMsg] = useState<string | null>(null);
    const [engineTime, setEngineTime] = useState<number>(1000); // default 1s
    const [engineStats, setEngineStats] = useState<EngineStats>({
        bestMove: "",
        depth: 0,
        nodes: 0,
        ordering: "",
        time: "",
    });

    const fetchPieces = () => {
        fetch("http://localhost:5045/api/chess/getpieces")
        .then((res) => res.json())
        .then((data: GuiPiece[]) => {
            setPieces(data);
        })
        .catch((err) => console.error("Failed to fetch pieces:", err));
    };

    const handleEngineMove = (timeOverride?: number) => {
        fetch("http://localhost:5045/api/chess/engineMove", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ Time: timeOverride ?? engineTime }),
        })
        .then(res => res.json())
        .then((data) => {
            fetchPieces();
            if (data.result) setModalMsg(data.result);

            fetch("http://localhost:5045/api/chess/enginestats")
            .then(res => res.json())
            .then(setEngineStats);
        });
    };

    const handleTakeBack = () => {
        fetch("http://localhost:5045/api/chess/takemove", { method: "POST" })
        .then(res => res.json())
        .then(data => {
            setPieces(data.pieces);
            if (data.result) setModalMsg(data.result);
        });
    };

    const handleNewGame = () => {
        fetch("http://localhost:5045/api/chess/newgame", { method: "POST" })
        .then(res => res.json())
        .then(data => {
            setPieces(data.pieces);
            setModalMsg(null);  // Clear result modal
        });
    };

    useEffect(() => {
        fetchPieces();
    }, []);

    return (
        <>
            <SetFen fetchPieces={fetchPieces} />
            <Board pieces={pieces} fetchPieces={fetchPieces} setModalMsg={setModalMsg} onEngineMove={handleEngineMove} engineTime={engineTime}/>
            <ResultModal show={!!modalMsg} onClose={() => setModalMsg(null)}>
              {modalMsg}
            </ResultModal>
            <EngineOutput 
                onEngineMove={handleEngineMove} 
                setEngineTime={setEngineTime} 
                engineTime = {engineTime}
                onTakeBack={handleTakeBack}
                onNewGame={handleNewGame}
                engineStats={engineStats}
            />
        </>
    );
}
    
    
export default ChessGame;