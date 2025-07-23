import { useState, useEffect } from "react";
import Board from "./Board";
import SetFEN from "./SetFEN";
import ResultModal from "./ResultModal";
import EngineOutput from "./EngineOutput";

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
    const BASE = import.meta.env.VITE_API_BASE_URL as string;
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
    const [flipped, setFlipped] = useState<boolean>(false);


    const fetchPieces = () => {
        fetch(`${BASE}/getpieces`, {credentials: "include"})
        .then((res) => res.json())
        .then((data: GuiPiece[]) => {
            setPieces(data);
        })
        .catch((err) => console.error("Failed to fetch pieces:", err));
    };

    const handleEngineMove = (timeOverride?: number) => {
        fetch(`${BASE}/engineMove`, {
            method: "POST",
            credentials: "include",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ Time: timeOverride ?? engineTime }),
        })
        .then(res => res.json())
        .then((data) => {
            setPieces(data.pieces);
            if (data.result) setModalMsg(data.result);

            fetch(`${BASE}/enginestats`, {credentials: "include"})
            .then(res => res.json())
            .then(setEngineStats);
        });
    };

    const handleTakeBack = () => {
        fetch(`${BASE}/takemove`, { method: "POST", credentials: "include" })
        .then(res => res.json())
        .then(data => {
            setPieces(data.pieces);
            if (data.result) setModalMsg(data.result);
        });
    };

    const handleNewGame = () => {
        fetch(`${BASE}/newgame`, { method: "POST", credentials: "include" })
        .then(res => res.json())
        .then(data => {
            setPieces(data.pieces);
            setModalMsg(null);  // Clear result modal
        });
    };

    useEffect(() => {
        const initGame = async () => {
            await fetch(`${BASE}/newgame`, {
                method: "POST",
                credentials: "include",
            });

            fetchPieces();
        };

        initGame();
    }, []); 

    return (
        <>
            <SetFEN fetchPieces={fetchPieces} />
            <Board pieces={pieces} setPieces={setPieces} setModalMsg={setModalMsg} onEngineMove={handleEngineMove} engineTime={engineTime} flipped={flipped}/>
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
                flipped={flipped}
                setFlipped={setFlipped}
            />
        </>
    );
}
    
    
export default ChessGame;