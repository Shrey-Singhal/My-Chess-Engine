import React, { useRef, useState, type JSX } from "react";

type GuiPiece = {
    fileClass: string;
    rankClass: string;
    imagePath: string;
};

type BoardProps = {
    pieces: GuiPiece[];
    fetchPieces: () => void;
    setModalMsg: (msg: string | null) => void;
    onEngineMove: (t: number)=> void;
    engineTime: number;
    flipped: boolean;
}

function Board({ pieces, fetchPieces, setModalMsg, onEngineMove, engineTime, flipped }: BoardProps) {
    //const [pieces, setPieces] = useState<GuiPiece[]>([]);
    const BASE = import.meta.env.VITE_API_BASE_URL as string;
    const BOARD_SIZE = 600;
    const SQUARE_SIZE = BOARD_SIZE / 8;
    const [selectedSquares, setSelectedSquares] = useState<{
        from: { file: number; rank: number } | null;
        to: { file: number; rank: number } | null;
    }>({ from: null, to: null });
    
    //const [gameStatus, setGameStatus] = useState<string | null>(null);

    function toLogicalSquare(displayFile: number, displayRank: number) {
        const file = flipped ? 7 - displayFile : displayFile;
        const rank = flipped ? displayRank : 7 - displayRank;
        return { file, rank };
    }


    const boardRef = useRef<HTMLDivElement>(null);

    //render squares
    const generateBoardSquares = (): JSX.Element[] => {
        const squares: JSX.Element[] = [];

        for (let displayRank = 0; displayRank < 8; displayRank++) {

            for (let displayFile = 0; displayFile < 8; displayFile++) {
                const { file, rank } = toLogicalSquare(displayFile, displayRank);

                const isLight = (file + rank) % 2 === 0;
                const isSelected =
                    (selectedSquares.from?.file === file &&
                        selectedSquares.from?.rank === rank) ||
                    (selectedSquares.to?.file === file &&
                        selectedSquares.to?.rank === rank);

                //const combinedClass = `${squares_style} ${rankClass} ${fileClass} ${colorClass} ${selectedClass}`;
                squares.push(
                    <div
                        key={`sq-${displayRank}-${displayFile}`}
                        className={`absolute ${isLight ? "Light" : "Dark"} ${
                        isSelected ? "SqSelected" : ""
                        }`}
                        style={{
                        width: SQUARE_SIZE,
                        height: SQUARE_SIZE,
                        top: displayRank * SQUARE_SIZE,
                        left: displayFile * SQUARE_SIZE,
                        }}
                        onClick={(e) => handleClick(e, "Square")}
                    />
                );
            }
        }

        return squares;
    };

    const handleClick = (e: React.MouseEvent, type: "Piece" | "Square") => {
        if (!boardRef.current) return;
        const { left, top } = boardRef.current.getBoundingClientRect();
        const x = e.clientX - left;
        const y = e.clientY - top;

        const displayFile = Math.floor(x / SQUARE_SIZE);
        const displayRank = Math.floor(y / SQUARE_SIZE);
        const { file, rank } = toLogicalSquare(displayFile, displayRank);

        // Call backend to convert file/rank to 120-based square index & printable square
        fetch(`${BASE}/fr2sq?file=${file}&rank=${rank}`, {credentials: "include"})
            .then((res) => res.json())
            .then((data) => {
                const newSq = data.sq;
                const prSq = data.prSq; // Algebraic (like "e4", "d5")

                // Print the selected square every time
                console.log(`Selected square: ${prSq} (${file}, ${rank})`);

                // PIECE CLICK
                if (type === "Piece") {
                    if (!selectedSquares.from) {
                        // First piece click: set from
                        setSelectedSquares({ from: { file, rank }, to: null });

                        // Tell backend: set from
                        fetch(`${BASE}/setusermove`, {
                            method: "POST",
                            credentials: "include",
                            headers: { "Content-Type": "application/json" },
                            body: JSON.stringify(newSq),
                        }).then(() => {
                            console.log(`From square set to ${prSq}`);
                        });
                    } else {
                        // Second piece click: set to
                        setSelectedSquares({ from: selectedSquares.from, to: { file, rank } });

                        // Tell backend: set to
                        fetch(`${BASE}/setusermove`, {
                            method: "POST",
                            credentials: "include",
                            headers: { "Content-Type": "application/json" },
                            body: JSON.stringify(newSq),
                        })
                        .then(() =>
                            // Now call makemove after both from and to are set
                            fetch(`${BASE}/makeusermove`, { method: "POST", credentials: "include" })
                        )
                        .then(async (res) => {
                            if (!res.ok) {
                                //move was invalid
                                setSelectedSquares({from: null, to: null});
                                await fetch(`${BASE}/resetusermove`, { method: "POST", credentials: "include" }); // reset backend
                                return null;
                            }
                            else {
                                fetchPieces();
                            }
                            return res.json();
                        })                        
                        
                        .then((moveData) => {
                            if (!moveData) return;

                            if (moveData.result) {
                                setModalMsg(moveData.result);
                            }

                            // Print move as algebraic: get fromSq and toSq from backend if available
                            if (moveData.fromSq && moveData.toSq) {
                                console.log(`Move made: ${moveData.fromSq} -> ${moveData.toSq}`);
                            }

                            // Reset UI and backend
                            fetch(`${BASE}/resetusermove`, { method: "POST", credentials: "include" })
                            .then(() => setSelectedSquares({ from: null, to: null }));

                            onEngineMove(engineTime);
                        })
                        .catch(() => {
                            setSelectedSquares({ from: null, to: null });
                            fetch(`${BASE}/resetusermove`, { method: "POST", credentials: "include" });
                        });
                    }
                }

                // SQUARE CLICK
                if (type === "Square") {
                    if (selectedSquares.from) {
                        setSelectedSquares({
                            from: selectedSquares.from,
                            to: { file, rank },
                        });

                        fetch(`${BASE}/setusermove`, {
                            method: "POST",
                            credentials: "include",
                            headers: { "Content-Type": "application/json" },
                            body: JSON.stringify(newSq),
                        })
                        .then(() =>
                            // Now call makemove after both from and to are set
                            fetch(`${BASE}/makeusermove`, { method: "POST", credentials: "include" })
                        )
                        .then(async (res) => {
                            if (!res.ok) {
                                //move was invalid
                                setSelectedSquares({from: null, to: null});
                                await fetch(`${BASE}/resetusermove`, { method: "POST", credentials: "include" }); // reset backend
                                return null;
                            }
                            else {
                                fetchPieces();
                            }
                            return res.json();
                        })
                        
                        .then((moveData) => {
                            if (!moveData) return;

                            if (moveData.result) {
                                setModalMsg(moveData.result);
                            }

                            // Print move as algebraic: get fromSq and toSq from backend if available
                            if (moveData.fromSq && moveData.toSq) {
                                console.log(`Move made: ${moveData.fromSq} -> ${moveData.toSq}`);
                            }

                            // Reset UI and backend
                            fetch(`${BASE}/resetusermove`, { method: "POST", credentials: "include" })
                            .then(() => setSelectedSquares({ from: null, to: null }));

                            onEngineMove(engineTime);
                        })
                        .catch(() => {
                            setSelectedSquares({ from: null, to: null });
                            fetch(`${BASE}/resetusermove`, { method: "POST", credentials: "include" });
                        });
                    }
                }
            })
            .catch((err) => console.error("Error fetching square:", err));
    };


    return (
        <div
        ref={boardRef}
        id="Board"
        className="relative left-38"
        style={{ width: BOARD_SIZE, height: BOARD_SIZE }}
        >
        {generateBoardSquares()}

        {pieces.map((p, i) => {
            // parse the backend‐provided `fileClass` & `rankClass`
            const fileIdx =
            parseInt(p.fileClass.replace("file", ""), 10) - 1;
            const rankIdx =
            parseInt(p.rankClass.replace("rank", ""), 10) - 1;

            // compute where to draw it on screen
            const displayFile = flipped ? 7 - fileIdx : fileIdx;
            const displayRank = flipped ? rankIdx : 7 - rankIdx;

            return (
            <img
                key={`pc-${i}-${p.imagePath}`}
                src={`/images/${p.imagePath}`}
                className="Piece absolute"
                style={{
                width: SQUARE_SIZE,
                height: SQUARE_SIZE,
                left: displayFile * SQUARE_SIZE,
                top: displayRank * SQUARE_SIZE,
                }}
                alt={p.imagePath}
                onClick={(e) => handleClick(e, "Piece")}
            />
            );
        })}
        </div>
    );
}
export default Board;
