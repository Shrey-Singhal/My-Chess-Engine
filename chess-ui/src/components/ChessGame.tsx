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

const ChessGame = () => {
    const [pieces, setPieces] = useState<GuiPiece[]>([]);
    const [modalMsg, setModalMsg] = useState<string | null>(null);
    const [engineTime, setEngineTime] = useState<number>(1000); // default 1s

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
            <EngineOutput onEngineMove={handleEngineMove} setEngineTime={setEngineTime} engineTime = {engineTime}/>
        </>
    );
}
    
    
export default ChessGame;