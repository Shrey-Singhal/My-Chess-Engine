import React, { useState, useEffect } from "react";
import Board from "./board";
import SetFen from "./setFEN";
import ResultModal from "./ResultModal";

type GuiPiece = {
    fileClass: string;
    rankClass: string;
    imagePath: string;
}

const ChessGame = () => {
    const [pieces, setPieces] = useState<GuiPiece[]>([]);
    const [modalMsg, setModalMsg] = useState<string | null>(null);

    const fetchPieces = () => {
        fetch("http://localhost:5045/api/chess/getpieces")
        .then((res) => res.json())
        .then((data: GuiPiece[]) => {
            setPieces(data);
        })
        .catch((err) => console.error("Failed to fetch pieces:", err));
    };

    useEffect(() => {
        fetchPieces();
    }, []);

    return (
        <>
            <SetFen fetchPieces={fetchPieces} />
            <Board pieces={pieces} fetchPieces={fetchPieces} setModalMsg={setModalMsg} />
            <ResultModal show={!!modalMsg} onClose={() => setModalMsg(null)}>
              {modalMsg}
            </ResultModal>
        </>
    );
}
    
    
export default ChessGame;