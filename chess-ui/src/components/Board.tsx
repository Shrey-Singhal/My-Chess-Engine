import React, { useRef, useState, type JSX } from "react";

type GuiPiece = {
    fileClass: string;
    rankClass: string;
    imagePath: string;
};

type BoardProps = {
    pieces: GuiPiece[];
    fetchPieces: () => void;
}

function Board({ pieces, fetchPieces }: BoardProps) {
    //const [pieces, setPieces] = useState<GuiPiece[]>([]);
    const squares_style = "absolute w-[60px] h-[60px]";
    const [selectedSquares, setSelectedSquares] = useState<{
        from: { file: number; rank: number } | null;
        to: { file: number; rank: number } | null;
    }>({ from: null, to: null });
    
    const boardRef = useRef<HTMLDivElement>(null);

    //render squares
    const generateBoardSquares = (): JSX.Element[] => {
        const squares: JSX.Element[] = [];
        let light = 1;

        for (let rank = 7; rank >= 0; rank--) {
            light ^= 1;
            const rankClass = `rank${rank + 1}`;

            for (let file = 0; file <= 7; file++) {
                const fileClass = `file${file + 1}`;
                const colorClass = light === 0 ? "Light" : "Dark";
                light ^= 1;

                const isSelected =
                    (selectedSquares.from?.file === file && selectedSquares.from?.rank === rank) ||
                    (selectedSquares.to?.file === file && selectedSquares.to?.rank === rank);

                const selectedClass = isSelected ? "SqSelected" : "";

                const combinedClass = `${squares_style} ${rankClass} ${fileClass} ${colorClass} ${selectedClass}`;
                squares.push(
                    <div
                        key={`${rankClass}-${fileClass}`}
                        className={combinedClass}
                        onClick={(e) => handleClick(e, "Square")}
                    ></div>
                );
            }
        }

        return squares;
    };

    const handleClick = (e: React.MouseEvent, type: "Piece" | "Square") => {
        const position = boardRef.current?.getBoundingClientRect();
        if (!position) return;

        const workedX = Math.floor(position.left);
        const workedY = Math.floor(position.top);

        const pageX = Math.floor(e.pageX);
        const pageY = Math.floor(e.pageY);

        const file = Math.floor((pageX - workedX) / 60);
        const rank = 7 - Math.floor((pageY - workedY) / 60);

        // Call backend to convert file/rank to 120-based square index & printable square
        fetch(`http://localhost:5045/api/chess/fr2sq?file=${file}&rank=${rank}`)
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
                        fetch("http://localhost:5045/api/chess/setusermove", {
                            method: "POST",
                            headers: { "Content-Type": "application/json" },
                            body: JSON.stringify(newSq),
                        }).then(() => {
                            console.log(`From square set to ${prSq}`);
                        });
                    } else {
                        // Second piece click: set to
                        setSelectedSquares({ from: selectedSquares.from, to: { file, rank } });

                        // Tell backend: set to
                        fetch("http://localhost:5045/api/chess/setusermove", {
                            method: "POST",
                            headers: { "Content-Type": "application/json" },
                            body: JSON.stringify(newSq),
                        })
                        .then(() =>
                            // Now call makemove after both from and to are set
                            fetch("http://localhost:5045/api/chess/makeusermove", { method: "POST" })
                        )
                        .then(async (res) => {
                            if (!res.ok) {
                                //move was invalid
                                setSelectedSquares({from: null, to: null});
                                await fetch("http://localhost:5045/api/chess/resetusermove", { method: "POST" }); // reset backend
                                return null;
                            }
                            else {
                                fetchPieces();
                            }
                            return res.json();
                        })                        
                        
                        .then((moveData) => {
                            if (!moveData) return;
                            // Print move as algebraic: get fromSq and toSq from backend if available
                            if (moveData.fromSq && moveData.toSq) {
                                console.log(`Move made: ${moveData.fromSq} -> ${moveData.toSq}`);
                            }

                            // Reset UI and backend
                            fetch("http://localhost:5045/api/chess/resetusermove", { method: "POST" })
                            .then(() => setSelectedSquares({ from: null, to: null }));
                        })
                        .catch(() => {
                            setSelectedSquares({ from: null, to: null });
                            fetch("http://localhost:5045/api/chess/resetusermove", { method: "POST" });
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

                        fetch("http://localhost:5045/api/chess/setusermove", {
                            method: "POST",
                            headers: { "Content-Type": "application/json" },
                            body: JSON.stringify(newSq),
                        })
                        .then(() =>
                            // Now call makemove after both from and to are set
                            fetch("http://localhost:5045/api/chess/makeusermove", { method: "POST" })
                        )
                        .then(async (res) => {
                            if (!res.ok) {
                                //move was invalid
                                setSelectedSquares({from: null, to: null});
                                await fetch("http://localhost:5045/api/chess/resetusermove", { method: "POST" }); // reset backend
                                return null;
                            }
                            else {
                                fetchPieces();
                            }
                            return res.json();
                        })
                        
                        .then((moveData) => {
                            if (!moveData) return;
                            // Print move as algebraic: get fromSq and toSq from backend if available
                            if (moveData.fromSq && moveData.toSq) {
                                console.log(`Move made: ${moveData.fromSq} -> ${moveData.toSq}`);
                            }

                            // Reset UI and backend
                            fetch("http://localhost:5045/api/chess/resetusermove", { method: "POST" })
                            .then(() => setSelectedSquares({ from: null, to: null }));
                        })
                        .catch(() => {
                            setSelectedSquares({ from: null, to: null });
                            fetch("http://localhost:5045/api/chess/resetusermove", { method: "POST" });
                        });
                    }
                }
            })
            .catch((err) => console.error("Error fetching square:", err));
    };


    return (
        <div
            className="relative top-5 left-14 w-[480px] h-[480px]"
            ref={boardRef}
            id="Board"
        >
            {generateBoardSquares()}
            {pieces.map((p, i) => {
                const rankClass = p.rankClass;
                const fileClass = p.fileClass;
                const imgSrc = `/images/${p.imagePath}`;
                return (
                    <img
                        key={i}
                        src={imgSrc}
                        className={`Piece ${rankClass} ${fileClass} absolute w-[60px] h-[60px]`}
                        alt={p.imagePath}
                        onClick={(e) => handleClick(e, "Piece")}
                    />
                );
            })}
        </div>
    );
}
export default Board;
