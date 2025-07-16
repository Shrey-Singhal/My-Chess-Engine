import React, { useRef, useState, type JSX } from "react";

type GuiPiece = {
    fileClass: string;
    rankClass: string;
    imagePath: string;
}

function Board({pieces}: {pieces: GuiPiece[]}) {
    //const [pieces, setPieces] = useState<GuiPiece[]>([]);
    const squares_style = "absolute w-[60px] h-[60px]";
    const [selectedSquare, setSelectedSquare] = useState<{ file: number; rank: number } | null>(null);

    const boardRef = useRef<HTMLDivElement>(null);


    //render squares
    const generateBoardSquares = (): JSX.Element[] => {
        const squares: JSX.Element[] = [];
        let light = 1;

        for (let rank = 7; rank >= 0; rank--) {
            light ^= 1;
            const rankClass = `rank${rank+1}`;

            for (let file = 0; file <= 7; file++) {
                const fileClass = `file${file+1}`;
                const colorClass = light === 0 ? "Light" : "Dark";
                light ^= 1;

                const isSelected = selectedSquare?.file === file && selectedSquare?.rank === rank;
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
    }

    const handleClick = (e: React.MouseEvent, type: "Piece" | "Square") => {
        console.log(`${type} Click`);
        console.log(`ClickedSquare() at ${e.pageX}, ${e.pageY}`);
        const position = boardRef.current?.getBoundingClientRect();
        if (!position) return;
        
        const workedX = Math.floor(position.left);
        const workedY = Math.floor(position.top);

        const pageX = Math.floor(e.pageX);
        const pageY = Math.floor(e.pageY);
        
        const file = Math.floor((pageX - workedX) / 60);
        const rank = 7 - Math.floor((pageY - workedY) / 60);
        
        // Call backend to convert file/rank to 120-based square index
        fetch(`http://localhost:5045/api/chess/fr2sq?file=${file}&rank=${rank}`)
            .then(res => res.json())
            .then(data => {
                console.log("Clicked sq:", data.prSq);
            })
            .catch(err => console.error("Error fetching square:", err));
        
            setSelectedSquare({file, rank});
    }

    return (
        <div className="relative top-5 left-14 w-[480px] h-[480px]" ref={boardRef} id = "Board">
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
